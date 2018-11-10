using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoelacanthServer
{
    public class Debug
    {
        private static int _count;
        public static int Count
        {
            get { return _count; }
            set { _count = value; }
        }
        
        public static void Log(string value)
        {
            Console.WriteLine(value);
        }

        public static  void Log(short value)
        {
            Console.WriteLine(value);
        }

        public static void Log(int value)
        {
            Console.WriteLine(value);
        }

        public static void Log(float value)
        {
            Console.WriteLine(value);
        }

        public static void Log(double value)
        {
            Console.WriteLine(value);
        }
    }
}
