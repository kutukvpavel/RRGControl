using Avalonia;
using Avalonia.Controls;

namespace RRGControl.Views
{
    public partial class Scripts : Window
    {
        public Scripts()
        {
            InitializeComponent();
            btnCancel.Click += BtnCancel_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnOK.Click += BtnOK_Click;
            btnAdd.Click += BtnAdd_Click;
            btnRemove.Click += BtnRemove_Click;
            btnPreview.Click += BtnPreview_Click;
            lstLeft.SelectionChanged += LstLeft_SelectionChanged;
            lstRight.SelectionChanged += LstRight_SelectionChanged;
        }

        private ViewModels.ScriptsViewModel? ViewModel => DataContext as ViewModels.ScriptsViewModel;

        private async void BtnEdit_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (ViewModel == null) return;
            var w = ViewModel.GetEditorWindow();
            if (w == null) return;
            await w.ShowDialog(this);
        }
        private async void BtnPreview_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (ViewModel == null) return;
            ViewModel.Choose();
            var pm = new ViewModels.ScriptPreviewViewModel(ViewModel.Compiled, ViewModel.Duration, (Application.Current as App)?.MyNetwork, 0);
            var w = new ScriptPreviewWindow() 
            {
                DataContext = pm
            };
            await w.ShowDialog(this);
        }
        private void BtnRemove_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ViewModel?.Remove();
        }

        private void BtnAdd_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ViewModel?.Add();
        }
        private void BtnOK_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (ViewModel == null) return;
            ViewModel.Choose();
            ViewModel.Save();
            Close();
        }
        private void BtnUpdate_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (ViewModel == null) return;
            ViewModel.Update();
        }
        private void BtnCancel_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (ViewModel == null) return;
            ViewModel.Restore();
            Close();
        }
        private void LstRight_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            ViewModel?.Invalidate();
        }

        private void LstLeft_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            ViewModel?.Invalidate();
        }
    }
}
