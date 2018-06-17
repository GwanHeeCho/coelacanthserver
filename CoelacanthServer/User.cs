using System;
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
        string nick;
        UserData data = new UserData(); // 소켓, 버퍼, 데이터 길이 등을 저장할 클래스 변수를 생성한다.
        User hostUser = null, guestUser = null;

        UdpClient udp = new UdpClient();
        IPEndPoint multicastEP = new IPEndPoint(IPAddress.Parse("229.1.1.229"), 2020);
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

                if (bytesRead > 0) {
                    data.recvlen += bytesRead;
                    while (true)
                    {
                        short length;
                        Util.GetShort(data.buffer, 0, out length);

                        if (length > 0 && data.recvlen >= length)
                        {
                            ParsePacket(length);
                            data.recvlen -= length;

                            if (data.recvlen > 0) {
                                Buffer.BlockCopy(data.buffer, length, data.buffer, 0, data.recvlen);
                            }
                            else {
                                handler.BeginReceive(data.buffer, data.recvlen, UserData.BufferSize, 0, new AsyncCallback(ReadCallback), data);
                                break;
                            }
                        }
                        else {
                            handler.BeginReceive(data.buffer, data.recvlen, UserData.BufferSize, 0, new AsyncCallback(ReadCallback), data);
                            break;
                        }
                    }
                }
                else {
                    handler.BeginReceive(data.buffer, data.recvlen, UserData.BufferSize, 0, new AsyncCallback(ReadCallback), data);
                }
            }

            catch (Exception)
            {
                Server.DeleteUser(this);
                Console.WriteLine("클라이언트 종료 신호");
                //Console.WriteLine(nick + " 님이 종료하셨습니다.");
                //Disconnect();
            }
        }

        private void ParsePacket(int length)
        {
            // 192.168.0.2:2020
            // Equals("CONNECT") : 클라이언트가 서버에 접속할 경우
            // Equals("DISCONNECT") : 클라이언트가 접속을 끊었을 경우
            // Equals("READY") : 모든 인원이 게임 준비를 했을 경우
            // Equals("START") : 호스트가 게임을 시작할 경우
            string msg = Encoding.UTF8.GetString(data.buffer, 2, length - 2);
            string[] text = msg.Split(':');
            Console.WriteLine(msg);
            if (text[0].Equals("CONNECT"))
            {
                nick = text[1];
                Console.WriteLine("[ :: " + Server.UserList.Count + "명 접속 :: ]");
                WriteLine(string.Format("INITIALIZE:{0}", Server.UserList.Count));



                Console.WriteLine("클라이언트 초기화 진행");
                /* [ 2018-04-08 ]
                 * 클라이언트에 인원 3명 충족되면 게임 시작을 위한 초기 데이터 전송 구현해놓을 것
                 * 1. 3명일 경우, 더 이상 난입 불가능
                 * 2. 게임 준비를 위한 대기시간과 초기 데이터를 전송
                */
            }
            else if (text[0].Equals("DISCONNECT"))
            {
                if (nick.Length > 0)
                {
                }
                data.workSocket.Shutdown(SocketShutdown.Both);
                data.workSocket.Close();
            }
            else if (text[0].Equals("INITIALIZE"))
            {
                Console.WriteLine("초기화 완료");
                WriteLine(string.Format("INITIALIZE:{0}", true));
            }
            else if (text[0].Equals("GAMESTART"))
            {                
                Console.WriteLine("데이터 설정 완료");
                WriteLine(string.Format("POSITION"));
            }
            else if (text[0].Equals("GOOD"))
            {
                Console.WriteLine("위치초기화");
                WriteLine(string.Format("GOOD"));
            }
            else if(text[0].Equals("NICKNAME"))
            {
                Console.WriteLine("유저닉네임");
                WriteLine(string.Format("NICKNAME:{0}", text[1]));
            }
            else if(text[0].Equals("NICKERROR"))
            {
            }
            else if(text[0].Equals("WEAPONCHANGE"))
            {
                //Console.WriteLine("무기교체:{0}:{1}", text[1], text[2]);
            }
            else if(text[0].Equals("FALL"))
            {
            }
            else if (text[0].Equals("POSITION"))
            {
                WriteLine(string.Format("POSITION:{0}:{1}:{2}:{3}", text[1], text[2], text[3], text[4]));
                //dgram = Encoding.ASCII.GetBytes(text[1]);
                //byte[] bStrByte = Encoding.UTF8.GetBytes(str0);
                // string str1 = Encoding.Default.GetString(bStrByte); // byte -> string

                //.Send(dgram, dgram.Length, multicastEP);
                //byte[] _position = Encoding.UTF8.GetBytes(text[1]);

                //MulticastWrite(string.Format("POSITION:{0}", text[1]));
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
            MulticastServer server = new MulticastServer(4040);
        }

        void Disconnect()
        {
            // 호스트와 게스트 클라이언트 구분을 위해 설정
            // 1. 호스트가 남아있을 경우, 룸 유지집
            // 2. 호스트가 종료했을 경우, 룸 제거
            if (nick.Length > 0)
            {
                if (hostUser != null || guestUser != null)
                {
                    // 내가 방에 입장했을 경우,
                    if (hostUser != this) // hostUser가 다른 사람이면 게스트로 입장한 상태
                    {
                        hostUser.WriteLine("OUT"); // 게스트가 나감
                        hostUser.guestUser = null; // 다른 유저가 접속할 수 있도록 초기화
                    }
                    // 내가 호스트일 경우,
                    else
                    {
                        if (guestUser != null)
                        {
                            // 게스트 퇴장을 서버에 알림
                            guestUser.WriteLine("OUT");
                            guestUser.hostUser = this;
                            guestUser.guestUser = null;
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
