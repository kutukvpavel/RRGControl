using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace RRGControl.Models
{
    public class UnitModel
    {
        [JsonPropertyName("Name")]
        public string Name { get; set; } = string.Empty;
    }
    public class ConfigModel
    {
        [JsonPropertyName("Units")]
        public Dictionary<string, UnitModel> Units { get; set; } = new Dictionary<string, UnitModel>();
    }
}