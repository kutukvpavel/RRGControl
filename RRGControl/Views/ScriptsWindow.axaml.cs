using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using RRGControl.ViewModels;
using ScottPlot.Avalonia; 
using ScottPlot;         
using ReactiveUI; 

namespace RRGControl.Views
{
    public partial class ScriptsWindow : Window 
    {
        private AvaPlot? MyPlot;
        public ScriptsWindow()
        {
            InitializeComponent();

            // get plot control
            MyPlot = this.FindControl<AvaPlot>("Plot1");

            this.DataContextChanged += (s, e) =>
            {
                if (DataContext is CreateScriptViewModel vm)
                {
                    vm.PropertyChanged += (sender, args) =>
                    {
                        if (args.PropertyName == nameof(vm.RequestPlotUpdate))
                        {
                            UpdateMyPlot();
                        }
                    }; // update plot when requested
                    
                    UpdateMyPlot();
                }
            };
        }

        private void UpdateMyPlot()
        {
            var vm = DataContext as CreateScriptViewModel;
            if (vm == null || MyPlot == null) return;
            
            var plot = MyPlot.Plot;
            plot.Clear(); 

            var data = vm.PlotData; 
            
            if (data != null && data.Count > 0)
            {
                int colorIdx = 0;
                var colors = new Color[] { Colors.Blue, Colors.Red, Colors.Green, Colors.Orange, Colors.Purple };

                foreach (var line in data)
                {
                    double[] xs = line.Value.Xs;
                    double[] ys = line.Value.Ys;

                    if (xs.Length > 0)
                    {
                        var scatter = plot.Add.Scatter(xs, ys);
                        
                        scatter.ConnectStyle = ConnectStyle.StepHorizontal; 
                        scatter.LineWidth = 2;
                        scatter.LegendText = line.Key;

                        scatter.Color = colors[colorIdx % colors.Length];
                        colorIdx++;
                    }
                }
                
                plot.ShowLegend(); 
                plot.Axes.AutoScale();
                
                var limits = plot.Axes.GetLimits();
                if (limits.Top == limits.Bottom)
                {
                    plot.Axes.SetLimitsY(limits.Bottom - 1, limits.Bottom + 10);
                }
            }
            else
            {  // default view (empty)
                plot.Axes.SetLimits(0, 60, 0, 100);
            }

            plot.Axes.Bottom.Label.Text = "Time (sec)";
            plot.Axes.Left.Label.Text = "Flow (ml/min)";

            MyPlot.Refresh();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}