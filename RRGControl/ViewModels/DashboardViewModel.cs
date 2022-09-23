using Avalonia.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace RRGControl.ViewModels
{
    public class DashboardViewModel : ViewModelBase, INotifyPropertyChanged, INotifyDataErrorInfo
    {
        private static string[] UnknownError = { "Unknown error" };

        public new event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public DashboardViewModel(MyModbus.RRGUnit u)
        {
            mUnit = u;
            mUnit.PropertyChanged += MUnit_PropertyChanged;
            base.PropertyChanged += PropertyChanged;
            mTimer = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, ConfigProvider.Settings.AutoUpdateIntervalMs), 
                DispatcherPriority.Background, Timer_Callback);
            if (u.UnitConfig.EnableAutoupdate) mTimer.Start();
        }

        private void MUnit_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e); //Keep same property names for simplicity!
        }
        private void Timer_Callback(object sender, EventArgs e)
        {
            if (mUnit.Present) mUnit.Registers[ConfigProvider.MeasuredRegName].Read(); //Intentional
        }
        private void AddError(string prop, string msg)
        {
            if (!Errors.ContainsKey(prop)) Errors.Add(prop, string.Empty);
            Errors[prop] = msg;
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(prop));
        }
        private void RemoveError(string prop)
        {
            if (Errors.ContainsKey(prop))
            {
                Errors.Remove(prop);
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(prop));
            }
        }

        private readonly MyModbus.RRGUnit mUnit;
        private bool mAddrContention = false;
        private DispatcherTimer mTimer;

        public string Address
        {
            get => mUnit.Address.ToString();
            set
            {
                Task.Run(() => { if (!mUnit.ChangeAddress(ushort.Parse(value)).Wait(1000)) AddressContention = true; });
            }
        }
        public bool AddressContention
        {
            get => mAddrContention;
            set
            {
                mAddrContention = value;
                Dispatcher.UIThread.InvokeAsync(() =>
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AddressContention))));
            }
        }
        public string Mode { get => mUnit.Mode; }
        public string Measured 
        { 
            get => $"{mUnit.Measured.ToString(mUnit.UnitConfig.FlowrateNumberFormat)}  ({mUnit.Measured / mUnit.MaxFlowrate * 100:F1}%)"; 
        }
        public string Setpoint 
        { 
            get => mUnit.Setpoint.ToString(mUnit.UnitConfig.FlowrateNumberFormat);
            set => mUnit.Setpoint = double.Parse(value);
        }
        public string UnitModel => mUnit.UnitConfig.Model;
        public string Units => mUnit.UnitConfig.ConversionUnits;
        public string MaxFlow => mUnit.MaxFlowrate.ToString(mUnit.UnitConfig.FlowrateNumberFormat);
        public bool AutoUpdate { get => mTimer.IsEnabled; set => mTimer.IsEnabled = value; }

        public bool HasErrors => Errors.Count > 0;
        private Dictionary<string, string> Errors = new Dictionary<string, string>();

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
            bool res = double.TryParse(s, out double v) && mUnit.ValidateSetpoint(v);
            if (res) RemoveError(nameof(Setpoint));
            else AddError(nameof(Setpoint), "Invalid value");
            return res;
        }
        public bool ValidateAddress(string s)
        {
            bool res = ushort.TryParse(s, out ushort v) && mUnit.ValidateAddress(v);
            if (res) RemoveError(nameof(Address));
            else AddError(nameof(Address), "Invalid value");
            return res;
        }

        public IEnumerable GetErrors(string? propertyName)
        {
            try
            {
                if (!HasErrors) return Enumerable.Empty<string>();
                return new string[] { Errors[propertyName ?? string.Empty] };
            }
            catch (KeyNotFoundException)
            {
                return UnknownError;
            }
        }
    }
}
