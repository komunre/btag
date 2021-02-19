using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;

namespace btag.Tests
{
    public class Server
    {
        public string data = null;
        Socket[] clients = new Socket[100];

        public void Listen(int port)
        {
            IPHostEntry entry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = entry.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddress, port);

            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            listener.Bind(endPoint);
            listener.Listen(10);

            while (true)
            {
                Thread.Sleep(50);
                var clIndex = GetNullClient(clients);
                clients[clIndex] = listener.Accept();
                data = null;
                HandleClient(clIndex);
            }
        }

        private async void HandleClient(int clIndex)
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    var length = new byte[2];
                    clients[clIndex].Receive(length);
                    var message = new byte[BitConverter.ToUInt16(length)];
                    clients[clIndex].Receive(message);

                }
            });
        } 

        private int GetNullClient(in Socket[] clients)
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
