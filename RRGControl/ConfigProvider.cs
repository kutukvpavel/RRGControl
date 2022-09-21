using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace RRGControl
{
    public static class ConfigProvider
    {
        public class GeneralSettings
        {
            public string FlowrateDisplayFormat { get; set; } = "F1";
            public bool AllowUnitAddressChange { get; set; } = false;
        }

        public const string AddressRegName = "NetworkAddress";
        public const string OperationModeRegName = "OperationMode";
        public const string SetpointRegName = "Setpoint";
        public const string MeasuredRegName = "Measured";
        public const string OpenModeName = "Open";
        public const string ClosedModeName = "Closed";
        public const string RegulateModeName = "Regulate";

        static ConfigProvider()
        {
            SerializerOptions.Converters.Add(new StringEnumConverter());
        }
        public static JsonSerializerSettings SerializerOptions { get; set; } = new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.Auto
        };
        public static readonly MyModbus.RRGModelConfig RRG = new MyModbus.RRGModelConfig("RRG", new List<MyModbus.ModbusRegisterBase>()
        {
            new MyModbus.ModbusRegisterBase(AddressRegName, 0, 0, 1, 65535),
            new MyModbus.ModbusRegisterBase(OperationModeRegName, 2, 19, new Dictionary<string, ushort>()
            {
                { RegulateModeName, 19 },
                { OpenModeName, 6 },
                { ClosedModeName, 10 }
            }),
            new MyModbus.ModbusRegisterBase(SetpointRegName, 4, 0, 0, 10000),
            new MyModbus.ModbusRegisterBase(MeasuredRegName, 5)
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
        public static readonly MyModbus.RRGUnitMapping ExampleMapping = new MyModbus.RRGUnitMapping(
            new Dictionary<ushort, MyModbus.RRGUnitConfig>()
            {
                { 
                    1,
                    new MyModbus.RRGUnitConfig() 
                    { ConversionFactor = 0.0009, Model = "RRG20", Name = "Example1", ConversionUnits = "L/h" } 
                },
                { 
                    2,
                    new MyModbus.RRGUnitConfig() 
                    { ConversionFactor = 0.015, Model = "RRG20", Name = "Example2", ConversionUnits = "mL/min" } 
                }
            }
            )
        {
            Port = "COM1",
            Type = MyModbus.ModbusType.RTU
        };


        public static event EventHandler<string>? LogEvent;
        public static GeneralSettings Settings { get; private set; }

        private static List<T> ReadConfigFiles<T>(string folder)
        {
            const string name = "Config folder reader";

            List<T> result = new List<T>();
            try
            {
                foreach (var item in Directory.EnumerateFiles(folder, "*.json", SearchOption.AllDirectories))
                {
                    try
                    {
                        var m = JsonConvert.DeserializeObject<T>(File.ReadAllText(item), SerializerOptions);
                        if (m == null) throw new InvalidOperationException($"File \"{item}\" deserializes as NULL.");
                        result.Add(m);
                    }
                    catch (Exception ex)
                    {
                        LogEvent?.Invoke(name, ex.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                LogEvent?.Invoke(name, ex.ToString());
            }
            return result;
        }

        public static List<MyModbus.RRGModelConfig> ReadModelConfigurations(string folder)
        {
            var raw = ReadConfigFiles<MyModbus.RRGModelConfig>(folder);
            if (raw.Count == 0) return raw;
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
        public static void ReadGeneralSettings(string filePath)
        {
            GeneralSettings? t = null;
            try
            {
                t = JsonConvert.DeserializeObject<GeneralSettings>(File.ReadAllText(filePath), SerializerOptions);
            }
            catch (Exception ex)
            {
                LogEvent?.Invoke("General settings deserializer", ex.ToString());
            }
            Settings = t ?? new GeneralSettings();
        }
        public static string Serialize<T>(T input)
        {
            return JsonConvert.SerializeObject(input, SerializerOptions);
        }
    }
}
