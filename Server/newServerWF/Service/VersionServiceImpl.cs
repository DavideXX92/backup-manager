using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace newServerWF
{
    class VersionServiceImpl : VersionService
    {
        public Version saveVersion(Dir dirTree, string username)
        {
            UserDao userDao = new UserDaoImpl();
            VersionDao versionDao = new VersionDaoImpl();
            DirDao dirDao = new DirDaoImpl();   
            try
            {
                int idUser = userDao.getIdByUsername(username);
                int idVersion;
                using (TransactionScope scope = new TransactionScope())
                {
                    idVersion = versionDao.addVersion(idUser);
                    int idRootDir = dirDao.saveDirectory(dirTree.name, -1, idUser, idVersion, dirTree.creationTime, dirTree.lastWriteTime);
                    saveTreeRecursive(dirTree, idRootDir, idUser, idVersion);
                    scope.Complete();
                }
                Version version = versionDao.getVersionInfo(idUser, idVersion);
                version.dirTree = dirTree;
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

    }
}
