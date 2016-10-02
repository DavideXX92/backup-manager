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
            makeDirTreeRecursive(rootDir);
            return rootDir;
        }
        public void createDirTree(Dir dirTree, string pathDst)
        {
            createDirTreeRecursive(dirTree, pathDst);
        }
        public Dictionary<string, File> getAllFileIntoAmap(Dir dirTree)
        {
            Dictionary<string, File> fileMap = new Dictionary<string, File>();
            getAllFilesOfDirRecursive(dirTree, fileMap);
            return fileMap;
        }

        private void makeDirTreeRecursive(Dir targetDir)
        {
            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(targetDir.path);
            foreach (string fileName in fileEntries)
                targetDir.elencoFile.Add(new File(fileName, targetDir));

            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = Directory.GetDirectories(targetDir.path);
            foreach (string subdirectory in subdirectoryEntries)
            {
                Dir subDir = new Dir(subdirectory, targetDir);
                targetDir.elencoSubdirectory.Add(subDir);
                makeDirTreeRecursive(subDir);
            }
        }
        private void createDirTreeRecursive(Dir targetDir, string rootPath)
        {
            string dirPath = rootPath + @"\" + targetDir.path;
            Directory.CreateDirectory(dirPath);
            foreach (Dir dir in targetDir.elencoSubdirectory)
                createDirTreeRecursive(dir, rootPath);
        }
        private void getAllFilesOfDirRecursive(Dir targetDir, Dictionary<string, File> fileMap)
        {
            foreach (File file in targetDir.elencoFile)
                fileMap[file.hash] = file;
            foreach (Dir dir in targetDir.elencoSubdirectory)
                getAllFilesOfDirRecursive(dir, fileMap);
        }
    }
}
