using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;

namespace RRGControl.MyModbus
{
    public class RRGModelConfig : List<ModbusRegisterBase>
    {
        public const char ModelPathSeparator = '/';
        public const string AddressRegName = "NetworkAddress";
        public const string OperationModeRegName = "OperationMode";
        public const string SetpointRegName = "Setpoint";
        public const string MeasuredRegName = "Measured";

        public static event EventHandler<string>? LogEvent;

        public RRGModelConfig(string model, List<ModbusRegisterBase> regs) : base(regs)
        {
            ModelPath = model;
        }

        public string ModelPath { get; set; }
        public string TrimmedPath { get => ModelPath.Trim().Trim(ModelPathSeparator); }
        public string Model { get => TrimmedPath.Split(ModelPathSeparator).Last(); }
        public int InheritanceLevel { get => TrimmedPath.Count(x => x == ModelPathSeparator); }
        public string? ParentPath { get => Directory.GetParent(TrimmedPath)?.FullName; }

        public void AppendInherited(RRGModelConfig parent)
        {
            if (parent.TrimmedPath != ParentPath) return;
            foreach (var item in parent)
            {
                if (this.All(x => x.Name != item.Name))
                {
                    Add(item);
                }
            }
        }
    }
}
