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
            lstLeft.SelectionChanged += LstLeft_SelectionChanged;
            lstRight.SelectionChanged += LstRight_SelectionChanged;
        }

        private ViewModels.ScriptsViewModel? ViewModel => DataContext as ViewModels.ScriptsViewModel;

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
