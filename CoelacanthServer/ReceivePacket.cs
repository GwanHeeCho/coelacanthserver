using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace CoelacanthServer
{
    class ReceivePacket
    {
        private int _packet;
        public int Packet
        {
            get { return _packet; }
            set { _packet = value; }
        }

        private byte[] _bytes = new byte[25];
        public byte[] Bytes
        {
            get { return _bytes; }
            set { _bytes = value; }
        }

        private Socket _socket;
        public Socket userSocket
        {
            get { return _socket; }
            set { _socket = value; }
        }

        private UdpClient _udp;
        public UdpClient UDP
        {
            get { return _udp; }
            set { _udp = value; }
        }

        private IPEndPoint _remote;
        public IPEndPoint Remote
        {
            get { return _remote; }
            set { _remote = value; }
        }
        
        public ReceivePacket(Socket socket, UdpClient udp, IPEndPoint remote)
        {
            userSocket = socket;
            UDP = udp;
            Remote = remote;
            Console.WriteLine("7. 패킷 수신 초기화 : " + " Socket : " + userSocket.ToString() + " UdpClient : " + UDP.ToString() + " IPEndPoint : " + Remote);

            while (true)
            {
                Packet = userSocket.Receive(Bytes, 0, Bytes.Length, SocketFlags.None);

                byte[] id = Bytes.Take(1).ToArray();
                byte[] xs = Bytes.Skip(1).Take(8).ToArray();
                byte[] ys = Bytes.Skip(9).Take(16).ToArray();
                byte[] zs = Bytes.Skip(17).Take(24).ToArray();

                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(xs);
                    Array.Reverse(ys);
                    Array.Reverse(zs);
                }

                int x = BitConverter.ToInt32(xs, 0);
                int y = BitConverter.ToInt32(ys, 0);
                int z = BitConverter.ToInt32(zs, 0);

                Console.WriteLine("id : " + id[0] + " x : " + x + " y : " + y + " z : " + z);
                UDP.Send(Bytes, Bytes.Length, remote);
            }
        }
    }
}
