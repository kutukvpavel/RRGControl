using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRGControl.MyModbus
{
    public class ModbusRegister : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public static event EventHandler<string>? LogEvent;

        public ModbusRegister(ModbusRegisterBase b, Connection c, ushort unitAddr)
        {
            Connection = c;
            Base = b;
            UnitAddress = unitAddr;
        }

        public Connection Connection { get; }
        public ushort UnitAddress { get; }
        public ModbusRegisterBase Base { get; }
        public ushort Value { get => _value ?? Base.DefaultValue; }

        private ushort? _value = null;
        private void NonFixedTypeThrow()
        {
            if (Base.ValueType != RegisterValueType.Fixed)
                throw new InvalidOperationException($"No string representation for value '{Value}' of '{Base.Name}' exits.");
        }

        public string GetValueName()
        {
            NonFixedTypeThrow();
            return Base.Values.First(x => x.Value == Value).Key;
        }
        public bool Read()
        {
            try
            {
                _value = Connection.ReadRegister(this);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
                return true;
            }
            catch (Exception ex)
            {
                LogEvent?.Invoke(this, ex.ToString());
                return false;
            }
        }
        public void Write(ushort v)
        {
            if (Base.Type == RegisterType.ReadOnly) 
                throw new InvalidOperationException("Attempt to write into a read-only register.");
            if (Base.ValidateValue(v))
            {
                Connection.WriteRegister(this, v);
                Read();
            }
            else
            {
                LogEvent?.Invoke(this, $"Invalid value to be written to '{Base.Name}': {v}.");
            }
        }
        public void WriteByName(string n)
        {
            try
            {
                NonFixedTypeThrow();
                Write(Base.Values[n]);
            }
            catch (Exception ex)
            {
                LogEvent?.Invoke(this, ex.ToString());
            }
        }
    }
}
