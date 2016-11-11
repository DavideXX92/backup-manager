using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace PDSclient {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        static UserController userController;
        static User user;
        static string monitorDir;
        static Loading.LoadFunction loadStatus;
        static Signin login_form;

        public App(){
            InitializeComponent();
        }

        [STAThread]
        static void Main() {       
            App app = new App();
            app.Startup += onStartUp;                  
            app.Run();       
        }

        private static void onStartUp(object app, StartupEventArgs startupEventArgs) {           
            Loading load_window;
            MainWindow mainWindow = null;
            MainWindow main = null;
            bool isLogged;
            bool isVersionCongruent;
            loadStatus = Loading.LoadFunction.Checking;
            
            while (true) {
                try {
                    userController = null;
                    user = null;
                    monitorDir = null;
                    isLogged = false;
                    isVersionCongruent = false;
                    main = new MainWindow();
                    login_form = new Signin((UserController userContr) => { userController = userContr; }, (User u) => { user = u; });

                    showCenteredWindow(login_form, false);

                    if (userController != null && user != null)
                        isLogged = true;

                    if (!isLogged) {                        
                        Application.Current.Shutdown();                        
                        return;
                    }

                    if (user.monitorDir.Count > 0) {
                        monitorDir = checkMonitorDir(user.monitorDir);
                        if (monitorDir == null) {
                            String msg = "Directory not found in the current device.\r\nLocate the actual path, download the last version\r\n or perform a new synchronization";
                            Alert alert = new Alert(msg, chooseMonitorDirPosition, "Locate path", alertRestoreVersion, "Last version", noDirInDeviceHandler, "New synch");
                            showCenteredWindow(alert, true);
                        }
                    } else { //if no items are in user.monitorDir, there is no syncrhonized directory in the server for the current user
                        String msg = "Choose a directory to monitor";
                        Alert alert = new Alert(msg, chooseNewMonitorDir, "Path");
                        showCenteredWindow(alert, true);
                    }

                    if (monitorDir == null) {//alert window should set a monitorDir
                        Application.Current.Shutdown();   
                        return;
                    }

                    load_window = new Loading(monitorDir, loadStatus, userController, (Boolean var) => { isVersionCongruent = var; });
                    showCenteredWindow(load_window, false);

                    if (!isVersionCongruent) {
                        throw new IncontruentVersionException();
                    }
                    mainWindow = new MainWindow(userController, monitorDir);
                    showCenteredWindow(mainWindow, false);
                    Application.Current.Shutdown();
                    break;
                } catch (NetworkException) {
                    if (mainWindow.ShowInTaskbar == true) {
                        String msg = "Network error\r\nCheck if your Internet connection is up";
                        Alert alert = new Alert(msg, (object sender, RoutedEventArgs e) => { (sender as Button).Tag = "Close"; return; }, "Ok");
                        showCenteredWindow(alert, true);
                    } else {
                        mainWindow.showBaloonInTrayIcon("Network error","Check if your Internet connection is up", typeOfMessage.ErrorMessage);
                        System.Threading.Thread.Sleep(3000);                        
                    }
                    userController.disableAutoSync();                    
                    mainWindow.Close();                    
                } catch (ServerException) {
                    if (mainWindow != null) {
                        if (mainWindow.ShowInTaskbar == true) {
                            String msg = "Server error\r\nThe server may be down or under maintenance";
                            Alert alert = new Alert(msg, (object sender, RoutedEventArgs e) => { (sender as Button).Tag = "Close"; return; }, "Ok");
                            showCenteredWindow(alert, true);
                        } else {
                            mainWindow.showBaloonInTrayIcon("Server error", "The server may be down or under maintenance", typeOfMessage.ErrorMessage);
                            System.Threading.Thread.Sleep(3000);
                            mainWindow.trayIcon.Dispose();
                        }
                    }
                    if(userController != null)
                        userController.logout();
                    Application.Current.Shutdown();   
                    break;
                } catch (IncontruentVersionException) {
                    Application.Current.Shutdown();
                    break;
                }
                catch (Exception ex) {
                    Console.Write("Strange exception occured: " + ex.Message);
                    Alert alert = new Alert("Undefined error.", (object sender, RoutedEventArgs e) => { (sender as Button).Tag = "Close"; }, "Ok");
                    showCenteredWindow(alert, true);
                    if(userController != null)
                        userController.logout();
                    Application.Current.Shutdown();
                    break;
                }
            }
        }

        private static void noDirInDeviceHandler(object sender, RoutedEventArgs e) {
            try {
                String msg = "Are you sure you want to choose an other directory?\r\nAll the stored data will be lost!";
                Alert alert = new Alert(msg, chooseNewMonitorDir, "Choose dir", backButton, "Back");
                showCenteredWindow(alert, true);
                if(monitorDir != null)
                    monitorDir += @"\" + getMonitorDirName();
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

        private static void backButton(object sender, RoutedEventArgs e) {
            (sender as Button).Tag = "Close";
        }

        private static void chooseMonitorDirPosition(object sender, RoutedEventArgs e) {
            try {
                monitorDir = dialog();
                if (monitorDir != null) {
                    if(getMonitorDirName() == monitorDir.Substring(monitorDir.LastIndexOf(@"\") + 1)){
                        (sender as Button).Tag = "Close";
                        userController.addMonitorDir(monitorDir);
                    } else {
                        monitorDir = null;
                        String msg = "Error\r\nThe chosen directory must have the same name of the monitored one";
                        Alert alert = new Alert(msg, (object s, RoutedEventArgs rea) => { (s as Button).Tag = "Close"; return; }, "Ok");
                        showCenteredWindow(alert, true);
                    }
                }
            } catch (ServerException) {
                if (monitorDir != null) {
                    (sender as Button).Tag = "Close";
                    String msg = "Warning: the path was not stored into the server\r\nIt will be asked during the next program reboot";
                    Alert alert = new Alert(msg, (object s, RoutedEventArgs rea) => { (sender as Button).Tag = "Close"; return; }, "Ok");
                    showCenteredWindow(alert, true);
                }
            } catch (NetworkException networkException) {
                throw networkException;
            } catch (BusyResourceException) {
                //writeOnConsole("Request denied.\r\nAnother operation is in progress, wait until it ends", typeOfMessage.ErrorMessage);
            }
        }

        private static String checkMonitorDir(List<String> possiblePaths) {
            foreach (String path in possiblePaths)
                Console.WriteLine("path: " + path);

            foreach (String path in possiblePaths)
                if (Directory.Exists(path))
                    return path;

            return null;
        }

        private static void alertRestoreVersion(object sender, RoutedEventArgs e) {
            monitorDir = dialog();
            if (monitorDir != null) {
                try {
                    //monitorDir = userController.restoreDir(monitorDir);
                    //userController.addMonitorDir(monitorDir);
                    monitorDir += @"\" + getMonitorDirName();
                    loadStatus = Loading.LoadFunction.RestoringDir;
                    (sender as Button).Tag = "Close";
                } catch (ServerException) {
                    (sender as Button).Tag = "Close";
                    String msg = "Warning: the path was not stored into the server\r\nIt will be asked during the next program reboot";
                    Alert alert = new Alert(msg, (object s, RoutedEventArgs rea) => { (sender as Button).Tag = "Close"; return; }, "Ok");
                    showCenteredWindow(alert, true);
                } catch (NetworkException networkException) {
                    throw networkException;
                }
            }
        }

        private static String dialog() {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            if ((folderBrowserDialog1.ShowDialog()) == System.Windows.Forms.DialogResult.OK) {
                return folderBrowserDialog1.SelectedPath;
            }
            return null;
        }

        private static void chooseNewMonitorDir(object sender, RoutedEventArgs e) {
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
                //writeOnConsole("Request denied.\r\nAnother operation is in progress, wait until it ends", typeOfMessage.ErrorMessage);
            }
        }

        private static String getMonitorDirName() {
            if (userController.getMonitorDir().Count == 0)
                return null;
            String path1 = userController.getMonitorDir()[0];
            int index = path1.LastIndexOf(@"\") +1;
            String name = path1.Substring(index);
            return name;
        }

        private static void showCenteredWindow(Window window, Boolean topmost) {
            try {
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                window.Topmost = topmost;
                window.ShowDialog();
            }catch (Exception) {
                throw;
            }
        }
    }
}
