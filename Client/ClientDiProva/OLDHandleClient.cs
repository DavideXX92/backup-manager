using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ClientDiProva
{
    class OLDHandleClient
    {/*
        private TcpClient client;
        private HandlePackets clientConn;
        private User clientUser;
        public string monitorDir { get; set; }
        
        //private String monitorDir = "C:/ricevuti/monitorDir/";
        //private string monitorDir = @"C:\ricevuti\monitorDir\";  

        public HandleClient()
        {
            this.monitorDir = null;
            this.clientUser = null;
        }

        public HandleClient(TcpClient client): this()
        {
            this.client = client;
            try{
                clientConn = new HandlePackets(client);
            } 
            catch(Exception e){
                throw;
            }
        }

        public HandleClient(string serverIP, int port): this()
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

        //
        public void registerRequest(string username, string password)
        {
            string code = "004";
            User user = new User(username, password);
            Register request = new Register(user);
            Register response = (Register)clientConn.doRequest(code, request);
            if (response.error==null)
            {
                Console.WriteLine(response.message);
            }
            else
            {
                Console.WriteLine(response.error);
            }
        }

        //
        public void loginRequest()
        {
            string code = "003";
            User user = new User("giuseppe", "piscopo");
            Login request = new Login(user);
            Login response = (Login)clientConn.doRequest(code, request);
            if (response.error==null)
            {
                user = response.user;
                clientUser = user;
                Console.WriteLine(response.message);
                MyConsole.write("Logged");
                MyConsole.write("id: " + user.idUser);
                MyConsole.write("User: " + user.username);
                MyConsole.write("Pass: " + user.password);
            }
            else
            {
                MyConsole.write(response.error);
            }
        }

        //
        public void addNewVersion()
        {
            MyConsole.write("Richiesta di creazione nuova versione");
            string code = "011";
            if (monitorDir!=null)
            {
                Dir dirTree = new Dir(monitorDir, null);
                createTree(dirTree);
                Console.WriteLine("createTree...done!");
                NewVersion newVersionRequest = new NewVersion(dirTree);
                try
                {
                    NewVersion newVersionResponse = (NewVersion)clientConn.doRequest(code, newVersionRequest);
                    List<String> elencoHash = newVersionResponse.elencoHash;
                    if (elencoHash.Count > 0)
                        sendFilesRequired(dirTree, elencoHash);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                } 
            }
        }

        //
        private void sendFilesRequired(Dir dirTree, List<String> elencoHash)
        {
            Dictionary<string, File> fileMap = createMapFromTree(dirTree);
            Console.WriteLine("File da inviare: " + elencoHash.Count);
            int i=1;
            foreach(String hash in elencoHash)
            {
                Console.WriteLine("invio file: " + (i++) + "/" + elencoHash.Count);
                sendAfile(fileMap[hash]);
            }
            Console.WriteLine("Tutti i file sono stati inviati correttamente");
        }

        //
        public void restoreDir()
        {
            MyConsole.write("Richiesta di ripristino della cartella");
            string code = "012";
            Restore restoreRequest = new Restore(0);
            try
            { 
                Restore restoreResponse = (Restore)clientConn.doRequest(code, restoreRequest);
                Dir dirTree = restoreResponse.dirTree;
                List<File> elencoFile = restoreResponse.elencoFile;
                createPhisicallyTreeRecursive(@"c:\tmp", dirTree);
                requireFileMissing(elencoFile);
            }
            catch (Exception e)
            {
                MyConsole.write("Problema durante il restore della cartella: " + e.Message);
            }
        }
        
        //
        private void requireFileMissing(List<File> elencoFile)
        {
            MyConsole.write(elencoFile.Count + " file devono essere trasferiti");
            int i=1;
            foreach(File file in elencoFile)
            {
                MyConsole.write("trasferimento file " + (i++) + "/" + elencoFile.Count);
                requestAfile(file);
            }
            MyConsole.write("Cartella ripristinata");
        }

        //
        private void createPhisicallyTreeRecursive(string rootPath, Dir targetDir)
        {
            string dirPath = rootPath + @"\" + targetDir.path;
            Directory.CreateDirectory(dirPath);
            foreach (Dir dir in targetDir.elencoSubdirectory)
                createPhisicallyTreeRecursive(rootPath, dir);
        }

        //
        private Dictionary<string, File> createMapFromTree(Dir dirTree)
        {
            Dictionary<string, File> fileMap = new Dictionary<string, File>();
            createMapRecursive(dirTree, fileMap);
            return fileMap;
        }

        //
        private void createMapRecursive(Dir targetDir, Dictionary<string, File> fileMap)
        {
            foreach (File file in targetDir.elencoFile)
                fileMap[file.hash] = file;
            foreach (Dir dir in targetDir.elencoSubdirectory)
                createMapRecursive(dir, fileMap);
        }

        //
        private void createTree(Dir targetDir)
        {

            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(targetDir.path);
            foreach (string fileName in fileEntries)
                targetDir.elencoFile.Add(getFile(fileName, targetDir));

            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = Directory.GetDirectories(targetDir.path);
            foreach (string subdirectory in subdirectoryEntries)
            {
                Dir subDir = new Dir(subdirectory, targetDir);
                targetDir.elencoSubdirectory.Add(subDir);
                createTree(subDir);
            }
        }

        //
        private File getFile(String filename, Dir parentDir)
        {
            File file = new File();
            FileInfo fileinfo = new FileInfo(filename);
            file.name = fileinfo.Name;
            file.path = parentDir.path + @"\" + file.name;
            file.parentDir = parentDir;
            file.size = (int)fileinfo.Length;
            file.hash = getHash(filename);
            file.extension = Path.GetExtension(filename);
            return file;
        }

        //
        private string getHash(string filename)
        {
            HashAlgorithm sha1 = HashAlgorithm.Create();
            using (FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
                return BitConverter.ToString(sha1.ComputeHash(stream));
        }

        //NO
        public void synchronizeRequest()
        {
            MyConsole.write("Richiesta di sincronizzazione");
            string code = "006";
            FileSystem fs = new FileSystem();
            try 
            {
                List<File> AllFilesInMonitorDir = fs.getAllFiles(monitorDir);
                List<File> filesToBackup = (List<File>)clientConn.doRequest(code, AllFilesInMonitorDir);               
                MyConsole.write("Richiesta completata.");
               
                MyConsole.write("saranno inviati " + filesToBackup.Count + " file");
                for (int i = 0; i < filesToBackup.Count; i++) 
                {
                    sendAfile(filesToBackup[i]);
                    MyConsole.write(i+1 + "/" + filesToBackup.Count + "sent");
                }
                MyConsole.write("Alle files are sent.");

                completeSyncronization();

            }catch(Exception e){
                MyConsole.write("Problema di sincronizzazione: " + e.Message);
                Console.WriteLine("Problema di sincronizzazione: " + e.Message);
            }
        }

        //NO
        public void completeSyncronization()
        {
            GenericReq request = new GenericReq();
            try{
                GenericReq response = (GenericReq)clientConn.doRequest("008", request);
                if (response.error==null)
                {
                    Console.WriteLine(response.message);
                    MyConsole.write(response.message);
                }
                else
                {
                    Console.WriteLine(response.error);
                    MyConsole.write(response.error);
                }
            }catch(Exception e){
                MyConsole.write("Problema durante il completamento della sincronizzazione: " + e.Message);
            }
                

        }

        //
        public void requestAfile(File file)
        {
            String path = @"c:\tmp\" + file.path;
            try
            {
                WrapFile wrapFile = new WrapFile(file, -1, new FileStream(path, FileMode.Create, FileAccess.Write));
                wrapFile = (WrapFile)clientConn.doRequest("001", wrapFile);
                if (wrapFile.error == null)
                    MyConsole.write(wrapFile.message+" and received by the client");
                else
                    MyConsole.write(wrapFile.error);
            }catch(Exception e){
                MyConsole.write(e.Message);
            }
        }

        //
        public void sendAfile(File file)
        {
            string pathSrc = file.path;
            WrapFile wrapFile = new WrapFile(file, file.size, new FileStream(pathSrc, FileMode.Open, FileAccess.Read));
            try
            {
                wrapFile = (WrapFile)clientConn.doRequest("002", wrapFile);
                if(wrapFile.error==null)
                    MyConsole.write(wrapFile.message);
                else
                    MyConsole.write(wrapFile.error);
            }
            catch(Exception e){
                MyConsole.write(e.Message);
            }
        }
        */
    }
}
