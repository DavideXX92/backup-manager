using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace ClientDiProva
{
    class HandleClientImpl : HandleClient
    {
        private HandlePackets clientConn;
        private User clientUser;
        private string monitorDir { get; set; }
        private Watcher watcher;
        private Timer timer;

        public HandleClientImpl(string serverIP, int port)
        {
            this.monitorDir = null;
            this.clientUser = null;
            this.watcher = null;
            try
            {
                this.clientConn = new HandlePackets(serverIP, port);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        
        public void setMonitorDir(string pathDir)
        {
            monitorDir = pathDir;
        }
        public void registerRequest(string username, string password)
        {
            string code = "004";
            User user = new User(username, password);
            Register request = new Register(user);
            Register response = (Register)clientConn.doRequest(code, request);
            if (response.error == null)
            {
                Console.WriteLine(response.message);
            }
            else
            {
                Console.WriteLine(response.error);
            }
        }
        public void loginRequest(string username, string password)
        {
            string code = "005";
            User user = new User(username, password);
            Login request = new Login(user);
            Login response = (Login)clientConn.doRequest(code, request);
            if (response.error == null)
            {
                user = response.user;
                clientUser = user;
                Console.WriteLine(response.message);
                MyConsole.write("Logged");
                MyConsole.write("id: " + user.idUser);
                MyConsole.write("User: " + user.username);
                MyConsole.write("Pass: " + user.password);
                List<Version> versionList = askStoredVersions();
            }
            else
            {
                MyConsole.write(response.error);
            }
        }
        public void logoutRequest()
        {
            string code = "000";
            try
            {
                GenericRequest request = new GenericRequest();
                GenericRequest response = (GenericRequest)clientConn.doRequest(code, request);
                if (response.error == null)
                {
                    Console.WriteLine(response.message);
                    MyConsole.write(response.message);
                }
                else
                {
                    Console.WriteLine(response.error);
                    MyConsole.write(response.error);
                }
            }
            catch (Exception e)
            {
                MyConsole.write("Impossibile eseguire il logout: " + e.Message);
            }
        }
        public Version createNewVersion()
        {
            MyConsole.write("Richiesta di creazione nuova versione");
            string code = "006";
            DirTreeService dirTreeService = new DirTreeServiceImpl();
            Dir dirTree = dirTreeService.makeDirTree(new Dir(monitorDir, null));
            CreateVersion request = new CreateVersion(new Version(dirTree));
            try
            {
                CreateVersion response = (CreateVersion)clientConn.doRequest(code, request);
                if(response.error!=null)
                    throw new Exception(response.error);
                List<String> elencoHash = response.elencoHash;
                if (elencoHash.Count > 0)
                {
                    Dictionary<string, File> fileMap = dirTreeService.getAllFileIntoAmap(dirTree);
                    sendRequestedFile(fileMap, elencoHash);
                }
                return response.version;
            }
            catch (Exception e)
            {
                MyConsole.write(e.Message);
                throw;
            }
        }
        public void updateVersion(MyBuffer bufferOperation)
        {
            MyConsole.write("Richiesta di aggiornamento della versione");
            string code = "007";
            try
            {
                UpdateVersion response = (UpdateVersion)clientConn.doRequest(code, new UpdateVersion(bufferOperation.list));
                if (response.error != null)
                    throw new Exception(response.error);
                List<String> elencoHash = response.elencoHash;
                if (elencoHash.Count > 0)
                {
                    Dictionary<string, File> fileMap = bufferOperation.getAllFileIntoAmap();
                    sendRequestedFile(fileMap, elencoHash);
                }
                MyConsole.write("La versione e' stata aggiornata");
                watcher.buffer.list.Clear();
            }
            catch (Exception e)
            {
                MyConsole.write(e.Message);
            }
        }
        public void closeVersion(Version version)
        {
            MyConsole.write("Richiesta di chiusura della versione");
            string code = "009";
            try
            {
                CloseVersion response = (CloseVersion)clientConn.doRequest(code, new CloseVersion(version));
                if (response.error != null)
                    throw new Exception(response.error);
            }
            catch (Exception e)
            {
                MyConsole.write(e.Message);
                throw;
            }
        }
        public Version restoreVersion(int idVersion)
        {
            MyConsole.write("Richiesta di ripristino della cartella");
            string code = "010";
            RestoreVersion request = new RestoreVersion(new Version(idVersion));
            try
            {
                RestoreVersion response = (RestoreVersion)clientConn.doRequest(code, request);
                Dir dirTree = response.version.dirTree;
                List<File> elencoFile = response.elencoFile;
                DirTreeService dirTreeService = new DirTreeServiceImpl();
                dirTreeService.createDirTree(dirTree, @"c:\tmp");
                requireMissingFile(elencoFile);
                MyConsole.write("La version è stata ripristinata");
                return response.version;
            }
            catch (Exception e)
            {
                MyConsole.write("Problema durante il restore della cartella: " + e.Message);
                throw;
            }
        }
        public void synchronize()
        {
            try
            {
                List<Version> storedVersion = askStoredVersions();
                if (storedVersion.Count == 0)
                {
                    MyConsole.write("Sul server non è presenta nessuna versione, ne creo una nuova");
                    Version version = createNewVersion(); //versione backup completo
                    closeVersion(version);                //voglio almeno una versione completa sul server
                    version = createNewVersion();         //versione corrente su cui lavorare
                }
                watcher = new Watcher(monitorDir);
                watcher.set();
                Console.WriteLine("Watcher set");
                timer = new Timer(10000);
                timer.Elapsed += OnTimedEvent;
                timer.AutoReset = true;
                timer.Enabled = true;
                Console.WriteLine("Timer set");
            }
            catch (Exception e)
            {
                MyConsole.write("Impossibile effettuare la sincronizzazione: " + e.Message);
            }
        }

        public void test()
        {
            CheckFile request = new CheckFile("addFile");
            //request.oldPath = monitorDir.Substring(monitorDir.LastIndexOf(@"\")) + @"\documents\lavoro\mia\tua\sua";
            //request.newPath = monitorDir.Substring(monitorDir.LastIndexOf(@"\")) + @"\documents\lavoro\mia\tua\nostra";
            request.file = new File(monitorDir + @"\music\mp3\dj.txt", monitorDir);
            //request.path = monitorDir.Substring(monitorDir.LastIndexOf(@"\")) + @"\documents\lavoro";
            try
            {
                CheckFile response = (CheckFile)clientConn.doRequest("013", request);
                MyConsole.write(response.message);
            }
            catch (Exception e)
            {
                MyConsole.write("Problema durante il test: " + e.Message);
            }
        }

        private List<Version> askStoredVersions()
        {
            MyConsole.write("Chiedo al server le versioni salvate");
            string code = "011";
            StoredVersions request = new StoredVersions();
            try
            {
                StoredVersions response = (StoredVersions)clientConn.doRequest(code, request);
                if (response.error != null)
                    throw new Exception(response.error);
                return response.storedVersions;
            }
            catch (Exception e)
            {
                MyConsole.write("Problema nella rete: " + e.Message);
                throw;
            }

        }
        private void requireMissingFile(List<File> elencoFile)
        {
            MyConsole.write(elencoFile.Count + " file devono essere trasferiti");
            int i = 1;
            foreach (File file in elencoFile)
            {
                MyConsole.write("trasferimento file " + (i++) + "/" + elencoFile.Count);
                requestAfile(file);
            }
            MyConsole.write("Tutti i file sono stati ricevuti correttamente");
        }
        private void requestAfile(File file)
        {
            String path = @"c:\tmp\" + file.path;
            try
            {
                WrapFile wrapFile = new WrapFile(file, -1, new FileStream(path, FileMode.Create, FileAccess.Write));
                wrapFile = (WrapFile)clientConn.doRequest("001", wrapFile);
                if (wrapFile.error == null)
                    MyConsole.write(wrapFile.message + " and received by the client");
                else
                    MyConsole.write(wrapFile.error);
            }
            catch (Exception e)
            {
                MyConsole.write(e.Message);
            }
        }
        private void sendRequestedFile(Dictionary<string, File> fileMap, List<String> elencoHash)
        {
            Console.WriteLine("File da inviare: " + elencoHash.Count);
            int i = 1;
            foreach (String hash in elencoHash)
            {
                Console.WriteLine("invio file: " + (i++) + "/" + elencoHash.Count);
                try
                {
                    if (fileMap.ContainsKey(hash))
                        sendAfile(fileMap[hash]);
                    else
                    {
                        Console.WriteLine("Il file: " + hash + "non e' più presente sul pc");
                        //mandare una request per decrementare il countere dell'hash
                    }     
                }catch(Exception e)
                {
                    Console.WriteLine("Impossibile inviare il file " + e.Message);
                }     
            }
            Console.WriteLine("Tutti i file sono stati inviati correttamente");
        }
        private void sendAfile(File file)
        {
            string pathSrc = file.absolutePath;
            WrapFile wrapFile;
            try
            {
                wrapFile = new WrapFile(file, file.size, new FileStream(pathSrc, FileMode.Open, FileAccess.Read));
                try
                {
                    wrapFile = (WrapFile)clientConn.doRequest("002", wrapFile);
                    if (wrapFile.error == null)
                        MyConsole.write(wrapFile.message);
                    else
                    {
                        MyConsole.write(wrapFile.error);
                        throw new Exception(wrapFile.error);
                    }
                }
                catch (Exception e)
                {
                    MyConsole.write(e.Message);
                }
            }catch(Exception e)
            {
                Console.WriteLine("impossibile creare il wrap file.. forse il file non esiste?");
            }
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            Console.WriteLine("The Elapsed event was raised at {0:HH:mm:ss.fff}", e.SignalTime);
            //printOperation(watcher.buffer.list);
            if( watcher.buffer.list.Count>0 )
                updateVersion(watcher.buffer);
        }
        private void printOperation(List<Operation> list)
        {
            foreach (Operation operation in list)
            {
                Console.WriteLine(operation.type);
                if (operation.dir != null) Console.WriteLine("DIR PATH: " + operation.dir.path);
                if (operation.file != null) Console.WriteLine("FILE PATH: " + operation.file.path);
                Console.WriteLine("PATH: " + operation.path);
            }
        }
        
    }
}
