using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace RRGControl.Models
{
    public class UnitModel
    {
        public string Name { get; set; } = string.Empty;
    }
    public class ConfigModel
    {
        public Dictionary<string, UnitModel> Units { get; set; } = new Dictionary<string, UnitModel>();
    }
}