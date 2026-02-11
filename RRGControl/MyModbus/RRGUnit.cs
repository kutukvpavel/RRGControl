using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace RRGControl.MyModbus
{
    public class RRGUnit : INotifyPropertyChanged
    {
        public static event EventHandler<string>? LogEvent;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<ModbusRegister>? RegisterChanged;

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
                r.PropertyChanged += OnPropertyChanged;
                r.RegisterChanged += OnRegisterChanged;
                r.ReadTimeout += OnReadTimeout;
                Registers.Add(item.Name, r);
            }
        }

        private void OnReadTimeout(object? sender, EventArgs e)
        {
            if (sender == null) return;
            Present = false;
        }
        private void OnRegisterChanged(object? sender, EventArgs e)
        {
            if (sender == null) return;
            RegisterChanged?.Invoke(this, (ModbusRegister)sender);
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            string? pn = (sender as ModbusRegister)?.Base.Name switch
            { 
                ConfigProvider.AddressRegName => nameof(Address),
                ConfigProvider.MeasuredRegName => nameof(Measured),
                ConfigProvider.OperationModeRegName => nameof(Mode),
                ConfigProvider.SetpointRegName => nameof(Setpoint),
                _ => null
            };
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(pn));
        }

        public ushort Address { get; private set; }
        public Connection Connection { get; }
        public RRGModelConfig ModbusConfig { get; }
        public RRGUnitConfig UnitConfig { get; }

        public Dictionary<string, ModbusRegister> Registers { get; private set; } = new Dictionary<string, ModbusRegister>(0);

        //Default properties
        public double MaxFlowrate => UnitConfig.ConvertToUI(Registers[ConfigProvider.SetpointRegName].Base.Limits.Max);
        public double Measured 
        { 
            get => UnitConfig.ConvertToUI(Registers[ConfigProvider.MeasuredRegName].Value);
        }
        public double Setpoint
        {
            get => UnitConfig.ConvertToUI(Registers[ConfigProvider.SetpointRegName].Value);
#pragma warning disable CS4014
            set 
            {
                Registers[ConfigProvider.SetpointRegName].Write(UnitConfig.ConvertToRegister(value));
                if (!UnitConfig.AutoOpenClose) return;
                if ((value > 0) && (Mode == ConfigProvider.ClosedModeName))
                {
                    Mode = ConfigProvider.RegulateModeName;
                }
                else if ((value <= 0) && (Mode != ConfigProvider.ClosedModeName))
                {
                    Mode = ConfigProvider.ClosedModeName;
                }
            }
#pragma warning restore
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
#pragma warning disable CS4014
            set => Registers[ConfigProvider.OperationModeRegName].WriteByName(value);
#pragma warning restore
        }
        private bool mPresent = false;
        public bool Present 
        { 
            get => mPresent;
            private set
            {
                bool changed = mPresent != value;
                mPresent = value;
                if (changed)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Present)));
                    LogEvent?.Invoke(this, $"Unit '{UnitConfig.Name}' went {(mPresent ? "ONline" : "OFFline")}.");
                }
            }
        }

        public async Task<bool> ChangeAddress(ushort a)
        {
            var addrReg = Registers[ConfigProvider.AddressRegName];
            await addrReg.Write((short)a);
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
                item.Value.PropertyChanged -= OnPropertyChanged;
                item.Value.RegisterChanged -= OnRegisterChanged;
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
                return Registers[ConfigProvider.AddressRegName].Base.ValidateValue((short)v);
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
