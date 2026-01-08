using System.Windows.Controls;
using System.Windows;

namespace ZumaWPF.Views;

public partial class GameOverView : UserControl
{
    public event System.Action? Retry;
    public event System.Action? MainMenu;
    
    public GameOverView()
    {
        InitializeComponent();
    }
    
    public void SetScore(int score)
    {
        ScoreText.Text = $"Очки: {score}";
    }
    
    private void Retry_Click(object sender, RoutedEventArgs e)
    {
        Retry?.Invoke();
    }
    
    private void MainMenu_Click(object sender, RoutedEventArgs e)
    {
        MainMenu?.Invoke();
    }
}

