using Avalonia.Controls;
using MsBox.Avalonia;
using AIcon = MsBox.Avalonia.Enums.Icon;
using System;
using System.Text;
using ScottPlot;
using Avalonia;

namespace RRGControl.Views
{
    public partial class ScriptPreview : UserControl
    {
        public ScriptPreview()
        {
            InitializeComponent();
            DefaultAxesColor = Plot1.Plot.Axes.Bottom.FrameLineStyle.Color;
            DefaultDataBkgColor = Plot1.Plot.DataBackground.Color;
            DefaultFigureBkgColor = Plot1.Plot.FigureBackground.Color;
            DefaultGridColor = Plot1.Plot.Grid.MajorLineColor;
            DefaultLegendBackgroundColor = Plot1.Plot.Legend.BackgroundColor;
            DefaultLegendFontColor = Plot1.Plot.Legend.FontColor;
            DefaultLegendOutlineColor = Plot1.Plot.Legend.OutlineColor;
            
            DataContextChanged += (o, e) =>
            {
                if (ViewModel == null) return;
                ViewModel.ReplotRequested += (o, e) =>
                {
                    Plot(e.AutoScale);
                };
                Plot();
            };
            //Loaded += (o, e) => Plot();
            if (Application.Current != null) Application.Current.ActualThemeVariantChanged += ActualThemeVariant_Changed;
        }

        protected Color DefaultFigureBkgColor;
        protected Color DefaultDataBkgColor;
        protected Color DefaultAxesColor;
        protected Color DefaultGridColor;
        protected Color DefaultLegendBackgroundColor;
        protected Color? DefaultLegendFontColor;
        protected Color DefaultLegendOutlineColor;
        private ViewModels.ScriptPreviewViewModel? ViewModel => DataContext as ViewModels.ScriptPreviewViewModel;

        private async void Plot(bool autoscale = true)
        {
            if (ViewModel == null) return;
            Plot1.Plot.Clear();
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
            if (autoscale) Plot1.Plot.Axes.AutoScale();
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

        private void ActualThemeVariant_Changed(object? sender, EventArgs e)
        {
            if (App.IsDarkThemed)
            {
                // change figure colors
                Plot1.Plot.FigureBackground.Color = Color.FromHex("#181818");
                Plot1.Plot.DataBackground.Color = Color.FromHex("#1f1f1f");
                // change axis and grid colors
                Plot1.Plot.Axes.Color(Color.FromHex("#d7d7d7"));
                Plot1.Plot.Grid.MajorLineColor = Color.FromHex("#404040");
                // change legend colors
                Plot1.Plot.Legend.BackgroundColor = Color.FromHex("#404040");
                Plot1.Plot.Legend.FontColor = Color.FromHex("#d7d7d7");
                Plot1.Plot.Legend.OutlineColor = Color.FromHex("#d7d7d7");   
            }
            else
            {
                // change figure colors
                Plot1.Plot.FigureBackground.Color = DefaultFigureBkgColor;
                Plot1.Plot.DataBackground.Color = DefaultDataBkgColor;
                // change axis and grid colors
                Plot1.Plot.Axes.Color(DefaultAxesColor);
                Plot1.Plot.Grid.MajorLineColor = DefaultGridColor;
                // change legend colors
                Plot1.Plot.Legend.BackgroundColor = DefaultLegendBackgroundColor;
                Plot1.Plot.Legend.FontColor = DefaultLegendFontColor;
                Plot1.Plot.Legend.OutlineColor = DefaultLegendOutlineColor;
            }
            Plot1.Refresh();
        }
    }
}
