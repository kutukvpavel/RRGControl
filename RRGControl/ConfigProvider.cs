using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RRGControl
{
    public class GasDatabase : List<MyModbus.RRGGas>
    {
        public GasDatabase() : base() { }
    }
    public static class ConfigProvider
    {
        public const string FilenameFilter = "*.json";
        public class GeneralSettings
        {
            private const string DefaultModelsSubfolder = "models";
            private const string DefaultUnitsSubfolder = "mapping";
            private const string DefaultScriptsFolder = "scripts";
            private const string DefaultCsvsFolder = "csvs";
            public const string DefaultGasFileName = "gases.json";

            public const string DefaultFileName = "config.json";

            public string ModelsFolder { get; set; } = DefaultModelsSubfolder;
            public string UnitsFolder { get; set; } = DefaultUnitsSubfolder;
            public string ScriptsFolder { get; set; } = DefaultScriptsFolder;
            public string CsvFolder { get; set; } = DefaultCsvsFolder;
            public string GasFileName { get; set; } = DefaultGasFileName;
            public bool DisableUnitAddressChange { get; set; } = true;
            public bool AutoScanOnStartup { get; set; } = true;
            public int AutoUpdateIntervalMs { get; set; } = 500;
            public string PipeName { get; set; } = "RRGControl_Pipe";
            public int OutboundSocketPort { get; set; } = 44753;
            public int InboundSocketPort { get; set; } = 44754;
            public int AutoRescanIntervalS { get; set; } = 5;
            public string PercentFormat { get; set; } = "F1";
            public bool ExampleGenerationAvailable { get; set; } = true;
            public int TimeoutMs { get; set; } = 400;
        }
        public class LastUsedScripts : List<string> 
        {
            public const string DefaultFileName = "last_used.json";

            [JsonConstructor]
            public LastUsedScripts() : base() { }
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
            new MyModbus.ModbusRegisterBase(AddressRegName, 0, 0, 1, 255),
            new MyModbus.ModbusRegisterBase(OperationModeRegName, 2, 19, new Dictionary<string, short>()
            {
                { RegulateModeName, 19 },
                { OpenModeName, 6 },
                { ClosedModeName, 10 }
            }),
            new MyModbus.ModbusRegisterBase(SetpointRegName, 4, 0, 0, 10000),
            new MyModbus.ModbusRegisterBase(MeasuredRegName, 5) { FirstBitAsSign = true }
        });
        public static readonly MyModbus.RRGModelConfig RRG20 = new MyModbus.RRGModelConfig(
            $"RRG{MyModbus.RRGModelConfig.ModelPathSeparator}RRG20", new List<MyModbus.ModbusRegisterBase>()
            {
                new MyModbus.ModbusRegisterBase("SoftStartTime", 19, 200, 30, 10000),
                new MyModbus.ModbusRegisterBase("SoftStartStep", 20, 10, 2, 10),
                new MyModbus.ModbusRegisterBase("SoftStartEnable", 21, 1, new Dictionary<string, short>()
                {
                    { "Enable", 1 },
                    { "Disable", 0 }
                })
            });
        public static readonly MyModbus.RRGModelConfig RRG12 = new MyModbus.RRGModelConfig(
            $"RRG{MyModbus.RRGModelConfig.ModelPathSeparator}RRG12", new List<MyModbus.ModbusRegisterBase>()
            {
                new MyModbus.ModbusRegisterBase("SerialNumber", 1),
                new MyModbus.ModbusRegisterBase("StatusFlags", 3),
                new MyModbus.ModbusRegisterBase("Baudrate", 6, 0x02, new Dictionary<string, short>()
                {
                    { "9600", 0x00 },
                    { "38400", 0x01 },
                    { "19200", 0x02 }
                }) { OnlyLowByte = true, LastValueSpans = true },
                new MyModbus.ModbusRegisterBase("OperationMode", 2, 2, new Dictionary<string, short>()
                {
                    { RegulateModeName, 2 },
                    { OpenModeName, 6 },
                    { ClosedModeName, 10 }
                }) { WriteAsCoils = true, CoilAddress = 2, CoilLength = 2 }
            });
        public static readonly MyModbus.RRGUnitMapping ExampleMapping = new MyModbus.RRGUnitMapping(
            new Dictionary<ushort, MyModbus.RRGUnitConfig>()
            {
                { 
                    1, new MyModbus.RRGUnitConfig() 
                    {
                        ConversionFactor = 0.015,
                        Model = "RRG", 
                        Name = "Air", 
                        ConversionUnits = "mL/min"
                    } 
                },
                { 
                    2, new MyModbus.RRGUnitConfig() 
                    { 
                        ConversionFactor = 0.0015,
                        Model = "RRG", 
                        Name = "Gas", 
                        ConversionUnits = "mL/min" 
                    } 
                }
            })
        {
            Port = "COM2",
            Type = MyModbus.ModbusType.RTU
        };
        public static readonly MyModbus.RRGGas[] ExampleGases = new MyModbus.RRGGas[]
        {
            new(1, "Nitrogen", "N2"),
            new(1, "Air"),
            new(1.45, "Argon", "Ar"),
            new(0.74, "Carbon Dioxide", "CO2"),
            new(1.454, "Helium", "He"),
            new(1.01, "Hydrogen", "H2"),
            new(0.73, "Ammonia", "NH3"),
            new(0.67, "Arsine", "ArH3"),
            new(0.60, "Carbon Disulfide", "CS2"),
            new(1, "Carbon Monoxide", "CO"),
            new(0.31, "Carbon Tetrachloride", "CCl4"),
            new(0.42, "Carbon Tetrafluoride", "CF4"),
            new(0.86, "Chlorine", "Cl2"),
            new(0.44, "Diborane", "B2H6"),
            new(0.40, "Dichlorosilane", "SiH2Cl2"),
            new(0.50, "Fluoroform", "CHF3"),
            new(0.27, "Germanium Tetrachloride"),
            new(1, "Hydrogen Chloride", "HCl"),
            new(0.72, "Methane", "CH4"),
            new(0.99, "Nitrogen Monoxide", "NO"),
            new(0.74, "Nitrogen Dioxide", "NO2"),
            new(1, "Oxygen", "O2"),
            new(0.76, "Phosphine", "PH3"),
            new(0.60, "Silane", "SiH4"),
            new(0.28, "Silicon Tetrachloride", "SiCl4"),
            new(0.26, "Sulfure Hexafluoride", "SF6"),
            new(0.33, "Trichlorosilane", "SiHCl3")
        };
        public static readonly Adapters.Script ExampleScript = new(
            "Example Script", "Example Comment", new List<Adapters.Script.Element>()
            {
                new(1, new Adapters.Packet[] { new("Gas", OperationModeRegName, RegulateModeName) }),
                new(5, new Adapters.Packet[] { new("Air", OperationModeRegName, RegulateModeName)}),
                new(1, new Adapters.Packet[] { new("Air", SetpointRegName, "10") { ConvertUnits = true }}),
                new(10, new Adapters.Packet[] { new("Gas", SetpointRegName, "100") { ConvertUnits = true }}),
                new(1, new Adapters.Packet[] { new("Air", SetpointRegName, "0") { ConvertUnits = true }}),
                new(10, new Adapters.Packet[] { new("Gas", SetpointRegName, "0") { ConvertUnits = true }}),
                new(1, new Adapters.Packet[] { new("Air", OperationModeRegName, RegulateModeName)}),
                new(5, new Adapters.Packet[] { new("Gas", OperationModeRegName, ClosedModeName)})
            });


        public static event EventHandler<string>? LogEvent;
        public static GeneralSettings Settings { get; private set; } = new GeneralSettings();
        public static LastUsedScripts LastScripts { get; private set; } = new LastUsedScripts();
        public static GasDatabase KnownGases { get; private set; } = new GasDatabase();

        private static List<T> ReadConfigFiles<T>(string folder)
        {
            const string name = "Config folder reader";

            List<T> result = new List<T>();
            try
            {
                foreach (var item in Directory.EnumerateFiles(folder, FilenameFilter, SearchOption.AllDirectories))
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
        private static T DeserializeSettingsFile<T>(string filePath) where T : new()
        {
            T? t = default;
            try
            {
                t = JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath), SerializerOptions);
            }
            catch (Exception ex)
            {
                LogEvent?.Invoke("Settings deserializer", $"For file '{filePath}': {ex}");
            }
            return t ?? new T();
        }

        public static List<MyModbus.RRGModelConfig> ReadModelConfigurations()
        {
            var raw = ReadConfigFiles<MyModbus.RRGModelConfig>(Settings.ModelsFolder);
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
        public static List<MyModbus.RRGUnitMapping> ReadUnitMappings()
        {
            KnownGases = DeserializeSettingsFile<GasDatabase>(Settings.GasFileName);
            foreach (var item in KnownGases)
            {
                for (int i = 0; i < item.Aliases.Length; i++)
                {
                    item.Aliases[i] = item.Aliases[i].ToLowerInvariant();
                }
            }
            return ReadConfigFiles<MyModbus.RRGUnitMapping>(Settings.UnitsFolder);
        }
        public static void ReadGeneralSettings(string filePath)
        {
            Settings = DeserializeSettingsFile<GeneralSettings>(filePath);
        }
        public static void ReadLastUsedScripts(string filePath)
        {
            LastScripts = DeserializeSettingsFile<LastUsedScripts>(filePath);
        }
        public static void SaveLastUsedScripts(string filePath)
        {
            try
            {
                File.WriteAllText(filePath, Serialize(LastScripts));
            }
            catch (Exception ex)
            {
                LogEvent?.Invoke("Last used scripts serializer", $"{ex}");
            }
        }
        public static string Serialize<T>(T input)
        {
            return JsonConvert.SerializeObject(input, SerializerOptions);
        }
        public static MyModbus.RRGGas? TryGetGas(string name)
        {
            return KnownGases.Where(x => x.Aliases.Any(y => y == name.ToLowerInvariant())).FirstOrDefault();
        }
    }
}
