using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using RRGControl.ViewModels;
using RRGControl.Views;
using CommandLine;
using System;
using System.IO;
using LLibrary;
using MessageBox.Avalonia;

namespace RRGControl
{
    public partial class App : Application
    {
        private const string DefaultModelsSubfolder = "models";
        private const string DefaultUnitsSubfolder = "mapping";
        private const string DefaultConfigFileName = "config.json";
        private class Options
        {
            [Option('m', "models", Required = false, HelpText = @"Folder containing model description files,
can be absolute or relative to working directory.", Default = DefaultModelsSubfolder)]
            public string ModelsFolder { get; set; } = DefaultModelsSubfolder;
            [Option('u', "units", Required = false, HelpText = @"Folder containing unit mapping files,
can be absolute or relative to working directory.", Default = DefaultUnitsSubfolder)]
            public string UnitsFolder { get; set; } = DefaultUnitsSubfolder;
            [Option('c', "config", Required = false, HelpText = @"General settings file path,
can be absolute or relative to working directory.", Default = DefaultConfigFileName)]
            public string SettingsFile { get; set; } = DefaultConfigFileName;
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
        private Options CurrentOptions { get; set; } = new Options();
        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                CurrentOptions = Parser.Default.ParseArguments<Options>(desktop.Args).Value;
                InitLogs();
                ConfigProvider.ReadGeneralSettings(CurrentOptions.SettingsFile);
                StartRRGServer(CurrentOptions.ModelsFolder, CurrentOptions.UnitsFolder);
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(MyNetwork)
                };
            }
            base.OnFrameworkInitializationCompleted();
        }
        public void GenerateExamples()
        {
            File.WriteAllText(ExampleHelper(CurrentOptions.UnitsFolder, "example.json"),
                ConfigProvider.Serialize(ConfigProvider.ExampleMapping));
            File.WriteAllText(ExampleHelper(CurrentOptions.ModelsFolder, "RRG.json"),
                ConfigProvider.Serialize(ConfigProvider.RRG));
            File.WriteAllText(ExampleHelper(CurrentOptions.ModelsFolder, "RRG20.json"),
                ConfigProvider.Serialize(ConfigProvider.RRG20));
            File.WriteAllText(CurrentOptions.SettingsFile, ConfigProvider.Serialize(ConfigProvider.Settings));
        }


        public static void ShowMessageBox(string title, string contents)
        {
            var w = MessageBoxManager.GetMessageBoxStandardWindow(title, contents);
            w.Show();
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
            MyModbus.Connection.LogEvent += Log;
            MyModbus.RRGUnit.LogEvent += Log;
            MyModbus.ModbusRegister.LogEvent += Log;
            MainWindowViewModel.LogEvent += Log;
        }
        private static void Log(object? sender, string msg)
        {
            string lbl = "";
            var t = sender?.GetType();
            if (t != null)
            {
                if (t == typeof(string)) lbl = (string?)sender ?? "";
                else lbl = t.Name;
            }
            LogInstance.Log(lbl, msg);
        }
        private static string ExampleHelper(string dir, string f)
        {
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            return Path.Combine(dir, f);
        }
    }
}
