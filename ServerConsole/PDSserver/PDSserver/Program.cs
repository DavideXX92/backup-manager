using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PDSserver
{
    class Program
    {
        static void Main(string[] args)
        {
            ServerTCP server;

            server = new ServerTCP(Config.ServerPort);
            Thread serverThread = new Thread(server.Start);
            serverThread.Start();

            Console.WriteLine("");
            string cmd = Console.ReadLine();
            if (cmd.Equals("stop"))
            {
                server.Stop();
            }
            Console.Read();
                

            
        }
    }
}
