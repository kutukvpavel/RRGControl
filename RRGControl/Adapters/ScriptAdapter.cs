using Avalonia.Threading;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
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

    public class ScriptAdapter : IAdapter, INotifyPropertyChanged
    {
        public static event EventHandler<string>? LogEvent;

        public event EventHandler<Packet>? PacketReceived;
        public event EventHandler<Packet>? PacketSent;
        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? ExecutionFinished;

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
                mDuration = mScript?.GetDuration();
            }
        }
        public Dictionary<int, Packet>? Compiled { get => mCompiled; }
        public ScriptAdapterState State
        {
            get => mState;
            private set
            {
                mState = value;
                Dispatcher.UIThread.Post(() =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(State)));
                });
            }
        }
        public double? Progress => (double)mTicks / mDuration;

        public void Send(Packet p)
        {
            if (State == ScriptAdapterState.Running) PacketSent?.Invoke(this, p);
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
        public void Push()
        {
            mBackup = Script;
        }
        public void Pop()
        {
            Script = mBackup;
        }

        private readonly CancellationToken mToken;
        private readonly BlockingCollection<Packet> mQueue = new BlockingCollection<Packet>();
        private readonly Thread mQueueThread;
        private readonly Timer mTimer;
        private int mTicks = 0;
        private Script? mScript = null;
        private int? mDuration;
        private Dictionary<int, Packet>? mCompiled = null;
        private ScriptAdapterState mState = ScriptAdapterState.Stopped;
        private Script? mBackup;

        private void MTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (State == ScriptAdapterState.Cancelled) return;
            if (mTicks > (mDuration ?? 0))
            {
                Stop();
                Dispatcher.UIThread.Post(() => { ExecutionFinished?.Invoke(this, new EventArgs()); });
            }
            try
            {
                Dispatcher.UIThread.Post(() => { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Progress))); });
                if (mCompiled == null) return;
                if (mCompiled.TryGetValue(mTicks, out Packet? p))
                {
                    if (p != null)
                        mQueue.Add(p);
                }
            }
            catch (Exception ex)
            {
                LogEvent?.Invoke(this, ex.ToString());
            }
            finally
            {
                mTicks++;
            }
        }
        private void QueueThread(object? arg)
        {
            while (!mToken.IsCancellationRequested)
            {
                try
                {
                    var v = mQueue.Take(mToken);
                    PacketReceived?.Invoke(this, v);
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
