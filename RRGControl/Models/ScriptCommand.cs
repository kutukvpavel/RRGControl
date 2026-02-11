using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace RRGControl.Models
{
    public class ScriptCommand : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public ScriptCommand()
        {
            UnitSetpoints = new();
        }
        public ScriptCommand(IEnumerable<UnitSetpoint> setpoints)
        {
            UnitSetpoints = new(setpoints);
        }
        public ScriptCommand(int duration, IEnumerable<UnitSetpoint> setpoints) : this(setpoints)
        {
            Duration = duration;
        }

        private int _duration = 10;
        public int Duration
        {
            get => _duration;
            set 
            {
                if (_duration != value)
                {
                    _duration = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Duration)));
                }
            }
        }

        public ObservableCollection<UnitSetpoint> UnitSetpoints { get; }

        public Adapters.Script.Element GetScriptAdapterElement()
        {
            return new Adapters.Script.Element(Duration, UnitSetpoints.Select(x =>
                new Adapters.Packet(x.UnitName, ConfigProvider.SetpointRegName, x.Setpoint.ToString(CultureInfo.InvariantCulture))).ToArray());
        }
    }
}