using Newtonsoft.Json;
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

        public class LegacyScript
        {
            public class Element
            {
                [JsonConstructor]
                public Element() : base() { }
                [JsonProperty(Required = Required.Always)]
                public int Duration { get; set; }
                [JsonProperty(Required = Required.Always)]
                public Packet Command { get; set; }
            }
            [JsonConstructor]
            public LegacyScript() { }
            public string Name { get; set; }
            public string Comment { get; set; }
            public List<Element> Commands { get; set; } = new List<Element>();
        }

        public class Element
        {
            [JsonConstructor]
            public Element() : base() { }
            public Element(LegacyScript.Element src)
            {
                Duration = src.Duration;
                Packets = new Packet[] { src.Command };
            }

            public Element(int dur, Packet[] p)
            {
                Duration = dur;
                Packets = p;
            }

            [JsonProperty(Required = Required.Always)]
            public int Duration { get; set; }
            [JsonProperty(Required = Required.Always)]
            public Packet[] Packets { get; set; }
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
        public Script(LegacyScript src)
        {
            Name = src.Name;
            Comment = src.Comment;
            Commands = src.Commands.Select(x => new Element(x)).ToList();
        }

        public string Name { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public List<Element> Commands { get; set; } = new List<Element>();

        public int GetDuration()
        {
            return Commands.Sum(x => x.Duration);
        }
        public Dictionary<int, Packet[]> Compile()
        {
            var res = new Dictionary<int, Packet[]>(Commands.Count);
            int time = 0;
            foreach (var item in Commands)
            {
                res.Add(time, item.Packets);
                time += item.Duration;
            }
            return res;
        }

        private static T? FromJson<T>(string json, string schema)
        {
            using TextReader tr = new StringReader(json);
            using JsonReader r = new JsonTextReader(tr);
            using JsonValidatingReader vr = new(r);
            vr.Schema = JsonSchema.Parse(schema);
            IList<string> msgs = new List<string>();
            vr.ValidationEventHandler += (o, e) => { msgs.Add($"\t!! Validation message at {e.Path}: {e.Message}"); };
            JsonSerializer s = JsonSerializer.Create(ConfigProvider.SerializerOptions);
            T? res = default;
            try
            {
                res = s.Deserialize<T>(vr);
                if (msgs.Any()) LogEvent?.Invoke(LogTag, string.Join('\n', msgs));
            }
            catch (Exception ex)
            {
                LogEvent?.Invoke(LogTag, ex.Message);
                throw;
            }
            return res;
        }
        public static Script? FromJson(string json)
        {
            Script? res = null;
            try
            {
                res = FromJson<Script>(json, Schema);
            }
            catch (Exception)
            {
                LogEvent?.Invoke(LogTag, "Trying legacy schema...");
                var ls = FromJson<LegacyScript>(json, LegacySchema);
                res = (ls == null) ? null : new Script(ls);
            }
            return res;
        }

        private static readonly string LogTag = "Script Validation";
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
                    'Packets': {
                        'type': 'array',
                        'required': true,
                        'items': {
                            'type': 'object',
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
	}
}";
        private static readonly string LegacySchema = @"
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
