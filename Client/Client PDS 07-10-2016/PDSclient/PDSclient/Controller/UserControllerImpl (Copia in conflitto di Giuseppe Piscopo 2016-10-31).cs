using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Controls;

namespace PDSclient
{
    class UserControllerImpl : UserController
    {
        private NetworkService networkService;
        private DirTreeService dirTreeService;
        private Watcher watcher;
        private Timer timer;
        private Timer timerHello;
        private const int nHelloMissAllowed = 3;
        private int retry;
        private bool imUpdating = false;        
        private BackgroundWorker worker;

        public UserControllerImpl()
        {
            try
            {
                this.networkService = new NetworkServiceImpl("127.0.0.1", 8001);
            }catch(Exception e)
            {
                throw e;
            }
            this.dirTreeService = new DirTreeServiceImpl();
            this.watcher = null;
            this.timer = null;
            this.timerHello = null;
            this.retry = 0;           
            this.worker = null;
            //setHelloMessage(5);
        }

        /*public void setHelloMessage(int second)
        {
            if (timerHello == null)
            {
                timerHello = new Timer(second*1000);
                timerHello.Elapsed += TimerHelloEvent;
                timerHello.AutoReset = true;
                timerHello.Enabled = true;
                Console.WriteLine("TimerHello set");
            }
        }*/
        public void sendHelloMessage()
        {
            try
            {
                networkService.sendHelloMessage();
            }
            catch (NetworkException e)
            {
                throw e;
            }
        }
        public void addMonitorDir(string path)
        {
            try
            {
                networkService.addMonitorDir(path);
            }
            catch (NetworkException e)
            {
                throw e;
            }
            catch (ServerException e)
            {
                throw e;
            }
        }
        public List<string> getMonitorDir()
        {
            try
            {
                return networkService.getMonitorDir();
            }
            catch (NetworkException e)
            {
                string error = "Problema di rete: impossibile recuperare la cartella da monitorare";
                Console.WriteLine(error + "\nException: " + e.Message);
                throw e;
            }
        }
        public void deleteUserRepository()
        {
            try
            {
                networkService.deleteUserRepository();
            }
            catch (NetworkException e)
            {
                string error = "Problema di rete: impossibile cancellare il repository dell'utente";
                Console.WriteLine(error + "\nException: " + e.Message);
                throw e;
            }
            catch (ServerException e)
            {
                throw e;
            }
        }
        public void register(string username, string password)
        {
            try
            {
                var sha1 = new SHA1CryptoServiceProvider();
                var passEncrypted = sha1.ComputeHash(Encoding.ASCII.GetBytes(password));
                networkService.registerRequest(username, password);
            }
            catch (NetworkException e)
            {
                throw e;
            }
            catch (ServerException e)
            {
                throw e;
            }
        }
        public User login(string username, string password)
        {
            try
            {
                var sha1 = new SHA1CryptoServiceProvider();
                vaR passEncrypted = sha1.ComputeHash(Encoding.ASCII.GetBytes(password));
                return networkService.loginRequest(username, passEncrypted);
            }
            catch (NetworkException e)
            {
                throw e;
            }
            catch (ServerException e)
            {
                throw e;
            }
        }
        public void logout()
        {
            try
            {
                networkService.logoutRequest();
                if (watcher != null) watcher = null;
                if (timer != null) timer = null;
            }
            catch (NetworkException e)
            {
                throw e;
            }
            catch (ServerException e)
            {
                throw e;
            }
        }
        public Version createNewVersion(string monitorDir)
        {
            try
            {
                Dir dirTree = dirTreeService.makeDirTree(new Dir(monitorDir, null));
                Version newVersion = networkService.createNewVersion(dirTree);

                syncFile(newVersion.dirTree);

                return newVersion;

            }
            catch (NetworkException e)
            {
                throw e;
            }
            catch (ServerException e)
            {
                throw e;
            }
        }
        public void restoreVersion(int idVersion, string monitorDir)
        {
            try
            {
                Version version = networkService.getVersion(idVersion);
                string pathDst = Directory.GetParent(monitorDir).FullName + @"\restoreTmp";
                dirTreeService.createDirTree(version.dirTree, pathDst);
                requireMissingFile(dirTreeService.getAllFileIntoAlist(version.dirTree), pathDst+@"\");
                Directory.Move(monitorDir, monitorDir + "OLD");
                string dirRestored = pathDst + @"\" + version.dirTree.name;
                Directory.Move(dirRestored, Directory.GetParent(monitorDir).FullName + @"\" + version.dirTree.name);
                Directory.Delete(pathDst);
                watcher.bufferRead.clearList();
                watcher.bufferWrite.clearList();
                if (watcher != null) watcher = null;
                if (timer != null) timer = null;
            }
            catch (NetworkException e)
            {
                throw e;
            }
            catch (ServerException e)
            {
                throw e;
            }
        }
        public string restoreDir(string pathDst)
        {
            try
            {
                Version version = networkService.getLastVersion();
                dirTreeService.createDirTree(version.dirTree, pathDst);
                requireMissingFile(dirTreeService.getAllFileIntoAlist(version.dirTree), pathDst + @"\");
                string path = pathDst + @"\" + version.dirTree.name;
                return path;
            }
            catch (NetworkException e)
            {
                throw e;
            }
        }
        public List<Version> askStoredVersions()
        {
            try
            {
                List<Version> elencoVersions = new List<Version>();
                List<int> elencoID = networkService.askIDofAllVersions();
                if (elencoID.Count > 0)
                { 
                    foreach (int idVersion in elencoID)
                        elencoVersions.Add(networkService.getVersion(idVersion));
                }
                return elencoVersions;
            }
            catch (NetworkException e)
            {
                throw e;
            }
            catch (ServerException e)
            {
                throw e;
            }
        }
        public void synchronize(string monitorDir)
        {
            try
            {
                List<Version> storedVersion = askStoredVersions();
                if (storedVersion.Count == 0)
                {
                    Console.WriteLine("Sul server non è presenta nessuna versione, ne creo una nuova");
                    Version version;                          //versione backup completo
                    version = createNewVersion(monitorDir);   //voglio almeno una versione completa sul server 
                    worker = null;
                    version = createNewVersion(monitorDir);   //versione corrente su cui lavorare
                }
            }
            catch (NetworkException e)
            {
                throw e;
            }
            catch (ServerException e)
            {
                throw e;
            }
            
        }
        public void watcherInit(string monitorDir){
            if(watcher==null)
            {
                watcher = new Watcher(monitorDir);
                watcher.set();
                Console.WriteLine("Watcher set");   
            }  
        }
        public void timerInit()
        {
            if(timer==null)
            {
                timer = new Timer(10000);
                timer.Elapsed += OnTimedEvent;
                timer.AutoReset = true;
            }
        }
        public void enableAutoSync()
        {
            timer.Enabled = true;
            Console.WriteLine("Timer set");
        }
        public void disableAutoSync()
        {
            timer.Enabled = false;
            Console.WriteLine("Timer stop");
        }
        public int manualSync()
        {
            try
            {
                return updateVersionWrapper();
            }
            catch (NetworkException e)
            {
                throw e;
            }
            catch (ServerException e)
            {
                throw e;
            }
        }
        public bool checkIfCurrentVersionIsUpdated(string monitorDir)
        {
            try
            {
                Version openVersion = networkService.getOpenVersion();
                if (openVersion == null)
                    throw new Exception("Sul server non e' presente nessuna versione aperta");
                Dir dirTreeServer = openVersion.dirTree;

                Dir dirTreeClient = dirTreeService.makeDirTree(new Dir(monitorDir, null, monitorDir));

                if (dirTreeService.areEqual(dirTreeServer, dirTreeClient))
                {
                    Console.WriteLine("Le due versioni sono uguali");
                    return true;
                }
                else
                {
                    Console.WriteLine("Le due versioni sono diverse");
                    return false;
                }   
            }
            catch (Exception e)
            {
                throw e;
            }   
        }
        public bool checkIfthereAreFileToSend()
        {
            try
            {
                List<string> hashToSend = networkService.askHashToSend();
                if (hashToSend.Count > 0)
                    return true;
                else
                    return false;
            }
            catch (NetworkException e)
            {
                throw e;
            }
            catch (ServerException e)
            {
                throw e;
            }
        }
        public void uploadFile(string monitorDir)
        {
            Dir dirTree = new Dir(monitorDir, null);
            dirTreeService.makeDirTree(dirTree);
            syncFile(dirTree);
        }

        private void syncFile(Dir dirTree)
        {
            List<string> hashToSend = networkService.askHashToSend();
            
            if (hashToSend.Count > 0)
            {
                Console.WriteLine(hashToSend.Count + " file devono essere trasferiti al server");
                Dictionary<string, File> fileMap = dirTreeService.getAllFileIntoAmap(dirTree);
                sendRequestedFile(fileMap, hashToSend);
                Console.WriteLine("File inviati");
            } else {
                Console.WriteLine("Nessun file da inviare");
            }
        }
        private void sendRequestedFile(Dictionary<string, File> fileMap, List<String> elencoHash)
        {
            int i = 1; 
            foreach (String hash in elencoHash)
            {
                Console.WriteLine("invio file: " + (i) + "/" + elencoHash.Count);
                try
                {
                    if (fileMap.ContainsKey(hash))
                        networkService.sendAfile(fileMap[hash]);
                    else
                    {
                        Console.WriteLine("Il file: " + hash + " non e' più presente sul pc");
                        //mandare una request per decrementare il counter dell'hash
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Impossibile inviare il file " + (i) + "/" + elencoHash.Count + " " + e.Message);
                }
                if (worker != null) {
                    worker.ReportProgress(i * 100 / elencoHash.Count);
                    //System.Threading.Thread.Sleep(1000);
                }
                i++;
            }
        }
        private void requireMissingFile(List<File> elencoFile, string rootPath)
        {
            Console.WriteLine(elencoFile.Count + " file devono essere trasferiti");
            int i = 1;
            foreach (File file in elencoFile)
            {
                Console.WriteLine("trasferimento file " + (i++) + "/" + elencoFile.Count);
                try
                {
                    networkService.requestAfile(file, rootPath);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Errore durante il ripristino del file " + (i++) + "/" + elencoFile.Count);
                    throw e;
                }                
            }
            Console.WriteLine("Tutti i file sono stati ricevuti correttamente");
        }
        private void updateVersion(MyBuffer bufferOperation)
        {
            imUpdating = true;
            try
            {
                networkService.updateVersion(bufferOperation);
            }
            catch (Exception e)
            {
                imUpdating = false;
                throw e;
            }
            
            List<string> hashToSend = networkService.askHashToSend();
            if (hashToSend.Count > 0)
            {
                Console.WriteLine(hashToSend.Count + " file devono essere trasferiti al server");
                Dictionary<string, File> fileMap = bufferOperation.getAllFileIntoAmap();
                sendRequestedFile(fileMap, hashToSend);
                Console.WriteLine("File inviati");
            }
            watcher.bufferWrite.clearList();
            imUpdating = false;
            Console.WriteLine("La versione e' stata aggiornata");
        }
        private int updateVersionWrapper()
        {
            int res = 0;
            try
            {
                if (!imUpdating)
                {
                    if (watcher.bufferWrite.list.Count > 0) //se l'update precedente ha avuto dei problemi
                    {
                        updateVersion(watcher.bufferWrite); //provo a fare un altro updateVersion solo con quei file
                        res = 1;
                    }
                    else                                    //invece se tutti i file precedenti sono stati mandati
                    {
                        if (watcher.bufferRead.list.Count > 0) //controllo se ci sono nuovi file da inviare
                        {
                            watcher.bufferWrite.list = watcher.bufferRead.clearList();
                            updateVersion(watcher.bufferWrite);
                            res = 1;
                        }
                    }
                }
                else
                    res = -1;
                return res;
            }
            catch (NetworkException e)
            {
                throw e;
            }
            catch (ServerException e)
            {
                throw e;
            }
        }
        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            Console.WriteLine("The Elapsed event was raised at {0:HH:mm:ss.fff}", e.SignalTime);
            try
            {
                updateVersionWrapper();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Impossibile aggiornare la versione. \nException: " + ex.Message);
            }
        }
        /*private void TimerHelloEvent(Object source, ElapsedEventArgs e)
        {
            try
            {
                networkService.sendHelloMessage();
            }
            catch (NetworkException)
            {
                retry++;
                Console.WriteLine("helloMessage persi: " + retry);
                if (retry == nHelloMissAllowed)
                {
                    Console.WriteLine("Connessione persa...");
                    timerHello.Stop(); timerHello = null;
                    if (timer != null)
                        timer.Stop(); timer = null;
                    retry = 0;
                }
            }            
        }*/

        public void runThread(object sender, DoWorkEventArgs e) {
            List<object> parameter = e.Argument as List<object>;
            if(!(parameter[0] is functionAsynchronous))
                throw new ArgumentException();
            this.worker = sender as BackgroundWorker;

            switch ((functionAsynchronous)parameter[0]) {
                case functionAsynchronous.Synchronize:
                    String monitorDirSynch = parameter[1] as String;
                    synchronize(monitorDirSynch);
                    break;
                case functionAsynchronous.CheckVersion:
                    String monitorDirCheck = parameter[1] as String;
                    e.Result = checkIfCurrentVersionIsUpdated(monitorDirCheck);
                    break;
                case functionAsynchronous.UploadFile:
                    String monitorDirUpload = parameter[1] as String;
                    uploadFile(monitorDirUpload);
                    break;
                case functionAsynchronous.CreateNewVersion:
                    String monitorDirNewVersion = parameter[1] as String;
                    createNewVersion(monitorDirNewVersion);
                    break;
                case functionAsynchronous.RestoreDir:
                    break;
            }                        
        }
    }
}