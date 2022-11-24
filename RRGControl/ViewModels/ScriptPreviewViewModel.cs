using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;

namespace RRGControl.ViewModels
{
    public class ScriptPreviewViewModel
    {
        public class MyPlotData
        {
            public MyPlotData(string l, double[] x, double[] y, bool offline)
            {
                LegendEntry = l;
                DataX = x;
                DataY = y;
                Offline = offline;
            }

            public string LegendEntry { get; }
            public double[] DataX { get; }
            public double[] DataY { get; }
            public bool Offline { get; }
        }

        public ScriptPreviewViewModel(Dictionary<int, Adapters.Packet> compiled, int duration, Models.Network? n)
        {
            mScript = compiled;
            var units = mScript.Values.DistinctBy(x => x.UnitName).Select(x => x.UnitName);
            Data = new List<MyPlotData>();
            foreach (var item in units)
            {
                try
                {
                    var tmp = compiled.Where(x => x.Value.UnitName == item && x.Value.RegisterName == ConfigProvider.SetpointRegName
                        && x.Value.TryConvertValueToDouble(out _))
                            .ToDictionary(x => (double)x.Key, x => x.Value.ConvertValueToDouble());
                    try
                    {
                        var lst = tmp.Last();
                        tmp.Add(duration, lst.Value);
                    }
                    catch (InvalidOperationException)
                    { }
                    var u = n?.FindUnitByName(item);
                    var ol = u?.Present ?? false ? "Online" : "Offline";
                    Data.Add(new MyPlotData($"{item}, {u?.UnitConfig?.ConversionUnits ?? "N/A"}, {ol}",
                        tmp.Keys.ToArray(), tmp.Values.ToArray(), !(u?.Present ?? false)));
                }
                catch (Exception ex)
                {
                    ErrorUnits.Add(new Tuple<string, Exception>(item, ex));
                }
            }
        }

        public List<MyPlotData> Data { get; }
        public List<Tuple<string, Exception>> ErrorUnits { get; } = new List<Tuple<string, Exception>>();

        private readonly Dictionary<int, Adapters.Packet> mScript;
    }
}
