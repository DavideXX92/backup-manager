using MySql.Data.MySqlClient;
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

namespace PDSserver
{
    class ServerTCP
    {
        private bool serverIsRunning;
        private Thread t;
        private int backlog = 15;
        private Socket serverSocket, keepaliveSocket;
        private int serverPort;
        private readonly object sync = new object();
        private List<HandleClient> hcList = new List<HandleClient>();
        private int counter;

        private delegate HandleClient MyTaskWorkerDelegate();

        public ServerTCP(int serverPort)
        {
            this.serverPort = serverPort;
            serverIsRunning = false;
            counter = 0;
        }

        public void Start(){
            serverIsRunning = true;
            MyConsole.setServerLog(Config.serverLogPath);
            t = new Thread(loopClients); // lambda function (parameters) => {istructions}
            t.Start();
        }

        private void loopClients()
        {
            // Server Socket
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, Config.ServerPort));

            // KeepAlive Socket
            keepaliveSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            keepaliveSocket.Bind(new IPEndPoint(IPAddress.Any, Config.KeepalivePort));
            keepaliveSocket.Listen(backlog);

            // Mark all users as offline (useful if server crashed)
            try { 
                logOutAll();
            }
            catch (Exception e)
            {
                MyConsole.Write("Error accessing database: " + e.Message + "; aborting...");
                return;
            }

            serverSocket.Listen(backlog);
            MyConsole.Write("Server started at port " + Config.ServerPort);

            while (serverIsRunning)
            {
                try
                {
                    MyConsole.Write("Waiting for a connection...");
                    Socket clientSocket = serverSocket.Accept();
                    Monitor.Enter(sync);
                    if (serverIsRunning)
                    {
                        MyConsole.Write("Connection accepted from " + clientSocket.RemoteEndPoint);
                        // client found, create a class to handle communication
                        HandleClient handleClient = new HandleClient(clientSocket, keepaliveSocket, ++counter);
                        hcList.Add(handleClient);
                        MyTaskWorkerDelegate worker = new MyTaskWorkerDelegate(handleClient.start);
                        IAsyncResult result = worker.BeginInvoke(new AsyncCallback(clearClient), worker);
                    }
                    Monitor.Exit(sync);
                }
                catch (Exception x)
                {
                    MyConsole.Write("Stopping...done!"); // because i force serverSocket.stop();
                } 
            }
        }

        public void Stop()
        {
            MyConsole.Write("Stopping...");
            Monitor.Enter(sync);
            serverIsRunning = false;
            serverSocket.Close();
            foreach (HandleClient handleClient in hcList)
                handleClient.stop();
            counter = 0;
            Monitor.Exit(sync);
        }
        
        private void clearClient(IAsyncResult result)
        {
            MyTaskWorkerDelegate d = (MyTaskWorkerDelegate)result.AsyncState;
            HandleClient client = d.EndInvoke(result);
            hcList.Remove(client);
            MyConsole.Write("Client " + client.id + " disconnected");
        }

        private void logOutAll(){
            UserService userService = new UserServiceImpl();
            List<User> users = userService.getUsers();
            try{
                foreach (User user in users)
                {
                    user.isLogged = false;
                    userService.updateUser(user);
                }
            }catch(Exception e)
            {
                throw;
            }
        }
    }
}
