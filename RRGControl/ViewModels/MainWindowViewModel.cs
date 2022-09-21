using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace RRGControl.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public static event EventHandler<string>? LogEvent;
        public new event EventHandler<PropertyChangedEventArgs>? PropertyChanged;

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
                    Connections.Add(new ConnectionViewModel(item));
                }
                catch (Exception ex)
                {
                    LogEvent?.Invoke(this, ex.ToString());
                }
            }
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

        public void RescanNetwork()
        {
            try
            {
                mNetwork.Scan();
                Status = "Network scan finished.";
            }
            catch (Exception ex)
            {
                LogEvent?.Invoke(this, ex.ToString());
                Status = "Network scan failed";
            }
        }
        public void ReadAll()
        {
            try
            {
                mNetwork.ReadAll();
                Status = "Reading all units finished.";
            }
            catch (Exception ex)
            {
                LogEvent?.Invoke(this, ex.ToString());
                Status = "Reading all units failed";
            }
        }
    }
}
