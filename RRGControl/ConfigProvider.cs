using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;

namespace RRGControl
{
    public static class ConfigProvider
    {
        public static JsonSerializerOptions SerializerOptions { get; set; } = new JsonSerializerOptions()
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        public static readonly MyModbus.RRGModelConfig RRG = new MyModbus.RRGModelConfig("RRG", new List<MyModbus.ModbusRegisterBase>()
        {
            new MyModbus.ModbusRegisterBase(MyModbus.RRGModelConfig.AddressRegName, 0, 0, 1, 65535),
            new MyModbus.ModbusRegisterBase(MyModbus.RRGModelConfig.OperationModeRegName, 2, 19, new Dictionary<string, ushort>()
            {
                { "Regulate", 19 },
                { "Open", 6 },
                { "Close", 10 }
            }),
            new MyModbus.ModbusRegisterBase(MyModbus.RRGModelConfig.SetpointRegName, 4, 0, 0, 10000),
            new MyModbus.ModbusRegisterBase(MyModbus.RRGModelConfig.MeasuredRegName, 5)
        });
        public static readonly MyModbus.RRGModelConfig RRG20 = new MyModbus.RRGModelConfig(
            $"RRG{MyModbus.RRGModelConfig.ModelPathSeparator}RRG20", new List<MyModbus.ModbusRegisterBase>()
        {
            new MyModbus.ModbusRegisterBase("SoftStartTime", 19, 200, 30, 10000),
            new MyModbus.ModbusRegisterBase("SoftStartStep", 20, 10, 2, 10),
            new MyModbus.ModbusRegisterBase("SoftStartEnable", 21, 1, new Dictionary<string, ushort>()
            {
                { "Enable", 1 },
                { "Disable", 0 }
            })
        });

        public static event EventHandler<string> LogEvent;

        private static List<T> ReadConfigFiles<T>(string folder)
        {
            List<T> result = new List<T>();
            foreach (var item in Directory.EnumerateFiles(folder, "*.json", SearchOption.AllDirectories))
            { 
                try
                {
                    var m = JsonSerializer.Deserialize<T>(File.ReadAllText(item), SerializerOptions);
                    if (m == null) throw new InvalidOperationException($"File \"{item}\" deserializes as NULL.");
                    result.Add(m);
                }
                catch (Exception ex)
                {
                    LogEvent?.Invoke("Config folder reader", ex.ToString());
                }
            }
            return result;
        }

        public static List<MyModbus.RRGModelConfig> ReadModelConfigurations(string folder)
        {
            var raw = ReadConfigFiles<MyModbus.RRGModelConfig>(folder);
            //Process inheritance
            var totalGenerations = raw.Max(x => x.InheritanceLevel);
            var lastGen = raw.Where(x => x.InheritanceLevel == 0);
            for (int i = 1; i <= totalGenerations; i++)
            {
                var ithGen = raw.Where(x => x.InheritanceLevel == i);
                foreach (var item in ithGen)
                {
                    foreach (var parent in lastGen)
                    {
                        item.AppendInherited(parent);
                    }
                }
                lastGen = ithGen;
            }
            return raw;
        }
        public static List<MyModbus.RRGUnitMapping> ReadUnitMappings(string folder)
        {
            return ReadConfigFiles<MyModbus.RRGUnitMapping>(folder);
        }
    }
}
