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
        UserData data = new UserData(); // 소켓, 버퍼, 데이터 길이 등을 저장할 클래스 변수를 생성한다.
        UserPrivateData member = new UserPrivateData();
        User hostUser = null;        
        User[] guestUser = new User[3];
        User testUser = null;
        int index = 0;

        //int index = 0;

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
                Console.WriteLine("[ :: 현재 접속중인 인원 : " + Server.UserList.Count + " :: ]");
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

        private static int GuestUser(int count)
        {
            return count--;
        }

        private static User PlayerStat(User user, string nick, int id, float x, float z, float rotate, string room, bool ready)
        {
            user.member.nickname = nick;
            user.member.id = id;
            user.member.x = x;
            user.member.z = z;
            user.member.rotate = rotate;
            user.member.room = room;
            user.member.ready = ready;
            PlayerStat(user);
            return user;
        }

        private static void PlayerStat(User user)
        {
            Console.WriteLine("닉네임 : " + user.member.nickname);
            Console.WriteLine("아이디 : " + user.member.id);
            Console.WriteLine("x좌표 : " + user.member.x);
            Console.WriteLine("z좌표 : " + user.member.z);
            Console.WriteLine("회전값 : " + user.member.rotate);
            Console.WriteLine("룸코드 : " + user.member.room);
            Console.WriteLine("준비상태 : " + user.member.ready);
        }


        private void ParsePacket(int length)
        {
            string msg = Encoding.UTF8.GetString(data.buffer, 2, length - 2);
            string[] text = msg.Split(':');
            Console.WriteLine(Server.systemTime + " 받음 = " + msg);

            if (text[0].Equals("CONNECT"))
            {
                Console.WriteLine("[ :: 현재 접속중인 인원 : " + Server.UserList.Count + " :: ]");

                // 룸 생성한 호스트 유저의 룸 번호 리스트
                List<User> Room = new List<User>();
                index = GuestUser(Server.UserList.Count);

                //  게스트 유저 접속 시, 개설 된 방이 있는지 확인
                for (int i = 0; i < Server.UserList.Count; i++)
                {
                    if (Server.UserList[i] != this) // 게스트 유저이고
                        if (Server.UserList[i].hostUser != null) // 호스트가 아닐 경우
                        {
                            // 게스트유저 검사
                            for (; i < Server.UserList.Count; i++)
                            {

                            }

                            if (Server.UserList[i].testUser == null) // 
                                Room.Add(Server.UserList[i]);
                        }
                }
                //  게스트 유저가 접속 가능 한 방 목록 조회
                if (Room.Count > 0)
                {
                    User host = Room[Server.Rand.Next(Room.Count)];
                    host.guestUser[index] = this;
                    hostUser = host;
                    guestUser[index] = this;
                    for (int i = 0; i < Server.UserList.Count; i++)
                    {
                        if (i == 0)
                            Console.WriteLine("host : " + hostUser.member.nickname);
                        else
                            Console.WriteLine("guest : " + guestUser[i].member.nickname);
                    }
                }

                // 호스트 유저 접속 시, 룸 생성 (4인 플레이 기준 동작)
                if (Server.UserList.Count % 4 == 1)
                {
                    // 개설 된 룸이 없는 경우
                    if (Room.Count == 0)
                    {
                        Console.WriteLine(Server.systemTime + " 호스트유저 지정 : 룸 생성");
                        Server.RoomPrivateKey = PrivateCharKey(Server.randomRoomNumber, 20);
                        hostUser = this;
                        guestUser = null;
                        PlayerStat(hostUser, text[1], int.Parse(text[2]), 0, 0, 0, Server.RoomPrivateKey, false);
                        WriteLine(string.Format("CREATEROOM:{0}:{1}:{2}", hostUser.member.nickname, hostUser.member.id, Server.RoomPrivateKey));
                    }
                }
                else if (Server.UserList.Count != 0)
                {
                    Console.WriteLine(Server.systemTime + " 게스트유저 지정 : 룸 참가");
                    guestUser[index].member.nickname = text[1];
                    Console.WriteLine("닉네임 : " + guestUser[index].member.nickname);
                }
                //else if (Server.UserList.Count <= 4)
                //{
                //    Console.WriteLine(Server.systemTime + " 게스트유저");
                //    testUser.member.nickname = text[1];
                //    Console.WriteLine(testUser.member.nickname);
                //    testUser.member.id = int.Parse(text[2]);
                //    Console.WriteLine(testUser.member.id);
                //    testUser.member.x = 0;
                //    Console.WriteLine(testUser.member.x);
                //    testUser.member.z = 0;
                //    Console.WriteLine(testUser.member.z);
                //    testUser.member.rotate = 0;
                //    Console.WriteLine(testUser.member.rotate);
                //    testUser.member.room = Server.RoomPrivateKey;
                //    Console.WriteLine(testUser.member.room);
                //    testUser.member.ready = false;
                //    Console.WriteLine(testUser.member.ready);
                //    WriteLine(string.Format("CREATEROOM:{0}:{1}:{2}", testUser.member.nickname, testUser.member.id, Server.RoomPrivateKey));
                //    Console.WriteLine(Server.systemTime + " 게스트유저 지정 : 룸 참가");
                //}
                else
                {
                    Console.WriteLine("비정상 접근");
                    Server.DeleteUser(this);
                    data.workSocket.Shutdown(SocketShutdown.Both);
                    data.workSocket.Close();
                }
            }
            else if (text[0].Equals("READY")) // 클라이언트가 GUEST나 HOST 패킷을 받고 READY를 송신한 경우
            {
                //bool _ready = true;

                //if (hostUser != null && guestUser != null) // 호스트와 게스트가 모두 있는 경우면
                //{
                //    if (hostUser._ready && guestUser[0]._ready) // 임시 2인용 테스트
                //    {
                //        hostUser._start = true;
                        // 1. 캐릭터 4종류 중 한가지 할당
                        // 2. 맵 랜덤 할당 (호스트와 게스트 매칭)
                        // 3-1. 게스트의 캐릭터를 호스트와 다르게 설정
                        // 3-2. 게스트들의 캐릭터가 중복되지 않도록 설정
                        // 1번째 게스트 : userlist.hostUser.charracternumber != number ? number : number;
                        // 2~3번째 게스트 : get.hostuser.characternumber && get.guestuser.characeternumber != number;
                        
                        // 모든 유저에게 게임 시작 패킷 전송
                        //hostUser.WriteLine(string.Format("GAMESTART:{0}", characterNumber));
                        //guestUser[Convert.ToInt32(text[1])].WriteLine(string.Format("GAMESTART{0}", characterNumber));

                //        Console.WriteLine("game start");
                //    }
                //}
            }
            else if (text[0].Equals("START"))
            {
                
            }
            else if (text[0].Equals("BTNSTART"))
            {
                /*
                Console.WriteLine("들어오긴하니?");
                int max = 4;
                int create = (max - Server.UserList.Count);
                Console.WriteLine(create);
                for (int i = 0; i < create; i++)
                {
                    Server.UserList.Add(new User(null));
                    WriteLine(string.Format("BTNSTART:{0}", i));
                }
                */
            }
            else if (text[0].Equals("MOVE"))
            {
                //if (hostUser.nickname != null)
                //{
                //    if (Server.UserList.Count % 4 == 1)
                //    {
                //        int from = int.Parse(text[1]);
                //        int to = int.Parse(text[2]);

                //        hostUser.WriteLine(string.Format("MOVE:{0}:{1}", from, to));
                //        for (int i = 0; i < Server.UserList.Count; i++)
                //            guestUser[i].WriteLine(string.Format("MOVE:{0}:{1}", from, to));

                //        Console.WriteLine(string.Format("{0} moved player {1} to {2}", nickname, from, to));
                //    }
                //    else
                //    {
                //        Console.WriteLine("예외처리 " + nickname);
                //    }
                //}
            }
            else if(text[0].Equals("ROTATE"))
            {
                //UserPrivateData.Rotate = float.Parse(text[3]);
                //WriteLine(string.Format("ROTATE:{0}", UserPrivateData.Rotate));
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
                //string str1 = Encoding.Default.GetString(bStrByte); // byte -> string

                //Send(dgram, dgram.Length, multicastEP);
                //byte[] _position = Encoding.UTF8.GetBytes(text[1]);

                //MulticastWrite(string.Format("POSITION:{0}", text[1]));
            }
            else if (text[0].Equals("DISCONNECT"))
            {
                if (text[1].Length > 0)
                {
                    if (hostUser.member.id == int.Parse(text[2]) && Server.RoomPrivateKey == text[3])
                    {
                    }
                    Server.DeleteUser(this);
                    data.workSocket.Shutdown(SocketShutdown.Both);
                    data.workSocket.Close();
                }
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
                //Disconnect();
                Console.WriteLine("WriteLine Error : " + ex.Message);
                // 1. 접속이 원활하지 않을 경우, 소켓을 닫는다.
                data.workSocket.Shutdown(SocketShutdown.Both);
                data.workSocket.Close();

                Server.DeleteUser(this);
                Console.WriteLine("[ :: 현재 접속중인 인원 : " + Server.UserList.Count + " :: ]");
            }
        }

        public void WriteLine(string text)
        {
            // 호스트 종료 시에도 서버가 돌아가야 함
            // 룸 3개 기준, 12인 플레이를 위해 호스트를 받을 서버임으로 꺼지지 않게 설정
            byte[] buff = new byte[4096];
            Buffer.BlockCopy(ShortToByte(Encoding.UTF8.GetBytes(text).Length + 2), 0, buff, 0, 2);
            Buffer.BlockCopy(Encoding.UTF8.GetBytes(text), 0, buff, 2, Encoding.UTF8.GetBytes(text).Length);
            Console.WriteLine(Server.systemTime + " 받음 = " + text);
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
            if (hostUser.member.nickname.Length > 0)
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
