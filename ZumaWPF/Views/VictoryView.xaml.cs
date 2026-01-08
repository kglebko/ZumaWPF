using System.Windows.Controls;
using System.Windows;

namespace ZumaWPF.Views;

public partial class VictoryView : UserControl
{
    public event System.Action? NextLevel;
    public event System.Action? MainMenu;
    
    public VictoryView()
    {
        InitializeComponent();
    }
    
    public void SetScore(int score)
    {
        ScoreText.Text = $"Очки: {score}";
    }
    
    private void NextLevel_Click(object sender, RoutedEventArgs e)
    {
        NextLevel?.Invoke();
    }
    
    private void MainMenu_Click(object sender, RoutedEventArgs e)
    {
        MainMenu?.Invoke();
    }
}

