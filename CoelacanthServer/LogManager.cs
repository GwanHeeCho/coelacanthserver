<<<<<<< HEAD
using System;
using System.IO;

namespace CoelacanthServer
{
    public class LogManager
    {
        private static string today = DateTime.Now.ToString("yyyy-MM-dd");
        public static StreamWriter sw = new StreamWriter("log\\CoelacanthPlayLog_" + today + "_main.txt", true);

        public static void logText(string _text)
        {
            sw.WriteLine(_text);
        }
    }
=======
using System;
using System.IO;

namespace CoelacanthServer
{
    public class LogManager
    {
        private static string today = DateTime.Now.ToString("yyyy-MM-dd");

        public static void logText(string _text)
        {
            StreamWriter sw = new StreamWriter("log\\CoelacanthPlayLog_" + today + "_main.txt", true);
            sw.WriteLine(_text);
            sw.Close();
        }
    }
>>>>>>> 9ecb2a80823676152cbd710b819efdf29a32a8fe
}