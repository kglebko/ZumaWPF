using System.Windows.Controls;
using System.Windows;

namespace ZumaWPF.Views;

public partial class PauseView : UserControl
{
    public event System.Action? Resume;
    public event System.Action? Save;
    public event System.Action? MainMenu;
    
    public PauseView()
    {
        InitializeComponent();
    }
    
    private void Resume_Click(object sender, RoutedEventArgs e)
    {
        Resume?.Invoke();
    }
    
    private void Save_Click(object sender, RoutedEventArgs e)
    {
        Save?.Invoke();
    }
    
    private void MainMenu_Click(object sender, RoutedEventArgs e)
    {
        MainMenu?.Invoke();
    }
}

