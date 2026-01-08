using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;

namespace ZumaWPF.Views;

public partial class LoginView : UserControl
{
    public event System.Action<string>? Login;
    
    public LoginView()
    {
        InitializeComponent();
        Loaded += (s, e) => UsernameTextBox.Focus();
    }
    
    private void Login_Click(object sender, RoutedEventArgs e)
    {
        AttemptLogin();
    }
    
    private void UsernameTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            AttemptLogin();
        }
    }
    
    private void AttemptLogin()
    {
        var username = UsernameTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(username))
        {
            ErrorText.Text = "Имя не может быть пустым";
            ErrorText.Visibility = Visibility.Visible;
            return;
        }
        
        ErrorText.Visibility = Visibility.Collapsed;
        Login?.Invoke(username);
    }
}

