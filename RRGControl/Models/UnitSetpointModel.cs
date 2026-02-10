using ReactiveUI;
using System;

namespace RRGControl.Models
{
    public class UnitSetpointModel : ReactiveObject
    {
        public string UnitId { get; set; } = string.Empty;
        public string UnitName { get; set; } = string.Empty;

        public string FullDisplayName => $"{UnitId}_{UnitName}";

        private string _gasType = "N2";
        public string GasType 
        { 
            get => _gasType;
            set => this.RaiseAndSetIfChanged(ref _gasType, value);
        }

        private double _setpoint;
        public double Setpoint
        {
            get => _setpoint;
            set {
                this.RaiseAndSetIfChanged(ref _setpoint, value);
                this.RaisePropertyChanged(nameof(DisplaySummary));
            }
        }

        public string DisplaySummary => UnitName + " (" + GasType + "): " + Setpoint;
    }
}