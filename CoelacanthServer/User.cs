using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
namespace CoelacanthServer
{
    class User
    {
        /* ---------------------------------------------
         * 1. 유저의 버퍼, 데이터, 소켓 정보를 저장할 클래스 변수 생성
         * 2. 정보 조회에 필요한 데이터 지정 (DB 추가되면, 따로 가져올 예정)
         * 3. 호스트와 게스트 구분 지정
        --------------------------------------------- */
        string nickname;
        int id;
        bool ready;
        string weapon;

        UserData data = new UserData(); // 소켓, 버퍼, 데이터 길이 등을 저장할 클래스 변수를 생성한다.
        User hostUser = null;        
        User[] guestUser = new User[3];
        List<User> Room = new List<User>();

        bool _ready = false;
        bool _start = false;
        int max = 0;

        UdpClient udp = new UdpClient();
        IPEndPoint multicastEP = new IPEndPoint(IPAddress.Parse("229.1.1.229"), 12900);
        public User(Socket socket)
        {
            
            data.workSocket = socket; // UserData의 workSocket를 서버에 연결된 소켓으로 설정한다.
            // 비동기 소켓 리시브를 실행한다. 클라이언트에서 데이터가 도착하면 ReadCallback이 자동으로 호출된다.
            data.workSocket.BeginReceive(data.buffer, data.recvlen, UserData.BufferSize, 0, new AsyncCallback(ReadCallback), data);
            WriteLine("CONNECT"); // 유저가 접속했을때 곧바로 클라이언트로 보내지는 패킷

        }
        /* ---------------------------------------------
         * 유저가 소켓에 접속하면서 대리자 비동기 작업을 시작한다.
         * 완료가 되었을 때, 콜백이 되는데 인자를 넘겨준다.
         * [참고]
         * http://blog.daum.net/creazier/15309346
         * https://msdn.microsoft.com/ko-kr/library/wyd0d1e5(v=vs.100).aspx (콜백 기술을 사용하여 비동기 웹 서비스 클라이언트 구현)
         * https://msdn.microsoft.com/ko-kr/library/system.asynccallback(v=vs.110).aspx (AsyncCallback 대리자)
        --------------------------------------------- */
        void ReadCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = data.workSocket;
                int bytesRead = handler.EndReceive(ar);

                if (bytesRead > 0)
                {
                    data.recvlen += bytesRead;
                    while (true)
                    {
                        short length;
                        Util.GetShort(data.buffer, 0, out length);

                        if (length > 0 && data.recvlen >= length)
                        {
                            ParsePacket(length);
                            data.recvlen -= length;

                            if (data.recvlen > 0)
                            {
                                Buffer.BlockCopy(data.buffer, length, data.buffer, 0, data.recvlen);
                            }
                            else
                            {
                                handler.BeginReceive(data.buffer, data.recvlen, UserData.BufferSize, 0, new AsyncCallback(ReadCallback), data);
                                break;
                            }
                        }
                        else
                        {
                            handler.BeginReceive(data.buffer, data.recvlen, UserData.BufferSize, 0, new AsyncCallback(ReadCallback), data);
                            break;
                        }
                    }
                }
                else
                {
                    handler.BeginReceive(data.buffer, data.recvlen, UserData.BufferSize, 0, new AsyncCallback(ReadCallback), data);
                }
            }

            catch (Exception)
            {
                Server.DeleteUser(this);
                //Console.WriteLine(nick + " 님이 종료하셨습니다.");
                //Disconnect();
            }
        }

        private static string PrivateCharKey(Random _random, int _length, string _pool)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < _length; i++)
                sb.Append(_pool[(int)(_random.NextDouble() * _pool.Length)]);
            return sb.ToString();
        }

        // 룸의 정보를 구분할 수 있는 PK 값 생성하는 함수
        private static string PrivateCharKey(Random _random, int _length)
        {
            string charPool = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";
            return PrivateCharKey(_random, _length, charPool);
        }

        private void ParsePacket(int length)
        {
            string time = DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss]");
            string msg = Encoding.UTF8.GetString(data.buffer, 2, length - 2);
            string[] text = msg.Split(':');            
            Console.WriteLine(time + " 받은 메시지 : " + msg);

            if (text[0].Equals("CONNECT"))
            {
            }
            else if (text[0].Equals("INITIALIZE"))
            {
                Console.WriteLine("[ :: 현재 접속중인 인원 : " + Server.UserList.Count + " :: ]");
                max = Convert.ToInt32(Server.UserList.Count);
                if (Server.UserList.Count % 4 == 1)
                {
                    for (int i = 0; i < Server.UserList.Count; i++)
                    {
                        if (Server.UserList[i] != this)
                            if (Server.UserList[i].hostUser != null)
                                if (Server.UserList[i].guestUser[i] == null)
                                    Room.Add(Server.UserList[i]);
                    }

                    if (Room.Count <= 0)
                    {
                        Console.WriteLine("룸 생성");
                        hostUser = this;
                        guestUser = null;
                        var RoomID = PrivateCharKey(Server.randomRoomNumber, 20);
                        // 확인용
                        Console.WriteLine(time + " 받은 메시지 : " + RoomID);
                        WriteLine(string.Format("CREATEROOM:{0}", RoomID));
                    }
                    Console.WriteLine(time + " 호스트유저 지정"); 
                    hostUser.nickname = text[1];
                    hostUser.id = int.Parse(text[2]);
                    hostUser.ready = false;
                    hostUser.weapon = null;
                }
                else
                {
                    //guestUser[Server.UserList.Count - 1].nickname = text[1];
                    //guestUser[Server.UserList.Count - 1].id = int.Parse(text[2]);
                    Console.WriteLine("게스트유저");
                }
                //nickname = text[1];
            }
            else if (text[0].Equals("READY")) // 클라이언트가 GUEST나 HOST 패킷을 받고 READY를 송신한 경우
            {
                _ready = true;

                if (hostUser != null && guestUser != null) // 호스트와 게스트가 모두 있는 경우면
                {
                    if (hostUser._ready && guestUser[0]._ready) // 임시 2인용 테스트
                    {
                        hostUser._start = true;
                        // 1. 캐릭터 4종류 중 한가지 할당
                        // 2. 맵 랜덤 할당 (호스트와 게스트 매칭)
                        // 3-1. 게스트의 캐릭터를 호스트와 다르게 설정
                        // 3-2. 게스트들의 캐릭터가 중복되지 않도록 설정
                        // 1번째 게스트 : userlist.hostUser.charracternumber != number ? number : number;
                        // 2~3번째 게스트 : get.hostuser.characternumber && get.guestuser.characeternumber != number;
                        
                        // 모든 유저에게 게임 시작 패킷 전송
                        //hostUser.WriteLine(string.Format("GAMESTART:{0}", characterNumber));
                        //guestUser[Convert.ToInt32(text[1])].WriteLine(string.Format("GAMESTART{0}", characterNumber));

                        Console.WriteLine("game start");
                    }
                }
            }
            else if(text[0].Equals("RECOVERY"))
            {
            }
            else if (text[0].Equals("POSITION"))
            {
                WriteLine(string.Format("POSITION:{0}:{1}:{2}:{3}", text[1], text[2], text[3], text[4]));
                // 좌표 받아오기
                int x = int.Parse(text[1]);
                int y = int.Parse(text[2]);
                int z = int.Parse(text[3]);

                hostUser.WriteLine(string.Format("POSITION:{0}:{1}:{2}", x, y, z));
                //dgram = Encoding.ASCII.GetBytes(text[1]);
                //byte[] bStrByte = Encoding.UTF8.GetBytes(str0);
                // string str1 = Encoding.Default.GetString(bStrByte); // byte -> string

                //.Send(dgram, dgram.Length, multicastEP);
                //byte[] _position = Encoding.UTF8.GetBytes(text[1]);

                //MulticastWrite(string.Format("POSITION:{0}", text[1]));
            }
            else if (text[0].Equals("DISCONNECT"))
            {
                if (nickname.Length > 0)
                {
                }
                data.workSocket.Shutdown(SocketShutdown.Both);
                data.workSocket.Close();
            }
        }

        public void MulticastWrite(string text) 
        {
            try
            {
                // 서버에 패킷 정보를 보낸다.
                if (data.workSocket != null && data.workSocket.Connected)
                {
                    byte[] buff = new byte[4096];
                    // 1. 문자열의 2바이트를 버퍼에 넣는다.
                    // 2. 문자열을 byte로 형변환해서 버퍼에 추가한다.
                    // 3. 문자열 크기와 2바이트 정보를 소켓으로 전송한다.
                    Buffer.BlockCopy(ShortToByte(Encoding.UTF8.GetBytes(text).Length + 2), 0, buff, 0, 2);
                    Buffer.BlockCopy(Encoding.UTF8.GetBytes(text), 0, buff, 2, Encoding.UTF8.GetBytes(text).Length);
                    // 멀티캐스팅 테스트
                    //byte[] _position = Encoding.UTF8.GetBytes(text);
                    udp.Send(buff, Encoding.UTF8.GetBytes(text).Length + 2, multicastEP);
                    Console.WriteLine(text);
                }
            }

            catch (System.Exception ex)
            {
                Disconnect();
                Console.WriteLine("WriteLine Error : " + ex.Message);
                // 1. 접속이 원활하지 않을 경우, 소켓을 닫는다.
                data.workSocket.Shutdown(SocketShutdown.Both);
                data.workSocket.Close();

                Server.DeleteUser(this);
            }
        }

        public void WriteLine(string text)
        {
            // 호스트 종료 시에도 서버가 돌아가야 함
            // 룸 3개 기준, 12인 플레이를 위해 호스트를 받을 서버임으로 꺼지지 않게 설정
            byte[] buff = new byte[4096];
            Buffer.BlockCopy(ShortToByte(Encoding.UTF8.GetBytes(text).Length + 2), 0, buff, 0, 2);
            Buffer.BlockCopy(Encoding.UTF8.GetBytes(text), 0, buff, 2, Encoding.UTF8.GetBytes(text).Length);
            data.workSocket.Send(buff, Encoding.UTF8.GetBytes(text).Length + 2, 0);
            /*
            for (int i = Server.UserList.Count - 1; i >= 0; i--)
            {
                // 본인의 클라이언트가 서버와 연결 되어있는지 확
                if (data.workSocket != null && data.workSocket.Connected)
                {   // 서버에 패킷 정보를 보낸다.
                    byte[] buff = new byte[4096];
                    // 1. 문자열의 2바이트를 버퍼에 넣는다.
                    // 2. 문자열을 byte로 형변환해서 버퍼에 추가한다.
                    // 3. 문자열 크기와 2바이트 정보를 소켓으로 전송한다.
                    Socket client = Server.UserList[i].data.workSocket;
                    if (client != null && client.Connected)
                    {
                        try
                        {
                            Buffer.BlockCopy(ShortToByte(Encoding.UTF8.GetBytes(text).Length + 2), 0, buff, 0, 2);
                            Buffer.BlockCopy(Encoding.UTF8.GetBytes(text), 0, buff, 2, Encoding.UTF8.GetBytes(text).Length);
                            data.workSocket.Send(buff, Encoding.UTF8.GetBytes(text).Length + 2, 0);
                        }

                        catch (System.Exception ex)
                        {
                            Disconnect();
                            Console.WriteLine("WriteLine Error : " + ex.Message);
                            // 1. 접속이 원활하지 않을 경우, 소켓을 닫는다.
                            data.workSocket.Shutdown(SocketShutdown.Both);
                            data.workSocket.Close();

                            Server.DeleteUser(this);
                        }
                    }
                }
            }
            */
        }

        public void DeadReckoning(ref int x, ref int y, ref int z)
        {   // 현재 위치 = 이전 위치 + (속도 * 시간) + (1 / 2 * 가속도 * 시간 ^ 2)
            // 가속도 : 0 가정
            // 속도 : 0.5 pixel/ms 가정
            // 최신화 : 20ms 가정
            // x = x + (0.5 * 1) + 1 / 2 * 0 * 1 ^ 2)
            int _x = x + 10;
            int _y = y;
            int _z = z + 10;
        }

        byte[] ShortToByte(int val)
        {
            byte[] temp = new byte[2];
            temp[1] = (byte)((val & 0x0000ff00) >> 8);
            temp[0] = (byte)((val & 0x000000ff));
            return temp;
        }

        int NickLengthFind(string text)
        {
            if (text.Length > 0)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public static void udpServer()
        {
            // UDP 서버 접속
            MulticastServer server = new MulticastServer(12900);
        }

        void Disconnect()
        {
            Console.WriteLine("누가 나감");
            // 호스트와 게스트 클라이언트 구분을 위해 설정
            // 1. 호스트가 남아있을 경우, 룸 유지집
            // 2. 호스트가 종료했을 경우, 룸 제거
            if (nickname.Length > 0)
            {
                if (hostUser != null || guestUser != null)
                {
                    // 내가 방에 입장했을 경우,
                    if (hostUser != this) 
                    {
                        hostUser.WriteLine("OUT");
                        hostUser.guestUser = null;
                        //hostUser.ready = false;
                        //hostUser.start = false;
                    }
                    // 내가 호스트일 경우,
                    else
                    {
                        if (guestUser != null)
                        {
                            // 1. 호스트 유저 나간걸 알림
                            // 2. 게스트 유저 중, 프록시서버 운용 가능한 유저로 이전
                            // 3. 게스트 유저 초기화 진행
                            //guestUser.WriteLine("OUT"); // 호스트 유저 종료
                            //guestUser.hostUser = this; // 호스트 이전
                            //guestUser.guestUser = null; // 게스트는 초기화한다.
                            //guestUser.ready = false;
                            //guestUser.start = false;
                        }
                    }
                }
            }
            else {
                return;
            }
        }
    }
}
