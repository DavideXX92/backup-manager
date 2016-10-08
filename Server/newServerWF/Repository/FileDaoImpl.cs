using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace newServerWF
{
    class FileDaoImpl : FileDao
    {
        private int getMaxIdFile(int idUser, int idVersion)
        {
            DBConnect dbConnect = new DBConnect();
            MySqlConnection conn = dbConnect.OpenConnection();

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = "SELECT MAX(idFile) FROM file WHERE idUser=@idUser AND idVersion=@idVersion";
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@idUser", idUser);
            cmd.Parameters.AddWithValue("@idVersion", idVersion);
            MySqlDataReader dataReader = cmd.ExecuteReader();

            int idFile = -1;
            dataReader.Read();
            if (!dataReader.IsDBNull(0))
                idFile = dataReader.GetInt32(0);
           
            dbConnect.CloseConnection();
            return idFile;
        }
        public int checkIfFileExists(int idUser, int idVersion, string path)
        {
            DirDao dirDao = new DirDaoImpl();
            string pathWithoutFile = Path.GetDirectoryName(path);
            int idDir = dirDao.checkIfPathExists(idUser, idVersion, pathWithoutFile);
            if( idDir == -1)
                return -1;
            else{
                string name = path.Substring(path.LastIndexOf(@"\")).Substring(1);
                int idFile = checkIfFileExistsFromName(idUser, idVersion, name, idDir);
                return idFile;
            }
        }
        public int saveFile(string name, int size, string hash, string extension, int idDir, int idUser, int idVersion, DateTime creationTime, DateTime lastWriteTime)
        {
            DBConnect dbConnect = null;
            try
            {
                int idFile;
                using(TransactionScope scope = new TransactionScope())
                {
                    HashDao hashDao = new HashDaoImpl();
                    int idHash = hashDao.checkIfHashExists(hash, idUser);
                    if (idHash == -1)
                        idHash = hashDao.saveHash(hash, idUser);
                    else
                        hashDao.changeCounterOfHash(idHash, idUser, 1);
                    idFile = getMaxIdFile(idUser, idVersion) + 1;

                    dbConnect = new DBConnect();
                    MySqlConnection conn = dbConnect.OpenConnection();

                    string query = "INSERT INTO file(idFile, name, size, idHash, extension, idDir, idUser, idVersion, creationTime, lastWriteTime) VALUES(@idFile, @name, @size, @idHash, @extension, @idDir, @idUser, @idVersion, @creationTime, @lastWriteTime)";

                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandText = query;
                    cmd.Prepare();
                    cmd.Parameters.AddWithValue("@idFile", idFile);
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@size", size);
                    cmd.Parameters.AddWithValue("@idHash", idHash);
                    cmd.Parameters.AddWithValue("@extension", extension);
                    cmd.Parameters.AddWithValue("@idDir", idDir);
                    cmd.Parameters.AddWithValue("@idUser", idUser);
                    cmd.Parameters.AddWithValue("@idVersion", idVersion);
                    cmd.Parameters.AddWithValue("@creationTime", creationTime);
                    cmd.Parameters.AddWithValue("@lastWriteTime", lastWriteTime);

                    cmd.ExecuteNonQuery();
                    scope.Complete();
                    dbConnect.CloseConnection();
                }
                return idFile;   
             }
             catch (Exception e)
            {
                if(dbConnect!=null)
                    dbConnect.CloseConnection();
                Console.WriteLine(e.Message);
                throw;
            }
        }
        public void updateFile(int idUser, int idVersion, int idFile, string newHash, int newSize, DateTime newLastWriteTime)
        {
            DBConnect dbConnect = null;
            try
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    HashDao hashDao = new HashDaoImpl();
                    int idNewHash = hashDao.checkIfHashExists(newHash, idUser);
                    if (idNewHash == -1)
                    {
                        idNewHash = hashDao.saveHash(newHash, idUser);
                        int idOldHash = getIdHash(idUser, idVersion, idFile);
                        hashDao.changeCounterOfHash(idOldHash, idUser, -1);
                    }

                    dbConnect = new DBConnect();
                    MySqlConnection conn = dbConnect.OpenConnection();

                    string query = "UPDATE file SET idHash=@idHash, size=@size, lastWriteTime=@lastWriteTime WHERE idUser=@idUser AND idVersion=@idVersion AND idFile=@idFile";

                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandText = query;
                    cmd.Prepare();
                    cmd.Parameters.AddWithValue("@idHash", idNewHash);
                    cmd.Parameters.AddWithValue("@size", newSize);
                    cmd.Parameters.AddWithValue("@lastWriteTime", newLastWriteTime);
                    cmd.Parameters.AddWithValue("@idUser", idUser);
                    cmd.Parameters.AddWithValue("@idVersion", idVersion);
                    cmd.Parameters.AddWithValue("@idFile", idFile);

                    cmd.ExecuteNonQuery();
                    scope.Complete();
                    dbConnect.CloseConnection();
                }
            }
            catch (Exception e)
            {
                if(dbConnect!=null)
                    dbConnect.CloseConnection();
                Console.WriteLine(e.Message);
                throw;
            }
        }
        public void deleteFile(int idUser, int idVersion, int idFile)
        {
            DBConnect dbConnect = new DBConnect();
            MySqlConnection conn = dbConnect.OpenConnection();

            string query = "DELETE FROM file WHERE idUser=@idUser AND idVersion=@idVersion AND idFile=@idFile";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = query;
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@idUser", idUser);
            cmd.Parameters.AddWithValue("@idVersion", idVersion);
            cmd.Parameters.AddWithValue("@idFile", idFile);

            cmd.ExecuteNonQuery();
            dbConnect.CloseConnection();
        }
        public void renameFile(int idUser, int idVersion, int idFile, string newName)
        {
            DBConnect dbConnect = new DBConnect();
            MySqlConnection conn = dbConnect.OpenConnection();

            string query = "UPDATE file SET name=@newName WHERE idUser=@idUser AND idVersion=@idVersion AND idFile=@idFile";

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = query;
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@newName", newName);
            cmd.Parameters.AddWithValue("@idUser", idUser);
            cmd.Parameters.AddWithValue("@idVersion", idVersion);
            cmd.Parameters.AddWithValue("@idFile", idFile);
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
        public List<File> getAllFilesOfDir(Dir dir, int idUser, int idVersion)
        {
            List<File> elencoFile = new List<File>();
            DBConnect dbConnect = new DBConnect();
            MySqlConnection conn = dbConnect.OpenConnection();

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = @"SELECT name, size, hash, extension, creationTime, lastWriteTime
                                FROM file, hash 
                                WHERE idDir=@idDir AND 
                                      file.idUser=@idUser AND 
                                      idVersion=@idVersion AND
                                      file.idUser=hash.idUser AND
                                      file.idHash=hash.idHash";
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@idDir", dir.idDir);
            cmd.Parameters.AddWithValue("@idUser", idUser);
            cmd.Parameters.AddWithValue("@idVersion", idVersion);
            MySqlDataReader dataReader = cmd.ExecuteReader();

            while (dataReader.Read())
            {
                elencoFile.Add(new File(
                    dataReader.GetString(dataReader.GetOrdinal("name")),
                    dir,
                    dataReader.GetInt32(dataReader.GetOrdinal("size")),
                    dataReader.GetString(dataReader.GetOrdinal("hash")),
                    dataReader.GetString(dataReader.GetOrdinal("extension")),
                    dataReader.GetDateTime("creationTime"),
                    dataReader.GetDateTime("lastWriteTime")
                ));
            }

            dataReader.Close();
            dbConnect.CloseConnection();
            return elencoFile;
        }

        private int checkIfFileExistsFromName(int idUser, int idVersion, string name, int idDir)
        {
            DBConnect dbConnect = new DBConnect();
            MySqlConnection conn = dbConnect.OpenConnection();

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = "SELECT idFile FROM file WHERE idUser=@idUser AND idVersion=@idVersion AND name=@name AND idDir=@idDir";
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@idUser", idUser);
            cmd.Parameters.AddWithValue("@idVersion", idVersion);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@idDir", idDir);
            MySqlDataReader dataReader = cmd.ExecuteReader();

            int idFile = -1;
            if (dataReader.HasRows)
            {
                dataReader.Read();
                idFile = dataReader.GetInt32(dataReader.GetOrdinal("idFile"));
            }
            dbConnect.CloseConnection();
            return idFile;
        }
        private int getIdHash(int idUser, int idVersion, int idFile)
        {
            DBConnect dbConnect = new DBConnect();
            MySqlConnection conn = dbConnect.OpenConnection();

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = "SELECT idHash FROM file WHERE idUser=@idUser AND idVersion=@idVersion AND idFile=@idFile";
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@idUser", idUser);
            cmd.Parameters.AddWithValue("@idVersion", idVersion);
            cmd.Parameters.AddWithValue("@idFile", idFile);
            MySqlDataReader dataReader = cmd.ExecuteReader();

            int idHash = -1;
            if(dataReader.HasRows)
            {
                dataReader.Read();
                idHash = dataReader.GetInt32(0);
            }
            dbConnect.CloseConnection();
            return idHash;
        }
    }
}
