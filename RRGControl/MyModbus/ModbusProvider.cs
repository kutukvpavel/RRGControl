using NModbus;
using System.Collections.Generic;
using System.Linq;

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
