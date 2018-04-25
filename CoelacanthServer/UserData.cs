using System.Net.Sockets;

namespace CoelacanthServer
{
    class UserData
    {
        // 유저의 버퍼 크기
        // 서버 접속을 위한 소켓
        // 보내고자하는 패킷을 담을 버퍼
        // 데이터의 길이
        // https://docs.microsoft.com/ko-kr/dotnet/framework/network-programming/asynchronous-server-socket-example
        public const int BufferSize = 32768;
        public Socket workSocket = null;
        public byte[] buffer = new byte[BufferSize];
        public int recvlen = 0;
    }
}
