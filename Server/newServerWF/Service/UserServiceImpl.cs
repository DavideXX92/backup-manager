using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newServerWF
{
    class UserServiceImpl : UserService
    {
        private UserDao userDao;

        public UserServiceImpl()
        {
            this.userDao = new UserDaoImpl();
        }

        public int getIdByUsername(string username)
        {
            if (userDao.exists(username))
                    return userDao.getIdByUsername(username);
            else
                throw new Exception("username not found");
        }
        public User getUser(string username)
        {
            if (userDao.exists(username))
                return userDao.findOne(username);
            else
                throw new Exception("username not found");
        }
        public User saveUser(User user)
        {
            try
            {
                return userDao.save(user);
            }catch(Exception e)
            {
                Console.WriteLine("Impossibile salvare l'utente");
                throw;
            }
        }
        public bool checkIfCredentialsAreCorrected(string username, string password)
        {
            return userDao.checkIfCredentialsAreCorrected(username, password);
        }
        public bool checkIfUsernameExists(string username)
        {
            return userDao.exists(username);
        }
    }
}
