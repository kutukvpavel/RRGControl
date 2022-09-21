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

        private ViewModels.MainWindowViewModel? MyVM { get => DataContext as ViewModels.MainWindowViewModel; }

        private void Rescan_Click(object sender, RoutedEventArgs e)
        {
            MyVM?.RescanNetwork();
        }
        private void ReadAll_Click(object sender, RoutedEventArgs e)
        {
            MyVM?.ReadAll();
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
