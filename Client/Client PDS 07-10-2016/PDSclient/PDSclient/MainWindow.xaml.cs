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
using System.ComponentModel;
using System.Windows.Threading;
using Hardcodet.Wpf.TaskbarNotification;

namespace PDSclient {
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public enum typeOfMessage { ErrorMessage, ServiceMessage, SuccessMessage };

    public partial class MainWindow : Window {
        public delegate void writeString(String text);        
        public enum programState { Synchronizing, Restoring, CreatingNewVersion, ManualSynchronizing, Monitoring };

        Dir tree = null;
        Loading.LoadFunction loadStatus = Loading.LoadFunction.Checking;
        Boolean _autoSynch;

        private List<Button> buttonToDisable = null;
        private List<ProgressBar> progressBars = null;

        //Nuove variabili                                               
        private List<Version> versions;
        private int _selectedVersionIndex = -1;
        private bool isVersionClick = false;
        private programState state = programState.Monitoring;

        System.Windows.Threading.DispatcherTimer helloTimer;
        private const int nHelloMissAllowed = 3;
        private int retry;

        private bool isVersionCongruent;

        UserController userController;
        String monitorDir;

        Boolean IsAutoSynchAble {
            get { return _autoSynch; }
            set {
                if (value == false) {
                    _autoSynch = false;
                    userController.disableAutoSync();                    
                    manualSynch.Visibility = Visibility.Visible;
                    trayIcon.ToolTipText = "Autosynch: OFF";
                } else if (value == true) {
                    _autoSynch = true;
                    userController.enableAutoSync();                    
                    manualSynch.Visibility = Visibility.Hidden;
                    trayIcon.ToolTipText = "Autosynch: ON";
                }
            }
        }

        public MainWindow() { }

        public MainWindow(UserController userController, String monitorDir) {
            versions = null;
            this.monitorDir = monitorDir;
            isVersionCongruent = false;
            this.userController = userController;

            InitializeComponent();
            buttonToDisable = new List<Button>() { buttonChangeDir, manualSynch, newVersion, buttonRestore };
            progressBars = new List<ProgressBar>() { buttonRestoreProgress, newVersionProgress, manualSynchProgress };
            state = programState.Monitoring;

            writeOnConsole("The monitored directory is: " + monitorDir, typeOfMessage.ServiceMessage);

            userController.timerInit(autoSynchWorkerComplete, autoSynch_ProgressChanged);
            setHelloMessage(5);
            writeOnConsole("Asking for stored versions...", typeOfMessage.ServiceMessage);
            versions = userController.askStoredVersions();
            writeOnConsole("Versions retrieved: " + (versions.Count - 1), typeOfMessage.ServiceMessage);
            checkBoxAutoSynch.IsChecked = true;

            trayIcon.DoubleClickCommandParameter = this;
        }

        public void paintDirTree() {
            if (!treeView.Items.IsEmpty)
                treeView.Items.Clear();

            treeView.Items.Add(tree);
            if (treeView.SelectedItem == null) {
                if (!treeView.Items.IsEmpty) {
                    (treeView.Items.GetItemAt(0) as Dir).IsSelected = true; //QUAAAAA
                    treeView.Items.Refresh();
                }
            }
        }

        public void paintDirList() {            
            if (treeView.SelectedItem != null)
                listDataBinding.ItemsSource = (treeView.SelectedItem as Dir).exploreDir();
        }

        public void writeOnConsole(string s, typeOfMessage tom) {
            TextRange tr = new TextRange(console.Document.ContentEnd, console.Document.ContentEnd);
            String txt = DateTime.Now.ToString("hh:mm:ss tt") + "- " + s + "\n";

            switch (tom) {
                case typeOfMessage.ErrorMessage:
                    tr.Text = txt;
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, "#FFCCCC");
                    //tr.ApplyPropertyValue(TextElement.FontWeightProperty, "Bold");
                    break;
                case typeOfMessage.ServiceMessage:
                    tr.Text = txt;
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                    break;
                case typeOfMessage.SuccessMessage:
                    tr.Text = txt;
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                    break;
            }
            console.ScrollToEnd();
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

        /*
         *  Le seguenti funzioni vengono richiamate da un alert 
         */


        private void manualSynchWorkerComplete(object sender, RunWorkerCompletedEventArgs e) {
            try {
                if (e.Error != null) {
                    throw e.Error;
                }
                int ris = (int)e.Result;
                enableButtons();
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
                //throw;
                writeOnConsole("Network error", typeOfMessage.ErrorMessage);
            } finally {
                state = programState.Monitoring;
            }
        }

        private void manualSynchHandler(object sender, RoutedEventArgs e) {
            try {
                state = programState.ManualSynchronizing;
                disableButtons();
                newWorker(functionAsynchronous.ManualSynch, manualSynchWorkerComplete, new List<Object>());
            } catch (Exception ex) {
                Console.WriteLine(ex);
            }
        }

        private void newSynchHandler(object sender, RoutedEventArgs e) {
            String msg = "Are you sure you want to choose an other directory?\r\nAll the stored data will be lost!";
            Alert alert = new Alert(msg, chooseNewMonitorDir, "Choose dir", backButton, "Back");
            alert.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            alert.Topmost = true;
            alert.ShowDialog();
            if (loadStatus == Loading.LoadFunction.Synchronizing)
                (sender as Button).Tag = "Close";
        }

        private void newVersionHandler(object sender, RoutedEventArgs e) {
            String msg = "The current version will be closed and a new one will be created\r\nAre you sure you want to proceed?";
            Alert alert = new Alert(msg, alertNewVersionHandler, "Yes", backButton, "No");
            alert.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            alert.Topmost = true;
            alert.ShowDialog();
        }

        private void newVersionWorkerComplete(object sender, RunWorkerCompletedEventArgs e) {
            try {
                if (e.Error != null) {
                    throw e.Error;
                }
                writeOnConsole("Version creation successful", typeOfMessage.SuccessMessage);
                enableButtons();

            } catch (ServerException ex) {
                writeOnConsole("Error during the creation of a new directory: " + ex.Message, typeOfMessage.ServiceMessage);
            } catch (NetworkException ex) {
                writeOnConsole("Network error", typeOfMessage.ErrorMessage);
                //throw;
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            } finally {
                state = programState.Monitoring;
            }

            updateVersions();
            fillVersionTree();
        }

        private void alertNewVersionHandler(object sender, RoutedEventArgs e) {
            try {
                state = programState.CreatingNewVersion;
                disableButtons();
                (sender as Button).Tag = "Close";
                newWorker(functionAsynchronous.CreateNewVersion, newVersionWorkerComplete, new List<object>() { monitorDir });
            } catch (BusyResourceException) {
                writeOnConsole("Request denied.\r\nAnother operation is in progress, wait until it ends", typeOfMessage.ErrorMessage);
            } catch (Exception) {
                throw;
            }
        }

        private void backButton(object sender, RoutedEventArgs e) {
            (sender as Button).Tag = "Close";
        }



        private void chooseNewMonitorDir(object sender, RoutedEventArgs e) {
            try {
                monitorDir = dialog();
                if (monitorDir != null) {
                    (sender as Button).Tag = "Close";
                    userController.watcherDelete();
                    userController.deleteUserRepository();
                    loadStatus = Loading.LoadFunction.Synchronizing;
                }
            } catch (NetworkException networkException) {
                //throw networkException;
                writeOnConsole("Network error", typeOfMessage.ErrorMessage);
            } catch (ServerException serverException) {
                throw serverException;
            } catch (BusyResourceException) {
                writeOnConsole("Request denied.\r\nAnother operation is in progress, wait until it ends", typeOfMessage.ErrorMessage);
            }
        }

        private void checkBoxAutoChecked(object sender, RoutedEventArgs e) {
            IsAutoSynchAble = true;
        }
        private void checkBoxAutoUnchecked(object sender, RoutedEventArgs e) {
            IsAutoSynchAble = false;
        }

        private void restoreWorkerComplete(object sender, RunWorkerCompletedEventArgs e) {
            try {
                if (e.Error != null) {
                    throw e.Error;
                }
                try {
                    userController.createNewVersion(monitorDir);
                    writeOnConsole("Version restored successful", typeOfMessage.SuccessMessage);
                } catch (ServerException) {
                    throw;
                } catch (BusyResourceException) {
                    writeOnConsole("Request denied.\r\nAnother operation is in progress, wait until it ends", typeOfMessage.ErrorMessage);
                }
                //userController.watcherInit(monitorDir);
                
                updateVersions();
                fillVersionTree();
                enableButtons();
            } catch (NetworkException) {
                //throw;
                writeOnConsole("Network error", typeOfMessage.ErrorMessage);
            } catch (ServerException) {
                writeOnConsole("Restore failed", typeOfMessage.ErrorMessage);
            } finally {
                state = programState.Monitoring;
            }
        }

        private void changeMonitoredDirHandler(object sender, RoutedEventArgs e) {
            try {
                loadStatus = Loading.LoadFunction.Checking; //Serve per permettere all'alert di settare la variabile a 'syncrhonizing'
                String msg = "Are you sure you want to choose an other directory?\r\nAll the stored data will be lost!";
                Alert alert = new Alert(msg, chooseNewMonitorDir, "Choose dir", backButton, "Back");
                alert.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                alert.Topmost = true;
                alert.ShowDialog();
                if (loadStatus == Loading.LoadFunction.Synchronizing) {
                    //(sender as Button).Tag = "Close";
                    Loading load_window = new Loading(monitorDir, loadStatus, userController, (Boolean var) => { isVersionCongruent = var; });
                    load_window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    this.Hide();
                    load_window.ShowDialog();
                    if (!isVersionCongruent)
                        throw new IncontruentVersionException(); //MODOFICARE

                    listDataBinding.ItemsSource = null;
                    updateVersions();
                    _selectedVersionIndex = 0;
                    fillVersionTree();
                    writeOnConsole("Monitor directory changed: " + monitorDir, typeOfMessage.SuccessMessage);
                    this.ShowDialog();                    
                }
            } catch (ServerException) {
                throw;
            } catch (NetworkException) {
                //throw;
                writeOnConsole("Network error", typeOfMessage.ErrorMessage);
            } catch (IncontruentVersionException) {
                throw;
            } catch (Exception) {
                throw;
            }
        }

        private void buttonRestoreHandler(object sender, RoutedEventArgs e) {
            try {
                state = programState.Restoring;
                disableButtons();
                newWorker(functionAsynchronous.RestoreVersion, restoreWorkerComplete, new List<object>() { (versionList.SelectedItem as Version).idVersion, monitorDir });
            } catch (Exception) {
                throw;
            }
        }



        private void buttonReduceHandler(object sender, RoutedEventArgs e) {
            this.WindowState = System.Windows.WindowState.Minimized;
            this.ShowInTaskbar = false;
        }

        private void buttonCloseHandler(object sender, RoutedEventArgs e) {
            try {
                if (state == programState.Monitoring) {
                    Close();
                    userController.logout();
                } else {
                    String msg;
                    if (state == programState.Restoring)
                        msg = "Warning!\r\nChecking in progress.\r\nAre you sure you want to close the program?";
                    else
                        msg = "Warning!\r\nSynchronizing in progress.\r\nAre you sure you want to close the program?";

                    Alert alert = new Alert(msg, (object s, RoutedEventArgs rea) => { Close(); (sender as Button).Tag = "Close"; return; }, "Yes",
                                                (object s, RoutedEventArgs rea) => { (sender as Button).Tag = "Close"; return; }, "No");
                    alert.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    alert.Topmost = true;
                    alert.ShowDialog();
                }
            } catch (BusyResourceException) {
                writeOnConsole("Request denied.\r\nAnother operation is in progress, wait until it ends", typeOfMessage.ErrorMessage);
            } catch (Exception) {
                throw;
            }
        }

        private void windowLeftButtonDownHandler(object sender, MouseButtonEventArgs e) {
            DragMove();
        }

        private void helloWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            try {
                if (e.Error != null) {
                    throw e.Error;
                }
                retry = 0;
            } catch (NetworkException) {
                retry++;
                Console.WriteLine("helloMessage persi: " + retry);
                if (retry >= nHelloMissAllowed) {
                    Console.WriteLine("Connessione persa...");
                    writeOnConsole("Connection lost...", typeOfMessage.ErrorMessage);
                    unsetHelloMessage();
                    throw new NetworkException();
                }
            }
        }
        private void OnTimerTic(Object source, EventArgs e) {
            newWorker(functionAsynchronous.Hello, helloWorkerCompleted, new List<object>());
        }
        public void setHelloMessage(int second) {
            if (helloTimer == null) {

                helloTimer = new System.Windows.Threading.DispatcherTimer();
                helloTimer.Tick += new EventHandler(OnTimerTic);
                helloTimer.Interval = new TimeSpan(0, 0, second);
                helloTimer.Start();
                Console.WriteLine("TimerHello set");
            }
        }
        private void unsetHelloMessage() {
            helloTimer.Stop();
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            foreach (ProgressBar progressBar in progressBars)
                if (progressBar.Visibility != Visibility.Collapsed) {
                    progressBar.Value = e.ProgressPercentage;
                    break;
                }
        }

        private void autoSynch_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            if (e.ProgressPercentage == -1) {
                writeOnConsole("Synchronizing...", typeOfMessage.ServiceMessage);
                showBaloonInTrayIcon("Synchronizing...", "Synchronization in progress", typeOfMessage.ServiceMessage);                
            }
            if (state != programState.Synchronizing) {
                state = programState.Synchronizing;
            }
        }
        private void autoSynchWorkerComplete(object sender, RunWorkerCompletedEventArgs e) {
            try {
                if (e.Error != null) {
                    throw e.Error;
                }
                if (state == programState.Synchronizing) {
                    state = programState.Monitoring;
                    writeOnConsole("Synchronization... done", typeOfMessage.ServiceMessage);
                    showBaloonInTrayIcon("Synchronizing...", "Synchronization complete", typeOfMessage.ServiceMessage);
                }
            } catch (Exception ex) {
                if (ex is BusyResourceException)
                    Console.WriteLine("Impossibile aggiornare la versione se si sta eseguendo già un'altra operazione: " + ex.Message);
                else
                    Console.WriteLine("Impossibile aggiornare la versione. \nException: " + ex.Message);
            }
        }

        private void newWorker(functionAsynchronous functionToRun, RunWorkerCompletedEventHandler handler, List<object> optional_parameters) {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += userController.runThread;//userController.runThread;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += handler;
            List<object> parameters = new List<object> { functionToRun };
            parameters.AddRange(optional_parameters);
            worker.RunWorkerAsync(parameters);
        }

        public void showBaloonInTrayIcon(String title, String text, typeOfMessage tom) {
            switch (tom) {
                case typeOfMessage.ErrorMessage:
                    trayIcon.ShowBalloonTip(title, text, BalloonIcon.Error);
                    break;
                case typeOfMessage.ServiceMessage:                                        
                    trayIcon.ShowBalloonTip(title, text, BalloonIcon.Info);                    
                    break;
            }            
        }

        /*
         *Disabilita i bottoni durante l'esecuzione di un'operazione in background
         */
        private void disableButtons() {
            Button sender = null;
            ProgressBar senderProgress = null;

            switch (state) {
                case programState.Restoring:
                    sender = buttonRestore;
                    senderProgress = buttonRestoreProgress;
                    break;
                case programState.ManualSynchronizing:
                    sender = manualSynch;
                    senderProgress = manualSynchProgress;
                    break;
                case programState.CreatingNewVersion:
                    sender = newVersion;
                    senderProgress = newVersionProgress;
                    break;
            }
            foreach (Button btn in buttonToDisable)
                if (sender == null || btn.Name != sender.Name)
                    btn.IsEnabled = false;

            if (sender != null && senderProgress != null) {
                senderProgress.Value = 0;
                senderProgress.Visibility = Visibility.Visible;
                sender.Visibility = Visibility.Collapsed;
            }
        }

        private void enableButtons() {
            Button sender = null;
            ProgressBar senderProgress = null;

            switch (state) {
                case programState.Restoring:
                    sender = buttonRestore;
                    senderProgress = buttonRestoreProgress;
                    break;
                case programState.ManualSynchronizing:
                    sender = manualSynch;
                    senderProgress = manualSynchProgress;
                    break;
                case programState.CreatingNewVersion:
                    sender = newVersion;
                    senderProgress = newVersionProgress;
                    break;
            }
            foreach (Button btn in buttonToDisable)
                if (sender == null || btn.Name != sender.Name)
                    btn.IsEnabled = true;

            if (sender != null && senderProgress != null) {
                senderProgress.Value = 0;
                senderProgress.Visibility = Visibility.Collapsed;
                sender.Visibility = Visibility.Visible;
            }
        }
    }
}