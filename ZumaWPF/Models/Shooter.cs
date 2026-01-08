using System.Windows;
using System.Windows.Media;

namespace ZumaWPF.Models;

public class Shooter
{
    public Point Position { get; set; }
    public double Angle { get; set; }
    public Ball? CurrentBall { get; set; }
    public Ball? NextBall { get; set; }
    public bool IsShooting { get; set; }
    
    public Shooter(Point position)
    {
        Position = position;
        Angle = 0;
        IsShooting = false;
    }
}

