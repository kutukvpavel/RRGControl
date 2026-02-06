using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using CommandLine;
using LLibrary;
using MsBox.Avalonia;
using RRGControl.ViewModels;
using RRGControl.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RRGControl
{
    public partial class App : Application
    {
        public event EventHandler? ThemeVariantChanged;

        private class Options
        {
            [Option('c', "config", Required = false, HelpText = @"General settings file path,
can be absolute or relative to working directory.", Default = ConfigProvider.GeneralSettings.DefaultFileName)]
            public string SettingsFile { get; set; } = ConfigProvider.GeneralSettings.DefaultFileName;
            [Option('s', "scripts", Required = false, HelpText = @"Last used scripts file path,
can be absolute or relative to working directory.", Default = ConfigProvider.LastUsedScripts.DefaultFileName)]
            public string LastScriptsFile { get; set; } = ConfigProvider.LastUsedScripts.DefaultFileName;
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
        public override async void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                CurrentOptions = Parser.Default.ParseArguments<Options>(desktop.Args).Value;
                InitLogs();
                Log(this, "Starting up.");
                try
                {
                    ConfigProvider.ReadGeneralSettings(CurrentOptions.SettingsFile);
                    ConfigProvider.ReadLastUsedScripts(CurrentOptions.LastScriptsFile);
                    StartRRGServer();
                    desktop.ShutdownRequested += Desktop_ShutdownRequested;
                    if (Current != null) Current.ActualThemeVariantChanged += ActualThemeVariant_Changed;
                    desktop.MainWindow = new MainWindow
                    {
                        DataContext = new MainWindowViewModel(MyNetwork, MyScript)
                    };
                }
                catch (System.Exception ex)
                {
                    Log(this, ex.ToString());
                    await ShowMessageBox("Unhandled Error", $"Unhandled error occurred. The following message was logged: {ex}.");
                    throw;
                }
            }
            base.OnFrameworkInitializationCompleted();
        }
        public void ActualThemeVariant_Changed(object? sender, EventArgs e)
        {
            ThemeVariantChanged?.Invoke(this, new EventArgs());
        }
        public void GenerateExamples()
        {
            ExampleHelper(ConfigProvider.Settings.UnitsFolder, "example.json", ConfigProvider.ExampleMapping);
            ExampleHelper(ConfigProvider.Settings.ModelsFolder, "RRG.json",ConfigProvider.RRG);
            ExampleHelper(ConfigProvider.Settings.ModelsFolder, "RRG20.json", ConfigProvider.RRG20);
            ExampleHelper(ConfigProvider.Settings.ModelsFolder, "RRG12.json", ConfigProvider.RRG12);
            ExampleHelper(ConfigProvider.Settings.ModelsFolder, "Bronkhorst.json", ConfigProvider.Bronkhorst);
            ExampleHelper(ConfigProvider.Settings.ScriptsFolder, "example.json", ConfigProvider.ExampleScript);
            if (!File.Exists(ConfigProvider.Settings.GasFileName))
                File.WriteAllText(ConfigProvider.Settings.GasFileName, ConfigProvider.Serialize(ConfigProvider.ExampleGases));
            if (!File.Exists(CurrentOptions.SettingsFile))
                File.WriteAllText(CurrentOptions.SettingsFile, ConfigProvider.Serialize(ConfigProvider.Settings));
        }
        public void SaveLastUsedScripts()
        {
            ConfigProvider.SaveLastUsedScripts(CurrentOptions.LastScriptsFile);
        }

#pragma warning disable CS8618
        public Models.Network MyNetwork { get; private set; }
        public Models.Scripts MyScript { get; private set; }
        public CancellationTokenSource Cancellation { get; private set; }
#pragma warning restore

        private void StartRRGServer()
        {
            MyModbus.ModbusProvider p;
            try
            {
                p = new MyModbus.ModbusProvider(ConfigProvider.ReadModelConfigurations());
            }
            catch (ArgumentException)
            {
                throw new Exception("Model database is invalid. Check for duplicates.");
            }
            Cancellation = new CancellationTokenSource();
            var a = new List<Adapters.IAdapter>();
            try
            {
                if (ConfigProvider.Settings.PipeName.Length > 0)
                    a.Add(new Adapters.NamedPipeAdapter(ConfigProvider.Settings.PipeName, Cancellation.Token));
            }
            catch (Exception ex)
            {
                Log(this, $"Failed to initialize named pipe provider: {ex}");
            }
            try
            {
                if (ConfigProvider.Settings.OutboundSocketPort > 0 || ConfigProvider.Settings.InboundSocketPort > 0)
                    a.Add(new Adapters.SocketAdapter(ConfigProvider.Settings.InboundSocketPort,
                        ConfigProvider.Settings.OutboundSocketPort, Cancellation.Token));
            }
            catch (Exception ex)
            {
                Log(this, $"Failed to initialize socket provider: {ex}");
            }
            var s = new Adapters.ScriptAdapter(Cancellation.Token);
            a.Add(s);
            var mapping = ConfigProvider.ReadUnitMappings();
            foreach (var item in mapping)
            {
                foreach (var unit in item.Units)
                {
                    if (unit.Value.GasName == null) continue;
                    var props = ConfigProvider.TryGetGas(unit.Value.GasName);
                    if (props != null)
                    {
                        unit.Value.GasFactor = props.Factor;
                    }
                }
            }
            MyScript = new Models.Scripts(s, ConfigProvider.Settings.ScriptsFolder, ConfigProvider.Settings.CsvFolder,
                mapping, Cancellation.Token);
            MyNetwork = new Models.Network(p, mapping, a, ConfigProvider.Settings.AutoRescanIntervalS);
        }
        private void Desktop_ShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
        {
            Log(this, "Shutdown requested.");
            Cancellation.Cancel();
        }
        private void InitLogs()
        {
            ConfigProvider.LogEvent += Log;
            Adapters.Script.LogEvent += Log;
            Models.Network.LogEvent += Log;
            MyModbus.Connection.LogEvent += Log;
            MyModbus.RRGUnit.LogEvent += Log;
            MyModbus.ModbusRegister.LogEvent += Log;
            MainWindowViewModel.LogEvent += Log;
            Models.Scripts.LogEvent += Log;
            Adapters.NamedPipeAdapter.LogEvent += Log;
            Adapters.SocketAdapter.LogEvent += Log;
            Adapters.ScriptAdapter.LogEvent += Log;
        }
        private Options CurrentOptions { get; set; } = new Options();

        public static async Task ShowMessageBox(string title, string contents)
        {
            var w = MessageBoxManager.GetMessageBoxStandard(title, contents);
            await w.ShowAsync();
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
        private static void ExampleHelper<T>(string dir, string f, T src)
        {
            var p = Path.Combine(dir, f);
            if (File.Exists(p)) return;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(p, ConfigProvider.Serialize(src));
        }

        public static bool IsDarkThemed => (string?)(Current?.ActualThemeVariant.Key) == (string)ThemeVariant.Dark.Key;
        public static IBrush GreenOK => IsDarkThemed ? Brushes.DarkGreen : Brushes.LightGreen;
        public static IBrush OrangeWarning => IsDarkThemed ? Brushes.DarkRed : Brushes.LightSalmon;
    }
}
