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
            Command.PropertyChanged += (o, e) =>
            {
                this.RaisePropertyChanged(nameof(DisplaySummary));
            };
        }

        public Models.ScriptCommand Command { get; }

        public string DisplaySummary
        {
            get
            {
                var activeParts = Command.UnitSetpoints.Where(x => x.Setpoint > 0).Select(x => $"{x.UnitName} : {x.Setpoint}");
                string flowsText = activeParts.Any() ? string.Join("; ", activeParts) : "No active flows";
                return $"Time: {Command.Duration} s\t|\t{flowsText}";
            }
        }
    }
}