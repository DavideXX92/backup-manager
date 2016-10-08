using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientDiProva
{
    interface DirTreeService
    {
        Dir makeDirTree(Dir rootDir);
        void createDirTree(Dir dirTree, string pathDst);
        Dictionary<string, Dir> getAllDirIntoAmap(Dir dirTree);
        Dictionary<string, File> getAllFileIntoAmap(Dir dirTree);
        List<File> getAllFileIntoAlist(Dir dirTree);
        bool areEqual(Dir dirTreeServer, Dir dirTreeClient);
    }
}
