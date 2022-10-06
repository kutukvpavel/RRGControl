using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RRGControl.Adapters
{
    public class Script
    {
        [JsonConstructor]
        public Script() { }
        public Script(string n, string c, List<Tuple<int, Packet>> src)
        {
            Name = n;
            Comment = c;
            Commands = src;
        }
        public Script(List<Script> src)
        {
            Name = "";
            foreach (var item in src)
            {
                Name += $"{item.Name};";
                Commands.AddRange(item.Commands);
            }
            Comment = "Compiled script";
        }

        public string Name { get; set; } = "Example Script";
        public string Comment { get; set; } = "...";
        public List<Tuple<int, Packet>> Commands { get; set; } = new List<Tuple<int, Packet>>();
        
        public int GetDuration()
        {
            return Commands.Sum(x => x.Item1);
        }
        public Dictionary<int, Packet> Compile()
        {
            var res = new Dictionary<int, Packet>(Commands.Count);
            int time = 0;
            foreach (var item in Commands)
            {
                res.Add(time, item.Item2);
                time += item.Item1;
            }
            return res;
        }

        public static Script? FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Script>(json, ConfigProvider.SerializerOptions);
        }
    }
}
