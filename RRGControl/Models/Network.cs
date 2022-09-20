using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRGControl.Models
{
    public class Network
    {
        public static event EventHandler<string>? LogEvent;

        public Network(MyModbus.ModbusProvider p, List<MyModbus.RRGUnitMapping> m)
        {
            Provider = p;
            Connections = new List<MyModbus.Connection>(m.Count);
            foreach (var item in m)
            {
                try
                {
                    Connections.Add(new MyModbus.Connection(p, item));
                }
                catch (Exception ex)
                {
                    LogEvent?.Invoke(this, $"Can't create connection: {ex}");
                }
            }
        }

        public List<MyModbus.Connection> Connections { get; }
        public MyModbus.ModbusProvider Provider { get; }

        public void Scan()
        {
            foreach (var item in Connections)
            {
                item.Scan();
            }
        }
    }
}
