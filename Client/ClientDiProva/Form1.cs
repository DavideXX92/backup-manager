using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientDiProva
{
    public partial class Form1 : Form
    {
        public delegate void delegato(string str);
        //private HandleClient hc;
        private UserController userController;
        private User user;
        private string monitorDir;
        private List<Version> versionList;

        public Form1()
        {
            InitializeComponent();
            delegato del = (str) => { consoleWrite = str; };
            MyConsole.setDel(del);
            user = null;
            monitorDir = null;
        }

        public string consoleWrite
        {
            set { consoleBox.Invoke(new MethodInvoker(delegate() { consoleBox.AppendText((DateTime.Now.ToString("HH:mm:ss") + "\t" + value + "\n")); })); }
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            try
            {
                userController = new UserControllerImpl();
            }catch (Exception ex)
            {
                MyConsole.write(ex.Message);
            }
        }

        private void disconnectButton_Click(object sender, EventArgs e)
        {
            try
            {
                userController.logout();
                MyConsole.write("Hai effettuato il logout");
            }
            catch (Exception ex)
            {
                if (ex is ServerException)
                {
                    Console.WriteLine("Logout fallito: " + ex.Message);
                    MyConsole.write("Problema nel server: impossibile eseguire il logout");
                }
                else if (ex is NetworkException)
                {
                    string error = "Problema di rete: impossibile eseguire il logout";
                    MyConsole.write(error);
                    Console.WriteLine(error + "\nException: " + ex.Message);
                }
            }
        }

        private void loginRequest_Click(object sender, EventArgs e)
        {
            try
            {
                user = userController.login("giuseppe", "piscopo");
                MyConsole.write("Login riuscito");
                
                if(user.monitorDir==null)
                {
                    MyConsole.write("Non e' stata trovata nessuna cartella da monitorare, scegline una");
                }
                else{
                    monitorDir = user.monitorDir;
                    MyConsole.write("La cartella che stai monitorando e': " + monitorDir);
                    dirLabel.Text = monitorDir;
                    
                    try{
                        MyConsole.write("Controllo se sul server sono presenti delle versioni...");
                        versionList = userController.askStoredVersions();
                        MyConsole.write("Sono state trovate "+ versionList.Count + " versioni");
                    }catch(Exception ex){
                        string error = "Problema di rete: impossibile ottenere la lista delle versioni salvate";
                        MyConsole.write(error);
                        Console.WriteLine(error + "\nException: " + ex.Message);
                    }
                }  
            }
            catch (Exception ex)
            {
                if (ex is ServerException)
                    MyConsole.write("Login fallito: " + ex.Message);
                else if (ex is NetworkException)
                {
                    string error = "Problema di rete: impossibile eseguire il login";
                    MyConsole.write(error);
                    Console.WriteLine(error + "\nException: " + ex.Message);
                }
            }
        }

        private void registerRequest_Click(object sender, EventArgs e)
        {
            try
            {
                userController.register("giuseppee", "piscopo");
                MyConsole.write("Registrazione avvenuta con successo");
            }
            catch (Exception ex)
            {
                if (ex is ServerException)
                    MyConsole.write("Impossibile registrarsi: " + ex.Message);
                else if (ex is NetworkException)
                {
                    string error = "Problema di rete: impossibile registrarsi";
                    Console.WriteLine(error + "\nException: " + ex.Message);
                }
            }
        }

        private void chooseDir_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                try{
                    userController.setMonitorDir(folderBrowserDialog1.SelectedPath);
                    dirLabel.Text = folderBrowserDialog1.SelectedPath;
                }catch(Exception ex)
                {
                    string error = "Problema di rete: impossibile settare la cartella da monitorare";
                    Console.WriteLine(error + "\nException: " + ex.Message);
                }
            }
        }

        private void addNewVersion_Click(object sender, EventArgs e)
        {
            if( monitorDir==null )
                MyConsole.write("Errore: non hai selezionato nessuna cartella da monitorare");
            else
                try
                {
                    userController.createNewVersion(monitorDir);
                    MyConsole.write("Versione creata correttamente sul server");
                }catch (Exception ex)
                {
                    if (ex is ServerException)
                    {
                        MyConsole.write("Problema nel server: impossibile creare una nuova versione");
                        Console.WriteLine("Problema nel server: " + ex.Message);
                    }
                    
                    else if (ex is NetworkException)
                    {
                        string error = "Problema di rete: impossibile creare una nuova versione";
                        MyConsole.write(error);
                        Console.WriteLine(error + "\nException: " + ex.Message);
                    }
                }
        }

        private void restore_Click(object sender, EventArgs e)
        {
            try
            {
                //hc.restoreVersion(1);
                int idVersionToRestore = 4;
                string pathDst = @"c:\tmp";
                userController.restoreVersion(idVersionToRestore, pathDst);
                MyConsole.write("La versione " + idVersionToRestore + " e' stata ripristinata");
            }
            catch (Exception ex)
            {
                string error = "Problema di rete: impossibile ripristinare la versione";
                MyConsole.write(error);
                Console.WriteLine(error + "\nException: " + ex.Message);
            }
        }

        //NO
        private void browse_Click(object sender, EventArgs e)
        {
            /*OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "txt files (*.txt)|*.txt|pdf files (*.pdf)|*.pdf|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 3;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                file = new File();
                file.name = openFileDialog1.FileName ;
                FileInfo fileinfo = new FileInfo(file.name);
                FileSystem fs = new FileSystem();
                file.path = fs.getRelativePath(fileinfo.DirectoryName, @"C:\\ricevuti\monitorDir\");
                file.size = (int)fileinfo.Length;
                filenameLabel.Text = file.name + " " + file.size / 1024 + " KB";
                
                file.hash = fs.getHash(file.name);
            }*/
        }

        //NO
        private void sendFile_Click(object sender, EventArgs e)
        {
            //hc.sendAfile(file);
        }

        
        private void synchronizeButton_Click(object sender, EventArgs e)
        {
            if (monitorDir == null)
                MyConsole.write("Errore: non hai selezionato nessuna cartella da monitorare");
            try
            {
                userController.synchronize(monitorDir);
            }catch(Exception ex)
            {
                string error = "Problema di rete: impossibile eseguire la sincronizzazione";
                MyConsole.write(error);
                Console.WriteLine(error + "\nException: " + ex.Message);
            }
        }

        private void testButton_Click(object sender, EventArgs e)
        {
            //hc.test();
        }

        private void checkVersion_Click(object sender, EventArgs e)
        {
            try
            {
                userController.checkIfCurrentVersionIsUpdated(monitorDir);
            }
            catch (Exception ex)
            {
                string error = "Problema di rete: impossibile controllare la versione";
                MyConsole.write(error);
                Console.WriteLine(error + "\nException: " + ex.Message);
            }
        }


        

       

       

    }
}
