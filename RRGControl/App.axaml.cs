using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CommandLine;
using LLibrary;
using MessageBox.Avalonia;
using RRGControl.ViewModels;
using RRGControl.Views;
using System.Collections.Generic;
using System.IO;
using System.Threading;

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
        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                CurrentOptions = Parser.Default.ParseArguments<Options>(desktop.Args).Value;
                InitLogs();
                Log(this, "Starting up.");
                ConfigProvider.ReadGeneralSettings(CurrentOptions.SettingsFile);
                StartRRGServer();
                desktop.ShutdownRequested += Desktop_ShutdownRequested;
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
        public Models.Network MyNetwork { get; private set; }
        public CancellationTokenSource Cancellation { get; private set; }

        private void StartRRGServer()
        {
            var p = new MyModbus.ModbusProvider(ConfigProvider.ReadModelConfigurations());
            Cancellation = new CancellationTokenSource();
            var a = new List<Adapters.IAdapter>();
            if (ConfigProvider.Settings.PipeName.Length > 0)
                a.Add(new Adapters.NamedPipeAdapter(ConfigProvider.Settings.PipeName, Cancellation.Token));
            if (ConfigProvider.Settings.OutboundSocketPort > 0 || ConfigProvider.Settings.InboundSocketPort > 0) 
                a.Add(new Adapters.SocketAdapter(ConfigProvider.Settings.InboundSocketPort, 
                    ConfigProvider.Settings.OutboundSocketPort, Cancellation.Token));
            a.Add(new Adapters.ScriptAdapter(Cancellation.Token));
            MyNetwork = new Models.Network(p, ConfigProvider.ReadUnitMappings(), a, ConfigProvider.Settings.AutoRescanIntervalS);
        }
        private void Desktop_ShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
        {
            Log(this, "Shutdown requested.");
            Cancellation.Cancel();
        }
        private void InitLogs()
        {
            ConfigProvider.LogEvent += Log;
            Models.Network.LogEvent += Log;
            MyModbus.Connection.LogEvent += Log;
            MyModbus.RRGUnit.LogEvent += Log;
            MyModbus.ModbusRegister.LogEvent += Log;
            MainWindowViewModel.LogEvent += Log;
            Adapters.NamedPipeAdapter.LogEvent += Log;
            Adapters.SocketAdapter.LogEvent += Log;
            Adapters.ScriptAdapter.LogEvent += Log;
        }
        private Options CurrentOptions { get; set; } = new Options();

        public static void ShowMessageBox(string title, string contents)
        {
            var w = MessageBoxManager.GetMessageBoxStandardWindow(title, contents);
            w.Show();
        }
        private static L LogInstance = new L();
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
