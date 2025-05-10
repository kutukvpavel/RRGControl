using Avalonia.Controls;
using MsBox.Avalonia;
using AIcon = MsBox.Avalonia.Enums.Icon;
using System;
using System.Text;
using ScottPlot;

namespace RRGControl.Views
{
    public partial class ScriptPreview : Window
    {
        //AvaPlot Plot1;
        public ScriptPreview()
        {
            InitializeComponent();
        }

        private ViewModels.ScriptPreviewViewModel? ViewModel => DataContext as ViewModels.ScriptPreviewViewModel;

        private async void OnOpened(object sender, EventArgs e)
        {
            if (ViewModel == null) return;
            foreach (var item in ViewModel.Data)
            {
                var p = Plot1.Plot.Add.Scatter(item.DataX, item.DataY);
                p.LegendText = item.LegendEntry;
                p.LineWidth = 1.5f;
                p.LineStyle.Pattern = item.Offline ? LinePattern.Dashed : LinePattern.Solid;
                p.ConnectStyle = ConnectStyle.StepHorizontal;
            }
            Plot1.Plot.Axes.Bottom.Label.Text = "Time (seconds)";
            Plot1.Plot.Axes.Left.Label.Text = "Flow Rate";
            Plot1.Plot.Legend.Alignment = Alignment.UpperRight;
            Plot1.Plot.Add.VerticalLine(ViewModel.CurrentProgressX,
                pattern: ViewModel.CurrentProgressX > 0 ? LinePattern.Solid : LinePattern.Dotted);
            Plot1.Refresh();
            if (ViewModel.ErrorUnits.Count > 0)
            {
                var sb = new StringBuilder($"Could not parse scripts for following units:{Environment.NewLine}");
                foreach (var item in ViewModel.ErrorUnits)
                {
                    sb.Append($"\t{item.Item1}: {item.Item2.Message};{Environment.NewLine}");
                }
                sb.Append("Scripts for the units listed above will not be displayed.");
                var mb = MessageBoxManager.GetMessageBoxStandard(
                    "Script Parser Error",
                    sb.ToString(),
                    icon: AIcon.Warning);
                await mb.ShowAsync();
            }
        }
    }
}
