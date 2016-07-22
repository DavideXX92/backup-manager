using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace WpfApplication1 {
    class ClientTcp {

        TcpClient client;
        Signin.del1 printcs;
        NetworkStream nwStream;

        public ClientTcp(Signin.del1 pc) {
            client = new TcpClient();
            printcs = pc;

            IPAddress remoteIP = IPAddress.Parse("127.0.0.1");
            int remotePort = int.Parse("80");
            IPEndPoint IP_End = new IPEndPoint(remoteIP, remotePort);

            try {
                printcs("Connecting...\n");
                client.Connect(IP_End);
                if (client.Connected) {
                    printcs("Connected to Server: " + IP_End.ToString() + "\n");
                    nwStream = client.GetStream();
                }
            } catch (Exception x) {
                printcs(x.Message.ToString());
            }
        }

        public int login(string username, string pw, String percorso) {
            string text_to_send;
            string receive;

            if (client.Connected) {
                try {
                    text_to_send = "+LOGIN" + username + "\r\n" + pw + "\r\n";
                    byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(text_to_send);
                    nwStream.Write(bytesToSend, 0, bytesToSend.Length);

                    byte[] bytesToRead = new byte[client.ReceiveBufferSize];
                    int bytesRead = nwStream.Read(bytesToRead, 0, client.ReceiveBufferSize);
                    if (bytesRead <= 0) {
                        client.Close();
                        return -1;
                    }
                    receive = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
                    if (receive.Substring(0, 1) == "+") {
                        if (receive.Substring(0, 2) == "OK") {
                            String tmp = receive.Substring(0, 1);
                            do {
                                if (String.Equals(tmp, "\r") == false) {
                                    percorso += tmp;
                                }
                                tmp = receive.Substring(0, 1);
                            } while (String.Equals(tmp, "\n") == false && percorso.Length < 65000); //NOTA: la seconda condizione è per robustezza
                        }
                    } else if (receive.Substring(0, 1) == "-") {
                        if (receive.Substring(0, 3) == "ERR") {
                            return -2; //username o password errate
                        }
                    }
                } catch (Exception e) {
                    throw e;
                }
            }
            return -1; //connessione col server persa
        }
    }
}
