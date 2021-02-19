using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace btag.Tests
{
    public class Server
    {
        public string data = null;

        public void Listen(int port)
        {
            IPHostEntry entry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = entry.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddress, port);

            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            listener.Bind(endPoint);
            listener.Listen(10);

            Socket[] clients = new Socket[100];
            while (true)
            {
                Thread.Sleep(50);
                clients[GetNullClient(clients)] = listener.Accept();
                data = null;


            }
        }

        public int GetNullClient(in Socket[] clients)
        {
            for (int x = 0; x < clients.Length; x++)
            {
                if (clients[x] == null)
                {
                    return x;
                }
            }
            return 0;
        }
    }
}
