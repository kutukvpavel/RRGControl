using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using System;

namespace RRGControl
{
    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args) => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI();


        public static Models.Network MyNetwork { get; private set; }
        public static void StartRRGServer(string modelsFolder, string mappingsFolder)
        {
            var p = new MyModbus.ModbusProvider(ConfigProvider.ReadModelConfigurations(modelsFolder));
            MyNetwork = new Models.Network(p, ConfigProvider.ReadUnitMappings(mappingsFolder));
            
        }
    }
}
