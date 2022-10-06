using Avalonia.Controls;
using System;

namespace RRGControl.Views
{
    public partial class ScriptPreview : Window
    {
        public ScriptPreview()
        {
            InitializeComponent();
        }

        private ViewModels.ScriptPreviewViewModel? ViewModel => DataContext as ViewModels.ScriptPreviewViewModel;

        private void OnOpened(object sender, EventArgs e)
        {
            if (ViewModel == null) return;
            foreach (var item in ViewModel.Data)
            {
                Plot1.Plot.AddScatterStep(item.Value.Item1, item.Value.Item2, label: item.Key);
            }
            Plot1.Refresh();
        }
    }
}
