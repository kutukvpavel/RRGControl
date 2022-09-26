using NamedPipeWrapper;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace RRGControl.Adapters
{
    public class NamedPipeAdapter : AdapterBase, IAdapter, IDisposable
    {
        public static event EventHandler<string>? LogEvent;

        public NamedPipeAdapter(string pipeName, CancellationToken t) : base(t)
        {
            mServer = new NamedPipeServer<string>(pipeName);
            mServer.ClientConnected += MServer_ClientConnected;
            mServer.ClientMessage += MServer_ClientMessage;
            mServer.ClientDisconnected += MServer_ClientDisconnected;
            mServer.Error += MServer_Error;
            mServer.Start();
        }

        public bool IsConnected { get => mClients > 0; }

        public override void Send(Packet p)
        {
            if (!IsConnected) return;
            base.Send(p);
        }

        private void MServer_Error(Exception exception)
        {
            LogEvent?.Invoke(this, exception.ToString());
        }
        private void MServer_ClientDisconnected(NamedPipeConnection<string, string> connection)
        {
            mClients--;
        }
        private void MServer_ClientMessage(NamedPipeConnection<string, string> connection, string message)
        {
            var p = Packet.FromJson(message);
            if (p == null) LogEvent?.Invoke(this, $"Input packet deserialized as null, contents: {message}");
            else mReceiverQueue.Add(p);
        }
        private void MServer_ClientConnected(NamedPipeConnection<string, string> connection)
        {
            mClients++;
        }
        protected override void SendItem(Packet p)
        {
            mServer.PushMessage(p.GetJson());
        }
        protected override Packet ReceiveItem(CancellationToken t)
        {
            return mReceiverQueue.Take(t);
        }
        protected override void Log(string msg)
        {
            LogEvent?.Invoke(this, msg);
        }

        public void Dispose()
        {
            try
            {
                mServer.Stop();
            }
            catch (ObjectDisposedException)
            {

            }
        }

        private readonly NamedPipeServer<string> mServer;
        private int mClients = 0;
        private readonly BlockingCollection<Packet> mReceiverQueue = new BlockingCollection<Packet>();
    }
}
