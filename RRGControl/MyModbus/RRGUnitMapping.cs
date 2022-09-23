using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RRGControl.MyModbus
{
    public enum ModbusType
    {
        RTU,
        TCP
    }

    public class RRGUnitMapping
    {
        [JsonConstructor]
        public RRGUnitMapping() { }
        public RRGUnitMapping(Dictionary<ushort, RRGUnitConfig> d)
        {
            Units = d;
        }

        public string Port { get; set; } = "COM1";
        public int Baudrate { get; set; } = 19200;
        [JsonConverter(typeof(StringEnumConverter))]
        public ModbusType Type { get; set; } = ModbusType.RTU;
        public Dictionary<ushort, RRGUnitConfig> Units { get; set; } = new Dictionary<ushort, RRGUnitConfig>();
    }
}
