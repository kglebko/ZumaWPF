using System.Windows.Controls;
using System.Windows;
using ZumaWPF.Models;

namespace ZumaWPF.Views;

public partial class LevelSelectionView : UserControl
{
    public event System.Action<Level>? LevelSelected;
    public event System.Action? Back;
    
    public LevelSelectionView()
    {
        InitializeComponent();
    }
    
    public void LoadLevels(System.Collections.Generic.List<Level> levels)
    {
        LevelsContainer.ItemsSource = levels;
    }
    
    private void LevelButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Level level)
        {
            LevelSelected?.Invoke(level);
        }
    }
    
    private void Back_Click(object sender, RoutedEventArgs e)
    {
        Back?.Invoke();
    }
}

