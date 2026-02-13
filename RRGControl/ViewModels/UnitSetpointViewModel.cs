using ReactiveUI;

namespace RRGControl.ViewModels
{
    public class UnitSetpointViewModel : ViewModelBase
    {
        public UnitSetpointViewModel(Models.UnitSetpoint s)
        {
            mSetpoint = s;
        }

        private readonly Models.UnitSetpoint mSetpoint;

        public string UnitName => mSetpoint.UnitName;
        public double Setpoint
        {
            get => mSetpoint.Setpoint;
            set
            {
                if (mSetpoint.Setpoint == value) return;
                mSetpoint.Setpoint = value;
                this.RaisePropertyChanged();
            }
        }
    }
}