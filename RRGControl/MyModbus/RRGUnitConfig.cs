using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRGControl.MyModbus
{
    public class RRGUnitConfig
    {
        public string Name { get; set; }
        public string Model { get; set; }
        public double ConversionFactor { get; set; }

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
