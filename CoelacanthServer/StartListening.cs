using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace CoelacanthServer
{
    class StartListening
    {
        public static List<User> UserList = new List<User>();
        public static ManualResetEvent thread = new ManualResetEvent(false);

        public StartListening(int unicast, int multicast)
        {
            UserList.Clear();
            Console.WriteLine("2. 클라이언트 유니캐스트 접속 : unicastConnected 호출");
            unicastConnected(unicast, multicast);
        }

        // 유니캐스트 통신 : 서버 데이터 송신을 위한 용도
        public static void unicastConnected(int unicast, int multicast)
        {
            Console.WriteLine("3. 클라이언트 접속 초기화 작업 시작");
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, unicast);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            socket.LingerState = new LingerOption(true, 0);
            socket.SendBufferSize = 81920;
            socket.ReceiveBufferSize = 81920;
            try
            {
                Console.WriteLine("4. 클라이언트 유니캐스트 바인딩 시도");
                socket.Bind(endPoint);
                while (true)
                {
                    thread.Reset();
                    Console.WriteLine("5. 클라이언트 멀티캐스트 접속 : multicastConnected 호출");
                    multicastConnected(multicast, socket);
                    thread.WaitOne();
                }
            }
            catch (Exception ex)
            {
                Error error = new Error(ex.ToString());
            }
        }

        // 멀티캐스트 통신 : 서버 데이터 수신을 위한 용도
        public static void multicastConnected(int multicast, Socket socket) 
        {
            IPAddress multicastIP = IPAddress.Parse("229.1.1.229");
            UdpClient udpMulticast = new UdpClient();
            udpMulticast.JoinMulticastGroup(multicastIP);
            IPEndPoint remote = new IPEndPoint(multicastIP, multicast);
            Console.WriteLine("6. 클라이언트 멀티캐스트 초기화");
            ReceivePacket receive = new ReceivePacket(socket, udpMulticast, remote);
        }

        static void AcceptCallback(IAsyncResult arr)
        {
            Console.WriteLine("10. 비동기 접속 : 유저 리스트 추가");
            Socket listen = (Socket)arr.AsyncState;
            Socket handler = listen.EndAccept(arr);
            handler.LingerState = new LingerOption(true, 0);
            handler.SendBufferSize = 81920;
            handler.ReceiveBufferSize = 81920;
            UserList.Add(new User(handler));
        }
    }
}
