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

namespace ClientDiProva
{
    class HandlePackets
    {
        private TcpClient client;
        //private NetworkStream netStream;
        private SslStream netStream;
        private const int CODELENGTH = 3;
        private const int DIMBUF = 65536;
        private byte[] rBuffer = new byte[DIMBUF];
        private byte[] wBuffer = new byte[DIMBUF];
        private int bytesRead;
        private bool isListen;


        public HandlePackets(string serverIP, int port)
        {
            connectToServer(serverIP, port);
        }

        private void connectToServer(string serverIP, int port)
        {
            IPAddress remoteIP = IPAddress.Parse(serverIP);

            this.client = new TcpClient();
            IPEndPoint IP_End = new IPEndPoint(remoteIP, port);
            try
            {
                MyConsole.write("Connecting...");
                client.Connect(IP_End);
                if (client.Connected)
                {
                    this.netStream = new SslStream(client.GetStream(), false, certificateValidationCallback);
                    try
                    {
                        netStream.AuthenticateAsClient("localhost");
                        MyConsole.write("Connected to Server: " + IP_End.ToString() + "\n");
                    }
                    catch (AuthenticationException e)
                    {
                        Console.WriteLine("Exception: {0}", e.Message);
                        if (e.InnerException != null)
                            Console.WriteLine("Inner exception: {0}", e.InnerException.Message);
                        MyConsole.write("Authentication failed - closing the connection");
                        Console.WriteLine("Authentication failed - closing the connection.");
                        this.netStream.Close();
                        throw;
                    }
                }    
            }
            catch (Exception x)
            {
                this.client.Close();
                throw;
            }
        }
        private bool certificateValidationCallback(Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;
            if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors)
                return true; //we don't have a proper certificate tree
            return false;
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
            }catch(Exception e){
                throw e;
            }
        }
        private void receiveFile(WrapFile wrapFile)
        {
            try
            {
                //Receive size
                byte[] lengthBytes = new byte[sizeof(int)];
                int bytesRead = netStream.Read(lengthBytes, 0, lengthBytes.Length);
                int length = BitConverter.ToInt32(lengthBytes, 0);

                int bytesReamining = length;
                int bytesReceived = 0;
                bytesRead = 0;
                int bytesToRead;
                int chunk = DIMBUF;

                while (bytesReamining > 0)
                {
                    if (bytesReamining > chunk)
                        bytesToRead = chunk;
                    else
                        bytesToRead = bytesReamining;

                    bytesRead = netStream.Read(rBuffer, 0, bytesToRead);
                    if (bytesRead < 0)
                        break;
                    wrapFile.fs.Write(rBuffer, 0, bytesRead);
                    bytesReceived += bytesRead;
                    bytesReamining -= bytesRead;
                }
                wrapFile.fs.Close();
            }catch(Exception e){
                throw e;
            }
        }

        /*CLIENT*/
        public Object doRequest(string code, Object objRequest)
        {
            string receive;
            byte[] codeBytes = new byte[CODELENGTH];

            try
            {
                sendPacket(code, objRequest);

                bytesRead = netStream.Read(codeBytes, 0, codeBytes.Length);
                if (bytesRead <= 0)
                {
                    MyConsole.write(Thread.CurrentThread.ManagedThreadId + " Connection lost with the client...");
                    return null;
                }
                else
                {
                    receive = Encoding.ASCII.GetString(codeBytes, 0, bytesRead);
                    MyConsole.write("You: " + receive);
                    Object objSerialized = receiveResponse();
                    Type t = objRequest.GetType();
                    string json = (string)objSerialized;
                    Object objResponse = JsonConvert.DeserializeObject(json, t);

                    if (receive.Equals("001"))
                    {
                        WrapFile wrapFileRes = (WrapFile)objResponse;
                        try{
                            receiveFile((WrapFile)objRequest);
                            wrapFileRes.message = "File ricevuto correttamente";
                        }catch(Exception e){
                            Console.WriteLine("Errore di rete: impossibile ricevere il file");
                            wrapFileRes.error = "Errore durante la ricezione del file";
                        }
                        finally
                        {
                            objResponse = (Object)wrapFileRes;
                        }
                    }

                    return objResponse;
                }
            }catch(Exception e){
                Console.Write(e.Message);
                throw e;
            }

        }
        private void sendPacket(string code, Object obj)
        {
            byte[] codeBytes = new byte[3];
            byte[] lengthBytes = new byte[sizeof(int)];
            byte[] objBuffer = new byte[DIMBUF];
            string json;

            try
            {
                if (obj.GetType() == typeof(WrapFile))
                {
                    WrapFile wrapFile = (WrapFile)obj;
                    json = JsonConvert.SerializeObject(wrapFile.file, Formatting.Indented, new JsonSerializerSettings() { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore }); //mando solo il file non tutto il wrapfile
                }
                else
                    json = JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings(){ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore});

                Console.WriteLine(json);
                objBuffer = ASCIIEncoding.ASCII.GetBytes(json);

                int length = objBuffer.Length;

                codeBytes = ASCIIEncoding.ASCII.GetBytes(code);
                netStream.Write(codeBytes, 0, codeBytes.Length);

                lengthBytes = BitConverter.GetBytes(length);
                netStream.Write(lengthBytes, 0, lengthBytes.Length);

                netStream.Write(objBuffer, 0, objBuffer.Length);

                if (code.Equals("002"))
                {
                    sendFile((WrapFile)obj);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
           
        }
        private Object receiveResponse()
        {
            int length;
            byte[] lengthBytes = new byte[sizeof(int)];
            Object obj;

            try
            {
                bytesRead = netStream.Read(lengthBytes, 0, lengthBytes.Length);
                length = BitConverter.ToInt32(lengthBytes, 0);

                obj = receiveObject(length);
                
                return obj;

            }catch(Exception e){
                throw e;
            }
            
        }
        private Object receiveObject(int length)
        {
            int bytesReamining = length;
            int bytesReceived = 0;
            int bytesRead;
            int bytesToRead;
            int chunk = DIMBUF;
            string json;
                
            while (bytesReamining > 0)
            {
                if (bytesReamining > chunk)
                    bytesToRead = chunk;
                else
                    bytesToRead = bytesReamining;

                bytesRead = netStream.Read(rBuffer, bytesReceived, bytesToRead);
                if (bytesRead < 0)
                    break;
                bytesReceived += bytesRead;
                bytesReamining -= bytesRead;
            }
            if (bytesReceived == length)
            {
                byte[] objBytes = new byte[length];
                Buffer.BlockCopy(rBuffer, 0, objBytes, 0, length);
                //Object obj = ByteArrayToObject(objBytes);
                json = Encoding.ASCII.GetString(objBytes, 0, length);
                Object obj = (Object)json;
                return obj;
            }
            else
                //return null;
                throw new Exception("Impossibile ricevere il file (l'object), problema di rete");        
        }

        // Convert an Object to a byte array - Serialize
        /*private byte[] ObjectToByteArray(Object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }*/

        // Convert a byte array to an Object - Deserialize
        /*private Object ByteArrayToObject(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            binForm.Binder = new MyBinder();
            Object obj = (Object)binForm.Deserialize(memStream);
            return obj;
        }*/

    }
}
