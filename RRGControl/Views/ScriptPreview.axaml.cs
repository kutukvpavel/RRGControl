using Avalonia.Controls;
using System;

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
        }
    }
}
