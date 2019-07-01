using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
           Socket sSocket = Server();

            List<Task> tasks = new List<Task>();
            tasks.Add(Task.Factory.StartNew(() => HandleClient(sSocket)));
            tasks.Add(Task.Factory.StartNew(() => HandleClient(sSocket)));
            tasks.Add(Task.Factory.StartNew(() => HandleClient(sSocket)));
            tasks.Add(Task.Factory.StartNew(() => HandleClient(sSocket)));

            Task.WaitAll(tasks.ToArray());
        }

        static int ConnectionCount = 0;

        static Socket Server()
        {
            int port = 6000;
            string host = "192.168.50.237";

            IPAddress ip = IPAddress.Parse(host);
            IPEndPoint ipe = new IPEndPoint(IPAddress.Any, port);

            Socket sSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //sSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            sSocket.Bind(ipe);
            sSocket.Listen(0);
            Console.WriteLine("监听已经打开，请等待");

            //sSocket.Close();
            return sSocket;
        }

        static void HandleClient(Socket sSocket)
        {
            while (true)
            {
                //receive message
                Socket serverSocket = sSocket.Accept();
                Console.WriteLine("连接已经建立");
                Console.WriteLine($"Connection count: {ConnectionCount++}.");

                string recStr = "";
                byte[] recByte = new byte[4096];
                int bytes = serverSocket.Receive(recByte, recByte.Length, 0);
                recStr += Encoding.ASCII.GetString(recByte, 0, bytes);

                //send message
                Console.WriteLine("服务器端获得信息:{0}", recStr);
                string sendStr = $"send to client :hello. Client Port: {(serverSocket.RemoteEndPoint as IPEndPoint)?.Port}, Server Port: {(serverSocket.LocalEndPoint as IPEndPoint).Port}";
                byte[] sendByte = Encoding.ASCII.GetBytes(sendStr);
                serverSocket.Send(sendByte, sendByte.Length, 0);
                serverSocket.Close();
            }
        }
    }
}
