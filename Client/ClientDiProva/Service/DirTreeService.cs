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
        Dictionary<string, File> getAllFileIntoAmap(Dir dirTree);
    }
}
