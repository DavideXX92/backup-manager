using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDSclient
{
    class NetworkServiceImpl : NetworkService
    {
        private HandlePackets clientConn, controlConn;
        public NetworkServiceImpl(string serverIP, int serverPort, int keepalivePort)
        {
            try
            {
                this.clientConn = new HandlePacketsSecure(serverIP, serverPort);
                this.controlConn = new HandlePacketsUnsecure(serverIP, keepalivePort);
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
                GenericRequest response = (GenericRequest)controlConn.doRequest(code, request);
                Console.WriteLine(response.message);
            }
            catch (Exception e)
            {
                throw new NetworkException(e.Message);
            }
        }
        public void addMonitorDir(string path)
        {
            Console.WriteLine("Richiesta di aggiunta della monitorDir");
            string code = "018";
            MonitorDir request = new MonitorDir(path);
            MonitorDir response = null;
            try
            {
                response = (MonitorDir)clientConn.doRequest(code, request);
            }
            catch (Exception e)
            {
                if (e is BusyResourceException)
                    throw new BusyResourceException(e.Message);
                else
                    throw new NetworkException(e.Message);
            }
            if (response.error != null)
                throw new ServerException(response.error);
        }
        public List<string> getMonitorDir()
        {
            Console.WriteLine("Richiesta del path della monitorDir");
            string code = "015";
            MonitorDir request = new MonitorDir();
            try
            {
                MonitorDir response = (MonitorDir)clientConn.doRequest(code, request);
                return response.monitorDir;
            }
            catch (Exception e)
            {
                if (e is BusyResourceException)
                    throw new BusyResourceException(e.Message);
                else
                    throw new NetworkException(e.Message);
            }
        }
        public void registerRequest(string username, string password)
        {
            Console.WriteLine("Richiesta di registrazione");
            string code = "004";
            User user = new User(username, password);
            Register request = new Register(user);
            Register response = null;
            try
            {
                response = (Register)clientConn.doRequest(code, request);
            }
            catch (Exception e)
            {
                if (e is BusyResourceException)
                    throw new BusyResourceException(e.Message);
                else
                    throw new NetworkException(e.Message);
            }
            if (response.error != null)
                throw new ServerException(response.error);
        }
        public User loginRequest(string username, string password)
        {
            Console.WriteLine("Richiesta di login");
            string code = "005";
            User user = new User(username, password);
            Login request = new Login(user);
            Login response = null;
            try{
                response = (Login)clientConn.doRequest(code, request);
            }catch(Exception e){
                if (e is BusyResourceException)
                    throw new BusyResourceException(e.Message);
                else
                    throw new NetworkException(e.Message);
            }
            if (response.error != null)
                throw new ServerException(response.error); 
            return response.user;                        
        }
        public void logoutRequest()
        {
            Console.WriteLine("Richiesta di logout");
            string code = "000";
            GenericRequest request = new GenericRequest();
            GenericRequest response;
            try
            {
                response = (GenericRequest)clientConn.doRequest(code, request);
            }
            catch (Exception e)
            {
                if (e is BusyResourceException)
                    throw new BusyResourceException(e.Message);
                else
                    throw new NetworkException(e.Message);
            }
            if (response.error != null)
                throw new ServerException(response.error);
        }
        public Version createNewVersion(Dir dirTree)
        {
            Console.WriteLine("Richiesta di creazione nuova versione");
            string code = "006";
            DirTreeService dirTreeService = new DirTreeServiceImpl();
            CreateVersion request = new CreateVersion(new Version(dirTree));
            CreateVersion response;
            try
            {
                response = (CreateVersion)clientConn.doRequest(code, request);
            }
            catch (Exception e)
            {
                if (e is BusyResourceException)
                    throw new BusyResourceException(e.Message);
                else
                    throw new NetworkException(e.Message);
            }
            if (response.error != null)
                throw new ServerException(response.error);
            return response.version;
        }
        public void updateVersion(MyBuffer bufferOperation)
        {
            Console.WriteLine("Richiesta di aggiornamento della versione");
            string code = "007";
            UpdateVersion request = new UpdateVersion(bufferOperation.list);
            UpdateVersion response;
            try
            {
                response = (UpdateVersion)clientConn.doRequest(code, request);
            }
            catch (Exception e)
            {
                if (e is BusyResourceException)
                    throw new BusyResourceException(e.Message);
                else
                    throw new NetworkException(e.Message);
            }
            if (response.error != null)
                throw new ServerException(response.error);
        }
        public void closeVersion(int idVersion)
        {
            Console.WriteLine("Richiesta di chiusura della versione");
            string code = "009";
            CloseVersion request = new CloseVersion(idVersion);
            CloseVersion response;
            try
            {
                response = (CloseVersion)clientConn.doRequest(code, request);
            }
            catch (Exception e)
            {
                if (e is BusyResourceException)
                    throw new BusyResourceException(e.Message);
                else
                    throw new NetworkException(e.Message);
            }
            if (response.error != null)
                throw new ServerException(response.error);
        }
        public Version getVersion(int idVersion)
        {
            Console.WriteLine("Richiesta della versione con id=" + idVersion);
            string code = "010";
            GetVersion request = new GetVersion(new Version(idVersion));
            GetVersion response;
            try
            {
                response = (GetVersion)clientConn.doRequest(code, request);
            }
            catch (Exception e)
            {
                if (e is BusyResourceException)
                    throw new BusyResourceException(e.Message);
                else
                    throw new NetworkException(e.Message);
            }
            if (response.error != null)
                throw new ServerException(response.error);
            return response.version;
        }
        public Version getOpenVersion()
        {
            Console.WriteLine("Richiesta della version aperta");
            string code = "016";
            GetVersion request = new GetVersion(new Version());
            GetVersion response;
            try
            {
                response = (GetVersion)clientConn.doRequest(code, request);
            }
            catch (Exception e)
            {
                if (e is BusyResourceException)
                    throw new BusyResourceException(e.Message);
                else
                    throw new NetworkException(e.Message);
            }
            if (response.error != null)
                throw new ServerException(response.error);
            return response.version;
        }
        public Version getLastVersion()
        {
            Console.WriteLine("Richiesta dell'ultima versione chiusa");
            string code = "020";
            GetVersion request = new GetVersion(new Version());
            GetVersion response;
            try
            {
                response = (GetVersion)clientConn.doRequest(code, request);
            }
            catch (Exception e)
            {
                if (e is BusyResourceException)
                    throw new BusyResourceException(e.Message);
                else
                    throw new NetworkException(e.Message);
            }
            if (response.error != null)
                throw new ServerException(response.error);
            return response.version;
        }
        public void deleteUserRepository()
        {
            Console.WriteLine("Richiesta di cancellazione del repository per l'utente");
            string code = "019";
            GenericRequest request = new GenericRequest();
            GenericRequest response;
            try
            {
                response = (GenericRequest)clientConn.doRequest(code, request);
            }
            catch (Exception e)
            {
                if (e is BusyResourceException)
                    throw new BusyResourceException(e.Message);
                else
                    throw new NetworkException(e.Message);
            }
            if (response.error != null)
                throw new ServerException(response.error);
        }
        public List<int> askIDofAllVersions()
        {
            Console.WriteLine("Chiedo al server tutti gli ID delle versioni salvate");
            string code = "011";
            StoredVersions request = new StoredVersions();
            StoredVersions response;
            try
            {
                response = (StoredVersions)clientConn.doRequest(code, request);
            }
            catch (Exception e)
            {
                if (e is BusyResourceException)
                    throw new BusyResourceException(e.Message);
                else
                    throw new NetworkException(e.Message);
            }
            if (response.error != null)
                throw new ServerException(response.error);
            return response.elencoID;
        }
        public List<string> askHashToSend()
        {
            Console.WriteLine("Chiedo al server i file che non sono ancora stati ricevuti");
            string code = "014";
            HashRequest request = new HashRequest();
            HashRequest response;
            try
            {
                response = (HashRequest)clientConn.doRequest(code, request);
            }
            catch (Exception e)
            {
                if (e is BusyResourceException)
                    throw new BusyResourceException(e.Message);
                else
                    throw new NetworkException(e.Message);
            }
            if (response.error != null)
                throw new ServerException(response.error);
            return response.elencoHash;
        }
        
        public void requestAfile(File file, string pathDst)
        {
            Console.WriteLine("Richiesta di un file al server");
            String code = "001";
            pathDst += file.path;
            WrapFile wrapFile = new WrapFile(file, -1, new FileStream(pathDst, FileMode.Create, FileAccess.Write));
            try
            {
                wrapFile = (WrapFile)clientConn.doRequest(code, wrapFile);
            }
            catch (Exception e)
            {
                if (e is BusyResourceException)
                    throw new BusyResourceException(e.Message);
                else
                    throw new NetworkException(e.Message);
            }
            if (wrapFile.error != null)
                throw new ServerException(wrapFile.error);
        }
        public void sendAfile(File file)
        {
            Console.WriteLine("Invio di un file al server");
            string code = "002";
            string pathSrc = file.absolutePath;
            WrapFile wrapFile = new WrapFile(file, file.size, new FileStream(pathSrc, FileMode.Open, FileAccess.Read));
            try
            {
                wrapFile = (WrapFile)clientConn.doRequest(code, wrapFile);
            }
            catch (Exception e)
            {
                if (e is BusyResourceException)
                    throw new BusyResourceException(e.Message);
                else
                    throw new NetworkException(e.Message);
            }
            if (wrapFile.error != null)
                throw new ServerException(wrapFile.error);
        }
        
    }
}
