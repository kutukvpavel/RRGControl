using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Timer = System.Timers.Timer;

namespace RRGControl.Adapters
{
    public enum ScriptAdapterState
    {
        Stopped,
        Paused,
        Running,
        Cancelled
    }

    public class ScriptAdapter : IAdapter
    {
        public static event EventHandler<string>? LogEvent;

        public event EventHandler<Packet>? PacketReceived;

        public ScriptAdapter(CancellationToken t)
        {
            mToken = t;
            mToken.Register(Cancel);
            mQueueThread = new Thread(QueueThread);
            mQueueThread.Start();
            mTimer = new Timer(1000.0) { Enabled = false, AutoReset = true };
            mTimer.Elapsed += MTimer_Elapsed;
        }

        public Script? Script
        {
            get => mScript;
            set
            {
                mScript = value;
                mCompiled = mScript?.Compile();
            }
        }
        public Dictionary<int, Packet>? Compiled { get => mCompiled; }
        public ScriptAdapterState State { get; private set; }

        public void Send(Packet p)
        {
            //Do nothing
        }
        public void Start()
        {
            if (mTimer.Enabled) mTimer.Stop();
            mTimer.Start();
            State = ScriptAdapterState.Running;
        }
        public void Stop()
        {
            mTicks = 0;
            mTimer.Stop();
            State = ScriptAdapterState.Stopped;
        }
        public void Pause()
        {
            mTimer.Stop();
            State = ScriptAdapterState.Paused;
        }
        public void Rewind(int targetTicks)
        {
            mTicks = targetTicks;
        }

        private readonly CancellationToken mToken;
        private readonly BlockingCollection<Packet> mQueue = new BlockingCollection<Packet>();
        private readonly Thread mQueueThread;
        private readonly Timer mTimer;
        private int mTicks = 0;
        private Script? mScript = null;
        private Dictionary<int, Packet>? mCompiled = null;

        private void MTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (State == ScriptAdapterState.Cancelled) return;
            if (mCompiled == null) return;
            if (mCompiled.TryGetValue(mTicks, out Packet? p))
            {
                if (p != null) mQueue.Add(p);
            }
            mTicks++;
        }
        private void QueueThread(object? arg)
        {
            while (mToken.IsCancellationRequested)
            {
                try
                {
                    PacketReceived?.Invoke(this, mQueue.Take(mToken));
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    LogEvent?.Invoke(this, ex.ToString());
                }
            }
        }
        private void Cancel()
        {
            mTimer.Stop();
            mTimer.Dispose();
            State = ScriptAdapterState.Cancelled;
        }
    }
}
