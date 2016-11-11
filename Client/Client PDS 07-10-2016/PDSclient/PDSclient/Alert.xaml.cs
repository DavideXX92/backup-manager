using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PDSclient {
    /// <summary>
    /// Interaction logic for Alert.xaml
    /// </summary>
    public partial class Alert : Window {
        public Alert() {
            InitializeComponent();
        }

        public Alert(String msg, RoutedEventHandler buttonConfirmHandler, String buttonConfirmName) {
            InitializeComponent();
            setMessage(msg);
            setButton(buttonConfirmName, buttonConfirmHandler, alertConfirm);
            alertReset.Visibility = Visibility.Collapsed;
            alertThingy.Visibility = Visibility.Collapsed;
        }

        public Alert(String msg, RoutedEventHandler buttonConfirmHandler, String buttonConfirmName, RoutedEventHandler buttonResetHandler, String buttonResetName) {            
            InitializeComponent();
            setMessage(msg);
            setButton(buttonConfirmName, buttonConfirmHandler, alertConfirm);
            setButton(buttonResetName, buttonResetHandler, alertReset);
            alertThingy.Visibility = Visibility.Collapsed;
        }

        public Alert(String msg, RoutedEventHandler buttonConfirmHandler, String buttonConfirmName, RoutedEventHandler buttonThingyHandler, String buttonThingyName, RoutedEventHandler buttonResetHandler, String buttonResetName) {
            InitializeComponent();
            setMessage(msg);
            setButton(buttonConfirmName, buttonConfirmHandler, alertConfirm);
            setButton(buttonResetName, buttonResetHandler, alertReset);
            setButton(buttonThingyName, buttonThingyHandler, alertThingy);
        }

        private void setButton(string buttonConfirmName, RoutedEventHandler buttonConfirmHandler, Button button) {
            button.Tag = "Open";
            button.Click += buttonConfirmHandler;
            button.Click += (object sender, RoutedEventArgs e) => { isWindowToClose(sender as Button); };
            button.Content = buttonConfirmName;
        }

        private void isWindowToClose(Button button){
            if (button.Tag == "Close")
                Close();
        }

        private void setMessage(String msg) {
            messageAlert.Content = msg;
        }

        private void windowLeftButtonDownHandler(object sender, MouseButtonEventArgs e) {
            DragMove();
        }

        private void buttonReduceHandler(object sender, RoutedEventArgs e) {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void buttonCloseHandler(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
