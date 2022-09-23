using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace RRGControl.ViewModels
{
    public class RegisterViewModel : ViewModelBase, INotifyPropertyChanged, INotifyDataErrorInfo
    {
        public new event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public RegisterViewModel(MyModbus.ModbusRegister r)
        {
            mRegister = r;
            TextboxMask = GetMask(mRegister);
            mRevLookup = mRegister.Base.Values?.ToDictionary(x => x.Value, x => x.Key);
            ComboboxItems = mRevLookup?.Values;
            mRegister.PropertyChanged += MRegister_PropertyChanged;
            base.PropertyChanged += PropertyChanged;
        }

        private void MRegister_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }

        private MyModbus.ModbusRegister mRegister;
        private Dictionary<ushort, string>? mRevLookup;
        private string? mWriteCmb;
        private string? mWriteTxt;

        public bool ShowCombobox { get => mRegister.Base.ValueType == MyModbus.RegisterValueType.Fixed; }
        public IEnumerable<string>? ComboboxItems { get; }
        public bool ShowTextbox { get => mRegister.Base.ValueType == MyModbus.RegisterValueType.Range; }
        public string TextboxMask { get; }
        public string Name { get => mRegister.Base.Name; }
        public string TextboxValue
        {
            get => ShowTextbox ? mRegister.Value.ToString() : "";
            set => mWriteTxt = value;
        }
        public string? ComboboxValue
        {
            get => ShowCombobox ? mRevLookup?[mRegister.Value] : null;
            set => mWriteCmb = value;
        }

        public bool HasErrors => Errors.Count > 0;
        public Dictionary<string, string> Errors { get; } = new Dictionary<string, string>();

        public async Task Read()
        {
            await mRegister.Read();
        }
        public async Task Write()
        {
            bool success = false;
            string pn = string.Empty;
            try
            {
                switch (mRegister.Base.ValueType)
                {
                    case MyModbus.RegisterValueType.Range:
                        pn = nameof(TextboxValue);
                        success = await mRegister.WriteStringRepresentation(mWriteTxt ?? mRegister.Base.DefaultValue.ToString());
                        break;
                    case MyModbus.RegisterValueType.Fixed:
                        pn = nameof(ComboboxValue);
                        success = await mRegister.WriteStringRepresentation(mWriteCmb ?? mRegister.Base.DefaultValue.ToString());
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(mRegister.Base.ValueType));
                }
            }
            catch (FormatException)
            {
                if (!Errors.ContainsKey(pn)) Errors.Add(pn, string.Empty);
                Errors[pn] = "Invalid format";
            }
            if (success)
            {
                if (Errors.ContainsKey(pn)) Errors.Remove(pn);
            }
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(
                    ShowCombobox ? nameof(ComboboxValue) : nameof(TextboxValue)));
        }

        private static string GetMask(MyModbus.ModbusRegister mRegister)
        {
            var req = Enumerable.Repeat('0', mRegister.Base.Limits?.Min.ToString().Length ?? 0);
            var add = Enumerable.Repeat('9', (mRegister.Base.Limits?.Max.ToString().Length - req.Count()) ?? 0);
            return new string(add.Concat(req).ToArray());
        }
        private static string[] UnknownError { get; } = { "Unknown error" };

        public System.Collections.IEnumerable GetErrors(string? propertyName)
        {
            if (!HasErrors) return Enumerable.Empty<string>();
            try
            {
                return new string[] { Errors[propertyName ?? string.Empty] };
            }
            catch (KeyNotFoundException)
            {
                return UnknownError;
            }
        }
    }
}
