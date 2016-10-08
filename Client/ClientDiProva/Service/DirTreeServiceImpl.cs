using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientDiProva
{
    class DirTreeServiceImpl : DirTreeService
    {
        public Dir makeDirTree(Dir rootDir)
        {
            makeDirTreeRecursive(rootDir, rootDir.path);
            return rootDir;
        }
        public void createDirTree(Dir dirTree, string pathDst)
        {
            createDirTreeRecursive(dirTree, pathDst);
        }
        public Dictionary<string, Dir> getAllDirIntoAmap(Dir dirTree){
            Dictionary<string, Dir> dirMap = new Dictionary<string, Dir>();
            getAllSubDirOfDirRecursive(dirTree, dirMap);
            return dirMap;
        }
        public Dictionary<string, File> getAllFileIntoAmap(Dir dirTree)
        {
            Dictionary<string, File> fileMap = new Dictionary<string, File>();
            getAllFilesOfDirRecursive(dirTree, fileMap);
            return fileMap;
        }
        public List<File> getAllFileIntoAlist(Dir dirTree)
        {
            List<File> elencoFile = new List<File>();
            getAllFilesOfDirRecursive(dirTree, elencoFile);
            return elencoFile;
        }
        public bool areEqual(Dir dirTreeServer, Dir dirTreeClient)
        {         
            List<File> fileDTS = getAllFileIntoAlist(dirTreeServer);
            Dictionary<string, List<File>> fileDTC = getAllFileIntoAmapList(dirTreeClient);

            Dictionary<string, Dir> dirDTS = getAllDirIntoAmap(dirTreeServer);
            Dictionary<string, Dir> dirDTC = getAllDirIntoAmap(dirTreeClient);

            //Count the number of file from the dirTreeClient
            int nFileDTC = 0;
            foreach (List<File> list in fileDTC.Values)
                nFileDTC += list.Count;

            //Check if the number of file are the same in both versions
            if (fileDTS.Count != nFileDTC)
                return false;

            //Check if the number of dir are the same in both versions
            if (dirDTS.Count != dirDTC.Count)
                return false;

            //Compare each file between client and server
            //Each file in dirTreeServer must be found in the client with the same hash and path
            foreach (File file in fileDTS)              //for each file in the server
            {
                if (!fileDTC.ContainsKey(file.hash))    //we check in the client if exists a file with that hash
                    return false;
                else
                {
                    bool res = false;
                    foreach (File tmp in fileDTC[file.hash]) //more than one file could be have the same hash
                    {
                        if (tmp.relativePath.Equals(file.path)) //so we check if there is one with the the same path too
                        {
                            res = true;                      //in that case we are sure that a file in the server
                            break;                           //is contained also in the client in the same path
                        }
                    }
                    if (res == false)
                        return false;
                }                                            //if at least a file is not contained in the client
            }                                                //the version are not equal!

            //Compare each dir between client and server
            //Each dir in dirTreeServer must be found in the client with the same path
            foreach (Dir dir in dirDTS.Values)              
                if (!dirDTC.ContainsKey(dir.relativePath))           
                    return false;

            return true;                                     //DirTrees are equal!

        }

        private void makeDirTreeRecursive(Dir targetDir, string monitorDir)
        {
            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(targetDir.path);
            foreach (string fileName in fileEntries)
                targetDir.elencoFile.Add(new File(fileName, targetDir, monitorDir));

            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = Directory.GetDirectories(targetDir.path);
            foreach (string subdirectory in subdirectoryEntries)
            {
                Dir subDir = new Dir(subdirectory, targetDir, monitorDir);
                targetDir.elencoSubdirectory.Add(subDir);
                makeDirTreeRecursive(subDir, monitorDir);
            }
        }
        private void createDirTreeRecursive(Dir targetDir, string rootPath)
        {
            string dirPath = rootPath + @"\" + targetDir.path;
            Directory.CreateDirectory(dirPath);
            foreach (Dir dir in targetDir.elencoSubdirectory)
                createDirTreeRecursive(dir, rootPath);
        }
        private void getAllSubDirOfDirRecursive(Dir targetDir, Dictionary<string, Dir> dirMap)
        {
            foreach (Dir dir in targetDir.elencoSubdirectory)
                dirMap[dir.relativePath] = dir;
            foreach (Dir dir in targetDir.elencoSubdirectory)
                getAllSubDirOfDirRecursive(dir, dirMap);
        }
        private Dictionary<string, List<File>> getAllFileIntoAmapList(Dir dirTree)
        {
            Dictionary<string, List<File>> dirMap = new Dictionary<string, List<File>>();
            getAllFilesOfDirRecursive(dirTree, dirMap);
            return dirMap;
        }
        private void getAllFilesOfDirRecursive(Dir targetDir, Dictionary<string, List<File>> fileMap)
        {
            foreach (File file in targetDir.elencoFile)
            {
                if (fileMap.ContainsKey(file.hash))
                    fileMap[file.hash].Add(file);
                else
                {
                    fileMap[file.hash] = new List<File>();
                    fileMap[file.hash].Add(file);
                }
            }
            foreach (Dir dir in targetDir.elencoSubdirectory)
                getAllFilesOfDirRecursive(dir, fileMap);
        }
        private void getAllFilesOfDirRecursive(Dir targetDir, Dictionary<string, File> fileMap)
        {
            foreach (File file in targetDir.elencoFile)
                fileMap[file.hash] = file;
            foreach (Dir dir in targetDir.elencoSubdirectory)
                getAllFilesOfDirRecursive(dir, fileMap);
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
