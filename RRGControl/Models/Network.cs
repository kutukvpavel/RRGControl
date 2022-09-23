using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace RRGControl.Models
{
    public class Network
    {
        public static event EventHandler<string>? LogEvent;

        public Network(MyModbus.ModbusProvider p, List<MyModbus.RRGUnitMapping> m, IEnumerable<Adapters.IAdapter> a)
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

        private IEnumerable<Adapters.IAdapter> mAdapters;
        private Dictionary<string, MyModbus.RRGUnit> mUnitsByName;

        private void Adapter_PacketReceived(object? sender, Adapters.Packet e)
        {
            try
            {
                var u = mUnitsByName[e.UnitName];
                var r = u.Registers[e.RegisterName];
                r.WriteStringRepresentation(e.Value).Wait();
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
                    item.Send(Adapters.Packet.FromRegister(((MyModbus.RRGUnit)sender).UnitConfig.Name, e));
                }
                catch (Exception ex)
                {
                    LogEvent?.Invoke(this, ex.ToString());
                }
            }
        }
    }
}
