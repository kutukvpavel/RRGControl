using NModbus;
using NModbus.SerialPortStream;
using RJCP.IO.Ports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RRGControl.MyModbus
{
    public class Connection
    {
        public static int Timeout { get; set; } = 300;

        public static event EventHandler<string>? LogEvent;

        public Connection(ModbusProvider p, RRGUnitMapping m)
        {
            Mapping = m;
            Master = m.Type switch
            {
                ModbusType.RTU => p.Factory.CreateRtuMaster(new SerialPortStreamAdapter(
                    mPort = new SerialPortStream(m.Port, m.Baudrate) { ReadTimeout = Timeout, WriteTimeout = Timeout })),
                ModbusType.TCP => p.Factory.CreateMaster(
                    mTcpClient = new TcpClient(IPEndPoint.Parse(m.Port)) { ReceiveTimeout = Timeout, SendTimeout = Timeout }),
                _ => throw new ArgumentOutOfRangeException(null, "ModbusType out of range.")
            };
            Master.Transport.Retries = 1;
            Master.Transport.WaitToRetryMilliseconds = Timeout;
            Units = m.Units.ToDictionary(x => x.Key, x => new RRGUnit(p.ConfigurationDatabase[x.Value.Model], x.Value, this, x.Key));
        }

        public IModbusMaster Master { get; }
        public RRGUnitMapping Mapping { get; }
        public Dictionary<ushort, RRGUnit> Units { get; }
        public bool IsUp { get => (Mapping.Type == ModbusType.TCP ? mTcpClient?.Connected : mPort?.IsOpen) ?? false; }

        public async Task Scan()
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
                    mPort?.Open();
                    break;
                case ModbusType.TCP:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
            foreach (var item in Units)
            {
                await item.Value.Probe();
            }
        }
        public async Task WriteRegister(ModbusRegister r, ushort v)
        {
            await mSemaphore.WaitAsync();
            try
            {
                ThrowHelper(r);
                await Master.WriteSingleRegisterAsync((byte)r.UnitAddress, r.Base.Address, v);
            }
            finally
            {
                mSemaphore.Release();
            }
        }
        public async Task<ushort> ReadRegister(ModbusRegister r)
        {
            await mSemaphore.WaitAsync();
            try
            {
                ThrowHelper(r);
                var ret = await Master.ReadHoldingRegistersAsync((byte)r.UnitAddress, r.Base.Address, 1);
                if (ret?.Length > 0)
                {
                    return ret[0];
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
        private SerialPortStream? mPort;
        private SemaphoreSlim mSemaphore = new SemaphoreSlim(1, 1);
        private TcpClient? mTcpClient;
    }
}
