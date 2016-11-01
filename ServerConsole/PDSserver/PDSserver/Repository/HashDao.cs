using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDSserver
{
    interface HashDao
    {
        int saveHash(string hash, int idUser);
        void deleteHash(string hash, int idUser);
        int checkIfHashExists(string hash, int idUser);
        void changeCounterOfHash(int idHash, int idUser, int op);
        void changeHashAsReceived(string hash, int idUser);
        List<string> getAllHashToBeingReceived(int idUser);
        List<string> getAllHashToRemove(int idUser);
    }
}
