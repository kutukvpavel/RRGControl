using System;
using System.Collections.Concurrent;
using System.Threading;

namespace RRGControl.Adapters
{
    public abstract class AdapterBase : IAdapter
    {
        public event EventHandler<Packet>? PacketReceived;

        protected AdapterBase(CancellationToken t)
        {
            mToken = t;
            mSenderThread = new Thread(ProcessSendQueue);
            mReceiverThread = new Thread(ProcessReceiveQueue);
            mSenderThread.Start(mToken);
            mReceiverThread.Start(mToken);
        }

        private void ProcessSendQueue(object? arg)
        {
            var t = (CancellationToken)(arg ?? new CancellationToken());
            while (!t.IsCancellationRequested)
            {
                try
                {
                    SendItem(mSenderQueue.Take(t));
                }
                catch (OperationCanceledException)
                { }
                catch (Exception ex)
                {
                    Log(ex.ToString());
                }
            }
        }
        private void ProcessReceiveQueue(object? arg)
        {
            var t = (CancellationToken)(arg ?? new CancellationToken());
            while (!t.IsCancellationRequested)
            {
                try
                {
                    PacketReceived?.Invoke(this, ReceiveItem(t));
                }
                catch (OperationCanceledException)
                { }
                catch (Exception ex)
                {
                    Log(ex.ToString());
                }
            }
        }

        public virtual void Send(Packet p)
        {
            mSenderQueue.Add(p);
        }
        protected abstract void SendItem(Packet p);
        protected abstract Packet ReceiveItem(CancellationToken t);
        protected abstract void Log(string msg);


        protected CancellationToken mToken;
        private readonly BlockingCollection<Packet> mSenderQueue = new BlockingCollection<Packet>();
        private readonly Thread mSenderThread;
        private readonly Thread mReceiverThread;
    }
}
