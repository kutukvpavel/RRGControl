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

        public class ReplotRequestedEventArgs : EventArgs
        {
            public ReplotRequestedEventArgs(bool autoscale = true)
            {
                AutoScale = autoscale;
            }

            public bool AutoScale { get; set; }
        }

        public event EventHandler<ReplotRequestedEventArgs>? ReplotRequested;

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
                            y => y.UnitName == item && y.RegisterName == ConfigProvider.SetpointRegName && y.TryGetRawDouble(out _)));
                    }).Where(x => x.Value != null).ToDictionary(x => (double)x.Key, x => x.Value!.ToRawDouble());
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
                    UpdateProgress(duration, progress, false);
                }
                catch (Exception ex)
                {
                    ErrorUnits.Add(new Tuple<string, Exception>(item, ex));
                }
            }
            ReplotPreview();
        }

        public void UpdateProgress(int duration, int progress, bool replot = true)
        {
            CurrentProgressX = progress * duration / 100.0;
            if (replot) ReplotPreview(false);
        }

        public void ReplotPreview(bool autoscale = true)
        {
            ReplotRequested?.Invoke(this, new ReplotRequestedEventArgs(autoscale));
        }

        public List<MyPlotData> Data { get; } = new();
        public List<Tuple<string, Exception>> ErrorUnits { get; } = new();
        public double CurrentProgressX { get; private set; }
    }
}
