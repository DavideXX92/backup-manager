using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace newServerWF
{
    interface FileDao
    {
        Dictionary<string, File> getAllFiles(User user);
        void addFile(File file, User user);
        void updateFile(File file, User user);
        void deleteFile(File file, User user);
    }
}
