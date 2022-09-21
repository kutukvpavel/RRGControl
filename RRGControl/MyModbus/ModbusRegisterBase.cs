﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace RRGControl.MyModbus
{
    public class RegisterLimits
    {
        public RegisterLimits(ushort min, ushort max)
        {
            T = new Tuple<ushort, ushort>(min, max);
        }

        private Tuple<ushort, ushort> T;

        public ushort Min { get => T.Item1; }
        public ushort Max { get => T.Item2; }
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
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RegisterValueType ValueType { get; set; } = RegisterValueType.Range;
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RegisterType Type { get; set; } = RegisterType.ReadOnly;
        public Dictionary<string, ushort>? Values { get; set; }
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
