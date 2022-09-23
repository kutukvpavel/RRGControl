using System;
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

        private string AboutAuthor => @$"ÐÐÃ series mass flow controller software

Kutukov Pavel, 2022-{DateTime.Today.Year}
kutukovps@my.msu.ru

Attributions:
Microsoft .NET Framework
Avalonia UI Framework
NuGet Packages: NamedPipeWrapper, LLibrary
Icons: freepik.com";
    }
}
