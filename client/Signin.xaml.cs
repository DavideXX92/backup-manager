using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApplication1 {
    /// <summary>
    /// Logica di interazione per Signin.xaml
    /// </summary>
    /// 
    
    public partial class Signin : Window {
        public delegate void del1(string s);
        public del1 setDirectoryPath;
        private ClientTcp client;

        public Signin(del1 setDirectoryPath, del1 printcs) {
            this.setDirectoryPath = setDirectoryPath;
            client = new ClientTcp(printcs);
            InitializeComponent();
        }

        private void _switchTab(object sender, RoutedEventArgs e) {
            if (sender == switchToRegistra)
                tabLogin_Reg.SelectedIndex = 1;
            else
                tabLogin_Reg.SelectedIndex = 0;
        }

        private void _login(object sender, RoutedEventArgs e) {
            String path = "";
            int n = client.login(usernameLog.Text, passwordLog.Text, path);
            if (n == -1)
                errorMessageLog.Content = "Errore, connessione col server caduta";
            else if (n == -2)
                errorMessageLog.Content = "Error, username o password errati";
            else
                setDirectoryPath(path);
            //TODO eliminare la setDirectoryPath qua sotto, serve per debug
            setDirectoryPath("C:\\Users\\Davide\\Documents\\Poli");
        }

        private void _registra(object sender, RoutedEventArgs e) {

        }

    }
}
