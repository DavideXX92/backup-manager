using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newServerWF
{
    interface UserDao
    {
        List<User> getAllUsers();
        bool findUser(User user);
        void addUser(User user);
        void updateUser(User user);
        void deleteUser(User user);
    }
}
