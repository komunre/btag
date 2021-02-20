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
        List<Socket> clients = new List<Socket>();

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
                clients.Add(listener.Accept());
                data = null;
                HandleClient(clients.Count - 1);
            }
        }

        private async void HandleClient(int clIndex)
        {
            await Task.Run(() =>
            {
                while (clients[clIndex].Connected)
                {
                    var length = new byte[2];
                    clients[clIndex].Receive(length);
                    var message = new byte[BitConverter.ToUInt16(length)];
                    clients[clIndex].Receive(message);

                }
            });
        } 

        public void Parse()
        {

        }
    }
}
