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
        private ServerController serverController;
        private ServerController kaController;

        private Thread serverThread, kaThread;

        private delegate ServerController MyTaskWorkerDelegate();

        public HandleClient(Socket clientSocket, Socket keepaliveSocket)
        { 
            this.clientSocket = clientSocket;
            this.keepaliveSocket = keepaliveSocket;
        }

        public HandleClient start(){
            
            this.serverController = new ServerControllerImpl(clientSocket);
            serverThread = new Thread(serverController.startLoop);
            serverThread.Start(true);

            Socket c = keepaliveSocket.Accept();
            MyConsole.Write(Thread.CurrentThread.ManagedThreadId + " Connection control accepted from " + c.RemoteEndPoint);
            this.kaController = new ServerControllerImpl(c);
            kaThread = new Thread(kaController.startLoop);
            kaThread.Start(false);

            MyConsole.Write(Thread.CurrentThread.ManagedThreadId + "ASPETTOOOO");
            serverThread.Join();
            kaThread.Join();
            MyConsole.Write(Thread.CurrentThread.ManagedThreadId + "OK ESCOO");

            return this;
            
        }

        public void stop(){
            keepaliveSocket.Close();
            //kaThread.Abort();
            //serverThread.Abort();
            kaController.stop();
            serverController.stop();
        }

    }
}
