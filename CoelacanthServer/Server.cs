using System;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace CoelacanthServer
{
    // EASY_INFO : TCP socket server
    // UPDT_DATE : 2018-03-12 ~
    // UPDT_USNM : 조관희
    // REMK_TEXT : 소스 관리 시스템 링크
    //             클라 : https://gitlab.com/enycal/Coelacanth
    //             서버 : https://gitlab.com/enycal/CoelacanthServer
    class Server
    {
        // 1. 접속하고있는 모든 유저의 정보를 담아서 관리할 리스트 생성
        // 2. 스레드 충돌을 막기 위해 사용할 멀티스레스 시그널 생성
        // 3. 클라이언트들에게 패킷을 전송할 멀티캐스트 서버 생성
        public static List<User> UserList = new List<User>();
        public static Random randomRoomNumber = new Random();
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        public static string systemTime = DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss]");
        public Server(int port)
        {   // 127.0.0.1 -p 2020
            // 1. 유저 초기화
            // 2. 클라이언트 소켓 접속 대기
            UserList.Clear();
            StartListening(port);
        }

        public static void DeleteUser(User temp)
        {
            UserList.Remove(temp);
        }

        public static void StartListening(int port)
        {
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, port);
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // Poor TCP Implementation with UDP 경우의 수 배제
            // 흐름제어 및 패킷로스 설계가 어려움
            listener.NoDelay = false;
            listener.LingerState = new LingerOption(true, 0);
            listener.SendBufferSize = 81920;
            listener.ReceiveBufferSize = 81920;
            try
            {
                // 1. 설정한 서버 데이터 값을 바탕으로 바인딩 시도
                // 2. 큐 형식으로 되어 있는 구조 안에 동시처리 가능 한 접속 처리 크기를 지정 (동시접속처리와 별개)
                // 4인 게임이라서 4개까지 제한을 두고, 룸 제작되면 천천히 증가시킬 예정
                listener.Bind(localEndPoint);
                listener.Listen(4);
                Console.WriteLine(systemTime + " : " + "「Coelacanth Server Online」");

                while (true)
                {
                    allDone.Reset();
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                    allDone.WaitOne();
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            Console.WriteLine("\n아무 키나 입력해주십시오.");
            Console.Read();
        }

        static void AcceptCallback(IAsyncResult ar)
        {   // 클라이언트가 접속에 성공한 경우 호출되는 콜백 함수
            allDone.Set();
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            handler.NoDelay = false;
            handler.LingerState = new LingerOption(true, 0);
            handler.SendBufferSize = 81920;
            handler.ReceiveBufferSize = 81920;
            UserList.Add(new User(handler));
        }

        public static void Read(string value)
        {
            Console.WriteLine(value);
        }

        public static void Wrtie(string value)
        {
            Console.WriteLine(value);
        }
    }
}
