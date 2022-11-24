using Avalonia.Controls;
using MessageBox.Avalonia;
using AIcon = MessageBox.Avalonia.Enums.Icon;
using System;
using System.Text;

namespace RRGControl.Views
{
    public partial class ScriptPreview : Window
    {
        public ScriptPreview()
        {
            InitializeComponent();
        }

        private ViewModels.ScriptPreviewViewModel? ViewModel => DataContext as ViewModels.ScriptPreviewViewModel;

        private void OnOpened(object sender, EventArgs e)
        {
            if (ViewModel == null) return;
            foreach (var item in ViewModel.Data)
            {
                var p = Plot1.Plot.AddScatterStep(item.DataX, item.DataY, 
                    label: item.LegendEntry, lineWidth: 1.5f);
                p.LineStyle = item.Offline ? ScottPlot.LineStyle.Dash : ScottPlot.LineStyle.Solid;
            }
            Plot1.Plot.XAxis.Label("Time (seconds)");
            Plot1.Plot.YAxis.Label("Flow Rate");
            Plot1.Plot.Legend(location: ScottPlot.Alignment.UpperRight);
            Plot1.Refresh();
            if (ViewModel.ErrorUnits.Count > 0)
            {
                var sb = new StringBuilder($"Could not parse scripts for following units:{Environment.NewLine}");
                foreach (var item in ViewModel.ErrorUnits)
                {
                    sb.Append($"\t{item.Item1}: {item.Item2.Message};{Environment.NewLine}");
                }
                sb.Append("Scripts for the units listed above will not be displayed.");
                var mb = MessageBoxManager.GetMessageBoxStandardWindow(
                    "Script Parser Error",
                    sb.ToString(),
                    icon: AIcon.Warning);
                mb.ShowDialog(this);
            }
        }
    }
}
