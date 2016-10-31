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
        List<User> getUsers();
        User getUser(string username);
        User saveUser(User user);
        void updateUser(User user);
        bool checkIfCredentialsAreCorrected(string username, string password);
        bool checkIfUsernameExists(string username);
        void addMonitorDir(string username, string path);
        void deleteMonitorDir(string username, string path);
        void createDirOfUser(string path);
    }
}
