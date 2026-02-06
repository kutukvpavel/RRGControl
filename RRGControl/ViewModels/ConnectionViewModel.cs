using Avalonia.Media;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace RRGControl.ViewModels
{
    public class ConnectionViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public new event PropertyChangedEventHandler? PropertyChanged;
        public ConnectionViewModel(MyModbus.Connection c)
        {
            mConnection = c;
            mConnection.PropertyChanged += MConnection_PropertyChanged;
            Units = new ObservableCollection<RRGUnitViewModel>();
            foreach (var item in c.Units)
            {
                var u = new RRGUnitViewModel(item.Value);
                u.PropertyChanged += U_PropertyChanged;
                Units.Add(u);
            }
            base.PropertyChanged += PropertyChanged;
            if (App.Current != null) App.Current.ActualThemeVariantChanged += ThemeVariant_Changed;
        }

        private void UpdateColors()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TabColor)));
        }
        private void ThemeVariant_Changed(object? sender, EventArgs e)
        {
            UpdateColors();
        }
        private void MConnection_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(mConnection.IsUp)) UpdateColors();
        }
        private void U_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }

        private MyModbus.Connection mConnection;

        public IBrush TabColor { get => mConnection.IsUp ? App.GreenOK : App.OrangeWarning; }
        public bool Remote { get => mConnection.Mapping.Type == MyModbus.ModbusType.TCP; }
        public bool Local { get => mConnection.Mapping.Type == MyModbus.ModbusType.RTU; }
        public string Port { get => mConnection.Mapping.Port; }
        public ObservableCollection<RRGUnitViewModel> Units { get; }
    }
}
