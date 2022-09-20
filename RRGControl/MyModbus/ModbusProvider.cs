using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NModbus;

namespace RRGControl.MyModbus
{
    public class ModbusProvider
    {
        public ModbusProvider(List<RRGModelConfig> cfgs)
        {
            ConfigurationDatabase = cfgs.ToDictionary(x => x.Model, x => x);
        }

        public ModbusFactory Factory { get; } = new ModbusFactory();

        public Dictionary<string, RRGModelConfig> ConfigurationDatabase { get; }
    }
}
