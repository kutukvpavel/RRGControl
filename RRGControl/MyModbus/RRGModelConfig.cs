using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace RRGControl.MyModbus
{
    public class RRGModelConfig
    {
        public const char ModelPathSeparator = '/';

        [JsonConstructor]
        public RRGModelConfig() { }
        public RRGModelConfig(string model, List<ModbusRegisterBase> regs)
        {
            Registers = regs;
            ModelPath = model;
        }

        public List<ModbusRegisterBase> Registers { get; set; } = new List<ModbusRegisterBase>();
        public string ModelPath { get; set; } = "RRG";
        [JsonIgnore]
        public string TrimmedPath { get => ModelPath.Trim().Trim(ModelPathSeparator); }
        [JsonIgnore]
        public string Model { get => TrimmedPath.Split(ModelPathSeparator).Last(); }
        [JsonIgnore]
        public int InheritanceLevel { get => TrimmedPath.Count(x => x == ModelPathSeparator); }
        [JsonIgnore]
        public string? ParentPath 
        { 
            get => string.Join(ModelPathSeparator, TrimmedPath.Split(ModelPathSeparator).SkipLast(1)); 
        }

        public void AppendInherited(RRGModelConfig parent)
        {
            if (parent.TrimmedPath != ParentPath) return;
            foreach (var item in parent.Registers)
            {
                if (Registers.All(x => x.Name != item.Name))
                {
                    Registers.Add(item);
                }
            }
        }       
    }
}
