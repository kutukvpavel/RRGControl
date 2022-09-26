using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NamedPipeWrapper;
using Newtonsoft.Json;


namespace RPCDebugHelper
{
    [Serializable]
    public class Packet
    {
        [JsonConstructor]
        public Packet()
        {

        }
        public Packet(string n, string r, string v)
        {
            UnitName = n;
            RegisterName = r;
            Value = v;
        }

        public string UnitName { get; set; }
        public string RegisterName { get; set; }
        public string Value { get; set; }

        public override string ToString()
        {
            return $"{UnitName}/{RegisterName} = {Value}";
        }
        public static Packet FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Packet>(json);
        }
        public string GetJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    static class Program
    {
        public static Packet[] Packets = new Packet[]
        {
            new Packet("Example1", "OperationMode", "Closed"),
            new Packet("Example1", "OperationMode", "Open"),
            new Packet("Example1", "OperationMode", "Regulate")
        };

        static void Main(string[] args)
        {
            Console.WriteLine("Waiting for messages...");

            var s = new UdpClient(44753);
            var t = Task.Run(() => { ReadSocket(s); });
            var p = new NamedPipeClient<string>("RRGControl_Pipe");
            p.ServerMessage += P_ServerMessage;
            p.Error += P_Error;
            p.Start();
            p.WaitForConnection();

            int i = 0;
            IPEndPoint ep = new IPEndPoint(IPAddress.Loopback, 44754);
            while (true)
            {
                Console.ReadKey();
                var b = Encoding.UTF8.GetBytes(Packets[i++].GetJson());
                i %= Packets.Length;
                s.Send(b, b.Length, ep);
            }
        }

        private static void P_Error(Exception exception)
        {
            Console.WriteLine(exception.Message);
        }

        private static void P_ServerMessage(NamedPipeConnection<string, string> connection, string message)
        {
            //Console.WriteLine(Packet.FromJson(message).ToString());
        }

        private static void ReadSocket(UdpClient s)
        {
            IPEndPoint e = new IPEndPoint(IPAddress.Any, 44753);
            while (true)
            {
                var b = s.Receive(ref e);
                Console.WriteLine(Encoding.UTF8.GetString(b));
            }
        }
    }
}
