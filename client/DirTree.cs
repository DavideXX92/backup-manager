using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApplication1 {
    public class DirTree {
        node root;
        node _current;
        node _focus;

        public node current {
            get { return _current; }
            set { _current = value; }
        }

        public node focus {
            get { return _focus; }
            set { _focus = value; }
        }

        public DirTree(path path) {
            root = new node(path, null);
            current = root;
            focus = root;
        }

        public node getRoot() {
            return root;
        }

        public void addNode(path path) {
            int pos;
            string tmp;

            for (pos = path.percorso.Length - 1; path.percorso[pos] != '\\'; pos--) ;
            tmp = path.percorso.Substring(0, pos);
            node padre = getNode(new path(tmp), root);
            padre.addChild(new node(path, padre));
        }

        //removeNode ritorna: 1 se il percorso riguarda una cartella, 0 altrimenti
        public int removeNode(string p) {
            path path = new path(p);
            node tmp = getNode(path, root);
            if (tmp != null) {
                tmp.father.children.Remove(tmp);
                if (current == tmp) {
                    current = tmp.father;
                    focus = tmp.father;
                }
                tmp = null;
                return 1;
            }
            return 0;
        }

        public void modifyName(string old, string nuovo) {
            node tmp = getNode(new path(old), root);
            if (tmp != null) {
                tmp.getPath().percorso = nuovo;
            }
        }

        private node getNode(path path, node n) {
            node tmp;

            tmp = n;
            if (n.getPath().percorso == path.percorso)
                return n;
            System.Collections.ObjectModel.ObservableCollection<node> lista = tmp.children;
            foreach (node i in lista) {
                tmp = getNode(path, i);
                if (tmp != null)
                    return tmp;
            }

            return null;
        }

        //NOTA setCurr setta anche la cartella che sarà selezionata nella visualizzazione ad albero!!
        /*public void setCurr(string cartella) {
            if (cartella == ".")
                current = root;
            else if (cartella == "..")
                current = current.father;
            else {
                System.Collections.ObjectModel.ObservableCollection<node> lista = current.getChildren();
                foreach(node n in lista)
                    if (n.getPath().percorso == cartella) {
                        current = n;
                        break;
                    }
            }
            focus = current;
            node tmp = current;
            while (tmp != root) {
                tmp.isExpanded = true;
                tmp = tmp.father;
            }
            tmp.isExpanded = true;
        }*/
    }

    public class node {
        private path nodePath;
        public System.Collections.ObjectModel.ObservableCollection<node> children { get; set; }
        private node _father;
        Boolean _isExpanded;
        Boolean _isVisited;

        public String ImgSrc {
            get { return (new System.IO.FileInfo(@"../../images/Icons/Folder_Opened.ico")).FullName; }
            set { }
        }

        public string Path {
            get{ return getPath().nome; }
            set{ }
        }

        public node father {
            get { return _father; }
            set { _father = value; }
        }

        public Boolean isExpanded {
            set { _isExpanded = value; }
            get { return _isExpanded; }
        }

        public Boolean isVisited {
            set { _isVisited = value; }
            get { return _isVisited; }
        } 

        public node(path p, node f) {
            nodePath = p;
            father = f;
            children = new System.Collections.ObjectModel.ObservableCollection<node>();
            isExpanded = false;
            isVisited = false;
        }

        public path getPath() {
            return nodePath;
        }

        public void addChild(node child) {
            children.Add(child);
        }

        public node getChild(String name) {
            foreach (node tmp in children)
                if (tmp.getPath().nome == name)
                    return tmp;
            return null;
        }
    }
}
