using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WpfApplication1 {
    //delegate void paintElenco(monitorDir md);
    //delegate void paintAlbero(node nodo, monitorDir md, int lv);
    delegate void paintElenco();
    delegate void paintAlbero(node nodo);
    delegate void printConsole(string s);

    class Watcher {
        string path;
        paintElenco elenco;
        paintAlbero albero;
        printConsole console;
        MonitorDir md;

        public Watcher(string pathDirectory, paintElenco pe, paintAlbero pa, printConsole pc, MonitorDir md) {
            path = pathDirectory;
            elenco = pe;
            albero = pa;
            console = pc;
            this.md = md;

            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = path;
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.Filter = "*.*";
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Renamed += new RenamedEventHandler(OnRenamed);
            watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.Deleted += new FileSystemEventHandler(OnChanged);
            watcher.Error += new ErrorEventHandler(OnError);
            watcher.EnableRaisingEvents = true;
            watcher.IncludeSubdirectories = true;
        }

        private void OnChanged(object source, FileSystemEventArgs e) {
            try {
                if (e.ChangeType == WatcherChangeTypes.Changed) {
                    Application.Current.Dispatcher.Invoke(console, "Changed: " + e.FullPath + " " + e.ChangeType);
                } else if (e.ChangeType == WatcherChangeTypes.Created) {
                    Application.Current.Dispatcher.Invoke(console, "Created: " + e.FullPath + " " + e.ChangeType);

                    FileAttributes attr = File.GetAttributes(e.FullPath);

                    //detect whether its a directory or file
                    if ((attr & FileAttributes.Directory) == FileAttributes.Directory) {
                        //TODO sincronizzazione server
                        md.addDirectory(e.FullPath);
                        updateMonitor();
                    } else {
                        //TODO sincronizzazione col server
                        updateMonitor();
                    }
                } else if (e.ChangeType == WatcherChangeTypes.Deleted) {
                    Application.Current.Dispatcher.Invoke(console, "Deleted: " + e.FullPath + " " + e.ChangeType);
                    //TODO sincronizzazione col server
                    md.removeItem(e.FullPath);
                    updateMonitor();                     
                }
            } catch (Exception ecc) {
                throw ecc;
            }
        }

        private void OnRenamed(object source, RenamedEventArgs e) {
            try {
                Application.Current.Dispatcher.Invoke(console, "Renamed: " + e.OldName + " to " + e.Name);
                FileAttributes attr = File.GetAttributes(e.FullPath);

                //detect whether its a directory or file
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory) {
                    //TODO sincronizzazione server
                    md.modifyName(e.OldFullPath, e.FullPath);
                    updateMonitor();
                } else {
                    //TODO sincronizzazione col server
                    updateMonitor();
                }
            } catch (Exception ecc) {
                throw ecc;
            }
        }

        private void OnError(object source, ErrorEventArgs e) {
            try {
                Application.Current.Dispatcher.Invoke(console, "The FileSystemWatcher has detected an error");
                if (e.GetException().GetType() == typeof(InternalBufferOverflowException)) {
                    Application.Current.Dispatcher.Invoke(console, "The file system watcher experienced an internal buffer overflow: " + e.GetException().Message);
                }
            } catch (Exception ecc) {
                throw ecc;
            }
        }

        private void updateMonitor(){
            //TODO lock + ottimizzazione (richiamare albero e elenco SOLO se necessario)
            Application.Current.Dispatcher.Invoke(albero, md.getAlbero().getRoot(), md, 0);
            Application.Current.Dispatcher.Invoke(elenco, md);
        }
    }
}
