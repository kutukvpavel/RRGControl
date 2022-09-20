using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRGControl.Models
{
    public class Network
    {
        public Network(MyModbus.ModbusProvider p, List<MyModbus.RRGUnitMapping> m)
        {
            Provider = p;
            Connections = new MyModbus.Connection[m.Count];
            int i = 0;
            foreach (var item in m)
            {
                Connections[i++] = new MyModbus.Connection(p, item);
            }
        }

        public MyModbus.Connection[] Connections { get; }
        public MyModbus.ModbusProvider Provider { get; }
    }
}
