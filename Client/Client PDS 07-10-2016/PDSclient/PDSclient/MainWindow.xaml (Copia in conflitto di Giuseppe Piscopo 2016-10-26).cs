using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Interactivity;

namespace PDSclient {
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public delegate void writeString(String text);

        Dir tree = null;
        Boolean pwdMatch = false;
        string path = "";
        Loading.LoadFunction loadStatus = Loading.LoadFunction.Checking;
        Boolean _autoSynch = false;

        //Nuove variabili
        private bool isLogged;
        private UserController userController;
        private User user;
        private string monitorDir;
        private List<Version> versions;

        Boolean IsAutoSynchAble {
            get { return _autoSynch; }
            set {
                if (value == false) {
                    _autoSynch = false;
                    userController.disableAutoSync();
                    buttonAutoSynch.Content = "Manual";
                } else if (value == true) {
                    _autoSynch = true;
                    userController.enableAutoSync();
                    buttonAutoSynch.Content = "Auto";
                }
            }
        }

        public MainWindow() {                
            MyConsole.setDel(printConsole);
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            versions = null;
            userController = null;
            user = null;
            monitorDir = null;
            isLogged = false;

            Signin login_form = new Signin((UserController userContr) => { userController = userContr; }, (User u) => { user = u; });
            login_form.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            login_form.Topmost = true;
            login_form.ShowDialog();
            
            if(userController!=null && user!=null)
                isLogged = true;

            if (!isLogged) {
                Close();
                return;
            }

            if (user.monitorDir.Count > 0) { 
                monitorDir = checkMonitorDir(user.monitorDir);
                if (monitorDir == null) {
                    String msg = "Directory not found.\r\nDoes it exist in the actual device?";
                    Alert alert = new Alert(msg, chooseMonitorDirPosition, "Yes(chosse path)", noDirInDeviceHandler, "No");
                    alert.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    alert.ShowDialog();
                }
            } else { //if no items are in user.monitorDir, there is no syncrhonized directory in the server for the current user
                String msg = "Choose a directory to monitor";
                Alert alert = new Alert(msg, chooseNewMonitorDir, "Path");
                alert.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                alert.ShowDialog();
            }

            if (monitorDir == null) {//alert window should set a monitorDir
                this.Close();
                return;
            }

            Loading load_window = new Loading(monitorDir, loadStatus, userController);            
            load_window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            load_window.ShowDialog();            

            InitializeComponent();
            if (isLogged)
            {
                if (user.monitorDir == null)
                {
                    printConsole("Non e' stata trovata nessuna cartella da monitorare, scegline una");
                }
                else
                {
                    //monitorDir = user.monitorDir;
                    printConsole("La cartella che stai monitorando e': " + monitorDir);
                    //dirLabel.Text = monitorDir;

                    try
                    {                        
                        userController.timerInit();
                        IsAutoSynchAble = true;
                        printConsole("Controllo se sul server sono presenti delle versioni...");
                        versions = userController.askStoredVersions();
                        printConsole("Sono state trovate " + versions.Count + " versioni");

                        //----------DA MODIFICARE-----------
                        foreach (Version tmp in versions)
                        {
                            if (tmp.idVersion == 1)
                            {
                                tree = tmp.dirTree;
                                break;
                            }
                        }
                        //----------------------------------

                    }
                    catch (Exception ex)
                    {
                        string error = "Problema di rete: impossibile ottenere la lista delle versioni salvate";
                        printConsole(error);
                        Console.WriteLine(error + "\nException: " + ex.Message);
                    }
                }  
            }
        }

        public void paintDirTree() {
            if(!treeView.Items.IsEmpty)
                treeView.Items.Clear();

            treeView.Items.Add(tree);         
        }
        
        public void paintDirList() {
            if (treeView.SelectedItem != null)
                listDataBinding.ItemsSource = (treeView.SelectedItem as Dir).exploreDir();
        }
        
        public void printConsole(string s) {
            console.AppendText(s + "\n");
        }

        public void showDirectory(object sender, RoutedEventArgs e) {          
            paintDirList();
            if(treeView.SelectedItem != null)
                showCurrentPosition((treeView.SelectedItem as Dir).path);
        }

        public void showCurrentPosition(string path) {
            fullPath.Content = monitorDir.Substring(0, monitorDir.LastIndexOf('\\')) + '\\' + path;
        }

        public void listMouseDoubleClick(object sender, RoutedEventArgs e) {

            if (listDataBinding.SelectedItem is Dir && treeView.SelectedItem != null && treeView.SelectedItem is Dir) {
                Dir listSelected = listDataBinding.SelectedItem as Dir;
                Dir treeSelected = treeView.SelectedItem as Dir;
                    
                treeSelected.IsSelected = false;
                if (listSelected.name == "..") {                   
                    listSelected.parentDir.IsSelected = true;
                    listSelected.expandTree();
                } else {
                    listSelected.IsSelected = true;
                    listSelected.expandTree();
                }
               
                treeView.Items.Refresh();
                paintDirList();
            }
        }

        private String dialog() {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            if ((folderBrowserDialog1.ShowDialog()) == System.Windows.Forms.DialogResult.OK) {
                return folderBrowserDialog1.SelectedPath;
            }
            return null;
        }

        private void treeView_ToolTipOpening(object sender, ToolTipEventArgs e) { }

        private void versionTreeLoaded(object sender, RoutedEventArgs e) {
            versionList.ItemsSource = getVersionsToShow();
            versionList.SelectedItem = versionList.Items.GetItemAt(0);            
        }

        private void versionSelectionChanged(object sender, RoutedEventArgs e) {
            if (versionList.SelectedIndex != -1) {
                Version currentVersion = versionList.SelectedItem as Version;
                tree = currentVersion.dirTree;

                paintDirTree();
                showCurrentPosition(tree.path);
            }
        }

        private List<Version> getVersionsToShow() {
            List<Version> shownVersions = new List<Version>(versions);
            shownVersions.RemoveAt(shownVersions.Count - 1);

            return shownVersions;
        }

        private void updateVersions() {
            versions = userController.askStoredVersions();
        }

        private String checkMonitorDir(List<String> possiblePaths) {
            foreach (String path in possiblePaths)
                Console.WriteLine("path: " + path);

            foreach (String path in possiblePaths)
                if (Directory.Exists(path))
                    return path;

            Console.WriteLine("La cartella non esiste, specificare il path o creare una nuova sincronizzazione?");
            return null;
        }

        /*
         *  Le seguenti funzioni vengono richiamate da un alert 
         */
        private void noDirInDeviceHandler(object sender, RoutedEventArgs e) {
            String msg = "Are you sure you want to choose an other directory?\r\nAll the stored data will be lost!";
            Alert alert = new Alert(msg, chooseNewMonitorDir, "Choose dir", backButton, "Back");
            alert.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            alert.ShowDialog();
            if (loadStatus == Loading.LoadFunction.Synchronizing)
                (sender as Button).Tag = "Close";
        }

        private void newSynchHandler(object sender, RoutedEventArgs e) {
            String msg = "Are you sure you want to choose an other directory?\r\nAll the stored data will be lost!";
            Alert alert = new Alert(msg, chooseNewMonitorDir, "Choose dir", backButton, "Back");
            alert.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            alert.ShowDialog();
            if (loadStatus == Loading.LoadFunction.Synchronizing)
                (sender as Button).Tag = "Close";
        }

        private void changeMonitoredDirHandler(object sender, RoutedEventArgs e) {
            loadStatus = Loading.LoadFunction.Checking; //Serve per permettere all'alert di settare la variabile a 'syncrhonizing'
            String msg = "Are you sure you want to choose an other directory?\r\nAll the stored data will be lost!";
            Alert alert = new Alert(msg, chooseNewMonitorDir, "Choose dir", backButton, "Back");
            alert.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            alert.ShowDialog();
            if (loadStatus == Loading.LoadFunction.Synchronizing) {
                (sender as Button).Tag = "Close";
                Loading load_window = new Loading(monitorDir, loadStatus, userController);
                load_window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                load_window.ShowDialog();        
            }
        }

        private void newVersionHandler(object sender, RoutedEventArgs e) {
            userController.createNewVersion(monitorDir);
            updateVersions();
            versionList.ItemsSource = getVersionsToShow();
        }

        private void backButton(object sender, RoutedEventArgs e) {
            (sender as Button).Tag = "Close";
        }

        private void chooseMonitorDirPosition(object sender, RoutedEventArgs e) {
            monitorDir = dialog();
            if(monitorDir != null)
                (sender as Button).Tag = "Close";
        }

        private void chooseNewMonitorDir(object sender, RoutedEventArgs e) {
            monitorDir = dialog();
            if (monitorDir != null) {
                userController.deleteUserRepository();
                (sender as Button).Tag = "Close";
                loadStatus = Loading.LoadFunction.Synchronizing;
            }            
        }

        private void buttonAutoSynchHandler(object sender, RoutedEventArgs e) {
            IsAutoSynchAble = !IsAutoSynchAble;
        }

        private void buttonRestoreHandler(object sender, RoutedEventArgs e) {
            //int idVersion = Int32.Parse((versionList.SelectedItem as String).Substring(9));
            //Console.WriteLine("Version to restore: " + idVersion);
            try
            {
                userController.restoreVersion(1, monitorDir);
                userController.createNewVersion(monitorDir);
            }
            catch(Exception ex)
            {

            }
            
        }

        private void buttonReduceHandler(object sender, RoutedEventArgs e) {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void buttonCloseHandler(object sender, RoutedEventArgs e) {
            Close();
        }

        private void windowLeftButtonDownHandler(object sender, MouseButtonEventArgs e) {
            DragMove();
        }

        private void versionList_Selected(object sender, RoutedEventArgs e) {

        }  
    }
}