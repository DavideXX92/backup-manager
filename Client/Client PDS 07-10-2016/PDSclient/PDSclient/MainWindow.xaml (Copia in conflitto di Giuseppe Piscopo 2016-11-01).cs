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

namespace PDSclient {
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public delegate void writeString(String text);
        public enum typeOfMessage {ErrorMessage, ServiceMessage};
        public enum programState {Synchronizing, Restoring, CreatingNewVersion, ManualSynchronizing, Monitoring};

        Dir tree = null;
        Loading.LoadFunction loadStatus = Loading.LoadFunction.Checking;
        Boolean _autoSynch;

        private List<Button> buttonToDisable = null;
        private List<ProgressBar> progressBars = null;

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

        System.Windows.Threading.DispatcherTimer helloTimer;
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
                    retry = 0;

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
                    buttonToDisable = new List<Button>() { buttonChangeDir, manualSynch, newVersion, buttonRestore };
                    progressBars = new List<ProgressBar>() { buttonRestoreProgress, newVersionProgress, manualSynchProgress };
                    state = programState.Monitoring;

                    if (isLogged) {
                        if (user.monitorDir == null) {
                            writeOnConsole("Non e' stata trovata nessuna cartella da monitorare, non dovrebbe capitare mai!", typeOfMessage.ErrorMessage);
                            return;
                        } else {
                            writeOnConsole("La cartella che stai monitorando e': " + monitorDir, typeOfMessage.ServiceMessage);

                            //userController.timerInit(new DispatcherWinFormsCompatAdapter(this.Dispatcher), autoSynchWorkerComplete, autoSynch_ProgressChanged);
                            userController.timerInit(autoSynchWorkerComplete, autoSynch_ProgressChanged);
                            //userController.timerInit();
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
            TextRange tr = new TextRange(console.Document.ContentEnd, console.Document.ContentEnd);
            String txt = DateTime.Now.ToString("hh:mm:ss tt") + "- " + s + "\n";

            switch (tom) {
                case typeOfMessage.ErrorMessage:
                    tr.Text = txt;
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, "#FFCCCC");
                    tr.ApplyPropertyValue(TextElement.FontWeightProperty, "Bold");
                    break;
                case typeOfMessage.ServiceMessage:
                    tr.Text = txt;
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                    break;
            }                        
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

        private void manualSynchWorkerComplete(object sender, RunWorkerCompletedEventArgs e) {
            try {
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
                throw;
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
            String msg = "The current version will be closed and a new one will be created\r\nAre you sure you want to proceed?";
            Alert alert = new Alert(msg, alertNewVersionHandler, "Yes", backButton, "No");
            alert.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            alert.ShowDialog();
        }

        private void newVersionWorkerComplete(object sender, RunWorkerCompletedEventArgs e) {
            try
            {
                object ris = e.Result; //Per catturare l'eventuale eccezione generata nel worker
                writeOnConsole("Versione creata correttamente", typeOfMessage.ServiceMessage);
                enableButtons();

            }
            catch (ServerException ex)
            {
                writeOnConsole("Errore durante la creazione della nuova versione: " + ex.Message, typeOfMessage.ServiceMessage);
            }
            catch (NetworkException ex)
            {
                string error = "Problema di rete: impossibile creare una nuova versione";
                writeOnConsole(error, typeOfMessage.ErrorMessage);
                Console.WriteLine(error + "\nException: " + ex.Message);
                throw;
            }
            finally
            {
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
            } 
            catch (Exception) {
                throw;
            }
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
            } catch (BusyResourceException) {
                writeOnConsole("Request denied.\r\nAnother operation is in progress, wait until it ends", typeOfMessage.ErrorMessage);
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
                object ris = e.Result; //Per catturare l'eventuale eccezione generata nel worker
                try {
                    userController.createNewVersion(monitorDir);
                } catch (ServerException) {
                    throw;
                } catch (BusyResourceException) {
                    writeOnConsole("Request denied.\r\nAnother operation is in progress, wait until it ends", typeOfMessage.ErrorMessage);
                }
                userController.watcherInit(monitorDir);
                //userController.timerInit(new DispatcherWinFormsCompatAdapter(this.Dispatcher), autoSynchWorkerComplete, autoSynch_ProgressChanged);
                userController.timerInit(autoSynchWorkerComplete, autoSynch_ProgressChanged);
                //userController.timerInit();
                updateVersions();
                versionList.ItemsSource = getVersionsToShow();
                enableButtons();
            } catch (NetworkException) {
                throw;
            } catch (ServerException) {
                writeOnConsole("Restore failed", typeOfMessage.ErrorMessage);
            } finally {
                state = programState.Monitoring;
            }
        }

        private void buttonRestoreHandler(object sender, RoutedEventArgs e) {
            try {
                state = programState.Restoring;
                disableButtons();
                newWorker(functionAsynchronous.RestoreVersion, restoreWorkerComplete, new List<object>() { (versionList.SelectedItem as Version).idVersion, monitorDir});
            } catch (Exception) {
                throw;
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
            try{
                if (state == programState.Monitoring) {
                    Close();
                    userController.logout();
                } else {
                    String msg;
                    if (state == programState.Restoring)
                        msg = "Warning!\r\nChecking in progress.\r\nAre you sure you want to close the program?";
                    else
                        msg = "Warning!\r\nSynchronizing in progress.\r\nAre you sure you want to close the program?";

                    Alert alert = new Alert(msg, (object s, RoutedEventArgs rea) => { userController.logout(); Close(); (sender as Button).Tag = "Close"; return; }, "Yes",
                                                (object s, RoutedEventArgs rea) => { (sender as Button).Tag = "Close"; return; }, "No");
                    alert.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    alert.ShowDialog();
                }
            }catch(BusyResourceException){
                writeOnConsole("Request denied.\r\nAnother operation is in progress, wait until it ends", typeOfMessage.ErrorMessage);
            }catch (Exception){
                throw;
            }
        }

        private void windowLeftButtonDownHandler(object sender, MouseButtonEventArgs e) {
            DragMove();
        }

        private void helloWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            try {
                object ris = e.Result; //Serve per far richiamare l'eventuale eccezione scatenata nel worker
                retry = 0;
            } catch (NetworkException) {
                retry++;
                Console.WriteLine("helloMessage persi: " + retry);
                if (retry >= nHelloMissAllowed) {
                    Console.WriteLine("Connessione persa...");
                    throw new NetworkException();   
                }
            }
        }
        private void OnTimerTic(Object source, EventArgs e) {
            newWorker(functionAsynchronous.Hello, helloWorkerCompleted, new List<object>());
        }
        public void setHelloMessage(int second) {
            if (helloTimer == null) {

                /*timer = new System.Timers.Timer(10000);
                timer.Elapsed += OnTimedEvent;                
                timer.SynchronizingObject = currentThread;
                timer.AutoReset = true;*/                
                helloTimer = new System.Windows.Threading.DispatcherTimer();
                helloTimer.Tick += new EventHandler(OnTimerTic);
                helloTimer.Interval = new TimeSpan(0, 0, 5);
                helloTimer.Start();
                Console.WriteLine("TimerHello set");
            }
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e) {                        
            foreach(ProgressBar progressBar in progressBars)
                if (progressBar.Visibility != Visibility.Collapsed) {
                    progressBar.Value = e.ProgressPercentage;
                    break;
                }
        }


        private void autoSynch_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            if (e.ProgressPercentage == -1) {
                writeOnConsole("Synchronizing...", typeOfMessage.ServiceMessage);
            }
            if (state != programState.Synchronizing) {
                state = programState.Synchronizing;               
            }
        }
        private void autoSynchWorkerComplete(object sender, RunWorkerCompletedEventArgs e) {
            try {
                object ris = e.Result; //Per lanciare l'eventuale eccezione genearta nel backgroundWorker
                if (state == programState.Synchronizing) {
                    state = programState.Monitoring;
                    writeOnConsole("Synchronizing... done", typeOfMessage.ServiceMessage);
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


    internal class DispatcherWinFormsCompatAdapter : ISynchronizeInvoke {
        #region IAsyncResult implementation
        private class DispatcherAsyncResultAdapter : IAsyncResult {
            private DispatcherOperation m_op;
            private object m_state;

            public DispatcherAsyncResultAdapter(DispatcherOperation operation) {
                m_op = operation;
            }

            public DispatcherAsyncResultAdapter(DispatcherOperation operation, object state)
                : this(operation) {
                m_state = state;
            }

            public DispatcherOperation Operation {
                get { return m_op; }
            }

            #region IAsyncResult Members

            public object AsyncState {
                get { return m_state; }
            }

            public WaitHandle AsyncWaitHandle {
                get { return null; }
            }

            public bool CompletedSynchronously {
                get { return false; }
            }

            public bool IsCompleted {
                get { return m_op.Status == DispatcherOperationStatus.Completed; }
            }

            #endregion
        }
        #endregion
        private Dispatcher m_disp;
        public DispatcherWinFormsCompatAdapter(Dispatcher dispatcher) {
            m_disp = dispatcher;
        }
        #region ISynchronizeInvoke Members

        public IAsyncResult BeginInvoke(Delegate method, object[] args) {
            if (args != null && args.Length > 1) {
                object[] argsSansFirst = GetArgsAfterFirst(args);
                DispatcherOperation op = m_disp.BeginInvoke(DispatcherPriority.Normal, method, args[0], argsSansFirst);
                return new DispatcherAsyncResultAdapter(op);
            } else {
                if (args != null) {
                    return new DispatcherAsyncResultAdapter(m_disp.BeginInvoke(DispatcherPriority.Normal, method, args[0]));
                } else {
                    return new DispatcherAsyncResultAdapter(m_disp.BeginInvoke(DispatcherPriority.Normal, method));
                }
            }
        }

        private static object[] GetArgsAfterFirst(object[] args) {
            object[] result = new object[args.Length - 1];
            Array.Copy(args, 1, result, 0, args.Length - 1);
            return result;
        }

        public object EndInvoke(IAsyncResult result) {
            DispatcherAsyncResultAdapter res = result as DispatcherAsyncResultAdapter;
            if (res == null)
                throw new InvalidCastException();

            while (res.Operation.Status != DispatcherOperationStatus.Completed || res.Operation.Status == DispatcherOperationStatus.Aborted) {
                Thread.Sleep(50);
            }

            return res.Operation.Result;
        }

        public object Invoke(Delegate method, object[] args) {
            if (args != null && args.Length > 1) {
                object[] argsSansFirst = GetArgsAfterFirst(args);
                return m_disp.Invoke(DispatcherPriority.Normal, method, args[0], argsSansFirst);
            } else {
                if (args != null) {
                    return m_disp.Invoke(DispatcherPriority.Normal, method, args[0]);
                } else {
                    return m_disp.Invoke(DispatcherPriority.Normal, method);
                }
            }
        }

        public bool InvokeRequired {
            get { return m_disp.Thread != Thread.CurrentThread; }
        }

        #endregion
    }
}