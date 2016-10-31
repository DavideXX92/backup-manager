using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace newServerWF
{
    class HandleClient
    {
        private Socket clientSocket;
        private Socket keepaliveSocket;
        private Socket c;


        public HandleClient(Socket clientSocket, Socket keepaliveSocket)
        { 
            this.clientSocket = clientSocket;
            this.keepaliveSocket = keepaliveSocket;
        }

        public HandleClient start(){

            ServerController serverController = new ServerControllerImpl(clientSocket);
            Thread serverThread = new Thread(serverController.startLoop);
            serverThread.Start(true);

            c = keepaliveSocket.Accept();
            MyConsole.Write(Thread.CurrentThread.ManagedThreadId + " Connection control accepted from " + c.RemoteEndPoint);
            ServerController kaController = new ServerControllerImpl(c);
            Thread kaThread = new Thread(kaController.startLoop);
            kaThread.Start(false);

            MyConsole.Write(Thread.CurrentThread.ManagedThreadId + "ASPETTOOOO");
            serverThread.Join();
            kaThread.Join();
            MyConsole.Write(Thread.CurrentThread.ManagedThreadId + "OK ESCOO");

            return this;
            
        }

        public void stop(){
            keepaliveSocket.Close();
            c.Close();
            clientSocket.Close();
        }

    }
}
