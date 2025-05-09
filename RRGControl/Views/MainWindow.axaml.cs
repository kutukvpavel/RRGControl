using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;

namespace RRGControl.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private ViewModels.MainWindowViewModel? MyVM { get => DataContext as ViewModels.MainWindowViewModel; }
        private FlowrateSummary? SummaryWindow;

        private void Summary_Click(object sender, RoutedEventArgs e)
        {
            if (MyVM == null) return;
            if (SummaryWindow == null)
            {
                SummaryWindow = new FlowrateSummary()
                {
                    DataContext = new ViewModels.FlowrateSummaryViewModel(MyVM.Connections),
                    ShowActivated = true
                };
                SummaryWindow.Closed += (o, e) => SummaryWindow = null;
                SummaryWindow.Show(this);
            }
            else
            {
                SummaryWindow.Activate();
            }
        }
        private async void OnStartup(object sender, EventArgs e)
        {
            if (ConfigProvider.Settings.AutoScanOnStartup && (MyVM != null))
            {
                await MyVM.RescanNetwork();
                //await MyVM.ReadAll();
            }
        }
        private async void Rescan_Click(object sender, RoutedEventArgs e)
        {
            if (MyVM == null) return;
            await MyVM.RescanNetwork();
        }
        private async void ReadAll_Click(object sender, RoutedEventArgs e)
        {
            if (MyVM == null) return;
            await MyVM.ReadAll();
        }
        private void About_Click(object sender, RoutedEventArgs e)
        {
            var a = new AboutBox();
            a.Show(this);
            a.Focus();
        }
        private async void GenerateExamples_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var a = (Avalonia.Application.Current as App);
                if (a == null) throw new NullReferenceException();
                a.GenerateExamples();
                await App.ShowMessageBox(App.Current?.Name ?? "",
@"Successfully generated example files.
Check working directory and its subfolders.");
                ConfigProvider.Settings.ExampleGenerationAvailable = false;
            }
            catch (Exception ex)
            {
                await App.ShowMessageBox(App.Current?.Name ?? "",
@$"Failed to generate examples due to thf following exception:
{ex}");
            }
        }
        private void ScriptStart_Click(object sender, RoutedEventArgs e)
        {
            MyVM?.ScriptStart();
        }
        private void ScriptPause_Click(object sender, RoutedEventArgs e)
        {
            MyVM?.ScriptPause();
        }
        private void ScriptStop_Click(object sender, RoutedEventArgs e)
        {
            MyVM?.ScriptStop();
        }
        private void ScriptConfigure_Click(object sender, RoutedEventArgs e)
        {
            var w = new Scripts() { DataContext = MyVM?.Scripts };
            w.ShowDialog(this);
        }
        private void ProgressBar_Click(object sender, PointerPressedEventArgs e)
        {
            if (MyVM == null) return;
            var w = new ScriptPreview() 
            { 
                DataContext = new ViewModels.ScriptPreviewViewModel(MyVM.Scripts.Compiled, MyVM.Scripts.Duration,
                    (App.Current as App)?.MyNetwork)
            };
            w.ShowDialog(this);
        }
    }
}
