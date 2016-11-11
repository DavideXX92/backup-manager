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
using WindowsInput;
using System.Timers;

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
        public bool isVersionCongruent;
        private UserController userController;
        private User user;
        private string monitorDir;
        private List<Version> versions;
        private System.Timers.Timer timer = null;

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
            while (true) {
                try {
                    MyConsole.setDel(printConsole);
                    this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

                    versions = null;
                    userController = null;
                    user = null;
                    monitorDir = null;
                    isLogged = false;
                    isVersionCongruent = false;

                    Signin login_form = new Signin((UserController userContr) => { userController = userContr; }, (User u) => { user = u; });
                    login_form.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    login_form.Topmost = true;
                    login_form.ShowDialog();

                    if (userController != null && user != null)
                        isLogged = true;

                    if (!isLogged) {
                        Close();
                        return;
                    }

                    if (user.monitorDir.Count > 0) {
                        monitorDir = checkMonitorDir(user.monitorDir);
                        if (monitorDir == null) {
                            String msg = "Directory not found in the current device.\r\nLocate the actual path, download the last version\r\n or perform a new synchronization";
                            Alert alert = new Alert(msg, chooseMonitorDirPosition, "Locate path", alertRestoreVersion, "Last version", noDirInDeviceHandler, "New synch");
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

                    Loading load_window = new Loading(monitorDir, loadStatus, userController, (Boolean var) => { isVersionCongruent = var; });
                    load_window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    load_window.ShowDialog();

                    if (!isVersionCongruent) {
                        this.Close();
                        return;
                    }

                    InitializeComponent();
                    if (isLogged) {
                        if (user.monitorDir == null) {
                            printConsole("Non e' stata trovata nessuna cartella da monitorare, non dovrebbe capitare mai!");
                            return;
                        } else {
                            printConsole("La cartella che stai monitorando e': " + monitorDir);

                            userController.timerInit();
                            IsAutoSynchAble = true;
                            printConsole("Controllo se sul server sono presenti delle versioni...");
                            versions = userController.askStoredVersions();
                            printConsole("Versioni recuperate");
                            //
                            if (timer == null)
                            {
                                timer = new System.Timers.Timer(5000);
                                timer.Elapsed += OnTimedEvent;
                                timer.AutoReset = true;
                                timer.Enabled = true;
                                Console.WriteLine("Timer set");
                            }
                            //
                            this.ShowDialog();
                            return;
                        }
                    }
                } catch (NetworkException) {
                    String msg = "Network error\r\nCheck if your Internet connection is up";
                    Alert alert = new Alert(msg, (object sender, RoutedEventArgs e) => { (sender as Button).Tag = "Close"; return; }, "Path");
                    alert.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    alert.ShowDialog();
                    this.Close();
                } catch (ServerException) {
                    String msg = "Server error\r\nThe server may be down or under maintenance";
                    Alert alert = new Alert(msg, (object sender, RoutedEventArgs e) => { (sender as Button).Tag = "Close"; return; }, "Path");
                    alert.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    alert.ShowDialog();
                    this.Close();
                    return;
                }
            }
        }

        public void paintDirTree() {
            if (!treeView.Items.IsEmpty)
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
            if (treeView.SelectedItem != null)
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

        private void fillVersionTree() {
            versionList.ItemsSource = getVersionsToShow();
            versionList.SelectedItem = versionList.Items.GetItemAt(0);
            //WORKAROUND-> il selected item non prende il focus, lo prende se si preme 'tab' dopo aver selezionato la listview (perchè??)
            versionList.Focus();
            InputSimulator s = new InputSimulator();
            s.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.TAB);
        }

        private void versionTreeLoaded(object sender, RoutedEventArgs e) {
            fillVersionTree();
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
            try {
                versions = userController.askStoredVersions();
            } catch (Exception e) {
                Console.WriteLine("Impossibile ricevere le versioni");
            }
        }

        private String checkMonitorDir(List<String> possiblePaths) {
            foreach (String path in possiblePaths)
                Console.WriteLine("path: " + path);

            foreach (String path in possiblePaths)
                if (Directory.Exists(path))
                    return path;

            return null;
        }

        /*
         *  Le seguenti funzioni vengono richiamate da un alert 
         */
        private void noDirInDeviceHandler(object sender, RoutedEventArgs e) {
            try {
                String msg = "Are you sure you want to choose an other directory?\r\nAll the stored data will be lost!";
                Alert alert = new Alert(msg, chooseNewMonitorDir, "Choose dir", backButton, "Back");
                alert.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                alert.ShowDialog();
                if (loadStatus == Loading.LoadFunction.Synchronizing)
                    (sender as Button).Tag = "Close";
            } catch (ServerException) {
                throw;
            } catch (NetworkException) {
                throw;
            } finally {
                (sender as Button).Tag = "Close";
            }
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
            try {
                loadStatus = Loading.LoadFunction.Checking; //Serve per permettere all'alert di settare la variabile a 'syncrhonizing'
                String msg = "Are you sure you want to choose an other directory?\r\nAll the stored data will be lost!";
                Alert alert = new Alert(msg, chooseNewMonitorDir, "Choose dir", backButton, "Back");
                alert.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                alert.ShowDialog();
                if (loadStatus == Loading.LoadFunction.Synchronizing) {
                    (sender as Button).Tag = "Close";
                    Loading load_window = new Loading(monitorDir, loadStatus, userController, (Boolean var) => { isVersionCongruent = var; });
                    load_window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    this.Hide();
                    load_window.ShowDialog();
                    updateVersions();
                    fillVersionTree();
                    this.Show();
                }
            } catch (ServerException) {
                throw;
            } catch (NetworkException) {
                throw;
            }
        }

        private void newVersionHandler(object sender, RoutedEventArgs e) {
            try {
                userController.createNewVersion(monitorDir);
                printConsole("Versione creata correttamente");
            } catch (ServerException ex){                
                printConsole("Errore durante la creazione della nuova versione: " + ex.Message);                                                    
            } catch(NetworkException ex){
            string error = "Problema di rete: impossibile creare una nuova versione";
                printConsole(error);
                Console.WriteLine(error + "\nException: " + ex.Message);
                throw;
            }
            
            updateVersions();
            versionList.ItemsSource = getVersionsToShow();
        }

        private void backButton(object sender, RoutedEventArgs e) {
            (sender as Button).Tag = "Close";
        }

        private void chooseMonitorDirPosition(object sender, RoutedEventArgs e) {
            try {
                monitorDir = dialog();
                userController.addMonitorDir(monitorDir);
                if (monitorDir != null)
                    (sender as Button).Tag = "Close";
            }catch(ServerException){
                if (monitorDir != null) {
                    (sender as Button).Tag = "Close";
                    String msg = "Warning: the path was not stored into the server\r\nIt will be asked during the next program reboot";
                    Alert alert = new Alert(msg, (object s, RoutedEventArgs rea) => { (sender as Button).Tag = "Close"; return; }, "Ok");
                    alert.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    alert.ShowDialog();
                }
            } catch (NetworkException networkException) {
                throw networkException;
            }
        }

        private void chooseNewMonitorDir(object sender, RoutedEventArgs e) {
            try {
                monitorDir = dialog();
                if (monitorDir != null) {
                    (sender as Button).Tag = "Close";
                    userController.deleteUserRepository();                    
                    loadStatus = Loading.LoadFunction.Synchronizing;
                }
            } catch (NetworkException networkException) {
                throw networkException;
            } catch (ServerException serverException) {
                throw serverException;
            }
        }

        private void buttonAutoSynchHandler(object sender, RoutedEventArgs e) {
            IsAutoSynchAble = !IsAutoSynchAble;
        }

        private void buttonRestoreHandler(object sender, RoutedEventArgs e) {
            try {                
                userController.restoreVersion((versionList.SelectedItem as Version).idVersion, monitorDir);
                try {
                    userController.createNewVersion(monitorDir);
                }catch(ServerException){
                    throw;
                }
                userController.watcherInit(monitorDir);
                userController.timerInit();
                updateVersions();
                versionList.ItemsSource = getVersionsToShow();
            } catch (NetworkException) {
                throw;
            } catch (ServerException) {
                printConsole("Restore failed");
            }
        }

        private void alertRestoreVersion(object sender, RoutedEventArgs e) {
            monitorDir = dialog();
            if (monitorDir != null) {
                try{
                    monitorDir = userController.restoreDir(monitorDir);
                    userController.addMonitorDir(monitorDir);
                    (sender as Button).Tag = "Close";
                }catch(ServerException){
                    (sender as Button).Tag = "Close";
                    String msg = "Warning: the path was not stored into the server\r\nIt will be asked during the next program reboot";
                    Alert alert = new Alert(msg, (object s, RoutedEventArgs rea) => { (sender as Button).Tag = "Close"; return; }, "Ok");
                    alert.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    alert.ShowDialog();
                } catch (NetworkException networkException) {
                    throw networkException;
                }
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

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            Console.WriteLine("The Elapsed event was raised at {0:HH:mm:ss.fff}", e.SignalTime);
            printConsole("Timer scattato");
        }
    }
}