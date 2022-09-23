using Avalonia.Controls;
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

        private ViewModels.MainWindowViewModel MyVM { get => (ViewModels.MainWindowViewModel)DataContext; }
        private FlowrateSummary? SummaryWindow;

        private void Summary_Click(object sender, RoutedEventArgs e)
        {
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
            if (ConfigProvider.Settings.AutoScanOnStartup)
            {
                await MyVM.RescanNetwork();
                await MyVM.ReadAll();
            }
        }
        private async void Rescan_Click(object sender, RoutedEventArgs e)
        {
            await MyVM.RescanNetwork();
        }
        private async void ReadAll_Click(object sender, RoutedEventArgs e)
        {
            await MyVM.ReadAll();
        }
        private void About_Click(object sender, RoutedEventArgs e)
        {
            var a = new AboutBox();
            a.Show(this);
            a.Focus();
        }
        private void GenerateExamples_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var a = (Avalonia.Application.Current as App);
                if (a == null) throw new NullReferenceException();
                a.GenerateExamples();
                App.ShowMessageBox(App.Current?.Name ?? "", 
@"Successfully generated example files.
Check working directory and its subfolders.");
            }
            catch (Exception ex)
            {
                App.ShowMessageBox(App.Current?.Name ?? "",
@$"Failed to generate examples due to thf following exception:
{ex}");
            }
        }
    }
}
