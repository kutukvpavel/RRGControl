using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RRGControl.Models
{
    public class GasModel
    {
        public List<string> Aliases { get; set; } = new List<string>();

        public double Factor { get; set; }
        public string DisplayName => Aliases.Count > 0 ? Aliases[0] : "Unknown Gas";
    }
}