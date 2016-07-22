using System;
using System.Collections.Generic;
using System.Drawing;
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

namespace WpfApplication1 {
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private MonitorDir md;

        public MainWindow() {
            string path = "";

            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Signin f1 = new Signin((string s) => { path = s; }, printConsole);
            f1.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            f1.Topmost = true;
            f1.ShowDialog();
            if (path == "")
                this.Close();

            try {
                md = new MonitorDir(path);
                paintMonitorElenco();
                paintMonitorAlbero(md.getAlbero().getRoot());
                //paintMonitorAlbero(md.getAlbero().getRoot(), md, 0);

                Watcher watcher = new Watcher(path, paintMonitorElenco, paintMonitorAlbero, printConsole, md);
            }catch(Exception e){
                printConsole(e.Message);
            }         
        }

        public void paintMonitorAlbero(node nodo) {
            //treeView.Items.Clear();
            /*
        WpfApplication1.nodo n = new WpfApplication1.nodo() { str= "root" };
        n.children.Add(new nodo() { str = "uno" });
        n.children.Add(new nodo() { str = "due" });
        WpfApplication1.nodo n2 = new WpfApplication1.nodo() { str = "tre" };
        n.children.Add(n2);
        n2.children.Add(new nodo() { str = "tre.uno" });
        treeView.Items.Add(n);*/
            treeView.Items.Add(md.getAlbero().getRoot());
        }

        /*
        public void paintMonitorAlbero(node nodo, monitorDir md, int lv) {
            if (nodo.getPath().percorso == md.getAlbero().getRoot().getPath().percorso) {
                lock(this)
                monitorAlbero.Items.Clear();
            }

            if (!nodo.isVisited) {
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
            Label label_tmp = new Label();
            StackPanel myStackPanel = new StackPanel();
            myStackPanel.Orientation = Orientation.Horizontal;
            myStackPanel.Margin = new System.Windows.Thickness(7 * lv, 0, 0, 0);
            Button bottone = new Button();
            label_tmp.Content = nodo.getPath().nome;
            System.Windows.Controls.Image img_tmp = new System.Windows.Controls.Image();
            img_tmp.Width = 20;
            img_tmp.Margin = new System.Windows.Thickness(5, 0, 0, 0);
            myStackPanel.Children.Add(bottone);
            try {
                FileInfo fileInfo = new FileInfo(@"../../images/folder.png");
                Uri url = new Uri(fileInfo.FullName);
                img_tmp.Source = new BitmapImage(url);
                myStackPanel.Children.Add(img_tmp);
            }catch(Exception e){
                //TODO
                //stampa da qualche parte errore
                printConsole(e.Message);
            }
                     
            myStackPanel.Children.Add(label_tmp);
            monitorAlbero.Items.Add(myStackPanel);
            if (nodo.getPath().percorso == md.curr_focus.getPath().percorso) {
                monitorAlbero.SelectedItem = myStackPanel;
                monitorAlbero.ScrollIntoView(myStackPanel);
                monitorAlbero.Focus();
            }
            if (!nodo.isExpanded) {
                System.Windows.Controls.Image img = new System.Windows.Controls.Image();
                img.Width = 10;
                try {
                    FileInfo fileInfo = new FileInfo(@"../../images/freccia_chiusa.png");
                    Uri url = new Uri(fileInfo.FullName);
                    img.Source = new BitmapImage(url);
                    bottone.Content = img;
                } catch (Exception e) {
                    //TODO
                    //stampa da qualche parte errore
                    printConsole(e.Message);
                }               
                bottone.Click += delegate { nodo.isExpanded = true; md.curr_focus = nodo; paintMonitorAlbero(md.getAlbero().getRoot(), md, 0); };
                label_tmp.MouseDown += delegate { md.getAlbero().current = nodo; paintMonitorElenco(md); };
            } else {
                System.Windows.Controls.Image img = new System.Windows.Controls.Image();
                img.Width = 10;
                try {
                    FileInfo fileInfo = new FileInfo(@"../../images/freccia_aperta.png");
                    Uri url = new Uri(fileInfo.FullName);
                    img.Source = new BitmapImage(url);
                    bottone.Content = img;
                } catch (Exception e) {
                    //TODO
                    //stampa da qualche parte errore
                    bottone.Width = 15;
                    printConsole(e.Message);
                }       
                bottone.Click += delegate { nodo.isExpanded = false; md.curr_focus = nodo; paintMonitorAlbero(md.getAlbero().getRoot(), md, 0); };
                label_tmp.MouseDown += delegate { md.getAlbero().current = nodo; paintMonitorElenco(md); };
                List<node> lista = nodo.getChildren();
                foreach (node n in lista) 
                    paintMonitorAlbero(n, md, lv + 1);
            }
        }*/

        public void paintMonitorElenco(){
            elencoDataBinding.ItemsSource = md.exploreCurrentLevel();
        }

        private void elencoMouseDoubleClick(object sender, MouseButtonEventArgs e){

            if (((ListView)sender).SelectedItem is item) {
                item originalSource = (item)((ListView)sender).SelectedItem;

                if (originalSource.getTipo() == 'd') {
                    md.getAlbero().current = originalSource.getNodo();
                    paintMonitorElenco();
                    //paintMonitorAlbero(md.getAlbero().getRoot(), md, 0);

                    TreeViewItem tvi = (TreeViewItem)(treeView.ItemContainerGenerator.ContainerFromItem(treeView.Items.GetItemAt(0)));
                    if (tvi != null)
                        JumpToNode(tvi, originalSource.Nome);                

                    //paintMonitorAlbero(md.getAlbero().getRoot());
                }
                else if (originalSource.getTipo() == 'f') {
                    System.Diagnostics.Process proc = new System.Diagnostics.Process();
                    proc.StartInfo.FileName = originalSource.getPercorso();
                    proc.StartInfo.UseShellExecute = true;
                    proc.Start();
                }
            }
        }
        

        private void JumpToNode(TreeViewItem tvi, string NodeName) {
            /*if (tvi != null) {
                node tmp = (node)(tvi.DataContext);
                Console.WriteLine("value: " + tmp.Path);
                if (tmp.Path == NodeName) {
                    Console.WriteLine("Trovato");
                    tvi.IsExpanded = true;
                    tvi.BringIntoView();
                    tvi.IsSelected = true;
                    return;
                }
                //else
                //vi.IsExpanded = false;

                if (tvi.HasItems) {
                    Console.WriteLine("Ha figli!");
                    
                    foreach (var item in tvi.Items) {
                        if (item is TreeViewItem) {
                            TreeViewItem temp = item as TreeViewItem;
                            JumpToNode(temp, NodeName);
                        }
                        else if(item is node) {
                            TreeViewItem temp = treeView.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                            JumpToNode(temp, NodeName);
                        }
                    }
                }
            }
            else
                Console.WriteLine("null");*/
            foreach(var tmp in treeView.ItemContainerGenerator.Items) {
                if (tmp is TreeViewItem) {
                    node node_tmp = (node)(tvi.DataContext);
                    Console.WriteLine("value: " + node_tmp.Path);
                    if (node_tmp.Path == NodeName) {
                        Console.WriteLine("Trovato");
                        tvi.IsExpanded = true;
                        tvi.BringIntoView();
                        tvi.IsSelected = true;
                        return;
                    }
                }
                else Console.WriteLine(tmp);
            }
        }

        private void espandiNodo(object sender, RoutedEventArgs e) {
            if (((TreeView)sender).SelectedItem is TreeViewItem) {
                node tmp = (node)((TreeView)sender).SelectedItem;
                if (!tmp.isVisited)
                    md.exploreUnvisitedNode(tmp);
            }
        }

        private void selectedDirectory(object sender, RoutedEventArgs e) {
            if (((TreeView)sender).SelectedItem is node) {
                node tmp = (node)((TreeView)sender).SelectedItem;

                md.getAlbero().current = tmp;
                Console.WriteLine("Current: " + md.getAlbero().current.Path);
                paintMonitorElenco();
            }
            else
                Console.WriteLine("Non è un nodo");
        }

        public void printConsole(string s) {
            consoleTmp.AppendText(s + "\n");
        }

        private void treeView_ToolTipOpening(object sender, ToolTipEventArgs e) {

        }
    }
}

/*
    MODIFICHE:
       1 - insieme di metodi per accedere a informazioni tipo "current" di dirtree nei vari livelli di "famiglia"
       2 - se apro una cartella nella scheda elenco devo espandere il corrispondente ramo della scheda albero
       3 - inserire l'esplorazione del file system all'apertura di rami nella scheda albero
       4 - probabilmente conviene inserire il tipo nodo nel listview e mettere l'informazione "item" al suo interno oppure
          inserire il campo nodo nell'item nel caso in cui questo sia una cartella, così facendo facilito il punto (2)
       5 - pulizia generale del codice
    */