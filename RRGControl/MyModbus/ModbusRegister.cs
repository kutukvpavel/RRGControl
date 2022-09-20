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
        public static event EventHandler<string> LogEvent;

        public event PropertyChangedEventHandler? PropertyChanged;

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

        public void Read()
        {
            try
            {
                _value = Connection.ReadRegister(this);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
            }
            catch (Exception ex)
            {
                LogEvent?.Invoke(this, ex.ToString());
            }
        }
        public void Write(ushort v)
        {
            if (Base.Type == RegisterValueType.ReadOnly) 
                throw new InvalidOperationException("Attempt to write into a read-only register.");
            if (Base.ValidateValue(v))
            {
                Connection.WriteRegister(this, v);
            }
            else
            {
                LogEvent?.Invoke(this, $"Invalid value to be written to {Base.Name}: {v}.");
            }
        }
    }
}
