using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace newServerWF
{
    class HandleClient
    {
        private TcpClient client;
        private HandlePackets clientConn;
        private Dizionario dizionario; //Class to associate messages with functions

        private User clientUser;
        private string pathDir = "C:/ricevuti/";
        private string clientDir;

        private List<File> filesToAdd;
        private List<File> filesToUpdate;
        private List<File> filesToRemove;

        public HandleClient(TcpClient client){
            this.client = client;
            this.clientUser = null;
            this.clientDir = null;
            //this.clientDir = "C:/ricevuti/user1/";
            dizionario = new Dizionario(this);
            filesToAdd = new List<File>();
            filesToUpdate = new List<File>();
            filesToRemove = new List<File>();
        }

        public HandleClient start()
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

        public Login handleLogin(Login loginRequest)
        {
            User user = loginRequest.user;
            Console.WriteLine("L'utente " + user.username + " sta tentando di autenticarsi.");
            MyConsole.write("User: " + user.username);
            MyConsole.write("Pass: " + user.password);
            UserDao userDao = new UserDaoImpl();
            Login loginResponse = new Login(user);
            if (userDao.findUser(user))
            {
                Console.WriteLine(user.username + ": autenticazione riuscita");
                loginResponse.user = user;
                loginResponse.message = user.username + "autenticazione riuscita";
                loginResponse.isLogged = true;
                clientUser = user;
                clientDir = pathDir + user.username + '/';
            }
            else
            {
                Console.WriteLine(user.username + ": impossibile autenticarsi");
                loginResponse.error = user.username + ": impossibile autenticarsi";
            }
            return loginResponse;
        }

        public Register handleRegister(Register registerRequest)
        {
            User user = registerRequest.user;
            UserDao userDao = new UserDaoImpl();
            Register registerResponse = new Register(user);
            if (userDao.findUser(user))
            {
                Console.WriteLine("Impossibile registrare l'utente: " + user.username + " username già esistente");
                registerResponse.error = "Impossibile registrarsi: username già esistente";
            }
            else
            {
                try
                {
                    userDao.addUser(user);
                    DBConnect dbConnect = new DBConnect();
                    dbConnect.createTableBackup(user.username);
                    registerResponse.isRegistred = true;
                    registerResponse.message = "Registrazione avvenuta con successo";
                }catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                    registerResponse.error = "Impossibile registrarsi: problemi col server";
                }
            }
            return registerResponse;

        }

        public List<File> handleSynchronizeRequest(List<File> filesFromClient)
        {
            try
            {
                FileDao fileDao = new FileDaoImpl();
                Dictionary<string,File> filesFromDB = fileDao.getAllFiles(clientUser);
                List<File> filesToRequest = new List<File>();
                foreach(File file in filesFromClient){
                    if (filesFromDB.ContainsKey(file.hash))
                    {
                        File f = filesFromDB[file.hash];
                        if (!f.path.Equals(file.path))
                        {
                            filesToUpdate.Add(f);
                            Console.WriteLine("File add into updateList");
                        }
                    }
                    else
                        filesToRequest.Add(file);
                }
                return filesToRequest;
            }catch(Exception e){
                Console.WriteLine(e.Message);
                return null;
            }
            
        }

        public WrapFile prepareReceiptOfFile(File file)
        {
            //Compute
            try
            {
                string path = clientDir;
                string tmp;
                tmp = Path.GetFileNameWithoutExtension(file.name);
                tmp += "RCV";
                tmp += Path.GetExtension(file.name);
                file.name = tmp;
                path += tmp;

                WrapFile wrapFile = new WrapFile(file, -1, new FileStream(path, FileMode.Create, FileAccess.Write));
                return wrapFile;
            }
            catch (Exception e)
            {
                MyConsole.write(e.Message);
                return null;
            }
            
        }

        public WrapFile fileReceived(WrapFile wrapFile)
        {
            //Save into db
            File file = wrapFile.file;
            filesToAdd.Add(file);
            Console.WriteLine("File add into addList");

            return wrapFile;
        }

        public WrapFile handleRequestOfFile(File file)
        {
            //Compute
            String name = file.name;
            String path = clientDir + name;
            FileInfo fileinfo = new FileInfo(path);
            file.size = (int)fileinfo.Length;
            WrapFile wrapFile = new WrapFile(file, file.size, new FileStream(path, FileMode.Open, FileAccess.Read));
            return wrapFile;
        }

        public GenericReq completeSynchronization(GenericReq request)
        {
            //begin transaction
            FileDao fileDao = new FileDaoImpl();
            foreach (File file in filesToAdd)
                fileDao.addFile(file, clientUser);
            foreach(File file in filesToUpdate)
                fileDao.updateFile(file, clientUser);
            foreach (File file in filesToRemove)
                fileDao.deleteFile(file, clientUser);
            //----
            //commit
            filesToAdd = null;
            filesToUpdate = null;
            filesToRemove = null;
            request.message = "Sincronizzazione completata con successo!";
            return request;
        }

    }
}