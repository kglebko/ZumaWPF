using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace ZumaWPF.Models;

public class Level
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string BackgroundPath { get; set; }
    public GamePath Path { get; set; }
    public double BallSpeed { get; set; }
    public int InitialBallCount { get; set; }
    
    public Level(int id, string name, GamePath path, double ballSpeed, string backgroundPath, int initialBallCount = 30)
    {
        Id = id;
        Name = name;
        Path = path;
        BallSpeed = ballSpeed;
        BackgroundPath = backgroundPath;
        InitialBallCount = initialBallCount;
    }
}

