using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace newServerWF
{
    class DirDaoImpl : DirDao
    {
        private int getMaxIdDir(int idUser, int idVersion)
        {
            DBConnect dbConnect = new DBConnect();
            MySqlConnection conn = dbConnect.OpenConnection();

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = "SELECT MAX(idDir) FROM directory WHERE idUser=@idUser AND idVersion=@idVersion";
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@idUser", idUser);
            cmd.Parameters.AddWithValue("@idVersion", idVersion);
            MySqlDataReader dataReader = cmd.ExecuteReader();

            int idDir = -1;
            dataReader.Read();
            if (!dataReader.IsDBNull(0))
                idDir = dataReader.GetInt32(0);
            
            dbConnect.CloseConnection();
            return idDir;
        }
        public int checkIfPathExists(int idUser, int idVersion, string path)
        {
            int idDir = -1;
            int idParent;
            string[] dirNames = path.Split('\\');
            Dir rootDir = getRootDir(idUser, idVersion);
            if( rootDir.name.Equals("\\"+dirNames[1]) )
            {
                idDir = rootDir.idDir;
                idParent = rootDir.idDir;
                for(int i=2; i<dirNames.Length; i++)
                {
                    idDir = checkIfDirExists(idUser, idVersion, @"\"+dirNames[i], idParent);
                    if (idDir == -1)
                        break;
                    else
                        idParent = idDir;
                }
            }
            return idDir;
        }
        public int createDirFromPath(int idUser, int idVersion, string path)
        {
            string[] dirNames = path.Split('\\');
            Dir rootDir = getRootDir(idUser, idVersion);
            int idParent = rootDir.idDir;
            int idDir = -1;
            try
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    for (int i = 2; i < dirNames.Length; i++)
                    {
                        idDir = checkIfDirExists(idUser, idVersion, @"\" + dirNames[i], idParent);
                        if (idDir == -1)
                            idDir = saveDirectory(@"\" + dirNames[i], idParent, idUser, idVersion, DateTime.Now, DateTime.Now);
                        idParent = idDir;
                    }
                    scope.Complete();
                }
                return idDir;
            }catch(Exception e)
            {
                throw;
            }  
        }
        public int saveDirectory(string name, int idParent, int idUser, int idVersion, DateTime creationTime, DateTime lastWriteTime)
        {
            int idDir = getMaxIdDir(idUser, idVersion) + 1;
            DBConnect dbConnect = new DBConnect();
            MySqlConnection conn = dbConnect.OpenConnection();

            string query = "INSERT INTO directory(idDir, name, idParent, idUser, idVersion, creationTime, lastWriteTime) VALUES(@idDir, @name, @idParent, @idUser, @idVersion, @creationTime, @lastWriteTime)";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = query;
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@idDir", idDir);
            cmd.Parameters.AddWithValue("@name", name);
            if (idParent == -1)
                cmd.Parameters.AddWithValue("@idParent", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@idParent", idParent);
            cmd.Parameters.AddWithValue("@idUser", idUser);
            cmd.Parameters.AddWithValue("@idVersion", idVersion);
            cmd.Parameters.AddWithValue("@creationTime", creationTime);
            cmd.Parameters.AddWithValue("@lastWriteTime", lastWriteTime);
            try
            {
                cmd.ExecuteNonQuery();
                dbConnect.CloseConnection();
                return idDir;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                dbConnect.CloseConnection();
                throw;
            }
        }
        public void updateDirectory(int idUser, int idVersion, int idDir, DateTime newLastWriteTime)
        {
            DBConnect dbConnect = new DBConnect();
            MySqlConnection conn = dbConnect.OpenConnection();

            string query = "UPDATE directory SET lastWriteTime=@lastWriteTime WHERE idUser=@idUser AND idVersion=@idVersion AND idDir=@idDir";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = query;
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@lastWriteTime", newLastWriteTime);
            cmd.Parameters.AddWithValue("@idUser", idUser);
            cmd.Parameters.AddWithValue("@idVersion", idVersion);
            cmd.Parameters.AddWithValue("@idDir", idDir);
            try
            {
                cmd.ExecuteNonQuery();
                dbConnect.CloseConnection();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                dbConnect.CloseConnection();
                throw;
            }
        }
        public void renameDirectory(int idUser, int idVersion, int idDir, string newName)
        {
            DBConnect dbConnect = new DBConnect();
            MySqlConnection conn = dbConnect.OpenConnection();

            string query = "UPDATE directory SET name=@newName WHERE idUser=@idUser AND idVersion=@idVersion AND idDir=@idDir";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = query;
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@newName", newName);
            cmd.Parameters.AddWithValue("@idUser", idUser);
            cmd.Parameters.AddWithValue("@idVersion", idVersion);
            cmd.Parameters.AddWithValue("@idDir", idDir);
            try
            {
                cmd.ExecuteNonQuery();
                dbConnect.CloseConnection();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                dbConnect.CloseConnection();
                throw;
            }
        }
        public void deleteDirectory(int idUser, int idVersion, int idDir)
        {
            try
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    deleteDirectoryRecursive(idUser, idVersion, idDir);
                    scope.Complete();
                }
            }
            catch(Exception e)
            {
                throw;
            }
            
        }
        public Dir getRootDir(int idUser, int idVersion)
        {
            DBConnect dbConnect = new DBConnect();
            MySqlConnection conn = dbConnect.OpenConnection();

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = "SELECT idDir, name, creationTime, lastWriteTime FROM directory WHERE idUser=@idUser AND idVersion=@idVersion AND idParent IS NULL";
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@idUser", idUser);
            cmd.Parameters.AddWithValue("@idVersion", idVersion);
            MySqlDataReader dataReader = cmd.ExecuteReader();

            if (dataReader.HasRows)
            {
                dataReader.Read();
                Dir dir = new Dir(dataReader.GetInt32(dataReader.GetOrdinal("idDir")), 
                                  dataReader.GetString(dataReader.GetOrdinal("name")), 
                                  null, 
                                  dataReader.GetDateTime("creationTime"),
                                  dataReader.GetDateTime("lastWriteTime")
                                  );
                dbConnect.CloseConnection();
                return dir;
            }
            else
            {
                dbConnect.CloseConnection();
                throw new Exception("root dir not found");
            }
        }
        public List<Dir> getAllSubDirOfDir(Dir dir, int idUser, int idVersion)
        {
            List<Dir> elencoDir = new List<Dir>();
            DBConnect dbConnect = new DBConnect();
            MySqlConnection conn = dbConnect.OpenConnection();

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = "SELECT idDir, name, creationTime, lastWriteTime FROM directory WHERE idUser=@idUser AND idVersion=@idVersion AND idParent=@idParent";
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@idUser", idUser);
            cmd.Parameters.AddWithValue("@idVersion", idVersion);
            cmd.Parameters.AddWithValue("@idParent", dir.idDir);
            MySqlDataReader dataReader = cmd.ExecuteReader();

            while (dataReader.Read())
            {
                elencoDir.Add(new Dir(
                    dataReader.GetInt32(dataReader.GetOrdinal("idDir")),
                    dataReader.GetString(dataReader.GetOrdinal("name")),
                    dir,
                    dataReader.GetDateTime("creationTime"),
                    dataReader.GetDateTime("lastWriteTime")
                ));
            }

            dataReader.Close();
            dbConnect.CloseConnection();
            return elencoDir;
        }

        private int checkIfDirExists(int idUser, int idVersion, string name, int idParent)
        {
            DBConnect dbConnect = new DBConnect();
            MySqlConnection conn = dbConnect.OpenConnection();

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = "SELECT idDir FROM directory WHERE idUser=@idUser AND idVersion=@idVersion AND name=@name AND idParent=@idParent";
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@idUser", idUser);
            cmd.Parameters.AddWithValue("@idVersion", idVersion);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@idParent", idParent);
            MySqlDataReader dataReader = cmd.ExecuteReader();

            int idDir = -1;
            if (dataReader.HasRows)
            {
                dataReader.Read();
                idDir = dataReader.GetInt32(dataReader.GetOrdinal("idDir"));
            }
            dbConnect.CloseConnection();
            return idDir;
        }
        private void deleteDirectoryRecursive(int idUser, int idVersion, int idDir)
        {
            List<int> idDirList = new List<int>();
            idDirList = getAllSubDirOfDir(idUser, idVersion, idDir);
            foreach (int idSubDir in idDirList)
                deleteDirectoryRecursive(idUser, idVersion, idSubDir);
            try
            {
                deleteDirectoryFromDB(idUser, idVersion, idDir);
            }
            catch (Exception e)
            {
                throw e;
            }

        }
        private void deleteDirectoryFromDB(int idUser, int idVersion, int idDir)
        {
            DBConnect dbConnect = new DBConnect();
            MySqlConnection conn = dbConnect.OpenConnection();

            string query = "DELETE FROM directory WHERE idUser=@idUser AND idVersion=@idVersion AND idDir=@idDir";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = query;
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@idUser", idUser);
            cmd.Parameters.AddWithValue("@idVersion", idVersion);
            cmd.Parameters.AddWithValue("@idDir", idDir);
            try
            {
                cmd.ExecuteNonQuery();
                dbConnect.CloseConnection();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                dbConnect.CloseConnection();
                throw;
            }
        }
        private List<int> getAllSubDirOfDir(int idUser, int idVersion, int idDir)
        {
            List<int> idDirList = new List<int>();
            DBConnect dbConnect = new DBConnect();
            MySqlConnection conn = dbConnect.OpenConnection();

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = "SELECT idDir FROM directory WHERE idUser=@idUser AND idVersion=@idVersion AND idParent=@idParent";
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@idUser", idUser);
            cmd.Parameters.AddWithValue("@idVersion", idVersion);
            cmd.Parameters.AddWithValue("@idParent", idDir);
            MySqlDataReader dataReader = cmd.ExecuteReader();

            while (dataReader.Read())
                idDirList.Add(dataReader.GetInt32(dataReader.GetOrdinal("idDir")));

            dataReader.Close();
            dbConnect.CloseConnection();
            return idDirList;

        }
    }
}
