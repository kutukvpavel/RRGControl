using Newtonsoft.Json;
using System;

namespace RRGControl.MyModbus
{
    public class RRGGas
    {
        public RRGGas(double factor, params string[] aliases)
        {
            Factor = factor;
            Aliases = aliases;
        }
        public string[] Aliases { get; }
        public double Factor { get; }
    }

    public class RRGUnitConfig
    {
        [JsonConstructor]
        public RRGUnitConfig() { }

        public string Name { get; set; } = "Example";
        public string Model { get; set; } = "RRG20";
        public double ConversionFactor { get; set; } = 1;
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        [System.ComponentModel.DefaultValue((double)1.0)]
        public double GasFactor { get; set; } = 1.0;
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? GasName { get; set; }
        public string ConversionUnits { get; set; } = "N/A";
        public string FlowrateNumberFormat { get; set; } = "F2";
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        [System.ComponentModel.DefaultValue((bool)true)]
        public bool EnableAutoupdate { get; set; } = true;

        /// <summary>
        /// Setpoint and measured flowrate conversion
        /// </summary>
        /// <param name="reg"></param>
        /// <returns></returns>
        public double ConvertToUI(short reg)
        {
            return reg * ConversionFactor * GasFactor;
        }
        /// <summary>
        /// Setpoint and measured flowrate conversion
        /// </summary>
        /// <param name="ui"></param>
        /// <returns></returns>
        public short ConvertToRegister(double ui)
        {
            return (short)Math.Round(ui / ConversionFactor / GasFactor);
        }
    }
}
