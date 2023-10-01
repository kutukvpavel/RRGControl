using System;
using System.Linq;
using System.Reflection;
using Avalonia.Controls;

namespace RRGControl.Views
{
    public partial class AboutBox : Window
    {
        public AboutBox()
        {
            InitializeComponent();
            DataContext = this;
        }

        private string AboutAuthor => @$"РРГ series mass flow controller software
Version: {(Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false)
            .FirstOrDefault() as AssemblyInformationalVersionAttribute)?.InformationalVersion}

Kutukov Pavel, 2022-{DateTime.Today.Year}
kutukovps@my.msu.ru

    Attributions
Libraries:
    Microsoft .NET Framework
    Avalonia UI Framework
    Newtonsoft.JSON
    ScottPlot
    NModbus
    MessageBox.Avalonia
    NamedPipeWrapper
    LLibrary
    MoreLinq
Icons:
    freepik.com
    iconleak.com";
    }
}
