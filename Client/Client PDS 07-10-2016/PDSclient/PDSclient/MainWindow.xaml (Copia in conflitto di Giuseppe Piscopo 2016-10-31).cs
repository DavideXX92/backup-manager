using System;
using System.Collections.Generic;

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
        public enum typeOfMessage {ErrorMessage, ServiceMessage};
        public enum programState {Synchronizing, Restoring, Monitoring};

        Dir tree = null;
        Loading.LoadFunction loadStatus = Loading.LoadFunction.Checking;
        Boolean _autoSynch;

        //Nuove variabili
        private bool isLogged;
        public bool isVersionCongruent;
        private UserController userController;
        private User user;
        private string monitorDir;

        private List<Version> versions;
        private int _selectedVersionIndex = -1;
        private bool isVersionClick = false;
        private programState state = programState.Monitoring;

        private System.Timers.Timer timerHello;
        private const int nHelloMissAllowed = 3;
        private int retry;

        Boolean IsAutoSynchAble {
            get { return _autoSynch; }
            set {
                if (value == false) {
                    _autoSynch = false;
                    userController.disableAutoSync();
                    //synchStatus.Content = "Auto synch: Off";
                    manualSynch.Visibility = Visibility.Visible;
                } else if (value == true) {
                    _autoSynch = true;
                    userController.enableAutoSync();
                    //synchStatus.Content = "Auto synch: On";
                    manualSynch.Visibility = Visibility.Hidden;
                }
            }
        }

        public MainWindow() {
            while (true) {
                try {
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
                            writeOnConsole("Non e' stata trovata nessuna cartella da monitorare, non dovrebbe capitare mai!", typeOfMessage.ErrorMessage);
                            return;
                        } else {
                            writeOnConsole("La cartella che stai monitorando e': " + monitorDir, typeOfMessage.ServiceMessage);

                            userController.timerInit();
                            setHelloMessage(5);
                            writeOnConsole("Controllo se sul server sono presenti delle versioni...", typeOfMessage.ServiceMessage);
                            versions = userController.askStoredVersions();
                            writeOnConsole("Versioni recuperate", typeOfMessage.ServiceMessage);
                            checkBoxAutoSynch.IsChecked = true;
                            this.ShowDialog();
                            return;
                        }
                    }
                } catch (NetworkException) {
                    String msg = "Network error\r\nCheck if your Internet connection is up";
                    Alert alert = new Alert(msg, (object sender, RoutedEventArgs e) => { (sender as Button).Tag = "Close"; return; }, "Ok");
                    alert.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    alert.ShowDialog();
                    this.Close();                    
                } catch (ServerException) {
                    String msg = "Server error\r\nThe server may be down or under maintenance";
                    Alert alert = new Alert(msg, (object sender, RoutedEventArgs e) => { (sender as Button).Tag = "Close"; return; }, "Ok");
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

        public void writeOnConsole(string s, typeOfMessage tom) {
            switch (tom) {
                case typeOfMessage.ErrorMessage:
                    TextRange tr = new TextRange(console.Document.ContentEnd, console.Document.ContentEnd);
                    tr.Text = "textToColorize";
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, "#FFCCCC");
                    tr.ApplyPropertyValue(TextElement.FontWeightProperty, "Bold");
                    break;
            }
            console.AppendText(DateTime.Now.ToString("hh:mm:ss tt") + "- " + s + "\n");
        }

        public void showDirectory(object sender, RoutedEventArgs e) {
            paintDirList();
            if (treeView.SelectedItem != null)
                showCurrentPosition((treeView.SelectedItem as Dir).path);
        }

        public void showCurrentPosition(string path) {
            fullPath.Text = monitorDir.Substring(0, monitorDir.LastIndexOf('\\')) + '\\' + path;
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
            } else if (listDataBinding.SelectedItem is File && treeView.SelectedItem != null && treeView.SelectedItem is Dir) {
                String msg = "Do you want to download this file?";
                Alert alert = new Alert(msg, showFile, "Yes", backButton, "No");
                alert.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                alert.ShowDialog();
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
            _selectedVersionIndex = _selectedVersionIndex - 1 >= 0 ? _selectedVersionIndex - 1 : 0;
            versionList.SelectedItem = versionList.Items.GetItemAt(_selectedVersionIndex);         
        }

        private void versionTreeLoaded(object sender, RoutedEventArgs e) {
            fillVersionTree();
            //WORKAROUND-> il selected item non prende il focus, lo prende se si preme 'tab' dopo aver selezionato la listview (perchè??)
            versionList.Focus();
            InputSimulator s = new InputSimulator();
            s.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.TAB);
        }

        private void onMouseVersionClick(object sender, MouseButtonEventArgs e) {
            isVersionClick = true;
        }

        private void versionSelectionChanged(object sender, RoutedEventArgs e) {
            if (versionList.SelectedIndex != -1) {
                Version currentVersion = versionList.SelectedItem as Version;
                tree = currentVersion.dirTree;
                if (isVersionClick) {
                    isVersionClick = false;
                    _selectedVersionIndex = versionList.SelectedIndex;
                }

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

        private void manualSynchHandler(object sender, RoutedEventArgs e) {
            try {
                int ris = userController.manualSync();
                switch (ris) {
                    case -1:
                        writeOnConsole("Another synchronization is running...", typeOfMessage.ErrorMessage);
                        break;
                    case 0:
                        writeOnConsole("Everything up to date", typeOfMessage.ErrorMessage);
                        break;
                    case 1:
                        writeOnConsole("Manual synch succed", typeOfMessage.ServiceMessage);
                        break;
                }
            } catch (ServerException) {
                writeOnConsole("Manual synch failed", typeOfMessage.ErrorMessage);
            } catch (NetworkException) {
                throw;
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
                writeOnConsole("Versione creata correttamente", typeOfMessage.ServiceMessage);
            } catch (ServerException ex){                
                writeOnConsole("Errore durante la creazione della nuova versione: " + ex.Message, typeOfMessage.ServiceMessage);
            } catch(NetworkException ex){
            string error = "Problema di rete: impossibile creare una nuova versione";
                writeOnConsole(error, typeOfMessage.ErrorMessage);
                Console.WriteLine(error + "\nException: " + ex.Message);
                throw;
            }
            
            updateVersions();
            fillVersionTree();
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

        private void checkBoxAutoChecked(object sender, RoutedEventArgs e) {
            IsAutoSynchAble = true;
        }
        private void checkBoxAutoUnchecked(object sender, RoutedEventArgs e) {
            IsAutoSynchAble = false;
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
                writeOnConsole("Restore failed", typeOfMessage.ErrorMessage);
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

        private void showFile(object sender, RoutedEventArgs e) {
            (sender as Button).Tag = "Close";
            
        }

        private void buttonReduceHandler(object sender, RoutedEventArgs e) {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void buttonCloseHandler(object sender, RoutedEventArgs e) {
            if(state == programState.Monitoring)
                Close();
            else {
                String msg;
                if (state == programState.Restoring)
                    msg = "Warning!\r\nChecking in progress.\r\nAre you sure you want to close the program?";
                else
                    msg = "Warning!\r\nSynchronizing in progress.\r\nAre you sure you want to close the program?";

                Alert alert = new Alert(msg, (object s, RoutedEventArgs rea) => { Close(); (sender as Button).Tag = "Close"; return; }, "Yes",
                                            (object s, RoutedEventArgs rea) => { (sender as Button).Tag = "Close"; return; }, "No");
                alert.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                alert.ShowDialog();
            }
        }

        private void windowLeftButtonDownHandler(object sender, MouseButtonEventArgs e) {
            DragMove();
        }

        private void TimerHelloEvent(Object source, ElapsedEventArgs e) {
            try {
                userController.sendHelloMessage();
            } catch (NetworkException) {
                retry++;
                Console.WriteLine("helloMessage persi: " + retry);
                if (retry == nHelloMissAllowed) {
                    Console.WriteLine("Connessione persa...");
                    timerHello.Stop(); timerHello = null;                    
                    retry = 0;
                }
            }
        }

        public void setHelloMessage(int second) {
            if (timerHello == null) {
                timerHello = new System.Timers.Timer(second * 1000);
                timerHello.Elapsed += TimerHelloEvent;
                timerHello.AutoReset = true;
                timerHello.Enabled = true;
                Console.WriteLine("TimerHello set");
            }
        }
    }
}