using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientDiProva
{
    class NetworkServiceImpl : NetworkService
    {
        private HandlePackets clientConn;
        public NetworkServiceImpl(string serverIP, int port)
        {
            try
            {
                this.clientConn = new HandlePackets(serverIP, port);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public void sendHelloMessage()
        {
            Console.WriteLine("hello message");
            string code = "017";
            GenericRequest request = new GenericRequest();
            request.message = "hello message";
            try
            {
                GenericRequest response = (GenericRequest)clientConn.doRequest(code, request);
                Console.WriteLine(response.message);
            }
            catch (Exception e)
            {
                throw new NetworkException(e.Message);
            }
        }
        public void setMonitorDir(string pathDir)
        {
            MyConsole.write("Richiesta di settaggio della monitorDir");
            string code = "012";
            MonitorDir request = new MonitorDir(pathDir);
            try
            {
                MonitorDir response = (MonitorDir)clientConn.doRequest(code, request);
                if (response.error != null)
                    throw new ServerException(response.error);
            }
            catch (Exception e)
            {   
                throw new NetworkException(e.Message);
            }
        }
        public string getMonitorDir()
        {
            MyConsole.write("Richiesta del path della monitorDir");
            string code = "015";
            MonitorDir request = new MonitorDir();
            try
            {
                MonitorDir response = (MonitorDir)clientConn.doRequest(code, request);
                return response.monitorDir;
            }
            catch (Exception e)
            {
                throw new NetworkException(e.Message);
            }
        }
        public void registerRequest(string username, string password)
        {
            MyConsole.write("Richiesta di registrazione");
            string code = "004";
            User user = new User(username, password);
            Register request = new Register(user);
            try
            {
                Register response = (Register)clientConn.doRequest(code, request);
                if (response.error != null)
                    throw new ServerException(response.error);
            }
            catch (Exception e)
            {
                throw new NetworkException(e.Message);
            }  
        }
        public User loginRequest(string username, string password)
        {
            MyConsole.write("Richiesta di login");
            string code = "005";
            User user = new User(username, password);
            Login request = new Login(user);
            try
            {
                Login response = (Login)clientConn.doRequest(code, request);
                if (response.error != null)
                    throw new ServerException(response.error);
                return response.user;
            }
            catch (Exception e)
            {
                throw new NetworkException(e.Message);
            }
        }
        public void logoutRequest()
        {
            MyConsole.write("Richiesta di logout");
            string code = "000";
            GenericRequest request = new GenericRequest();
            try
            {
                GenericRequest response = (GenericRequest)clientConn.doRequest(code, request);
                if (response.error != null)
                    throw new ServerException(response.error);
            }
            catch (Exception e)
            {
                throw new NetworkException(e.Message);
            }
        }
        public Version createNewVersion(Dir dirTree)
        {
            MyConsole.write("Richiesta di creazione nuova versione");
            string code = "006";
            DirTreeService dirTreeService = new DirTreeServiceImpl();
            CreateVersion request = new CreateVersion(new Version(dirTree));
            try
            {
                CreateVersion response = (CreateVersion)clientConn.doRequest(code, request);
                if (response.error != null)
                    throw new ServerException(response.error);
                return response.version;
            }
            catch (Exception e)
            {
                throw new NetworkException(e.Message);
            }
        }
        public void updateVersion(MyBuffer bufferOperation)
        {
            MyConsole.write("Richiesta di aggiornamento della versione");
            string code = "007";
            UpdateVersion request = new UpdateVersion(bufferOperation.list);
            try
            {
                UpdateVersion response = (UpdateVersion)clientConn.doRequest(code, request);
                if (response.error != null)
                    throw new ServerException(response.error);
            }
            catch (Exception e)
            {
                throw new NetworkException(e.Message);
            }
        }
        public void closeVersion(int idVersion)
        {
            MyConsole.write("Richiesta di chiusura della versione");
            string code = "009";
            CloseVersion request = new CloseVersion(idVersion);
            try
            {
                CloseVersion response = (CloseVersion)clientConn.doRequest(code, request);
                if (response.error != null)
                    throw new ServerException(response.error);
            }
            catch (Exception e)
            {
                throw new NetworkException(e.Message);
            }
        }
        public Version getVersion(int idVersion)
        {
            MyConsole.write("Richiesta della versione con id=" + idVersion);
            string code = "010";
            GetVersion request = new GetVersion(new Version(idVersion));
            try
            {
                GetVersion response = (GetVersion)clientConn.doRequest(code, request);
                if (response.error != null)
                    throw new ServerException(response.error);
                return response.version;
            }
            catch (Exception e)
            {
                throw new NetworkException(e.Message);
            }
        }
        public Version getOpenVersion()
        {
            MyConsole.write("Richiesta della version aperta");
            string code = "016";
            GetVersion request = new GetVersion(new Version());
            try
            {
                GetVersion response = (GetVersion)clientConn.doRequest(code, request);
                if (response.error != null)
                    throw new ServerException(response.error);
                return response.version;
            }
            catch (Exception e)
            {
                throw new NetworkException(e.Message);
            }
        }
        public List<int> askIDofAllVersions()
        {
            MyConsole.write("Chiedo al server tutti gli ID delle versioni salvate");
            string code = "011";
            StoredVersions request = new StoredVersions();
            try
            {
                StoredVersions response = (StoredVersions)clientConn.doRequest(code, request);
                if (response.error != null)
                    throw new ServerException(response.error);
                return response.elencoID;
            }
            catch (Exception e)
            {
                throw new NetworkException(e.Message);
            }
        }
        public List<string> askHashToSend()
        {
            MyConsole.write("Chiedo al server i file che non sono ancora stati ricevuti");
            string code = "014";
            HashRequest request = new HashRequest();
            try
            {
                HashRequest response = (HashRequest)clientConn.doRequest(code, request);
                if (response.error != null)
                    throw new ServerException(response.error);
                return response.elencoHash;
            }
            catch (Exception e)
            {
                throw new NetworkException(e.Message);
            }
        }

        public void requestAfile(File file, string pathDst)
        {
            //String path = @"c:\tmp\" + file.path;
            pathDst += file.path;
            MyConsole.write("Richiesta di un file al server");
            String code = "001";
            WrapFile wrapFile = new WrapFile(file, -1, new FileStream(pathDst, FileMode.Create, FileAccess.Write));
            try
            {
                wrapFile = (WrapFile)clientConn.doRequest(code, wrapFile);
                if (wrapFile.error != null)
                    throw new ServerException(wrapFile.error);
            }
            catch (Exception e)
            {
                throw new NetworkException(e.Message);
            }
        }
        public void sendAfile(File file)
        {
            MyConsole.write("Invio di un file al server");
            string code = "002";
            string pathSrc = file.absolutePath;
            WrapFile wrapFile = new WrapFile(file, file.size, new FileStream(pathSrc, FileMode.Open, FileAccess.Read));
            try
            {
                wrapFile = (WrapFile)clientConn.doRequest(code, wrapFile);
                if (wrapFile.error != null)
                    throw new ServerException(wrapFile.error);
            }
            catch (Exception e)
            {
                throw new NetworkException(e.Message);
            }
        }
        
    }
}
