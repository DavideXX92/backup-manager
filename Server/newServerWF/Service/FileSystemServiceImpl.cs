using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace newServerWF
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
            try
            {
                int idUser = userService.getIdByUsername(username);
                string pathWithoutFile = Path.GetDirectoryName(file.path);
                int idDir = dirDao.checkIfPathExists(idUser, version, pathWithoutFile);
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
            
        }
        public void renameFile(string username, int version, string oldPath, string newPath)
        {
            try
            {
                int idUser = userService.getIdByUsername(username);
                int idFile = fileDao.checkIfFileExists(idUser, version, oldPath);
                if (idFile == -1)
                    throw new Exception("Il file da rinominare non esiste");
                else
                {
                    string newName = newPath.Substring(newPath.LastIndexOf(@"\")).Substring(1);
                    fileDao.renameFile(idUser, version, idFile, newName);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("impossibile rinominare il file");
                throw e;
            }
        }
        public void updateFile(string username, int version, File file)
        {
            try
            {
                int idUser = userService.getIdByUsername(username);
                int idFile = fileDao.checkIfFileExists(idUser, version, file.path);
                if (idFile == -1)
                    throw new Exception("Il file da modificare non esiste");
                else
                    fileDao.updateFile(idUser, version, idFile, file.hash, file.size, file.lastWriteTime);
            }
            catch (Exception e)
            {
                Console.WriteLine("impossibile modificare il file");
                throw e;
            }
        }
        public void deleteFile(string username, int version, string path)
        {
            try
            {
                int idUser = userService.getIdByUsername(username);
                int idFile = fileDao.checkIfFileExists(idUser, version, path);
                if (idFile == -1)
                    throw new Exception("Il file da eliminare non esiste");
                else
                    fileDao.deleteFile(idUser, version, idFile);        
            }
            catch (Exception e)
            {
                Console.WriteLine("impossibile rinominare il file");
                throw e;
            }
        }
        public void setFileAsReceived(string username, string hash)
        {
            try
            {
                int idUser = userService.getIdByUsername(username);
                hashDao.changeHashAsReceived(hash, idUser);
            }catch(Exception e)
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
            }catch (Exception e)
            {
                Console.WriteLine("impossibile ricevere la lista degli hash mancanti");
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
                else
                    throw new Exception("La cartella è già presente, non verrà creata");
            }
            catch (Exception e)
            {
                Console.WriteLine("impossibile creare la directory");
                throw e;
            }
        }
        public void renameDir(string username, int version, string oldPath, string newPath)
        {
            try
            {
                int idUser = userService.getIdByUsername(username);
                int idDir = dirDao.checkIfPathExists(idUser, version, oldPath);
                if (idDir == -1)
                    throw new Exception("la cartella da rinominare non esiste");
                else
                {
                    string newName = newPath.Substring(newPath.LastIndexOf(@"\"));
                    dirDao.renameDirectory(idUser, version, idDir, newName);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("impossibile rinominare la cartella");
                throw e;
            }
        }
        public void updateDir(string username, int version, Dir dir)
        {
            try
            {
                int idUser = userService.getIdByUsername(username);
                int idDir = dirDao.checkIfPathExists(idUser, version, dir.path);
                if (idDir == -1)
                    throw new Exception("la cartella da aggiornare non esiste");
                else
                    dirDao.updateDirectory(idUser, version, idDir, dir.lastWriteTime);
            }
            catch (Exception e)
            {
                Console.WriteLine("impossibile aggiornare la cartella");
                throw e;
            }
        }
        public void deleteDir(string username, int version, string path)
        {
            try
            {
                int idUser = userService.getIdByUsername(username);
                int idDir = dirDao.checkIfPathExists(idUser, version, path);
                if (idDir == -1)
                    throw new Exception("la cartella da eliminare non esiste");
                else
                {
                    dirDao.deleteDirectory(idUser, version, idDir);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("impossibile eliminare la cartella");
                throw e;
            }
        }
    }
}
