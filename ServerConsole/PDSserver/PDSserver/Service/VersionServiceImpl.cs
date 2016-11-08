using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace PDSserver
{
    class VersionServiceImpl : VersionService
    {
        private delegate int cleanerDelegate(string serverDirRoot, string username);

        public Version saveVersion(Dir dirTree, string username, string clientDir)
        {
            UserDao userDao = new UserDaoImpl();
            VersionDao versionDao = new VersionDaoImpl();
            DirDao dirDao = new DirDaoImpl();   
            try
            {
                int idUser = userDao.getIdByUsername(username);
                int idVersion;
                DateTime dateCreation = DateTime.Now;
                using (TransactionScope scope = new TransactionScope())
                {
                    idVersion = versionDao.addVersion(idUser, dateCreation);
                    int idRootDir = dirDao.saveDirectory(dirTree.name, -1, idUser, idVersion, dirTree.creationTime, dirTree.lastWriteTime);
                    saveTreeRecursive(dirTree, idRootDir, idUser, idVersion);
                    scope.Complete();
                }
                Version version = new Version(idVersion, dirTree, dateCreation);

                return version;
            }catch(Exception e)
            {
                Console.WriteLine("impossibile creare la nuova versione");
                throw;
            }   
        }
        public Version getVersion(string username, int idVersion)
        {
            UserDao userDao = new UserDaoImpl();
            DirDao dirDao = new DirDaoImpl();
            VersionDao versionDao = new VersionDaoImpl();
            try
            {
                int idUser = userDao.getIdByUsername(username);
                Dir dirTreeFromDB = dirDao.getRootDir(idUser, idVersion);
                makeTreeRecursive(dirTreeFromDB, idUser, idVersion);
                Version version = versionDao.getVersionInfo(idUser, idVersion);
                version.dirTree = dirTreeFromDB;
                return version;
            }
            catch (Exception e)
            {
                Console.WriteLine("impossibile recuperare la versione");
                throw;
            }  
            
        }
        public void updateVersion(string username, int idVersion, UpdateVersion updateVersion)
        {
            FileSystemService fsService = new FileSystemServiceImpl();
            UserService userService = new UserServiceImpl();
            VersionDao versionDao = new VersionDaoImpl();
            MyConsole.Log("Aggiornamento versione");
            try
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    foreach (Operation operation in updateVersion.list)
                    {
                        switch (operation.type)
                        {
                            case "addFile":
                                fsService.addFile(username, idVersion, operation.file);
                                break;
                            case "updateFile":
                                fsService.updateFile(username, idVersion, operation.file);
                                break;
                            case "renameFile":
                                fsService.renameFile(username, idVersion, operation.oldPath, operation.newPath);
                                break;
                            case "deleteFile":
                                fsService.deleteFile(username, idVersion, operation.path);
                                break;
                            case "addDir":
                                fsService.addDir(username, idVersion, operation.dir);
                                break;
                            case "updateDir":
                                fsService.updateDir(username, idVersion, operation.dir);
                                break;
                            case "renameDir":
                                fsService.renameDir(username, idVersion, operation.oldPath, operation.newPath);
                                break;
                            case "deleteDir":
                                fsService.deleteDir(username, idVersion, operation.path);
                                break;
                        }
                        int idUser = userService.getIdByUsername(username);
                        versionDao.refreshLastUpdateDate(idUser, idVersion); 
                    }
                    scope.Complete();
                    MyConsole.LogCommit();
                }
            }catch(Exception e)
            {
                Console.WriteLine("impossibile aggiornare la versione: " + e.Message);
                MyConsole.LogRollback();
                MyConsole.Log("Aggiornamento fallito");
                throw;
            }
        }
        public void closeVersion(string username, int idVersion)
        {
            UserDao userDao = new UserDaoImpl();
            VersionDao versionDao = new VersionDaoImpl();
            try
            {
                int idUser = userDao.getIdByUsername(username);
                versionDao.closeVersion(idUser, idVersion);
            }
            catch (Exception e)
            {
                Console.WriteLine("impossibile chiudere la versione");
                throw;
            } 
        }
        public void deleteVersion(string username, int idVersion, string clientDir)
        {
            UserDao userDao = new UserDaoImpl();
            VersionDao versionDao = new VersionDaoImpl();
            try
            {
                int idUser = userDao.getIdByUsername(username);
                versionDao.deleteVersion(idUser, idVersion);

                //lancia il cleaner
                FileSystemService fsService = new FileSystemServiceImpl();
                fsService.cleaner(clientDir, username);

                //lancia il cleaner in modo asincrono
                //FileSystemService fsService = new FileSystemServiceImpl();
                //cleanerDelegate cleaner = new cleanerDelegate(fsService.cleaner);
                //IAsyncResult result = cleaner.BeginInvoke(clientDir, username, new AsyncCallback(cleanerFinished), cleaner);
            }
            catch (Exception e)
            {
                Console.WriteLine("impossibile eliminare la versione");
                throw;
            } 
        }
        public List<Version> getAllVersionsOfaUser(string username)
        {
            List<Version> versionList = new List<Version>();
            UserDao userDao = new UserDaoImpl();
            VersionDao versionDao = new VersionDaoImpl();
            try
            {
                int idUser = userDao.getIdByUsername(username);
                List<int> idVersionList = versionDao.getAllIdOfVersionsOfaUser(idUser);
                foreach (int idVersion in idVersionList)
                {
                    versionList.Add(getVersion(username, idVersion));
                }
                return versionList;
            }
            catch (Exception e)
            {
                Console.WriteLine("impossibile recuperare le versioni");
                throw;
            }   
            
        }
        public int getCurrentVersionID(string username)
        {
            UserDao userDao = new UserDaoImpl();
            VersionDao versionDao = new VersionDaoImpl();
            try
            {
                int idUser = userDao.getIdByUsername(username);
                return versionDao.getCurrentVersionID(idUser);
            }
            catch (Exception e)
            {
                Console.WriteLine("impossibile recuperare l'id della versione corrente");
                throw;
            }   
        }
        public int getLastClosedVersionID(string username)
        {
            UserDao userDao = new UserDaoImpl();
            VersionDao versionDao = new VersionDaoImpl();
            try
            {
                int idUser = userDao.getIdByUsername(username);
                return versionDao.getLastClosedVersionID(idUser);
            }
            catch (Exception e)
            {
                Console.WriteLine("impossibile recuperare l'id dell'ultima versione chiusa");
                throw;
            }   
        }
        public List<int> getAllIdOfVersions(string username)
        {
            UserDao userDao = new UserDaoImpl();
            VersionDao versionDao = new VersionDaoImpl();
            int idUser = userDao.getIdByUsername(username);
            return versionDao.getAllIdOfVersionsOfaUser(idUser);
        }
        public List<File> getAllFileIntoAlist(Version version)
        {
            List<File> elencoFile = new List<File>();
            getAllFilesOfDirRecursive(version.dirTree, elencoFile);
            return elencoFile;
        }
        
        private void saveTreeRecursive(Dir targetDir, int idDir, int idUser, int idVersion)
        {
            FileDao fileDao = new FileDaoImpl();
            DirDao dirDao = new DirDaoImpl();
            foreach (File file in targetDir.elencoFile)
                fileDao.saveFile(file.name, file.size, file.hash, file.extension, idDir, idUser, idVersion, file.creationTime, file.lastWriteTime);

            foreach (Dir dir in targetDir.elencoSubdirectory)
            {
                int idSubDir = dirDao.saveDirectory(dir.name, idDir, idUser, idVersion, dir.creationTime, dir.lastWriteTime);
                saveTreeRecursive(dir, idSubDir, idUser, idVersion);
            }
        }
        private void makeTreeRecursive(Dir targetDir, int idUser, int idVersion)
        {
            FileDao fileDao = new FileDaoImpl();
            DirDao dirDao = new DirDaoImpl();
            targetDir.elencoFile = fileDao.getAllFilesOfDir(targetDir, idUser, idVersion);
            targetDir.elencoSubdirectory = dirDao.getAllSubDirOfDir(targetDir, idUser, idVersion);
            foreach (Dir dir in targetDir.elencoSubdirectory)
                makeTreeRecursive(dir, idUser, idVersion);
        }
        private void getAllFilesOfDirRecursive(Dir targetDir, List<File> elencoFile)
        {
            foreach (File file in targetDir.elencoFile)
                elencoFile.Add(file);
            foreach (Dir dir in targetDir.elencoSubdirectory)
                getAllFilesOfDirRecursive(dir, elencoFile);
        }

        private void cleanerFinished(IAsyncResult result)
        {
            cleanerDelegate d = (cleanerDelegate)result.AsyncState;
            Console.WriteLine("Cleaner finished, hash deleted: " + d.EndInvoke(result));
        }
    }
}
