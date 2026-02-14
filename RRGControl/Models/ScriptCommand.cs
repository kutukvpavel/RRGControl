using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RRGControl.Models
{
    public class ScriptCommand
    {
        public ScriptCommand()
        {
            UnitSetpoints = Array.Empty<UnitSetpoint>();
        }
        public ScriptCommand(IEnumerable<UnitSetpoint> setpoints)
        {
            UnitSetpoints = setpoints.ToArray();
        }
        public ScriptCommand(int duration, IEnumerable<UnitSetpoint> setpoints) : this(setpoints)
        {
            Duration = duration;
        }
        public ScriptCommand(Adapters.Script.Element elementToImport) 
            : this(elementToImport.Duration, elementToImport.Packets.Select(x => new UnitSetpoint(x)))
        { }

        public int Duration { get; set; }
        public UnitSetpoint[] UnitSetpoints { get; }

        public Adapters.Script.Element GetScriptAdapterElement()
        {
            return new Adapters.Script.Element(Duration, UnitSetpoints.Select(x =>
                new Adapters.Packet(x.UnitName, ConfigProvider.SetpointRegName, x.Setpoint.ToString(CultureInfo.InvariantCulture))).ToArray());
        }
    }
}