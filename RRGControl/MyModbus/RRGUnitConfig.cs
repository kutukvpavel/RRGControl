using Newtonsoft.Json;
using System;

namespace RRGControl.MyModbus
{
    public class RRGUnitConfig
    {
        [JsonConstructor]
        public RRGUnitConfig() { }

        public string Name { get; set; } = "Example";
        public string Model { get; set; } = "RRG20";
        public double ConversionFactor { get; set; } = 1;
        public string ConversionUnits { get; set; } = "N/A";
        public string FlowrateNumberFormat { get; set; } = "F2";
        public bool EnableAutoupdate { get; set; } = true;

        /// <summary>
        /// Setpoint and measured flowrate conversion
        /// </summary>
        /// <param name="reg"></param>
        /// <returns></returns>
        public double ConvertToUI(ushort reg)
        {
            return reg * ConversionFactor;
        }
        /// <summary>
        /// Setpoint and measured flowrate conversion
        /// </summary>
        /// <param name="ui"></param>
        /// <returns></returns>
        public ushort ConvertToRegister(double ui)
        {
            return (ushort)Math.Round(ui / ConversionFactor);
        }
    }
}
