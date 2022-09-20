using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NModbus;

namespace RRGControl.Models
{

    public class RRGUnit : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public RRGUnit(MyModbus.RRGModelConfig mcfg, MyModbus.RRGUnitConfig ucfg, MyModbus.Connection c, ushort addr)
        {
            Connection = c;
            ModbusConfig = mcfg;
            UnitConfig = ucfg;
            Address = addr;
            InitRegs();
        }

        private void InitRegs()
        {
            Registers = new Dictionary<string, MyModbus.ModbusRegister>(ModbusConfig.Count);
            foreach (var item in ModbusConfig)
            {
                var r = new MyModbus.ModbusRegister(item, Connection, Address);
                r.PropertyChanged += RegisterChanged;
                Registers.Add(item.Name, r);
            }
        }
        private void RegisterChanged(object? sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }

        public ushort Address { get; private set; }
        public MyModbus.Connection Connection { get; }
        public MyModbus.RRGModelConfig ModbusConfig { get; }
        public MyModbus.RRGUnitConfig UnitConfig { get; }

        public Dictionary<string, MyModbus.ModbusRegister> Registers { get; private set; }

        //Default properties
        public double Flowrate 
        { 
            get => Registers[MyModbus.RRGModelConfig.MeasuredRegName].Value * UnitConfig.ConversionFactor;
        }
        public double Setpoint
        {
            get => Registers[MyModbus.RRGModelConfig.MeasuredRegName].Value * UnitConfig.ConversionFactor;
            set => Registers[MyModbus.RRGModelConfig.MeasuredRegName]
                .Write((ushort)Math.Round(value / UnitConfig.ConversionFactor));
        }

        public void ChangeAddress(ushort a)
        {
            Registers[MyModbus.RRGModelConfig.AddressRegName].Write(a);
            Address = a;
            foreach (var item in Registers)
            {
                item.Value.PropertyChanged -= RegisterChanged;
            }
            InitRegs();
        }
    }
}
