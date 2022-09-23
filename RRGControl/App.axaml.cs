using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CommandLine;
using LLibrary;
using MessageBox.Avalonia;
using RRGControl.ViewModels;
using RRGControl.Views;
using System.IO;

namespace RRGControl
{
    public partial class App : Application
    {
        private const string DefaultConfigFileName = "config.json";
        private class Options
        {
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
                StartRRGServer();
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(MyNetwork)
                };
            }
            base.OnFrameworkInitializationCompleted();
        }
        public void GenerateExamples()
        {
            File.WriteAllText(ExampleHelper(ConfigProvider.Settings.UnitsFolder, "example.json"),
                ConfigProvider.Serialize(ConfigProvider.ExampleMapping));
            File.WriteAllText(ExampleHelper(ConfigProvider.Settings.ModelsFolder, "RRG.json"),
                ConfigProvider.Serialize(ConfigProvider.RRG));
            File.WriteAllText(ExampleHelper(ConfigProvider.Settings.ModelsFolder, "RRG20.json"),
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
        private static void StartRRGServer()
        {
            var p = new MyModbus.ModbusProvider(ConfigProvider.ReadModelConfigurations());
            var a = new Adapters.IAdapter[] { new Adapters.NamedPipeAdapter(ConfigProvider.Settings.PipeName) };
            MyNetwork = new Models.Network(p, ConfigProvider.ReadUnitMappings(), a);
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
