using System;
using System.Collections.Concurrent;
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

        public NamedPipeAdapter(string pipeName, CancellationToken t)
        {
            mServer = new NamedPipeServer<Packet>(pipeName);
            mServer.ClientConnected += MServer_ClientConnected;
            mServer.ClientMessage += MServer_ClientMessage;
            mServer.ClientDisconnected += MServer_ClientDisconnected;
            mQueueThread = new Thread(ProcessQueue);
            mQueueThread.Start(t);
        }

        public bool IsConnected { get => mClients > 0; }

        public void Send(Packet p)
        {
            if (!IsConnected) return;
            mQueue.Add(p);
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
        private void ProcessQueue(object? arg)
        {
            var t = (CancellationToken)(arg ?? new CancellationToken());
            while (!t.IsCancellationRequested)
            {
                try
                {
                    mServer.PushMessage(mQueue.Take(t));
                }
                catch (OperationCanceledException)
                { }
                catch (Exception ex)
                {
                    LogEvent?.Invoke(this, ex.ToString());
                }
            }
        }

        private readonly NamedPipeServer<Packet> mServer;
        private int mClients = 0;
        private readonly BlockingCollection<Packet> mQueue = new BlockingCollection<Packet>();
        private readonly Thread mQueueThread;
    }
}
