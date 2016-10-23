﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace newServerWF
{
    class ServerControllerImpl : ServerController
    {
        private const int nOfMaxVersionToSaveForUser = 3;
        private const string serverDirRoot = @"c:\ServerDir\";

        private FileSystemService fsService;
        private UserService userService;
        private VersionService versionService;

        private TcpClient client;
        private HandlePackets clientConn;
        private Dizionario dizionario; //Class to associate messages with functions

        private User clientUser;
        private string clientDir;

        public ServerControllerImpl(TcpClient client)
        {
            this.client = client;
            this.clientUser = null;
            this.clientDir = null;
            this.dizionario = new Dizionario(this);
            this.fsService = new FileSystemServiceImpl();
            this.userService = new UserServiceImpl();
            this.versionService = new VersionServiceImpl();
        }

        public ServerController startLoop()
        {
            try
            {
                clientConn = new HandlePackets(client, dizionario.getDelegate());
                clientConn.startListen();
            }
            catch (Exception e)
            {
                MyConsole.Write("C'è stato un errore, chiudo la connessione con il client... dovrei vedere se riesco a ripristinarla piuttosto che chiuderla");
            }
            return this;
        }
        public void stop()
        { 
            clientConn.stopListen(); 
        }

        public GenericRequest helloMessage(GenericRequest request)
        {
            request.message = "hello message ack";
            return request;
        }
        public GenericRequest closeConnectionWithTheClient(GenericRequest request)
        {
            try
            {
                clientUser.isLogged = false;
                userService.updateUser(clientUser);
                clientConn.stopListen();
                request.message = "Client disconnesso correttamente";
                return request;
            }
            catch (Exception e)
            {
                request.error = e.Message;
                return request;
            }
        }
        public Register handleRegistration(Register registerRequest)
        {
            User user = registerRequest.user;
            Register registerResponse = new Register(user);
            if (userService.checkIfUsernameExists(user.username))
            {
                Console.WriteLine("Impossibile registrare l'utente: " + user.username + " username già esistente");
                registerResponse.error = "username già esistente";
            }
            else
            {
                try
                {
                    registerResponse.user = userService.saveUser(user);
                    registerResponse.isRegistred = true;
                    registerResponse.message = "Registrazione avvenuta con successo";
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    registerResponse.error = e.Message;
                }
            }
            return registerResponse;
        }
        public Login handleLogin(Login loginRequest)
        {
            User user = loginRequest.user;
            Console.WriteLine("L'utente " + user.username + " sta tentando di autenticarsi.");
            Login loginResponse = new Login(user);
            if (userService.checkIfCredentialsAreCorrected(user.username, user.password))
            {
                clientUser = userService.getUser(user.username);
                clientUser.isLogged = true;
                try
                {
                    userService.updateUser(user);
                }
                catch (Exception e)
                {
                    loginResponse.error = "problema di comunicazione col database";
                    return loginResponse;
                }  
                Console.WriteLine(user.username + ": autenticazione riuscita");
                loginResponse.user = clientUser;
                loginResponse.message = user.username + " autenticazione riuscita";
                clientDir = serverDirRoot + user.username + @"\";
                MyConsole.setClientLog(serverDirRoot + @"Log\" + clientUser.username + ".txt");
                MyConsole.Log(clientUser.username + " logged");
            }
            else
            {
                loginResponse.error = "username e/o password errati";
            }
            return loginResponse;
        }
        public MonitorDir addMonitorDir(MonitorDir request)
        {           
            try
            {
                userService.addMonitorDir(clientUser.username, request.path);
                clientUser.monitorDir.Add(request.path);
                request.message = "MonitorDir aggiunta";
            }
            catch (Exception e)
            {
                request.error = e.Message;
            }
            return request;
        }
        public MonitorDir changeMonitorDir(MonitorDir request)
        {
            try
            {
                userService.changeMonitorDir(clientUser.username, request.oldPath, request.newPath);
                clientUser.changeMonitorDir(request.oldPath, request.newPath);
                request.message = "MonitorDir modificata";
            }catch(Exception e)
            {
                request.error = e.Message;
            }
            return request;
        }
        public MonitorDir getMonitorDir(MonitorDir request)
        {
            request.monitorDir = clientUser.monitorDir;
            return request;
        }
        public CreateVersion createNewVersion(CreateVersion request)
        {
            try
            {
                List<int> idVersionList = versionService.getAllIdOfVersions(clientUser.username);
                using(TransactionScope scope = new TransactionScope())
                {
                    if (idVersionList.Count > nOfMaxVersionToSaveForUser)
                        versionService.deleteVersion(clientUser.username, idVersionList.ElementAt(0), clientDir);

                    int idOpenVersion = versionService.getCurrentVersionID(clientUser.username);
                    if (idOpenVersion != -1)
                        versionService.closeVersion(clientUser.username, idOpenVersion);

                    request.version = versionService.saveVersion(request.version.dirTree, clientUser.username);
                    scope.Complete();
                }
                return request;
            }catch(Exception e)
            {
                request.error = e.Message;
                return request;
            } 
        }
        public UpdateVersion updateVersion(UpdateVersion request)
        {
            try
            {
                int idVersion = versionService.getCurrentVersionID(clientUser.username);
                versionService.updateVersion(clientUser.username, idVersion, request);
                request.message = "versione aggiornata";
            }
            catch (Exception e)
            {
                request.error = e.Message;
                Console.WriteLine("impossibile aggiornare la versione");
            }
            return request;
        }
        public CloseVersion closeVersion(CloseVersion request)
        {
            try
            {
                versionService.closeVersion(clientUser.username, request.idVersion);
                request.message = "versione chiusa";
            }
            catch (Exception e)
            {
                request.error = e.Message;
                Console.WriteLine("impossibile chiudere la versione");
            }
            return request;
        }
        public StoredVersions getIDofAllVersions(StoredVersions request){
            try{
                request.elencoID = versionService.getAllIdOfVersions(clientUser.username);
            }catch(Exception e){
                request.error = e.Message;
            }
            return request;
        }
        public GetVersion getVersion(GetVersion request)
        {
            try
            {
                request.version = versionService.getVersion(clientUser.username, request.version.idVersion);
                request.message = "Versione recuperata";
            }
            catch (Exception e)
            {
                request.error = e.Message;
            }
            return request;
        }
        public GetVersion getOpenVersion(GetVersion request)
        {
            try
            {
                int idVersion = versionService.getCurrentVersionID(clientUser.username);
                if (idVersion == -1)
                {
                    request.version = null;
                    return request;
                }
                request.version = versionService.getVersion(clientUser.username, idVersion);
                request.message = "Versione aperta recuperata";
            }
            catch (Exception e)
            {
                request.error = e.Message;
            }
            return request;
        }
        public GenericRequest deleteUserRepository(GenericRequest request)
        {
            try
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    foreach (string monitorDir in clientUser.monitorDir)
                        userService.deleteMonitorDir(clientUser.username, monitorDir);
                    List<int> elencoID = versionService.getAllIdOfVersions(clientUser.username);
                    foreach (int idVersion in elencoID)
                        versionService.deleteVersion(clientUser.username, idVersion, clientDir);
                    scope.Complete();
                }
                clientUser = userService.getUser(clientUser.username);
                return request;
            }
            catch (Exception e)
            {
                request.error = e.Message;
                return request;
            }
        }
        public HashRequest sendHashToBeingReceived(HashRequest request)
        {
            try
            {
                request.elencoHash = fsService.getAllHashToBeingReceived(clientUser.username);
                request.message = "Lista degli hash mancanti creata";
            }
            catch(Exception e)
            {
                request.error = e.Message;
            }
            return request;
        }
        public WrapFile handleRequestOfFile(File file)
        {
            //String pathSrc = clientDir + file.hash + file.extension;
            String pathSrc = clientDir + file.hash;
            FileInfo fileinfo = new FileInfo(pathSrc);
            file.size = (int)fileinfo.Length;
            WrapFile wrapFile = new WrapFile(file, file.size, new FileStream(pathSrc, FileMode.Open, FileAccess.Read));
            return wrapFile;
        }
        public WrapFile initializeReceiptOfFile(File file)
        {
            //string pathDst = clientDir + file.hash + file.extension;
            string pathDst = clientDir + file.hash;
            WrapFile wrapFile = new WrapFile(file, -1, new FileStream(pathDst, FileMode.Create, FileAccess.Write));
            return wrapFile;
        }
        public WrapFile fileReceived(WrapFile wrapFile)
        {
            //Update the db
            File file = wrapFile.file;
            try
            {
                fsService.setFileAsReceived(clientUser.username, file.hash);
                Console.WriteLine("File saved by the server");
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
            } 
            return wrapFile;
        }
    
        public CheckFile checkFile(CheckFile request)
        {
            switch (request.operation)
            {
                case "checkFile":
                    if (fsService.checkIfFileExists(clientUser.username, 1, request.path))
                        request.message = "il file esiste";
                    else
                        request.message = "il file non esiste";
                    break;
                case "addFile":
                    try
                    {
                        fsService.addFile(clientUser.username, 1, request.file);
                        request.message = "File creato correttamente";

                    }catch(Exception e){
                        request.message = "impossibile creare il file: " + e.Message;
                        MyConsole.Write("impossibile creare il file: " + e.Message);
                    }
                    break;
                case "updateFile":
                    try
                    {
                        fsService.updateFile(clientUser.username, 1, request.file);
                        request.message = "File modificato correttamente";

                    }
                    catch (Exception e)
                    {
                        request.message = "impossibile modificare il file: " + e.Message;
                        MyConsole.Write("impossibile modificare il file: " + e.Message);
                    }
                    break;
                case "renameFile":
                    try
                    {
                        fsService.renameFile(clientUser.username, 1, request.oldPath, request.newPath);
                        request.message = "File rinominato correttamente";

                    }
                    catch (Exception e)
                    {
                        request.message = "impossibile rinominare il file: " + e.Message;
                        MyConsole.Write("impossibile rinominare il file: " + e.Message);
                    }
                    break;
                case "deleteFile":
                    try
                    {
                        fsService.deleteFile(clientUser.username, 1, request.path);
                        request.message = "File eliminato correttamente";
                    }
                    catch (Exception e)
                    {
                        request.message = "impossibile eliminare il file: " + e.Message;
                        MyConsole.Write("impossibile eliminare il file: " + e.Message);
                    }
                    break;
                case "checkDir":
                    if (fsService.checkIfDirExists(clientUser.username, 1, request.path))
                        request.message = "la directory esiste";
                    else
                        request.message = "la directory non esiste";
                    break;
                case "addDir":
                    try
                    {
                        fsService.addDir(clientUser.username, 1, request.dir);
                        request.message = "Directory creata correttamente";

                    }
                    catch (Exception e)
                    {
                        request.message = "impossibile creare la directory: " + e.Message;
                        MyConsole.Write("impossibile creare la directory: " + e.Message);
                    }
                    break;
                case "updateDir":
                    try
                    {
                        fsService.updateDir(clientUser.username, 1, request.dir);
                        request.message = "Directory aggiornata correttamente";

                    }
                    catch (Exception e)
                    {
                        request.message = "impossibile aggiornare la directory: " + e.Message;
                        MyConsole.Write("impossibile aggiornare la directory: " + e.Message);
                    }
                    break;
                case "renameDir":
                    try
                    {
                        fsService.renameDir(clientUser.username, 1, request.oldPath, request.newPath);
                        request.message = "Directory rinominata correttamente";

                    }
                    catch (Exception e)
                    {
                        request.message = "impossibile rinominare la cartella: " + e.Message;
                        MyConsole.Write("impossibile rinominare la cartella: " + e.Message);
                    }
                    break;
                case "deleteDir":
                    try
                    {
                        fsService.deleteDir(clientUser.username, 1, request.path);
                        request.message = "Directory eliminata correttamente";
                    }
                    catch (Exception e)
                    {
                        request.message = "impossibile eliminare la cartella: " + e.Message;
                        MyConsole.Write("impossibile eliminare la cartella: " + e.Message);
                    }
                    break;

            }
            return request;
        }
    
    }
}
