using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TcpTest
{
    class Program
    {
        static void Main(string[] args)
        {
            ServicePointManager.DefaultConnectionLimit = 1;

            Console.CancelKeyPress += Console_CancelKeyPress;

            while (true)
            {
                if (Console.ReadKey().Key == ConsoleKey.S)
                {
                    RunMultipleTime();
                    RunOneTime();
                }
            }
        }

        static void RunOneTime()
        {
            Client();
        }

        static void RunMultipleTime()
        {
            int count = 10000;
            List<Task> tasks = new List<Task>(count);
            for (int i = 0; i < count; i++)
            {
                tasks.Add(Task.Factory.StartNew(() => Client()));
            }

            Task.WaitAll(tasks.ToArray());
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
        }

        //netsh int ipv4 show dynamicport tcp

        //netsh int ipv4 set dynamicport tcp start=49153 num=500
        static void Client()
        {
            int port = int.Parse(ConfigurationManager.AppSettings["port"]);
            string host = ConfigurationManager.AppSettings["ip"];//服务器端ip地址

            IPAddress ip = IPAddress.Parse(host);
            IPEndPoint ipe = new IPEndPoint(ip, port);

            try
            {
                Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                clientSocket.NoDelay = true;
                //clientSocket.Bind(new IPEndPoint(IPAddress.Any, 0));
                clientSocket.Connect(ipe);

                //send message
                string sendStr = $"send to server : hello,ni hao. Client Port: {  (clientSocket.LocalEndPoint as IPEndPoint)?.Port}";
                byte[] sendBytes = Encoding.ASCII.GetBytes(sendStr);
                clientSocket.Send(sendBytes);

                //receive message
                string recStr = "";
                byte[] recBytes = new byte[4096];
                int bytes = clientSocket.Receive(recBytes, recBytes.Length, 0);
                recStr += Encoding.ASCII.GetString(recBytes, 0, bytes);
                Console.WriteLine(recStr);

                //clientSocket.Disconnect(true);
                // To weak
                if (DateTime.Now.Second % 4 == 0)
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }
            catch (SocketException e) when (!e.Message.Contains("refuse"))
            {
                Console.WriteLine($"Exception: {e.Message}");
            }
        }
    }
}
