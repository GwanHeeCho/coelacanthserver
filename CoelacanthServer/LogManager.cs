using System;
using System.IO;

namespace CoelacanthServer
{
    public class LogManager
    {
        private static LogManager _instance;
        public static LogManager Instance
        {
            get => _instance;
            set => _instance = value;
        }

        private static string _today = DateTime.Now.ToString("yyyy-MM-dd");

        public void logText(string _text)
        {
            StreamWriter sw = new StreamWriter("D:\\Program Files\\Develop\\CoelacanthServer\\CoelacanthServer\\log\\ServerLog_main_" + _today + "_.txt", true);
            sw.WriteLine(_text);
            sw.Close();
        }
    }
}