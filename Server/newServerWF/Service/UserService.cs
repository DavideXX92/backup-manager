using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newServerWF
{
    interface UserService
    {
        int getIdByUsername(string username);
        User getUser(string username);
        User saveUser(User user);
        void updateUser(User user);
        bool checkIfCredentialsAreCorrected(string username, string password);
        bool checkIfUsernameExists(string username);
    }
}
