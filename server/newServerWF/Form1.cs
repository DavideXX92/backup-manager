using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace newServerWF
{
    public partial class Form1 : Form
    {
        private enum state_t { on, off };
        private state_t state;
        private ServerTCP server;
        public delegate void WriteOnConsoleDel(string str);

        public Form1()
        {
            state = state_t.off;
            InitializeComponent();
            WriteOnConsoleDel del = (str) => { consoleWrite = str; };
            MyConsole.setConsole(del);
        }

        public string consoleWrite
        {
            set { consoleBox.Invoke(new MethodInvoker(delegate() { consoleBox.AppendText((DateTime.Now.ToString("HH:mm:ss") + "\t" + value + "\n")); })); }
        }

        private void stateButton_Click(object sender, EventArgs e)
        {
            if (state == state_t.off)
            {   // Start server
                state = state_t.on;
                stateLabel.Text = "ON";
                stateLabel.BackColor = Color.Green;
                stateButton.Text = "Stop";
                portBox.ReadOnly = true;
                server = new ServerTCP(8001);
                server.Start();
            }
            else
            {   // Stop server
                server.Stop();
                state = state_t.off;
                stateLabel.Text = "OFF";
                stateLabel.BackColor = Color.Red;
                stateButton.Text = "Start";
                portBox.ReadOnly = false;
            }
        }


    }
}
