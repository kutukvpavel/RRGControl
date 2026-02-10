using ReactiveUI;
using System.Text.Json.Serialization;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;

namespace RRGControl.Models
{
    public class ScriptCommandModel : ReactiveObject
    {
        private double _duration;
        public double Duration
        {
            get => _duration;
            set 
            {
                this.RaiseAndSetIfChanged(ref _duration, value);
                this.RaisePropertyChanged(nameof(DisplaySummary));
            }
        }

        [JsonPropertyName("UnitSetpoints")]
        public ObservableCollection<UnitSetpointModel> UnitSetpoints { get; set; } = new();

        [JsonIgnore]
        public string DisplaySummary
        {
            get
            {

                List<string> activeParts = new();
                foreach (var usp in UnitSetpoints)
                {
                    if (usp.Setpoint > 0)
                    {
                        activeParts.Add($"{usp.UnitName} : {usp.Setpoint}");
                    }
                }

                string flowsText = "No active flows";
                if (activeParts.Count > 0)
                {
                    flowsText = string.Join("; ", activeParts);
                }

                return $"Time: {Duration} s   |   {flowsText}";
            }
        }
    }
}