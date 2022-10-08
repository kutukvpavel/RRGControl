using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Avalonia.Threading;

namespace RRGControl.Models
{
    public class Network
    {
        public static event EventHandler<string>? LogEvent;

        public Network(MyModbus.ModbusProvider p, List<MyModbus.RRGUnitMapping> m, IEnumerable<Adapters.IAdapter> a,
            int rescan)
        {
            Provider = p;
            Connections = new List<MyModbus.Connection>(m.Count);
            foreach (var item in m)
            {
                try
                {
                    Connections.Add(new MyModbus.Connection(p, item));
                }
                catch (Exception ex)
                {
                    LogEvent?.Invoke(this, $"Can't create connection: {ex}");
                }
            }
            var units = new List<MyModbus.RRGUnit>(Connections.Sum(x => x.Units.Count));
            foreach (var item in Connections)
            {
                units.AddRange(item.Units.Values);
            }
            foreach (var item in units)
            {
                item.RegisterChanged += Item_RegisterChanged;
            }
            mUnitsByName = units.ToDictionary(x => x.UnitConfig.Name, x => x);
            mAdapters = a;
            foreach (var item in mAdapters)
            {
                item.PacketReceived += Adapter_PacketReceived;
            }
            if (rescan > 0)
            {
                mRescanTimer = new DispatcherTimer(new TimeSpan(0, 0, rescan), DispatcherPriority.Background, Rescan_Callback);
                mRescanTimer.Start();
            }
        }

        public List<MyModbus.Connection> Connections { get; }
        public MyModbus.ModbusProvider Provider { get; }

        public async Task Scan()
        {
            foreach (var item in Connections)
            {
                await item.Scan();
            }
        }
        public async Task ReadAll()
        {
            foreach (var item in Connections)
            {
                await item.ReadAll();
            }
        }
        public MyModbus.RRGUnit? FindUnitByName(string name)
        {
            if (mUnitsByName.TryGetValue(name, out MyModbus.RRGUnit? unit)) return unit;
            return null;
        }

        private readonly IEnumerable<Adapters.IAdapter> mAdapters;
        private readonly Dictionary<string, MyModbus.RRGUnit> mUnitsByName;
        private readonly DispatcherTimer? mRescanTimer;

        private async void Rescan_Callback(object? sender, EventArgs e)
        {
            await Scan();
        }
        private void Adapter_PacketReceived(object? sender, Adapters.Packet e)
        {
            try
            {
                var u = mUnitsByName[e.UnitName];
                if (!u.Present) return;
                var r = u.Registers[e.RegisterName];
                r.WriteStringRepresentation(e.GetConvertedValue(u)).Wait();
            }
            catch (Exception ex)
            {
                LogEvent?.Invoke(this, ex.ToString());
            }
        }
        private void Item_RegisterChanged(object? sender, MyModbus.ModbusRegister e)
        {
            if (sender == null) return;
            foreach (var item in mAdapters)
            {
                try
                {
                    item.Send(Adapters.Packet.FromRegister((MyModbus.RRGUnit)sender, e));
                }
                catch (Exception ex)
                {
                    LogEvent?.Invoke(this, $"For register '{e.Base.Name}': {ex}");
                }
            }
        }
    }
}
