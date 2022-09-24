using System.ComponentModel;

namespace RRGControl.ViewModels
{
    public class UnitFlowrateViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public new event PropertyChangedEventHandler? PropertyChanged;

        public UnitFlowrateViewModel(RRGUnitViewModel u)
        {
            mUnit = u;
            u.PropertyChanged += U_PropertyChanged;
            base.PropertyChanged += PropertyChanged;
        }

        private void U_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Flowrate)));
        }

        public string Flowrate { get => mUnit.Dashboard.Measured; }
        public string Units { get => mUnit.Dashboard.Units; }
        public string Name { get => mUnit.Name; }

        private RRGUnitViewModel mUnit;
    }
}
