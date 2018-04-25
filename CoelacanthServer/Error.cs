using System;
namespace CoelacanthServer
{
    public class Error
    {
        private int _errorcode;
        public int ErrorCode
        {
            get { return _errorcode; }
            set { _errorcode = value; }
        }

        public Error(string message)
        {
            ErrorMessage(message);
        }

        private int ErrorMessage(string message)
        {
            switch (message)
            {
                case "요청한 주소는 해당 컨텍스트에서 유효하지 않습니다": ErrorCode = 10049; return ErrorCode;
                case "네트워크를 사용할 수 없기 때문에 소켓 작업을 진행할 수 없습니다": ErrorCode = 10050; return ErrorCode;
                case "연결할 수 없는 네트워크에서 소켓 작업을 시도했습니다": ErrorCode = 10051; return ErrorCode;
                case "해당 작업이 진행되는 동안 오류가 발생하여 연결이 끊겼습니다": ErrorCode = 10052; return ErrorCode;
                case "현재 연결은 사용자의 호스트 시스템의 소프트웨어의 의해 중단되었습니다": ErrorCode = 10053; return ErrorCode;
                case "현재 연결은 원격 호스트에 의해 강제로 끊겼습니다": ErrorCode = 10054; return ErrorCode;
                case "대기열이 또는 버퍼가 부족하여 소켓에서 해당 작업을 진행하지 못했습니다": ErrorCode = 10055; return ErrorCode;
                case "이미 연결된 소켓에서 다른 연결을 요청했습니다": ErrorCode = 10056; return ErrorCode;
                case "소켓이 연결되어 있지 않거나 Sendto 호출을 사용하여 데이터그램 소켓에 보내는 경우에 주소가 제공되지 않아서 데이터를 보내거나 받도록 요청할 수 없습니다": ErrorCode = 10057; return ErrorCode;
                case "해당 소켓이 종료되었으므로 데이터 보내거나 받을 수 없습니다": ErrorCode = 10058; return ErrorCode;
                case "일부 커널 개체에 대한 참조가 너무 많습니다": ErrorCode = 10059; return ErrorCode;
                case "연결된 구성원으로부터 응답이 없어 연결하지 못했거나, 호스트로부터 응답이 없어 연결이 끊어졌습니다": ErrorCode = 10060; return ErrorCode;
                case "대상 컴퓨터에서 연결을 거부했으므로 연결하지 못했습니다": ErrorCode = 10061; return ErrorCode;
                case "이름을 해석할 수 없습니다": ErrorCode = 10062; return ErrorCode;
                case "이름 또는 이름의 구성 요소가 너무 깁니다": ErrorCode = 10063; return ErrorCode;
                case "호스트가 작동하지 않기 때문에 소켓 작업을 진행할 수 없습니다": ErrorCode = 10064; return ErrorCode;
                case "연결할 수 없는 호스트로 소켓 작업을 시도했습니다": ErrorCode = 10065; return ErrorCode;
                default: return 0;
            }

        }
    }
}
