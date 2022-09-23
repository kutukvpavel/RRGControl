using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRGControl.Adapters
{
    public interface IAdapter
    {
        public event EventHandler<Packet> PacketReceived;
        public Task Send(Packet p);
    }
}
