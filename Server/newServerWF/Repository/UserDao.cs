using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newServerWF
{
    interface UserDao
    {
        bool exists(string username);
        int getIdByUsername(string username);
        User findOne(string username);
        User save(User user);
        void update(User user);
        bool checkIfCredentialsAreCorrected(string username, string password);
        
        
    }
}
