using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;

namespace RRGControl.ViewModels
{
    public class RegisterViewModel : ViewModelBase
    {
        public RegisterViewModel(MyModbus.ModbusRegister r)
        {
            mRegister = r;
            mRegister.PropertyChanged += MRegister_PropertyChanged;
        }

        private void MRegister_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

        }

        private MyModbus.ModbusRegister mRegister;

        public bool ShowCombobox { get => mRegister.Base.ValueType == MyModbus.RegisterValueType.Fixed; }
        public string[]? ComboboxItems { get => mRegister.Base.Values?.Keys.ToArray(); }
        public bool ShowTextbox { get => mRegister.Base.ValueType == MyModbus.RegisterValueType.Range; }
        public string TextboxMask 
        {
            get
            {
                var req = Enumerable.Repeat('0', mRegister.Base.Limits?.Min.ToString().Length ?? 0);
                var add = Enumerable.Repeat('#', mRegister.Base.Limits?.Max.ToString().Length ?? 0);
                return new string(add.Concat(req).ToArray());
            }
        }
        public string Name { get => mRegister.Base.Name; }
    }
}
