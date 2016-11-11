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
    /// Logica di interazione per Signin.xaml
    /// </summary>
    /// 

    public partial class Signin : Window {
        public delegate void setUserControllerDel(UserController userController);
        public delegate void setUserDel(User user);
        private setUserControllerDel setUserController;
        private setUserDel setUser;
        private UserController userController;
        private bool imConnected;
        
        public Signin(setUserControllerDel userController, setUserDel user)
        {
            this.setUserController = userController;
            this.setUser = user;
            this.imConnected = false;
            this.userController = null;
            InitializeComponent();

            reinserisciPasswordReg.PasswordChanged += checkRetypePwd;
            passwordReg.PasswordChanged += checkRetypePwd;
        }

        private void _switchTab(object sender, RoutedEventArgs e) {
            if (sender == switchToRegistra)
                tabLogin_Reg.SelectedIndex = 1;
            else
                tabLogin_Reg.SelectedIndex = 0;
        }

        public void _login(object sender, RoutedEventArgs e) {
            if (!imConnected)
            {
                if (usernameLog.Text.Length == 0 || passwordLog.Password.Length == 0) {
                    errorMessageLog.Content = "You have to fill all the fields";
                    return;
                }

                try
                {
                    //Instanzio lo userController che automaticamente si connette al server
                    userController = new UserControllerImpl();
                    setUserController(userController);
                    imConnected = true;
                }
                catch (Exception ex)
                {
                    string error = "Server not reachable";
                    errorMessageLog.Content = error;
                    Console.WriteLine(error + "\nException: " + ex.Message);
                }
            }
            try{
                User user = userController.login(usernameLog.Text, passwordLog.Password);
                setUser(user);
                Console.WriteLine("Login riuscito");
                Close();                
            }
            catch (ServerException ex){
                errorMessageLog.Content = "Login fallito: " + ex.Message;
            }
            catch(NetworkException ex){                                                    
                string error = "Problema di rete: impossibile eseguire il login";
                errorMessageLog.Content = error;
                Console.WriteLine(error + "\nException: " + ex.Message);                
            } catch (Exception ex) {                
                string error = "Network error: registration failed";
                errorMessageReg.Content = error;
                Console.WriteLine(error + "\nException: " + ex.Message);
            }
        }

        private void _registra(object sender, RoutedEventArgs e)
        {
            if (!imConnected)
            {
                if (usernameReg.Text.Length == 0 || passwordReg.Password.Length == 0 || reinserisciPasswordReg.Password.Length == 0) {
                    errorMessageReg.Content = "You have to fill all the fields";
                    return;
                }
                if (passwordReg.Password != reinserisciPasswordReg.Password) {
                    errorMessageReg.Content = "Passwords don't match";
                    return;
                }

                try
                {
                    userController = new UserControllerImpl();
                    setUserController(userController);
                    imConnected = true;
                }
                catch (Exception ex)
                {
                    string error = "Server connection failed";
                    errorMessageReg.Content = error;
                    Console.WriteLine(error + "\nException: " + ex.Message);
                }
            }
            try
                {
                    userController.register(usernameReg.Text, passwordReg.Password);
                    Console.WriteLine("Registrazione avvenuta con successo");
                    tabLogin_Reg.SelectedIndex = 0;
                    errorMessageLog.Content = "Registration successful";
                }
                catch (ServerException ex){
                    errorMessageReg.Content = "Registration failed: " + ex.Message;
            } catch (NetworkException ex) {
                string error = "Network error: registration failed";
                errorMessageReg.Content = error;
                Console.WriteLine(error + "\nException: " + ex.Message);
            } catch (Exception ex) {
                string error = "Network error: registration failed";
                errorMessageReg.Content = error;
                Console.WriteLine(error + "\nException: " + ex.Message);
            }
        }

        private void OnKeyDownHandlerLogin(object sender, KeyEventArgs e) {
            if (e.Key == Key.Return)
                _login(logIn, new RoutedEventArgs());
        }

        private void OnKeyDownHandlerReg(object sender, KeyEventArgs e) {
            if (e.Key == Key.Return)
                _registra(Registra, new RoutedEventArgs());
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

        /*
         * Handler that manage the pwd comparison in the registration form
         */
        private void checkRetypePwd(object sender, RoutedEventArgs e) {
            if(!(sender is PasswordBox))
                return;
             PasswordBox passwordBox1 = sender as PasswordBox;           

            PasswordBox passwordBox2;
            if(passwordBox1.Name == "passwordReg"){
                passwordBox2 = reinserisciPasswordReg;
            }
            else if(passwordBox1.Name == "reinserisciPasswordReg"){
                passwordBox2 = passwordReg;
            }else
                return;
            if (passwordBox2 == null)
                return;

            if (passwordBox1.SecurePassword.Length == 0 || passwordBox2.SecurePassword.Length == 0) {
                passwordBox1.BorderBrush = Brushes.White;
                passwordBox2.BorderBrush = Brushes.White;
                passwordBox1.BorderThickness = new Thickness(1);
                passwordBox2.BorderThickness = new Thickness(1);                
            } else if (passwordBox1.Password == passwordBox2.Password) {
                passwordBox1.BorderBrush = Brushes.Green;
                passwordBox2.BorderBrush = Brushes.Green;                
                passwordBox1.BorderThickness = new Thickness(2);
                passwordBox2.BorderThickness = new Thickness(2);
            } else {
                passwordBox1.BorderBrush = Brushes.Red;
                passwordBox2.BorderBrush = Brushes.Red;
                passwordBox1.BorderThickness = new Thickness(2);
                passwordBox2.BorderThickness = new Thickness(2);
            }                      
        }
    }
}
