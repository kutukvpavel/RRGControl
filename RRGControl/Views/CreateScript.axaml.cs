using Avalonia.Controls;
using RRGControl.ViewModels;
using Avalonia.Input;

namespace RRGControl.Views
{
    public partial class CreateScript : Window 
    {
        public CreateScript()
        {
            InitializeComponent();
            this.DataContextChanged += (s, e) =>
            {
                if (DataContext is CreateScriptViewModel vm)
                {
                    vm.PlotUpdateRequested += (sender, args) => vm.PreviewViewModel.ReplotPreview();
                    vm.PreviewViewModel.ReplotPreview();
                }
            };
        }
    }
}