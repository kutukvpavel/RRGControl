using System;
using System.Linq;
using ReactiveUI;

namespace RRGControl.ViewModels
{
    public class ScriptCommandViewModel : ViewModelBase
    {
        public ScriptCommandViewModel(Models.ScriptCommand sc)
        {
            Command = sc;
            Setpoints = Command.UnitSetpoints.Select(x => new UnitSetpointViewModel(x)).ToArray();
            foreach (var item in Setpoints)
            {
                item.PropertyChanged += (o, e) =>
                {
                    this.RaisePropertyChanged(nameof(DisplaySummary));
                };
            }
        }

        public Models.ScriptCommand Command { get; }

        public UnitSetpointViewModel[] Setpoints { get; }
        public int Duration
        {
            get => Command.Duration;
            set
            {
                if (Command.Duration == value) return;
                Command.Duration = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(DisplaySummary));
            }
        }

        public string DisplaySummary
        {
            get
            {
                var activeParts = Setpoints.Where(x => x.Setpoint > 0).Select(x => $"{x.UnitName} : {x.Setpoint}");
                string flowsText = activeParts.Any() ? string.Join("; ", activeParts) : "No active flows";
                return $"Time: {Duration} s\t|\t{flowsText}";
            }
        }
    }
}