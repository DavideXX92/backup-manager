using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PDSclient
{
    public class File : Node
    {
        public string name { get; set; }
        public string path { get; set; }
        public string absolutePath {get; set;}
        public string relativePath { get; set; }
        public Dir parentDir { get; set; }
        public int size { get; set; }
        public string hash { get; set; }
        public string extension { get; set; }
        public DateTime creationTime { get; set; }
        public DateTime lastWriteTime { get; set; }
        public String creationTimeToString { get { return creationTime.ToShortDateString() + " " + creationTime.ToShortTimeString(); } }
        public String lastWriteTimeToString { get { return lastWriteTime.ToShortDateString() + " " + lastWriteTime.ToShortTimeString(); } }

        public File()
        {
        }

        public File(string path, string monitorDirPath)
        {
            try
            {
                FileInfo fileinfo = new FileInfo(path);
                this.name = fileinfo.Name;
                this.path = getPathFromMonitorDir(path, monitorDirPath);
                this.absolutePath = path;
                this.parentDir = null;
                this.size = (int)fileinfo.Length;
                this.hash = getHash(path);
                this.extension = Path.GetExtension(path);
                this.creationTime = fileinfo.CreationTime;
                this.lastWriteTime = fileinfo.LastWriteTime;
            }catch(Exception e)
            {
                Console.WriteLine("File ignorato: " + e.Message);
            }
        }

        public File(string filename, Dir parentDir, string monitorDir)
        {
            try
            {
                FileInfo fileinfo = new FileInfo(filename);
                this.name = fileinfo.Name;
                this.path = parentDir.path + @"\" + this.name;
                this.absolutePath = path;
                this.relativePath = getPathFromMonitorDir(absolutePath, monitorDir);
                this.parentDir = parentDir;
                this.size = (int)fileinfo.Length;
                this.hash = getHash(filename);
                this.extension = Path.GetExtension(filename);
                this.creationTime = fileinfo.CreationTime;
                this.lastWriteTime = fileinfo.LastWriteTime;
            }
            catch (Exception e)
            {
                Console.WriteLine("File ignorato: " + e.Message);
            }
        }

        public File(string name, Dir parentDir, int size, string hash, string extension, DateTime creationTime, DateTime lastWriteTime)
        {
            this.name = name;
            this.path = parentDir.path + @"\" + name;
            this.parentDir = parentDir;
            this.size = size;
            this.hash = hash;
            this.extension = extension;
            this.creationTime = creationTime;
            this.lastWriteTime = lastWriteTime;
        }

        private string getHash(string filename)
        {
            HashAlgorithm sha1 = HashAlgorithm.Create();
            using (FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
                return BitConverter.ToString(sha1.ComputeHash(stream));
        }
        private string getPathFromMonitorDir(string path, string monitorDirPath)
        {
            //string monitorDirName = monitorDirPath.Substring(monitorDirPath.LastIndexOf(@"\"));
            string monitorDirName = monitorDirPath.Substring(monitorDirPath.LastIndexOf(@"\")).Substring(1);
            return monitorDirName + path.Substring(monitorDirPath.Length);
        }

        public String ImgSrc
        {
            //get { return @"images/Icons/Generic_File.ico"; }
            get { return getIcon(); }
            set { }
        }

        public String Lenght
        {
            get
            {
                return (this.size / 1024 + 1) + " KB";
            }
            set { }
        }

        public String getIcon() {
            switch (extension) {                       
                    case ".txt":
                    case ".text":
                    case ".rtf":
                    case ".stx":
                        return @"images/Icons/txt.ico";                        
                    case ".arc":
                    case ".ark":
                    case ".arj":
                    case ".b1":
                    case ".bar":
                    case ".tar":
                    case ".bep":
                    case ".bz2":
                    case ".cpt":
                    case ".gz":
                    case ".gz2":
                    case ".z":
                    case ".rar":
                    case ".sda":
                    case ".sea":
                    case ".sfx":
                    case ".tgz":
                    case ".zip":
                    case ".7z":
                        return @"images/Icons/zippedFile.ico";                        
                    case ".au":
                    case ".midi":
                    case ".mid":
                    case ".mp3":
                    case ".spx":
                    case ".wav":
                        return @"images/Icons/AudioFile.ico";                        
                    case ".avi":
                    case ".mpeg":
                    case ".mpg":
                    case ".mpeg2":
                    case ".mpeg3":
                    case ".mpeg4":
                    case ".mp2":
                    case ".mp4":
                    case ".m3u":
                    case ".m4u":
                    case ".flv":
                        return @"images/Icons/VideoFile.ico";
                    case ".bat":
                        return @"images/Icons/batfile.ico";                        
                    case ".blp":
                        return @"images/Icons/MixedMediaFile.ico";                        
                    case ".art":
                    case ".bmp":
                    case ".png":
                    case ".jpg":
                    case ".jpeg":
                    case ".gif":
                    case ".cgm":
                    case ".iff":
                    case ".ico":
                    case ".pbm":
                    case ".pgm":
                    case ".pic":
                    case ".pct":
                    case ".ppm":
                    case ".tiff":
                    case ".tif":
                    case ".vbm":
                    case ".xbm":
                    case ".xpm":
                        return @"images/Icons/Image_file.ico";                        
                    case ".bsh":
                    case ".com":
                    case ".csh":
                    case ".ksh":
                    case ".scpt":
                    case ".sh":
                        return @"images/Icons/Shell File.ico";                        
                    case ".c":
                    case ".cxx":
                        return @"images/Icons/c.png";                        
                    case ".cpp":
                        return @"images/Icons/cpp.png";                        
                    case ".css":
                        return @"images/Icons/css.png";                        
                    case ".doc":
                    case ".docx":
                        return @"images/Icons/doc.png";                        
                    case ".egg":
                    case ".py":
                    case ".pyc":
                    case ".pyo":
                    case ".pyd":
                        return @"images/Icons/py.png";                        
                    case ".exe":
                    case ".pkg":
                    case ".prg":
                        return @"images/Icons/Executable.ico";                        
                    case ".h":
                        return @"images/Icons/Iconsh.png";                        
                    case ".hpp":
                        return @"images/Icons/hpp.png";                        
                    case ".html":
                    case ".htm":
                    case ".shtml":
                    case ".shm":
                    case ".asp":
                        return @"images/Icons/html.png";                        
                    case ".jar":
                    case ".java":
                        return @"images/Icons/java.png";                        
                    case ".js":
                        return @"images/Icons/js.png";                        
                    case ".lib":
                    case ".dll":
                    case ".so":
                    case ".dylib":
                        return @"images/Icons/Library_File.ico";                        
                    case ".odf":
                        return @"images/Icons/odf.png";                        
                    case ".odp":
                    case ".otp":
                        return @"images/Icons/otp.png";                        
                    case ".ods":
                    case ".ots":
                        return @"images/Icons/ods.png";                        
                    case ".odt":
                    case ".ott":
                        return @"images/Icons/odt.png";
                    case ".pdf":
                        return @"images/Icons/pdf.png";                        
                    case ".":
                    case "File.php3":
                    case ".php4":
                        return @"images/Icons/php.png";                        
                    case ".ppt":
                    case ".pptm":
                    case ".pptx":
                        return @"images/Icons/ppt.png";
                    case ".ruby":
                        return @"images/Icons/rb.png";
                    case ".sql":
                        return @"images/Icons/sql.png";
                    case ".url":
                        return @"images/Icons/url.ico";
                    case ".xls":
                    case ".xlsm":
                    case ".xlsx":
                        return @"images/Icons/xls.png";
                    case ".xml":
                        return @"images/Icons/xml.png";
                    default:
                        return @"images/Icons/Generic_File.ico";
                }
            }        
    }
}
