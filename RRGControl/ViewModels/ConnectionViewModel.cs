using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRGControl.ViewModels
{
    public class ConnectionViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public new event PropertyChangedEventHandler? PropertyChanged;
        public ConnectionViewModel(MyModbus.Connection c)
        {
            mConnection = c;
            Units = new ObservableCollection<RRGUnitViewModel>();
            foreach (var item in c.Units)
            {
                var u = new RRGUnitViewModel(item.Value);
                u.PropertyChanged += U_PropertyChanged;
                Units.Add(u);
            }
            base.PropertyChanged += PropertyChanged;
        }

        private void U_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }

        private MyModbus.Connection mConnection;

        public bool Remote { get => mConnection.Mapping.Type == MyModbus.ModbusType.TCP; }
        public bool Local { get => mConnection.Mapping.Type == MyModbus.ModbusType.RTU; }
        public string Port { get => mConnection.Mapping.Port; }
        public ObservableCollection<RRGUnitViewModel> Units { get; }
    }
}
