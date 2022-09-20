using Avalonia;
using Avalonia.Input;
using Avalonia.Controls;

namespace RRGControl.Views
{
    public partial class Dashboard : UserControl
    {
        public Dashboard()
        {
            InitializeComponent();
        }

        private void Setpoint_OnKeyDown(object sender, KeyEventArgs e)
        {
            GetValidationHarness(sender, e.Key, out ViewModels.DashboardViewModel? vm, out string txt);
            if (vm?.ValidateSetpoint(txt) ?? false) vm.Setpoint = txt;
        }

        private void Address_OnKeyDown(object sender, KeyEventArgs e)
        {
            GetValidationHarness(sender, e.Key, out ViewModels.DashboardViewModel? vm, out string txt);
            if (vm?.ValidateAddress(txt) ?? false) vm.Address = txt;
        }

        private void GetValidationHarness(object sender, Key k, out ViewModels.DashboardViewModel? vm, out string txt)
        {
            if (k == Key.Enter)
            {
                vm = DataContext as ViewModels.DashboardViewModel;
                var tb = sender as TextBox;
                txt = tb?.Text ?? "";
            }
            else
            {
                vm = null;
                txt = "";
            }
        }
    }
}
