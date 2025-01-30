using Avalonia.Media;
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
            if (r.Base.Type != MyModbus.RegisterType.ReadOnly && r.Base.ValueType != MyModbus.RegisterValueType.Fixed)
            {
                TextboxMask = GetMask(mRegister);
            }
            mRevLookup = mRegister.Base.Values?.ToDictionary(x => x.Value, x => x.Key);
            ComboboxItems = mRevLookup?.Values;
            mRegister.PropertyChanged += MRegister_PropertyChanged;
            base.PropertyChanged += PropertyChanged;
        }

        private void MRegister_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }

        private readonly MyModbus.ModbusRegister mRegister;
        private readonly Dictionary<short, string>? mRevLookup;
        private string? mWriteCmb;
        private string? mWriteTxt;
        private bool? mCompleted = null;

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
            get
            {
                if (!ShowCombobox) return null;
                try
                {
                    if (mRegister.Base.LastValueSpans)
                    {
                        return mRegister.GetValueStringRepresentation();
                    }
                    else return mRevLookup?[mRegister.Value];
                }
                catch (KeyNotFoundException)
                {
                    return null;
                }
            }
            set => mWriteCmb = value;
        }
        public bool IsReadOnly => mRegister.Base.Type == MyModbus.RegisterType.ReadOnly;
        public bool HasErrors => Errors.Count > 0;
        public Dictionary<string, string> Errors { get; } = new Dictionary<string, string>();
        public bool? MaskCompleted
        {
            get => mCompleted;
            set
            {
                mCompleted = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MaskCompleted)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MaskColor)));
            }
        }
        public IBrush MaskColor => MaskCompleted == null ? Brushes.DarkGray : 
            (MaskCompleted ?? false ? Brushes.LightGreen : Brushes.LightSalmon);
        public string LookupFailedValue => mRegister.Value.ToString();
        public IBrush ComboboxBorderColor => mRevLookup?.ContainsKey(mRegister.Value) ?? false ?
            Brushes.DarkGray : Brushes.LightCoral;

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
            catch (OverflowException)
            {
                if (!Errors.ContainsKey(pn)) Errors.Add(pn, string.Empty);
                Errors[pn] = "Out of range";
            }
            catch (Exception)
            {
                if (!Errors.ContainsKey(pn)) Errors.Add(pn, string.Empty);
                Errors[pn] = "Unknown error";
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
            return new string(req.Concat(add).ToArray());
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
