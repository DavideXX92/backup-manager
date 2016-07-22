using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newServerWF
{
    class FileSystem
    {
        private Form1 form;
        
        public FileSystem(Form1 form){
            this.form = form;
        }

        // Process all files in the directory passed in, recurse on any directories 
        // that are found, and process the files they contain.
        public int processDirectory(string targetDirectory)
        {
            if (Directory.Exists(targetDirectory))
            {
                writeOnConsole("directory exist");
                // Process the list of files found in the directory.
                string[] fileEntries = Directory.GetFiles(targetDirectory);
                foreach (string fileName in fileEntries)
                    processFile(fileName);

                // Recurse into subdirectories of this directory.
                string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
                foreach (string subdirectory in subdirectoryEntries)
                    processDirectory(subdirectory);
                return 1;
            }
            else
            {
                writeOnConsole("directory: " + targetDirectory + " not exist");
                return 0;
            }

        }

        // Insert logic for processing found files here.
        private void processFile(string path)
        {
            writeOnConsole("Processed file: " + path);
        }

        private void writeOnConsole(string str)
        {
            //form.consoleWrite = str;
            MyConsole.write(str);
        }
    }

    
}
