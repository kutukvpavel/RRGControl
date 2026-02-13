namespace RRGControl.Models
{
    public class UnitSetpoint
    {
        public UnitSetpoint(string name)
        {
            UnitName = name;
        }

        public string UnitName { get; }
        public double Setpoint { get; set; }
    }
}