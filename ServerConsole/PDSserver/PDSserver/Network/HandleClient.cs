using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PDSserver
{
    class HandleClient
    {
        public int id { get; set; }
        private Socket clientSocket;
        private Socket keepaliveSocket;
        private Socket c;


        public HandleClient(Socket clientSocket, Socket keepaliveSocket, int counter)
        { 
            this.clientSocket = clientSocket;
            this.keepaliveSocket = keepaliveSocket;
            this.id = counter;
        }

        public HandleClient start(){

            ServerController serverController = new ServerControllerImpl(clientSocket);
            Thread serverThread = new Thread(serverController.startLoop);
            serverThread.Start(true);

            c = keepaliveSocket.Accept();
            ServerController kaController = new ServerControllerImpl(c);
            Thread kaThread = new Thread(kaController.startLoop);
            kaThread.Start(false);

            serverThread.Join();
            kaThread.Join();

            return this;
            
        }

        public void stop(){
            keepaliveSocket.Close();
            c.Close();
            clientSocket.Close();
        }

    }
}
