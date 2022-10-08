using NModbus;
using NModbus.SerialPortStream;
using RJCP.IO.Ports;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RRGControl.MyModbus
{
    public class Connection : INotifyPropertyChanged
    {
        public static int Timeout { get; set; } = 300;

        public static event EventHandler<string>? LogEvent;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Connection(ModbusProvider p, RRGUnitMapping m)
        {
            mProvider = p;
            Mapping = m;
            Master = m.Type switch
            {
                ModbusType.RTU => p.Factory.CreateRtuMaster(new SerialPortStreamAdapter(
                    mPort = new SerialPortStream(m.Port, m.Baudrate) { ReadTimeout = Timeout, WriteTimeout = Timeout })),
                ModbusType.TCP => p.Factory.CreateMaster(mTcpClient = TcpClientFactory(m)),
                _ => throw new ArgumentOutOfRangeException(null, "ModbusType out of range.")
            };
            Master.Transport.Retries = 1;
            Master.Transport.WaitToRetryMilliseconds = Timeout;
            try
            {
                Units = m.Units.ToDictionary(x => x.Key, 
                    x => new RRGUnit(p.ConfigurationDatabase[x.Value.Model], x.Value, this, x.Key));
            }
            catch (Exception ex)
            {
                LogEvent?.Invoke(this, $"Invalid mapping file: {ex}");
                Units = new Dictionary<ushort, RRGUnit>();
            }
        }

        public IModbusMaster Master { get; private set; }
        public RRGUnitMapping Mapping { get; }
        public Dictionary<ushort, RRGUnit> Units { get; }
        public bool IsUp { get => (Mapping.Type == ModbusType.TCP ? mTcpClient?.Connected : mPort?.IsOpen) ?? false; }

        public async Task Scan()
        {
            if (!IsUp)
            {
                await mSemaphore.WaitAsync();
                try
                {
                    switch (Mapping.Type)
                    {
                        case ModbusType.RTU:
                            if (mPort?.IsOpen ?? false)
                            {
                                mPort.DiscardInBuffer();
                                mPort.DiscardOutBuffer();
                                mPort.Close();
                            }
                            try
                            {
                                mPort?.Open();
                            }
                            catch (InvalidOperationException)
                            {
                                mPort?.Close();
                                mPort?.Open();
                            }
                            break;
                        case ModbusType.TCP:
                            try
                            {
                                mTcpClient?.Close();
                            }
                            catch (Exception)
                            { }
                            mTcpClient = TcpClientFactory(Mapping);
                            Master = mProvider.Factory.CreateMaster(mTcpClient);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                catch (Exception ex)
                {
                    LogEvent?.Invoke(this, ex.Message);
                }
                finally
                {
                    mSemaphore.Release();
                }
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsUp)));
            foreach (var item in Units)
            {
                await item.Value.Probe();
            }
        }
        public async Task WriteRegister(ModbusRegister r, short v)
        {
            await mSemaphore.WaitAsync();
            try
            {
                ThrowHelper(r);
                unchecked
                {
                    var wrt = (ushort)v;
                    await Master.WriteSingleRegisterAsync((byte)r.UnitAddress, r.Base.Address, wrt);
                }
            }
            finally
            {
                mSemaphore.Release();
            }
        }
        public async Task<short> ReadRegister(ModbusRegister r)
        {
            await mSemaphore.WaitAsync();
            try
            {
                ThrowHelper(r);
                var ret = await Master.ReadHoldingRegistersAsync((byte)r.UnitAddress, r.Base.Address, 1);
                if (ret?.Length > 0)
                {
                    if (r.Base.FirstBitAsSign)
                    {
                        bool invert = (ret[0] & (ushort)(1 << 15)) > 0;
                        short magnitude = (short)(ret[0] & 0x7FFF);
                        return invert ? (short)(-1 * magnitude) : magnitude;
                    }
                    else
                    {
                        unchecked
                        {
                            return (short)ret[0];
                        }
                    }
                }
                else
                {
                    throw new ArgumentException("Empty response.");
                }
            }
            finally
            {
                mSemaphore.Release();
            }
        }
        public async Task ReadAll()
        {
            if (!IsUp) return;
            foreach (var item in Units)
            {
                try
                {
                    await item.Value.ReadAll();
                }
                catch (Exception ex)
                {
                    LogEvent?.Invoke(this, ex.ToString());
                }
            }
        }

        private void ThrowHelper(ModbusRegister r)
        {
            if (r.UnitAddress > 0xFF) throw new NotImplementedException("NModbus library only supports 8-bit unit addresses.");
        }
        private readonly SerialPortStream? mPort;
        private readonly SemaphoreSlim mSemaphore = new SemaphoreSlim(1, 1);
        private TcpClient? mTcpClient;
        private readonly ModbusProvider mProvider;

        private static TcpClient TcpClientFactory(RRGUnitMapping m)
        {
            return new TcpClient(IPEndPoint.Parse(m.Port)) { ReceiveTimeout = Timeout, SendTimeout = Timeout };
        }
    }
}
