using System.Collections.Generic;
using System.Windows.Media;

namespace ZumaWPF.Models;

public class GameState
{
    public int Score { get; set; }
    public int CurrentLevel { get; set; }
    public List<Ball> Chain { get; set; }
    public List<Ball> AllBalls { get; set; } // Все шарики для уровня
    public int NextBallIndex { get; set; } // Индекс следующего шарика для добавления
    public Shooter Shooter { get; set; }
    public Level? ActiveLevel { get; set; }
    public bool IsPaused { get; set; }
    public bool IsGameOver { get; set; }
    public bool IsVictory { get; set; }
    public double ChainProgress { get; set; }
    
    public GameState()
    {
        Score = 0;
        CurrentLevel = 1;
        Chain = new List<Ball>();
        AllBalls = new List<Ball>();
        NextBallIndex = 0;
        Shooter = new Shooter(new System.Windows.Point(400, 300));
        IsPaused = false;
        IsGameOver = false;
        IsVictory = false;
        ChainProgress = 0;
    }
}

