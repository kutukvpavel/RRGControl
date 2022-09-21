using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Avalonia.ReactiveUI;
using System.Collections;

namespace RRGControl.ViewModels
{
    public class DashboardViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public new event PropertyChangedEventHandler? PropertyChanged;

        public DashboardViewModel(MyModbus.RRGUnit u)
        {
            mUnit = u;
            mUnit.PropertyChanged += MUnit_PropertyChanged;
        }

        private void MUnit_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }

        private readonly MyModbus.RRGUnit mUnit;
        private bool mAddrContention = false;

        public string Address
        {
            get => mUnit.Address.ToString();
            set
            {
                if (!mUnit.ChangeAddress(ushort.Parse(value))) AddressContention = true;
            }
        }
        public bool AddressContention
        {
            get => mAddrContention;
            set
            {
                mAddrContention = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AddressContention)));
            }
        }
        public string Mode { get => mUnit.Mode; }
        public string Measured 
        { 
            get => $"{mUnit.Flowrate.ToString(ConfigProvider.Settings.FlowrateDisplayFormat)}  ({mUnit.Flowrate / mUnit.MaxFlowrate * 100:F1}%)"; 
        }
        public string Setpoint 
        { 
            get => mUnit.Setpoint.ToString(ConfigProvider.Settings.FlowrateDisplayFormat);
            set => mUnit.Setpoint = double.Parse(value);
        }
        public string UnitModel => mUnit.UnitConfig.Model;
        public string Units => mUnit.UnitConfig.ConversionUnits;
        public string MaxFlow => mUnit.MaxFlowrate.ToString(ConfigProvider.Settings.FlowrateDisplayFormat);

        public void SetStateOpen()
        {
            mUnit.Mode = ConfigProvider.OpenModeName;
        }
        public void SetStateClosed()
        {
            mUnit.Mode = ConfigProvider.ClosedModeName;
        }
        public void SetStateRegulate()
        {
            mUnit.Mode = ConfigProvider.RegulateModeName;
        }
        public bool ValidateSetpoint(string s)
        {
            if (!double.TryParse(s, out double v)) return false;
            if (!mUnit.ValidateSetpoint(v)) return false;
            return true;
        }
        public bool ValidateAddress(string s)
        {
            if (!ushort.TryParse(s, out ushort v)) return false;
            if (!mUnit.ValidateAddress(v)) return false;
            return true;
        }
    }
}
