using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RRGControl.Adapters
{
    public class Script
    {
        public class Element
        {
            [JsonConstructor]
            public Element() : base() { }

            public Element(int dur, Packet p)
            {
                Duration = dur;
                Command = p;
            }

            [JsonProperty(Required = Required.Always)]
            public int Duration { get; set; }
            [JsonProperty(Required = Required.Always)]
            public Packet Command { get; set; }
        }


        [JsonConstructor]
        public Script() { }
        public Script(string n, string c, List<Element> src)
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
        public List<Element> Commands { get; set; } = new List<Element>();
        
        public int GetDuration()
        {
            return Commands.Sum(x => x.Duration);
        }
        public Dictionary<int, Packet> Compile()
        {
            var res = new Dictionary<int, Packet>(Commands.Count);
            int time = 0;
            foreach (var item in Commands)
            {
                res.Add(time, item.Command);
                time += item.Duration;
            }
            return res;
        }

        public static Script? FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Script>(json, ConfigProvider.SerializerOptions);
        }
    }
}
