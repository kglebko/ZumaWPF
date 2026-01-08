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
        BallColors = new List<Color>
        {
            Color.FromRgb(231, 76, 60),
            Color.FromRgb(52, 152, 219),
            Color.FromRgb(46, 204, 113),
            Color.FromRgb(241, 196, 15),
            Color.FromRgb(155, 89, 182),
            Color.FromRgb(230, 126, 34)
        };
        DefaultBallSpeed = 50;
        PointsPerBall = 10;
        ComboMultiplier = 2;
        BallRadius = 20;
        MinComboSize = 3;
    }
}

