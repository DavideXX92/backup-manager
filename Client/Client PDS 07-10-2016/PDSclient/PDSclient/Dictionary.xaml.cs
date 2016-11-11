using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PDSclient {
    public partial class Dictionary : ResourceDictionary{
        public void passwordChangedHandler(object sender, RoutedEventArgs e) {
            if (!(sender is PasswordBox))
                return;
            PasswordBox passwordBox = sender as PasswordBox;

            object controlTemplateObj = passwordBox.Template;
            if (controlTemplateObj == null || !(controlTemplateObj is ControlTemplate))
                return;
            ControlTemplate controlTemplate = controlTemplateObj as ControlTemplate;

            if (passwordBox.SecurePassword.Length == 0) {
                (controlTemplate.FindName("pwdImage", passwordBox) as Image).Visibility = Visibility.Visible;
                (controlTemplate.FindName("pwdPlaceholder", passwordBox) as TextBox).Foreground = Brushes.LightGray;
                passwordBox.BorderBrush = Brushes.White;
            } else {
                (controlTemplate.FindName("pwdImage", passwordBox) as Image).Visibility = Visibility.Hidden;
                (controlTemplate.FindName("pwdPlaceholder", passwordBox) as TextBox).Foreground = Brushes.Transparent;
                if(passwordBox.BorderBrush == Brushes.White)
                    passwordBox.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4b8af1"));
            }
        }
    }
}