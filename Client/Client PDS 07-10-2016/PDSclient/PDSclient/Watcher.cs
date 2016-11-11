using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDSclient
{
    class Watcher
    {
        private string monitorDir { get; set; }
        public MyBuffer bufferRead { get; set; }
        public MyBuffer bufferWrite { get; set; }

        public Watcher(string monitorDir)
        {
            this.monitorDir = monitorDir;
            this.bufferRead = new MyBuffer();
            this.bufferWrite = new MyBuffer();
        }

        public void set()
        {
            // Create a new FileSystemWatcher and set its properties.
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = monitorDir;
            /* Watch for changes in LastAccess and LastWrite times, and
               the renaming of files or directories. */
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
               | NotifyFilters.FileName | NotifyFilters.DirectoryName;

            watcher.IncludeSubdirectories = true;

            // Add event handlers.
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.Deleted += new FileSystemEventHandler(OnChanged);
            watcher.Renamed += new RenamedEventHandler(OnRenamed);

            // Begin watching.
            watcher.EnableRaisingEvents = true;
        }

        // Define the event handlers.
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            FileSystemService fsService = new FileSystemServiceImpl();
            if (fsService.isAfile(e.FullPath))
                handleFileChange(e.ChangeType, e.FullPath, null);
            else if (fsService.isAdir(e.FullPath))
                handleDirChange(e.ChangeType, e.FullPath, null);
        }
        private void OnRenamed(object source, RenamedEventArgs e)
        {
            FileSystemService fsService = new FileSystemServiceImpl();
            if (fsService.isAfile(e.FullPath))
                handleFileChange(e.ChangeType, e.FullPath, e.OldFullPath);
            else if (fsService.isAdir(e.FullPath))
                handleDirChange(e.ChangeType, e.FullPath, e.OldFullPath);
        }

        private void handleFileChange(WatcherChangeTypes change, string path, string oldPath)
        {
            if (oldPath == null)
                Console.WriteLine("File: " + path + " " + change);
            else
                Console.WriteLine("File: {0} renamed to {1}", oldPath, path);

            FileSystemService fsService = new FileSystemServiceImpl();
            Operation operation = new Operation();
            File file = new File();

            switch (change)
            {
                case WatcherChangeTypes.Created:
                    operation.type = "addFile";
                    operation.file = new File(path, monitorDir);
                    bufferRead.addOperation(operation);
                    break;

                case WatcherChangeTypes.Deleted:
                    file = bufferRead.containsThisFile(fsService.getPathFromMonitorDir(path, monitorDir));
                    if (file != null)
                        bufferRead.removeThisFile(file);
                    else
                    {
                        operation.type = "deleteFile";
                        operation.path = fsService.getPathFromMonitorDir(path, monitorDir);
                        bufferRead.addOperation(operation);
                    }
                    break;

                case WatcherChangeTypes.Changed:
                    file = bufferRead.containsThisFile(fsService.getPathFromMonitorDir(path, monitorDir));
                    if (file != null)
                    {
                        File newfile = new File(path, monitorDir);
                        //inserire il lock
                        file.hash = newfile.hash;
                        file.size = newfile.size;
                        file.lastWriteTime = newfile.lastWriteTime;
                    }
                    else
                    {
                        operation.type = "updateFile";
                        operation.file = new File(path, monitorDir);
                        bufferRead.addOperation(operation);
                    }
                    break;

                case WatcherChangeTypes.Renamed:
                    file = bufferRead.containsThisFile(fsService.getPathFromMonitorDir(oldPath, monitorDir));
                    if (file != null)
                    {
                        File newfile = new File(path, monitorDir);
                        file.name = newfile.name;
                        file.path = newfile.path;
                        file.extension = newfile.extension;
                    }
                    else
                    {
                        operation.type = "renameFile";
                        operation.oldPath = fsService.getPathFromMonitorDir(oldPath, monitorDir);
                        operation.newPath = fsService.getPathFromMonitorDir(path, monitorDir);
                        bufferRead.addOperation(operation);
                    }
                    break;
            }
        }
        private void handleDirChange(WatcherChangeTypes change, string path, string oldPath)
        {
            if (oldPath == null)
                Console.WriteLine("Directory: " + path + " " + change);
            else
                Console.WriteLine("Directory: {0} renamed to {1}", oldPath, path);

            FileSystemService fsService = new FileSystemServiceImpl();
            Operation operation = new Operation();
            File file = new File();
            Dir dir;

            switch (change)
            {
                case WatcherChangeTypes.Created:
                    operation.type = "addDir";
                    dir = new Dir(fsService.getPathFromMonitorDir(path, monitorDir));
                    dir.setCreationTime(path);
                    dir.setLastWriteTime(path);
                    operation.dir = dir;
                    bufferRead.addOperation(operation);
                    exploreDir(path);
                    break;

                case WatcherChangeTypes.Deleted:
                    operation = bufferRead.containsThisDir(fsService.getPathFromMonitorDir(path, monitorDir));
                    if (operation != null)
                        bufferRead.removeThisDir(path);
                    else
                    {
                        operation = new Operation();
                        operation.type = "deleteDir";
                        operation.path = fsService.getPathFromMonitorDir(path, monitorDir);
                        bufferRead.addOperation(operation);
                    }
                    break;

                case WatcherChangeTypes.Changed:
                    operation = bufferRead.containsThisDir(fsService.getPathFromMonitorDir(path, monitorDir));
                    if (operation != null)
                        operation.dir.lastWriteTime = Directory.GetLastWriteTime(path);
                    else
                    {
                        operation = new Operation();
                        operation.type = "updateDir";
                        dir = new Dir(fsService.getPathFromMonitorDir(path, monitorDir));
                        dir.setLastWriteTime(path);
                        operation.dir = dir;
                        bufferRead.addOperation(operation);
                    }
                    break;

                case WatcherChangeTypes.Renamed:
                    operation = bufferRead.containsThisDir(fsService.getPathFromMonitorDir(oldPath, monitorDir));
                    if (operation != null)
                    {
                        operation.path = path;
                    }
                    else
                    {
                        operation = new Operation();
                        operation.type = "renameDir";
                        operation.oldPath = fsService.getPathFromMonitorDir(oldPath, monitorDir);
                        operation.newPath = fsService.getPathFromMonitorDir(path, monitorDir);
                        bufferRead.addOperation(operation);
                    }
                    break;
            }
        }

        private void exploreDir(string path)
        {
            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(path);
            foreach (string fileName in fileEntries)
                handleFileChange(WatcherChangeTypes.Created, fileName, null);

            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = Directory.GetDirectories(path);
            foreach (string subdirectory in subdirectoryEntries)
                handleDirChange(WatcherChangeTypes.Created, subdirectory, null);
        }
    }
}
