using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApplication1 {
    public class MonitorDir {
        private path rootPath;
        private DirTree albero;

        public MonitorDir(string p) {
            path.prefisso = p;
            rootPath = new path(p);
            albero = new DirTree(rootPath);
            exploreUnvisitedNode(albero.getRoot());
            albero.current = albero.getRoot();           //CONTROLLA
        }
        /*
        public string curr_path {
            get { return albero.current.getPath().percorso; }
            set { albero.setCurr(value); }   //CONTROLLA
        }
    }*/

        public node curr_focus {
            get { return albero.focus; }
            set { albero.focus = value; }
        }

        public List<item> exploreCurrentLevel() {
            List<item> list = new List<item>();

            if (!albero.current.isVisited)
                exploreUnvisitedNode(albero.current);

            System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(albero.current.getPath().percorso);
            if(!dirInfo.Exists)
                throw new Exception("Errore: la cartella " + albero.current.getPath().percorso + " non esiste");
            System.IO.DirectoryInfo[] dirInfos = dirInfo.GetDirectories("*.*");
            foreach (System.IO.DirectoryInfo d in dirInfos) {
                list.Add(new item(d.Name, d.FullName, d.CreationTime, d.LastWriteTime, albero.current.getChild(d.Name)));
            }


            System.IO.FileInfo[] fileInfos = dirInfo.GetFiles("*.*");
            foreach (System.IO.FileInfo d in fileInfos) {
                list.Add(new item(d.Name, d.FullName, d.CreationTime, d.LastWriteTime, d.Length));
            }

            return list;
        }

        public void exploreUnvisitedNode(node nodo) {
            System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(nodo.getPath().percorso);

            if (!dirInfo.Exists)
                throw new Exception("Errore: la cartella " + nodo.getPath().percorso + " non esiste");

            System.IO.DirectoryInfo[] dirInfos = dirInfo.GetDirectories("*.*");

            foreach (System.IO.DirectoryInfo d in dirInfos) {
                path tmp = new path(d.FullName);
                nodo.addChild(new node(tmp, nodo));
            }

            nodo.isVisited = true;
        }

        public Boolean isRoot() {
            if (rootPath.percorso == albero.current.getPath().percorso)
                return true;
            return false;
        }

        public DirTree getAlbero() {
            return albero;
        }

        public int removeItem(string path) {
            return albero.removeNode(path);
        }

        public void addDirectory(string path) {
            path p = new path(path);
            albero.addNode(p);
        }

        public void modifyName(string old, string nuovo) {
            albero.modifyName(old, nuovo);
        }
    }

    public class path {
        private static string _prefisso;//percorso per trovare la cartella da monitorare sulla macchina ospite
        private string _suffisso;//percorso che ha come radice la cartella da monitorare
        private string _nome;//nome della cartella o del file

        public path(string p) {
            percorso = p;
            int i;

            for (i = p.Length - 1; i >= 0; i--)
                if (p[i] == '\\')
                    break;

            _nome = p.Substring(i + 1, p.Length - i  - 1);
        }

        public string nome {
            get { return _nome; }
        }

        public static string prefisso {
            get { return _prefisso; }
            set { //implica la modifica della cartella di root
                    int i;

                    for (i = value.Length - 1; i >= 0; i--)
                        if (value[i] == '\\')
                            break;

                    _prefisso = value.Substring(0, i + 1);
            }
        }

        public string suffisso {
            get { return _suffisso; }
            set { _suffisso = value;
                int i;

                for (i = value.Length - 1; i > 0; i--){
                    if (value[i] == '\\')
                        break;
                }
                _nome = value.Substring((i + 1) , value.Length - i - 1);                               
            }
        }

        public string percorso {
            get { return _prefisso + _suffisso; }
            set { suffisso = value.Substring(prefisso.Length, value.Length - prefisso.Length); }
        }
    }

    public class item {
        char tipo;
        string percorso;
        long _lenght;
        node nodo;
        System.DateTime creationTime;
        System.DateTime lastModify;

        public string Nome {
            get;
            set;
        }

        public String Lenght {
            get {
                if (tipo != 'd')
                    return (_lenght / 1024 + 1) + " KB";
                else
                    return "";
            }
            set {}
        }

        public string ImgSrc {
            get {
                switch (tipo) {
                    case 'd':
                        return (new System.IO.FileInfo(@"../../images/Icons/Folder_Opened.ico")).FullName;
                    case 'f':
                        return (new System.IO.FileInfo(@"../../images/Icons/Generic_File.ico")).FullName;
                    default:
                        return "";
                }
            }
            set { ImgSrc = value; }
        }

        public item(string n, string p, System.DateTime ct, System.DateTime lm, long l) {
            tipo = 'f';
            Nome = n;
            percorso = p;
            creationTime = ct;
            lastModify = lm;
            _lenght = l;
            nodo = null;
        }

        public item(string n, string p, System.DateTime ct, System.DateTime lm, node nodo) {
            tipo = 'd';
            Nome = n;
            percorso = p;
            creationTime = ct;
            lastModify = lm;
            this.nodo = nodo;
        }

        public char getTipo() {
            return tipo;
        }

        public node getNodo() {
            return nodo;
        }

        public string getPercorso() {
            return percorso;
        }

        public System.DateTime getCreationTime() {
            return creationTime;
        }

        public System.DateTime getlastModify() {
            return lastModify;
        }

        public long getSize() {
            if (tipo == 'd')
                return -1;
            return _lenght;
        }
    }
}
