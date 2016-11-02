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

namespace PDSserver
{    
    class HandlePacketsSecure : HandlePackets
    {
        private Dizionario.InvokeFunc invokeFunc;
        private Socket socketClient;
        private SslStream netStream;
        private const int CODELENGTH = 3;
        private byte[] rBuffer;
        private byte[] wBuffer;
        private int bytesRead;
        private bool isListen;

        public HandlePacketsSecure(Socket socketClient, Dizionario.InvokeFunc invokeFunc)
        {
            this.socketClient = socketClient;
            try
            {
                SslStream sslStream = new SslStream(new NetworkStream(socketClient), false);
                X509Certificate2 serverCertificate = new X509Certificate2(Config.ServerCertificate, "prova", X509KeyStorageFlags.MachineKeySet);
                sslStream.AuthenticateAsServer(serverCertificate, false, SslProtocols.Tls, false);
                this.netStream = sslStream;
            }
            catch (AuthenticationException e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
                if (e.InnerException != null)
                    Console.WriteLine("Inner exception: {0}", e.InnerException.Message);
                Console.WriteLine("Authentication failed - closing the connection.");
                MyConsole.Write("Authentication failed - closing the connection");
                this.netStream.Close();
                this.socketClient.Close();
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine("Certificate Error: ", e.Message);
                MyConsole.Write("Certificate Error:" + e.Message + "closing the connection");
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
                int chunk = 1024;

                wBuffer = new byte[chunk];

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
                int chunk = 1024;

                rBuffer = new byte[chunk];

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

            try
            {
                while (socketClient.Connected && isListen)
                {
                    bytesRead = myReceive(netStream, codeBytes, 0, codeBytes.Length);
                    if (bytesRead <= 0)
                    {
                        isListen = false;
                        //MyConsole.Write("Connection lost with the client...");
                    }
                    else
                    {
                        receive = Encoding.ASCII.GetString(codeBytes, 0, bytesRead);
                        MyConsole.Write("RCV: " + receive);

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
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                socketClient.Close();
                netStream.Close();
            }
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
            int chunk = 1024;
            string json;

            rBuffer = new byte[length];

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
            string json;

            try
            {
                json = JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Serialize, PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects });

                //Console.WriteLine(json);

                codeBytes = ASCIIEncoding.ASCII.GetBytes(code);
                netStream.Write(codeBytes, 0, codeBytes.Length);

                int length = ASCIIEncoding.ASCII.GetBytes(json).Length;
                lengthBytes = BitConverter.GetBytes(length);
                netStream.Write(lengthBytes, 0, lengthBytes.Length);

                wBuffer = new byte[length];
                wBuffer = ASCIIEncoding.ASCII.GetBytes(json);
                netStream.Write(wBuffer, 0, length);

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

        private int myReceive(SslStream netStream, byte[] bufferDst, int offset, int bytesToRead){
            byte[] buffer = new byte[bytesToRead];
            int bytesRead = 0;
            do
            {
                try
                {
                    bytesRead += netStream.Read(buffer, bytesRead, 1);
                }
                catch (IOException e)
                {
                    return -1;
                }  
            } while (bytesRead < bytesToRead);
            Buffer.BlockCopy(buffer, 0, bufferDst, offset, bytesToRead);
            return bytesRead;
        }
    }
}
