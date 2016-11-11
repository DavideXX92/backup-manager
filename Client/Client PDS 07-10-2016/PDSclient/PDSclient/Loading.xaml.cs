using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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

namespace PDSclient {
    /// <summary>
    /// Interaction logic for Loading.xaml
    /// </summary>
    public partial class Loading : Window {

        public enum LoadFunction { Checking, Synchronizing, FixingIncongruentVersion, RestoringDir, RestoringLastVersion };
        public delegate void setBool(Boolean boolean);

        private LoadFunction kindFunction;
        private UserController userController;
        private String monitorDirName;
        private setBool setIsVersionCongruent;
        private string closeMessage;

        public Loading() {
            InitializeComponent();            
        }

        public Loading(String monitorDirName, LoadFunction kindFunction, UserController userController, setBool setIsVersionCongruent) {           
            this.userController = userController;
            this.monitorDirName = monitorDirName;
            this.setIsVersionCongruent = setIsVersionCongruent;

            InitializeComponent();
            setMonitorDir();
            setFunction(kindFunction);            
        }

        private void setMonitorDir() {
            loadWindowMonitorDir.Content = monitorDirName;
        }    

        private void setFunction(LoadFunction kindFunction) {
            this.kindFunction = kindFunction;

            switch(kindFunction){                            
                case LoadFunction.Checking:
                    functionLabel.Content = "Checking version...";
                    closeMessage = "Warning!\r\nChecking in progress.\r\nAre you sure you want to close the program?";
                    userController.watcherInit(monitorDirName);
                    break;
                case LoadFunction.Synchronizing:
                    functionLabel.Content = "Scanning file system...";
                    closeMessage = "Warning!\r\nSynchronizing in progress.\r\nAre you sure you want to close the program?";
                    userController.watcherInit(monitorDirName);
                    break;
                case LoadFunction.FixingIncongruentVersion:
                    functionLabel.Content = "Fixing incongruent version";
                    closeMessage = "Warning!\r\nFixing incongruent version in progress.\r\nAre you sure you want to close the program?";
                    loadingProgress.IsIndeterminate = false;
                    loadingProgress.Value = 0;
                    userController.watcherInit(monitorDirName);
                    break;
                case LoadFunction.RestoringDir:
                    functionLabel.Content = "Restoring to the last version";
                    closeMessage = "Warning!\r\nRestoring version in progress.\r\nAre you sure you want to close the program?";
                    loadingProgress.IsIndeterminate = false;
                    break;
            }            
        }

        private void onWindowShown(object sender, EventArgs e) {
            System.Threading.Thread.Sleep(300);
            //userController.setProgressBarToNotify(loadingProgress);
            switch (kindFunction) {
                case LoadFunction.Synchronizing:
                    try {
                        newWorker(functionAsynchronous.Synchronize, synchronizeWorkerComplete, new List<Object> { monitorDirName });
                    } catch (Exception) {
                        throw;
                    }
                    break;
                case LoadFunction.Checking:
                    try{
                        newWorker(functionAsynchronous.CheckVersion, checkingWorkerComplete, new List<object> {monitorDirName});
                    }catch(Exception){
                        throw;
                    }
                    break;
                case LoadFunction.RestoringDir:
                    try {
                        newWorker(functionAsynchronous.RestoreDir, RestoreWorkerComplete, new List<object> { monitorDirName });
                    } catch (Exception) {
                        throw;
                    }
                    break;
            }
        }

        void checkingWorkerComplete(object sender, RunWorkerCompletedEventArgs e) {
            try {
                if (e.Error != null) {
                    throw e.Error;
                }
                if ((Boolean)e.Result) {
                    Console.WriteLine("Le due versioni sono uguali");
                    if (userController.checkIfthereAreFileToSend()) {
                        functionLabel.Content = "Uploading missing files";                        
                        setFunction(LoadFunction.FixingIncongruentVersion);
                        newWorker(functionAsynchronous.UploadFile, uploadFileWorkerComplete, new List<object> { monitorDirName });
                    } else {
                        setIsVersionCongruent(true);
                        Close();
                    }
                }else{
                    Console.WriteLine("Le due versioni sono diverse");
                    String msg = "The version does not match with the one stored into the server.\r\nDo you want to download the latest version\r\nor do you want to upload the current version?";
                    Alert alert = new Alert(msg, buttonRestoreHandler, "Download", newVersionHandler, "Upload");
                    showCenterdWindow(alert, true);
                    if(kindFunction == LoadFunction.Checking)
                        Close();
                }
            } catch (NetworkException) {
                throw;
            } catch (ServerException) {
                throw;
            } catch (Exception) {
                throw;
            }            
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            if (loadingProgress.IsIndeterminate == true && kindFunction == LoadFunction.Synchronizing) {
                functionLabel.Content = "Synchronizing...";
                loadingProgress.IsIndeterminate = false;
            }
            loadingProgress.Value = e.ProgressPercentage;
        }

        void uploadFileWorkerComplete(object sender, RunWorkerCompletedEventArgs e) {
            try {
                if (e.Error != null) {
                    throw e.Error;
                }
                setIsVersionCongruent(true);
                Close();
            } catch (Exception) {
                throw;
            }
        }

        void synchronizeWorkerComplete(object sender, RunWorkerCompletedEventArgs e) {
            try {
                if (e.Error != null) {
                    throw e.Error;
                }
                setIsVersionCongruent(true);
                try {
                    userController.addMonitorDir(monitorDirName);
                } catch (ServerException) {
                    String msg = "Warning: the path was not stored into the server\r\nIt will be asked during the next program reboot";
                    Alert alert = new Alert(msg, (object s, RoutedEventArgs rea) => { (sender as Button).Tag = "Close"; return; }, "Ok");
                    alert.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    alert.ShowDialog();
                }
            }catch(NetworkException){
                throw;
            } catch (ServerException) {
                throw;
            } catch (Exception) {
                throw;
            }
        
            Close(); 
        }

        void newVersionWorkerComplete(object sender, RunWorkerCompletedEventArgs e) {
            try {
                if (e.Error != null) {
                    throw e.Error;
                }
                setIsVersionCongruent(true);
                this.Close();
                Console.WriteLine("Versione creata correttamente");
            } catch (ServerException serverException) {
                Console.WriteLine("Errore durante la creazione della nuova versione: " + serverException.Message);
                throw serverException;
            } catch (NetworkException networkException) {
                string error = "Problema di rete: impossibile creare una nuova versione";
                Console.WriteLine(error);
                Console.WriteLine(error + "\nException: " + networkException.Message);
                throw networkException;
            } catch (Exception) {
                throw;
            }
        }

        void RestoreWorkerComplete(object sender, RunWorkerCompletedEventArgs e) {
            try {
                if (e.Error != null) {
                    throw e.Error;
                }
                userController.watcherInit(monitorDirName);
                userController.createNewVersion(monitorDirName);               
                setIsVersionCongruent(true);
                this.Close();
            } catch (NetworkException) {
                Console.WriteLine("Dir restore failed");
                throw;
            } catch (Exception) {
                throw;
            }
        }

        private void buttonRestoreHandler(object sender, RoutedEventArgs e) {
            try {
                (sender as Button).Tag = "Close";
                setFunction(LoadFunction.RestoringLastVersion);
                newWorker(functionAsynchronous.RestoreLastVersion, RestoreWorkerComplete, new List<object> { monitorDirName });
            } catch (Exception ex) {
                Console.WriteLine(ex);
                throw ex;
            }
        }

        private void newVersionHandler(object sender, RoutedEventArgs e) {
            try {
                (sender as Button).Tag = "Close";
                setFunction(LoadFunction.Synchronizing);
                loadingProgress.IsIndeterminate = true;
                newWorker(functionAsynchronous.CreateNewVersion, newVersionWorkerComplete, new List<object> { monitorDirName });
            } catch (Exception ex) {
                Console.WriteLine(ex);
                throw ex;
            }
        }

        private void newWorker(functionAsynchronous functionToRun, RunWorkerCompletedEventHandler handler, List<object> optional_parameters) {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += userController.runThread;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += handler;
            List<object> parameters = new List<object> { functionToRun };
            parameters.AddRange(optional_parameters);
            worker.RunWorkerAsync(parameters);
        }

        private void buttonReduceHandler(object sender, RoutedEventArgs e) {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void buttonCloseHandler(object sender, RoutedEventArgs e) {
            Alert alert = new Alert(closeMessage, (object s, RoutedEventArgs rea) => { (s as Button).Tag = "Close"; Close(); }, "Yes", (object s, RoutedEventArgs rea) => { (s as Button).Tag = "Close"; return; }, "No");
            showCenterdWindow(alert, true);
        }

        private void windowLeftButtonDownHandler(object sender, MouseButtonEventArgs e) {
            DragMove();
        }
        private void showCenterdWindow(Window window, Boolean topmost) {
            try { 
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.Topmost = topmost;
            window.ShowDialog();
            } catch (Exception) {
                throw;
            }
        }
    }
}
