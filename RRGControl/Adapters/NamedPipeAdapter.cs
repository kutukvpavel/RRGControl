using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NamedPipeWrapper;

namespace RRGControl.Adapters
{
    public class NamedPipeAdapter : IAdapter
    {
        public static event EventHandler<string>? LogEvent;

        public event EventHandler<Packet>? PacketReceived;

        public NamedPipeAdapter(string pipeName)
        {
            mServer = new NamedPipeServer<Packet>(pipeName);
            mServer.ClientConnected += MServer_ClientConnected;
            mServer.ClientMessage += MServer_ClientMessage;
            mServer.ClientDisconnected += MServer_ClientDisconnected;
        }

        public bool IsConnected { get => mClients > 0; }

        public async Task Send(Packet p)
        {
            if (!IsConnected) return;
            await Task.Run(() => { mServer.PushMessage(p); });
        }

        private void MServer_ClientDisconnected(NamedPipeConnection<Packet, Packet> connection)
        {
            mClients--;
        }
        private void MServer_ClientMessage(NamedPipeConnection<Packet, Packet> connection, Packet message)
        {
            PacketReceived?.Invoke(this, message);
        }
        private void MServer_ClientConnected(NamedPipeConnection<Packet, Packet> connection)
        {
            mClients++;
        }

        private readonly NamedPipeServer<Packet> mServer;
        private int mClients = 0;
    }
}
