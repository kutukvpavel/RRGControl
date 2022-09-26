using Avalonia.Media;
using System.ComponentModel;

namespace RRGControl.ViewModels
{
    public class RRGUnitViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public new event PropertyChangedEventHandler? PropertyChanged;
        public RRGUnitViewModel(MyModbus.RRGUnit u)
        {
            mUnit = u;
            Registers = new RegisterViewModel[mUnit.Registers.Count];
            int i = 0;
            foreach (var item in mUnit.Registers)
            {
                Registers[i++] = new RegisterViewModel(item.Value);
            }
            Dashboard = new DashboardViewModel(u);
            mUnit.PropertyChanged += MUnit_PropertyChanged;
            base.PropertyChanged += PropertyChanged;
        }

        private void MUnit_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
            if (e.PropertyName == nameof(mUnit.Present))
            {
                mUnit.ReadAll();
            }
        }

        private MyModbus.RRGUnit mUnit;

        public IBrush TabColor { get => mUnit.Present ? Brushes.LightGreen : Brushes.Orange; }
        public string Name => mUnit.UnitConfig.Name;
        public RegisterViewModel[] Registers { get; }
        
        //Default dashboard
        public DashboardViewModel Dashboard { get; }
    }
}
