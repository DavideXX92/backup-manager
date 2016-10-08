using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace ClientDiProva
{
    class UserControllerImpl : UserController
    {
        private NetworkService networkService;
        private DirTreeService dirTreeService;
        private Watcher watcher;
        private Timer timer;

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
        }

        public void setMonitorDir(string path)
        {
            try
            {
                networkService.setMonitorDir(path);
            }catch(Exception e)
            {
                throw e;
            }
            
        }
        public string getMonitorDir()
        {
            try
            {
                return networkService.getMonitorDir();
            }
            catch (Exception e)
            {
                string error = "Problema di rete: impossibile recuperare la cartella da monitorare";
                Console.WriteLine(error + "\nException: " + e.Message);
                throw e;
            }
        }
        public void register(string username, string password)
        {
            try
            {
                networkService.registerRequest(username, password);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public User login(string username, string password)
        {
            try
            {
                return networkService.loginRequest(username, password);
            }
            catch (Exception e)
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
            catch (Exception e)
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
            catch (Exception e)
            {
                throw e;
            }
        }
        public void restoreVersion(int idVersion, string pathDst)
        {
            try
            {
                Version version = networkService.getVersion(idVersion);
                dirTreeService.createDirTree(version.dirTree, pathDst);
                requireMissingFile(dirTreeService.getAllFileIntoAlist(version.dirTree), pathDst);
            }
            catch (Exception e)
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
            catch (Exception e)
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
                    MyConsole.write("Sul server non è presenta nessuna versione, ne creo una nuova");
                    Version version;                          //versione backup completo
                    version = createNewVersion(monitorDir);   //voglio almeno una versione completa sul server 
                    version = createNewVersion(monitorDir);   //versione corrente su cui lavorare
                }
                else
                {
                    Dir dirTree = new Dir(monitorDir, null);
                    dirTreeService.makeDirTree(dirTree);
                    syncFile(dirTree);
                }
                watcher = new Watcher(monitorDir);
                watcher.set();
                Console.WriteLine("Watcher set");
                timer = new Timer(10000);
                timer.Elapsed += OnTimedEvent;
                timer.AutoReset = true;
                enableAutoSync();
            }
            catch(Exception e)
            {
                throw e;
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
        public void manualSync()
        {
            if (watcher.buffer.list.Count > 0)
                updateVersion(watcher.buffer);
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
                    MyConsole.write("Le due versioni sono uguali");
                    return true;
                }
                else
                {
                    MyConsole.write("Le due versioni sono diverse");
                    return false;
                }   
            }
            catch (Exception e)
            {
                throw e;
            }   
        }

        private void syncFile(Dir dirTree)
        {
            List<string> hashToSend = networkService.askHashToSend();
            MyConsole.write("Nessun file da inviare");
            if (hashToSend.Count > 0)
            {
                MyConsole.write(hashToSend.Count + " file devono essere trasferiti al server");
                Dictionary<string, File> fileMap = dirTreeService.getAllFileIntoAmap(dirTree);
                sendRequestedFile(fileMap, hashToSend);
                MyConsole.write("File inviati");
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
                        Console.WriteLine("Il file: " + hash + "non e' più presente sul pc");
                        //mandare una request per decrementare il countere dell'hash
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Impossibile inviare il file " + (i) + "/" + elencoHash.Count + " " + e.Message);
                }
                i++;
            }
        }
        private void requireMissingFile(List<File> elencoFile, string rootPath)
        {
            MyConsole.write(elencoFile.Count + " file devono essere trasferiti");
            int i = 1;
            foreach (File file in elencoFile)
            {
                MyConsole.write("trasferimento file " + (i++) + "/" + elencoFile.Count);
                networkService.requestAfile(file, rootPath);
            }
            MyConsole.write("Tutti i file sono stati ricevuti correttamente");
        }
        private void updateVersion(MyBuffer bufferOperation)
        {
            networkService.updateVersion(bufferOperation);
            List<string> hashToSend = networkService.askHashToSend();
            if (hashToSend.Count > 0)
            {
                MyConsole.write(hashToSend.Count + " file devono essere trasferiti al server");
                Dictionary<string, File> fileMap = bufferOperation.getAllFileIntoAmap();
                sendRequestedFile(fileMap, hashToSend);
                MyConsole.write("File inviati");
            }
            watcher.buffer.list.Clear();
            Console.WriteLine("La versione e' stata aggiornata");
        }
        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            Console.WriteLine("The Elapsed event was raised at {0:HH:mm:ss.fff}", e.SignalTime);
            if (watcher.buffer.list.Count > 0)
                updateVersion(watcher.buffer);
        }
    }
}
