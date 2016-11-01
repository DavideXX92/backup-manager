using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDSserver
{
    class UserServiceImpl : UserService
    {
        private UserDao userDao;
        private MonitorDirDao monitorDirDao;

        public UserServiceImpl()
        {
            this.userDao = new UserDaoImpl();
            this.monitorDirDao = new MonitorDirDaoImpl();
        }

        public int getIdByUsername(string username)
        {
            if (userDao.exists(username))
                    return userDao.getIdByUsername(username);
            else
                throw new Exception("username not found");
        }
        public List<User> getUsers()
        {
            return userDao.getUsers();
        }
        public User getUser(string username)
        {
            if (userDao.exists(username)){
                User user = userDao.findOne(username);
                user.monitorDir = monitorDirDao.getMonitorDirsByIdUser(user.idUser);
                return user;
            }
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
        public void updateUser(User user)
        {
            userDao.update(user);
        }
        public bool checkIfCredentialsAreCorrected(string username, string password)
        {
            return userDao.checkIfCredentialsAreCorrected(username, password);
        }
        public bool checkIfUsernameExists(string username)
        {
            return userDao.exists(username);
        }
        public void addMonitorDir(string username, string path)
        {
            int idUser = userDao.getIdByUsername(username);
            try
            {
                monitorDirDao.addMonitorDir(path, idUser);
            }
            catch (Exception e)
            {
                Console.WriteLine("Impossibile aggiungere la monitorDir");
                throw;
            }
        }
        public void deleteMonitorDir(string username, string path)
        {
            int idUser = userDao.getIdByUsername(username);
            try
            {
                monitorDirDao.deleteMonitorDir(path, idUser);
            }
            catch (Exception e)
            {
                Console.WriteLine("Impossibile eliminare la monitorDir");
                throw;
            }
        }
        public void createDirOfUser(string path)
        {
            try
            {
                Directory.CreateDirectory(path);
            }catch(Exception e)
            {
                Console.WriteLine("Impossibile creare la cartella sul server");
                throw;
            }
        }
    }
}
