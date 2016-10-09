using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace newServerWF
{
    class ServerTCP
    {
        private bool serverIsRunning;
        private Thread t;
        private TcpListener listener;
        private int serverPort;
        private readonly object sync = new object();
        private List<ServerController> scList = new List<ServerController>();
        private string serverLogPath = @"c:\ServerDir\Log\serverLog.txt";

        private delegate ServerController MyTaskWorkerDelegate();

        public ServerTCP(int serverPort)
        {
            this.serverPort = serverPort;
            serverIsRunning = false;   
        }

        public void Start(){
            serverIsRunning = true;
            MyConsole.setServerLog(serverLogPath);
            MyConsole.Write("Server started");
            t = new Thread(() => loopClients()); // lambda function (parameters) => {istructions}
            t.Start();
        }

        private void loopClients()
        {
            listener = new TcpListener(IPAddress.Any, serverPort); //Listen on all the network interface
            MyConsole.Write(Thread.CurrentThread.ManagedThreadId + " The server is running at port " + serverPort);
            // Start Listening at the specified port
            listener.Start();
            while (serverIsRunning)
            {
                try
                {
                    // wait for client connection
                    MyConsole.Write(Thread.CurrentThread.ManagedThreadId + " Waiting for a connection...");
                    TcpClient newClient = listener.AcceptTcpClient();
                    Monitor.Enter(sync);
                    if (serverIsRunning)
                    {
                        MyConsole.Write(Thread.CurrentThread.ManagedThreadId + " Connection accepted from " + newClient.Client.RemoteEndPoint);

                        // client found.
                        // create a class to handle communication
                        ServerController serverController = new ServerControllerImpl(newClient);
                        scList.Add(serverController);
                        MyTaskWorkerDelegate worker = new MyTaskWorkerDelegate(serverController.startLoop);
                        IAsyncResult result = worker.BeginInvoke(new AsyncCallback(clearClient), worker);
                    }
                    Monitor.Exit(sync);
                }
                catch (Exception x)
                {
                    MyConsole.Write(Thread.CurrentThread.ManagedThreadId + " Server stopped"); // because i force listener.stop();
                } 
            }
        }

        private void clearClient(IAsyncResult result)
        {
            MyTaskWorkerDelegate d = (MyTaskWorkerDelegate)result.AsyncState;
            ServerController client = d.EndInvoke(result);
            scList.Remove(client);
        }
       
        public void Stop(){
            Monitor.Enter(sync); 
            serverIsRunning = false;
            listener.Stop();
            foreach (ServerController serverController in scList)
                serverController.stop();
            Monitor.Exit(sync); 
        }

    }
}
