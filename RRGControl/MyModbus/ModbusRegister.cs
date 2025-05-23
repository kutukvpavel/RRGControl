﻿using Avalonia.Threading;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace RRGControl.MyModbus
{
    public class ModbusRegister : INotifyPropertyChanged
    {
        public static event EventHandler<string>? LogEvent;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? RegisterChanged;
        public event EventHandler? ReadTimeout;

        public ModbusRegister(ModbusRegisterBase b, Connection c, ushort unitAddr)
        {
            Connection = c;
            Base = b;
            UnitAddress = unitAddr;
        }

        public Connection Connection { get; }
        public ushort UnitAddress { get; }
        public ModbusRegisterBase Base { get; }
        public short Value { get => _value ?? Base.DefaultValue; }

        private short? _value = null;
        private void NonFixedTypeThrow()
        {
            if (Base.ValueType != RegisterValueType.Fixed)
                throw new InvalidOperationException($"No string representation for value '{Value}' of '{Base.Name}' exits.");
        }
        private void RaiseReadTimeout()
        {
            Dispatcher.UIThread.Post(() => ReadTimeout?.Invoke(this, new EventArgs()));
        }

        public string GetValueStringRepresentation()
        {
            switch (Base.ValueType)
            {
                case RegisterValueType.Range:
                    return Value.ToString();
                case RegisterValueType.Fixed:
                    return GetValueName();
                default:
                    throw new ArgumentOutOfRangeException(nameof(Base.ValueType));
            }
        }
        public string GetValueName()
        {
            NonFixedTypeThrow();
            if (Base.Values == null) throw new InvalidOperationException("Allowed values array for a fixed type can not be null");
            try
            {
                var found = Base.Values.First(x => x.Value == Value);
                return found.Key;
            }
            catch (InvalidOperationException)
            {
                if (Base.LastValueSpans)
                {
                    var lastValue = Base.Values.Last();
                    if (Value >= lastValue.Value) return lastValue.Key;
                }
                throw;
            }
        }
        public async Task<bool> Read()
        {
            try
            {
                _value = await Connection.ReadRegister(this);
                Dispatcher.UIThread.Post(() =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
                });
                RegisterChanged?.Invoke(this, new EventArgs());
                return true;
            }
            catch (System.IO.IOException)
            {
                RaiseReadTimeout();
                return false;
            }
            catch (TimeoutException)
            {
                RaiseReadTimeout();
                return false;
            }
            catch (Exception ex)
            {
                LogEvent?.Invoke(this, ex.Message);
                return false;
            }
        }
        public async Task<bool> Write(short v)
        {
            if (Base.Type == RegisterType.ReadOnly) 
                throw new InvalidOperationException("Attempt to write into a read-only register.");
            if (Base.ValidateValue(v))
            {
                await Connection.WriteRegister(this, v);
                await Read();
                return true;
            }
            else
            {
                LogEvent?.Invoke(this, $"Invalid value to be written to '{Base.Name}': {v}.");
                return false;
            }
        }
        public async Task<bool> WriteByName(string n)
        {
            try
            {
                NonFixedTypeThrow();
                if (Base.Values == null) throw new InvalidOperationException("Allowed values array for a fixed type can not be null");
                await Write(Base.Values[n]);
                return true;
            }
            catch (Exception ex)
            {
                LogEvent?.Invoke(this, ex.ToString());
                return false;
            }
        }
        public async Task<bool> WriteStringRepresentation(string s)
        {
            switch (Base.ValueType)
            {
                case RegisterValueType.Range:
                    return await Write(short.Parse(s, System.Globalization.CultureInfo.InvariantCulture));
                case RegisterValueType.Fixed:
                    return await WriteByName(s);
                default:
                    throw new ArgumentOutOfRangeException(nameof(Base.ValueType));
            }
        }
    }
}
