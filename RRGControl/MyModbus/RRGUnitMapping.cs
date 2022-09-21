using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace RRGControl.MyModbus
{
    public enum ModbusType
    {
        RTU,
        TCP
    }
    public class RRGUnitMapping : Dictionary<ushort, RRGUnitConfig>
    {
        public RRGUnitMapping(Dictionary<ushort, RRGUnitConfig> d) : base(d) { }

        public string Port { get; set; } = "COM1";
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ModbusType Type { get; set; } = ModbusType.RTU;
    }
}
