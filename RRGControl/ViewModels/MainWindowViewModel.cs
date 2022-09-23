using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace RRGControl.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public static event EventHandler<string>? LogEvent;
        public new event PropertyChangedEventHandler? PropertyChanged;

        private readonly Models.Network mNetwork;
        private string mStatus = "Ready.";
        public MainWindowViewModel(Models.Network n)
        {
            mNetwork = n;
            Connections = new List<ConnectionViewModel>(n.Connections.Count);
            foreach (var item in n.Connections)
            {
                try
                {
                    var c = new ConnectionViewModel(item);
                    c.PropertyChanged += C_PropertyChanged;
                    Connections.Add(c);
                }
                catch (Exception ex)
                {
                    LogEvent?.Invoke(this, ex.ToString());
                }
            }
            base.PropertyChanged += PropertyChanged;
        }

        private void C_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }

        public List<ConnectionViewModel> Connections { get; }

        public string Status
        {
            get => mStatus;
            set
            {
                mStatus = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
            }
        }

        public async Task RescanNetwork()
        {
            try
            {
                Status = "Scanning network...";
                await mNetwork.Scan();
                Status = "Network scan OK.";
            }
            catch (Exception ex)
            {
                LogEvent?.Invoke(this, ex.ToString());
                Status = "Network scan failed";
            }
        }
        public async Task ReadAll()
        {
            try
            {
                Status = "Reading all unit registers, please wait...";
                await mNetwork.ReadAll();
                Status = "Reading all units OK.";
            }
            catch (Exception ex)
            {
                LogEvent?.Invoke(this, ex.ToString());
                Status = "Reading all units failed";
            }
        }
    }
}
