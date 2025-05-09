﻿using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#pragma warning disable CS0618
#pragma warning disable CS8618

namespace RRGControl.Adapters
{
    public class Script
    {
        public static event EventHandler<string>? LogEvent;

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
            using TextReader tr = new StringReader(json);
            using JsonReader r = new JsonTextReader(tr);
            using JsonValidatingReader vr = new JsonValidatingReader(r);
            vr.Schema = JsonSchema.Parse(Schema);
            IList<string> msgs = new List<string>();
            vr.ValidationEventHandler += (o, e) => { msgs.Add($"\t!! Validation message at {e.Path}: {e.Message}"); };
            JsonSerializer s = JsonSerializer.Create(ConfigProvider.SerializerOptions);
            Script? res;
            try
            {
                res = s.Deserialize<Script>(vr);
                if (msgs.Any()) LogEvent?.Invoke("Script Validation", string.Join('\n', msgs));
            }
            catch (Exception)
            {
                LogEvent?.Invoke("Script Validation", string.Join('\n', msgs));
                throw;
            }
            return res;
        }

        private static readonly string Schema = @"
{
	'$schema': 'http://json-schema.org/draft-04/schema#',
	'type': 'object',
	'properties': {
		'Name': {
			'type': 'string',
            'required': true
		},
		'Comment': {
			'type': 'string',
            'required': false
		},
		'Commands': {
			'type': 'array',
            'required': true,
			'items': {
				'type': 'object',
				'properties': {
					'Duration': {
						'type': 'integer',
                        'required': true
					},
					'Command': {
						'type': 'object',
                        'required': true,
						'properties': {
							'UnitName': {
								'type': 'string',
                                'required': true
							},
							'RegisterName': {
								'type': 'string',
                                'required': true
							},
							'Value': {
								'type': 'string',
                                'required': true
							},
							'ConvertUnits': {
								'type': 'boolean',
                                'required': false
							}
						}
                    }
                }
			}
		}
	}
}";
    }
}
