using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace RRGControl.MyModbus
{
    public enum ModbusType
    {
        RTU,
        TCP
    }
    public class RRGUnitMapping : Dictionary<ushort, RRGUnitConfig>
    {
        public string Port { get; set; }
        public ModbusType Type { get; set; }
    }
}
