using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace RRGControl.Adapters
{
    public class SocketAdapter : AdapterBase, IAdapter, IDisposable
    {
        public static event EventHandler<string>? LogEvent;

        public SocketAdapter(int portIn, int portOut, CancellationToken t) : base(t)
        {
            /*mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            mSocket.Bind(new IPEndPoint(IPAddress.Loopback, portOut));
            Log($"Outbound socket server started at {mSocket.LocalEndPoint}");*/
            mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            if (portIn > 0) mSocket.Bind(new IPEndPoint(IPAddress.Any, portIn));
            Log($"Inbound socket server started at {mSocket.LocalEndPoint}");
            mSocket.ReceiveTimeout = 1000;
            mSocket.SendTimeout = 500;
            if (portOut > 0) mEP = new IPEndPoint(IPAddress.Loopback, portOut);
        }

        public override void Send(Packet p)
        {
            //if (!mSocket.IsBound) return;
            base.Send(p);
        }
        public void Dispose()
        {
            try
            {
                mSocket.Close();
                mSocket.Dispose();
                mSocket.Close();
                mSocket.Dispose();
            }
            catch (ObjectDisposedException)
            {

            }
        }

        protected override void SendItem(Packet p)
        {
            if (mEP == null) return;
            byte[] b = Encoding.UTF8.GetBytes(p.GetJson());
            mSocket.SendTo(b, mEP);
        }
        protected override Packet ReceiveItem(CancellationToken t)
        {
            Packet? p = null;
            while (p == null && !t.IsCancellationRequested)
            {
                if (!mSocket.IsBound)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                try
                {
                    int n = mSocket.Receive(mBuffer);
                    p = Packet.FromJson(Encoding.UTF8.GetString(mBuffer, 0, n));
                    if (p == null)
                    {
                        Log("Packet deserialized as null. Invalid JSON?");
                    }
                }
                catch (SocketException ex)
                {
                    switch (ex.SocketErrorCode)
                    {
                        case SocketError.ConnectionReset:
                            break;
                        case SocketError.TimedOut:
                            break;
                        case SocketError.NoData:
                            break;
                        default:
                            throw;
                    }
                }
            }
            if (p == null) throw new OperationCanceledException();
            return p;
        }
        protected override void Log(string msg)
        {
            LogEvent?.Invoke(this, msg);
        }

        private readonly Socket mSocket;
        private readonly byte[] mBuffer = new byte[4096];
        private readonly IPEndPoint? mEP;
    }
}
