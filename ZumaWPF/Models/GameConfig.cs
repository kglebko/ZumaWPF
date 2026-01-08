using System.Collections.Generic;
using System.Windows.Media;

namespace ZumaWPF.Models;

public class GameConfig
{
    public List<Color> BallColors { get; set; }
    public double DefaultBallSpeed { get; set; }
    public int PointsPerBall { get; set; }
    public int ComboMultiplier { get; set; }
    public double BallRadius { get; set; }
    public int MinComboSize { get; set; }
    
    public GameConfig()
    {
        // Более приятные цвета
        BallColors = new List<Color>
        {
            Color.FromRgb(231, 76, 60),  // Мягкий красный
            Color.FromRgb(52, 152, 219), // Мягкий синий
            Color.FromRgb(46, 204, 113), // Мягкий зеленый
            Color.FromRgb(241, 196, 15), // Мягкий желтый
            Color.FromRgb(155, 89, 182), // Мягкий фиолетовый
            Color.FromRgb(230, 126, 34)  // Мягкий оранжевый
        };
        DefaultBallSpeed = 50;
        PointsPerBall = 10;
        ComboMultiplier = 2;
        BallRadius = 20;
        MinComboSize = 3;
    }
}

