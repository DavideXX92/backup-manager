using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace PDSserver
{
    class FileSystemServiceImpl : FileSystemService
    {
        private DirDao dirDao;
        private FileDao fileDao;
        private HashDao hashDao;
        private UserService userService;

        public FileSystemServiceImpl()
        {
            dirDao = new DirDaoImpl();
            fileDao = new FileDaoImpl();
            hashDao = new HashDaoImpl();
            userService = new UserServiceImpl();
        }

        /*FILE OPERATIONS*/
        public bool checkIfFileExists(string username, int version, string path)
        {
            int idUser = userService.getIdByUsername(username);
            int idFile = fileDao.checkIfFileExists(idUser, version, path);
            if (idFile != -1)
                return true;
            else
                return false;
        }
        public void addFile(string username, int version, File file)
        {
            if(checkIfFileExists(username, version, file.path))
            {
                Console.WriteLine("FILE: " + file.path + " già presente sul db, lo ignoro");
                return;
            }
            int idUser = userService.getIdByUsername(username);
            string pathWithoutFile = Path.GetDirectoryName(file.path);       
            int idDir = dirDao.checkIfPathExists(idUser, version, pathWithoutFile);
            try
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    if (idDir == -1)
                        idDir = dirDao.createDirFromPath(idUser, version, pathWithoutFile);
                    fileDao.saveFile(file.name, file.size, file.hash, file.extension, idDir, idUser, version, file.creationTime, file.lastWriteTime);
                    scope.Complete();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("impossibile creare il file");
                throw e;
            }
            Console.WriteLine("FILE: " + file.path + " Added");
            MyConsole.Append("FILE: " + file.path + " Added");
        }
        public void renameFile(string username, int version, string oldPath, string newPath)
        {
                int idUser = userService.getIdByUsername(username);
                int idFile = fileDao.checkIfFileExists(idUser, version, oldPath);
                if (idFile == -1)
                    throw new Exception("Il file da rinominare non esiste");
                else
                {
                    try
                    {
                        string newName = newPath.Substring(newPath.LastIndexOf(@"\")).Substring(1);
                        fileDao.renameFile(idUser, version, idFile, newName);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("impossibile rinominare il file");
                        throw e;
                    }
                    Console.WriteLine("FILE: " + oldPath + " renamed to: " + newPath);
                    MyConsole.Append("FILE: " + oldPath + " renamed to: " + newPath);
                }
        }
        public void updateFile(string username, int version, File file)
        {
            int idUser = userService.getIdByUsername(username);
            int idFile = fileDao.checkIfFileExists(idUser, version, file.path);
            if (idFile == -1)
                throw new Exception("Il file da modificare non esiste");
            else
            {
                try
                {
                    fileDao.updateFile(idUser, version, idFile, file.hash, file.size, file.lastWriteTime);
                }
                catch (Exception e)
                {
                    Console.WriteLine("impossibile modificare il file");
                    throw e;
                }
                Console.WriteLine("FILE: " + file.path + " Updated");
                MyConsole.Append("FILE: " + file.path + " Updated");
            }
        }
        public void deleteFile(string username, int version, string path)
        {
            int idUser = userService.getIdByUsername(username);
            int idFile = fileDao.checkIfFileExists(idUser, version, path);
            if (idFile == -1)
                throw new Exception("Il file da eliminare non esiste");
            else
            {
                try
                {
                    fileDao.deleteFile(idUser, version, idFile);
                }
                catch (Exception e)
                {
                    Console.WriteLine("impossibile eliminare il file");
                    throw e;
                }
                Console.WriteLine("FILE: " + path + " Deleted");
                MyConsole.Append("FILE: " + path + " Deleted");
            }
        }
        public void setFileAsReceived(string username, string hash)
        {
            try
            {
                int idUser = userService.getIdByUsername(username);
                hashDao.changeHashAsReceived(hash, idUser);
            }
            catch (Exception e)
            {
                Console.WriteLine("impossibile settare il file come ricevuto");
                throw e;
            }
        }
        public List<string> getAllHashToBeingReceived(string username)
        {
            try
            {
                int idUser = userService.getIdByUsername(username);
                return hashDao.getAllHashToBeingReceived(idUser);
            }
            catch (Exception e)
            {
                Console.WriteLine("impossibile ricevere la lista degli hash mancanti");
                throw e;
            }
        }
        public int cleaner(string serverDirRoot, string username)
        {
            try
            {
                int count = 0;
                int idUser = userService.getIdByUsername(username);
                List<string> hashToRemove = hashDao.getAllHashToRemove(idUser);
                foreach (string hash in hashToRemove)
                {
                    string pathFile = serverDirRoot + hash;
                    try
                    {
                        System.IO.File.Delete(pathFile);
                        hashDao.deleteHash(hash, idUser);
                        count++;
                        Console.WriteLine("Cleaner: eliminato il file: " + pathFile);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Cleaner: impossibile eliminare il file: " + pathFile);
                        Console.WriteLine(e.Message);
                    }
                }
                return count;
            }
            catch (Exception e)
            {
                Console.WriteLine("impossibile eseguire il cleaner: " + e.Message);
                throw e;
            }
        }

        /*DIR OPERATIONS*/
        public bool checkIfDirExists(string username, int version, string path)
        {
            int idUser = userService.getIdByUsername(username);
            int idDir = dirDao.checkIfPathExists(idUser, version, path);
            if (idDir != -1)
                return true;
            else
                return false;
        }
        public void addDir(string username, int version, Dir dir)
        {
            try
            {
                int idUser = userService.getIdByUsername(username);
                int idDir = dirDao.checkIfPathExists(idUser, version, dir.path);
                if (idDir == -1)
                    idDir = dirDao.createDirFromPath(idUser, version, dir.path);
            }
            catch (Exception e)
            {
                Console.WriteLine("impossibile creare la directory");
                throw e;
            }
            Console.WriteLine("DIR: " + dir.path + " Added");
            MyConsole.Append("DIR: " + dir.path + " Added");
        }
        public void renameDir(string username, int version, string oldPath, string newPath)
        {
            int idUser = userService.getIdByUsername(username);
            int idDir = dirDao.checkIfPathExists(idUser, version, oldPath);
            if (idDir == -1)
                throw new Exception("la cartella da rinominare non esiste");
            else
            {
                try
                {
                    string newName = newPath.Substring(newPath.LastIndexOf(@"\"));
                    dirDao.renameDirectory(idUser, version, idDir, newName);
                }
                catch (Exception e)
                {
                    Console.WriteLine("impossibile rinominare la cartella");
                    throw e;
                }
                Console.WriteLine("DIR: " + oldPath + " renamed to: " + newPath);
                MyConsole.Append("DIR: " + oldPath + " renamed to: " + newPath);
            }
        }
        public void updateDir(string username, int version, Dir dir)
        {
            int idUser = userService.getIdByUsername(username);
            int idDir = dirDao.checkIfPathExists(idUser, version, dir.path);
            if (idDir == -1)
                throw new Exception("la cartella da aggiornare non esiste");
            else
            {
                try
                {
                    dirDao.updateDirectory(idUser, version, idDir, dir.lastWriteTime);
                }
                catch (Exception e)
                {
                    Console.WriteLine("impossibile aggiornare la cartella");
                    throw e;
                }
                Console.WriteLine("DIR: " + dir.path + " Updated");
                MyConsole.Append("DIR: " + dir.path + " Updated");
            }
        }
        public void deleteDir(string username, int version, string path)
        {
            int idUser = userService.getIdByUsername(username);
            int idDir = dirDao.checkIfPathExists(idUser, version, path);
            if (idDir == -1)
                throw new Exception("la cartella da eliminare non esiste");
            else
            {
                try
                {
                    dirDao.deleteDirectory(idUser, version, idDir);
                }
                catch (Exception e)
                {
                    Console.WriteLine("impossibile eliminare la cartella");
                    throw e;
                }
                Console.WriteLine("DIR: " + path + " Deleted");
                MyConsole.Append("DIR: " + path + " Deleted");
            }
        }
    }
}
