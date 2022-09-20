using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRGControl.ViewModels
{
    public class ConnectionViewModel : ViewModelBase
    {
        public ConnectionViewModel(MyModbus.Connection c)
        {
            mConnection = c;
            Units = new ObservableCollection<RRGUnitViewModel>();
            foreach (var item in c.Units)
            {
                Units.Add(new RRGUnitViewModel(item.Value));
            }
        }

        private MyModbus.Connection mConnection;

        public bool Remote { get => mConnection.Mapping.Type == MyModbus.ModbusType.TCP; }
        public bool Local { get => mConnection.Mapping.Type == MyModbus.ModbusType.RTU; }
        public string Port { get => mConnection.Mapping.Port; }
        public ObservableCollection<RRGUnitViewModel> Units { get; }
    }
}
