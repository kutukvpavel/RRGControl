using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RRGControl.Adapters
{
    public class Packet
    {
        [JsonConstructor]
        public Packet()
        {

        }
        public Packet(string u, string r, string v)
        {
            UnitName = u;
            RegisterName = r;
            Value = v;
        }

        public string UnitName { get; set; }
        public string RegisterName { get; set; }
        public string Value { get; set; }

        public string GetJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static Packet? FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Packet>(json);
        }
        public static Packet FromRegister(string unitName, MyModbus.ModbusRegister r)
        {
            return new Packet(unitName, r.Base.Name, r.GetValueStringRepresentation());
        }
    }
}
