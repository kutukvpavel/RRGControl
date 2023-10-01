﻿using Newtonsoft.Json;
using System;
using System.Linq;

namespace RRGControl.Adapters
{
    public class Packet
    {
        public static string[] ConvertibleRegisters { get; } =
        {
            ConfigProvider.MeasuredRegName,
            ConfigProvider.SetpointRegName
        };

        [JsonConstructor]
        public Packet()
        {

        }
        public Packet(string u, string r, string v)
        {
            UnitName = u;
            RegisterName = r;
            Value = v;
        }

        public string UnitName { get; set; }
        public string RegisterName { get; set; }
        public string Value { get; set; }
        /// <summary>
        /// For inbound packets: when true, assume mapping units are used, when false, assume device units are used (10000)
        /// For outbound packets: ignored
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool ConvertUnits { get; set; } = false;

        public string GetJson()
        {
            return JsonConvert.SerializeObject(this);
        }
        public bool TryConvertValueToDouble(out double d)
        {
            try
            {
                d = ConvertValueToDouble();
                return true;
            }
            catch (Exception)
            {
                d = double.NaN;
                return false;
            }
        }
        public double ConvertValueToDouble()
        {
            if (double.TryParse(Value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, 
                out double v))
            {
                return v;
            }
            return double.Parse(Value, System.Globalization.CultureInfo.CurrentUICulture);
        }
        public string GetConvertedValue(MyModbus.RRGUnit u)
        {
            if (!ConvertUnits || !ConvertibleRegisters.Any(x => x == RegisterName)) return Value;
            return Math.Round(ConvertValueToDouble() / u.UnitConfig.ConversionFactor)
                .ToString("F0", System.Globalization.CultureInfo.InvariantCulture);
        }

        public static Packet? FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Packet>(json);
        }
        public static Packet FromRegister(MyModbus.RRGUnit u, MyModbus.ModbusRegister r)
        {
            bool convert = false;
            if (ConvertibleRegisters.Any(x => x == r.Base.Name)) convert = true;
            return new Packet(u.UnitConfig.Name, r.Base.Name, 
                convert ? (u.UnitConfig.ConversionFactor * r.Value).ToString() : r.GetValueStringRepresentation());
        }
    }
}
