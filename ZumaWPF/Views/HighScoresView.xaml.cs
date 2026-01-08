using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using ZumaWPF.Models;

namespace ZumaWPF.Views;

public partial class HighScoresView : UserControl
{
    public event System.Action? Back;
    
    public HighScoresView()
    {
        InitializeComponent();
    }
    
    public void LoadScores(List<HighScore> scores)
    {
        var indexedScores = scores.Select((score, index) => new
        {
            Index = index + 1,
            score.Username,
            score.Score,
            score.Level,
            score.Date
        }).ToList();
        
        ScoresList.ItemsSource = indexedScores;
    }
    
    private void Back_Click(object sender, RoutedEventArgs e)
    {
        Back?.Invoke();
    }
}

