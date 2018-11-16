using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace CoelacanthServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread tcp;
            tcp = new Thread(new ThreadStart(tcpServer));
            tcp.Start();

            Thread udp;
            udp = new Thread(new ThreadStart(udpServer));
            //udp.Start();

            while (true)
            {
                Thread.Sleep(1);
            }
        }

        public static void tcpServer()
        {
            // TCP 서버 접속
            Server StartServer = new Server(12800); 
        }

        public static void udpServer()
        {
            // UDP 서버 접속
            MulticastServer server = new MulticastServer(13000);
        }
        
    }
}