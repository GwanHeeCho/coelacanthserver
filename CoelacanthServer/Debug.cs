using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoelacanthServer
{
    public static class Debug
    {
        public static void Log(string value)
        {
            Console.WriteLine(Server.systemTime + value);
        }

        public static void Log(short value)
        {
            Console.WriteLine(Server.systemTime + value);
        }

        public static void Log(int value)
        {
            Console.WriteLine(Server.systemTime + value);
        }

        public static void Log(float value)
        {
            Console.WriteLine(Server.systemTime + value);
        }

        public static void Log(double value)
        {
            Console.WriteLine(Server.systemTime + value);
        }
    }
}
