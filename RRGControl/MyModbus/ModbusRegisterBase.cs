using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace RRGControl.MyModbus
{
    [JsonObject]
    public class RegisterLimits
    {
        [JsonConstructor]
        public RegisterLimits() { }
        public RegisterLimits(short min, short max)
        {
            Min = min;
            Max = max;
        }

        public short Min { get; set; }
        public short Max { get; set; }
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
        private static readonly RegisterLimits DefaultLimits = new RegisterLimits(0, short.MaxValue);

        private bool? ValidateLastValueSpans(short v)
        {
            try
            {
                var found = Values?.Values.First(x => x == v);
                return (found != null) ? true : null;
            }
            catch (InvalidOperationException)
            {
                if (Values == null) return null;
                if (v >= Values.Last().Value)
                {
                    return true;
                }
                return false;
            }
        }

        private ModbusRegisterBase(string name, ushort addr, short def) : this(name, addr)
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
        public ModbusRegisterBase(string name, ushort addr, short def, Dictionary<string, short> values) : this(name, addr, def)
        {
            ValueType = RegisterValueType.Fixed;
            Values = values;
        }
        public ModbusRegisterBase(string name, ushort addr, short def, RegisterLimits limits) : this(name, addr, def)
        {
            ValueType = RegisterValueType.Range;
            Limits = limits;
        }
        public ModbusRegisterBase(string name, ushort addr, short def, short min, short max) 
            : this(name, addr, def, new RegisterLimits(min, max))
        {

        }

        public string Name { get; set; } = "Example";
        public ushort Address { get; set; } = 0;
        [JsonConverter(typeof(StringEnumConverter))]
        public RegisterValueType ValueType { get; set; } = RegisterValueType.Range;
        [JsonConverter(typeof(StringEnumConverter))]
        public RegisterType Type { get; set; } = RegisterType.ReadOnly;
        public Dictionary<string, short>? Values { get; set; }
        [JsonProperty(IsReference = false, DefaultValueHandling = DefaultValueHandling.Include, 
            ObjectCreationHandling = ObjectCreationHandling.Replace)]
        public RegisterLimits Limits { get; set; } = DefaultLimits;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue(true)]
        public bool ShowInDashboard { get; set; } = true;
        public short DefaultValue { get; set; } = 0;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool FirstBitAsSign { get; set; } = false;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool OnlyLowByte { get; set; } = false;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool LastValueSpans { get; set; } = false;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool TreatAsCoils { get; set; } = false;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue(-1)]
        public short CoilAddress { get; set; } = -1;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public short CoilLength { get; set; } = 0;

        [JsonIgnore]
        protected short _CoilMask = 0;
        [JsonIgnore]
        public short CoilReadMask
        {
            get
            {
                if (_CoilMask == 0)
                {
                    if (CoilAddress < 0 || CoilLength <= 0) throw new ArgumentOutOfRangeException("CoilAddress or CoilLength");
                    for (int i = 0; i < CoilLength; i++)
                    {
                        _CoilMask |= (short)(ushort)((uint)1 << (i + CoilAddress));
                    }
                }
                return _CoilMask;
            }
        }
        
        public bool ValidateValue(short v)
        {
            return ValueType switch
            {
                RegisterValueType.Range => (v <= Limits.Max) && (v >= Limits.Min),
                RegisterValueType.Fixed => (LastValueSpans ? ValidateLastValueSpans(v)  : Values?.Values.Any(x => x == v)) ?? false,
                _ => throw new ArgumentException("Register type out of range")
            };
        }
    }
}
