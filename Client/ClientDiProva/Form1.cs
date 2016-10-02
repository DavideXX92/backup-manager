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
        private HandleClient hc;

        public Form1()
        {
            InitializeComponent();
            delegato del = (str) => { consoleWrite = str; };
            MyConsole.setDel(del);
        }

        public string consoleWrite
        {
            set { consoleBox.Invoke(new MethodInvoker(delegate() { consoleBox.AppendText((DateTime.Now.ToString("HH:mm:ss") + "\t" + value + "\n")); })); }
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            try
            {
                hc = new HandleClientImpl("127.0.0.1", 8001);
            }
            catch (Exception ex)
            {
                MyConsole.write(ex.Message);
            }
        }

        private void disconnectButton_Click(object sender, EventArgs e)
        {
            hc.logoutRequest();
        }

        private void loginRequest_Click(object sender, EventArgs e)
        {
            hc.loginRequest("giuseppe", "piscopo");
        }

        private void registerRequest_Click(object sender, EventArgs e)
        {
            hc.registerRequest("giuseppee", "piscopo");
        }

        private void chooseDir_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                hc.setMonitorDir(folderBrowserDialog1.SelectedPath);
                dirLabel.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void addNewVersion_Click(object sender, EventArgs e)
        {
            hc.createNewVersion();
        }

        private void restore_Click(object sender, EventArgs e)
        {
            hc.restoreVersion(1);
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

        //NO
        private void synchronizeButton_Click(object sender, EventArgs e)
        {
            hc.synchronize();
        }

        private void testButton_Click(object sender, EventArgs e)
        {
            hc.test();
        }

        

       

       

    }
}
