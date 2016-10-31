using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;
using Newtonsoft.Json;

namespace newServerWF
{
    class HandlePacketsUnsecure : HandlePackets
    {
        private Dizionario.InvokeFunc invokeFunc;
        private Socket socketClient;
        private NetworkStream netStream;
        private const int CODELENGTH = 3;
        private const int DIMBUF = 65536;
        private byte[] rBuffer = new byte[DIMBUF];
        private byte[] wBuffer = new byte[DIMBUF];
        private int bytesRead;
        private bool isListen;

        public HandlePacketsUnsecure(Socket socketClient, Dizionario.InvokeFunc invokeFunc)
        {
            this.socketClient = socketClient;
            try
            {
                this.netStream = new NetworkStream(socketClient);
                MyConsole.Write("connessione non sicura creata");
            }
            catch (Exception e)
            {
                MyConsole.Write("Exception: " + e.Message);
                MyConsole.Write("Impossibile creare la connessione non sicura");
                this.netStream.Close();
                this.socketClient.Close();
                throw;
            }
            this.invokeFunc = invokeFunc;
        }
      
        public void startListen()
        {
            isListen = true;
            loopReadMessage();
        }
        public void stopListen()
        {
            isListen = false;
        }

        private void sendFile(WrapFile wrapFile)
        {
            try
            {
                //Send Size
                byte[] lengthBytes = new byte[sizeof(int)];
                lengthBytes = BitConverter.GetBytes(wrapFile.size);
                netStream.Write(lengthBytes, 0, lengthBytes.Length);

                int length = wrapFile.size;
                int bytesReamining = length;
                int bytesSent = 0;
                int bytesToSend;
                int chunk = DIMBUF;

                while (bytesReamining > 0)
                {
                    if (bytesReamining > chunk)
                        bytesToSend = chunk;
                    else
                        bytesToSend = bytesReamining;

                    wrapFile.fs.Read(wBuffer, 0, bytesToSend);
                    netStream.Write(wBuffer, 0, bytesToSend);
                    bytesSent += bytesToSend;
                    bytesReamining -= bytesToSend;
                }
                wrapFile.fs.Close();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private void receiveFile(WrapFile wrapFile)
        {
            try
            {
                //Receive size
                byte[] lengthBytes = new byte[sizeof(int)];
                int bytesRead = myReceive(netStream, lengthBytes, 0, lengthBytes.Length);
                int length = BitConverter.ToInt32(lengthBytes, 0);

                int bytesReamining = length;
                int bytesReceived = 0;
                bytesRead = 0;
                int bytesToRead;
                //int chunk = DIMBUF;
                int chunk = 1024;

                while (bytesReamining > 0)
                {
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
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        /*SERVER*/
        private void loopReadMessage()
        {
            string receive;
            byte[] codeBytes = new byte[CODELENGTH];

            while (socketClient.Connected && isListen)
            {
                bytesRead = myReceive(netStream, codeBytes, 0, codeBytes.Length);
                if (bytesRead <= 0)
                {
                    isListen = false;
                    MyConsole.Write(Thread.CurrentThread.ManagedThreadId + " Connection lost with the client...");
                }
                else
                {
                    receive = Encoding.ASCII.GetString(codeBytes, 0, bytesRead);
                    MyConsole.Write("You: " + receive);
                    try
                    {
                        Object obj = receivePacket();
                        Object res = invokeFunc(receive, obj);

                        if (receive.Equals("002"))
                        {
                            WrapFile wrapFileRes = (WrapFile)res;
                            try
                            {
                                receiveFile((WrapFile)res);
                                res = invokeFunc("003", (WrapFile)res);
                                wrapFileRes.message = "file received by the server";
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Errore di rete: impossibile ricevere il file");
                                wrapFileRes.error = "Errore durante la trasmissione";
                            }
                            finally
                            {
                                res = (Object)wrapFileRes;
                            }
                        }

                        sendResponse(res, receive);

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
            wBuffer = ASCIIEncoding.ASCII.GetBytes("Connection closed by the server");
            netStream.Write(wBuffer, 0, wBuffer.Length);
            socketClient.Close();
            netStream.Close();
            MyConsole.Write("Client " + Thread.CurrentThread.ManagedThreadId + " disconnected");
        }
        private Object receivePacket()
        {
            int length;
            byte[] lengthBytes = new byte[sizeof(int)];
            Object obj;

            bytesRead = myReceive(netStream, lengthBytes, 0, lengthBytes.Length);
            length = BitConverter.ToInt32(lengthBytes, 0);

            if (length != 0)
                obj = receiveObject(length);
            else
                obj = null;

            return obj;
        }
        private Object receiveObject(int length)
        {
            int bytesReamining = length;
            int bytesReceived = 0;
            int bytesRead;
            int bytesToRead;
            //int chunk = DIMBUF;
            int chunk = 1024;
            string json;

            while (bytesReamining > 0)
            {
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
            if (bytesReceived == length)
            {
                byte[] objBytes = new byte[length];
                Buffer.BlockCopy(rBuffer, 0, objBytes, 0, length);
                json = Encoding.ASCII.GetString(objBytes, 0, length);
                Object obj = (Object)json;
                return obj;
            }
            else
                return null;
        }
        private void sendResponse(Object obj, string code)
        {
            byte[] codeBytes = new byte[3];
            byte[] lengthBytes = new byte[sizeof(int)];
            byte[] objBuffer = new byte[DIMBUF];
            string json;

            try
            {
                //json = JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore });
                json = JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Serialize, PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects });

                //Console.WriteLine(json);
                objBuffer = ASCIIEncoding.ASCII.GetBytes(json);

                int length = objBuffer.Length;

                codeBytes = ASCIIEncoding.ASCII.GetBytes(code);
                netStream.Write(codeBytes, 0, codeBytes.Length);

                lengthBytes = BitConverter.GetBytes(length);
                netStream.Write(lengthBytes, 0, lengthBytes.Length);

                netStream.Write(objBuffer, 0, objBuffer.Length);

                if (code.Equals("001"))
                {
                    sendFile((WrapFile)obj);
                }
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        private int myReceive(NetworkStream netStream, byte[] bufferDst, int offset, int bytesToRead)
        {
            byte[] buffer = new byte[bytesToRead];
            int bytesRead = 0;
            do
            {
                bytesRead += netStream.Read(buffer, bytesRead, 1);
                if (bytesRead <= 0)
                    return -1;
            } while (bytesRead < bytesToRead);
            Buffer.BlockCopy(buffer, 0, bufferDst, offset, bytesToRead);
            return bytesRead;
        }
    }
}