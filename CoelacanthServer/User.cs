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
        List<User> Room = new List<User>();
        User hostUser = null;
        User firstUser = null;
        User secondUser = null;
        User thirdUser = null;
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

        private void PlayerStat(string nick, int id, int sequence, float x, float z, float rotate, string room, bool ready, bool host)
        {
            this.member.nickname = nick;
            this.member.id = id;
            this.member.sequence = sequence;
            this.member.x = x;
            this.member.z = z;
            this.member.rotate = rotate;
            this.member.room = room;
            this.member.ready = ready;
            this.member.host = host;
            PlayerStat(this);
        }

        private static void PlayerStat(User user)
        {
            Console.WriteLine("===================");
            Console.WriteLine("닉네임 : " + user.member.nickname);
            Console.WriteLine("아이디 : " + user.member.id);
            Console.WriteLine("순번 : " + user.member.sequence);
            Console.WriteLine("x좌표 : " + user.member.x);
            Console.WriteLine("z좌표 : " + user.member.z);
            Console.WriteLine("회전값 : " + user.member.rotate);
            Console.WriteLine("룸코드 : " + user.member.room);
            Console.WriteLine("준비상태 : " + user.member.ready);
            Console.WriteLine("===================");
        }

        // 개설 된 룸 정보 찾기
        private void OpenRoomSearch()
        {
            for (int i = 0; i < Server.UserList.Count; i++)
            {
                if (Server.UserList[i] != this)
                {
<<<<<<< HEAD
                    if (Server.UserList[i].hostUser != null && Server.UserList[i].thirdUser == null)
=======
                    // 개설 된 방 확인
                    if (Server.UserList[i].member.host == true)
>>>>>>> 9ecb2a80823676152cbd710b819efdf29a32a8fe
                    {
                        // 4인 모두 나오게 될거임 hostUser에 host.hostUser를 넣어서 무조건 true 성립됨
                        // 호스트 유저의 룸 정보와 조회하려는 유저의 룸 정보를 비교해서 찾아내는 함수 구현해두기
                        
                        // 방 목록에 추가
                        Room.Add(Server.UserList[i]);
                    }
                }
            }
            Debug.Log("OPEN ROOM : " + Room.Count);
        }

        // 룸 생성
        private void CreatedRoom(string nickname, int id, int sequence, string room)
        {
            hostUser = this;
            firstUser = null;
            secondUser = null;
            thirdUser = null;
<<<<<<< HEAD
            PlayerStat(nickname, id, sequence, 0, 0, 0, room, false, true);
=======
            hostUser.PlayerStat(nickname, id, sequence, 0, 0, 0, room, false, true);
>>>>>>> 9ecb2a80823676152cbd710b819efdf29a32a8fe
            WriteLine(string.Format("INITIALIZE:{0}:{1}:{2}:{3}", sequence, nickname, id, room));
        }

        // 룸 랜덤 입장
        private void JoinAvailableRoom(User my, string nickname, int id, int sequence)
        {
            Debug.Log("유저정보 : " + nickname + ":" + id);
            if (hostUser == null)
            {
                // host에 hostUser의 정보가 있는 User 클래스 정보를 넣는다.
                User host = Room[Server.randomRoomNumber.Next(Room.Count)];
                hostUser = host.hostUser;
                PlayerStat(nickname, id, sequence, 0, 0, 0, host.member.room, false, false);

                JoinToGuest(my, host, sequence);
            }
            else
            {
            }
            WriteLine(string.Format("INITIALIZE:{0}:{1}:{2}:{3}", member.sequence, member.nickname, member.id, member.room));
        }

        // 게스트 순서 할당 (폴리싱)
        private int GuestSequence(User first, User second, User thrid)
        {
            // 정상적인 값이 나오지 않음
            return (first == null) ? 2 : (second == null ? 3 : 4);
        }

        // 게스트 유저 추가
        private void JoinToGuest(User my, User host, int number)
        {
            // Max Connecting 4인 기준 개발 (G-STAR 대용)
            if (my.hostUser.member.id == host.hostUser.member.id)
            {
                switch (number)
                {
                    case 2:
                        host.firstUser = my;
                        break;
                    case 3:
                        host.secondUser = my;
                        break;
                    case 4:
                        host.thirdUser = my;
                        host.hostUser.WriteLine(string.Format("FULL:{0}", host.hostUser.member.room));
                        break;
                    default:
                        break;
                }
            }
        }

        private void SaveInviteRoomUser()
        {

        }

        // 같은 룸 안의 유저들 확인
        private void ConveyInformationUser()
        {
            // 호스트의 룸 PK값이 같을 경우
            if (hostUser.member.room == this.member.room)
            {
                for (int i = 0; i < Room.Count; i++)
                {
                    // 호스트의 룸 PK와 개설된 룸의 호스트 룸 PK 값이 같을 경우
                    if (hostUser.member.room == Room[i].hostUser.member.room)
                    {
                        // 룸에 입장한 인원 수
                        /*
                         * 두번째 유저는 host.firstUser에 자신을 넣는다. (count == 2는 null이 아님)
                         * 세번째 유저는 host.secondUser에 자신을 넣는다. (count == 3은 null이 아님)
                         * 마지막 유저는 host.thirdUser에 자신을 넣는다. (count == 4는 null이 아님)
                         * 
                         * 방에 유저가 꽉찼으면 host.firstUser와 host.secondUser정보 가져오고, 내 정보 보내줌 (first, second 에게)
                         * 방에 3명 있으면 host.firstUser 정보 가져오고, 내 정보 보내줌 (fisrt 에게)
                         * 방에 2명 있으면 아무것도 안함 (초기화할때, 호스트 정보 가져옴)
                         * 방에 1명 있으면 아무것도 안함 (호스트 혼자있음)
                        */

                        // 두번째 유저
                        if (Room[i].firstUser != null)
                        {
                            if (Room[i].secondUser == null && Room[i].thirdUser == null)
                            {
                                Debug.Log("두번째 유저");
                            }
                        }
                        // 세번째 유저
                        else if (Room[i].secondUser != null)
                        {
                            if (Room[i].firstUser != null && Room[i].thirdUser == null)
                            {
                                Debug.Log("세번째 유저");
                            }
                        }
                        // 마지막 유저
                        else if (Room[i].thirdUser != null)
                        {
                            if (Room[i].firstUser != null & Room[i].secondUser != null)
                            {
                                Debug.Log("마번째 유저");
                            }
                        }
                    }
                }
            }
        }

        private void ParsePacket(int length)
        {
            Debug.Count++;
            string systemTime = DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss]");
            string msg = Encoding.UTF8.GetString(data.buffer, 2, length - 2);
            string[] text = msg.Split(':');
            string log = Debug.Count + ". " + systemTime + " 수신 = " + msg;
<<<<<<< HEAD

=======
    
>>>>>>> 9ecb2a80823676152cbd710b819efdf29a32a8fe
            LogManager.logText(log);
            Debug.Log(log);
            
            if (text[0].Equals("CONNECT"))
            {
                Console.WriteLine("[ :: Total online user : " + Server.UserList.Count + " :: ]");
                // 1. 접속한 유저의 정보가 정상적으로 넘어왔는지 확인한다.
                TryCatch.GetValue(text[1], text[2]);
                // 2. 개설 된 룸 정보를 가져온다.
                OpenRoomSearch();
                // 3-1. 개설 된 룸이 없으면 생성한다.
                if (Room.Count == 0)
                {
                    Server.RoomPrivateKey = PrivateCharKey(Server.randomRoomNumber, 20);
                    CreatedRoom(text[1], int.Parse(text[2]), Server.UserList.Count, Server.RoomPrivateKey);
                }
                // 3-2. 개설 된 룸에 랜덤하게 입장한다.
                else if (Room.Count > 0)
                {
                    JoinAvailableRoom(this, text[1], int.Parse(text[2]), Server.UserList.Count);
                }
                // 4. 룸 안의 유저들의 데이터 가져오기
                //ConveyInformationUser();
            }
            else if (text[0].Equals("FULL"))
            {
                Debug.Log("호스트 : " + text[1] + ":" + text[2]);
                Debug.Log("ROOM : " + text[3]);
                OpenRoomSearch();
                Debug.Log("OPEN ROOM : " + Room.Count);
                for (int i = 0; i < Room.Count; i++)
                {
                    if (text[3] == Room[i].hostUser.member.room)
                    {
                        Debug.Log("룸이 같다!");
                    }
                }
            }
            else if (text[0].Equals("CREATEROOM"))
            {
            }
            else if (text[0].Equals("JOINGAME"))
            {
            }
            else if (text[0].Equals("READY")) // 클라이언트가 GUEST나 HOST 패킷을 받고 READY를 송신한 경우
            {
                Debug.Log("PLAYER READY");
                // 레디를 내꺼에 넣는다
                if (this.member.nickname == text[1])
                    if (this.member.id == int.Parse(text[2]))
                        if (this.member.room == text[3])
                            this.member.ready = Convert.ToBoolean(text[4]);
                Debug.Log(this.member.nickname + ", " + this.member.id + ", " + this.member.ready);
                //// 호스트에 내꺼를 동기화한다
                //for (int i = 0; i < Room.Count; i++)
                //{
                //    // 개설 된 방의 호스트 유저와 내가 참여한 방의 PK값이 같을 경우
                //    if (Room[i].hostUser.member.room == hostUser.member.room)
                //    {
                //        Debug.Log("찾았다" + hostUser.hostUser.member.room);
                //        // 모든 유저를 호스트에 맞춰 동기화한다
                //        hostUser.WriteLine(string.Format("SROCE:{0}:{1}:{2}", this.member.nickname, this.member.id, this.member.score));
                //        firstUser.WriteLine(string.Format("SROCE:{0}:{1}:{2}", this.member.nickname, this.member.id, this.member.score));
                //        secondUser.WriteLine(string.Format("SROCE:{0}:{1}:{2}", this.member.nickname, this.member.id, this.member.score));
                //        thirdUser.WriteLine(string.Format("SROCE:{0}:{1}:{2}", this.member.nickname, this.member.id, this.member.score));
                //    }
                //    else
                //    {
                //        break;
                //    }
                //}
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
                Debug.Log(text[1] + ":" + text[2]);
                
            }
            else if (text[0].Equals("BTNSTART"))
            {
                // 호스트의 닉네임 아이디 룸정보 가져옴
                // 
                
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
            else if(text[0].Equals("SCORE"))
            {
                // 점수를 내꺼에 넣는다
                this.member.score = int.Parse(text[3]);
                Debug.Log(this.member.nickname + ", " + this.member.id + ", " + this.member.score);
                // 호스트에 내꺼를 동기화한다
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
                    Server.RoomCount--;
                    LogManager.sw.Close();
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
            Debug.Count++;
            // 호스트 종료 시에도 서버가 돌아가야 함
            // 룸 3개 기준, 12인 플레이를 위해 호스트를 받을 서버임으로 꺼지지 않게 설정
            string systemTime = DateTime.Now.ToString(". [yyyy-MM-dd HH:mm:ss] ");
            byte[] buff = new byte[4096];
            Buffer.BlockCopy(ShortToByte(Encoding.UTF8.GetBytes(text).Length + 2), 0, buff, 0, 2);
            Buffer.BlockCopy(Encoding.UTF8.GetBytes(text), 0, buff, 2, Encoding.UTF8.GetBytes(text).Length);
            Debug.Log(Debug.Count + systemTime + "발신 = " + text);
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
        }
    }
}
