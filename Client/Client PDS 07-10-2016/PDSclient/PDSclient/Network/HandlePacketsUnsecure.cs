using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;

namespace PDSclient {
    
    class HandlePacketsUnsecure : HandlePackets
    {
        private TcpClient client;
        private NetworkStream netStream;
        private const int CODELENGTH = 3;
        private byte[] rBuffer;
        private byte[] wBuffer;
        private int bytesRead;
        private bool imDoingRequest;
        private object _lock = new object();

        public HandlePacketsUnsecure(string serverIP, int port) {
            connectToServer(serverIP, port);
        }

        private void connectToServer(string serverIP, int port) {
            IPAddress remoteIP = IPAddress.Parse(serverIP);
            this.client = new TcpClient();
            IPEndPoint IP_End = new IPEndPoint(remoteIP, port);
            try
            {
                Console.WriteLine("Connecting...");
                client.Connect(IP_End);
                if (client.Connected)
                {
                    this.netStream = client.GetStream();
                    Console.WriteLine("Unsecured Connected to Server: " + IP_End.ToString() + "\n");
                }
            }
            catch (Exception x)
            {
                Console.WriteLine("impossibile stabilire la connessione non sicura");
                this.client.Close();
                throw;
            }
        }

        private void sendFile(WrapFile wrapFile) {
            try {
                //Send Size
                byte[] lengthBytes = new byte[sizeof(int)];
                lengthBytes = BitConverter.GetBytes(wrapFile.size);
                netStream.Write(lengthBytes, 0, lengthBytes.Length);

                int length = wrapFile.size;
                int bytesReamining = length;
                int bytesSent = 0;
                int bytesToSend;
                int chunk = 1024;

                wBuffer = new byte[chunk];

                wrapFile.fs.Lock(0, wrapFile.fs.Length);
                while (bytesReamining > 0) {
                    if (bytesReamining > chunk)
                        bytesToSend = chunk;
                    else
                        bytesToSend = bytesReamining;

                    wrapFile.fs.Read(wBuffer, 0, bytesToSend);
                    netStream.Write(wBuffer, 0, bytesToSend);
                    bytesSent += bytesToSend;
                    bytesReamining -= bytesToSend;
                }
                wrapFile.fs.Unlock(0, wrapFile.fs.Length);
                wrapFile.fs.Close();
            } catch (Exception e) {
                throw e;
            }
        }
        private void receiveFile(WrapFile wrapFile) {
            try {
                //Receive size
                byte[] lengthBytes = new byte[sizeof(int)];
                int bytesRead = myReceive(netStream, lengthBytes, 0, lengthBytes.Length);
                int length = BitConverter.ToInt32(lengthBytes, 0);

                int bytesReamining = length;
                int bytesReceived = 0;
                bytesRead = 0;
                int bytesToRead;
                int chunk = 1024;

                rBuffer = new byte[chunk];

                while (bytesReamining > 0) {
                    if (bytesReamining > chunk)
                        bytesToRead = chunk;
                    else
                        bytesToRead = bytesReamining;

                    bytesRead = myReceive(netStream, rBuffer, 0, bytesToRead);

                    if (bytesRead < 0)
                        break;
                    wrapFile.fs.Write(rBuffer, 0, bytesRead);
                    bytesReceived += bytesRead;
                    bytesReamining -= bytesRead;
                }
                wrapFile.fs.Close();
            } catch (Exception e) {
                throw e;
            }
        }

        /*CLIENT*/
        public Object doRequest(string code, Object objRequest) {
            lock (_lock)
            {
                if (imDoingRequest == false)
                    imDoingRequest = true;
                else
                    throw new BusyResourceException();
            }
            string receive;
            byte[] codeBytes = new byte[CODELENGTH];

            try {
                sendPacket(code, objRequest);

                bytesRead = myReceive(netStream, codeBytes, 0, codeBytes.Length);
                if (bytesRead <= 0)
                    throw new Exception("Connection lost with the client...");
                else {
                    receive = Encoding.ASCII.GetString(codeBytes, 0, bytesRead);
                    Console.WriteLine("You: " + receive);
                    Object objSerialized = receiveResponse();
                    Type t = objRequest.GetType();
                    string json = (string)objSerialized;
                    Object objResponse = JsonConvert.DeserializeObject(json, t);

                    if (receive.Equals("001")) {
                        WrapFile wrapFileRes = (WrapFile)objResponse;
                        try {
                            receiveFile((WrapFile)objRequest);
                            wrapFileRes.message = "File ricevuto correttamente";
                        } catch (Exception e) {
                            Console.WriteLine("Errore di rete: impossibile ricevere il file");
                            wrapFileRes.error = "Errore durante la ricezione del file";
                        } finally {
                            objResponse = (Object)wrapFileRes;
                        }
                    }

                    return objResponse;
                }
            } catch (Exception e) {
                Console.Write(e.Message);
                throw e;
            }
            finally
            {
                lock (_lock)
                {
                    imDoingRequest = false;
                }
            }

        }
        private void sendPacket(string code, Object obj) {
            byte[] codeBytes = new byte[3];
            byte[] lengthBytes = new byte[sizeof(int)];
            string json;

            try {
                if (obj.GetType() == typeof(WrapFile)) {
                    WrapFile wrapFile = (WrapFile)obj;
                    json = JsonConvert.SerializeObject(wrapFile.file, Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore }); //mando solo il file non tutto il wrapfile
                } else
                    json = JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore });

                //Console.WriteLine(json);

                codeBytes = ASCIIEncoding.ASCII.GetBytes(code);
                netStream.Write(codeBytes, 0, codeBytes.Length);

                int length = ASCIIEncoding.ASCII.GetBytes(json).Length;
                lengthBytes = BitConverter.GetBytes(length);
                netStream.Write(lengthBytes, 0, lengthBytes.Length);

                wBuffer = new byte[length];
                wBuffer = ASCIIEncoding.ASCII.GetBytes(json);
                netStream.Write(wBuffer, 0, length);

                if (code.Equals("002")) {
                    sendFile((WrapFile)obj);
                }
            } catch (Exception e) {
                throw e;
            }

        }
        private Object receiveResponse() {
            int length;
            byte[] lengthBytes = new byte[sizeof(int)];
            Object obj;

            try {
                bytesRead = myReceive(netStream, lengthBytes, 0, lengthBytes.Length);
                length = BitConverter.ToInt32(lengthBytes, 0);

                obj = receiveObject(length);

                return obj;

            } catch (Exception e) {
                throw e;
            }

        }
        private Object receiveObject(int length) {
            int bytesReamining = length;
            int bytesReceived = 0;
            int bytesRead;
            int bytesToRead;
            int chunk = 1024;
            string json;

            rBuffer = new byte[length];

            while (bytesReamining > 0) {
                if (bytesReamining > chunk)
                    bytesToRead = chunk;
                else
                    bytesToRead = bytesReamining;

                bytesRead = myReceive(netStream, rBuffer, bytesReceived, bytesToRead);
                if (bytesRead < 0)
                    break;
                bytesReceived += bytesRead;
                bytesReamining -= bytesRead;
            }
            if (bytesReceived == length) {
                byte[] objBytes = new byte[length];
                Buffer.BlockCopy(rBuffer, 0, objBytes, 0, length);
                json = Encoding.ASCII.GetString(objBytes, 0, length);
                Object obj = (Object)json;
                return obj;
            } else
                //return null;
                throw new Exception("Impossibile ricevere il file (l'object), problema di rete");
        }

        private int myReceive(NetworkStream netStream, byte[] bufferDst, int offset, int bytesToRead) {
            byte[] buffer = new byte[bytesToRead];
            int bytesRead = 0;
            do {
                bytesRead += netStream.Read(buffer, bytesRead, 1);
                if (bytesRead <= 0)
                    return -1;
            } while (bytesRead < bytesToRead);
            Buffer.BlockCopy(buffer, 0, bufferDst, offset, bytesToRead);
            return bytesRead;
        }
    }
}
