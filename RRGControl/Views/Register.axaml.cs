using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace RRGControl.Views
{
    public partial class Register : UserControl
    {
        public Register()
        {
            InitializeComponent();
            mtbInput.AddHandler(KeyDownEvent, Textbox_KeyDown, RoutingStrategies.Tunnel);
        }

        private async void Textbox_KeyDown(object sender, KeyEventArgs e)
        {
            var vm = DataContext as ViewModels.RegisterViewModel;
            var tb = sender as TextBox;
            if (tb?.Text != null && vm != null)
            {
                vm.TextboxValue = tb.Text.Trim('_');
                if (e.Key == Key.Enter)
                {
                    await vm.Write();
                }
            }
        }

        private async void R_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModels.RegisterViewModel;
            await vm.Read();
        }

        private async void W_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ViewModels.RegisterViewModel;
            await vm.Write();
        }
    }
}
