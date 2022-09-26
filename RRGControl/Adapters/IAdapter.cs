using System;

namespace RRGControl.Adapters
{
    public interface IAdapter
    {
        public event EventHandler<Packet>? PacketReceived;
        public void Send(Packet p);
    }
}
