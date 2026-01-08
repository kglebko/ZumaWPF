using System.Windows.Controls;
using System.Windows;

namespace ZumaWPF.Views;

public partial class MainMenuView : UserControl
{
    public event System.Action? NewGame;
    public event System.Action? Continue;
    public event System.Action? SelectLevel;
    public event System.Action? HighScores;
    public event System.Action? Manual;
    public event System.Action? Exit;
    
    public MainMenuView()
    {
        InitializeComponent();
    }
    
    public void UpdateContinueButton(bool hasSave)
    {
        ContinueButton.IsEnabled = hasSave;
        ContinueButton.Opacity = hasSave ? 1.0 : 0.5;
    }
    
    private void NewGame_Click(object sender, RoutedEventArgs e)
    {
        NewGame?.Invoke();
    }
    
    private void Continue_Click(object sender, RoutedEventArgs e)
    {
        Continue?.Invoke();
    }
    
    private void SelectLevel_Click(object sender, RoutedEventArgs e)
    {
        SelectLevel?.Invoke();
    }
    
    private void HighScores_Click(object sender, RoutedEventArgs e)
    {
        HighScores?.Invoke();
    }
    
    private void Manual_Click(object sender, RoutedEventArgs e)
    {
        Manual?.Invoke();
    }
    
    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Exit?.Invoke();
    }
}

