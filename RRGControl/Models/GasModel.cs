using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RRGControl.Models
{
    public class GasModel
    {
        [JsonPropertyName("Aliases")]
        public List<string> Aliases { get; set; } = new List<string>();

        [JsonPropertyName("Factor")]
        public double Factor { get; set; }
        public string DisplayName => Aliases.Count > 0 ? Aliases[0] : "Unknown Gas";
    }
}