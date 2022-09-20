using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using RRGControl.ViewModels;
using RRGControl.Views;
using CommandLine;
using System;
using System.IO;
using LLibrary;

namespace RRGControl
{
    public partial class App : Application
    {
        private const string DefaultModelsSubfolder = "models";
        private const string DefaultUnitsSubfolder = "mapping";
        private class Options
        {
            [Option('m', "models", Required = false, HelpText = "Folder containing model description files")]
            public string? ModelsFolder { get; set; }
            [Option('u', "units", Required = false, HelpText = "Folder containing unit mapping files")]
            public string? UnitsFolder { get; set; }

            public Options Process()
            {
                var res = new Options()
                {
                    ModelsFolder = Path.GetFullPath(ModelsFolder ?? DefaultModelsSubfolder),
                    UnitsFolder = Path.GetFullPath(UnitsFolder ?? DefaultUnitsSubfolder)
                };
                return res;
            }
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var o = Parser.Default.ParseArguments<Options>(desktop.Args).Value.Process();
                InitLogs();
                StartRRGServer(o.ModelsFolder, o.UnitsFolder);
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(MyNetwork)
                };
            }
            base.OnFrameworkInitializationCompleted();
        }

        public static Models.Network MyNetwork { get; private set; }
        private static L LogInstance = new L();
        private static void StartRRGServer(string modelsFolder, string mappingsFolder)
        {
            var p = new MyModbus.ModbusProvider(ConfigProvider.ReadModelConfigurations(modelsFolder));
            MyNetwork = new Models.Network(p, ConfigProvider.ReadUnitMappings(mappingsFolder));
        }
        private static void InitLogs()
        {
            ConfigProvider.LogEvent += Log;
            Models.Network.LogEvent += Log;
            MyModbus.RRGUnit.LogEvent += Log;
            MyModbus.ModbusRegister.LogEvent += Log;
        }
        private static void Log(object? sender, string msg)
        {
            string lbl = "";
            var t = sender?.GetType();
            if (t != null)
            {
                if (t == typeof(string)) lbl = (string)sender;
                else lbl = t.Name;
            }
            LogInstance.Log(lbl, msg);
        }
    }
}
