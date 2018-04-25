using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using System.Threading;

namespace CoelacanthServer
{
    class MulticastServer
    {
        readonly int unicast_Port = 4040;
        readonly int multicast_Port = 3030;

        public MulticastServer(int port) 
        {            
            if (port.Equals(unicast_Port))
            {
                Console.WriteLine("1. 서버 접속 시도 : " + port + " 포트 일치");
                StartListening listen = new StartListening(unicast_Port, multicast_Port);   
            }
        }
    }
}
