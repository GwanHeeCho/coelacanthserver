using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoelacanthServer
{
    class Util
    {
        /* ---------------------------------------------------
         * [참고]
         * http://sinoroo.tistory.com/entry/C-BitCOnverter%EB%A5%BC-%EC%9D%B4%EC%9A%A9%ED%95%9C-%EB%B3%80%ED%99%98
        --------------------------------------------------- */
        public static int GetShort(byte[] buffer, int index, out short value)
        {   // 바이트 배열에 지정된 된 위치에 2 바이트에서 변환 하는 16 비트 부호 있는 정수를 반환
            // https://msdn.microsoft.com/ko-kr/library/system.bitconverter.toint16(v=vs.110).aspx#%EC%84%A4%EB%AA%85
            value = BitConverter.ToInt16(buffer, index);
            return index + 2;
        }

        public static int GetInteger(byte[] buffer, int index, out int value)
        {   // 바이트 배열에 지정된 된 위치에서 4 바이트에서 변환 하는 32 비트 부호 있는 정수를 반환하는 함수
            // https://msdn.microsoft.com/ko-kr/library/system.bitconverter.toint32(v=vs.110).aspx
            value = BitConverter.ToInt32(buffer, index);
            return index + 4;
        }

        public static int GetString(byte[] buffer, int index, out string text)
        {
            // ParsePacket 함수에서 유저의 이벤트가 발생했을 경우, 사용한다.
            // http://www.csharpstudy.com/Tip/Tip-string-encoding.aspx
            short length;
            GetShort(buffer, index, out length);
            text = Encoding.UTF8.GetString(buffer, index + 2, length);
            return index + text.Length + 2;
        }

        public static byte[] IntToByte(int val)
        {
            // https://stackoverflow.com/questions/4058339/what-is-0xff-and-why-is-it-shifted-24-times
            // http://hkpark.netholdings.co.kr/web/inform/default2/inform_list.asp?menu_id=15637&id=1201 (진수법을 활용하면 좋은 점)
            /*
             * 10진수로 된 데이터를 8진수나 16진수로 표기하면 쉽다고 한다.
             * 2진수<->10진수는 어렵다고 하는데 자세한 부분을 더 찾아봐야 한다.
            */
            byte[] temp = new byte[4];
            temp[3] = (byte)((val & 0xff000000) >> 24);
            temp[2] = (byte)((val & 0x00ff0000) >> 16);
            temp[1] = (byte)((val & 0x0000ff00) >> 8);
            temp[0] = (byte)((val & 0x000000ff));
            return temp;
        }

        public static byte[] ShortToByte(int val)
        {
            byte[] temp = new byte[2];
            temp[1] = (byte)((val & 0x0000ff00) >> 8);
            temp[0] = (byte)((val & 0x000000ff));
            return temp;
        }

        /*
         * http://netmaid.tistory.com/60
         * http://msdn.microsoft.com/ko-kr/library/system.buffer.blockcopy.aspx
         * 아래 3가지는 자료형에 맞게 byte[] 데이터를 복사한다.
         * 데이터가 누락되어있을 경우, 데이터 크기에 맞게 원본 바이트 길이대로 맞춰준다고 한다.
         * 아직 제대로 쓰지 않지만, 유저 정보를 실시간으로 동기화 할 때, 사용해볼 예정   
        */
        public static int SetShort(byte[] buffer, int index, int value)
        {
            Buffer.BlockCopy(ShortToByte(value), 0, buffer, index, 2);
            return index + 2;
        }

        public static int SetInteger(byte[] buffer, int index, int value)
        {
            Buffer.BlockCopy(IntToByte(value), 0, buffer, index, 4);
            return index + 4;
        }

        public static int SetString(byte[] buffer, int index, string text)
        {
            byte[] temp = Encoding.UTF8.GetBytes(text);
            Buffer.BlockCopy(ShortToByte(temp.Length), 0, buffer, index, 2);
            Buffer.BlockCopy(temp, 0, buffer, index + 2, temp.Length);
            return index + temp.Length + 2;
        }
    }
}
