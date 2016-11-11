using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PDSclient {
    public class TrayIconCommand:ICommand {
        public void Execute(object parameter) {
            Window mainWindow = parameter as Window;
            mainWindow.ShowInTaskbar = true;
            mainWindow.WindowState = WindowState.Normal;
        }

        public bool CanExecute(object parameter) {
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }     
}
