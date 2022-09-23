using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NModbus;

namespace RRGControl.MyModbus
{
    public class RRGUnit : INotifyPropertyChanged
    {
        public static event EventHandler<string>? LogEvent;
        public event PropertyChangedEventHandler? PropertyChanged;

        public RRGUnit(RRGModelConfig mcfg, RRGUnitConfig ucfg, Connection c, ushort addr)
        {
            Connection = c;
            ModbusConfig = mcfg;
            UnitConfig = ucfg;
            Address = addr;
            InitRegs();
        }

        private void InitRegs()
        {
            Registers = new Dictionary<string, ModbusRegister>(ModbusConfig.Registers.Count);
            foreach (var item in ModbusConfig.Registers)
            {
                var r = new ModbusRegister(item, Connection, Address);
                r.PropertyChanged += RegisterChanged;
                Registers.Add(item.Name, r);
            }
        }
        private void RegisterChanged(object? sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }

        public ushort Address { get; private set; }
        public Connection Connection { get; }
        public RRGModelConfig ModbusConfig { get; }
        public RRGUnitConfig UnitConfig { get; }

        public Dictionary<string, ModbusRegister> Registers { get; private set; }

        //Default properties
        public double MaxFlowrate => UnitConfig.ConversionFactor * Registers[ConfigProvider.SetpointRegName].Base.Limits.Max;
        public double Flowrate 
        { 
            get => Registers[ConfigProvider.MeasuredRegName].Value * UnitConfig.ConversionFactor;
        }
        public double Setpoint
        {
            get => UnitConfig.ConvertToUI(Registers[ConfigProvider.SetpointRegName].Value);
            set => Registers[ConfigProvider.SetpointRegName].Write(UnitConfig.ConvertToRegister(value));
        }
        public string Mode
        {
            get
            {
                try
                {
                    return Registers[ConfigProvider.OperationModeRegName].GetValueName();
                }
                catch (Exception ex)
                {
                    LogEvent?.Invoke(this, ex.ToString());
                    return "";
                }
            }
            set => Registers[ConfigProvider.OperationModeRegName].WriteByName(value);
        }
        private bool mPresent = false;
        public bool Present 
        { 
            get => mPresent;
            private set
            {
                bool changed = mPresent != value;
                mPresent = value;
                if (changed) PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Present)));
            }
        }

        public async Task<bool> ChangeAddress(ushort a)
        {
            var addrReg = Registers[ConfigProvider.AddressRegName];
            await addrReg.Write(a);
            //Check whether the new address is now available
            try
            {
                var v = Connection.Master.ReadHoldingRegisters((byte)a, addrReg.Base.Address, 1)[0];
                if (v != a)
                {
                    LogEvent?.Invoke(this, $"Address change for '{UnitConfig.Name}' failed.");
                    if (!(await Probe()))
                    {
                        LogEvent?.Invoke(this, "Address recovery failed.");
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogEvent?.Invoke(this, ex.ToString());
            }
            //Then update register DB
            foreach (var item in Registers)
            {
                item.Value.PropertyChanged -= RegisterChanged;
            }
            InitRegs();
            return true;
        }
        public bool ValidateSetpoint(double v)
        {
            try
            {
                return Registers[ConfigProvider.SetpointRegName].Base.ValidateValue(UnitConfig.ConvertToRegister(v));
            }
            catch (Exception ex)
            {
                LogEvent?.Invoke(this, $"Failed to validate value '{v}' of '{ConfigProvider.SetpointRegName}': {ex}");
                return false;
            }
        }
        public bool ValidateAddress(ushort v)
        {
            try
            {
                return Registers[ConfigProvider.AddressRegName].Base.ValidateValue(v);
            }
            catch (Exception ex)
            {
                LogEvent?.Invoke(this, $"Failed to validate value '{v}' of '{ConfigProvider.AddressRegName}': {ex}");
                return false;
            }
        }
        public async Task<bool> Probe()
        {
            try
            {
                var r = Registers[ConfigProvider.AddressRegName];
                if (await r.Read())
                {
                    Present = (Address == r.Value);
                }
                else
                {
                    Present = false;
                }
            }
            catch (Exception ex)
            {
                LogEvent?.Invoke(this, $"Probing of unit '{UnitConfig.Name}' failed: {ex}");
                Present = false;
            }
            return Present;
        }
        public async Task ReadAll()
        {
            if (!Present) return;
            foreach (var item in Registers)
            {
                await item.Value.Read();
            }
        }
    }
}
