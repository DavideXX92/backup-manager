using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newServerWF
{
    class FileDaoImpl : FileDao
    {
        private Dictionary<string, File> files;

        public Dictionary<string, File> getAllFiles(User user) {
            Dictionary<string, File> files = new Dictionary<string, File>();
            File file;
            DBConnect dbConnect = new DBConnect();
            MySqlConnection conn = dbConnect.OpenConnection();

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = "SELECT * FROM " + user.username + " WHERE 1";
            cmd.Prepare();
            MySqlDataReader dataReader = cmd.ExecuteReader();

            while (dataReader.Read())
            {
                    file = new File(
                        dataReader.GetString(dataReader.GetOrdinal("name")), 
                        dataReader.GetString(dataReader.GetOrdinal("path")), 
                        dataReader.GetInt32(dataReader.GetOrdinal("size")), 
                        dataReader.GetString(dataReader.GetOrdinal("hash"))
                    );
                    files[file.hash] = file;    
            }
         
            dataReader.Close();
            dbConnect.CloseConnection();

            return files;
        }
        public void addFile(File file, User user)
        {
            DBConnect dbConnect = new DBConnect();
            MySqlConnection conn = dbConnect.OpenConnection();

            string query = "INSERT INTO "+user.username+"(idFile, name, path, size, hash) VALUES(NULL, @name, @path, @size, @hash)";
            
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = query;
            cmd.Prepare();
            //cmd.Parameters.AddWithValue("@idFile", DBNull.Value);
            cmd.Parameters.AddWithValue("@name", file.name);
            cmd.Parameters.AddWithValue("@path", file.path);
            cmd.Parameters.AddWithValue("@size", file.size);
            cmd.Parameters.AddWithValue("@hash", file.hash);
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
              
            

            dbConnect.CloseConnection();

        }
        public void updateFile(File file, User user)
        {

        }
        public void deleteFile(File file, User user)
        {

        }
    }
}
