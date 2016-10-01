using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientDiProva
{
    class HandleClientImpl : HandleClient
    {
        private HandlePackets clientConn;
        private User clientUser;
        private string monitorDir { get; set; }
        private MyBuffer buffer;

        public HandleClientImpl(string serverIP, int port)
        {
            this.monitorDir = null;
            this.clientUser = null;
            this.buffer = new MyBuffer();
            try
            {
                this.clientConn = new HandlePackets(serverIP, port);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public void disconnect()
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
                MyConsole.write("Impossibile disconnettersi: " + e.Message);
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
            string code = "003";
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
        public List<Version> askStoredVersions()
        {
            MyConsole.write("Chiedo al server le versioni salvate");
            string code = "007";
            StoredVersions request = new StoredVersions();
            try
            {
                StoredVersions response = (StoredVersions)clientConn.doRequest(code, request);
                if (response.storedVersions.Count == 0)
                    MyConsole.write("Sul server non è presenta nessuna versione, crearne una nuova");
                else
                    MyConsole.write("Trovate " + response.storedVersions.Count + " versioni");
                return response.storedVersions;
            }
            catch (Exception e)
            {
                MyConsole.write("Problema nella rete: " + e.Message);
                throw;
            }

        }
        public Version createNewVersion()
        {
            MyConsole.write("Richiesta di creazione nuova versione");
            string code = "011";
            DirTree dirTreeService = new DirTreeImpl();
            Dir dirTree = dirTreeService.makeDirTree(new Dir(monitorDir, null));
            CreateVersion request = new CreateVersion(new Version(dirTree));
            try
            {
                CreateVersion response = (CreateVersion)clientConn.doRequest(code, request);
                List<String> elencoHash = response.elencoHash;
                if (elencoHash.Count > 0)
                    sendRequestedFile(dirTree, elencoHash);
                MyConsole.write("La versione è stata creata correttamente sul server");
                return response.version;
            }
            catch (Exception e)
            {
                MyConsole.write(e.Message);
                throw;
            }
        }
        public void closeVersion(Version version)
        {
            MyConsole.write("Richiesta di chiusura della versione");
            string code = "008";
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
            string code = "012";
            RestoreVersion request = new RestoreVersion(new Version(idVersion));
            try
            {
                RestoreVersion response = (RestoreVersion)clientConn.doRequest(code, request);
                Dir dirTree = response.version.dirTree;
                List<File> elencoFile = response.elencoFile;
                DirTree dirTreeService = new DirTreeImpl();
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
                    Version version = createNewVersion(); //versione backup completo
                    closeVersion(version);                //voglio almeno una versione completa sul server
                    version = createNewVersion();         //versione corrente su cui lavorare
                }
                setWatcher(monitorDir);
                Console.WriteLine("Watcher set");
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
        private void sendRequestedFile(Dir dirTree, List<String> elencoHash)
        {
            DirTree dirTreeService = new DirTreeImpl();
            Dictionary<string, File> fileMap = dirTreeService.getAllFileIntoAmap(dirTree);
            Console.WriteLine("File da inviare: " + elencoHash.Count);
            int i = 1;
            foreach (String hash in elencoHash)
            {
                Console.WriteLine("invio file: " + (i++) + "/" + elencoHash.Count);
                sendAfile(fileMap[hash]);
            }
            Console.WriteLine("Tutti i file sono stati inviati correttamente");
        }
        private void sendAfile(File file)
        {
            string pathSrc = file.path;
            WrapFile wrapFile = new WrapFile(file, file.size, new FileStream(pathSrc, FileMode.Open, FileAccess.Read));
            try
            {
                wrapFile = (WrapFile)clientConn.doRequest("002", wrapFile);
                if (wrapFile.error == null)
                    MyConsole.write(wrapFile.message);
                else
                    MyConsole.write(wrapFile.error);
            }
            catch (Exception e)
            {
                MyConsole.write(e.Message);
            }
        }
        private void setWatcher(string path)
        {
            // Create a new FileSystemWatcher and set its properties.
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = path;
            /* Watch for changes in LastAccess and LastWrite times, and
               the renaming of files or directories. */
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
               | NotifyFilters.FileName | NotifyFilters.DirectoryName;

            watcher.IncludeSubdirectories = true;

            // Add event handlers.
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.Deleted += new FileSystemEventHandler(OnChanged);
            watcher.Renamed += new RenamedEventHandler(OnRenamed);

            // Begin watching.
            watcher.EnableRaisingEvents = true;
        }

        // Define the event handlers.
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            FileSystemService fsService = new FileSystemServiceImpl();
            if (fsService.isAfile(e.FullPath))
                handleFileChange(e.ChangeType, e.FullPath, null);
            else if (fsService.isAdir(e.FullPath))
                handleDirChange(e.ChangeType, e.FullPath, null);
        }
        private void OnRenamed(object source, RenamedEventArgs e)
        {
            FileSystemService fsService = new FileSystemServiceImpl();
            if (fsService.isAfile(e.FullPath))
                handleFileChange(e.ChangeType, e.FullPath, e.OldFullPath);
            else if (fsService.isAdir(e.FullPath))
                handleDirChange(e.ChangeType, e.FullPath, e.OldFullPath);
        }

        private void handleFileChange(WatcherChangeTypes change, string path, string oldPath)
        {
            if (oldPath == null)
                Console.WriteLine("File: " + path + " " + change);
            else
                Console.WriteLine("File: {0} renamed to {1}", oldPath, path);

            FileSystemService fsService = new FileSystemServiceImpl();
            CheckFile checkFile = new CheckFile();
            File file = new File();

            switch (change)
            {
                case WatcherChangeTypes.Created:
                    checkFile.operation = "addFile";
                    checkFile.file = new File(path, monitorDir);
                    buffer.list.Add(checkFile);
                    break;

                case WatcherChangeTypes.Deleted:
                    file = buffer.containsThisFile(fsService.getPathFromMonitorDir(path, monitorDir));
                    if (file != null)
                        buffer.removeThisFile(file);
                    else
                    {
                        checkFile.operation = "deleteFile";
                        checkFile.file = new File(path, monitorDir);
                        //checkFile.path = fsService.getPathFromMonitorDir(path, monitorDir);
                        buffer.list.Add(checkFile);
                    }
                    break;

                case WatcherChangeTypes.Changed:
                    file = buffer.containsThisFile(fsService.getPathFromMonitorDir(path, monitorDir));
                    if (file != null)
                    {
                        File newfile = new File(path, monitorDir);
                        file.hash = newfile.hash;
                        file.size = newfile.size;
                        file.lastWriteTime = newfile.lastWriteTime;
                    }
                    else
                    {
                        checkFile.operation = "updateFile";
                        checkFile.file = new File(path, monitorDir);
                        buffer.list.Add(checkFile);
                    }
                    break;

                case WatcherChangeTypes.Renamed:
                    file = buffer.containsThisFile(fsService.getPathFromMonitorDir(oldPath, monitorDir));
                    if (file != null)
                    {
                        File newfile = new File(path, monitorDir);
                        file.name = newfile.name;
                        file.path = newfile.path;
                        file.extension = newfile.extension;
                    }
                    else
                    {
                        checkFile.operation = "renameFile";
                        checkFile.oldPath = fsService.getPathFromMonitorDir(oldPath, monitorDir);
                        checkFile.newPath = fsService.getPathFromMonitorDir(path, monitorDir);
                        buffer.list.Add(checkFile);
                    }
                    break;
            }
        }
        private void handleDirChange(WatcherChangeTypes change, string path, string oldPath)
        {
            if (oldPath == null)
                Console.WriteLine("Directory: " + path + " " + change);
            else
                Console.WriteLine("Directory: {0} renamed to {1}", oldPath, path);

            FileSystemService fsService = new FileSystemServiceImpl();
            CheckFile checkFile = new CheckFile();
            File file = new File();
            Dir dir;

            switch (change)
            {
                case WatcherChangeTypes.Created:
                    checkFile.operation = "addDir";
                    dir = new Dir(fsService.getPathFromMonitorDir(path, monitorDir));
                    dir.setCreationTime(path);
                    dir.setLastWriteTime(path);
                    checkFile.dir = dir;   
                    buffer.list.Add(checkFile);
                    break;

                case WatcherChangeTypes.Deleted:
                    checkFile = buffer.containsThisDir(fsService.getPathFromMonitorDir(path, monitorDir));
                    if (checkFile != null)
                        buffer.removeThisDir(path);
                    else
                    {
                        checkFile = new CheckFile();
                        checkFile.operation = "deleteDir";
                        checkFile.path = fsService.getPathFromMonitorDir(path, monitorDir);
                        buffer.list.Add(checkFile);
                    }
                    break;

                case WatcherChangeTypes.Changed:
                    checkFile = buffer.containsThisDir(fsService.getPathFromMonitorDir(path, monitorDir));
                    if (checkFile != null)
                        checkFile.dir.lastWriteTime = Directory.GetLastWriteTime(path);
                    else
                    {
                        checkFile = new CheckFile();
                        checkFile.operation = "updateDir";
                        dir = new Dir(fsService.getPathFromMonitorDir(path, monitorDir));
                        dir.setLastWriteTime(path);
                        checkFile.dir = dir;   
                        buffer.list.Add(checkFile);
                    }
                    break;

                case WatcherChangeTypes.Renamed:
                    checkFile = buffer.containsThisDir(fsService.getPathFromMonitorDir(oldPath, monitorDir));
                    if (checkFile != null)
                    {
                        checkFile.path = path;
                    }
                    else
                    {
                        checkFile = new CheckFile();
                        checkFile.operation = "renameDir";
                        checkFile.oldPath = fsService.getPathFromMonitorDir(oldPath, monitorDir);
                        checkFile.newPath = fsService.getPathFromMonitorDir(path, monitorDir);
                        buffer.list.Add(checkFile);
                    }
                    break;
            }
        }
    }
}
