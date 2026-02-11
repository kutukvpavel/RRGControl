using System;
using System.ComponentModel;

namespace RRGControl.Models
{
    public class UnitSetpoint : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public UnitSetpoint(string name)
        {
            UnitName = name;
        }

        public string UnitName { get; }

        private double _setpoint = 0;
        public double Setpoint
        {
            get => _setpoint;
            set {
                if (_setpoint != value)
                {
                    _setpoint = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Setpoint)));
                }
            }
        }
    }
}