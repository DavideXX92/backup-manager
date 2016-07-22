using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Windows.Forms;

namespace newServerWF
{
    class DBConnect
    {
        private MySqlConnection connection;
        private string server;
        private string database;
        private string username;
        private string password;

        public DBConnect()
        {
            this.server = "localhost";
            this.username = "root";
            this.password = "admin";
            this.database = "gestionebackup";
            string connectionString;
            connectionString = "SERVER=" + server + ";" + "DATABASE=" +
            database + ";" + "UID=" + username + ";" + "PASSWORD=" + password + ";";
            this.connection = new MySqlConnection(connectionString);
        }

        //Open connection to database
        public MySqlConnection OpenConnection()
        {
            try
            {
                connection.Open();
                Console.WriteLine("Connection open with the database");
                return connection;
            }
            catch (MySqlException ex)
            {
                //When handling errors, you can your application's response based 
                //on the error number.
                //The two most common error numbers when connecting are as follows:
                //0: Cannot connect to server.
                //1045: Invalid user name and/or password.
                switch (ex.Number)
                {
                    case 0:
                        Console.WriteLine("Cannot connect to server.  Contact administrator");
                        break;

                    case 1045:
                        Console.WriteLine("Invalid username/password, please try again");
                        break;
                }
                return null;
            }
        }

        //Close connection to database
        public bool CloseConnection()
        {
            try
            {
                connection.Close();
                Console.WriteLine("Connection closed with the database");
                return true;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }

        }

        public List<string> showTables()
        {
            List<string> tables = new List<string>();
            string query = "SHOW TABLES";
            try
            {
                MySqlConnection conn = OpenConnection();
                //Create Command
                MySqlCommand cmd = new MySqlCommand(query, conn);
                //Create a data reader and Execute the command
                MySqlDataReader dataReader = cmd.ExecuteReader();
                //Read the data and store them in the list
                while (dataReader.Read())
                    tables.Add(dataReader.GetString(0));
                //close Data Reader
                dataReader.Close();

                CloseConnection();
                return tables;
            }
            catch (Exception e)
            {
                throw;
            }

        }

        public void setMonitorDir(User user, string monitorDir)
        {
            MySqlConnection conn = OpenConnection();
            try
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "UPDATE user SET monitorDir=@monitorDir WHERE username=@username";
                cmd.Prepare();
                cmd.Parameters.AddWithValue("@monitorDir", monitorDir);
                cmd.Parameters.AddWithValue("@username", user.username);
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw;
            }
            

            
        }

        public void createTableBackup(string tableName)
        {
            string query = @"
                CREATE TABLE IF NOT EXISTS `"+tableName+@"` (
                    `idFile` int(11) NOT NULL AUTO_INCREMENT,
                    `name` varchar(80) DEFAULT NULL,
                    `path` varchar(80) DEFAULT NULL,
                    `size` int(11) DEFAULT NULL,
                    `hash` binary(20) DEFAULT NULL,
                    PRIMARY KEY (`idFile`),
                    UNIQUE KEY `hash_UNIQUE` (`hash`),
                    UNIQUE KEY `name_UNIQUE` (`name`)
                )ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8;
            ";
            try
            {
                MySqlConnection conn = OpenConnection();
                //create mysql command
                MySqlCommand cmd = new MySqlCommand();
                //Assign the query using CommandText
                cmd.CommandText = query;
                //Assign the connection using Connection
                cmd.Connection = conn;

                //Execute query
                cmd.ExecuteNonQuery();

                //Close connection
                CloseConnection();
            }
            catch (Exception e)
            {
                throw;
            }
        }

    }
}
