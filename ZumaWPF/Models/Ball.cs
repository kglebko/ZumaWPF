using System.Windows.Media;
using System.Windows;

namespace ZumaWPF.Models;

public class Ball
{
    public Color Color { get; set; }
    public Point Position { get; set; }
    public double Radius { get; set; }
    public int Index { get; set; }
    public int OriginalIndex { get; set; } 
    public bool IsDestroyed { get; set; }
    
    public Ball(Color color, Point position, double radius = 20)
    {
        Color = color;
        Position = position;
        Radius = radius;
        IsDestroyed = false;
        OriginalIndex = -1;
    }
}

