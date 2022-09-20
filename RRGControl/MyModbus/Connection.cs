using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NModbus;
using NModbus.SerialPortStream;
using RJCP.IO.Ports;
using System.Net.Sockets;
using System.Net;

namespace RRGControl.MyModbus
{
    public class Connection
    {
        public Connection(ModbusProvider p, RRGUnitMapping m)
        {
            Mapping = m;
            Master = m.Type switch
            {
                ModbusType.RTU => p.Factory.CreateRtuMaster(new SerialPortStreamAdapter(new SerialPortStream(m.Port))),
                ModbusType.TCP => p.Factory.CreateMaster(new TcpClient(IPEndPoint.Parse(m.Port))),
                _ => throw new ArgumentOutOfRangeException(null, "ModbusType out of range.")
            };
            Units = m.ToDictionary(x => x.Key, x => new Models.RRGUnit(p.ConfigurationDatabase[x.Value.Model], x.Value, this, x.Key));
        }

        public IModbusMaster Master { get; }
        public RRGUnitMapping Mapping { get; }
        public Dictionary<ushort, Models.RRGUnit> Units { get; }

        public void WriteRegister(ModbusRegister r, ushort v)
        {
            ThrowHelper(r);
            Master.WriteSingleRegister((byte)r.UnitAddress, r.Base.Address, v);
        }
        public ushort ReadRegister(ModbusRegister r)
        {
            ThrowHelper(r);
            var ret = Master.ReadHoldingRegisters((byte)r.UnitAddress, r.Base.Address, 1);
            if (ret?.Length > 0)
            {
                return ret[0];
            }
            else
            {
                throw new ArgumentException("Empty response.");
            }
        }

        private void ThrowHelper(ModbusRegister r)
        {
            if (r.UnitAddress > 0xFF) throw new NotImplementedException("NModbus library only supports 8-bit unit addresses.");
        }
    }
}
