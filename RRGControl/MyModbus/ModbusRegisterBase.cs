using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RRGControl.MyModbus
{
    [JsonObject]
    public class RegisterLimits
    {
        [JsonConstructor]
        public RegisterLimits() { }
        public RegisterLimits(ushort min, ushort max)
        {
            Min = min;
            Max = max;
        }

        public ushort Min { get; set; }
        public ushort Max { get; set; }
    }
    public enum RegisterValueType
    {
        Range,
        Fixed
    }
    public enum RegisterType
    {
        ReadWrite,
        ReadOnly
    }
    public class ModbusRegisterBase
    {
        private static readonly RegisterLimits DefaultLimits = new RegisterLimits(0, 65535);

        private ModbusRegisterBase(string name, ushort addr, ushort def) : this(name, addr)
        {
            DefaultValue = def;
            Type = RegisterType.ReadWrite;
        }
        [JsonConstructor]
        public ModbusRegisterBase() { }
        public ModbusRegisterBase(string name, ushort addr)
        {
            Name = name;
            Address = addr;
        }
        public ModbusRegisterBase(string name, ushort addr, ushort def, Dictionary<string, ushort> values) : this(name, addr, def)
        {
            ValueType = RegisterValueType.Fixed;
            Values = values;
        }
        public ModbusRegisterBase(string name, ushort addr, ushort def, RegisterLimits limits) : this(name, addr, def)
        {
            ValueType = RegisterValueType.Range;
            Limits = limits;
        }
        public ModbusRegisterBase(string name, ushort addr, ushort def, ushort min, ushort max) 
            : this(name, addr, def, new RegisterLimits(min, max))
        {

        }

        public string Name { get; set; } = "Example";
        public ushort Address { get; set; } = 0;
        [JsonConverter(typeof(StringEnumConverter))]
        public RegisterValueType ValueType { get; set; } = RegisterValueType.Range;
        [JsonConverter(typeof(StringEnumConverter))]
        public RegisterType Type { get; set; } = RegisterType.ReadOnly;
        public Dictionary<string, ushort>? Values { get; set; }
        [JsonProperty(IsReference = false, DefaultValueHandling = DefaultValueHandling.Include, 
            ObjectCreationHandling = ObjectCreationHandling.Replace)]
        public RegisterLimits? Limits { get; set; } = DefaultLimits;
        public bool ShowInDashboard { get; set; } = true;
        public ushort DefaultValue { get; set; } = 0;
        
        public bool ValidateValue(ushort v)
        {
            return ValueType switch
            {
                RegisterValueType.Range => (v <= (Limits?.Max)) && (v >= (Limits?.Min)),
                RegisterValueType.Fixed => Values?.Values.Any(x => x == v) ?? false,
                _ => throw new ArgumentException("Register type out of range")
            };
        }
    }
}
