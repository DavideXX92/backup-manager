using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace newServerWF
{
    class HandleClientImpl : HandleClient
    {
        private TcpClient client;
        private HandlePackets clientConn;
        private Dizionario dizionario; //Class to associate messages with functions

        private User clientUser;
        private bool isLogged;
        private string serverDirRoot = @"c:\ServerDir\";
        private string clientDir;

        public HandleClientImpl(TcpClient client)
        {
            this.client = client;
            this.isLogged = false;
            this.clientUser = null;
            this.clientDir = null;
            this.dizionario = new Dizionario(this);
        }

        public HandleClient startLoop()
        {
            try
            {
                clientConn = new HandlePackets(client, dizionario.getDelegate());
                clientConn.startListen();
            }
            catch (Exception e)
            {
                MyConsole.write("C'è stato un errore, chiudo la connessione con il client... dovrei vedere se riesco a ripristinarla piuttosto che chiuderla");
            }
            return this;
        }
        public void stop()
        { 
            clientConn.stopListen(); 
        }

        public GenericRequest closeConnectionWithTheClient(GenericRequest request)
        {
            try
            {
                clientConn.stopListen();
                request.message = "Client disconnesso correttamente";
                return request;
            }
            catch (Exception e)
            {
                request.error = "Errore durante la disconessione: " + e.Message;
                return request;
            }
        }
        public Register handleRegistration(Register registerRequest)
        {
            User user = registerRequest.user;
            UserService userService = new UserServiceImpl();
            Register registerResponse = new Register(user);
            if (userService.checkIfUsernameExists(user.username))
            {
                Console.WriteLine("Impossibile registrare l'utente: " + user.username + " username già esistente");
                registerResponse.error = "Impossibile registrarsi: username già esistente";
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
                    registerResponse.error = "Impossibile registrarsi: problemi col server";
                }
            }
            return registerResponse;
        }
        public Login handleLogin(Login loginRequest)
        {
            User user = loginRequest.user;
            Console.WriteLine("L'utente " + user.username + " sta tentando di autenticarsi.");
            MyConsole.write("User: " + user.username);
            MyConsole.write("Pass: " + user.password);
            UserService userService = new UserServiceImpl();
            Login loginResponse = new Login(user);
            if (userService.checkIfCredentialsAreCorrected(user.username, user.password))
            {
                clientUser = userService.getUser(user.username);
                Console.WriteLine(user.username + ": autenticazione riuscita");
                loginResponse.user = clientUser;
                loginResponse.message = user.username + " autenticazione riuscita";
                loginResponse.isLogged = true;
                clientDir = serverDirRoot + user.username + @"\";
                isLogged = true;
            }
            else
            {
                Console.WriteLine(user.username + ": impossibile autenticarsi");
                loginResponse.error = user.username + ": impossibile autenticarsi";
            }
            return loginResponse;
        } 
        public CreateVersion createNewVersion(CreateVersion request)
        {
            VersionService versionService = new VersionServiceImpl();
            FileSystemService fsService = new FileSystemServiceImpl();
            try
            {
                request.version = versionService.saveVersion(request.version.dirTree, clientUser.username);
            }catch(Exception e)
            {
                request.error = "Impossibile creare la versione: " + e.Message;
                return request;
            }
            try{
                request.elencoHash = fsService.getAllHashToBeingReceived(clientUser.username);
                request.message = "Versione creata e lista degli hash ricevuti";
            }catch (Exception e)
            {
                request.error = "La versione e' stata creata, ma la lista degli hash mancanti non e' stata ricevuta: " + e.Message;
            }
            return request;
        }
        public UpdateVersion updateVersion(UpdateVersion request)
        {
            VersionService versionService = new VersionServiceImpl();
            FileSystemService fsService = new FileSystemServiceImpl();
            try
            {
                int idVersion = versionService.getCurrentVersionID(clientUser.username);
                versionService.updateVersion(clientUser.username, idVersion, request);
                request.message = "versione aggiornata";
            }
            catch (Exception e)
            {
                request.error = "impossibile aggiornare la versione";
                Console.WriteLine("impossibile aggiornare la versione");
                return request;
            }
            try
            {
                request.elencoHash = fsService.getAllHashToBeingReceived(clientUser.username);
                request.message = "Versione aggiornata e lista degli hash ricevuti";
            }
            catch (Exception e)
            {
                request.error = "La versione e' stata aggiornata, ma la lista degli hash mancanti non e' stata ricevuta: " + e.Message;
            }
            return request;
        }
        public CloseVersion closeVersion(CloseVersion request)
        {
            VersionService versionService = new VersionServiceImpl();
            try
            {
                versionService.closeVersion(clientUser.username, request.version.idVersion);
                request.message = "versione chiusa";
            }
            catch (Exception e)
            {
                request.error = "impossibile chiudere la versione";
                Console.WriteLine("impossibile chiudere la versione");
            }
            return request;
        }
        public RestoreVersion restoreVersion(RestoreVersion request)
        {
            VersionService versionService = new VersionServiceImpl();
            try
            {
                request.version = versionService.getVersion(clientUser.username, request.version.idVersion);
                request.elencoFile = versionService.getAllFileIntoAlist(request.version);
                request.message = "Versione recuperata";
            }
            catch (Exception e)
            {
                request.error = "Impossibile recuperare la versione: " + e.Message;
            }
            return request;
        }
        public StoredVersions sendStoredVersions(StoredVersions request)
        {
            VersionService versionService = new VersionServiceImpl();
            try
            {
                request.storedVersions = versionService.getAllVersionsOfaUser(clientUser.username);
                request.message = "Versioni recuperate";
            }
            catch(Exception e)
            {
                request.error = "Impossibile recuperare le versioni";
            }
            return request;
        }
        public WrapFile handleRequestOfFile(File file)
        {
            String pathSrc = clientDir + file.hash + file.extension;
            FileInfo fileinfo = new FileInfo(pathSrc);
            file.size = (int)fileinfo.Length;
            WrapFile wrapFile = new WrapFile(file, file.size, new FileStream(pathSrc, FileMode.Open, FileAccess.Read));
            return wrapFile;
        }
        public WrapFile initializeReceiptOfFile(File file)
        {
            string pathDst = clientDir + file.hash + file.extension;
            WrapFile wrapFile = new WrapFile(file, -1, new FileStream(pathDst, FileMode.Create, FileAccess.Write));
            return wrapFile;
        }
        public WrapFile fileReceived(WrapFile wrapFile)
        {
            //Update the db
            File file = wrapFile.file;
            FileSystemService fsService = new FileSystemServiceImpl();
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
            FileSystemService fsService = new FileSystemServiceImpl();
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
                        MyConsole.write("impossibile creare il file: " + e.Message);
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
                        MyConsole.write("impossibile modificare il file: " + e.Message);
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
                        MyConsole.write("impossibile rinominare il file: " + e.Message);
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
                        MyConsole.write("impossibile eliminare il file: " + e.Message);
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
                        MyConsole.write("impossibile creare la directory: " + e.Message);
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
                        MyConsole.write("impossibile aggiornare la directory: " + e.Message);
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
                        MyConsole.write("impossibile rinominare la cartella: " + e.Message);
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
                        MyConsole.write("impossibile eliminare la cartella: " + e.Message);
                    }
                    break;

            }
            return request;
        }
    }
}
