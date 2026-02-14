using System;

namespace RRGControl.Models
{
    public class UnitSetpoint
    {
        public UnitSetpoint(string name)
        {
            UnitName = name;
        }
        public UnitSetpoint(Adapters.Packet packet) : this(packet.UnitName)
        {
            //ToDo: support scripted commands other than Setpoint (at least view them gracefully)
            //Breaks legacy scrpts for now
            if (packet.RegisterName != ConfigProvider.SetpointRegName) throw new InvalidOperationException();
            Setpoint = packet.ConvertValueToDouble();
        }

        public string UnitName { get; }
        public double Setpoint { get; set; }
    }
}