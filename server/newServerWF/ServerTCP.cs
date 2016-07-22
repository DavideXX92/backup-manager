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
        private List<HandleClient> hcList = new List<HandleClient>();

        private delegate HandleClient MyTaskWorkerDelegate();

        public ServerTCP(int serverPort)
        {
            this.serverPort = serverPort;
            serverIsRunning = false;   
        }

        public void Start(){
            serverIsRunning = true;
            MyConsole.write("Server started");
            t = new Thread(() => loopClients()); // lambda function (parameters) => {istructions}
            t.Start();
        }

        private void loopClients()
        {
            listener = new TcpListener(IPAddress.Any, serverPort); //Listen on all the network interface
            MyConsole.write(Thread.CurrentThread.ManagedThreadId + " The server is running at port " + serverPort);
            // Start Listening at the specified port
            listener.Start();
            while (serverIsRunning)
            {
                try
                {
                    // wait for client connection
                    MyConsole.write(Thread.CurrentThread.ManagedThreadId + " Waiting for a connection...");
                    TcpClient newClient = listener.AcceptTcpClient();
                    Monitor.Enter(sync);
                    if (serverIsRunning)
                    {
                        MyConsole.write(Thread.CurrentThread.ManagedThreadId + " Connection accepted from " + newClient.Client.RemoteEndPoint);

                        // client found.
                        // create a class to handle communication
                        HandleClient hc = new HandleClient(newClient);
                        hcList.Add(hc);
                        MyTaskWorkerDelegate worker = new MyTaskWorkerDelegate(hc.start);
                        IAsyncResult result = worker.BeginInvoke(new AsyncCallback(clearClient), worker);
                    }
                    Monitor.Exit(sync);
                }
                catch (Exception x)
                {
                    MyConsole.write(Thread.CurrentThread.ManagedThreadId + " Server stopped"); // because i force listener.stop();
                } 
            }
        }

        private void clearClient(IAsyncResult result)
        {
            MyTaskWorkerDelegate d = (MyTaskWorkerDelegate)result.AsyncState;
            HandleClient client = d.EndInvoke(result);
            hcList.Remove(client);
        }
       
        public void Stop(){
            Monitor.Enter(sync); 
            serverIsRunning = false;
            listener.Stop();
            foreach (HandleClient hc in hcList)
                hc.stop();
            Monitor.Exit(sync); 
        }

    }
}
