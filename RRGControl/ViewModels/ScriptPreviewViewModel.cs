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

        public event EventHandler? ReplotRequested;

        public ScriptPreviewViewModel()
        {
            
        }
        public ScriptPreviewViewModel(Dictionary<int, Adapters.Packet[]> compiled, int duration, Models.Network? n, int progress)
        {
            UpdatePreview(compiled, duration, n, progress);
        }

        public void UpdatePreview(Dictionary<int, Adapters.Packet[]> compiled, int duration, Models.Network? n, int progress)
        {
            var units = compiled.Values.Select(x => x.Select(y => y.UnitName)).Flatten().Cast<string>().Distinct();
            Data.Clear();
            foreach (var item in units)
            {
                try
                {
                    var tmp = compiled.Select(x =>
                    {
                        return new KeyValuePair<int, Adapters.Packet?>(x.Key, x.Value.LastOrDefault(
                            y => y.UnitName == item && y.RegisterName == ConfigProvider.SetpointRegName && y.TryConvertValueToDouble(out _)));
                    }).Where(x => x.Value != null).ToDictionary(x => (double)x.Key, x => x.Value!.ConvertValueToDouble());
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
                    CurrentProgressX = progress * duration / 100.0;
                }
                catch (Exception ex)
                {
                    ErrorUnits.Add(new Tuple<string, Exception>(item, ex));
                }
            }
            ReplotPreview();
        }

        public void ReplotPreview()
        {
            ReplotRequested?.Invoke(this, new EventArgs());
        }

        public List<MyPlotData> Data { get; } = new();
        public List<Tuple<string, Exception>> ErrorUnits { get; } = new();
        public double CurrentProgressX { get; set; }
    }
}
