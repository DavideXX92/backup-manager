using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Threading;

namespace PDSclient
{
    class UserControllerImpl : UserController
    {
        private NetworkService networkService;
        private DirTreeService dirTreeService;
        private Watcher watcher;
        private System.Windows.Threading.DispatcherTimer dispatcherTimer;
        private bool imUpdating;  
        private bool threadWorking;
        public BackgroundWorker worker;
        private object _lock = new object();
        private RunWorkerCompletedEventHandler onWorkerComplete;
        private ProgressChangedEventHandler autoSynch_ProgressChanged;

        public UserControllerImpl()
        {
            try
            {
                this.networkService = new NetworkServiceImpl(Config.ServerIP, Config.ServerPort, Config.KeepalivePort);
            }catch(Exception e)
            {
                throw e;
            }
            this.dirTreeService = new DirTreeServiceImpl();
            this.watcher = null;         
            this.worker = null;
            this.dispatcherTimer = null;
            this.imUpdating = false;
            this.threadWorking = false;
        }

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
            catch (BusyResourceException e)
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
            catch (BusyResourceException e)
            {
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
            catch (BusyResourceException e)
            {
                throw e;
            }
        }
        public void register(string username, string password)
        {
            try
            {
                var sha1 = new SHA1CryptoServiceProvider();
                var passBytes = sha1.ComputeHash(ASCIIEncoding.ASCII.GetBytes(password));
                string passEncrypted = BitConverter.ToString(passBytes);   
                networkService.registerRequest(username, passEncrypted);
            }
            catch (NetworkException e)
            {
                throw e;
            }
            catch (ServerException e)
            {
                throw e;
            }
            catch (BusyResourceException e)
            {
                throw e;
            }
        }
        public User login(string username, string password)
        {
            try
            {
                var sha1 = new SHA1CryptoServiceProvider();
                var passBytes = sha1.ComputeHash(ASCIIEncoding.ASCII.GetBytes(password));
                string passEncrypted = BitConverter.ToString(passBytes);         
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
            catch (BusyResourceException e)
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
                if (dispatcherTimer != null) dispatcherTimer = null;
            }
            catch (NetworkException e)
            {
                throw e;
            }
            catch (ServerException e)
            {
                throw e;
            }
            catch (BusyResourceException e)
            {
                throw e;
            }
        }
        public Version createNewVersion(string monitorDir)
        {
            try
            {
                manualSync();
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
            catch (BusyResourceException e)
            {
                throw e;
            }
        }
        public void restoreLastVersion(string monitorDir)
        {
            try
            {
                int idOpenVersion;
                Version openVersion = networkService.getOpenVersion();
                idOpenVersion = openVersion.idVersion;
                restoreVersion(idOpenVersion, monitorDir);
                networkService.closeVersion(idOpenVersion);
            }
            catch (NetworkException e)
            {
                throw e;
            }
            catch (ServerException e)
            {
                throw e;
            }
            catch (BusyResourceException e)
            {
                throw e;
            }
        }
        public void restoreVersion(int idVersion, string monitorDir)
        {
            try
            {
                if (watcher != null) watcher = null;
                //watcher.bufferRead.clearList();
                //watcher.bufferWrite.clearList();
                Version version = networkService.getVersion(idVersion);
                string pathDst;
                if(monitorDir[monitorDir.Length - 1] == '\\')
                    monitorDir = Directory.GetParent(monitorDir).FullName;
                pathDst = Directory.GetParent(monitorDir).FullName;
                pathDst += @"\restoreTmp";                
                dirTreeService.createDirTree(version.dirTree, pathDst);
                requireMissingFile(dirTreeService.getAllFileIntoAlist(version.dirTree), pathDst+@"\");
                string tmpDirName = monitorDir + "OLD" + (new Random().Next(1000, 9999));
                Directory.Move(monitorDir, tmpDirName);
                string dirRestored = pathDst + @"\" + version.dirTree.name;
                Directory.Move(dirRestored, Directory.GetParent(monitorDir).FullName + @"\" + version.dirTree.name);
                Directory.Delete(pathDst);
                Directory.Delete(tmpDirName, true);
                watcherInit(monitorDir);
            }
            catch (NetworkException e)
            {
                throw e;
            }
            catch (ServerException e)
            {
                throw e;
            }
            catch (BusyResourceException e)
            {
                throw e;
            }
        }
        public string restoreDir(string pathDst)
        {
            try
            {
                int idOpenVersion;
                Version version = networkService.getOpenVersion();
                idOpenVersion = version.idVersion;
                dirTreeService.createDirTree(version.dirTree, pathDst);
                requireMissingFile(dirTreeService.getAllFileIntoAlist(version.dirTree), pathDst + @"\");
                string path = pathDst + @"\" + version.dirTree.name;
                networkService.closeVersion(idOpenVersion);
                return path;
            }
            catch (NetworkException e)
            {
                throw e;
            }
            catch (BusyResourceException e)
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
            catch (BusyResourceException e)
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
            catch (BusyResourceException e)
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
        public void watcherDelete()
        {
            if (watcher != null)
                watcher = null;
        }
        public void timerInit(RunWorkerCompletedEventHandler onWorkerComplete, ProgressChangedEventHandler autoSynch_ProgressChanged)
        {
            if(dispatcherTimer==null) {
                
                this.onWorkerComplete = onWorkerComplete;
                this.autoSynch_ProgressChanged = autoSynch_ProgressChanged;
                
                dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                dispatcherTimer.Tick += new EventHandler(OnTimedEvent);
                dispatcherTimer.Interval = new TimeSpan(0, 0, 10);
                dispatcherTimer.Start();
            }
        }
        public void enableAutoSync()
        {
            //timer.Enabled = true; //SCOMMENTARE
            dispatcherTimer.Start();
            Console.WriteLine("Timer set");
        }
        public void disableAutoSync()
        {
            //timer.Enabled = false; //SCOMMENTARE
            dispatcherTimer.Stop();
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
            catch (BusyResourceException e)
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
            catch (BusyResourceException e)
            {
                throw e;
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
            catch (BusyResourceException e)
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
                    worker.ReportProgress(i * 100 / elencoFile.Count);
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
                        if (worker != null)
                            worker.ReportProgress(-1);

                        updateVersion(watcher.bufferWrite); //provo a fare un altro updateVersion solo con quei file
                        res = 1;
                    }
                    else                                    //invece se tutti i file precedenti sono stati mandati
                    {
                        if (watcher.bufferRead.list.Count > 0) //controllo se ci sono nuovi file da inviare
                        {
                            if (worker != null)
                                worker.ReportProgress(-1);

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
            catch(Exception e){
                throw e;
            }
        }

        private void OnTimedEvent(Object source, EventArgs e)//ElapsedEventArgs e
        {
            Console.WriteLine("The Elapsed event was raised at {0:HH:mm:ss.fff}");
            lock (_lock)
            {
                if (threadWorking == false)
                {
                    threadWorking = true;
                    worker = newWorker(onWorkerComplete, autoSynch_ProgressChanged, new List<object>());
                }
                else
                    return;
            }
            worker.RunWorkerAsync(new List<object> { functionAsynchronous.AutoSynch });
        }
        private BackgroundWorker newWorker(RunWorkerCompletedEventHandler handler, ProgressChangedEventHandler autoSynch_ProgressChanged, List<object> optional_parameters)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += runThread;
            worker.ProgressChanged += autoSynch_ProgressChanged;
            worker.RunWorkerCompleted += handler;

            return worker;
        }       
        public void runThread(object sender, DoWorkEventArgs e)
        {

            List<object> parameter = e.Argument as List<object>;

            if ((functionAsynchronous)parameter[0] != functionAsynchronous.AutoSynch)
            {
                lock (_lock)
                {
                    if (threadWorking == false)
                    {
                        threadWorking = true;
                        this.worker = sender as BackgroundWorker;
                    }
                    else
                        return;
                }
            }

            try
            {
                if (!(parameter[0] is functionAsynchronous))
                    throw new ArgumentException();

                switch ((functionAsynchronous)parameter[0])
                {
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
                        String monitorDirRestore = parameter[1] as String;                        
                        monitorDirRestore = monitorDirRestore.Substring(0, monitorDirRestore.LastIndexOf(@"\"));
                        restoreDir(monitorDirRestore);
                        break;
                    case functionAsynchronous.ManualSynch:
                        e.Result = manualSync();
                        break;
                    case functionAsynchronous.RestoreVersion:
                        int idVersion = (int)parameter[1];
                        String monitorDirRestoreVersion = parameter[2] as String;
                        restoreVersion(idVersion, monitorDirRestoreVersion);
                        break;
                    case functionAsynchronous.RestoreLastVersion:
                        String monitorDirRestoreLastVersion = parameter[1] as String;
                        restoreLastVersion(monitorDirRestoreLastVersion);
                        break;
                    case functionAsynchronous.AutoSynch:
                        updateVersionWrapper();
                        break;
                    case functionAsynchronous.Hello:
                        sendHelloMessage();
                        break;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                lock (_lock)
                {
                    threadWorking = false;
                }
            }
        }                
    }
}