using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace RRGControl.ViewModels
{
    public class RRGUnitViewModel : ViewModelBase
    {
        public RRGUnitViewModel(MyModbus.RRGUnit u)
        {
            mUnit = u;
            Registers = new RegisterViewModel[mUnit.Registers.Count];
            int i = 0;
            foreach (var item in mUnit.Registers)
            {
                Registers[i++] = new RegisterViewModel(item.Value);
            }
            Dashboard = new DashboardViewModel(u);
        }

        private MyModbus.RRGUnit mUnit;

        public Color TabColor { get => mUnit.Present ? Colors.LightGreen : Colors.Orange; }
        public string Name => mUnit.UnitConfig.Name;
        public RegisterViewModel[] Registers { get; }
        
        //Default dashboard
        public DashboardViewModel Dashboard { get; }
    }
}
