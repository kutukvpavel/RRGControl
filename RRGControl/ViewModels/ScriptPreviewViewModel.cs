using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;

namespace RRGControl.ViewModels
{
    public class ScriptPreviewViewModel
    {
        public ScriptPreviewViewModel(Dictionary<int, Adapters.Packet> compiled)
        {
            mScript = compiled;
            var units = mScript.Values.DistinctBy(x => x.UnitName).Select(x => x.UnitName);
            Data = new Dictionary<string, Tuple<double[], double[]>>();
            foreach (var item in units)
            {
                var tmp = compiled.Where(x => x.Value.UnitName == item && x.Value.RegisterName == ConfigProvider.SetpointRegName)
                    .ToDictionary(x => (double)x.Key, x => double.Parse(x.Value.Value));
                Data.Add(item, new Tuple<double[], double[]>(tmp.Keys.ToArray(), tmp.Values.ToArray()));
            }
        }

        public Dictionary<string, Tuple<double[], double[]>> Data { get; }

        private Dictionary<int, Adapters.Packet> mScript;
    }
}
