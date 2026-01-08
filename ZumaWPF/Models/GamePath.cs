using System.Collections.Generic;
using System.Windows;
using System.Linq;

namespace ZumaWPF.Models;

public class GamePath
{
    public List<Point> Points { get; set; }
    public double TotalLength { get; private set; }
    
    public GamePath(List<Point> points)
    {
        Points = points;
        CalculateTotalLength();
    }
    
    private void CalculateTotalLength()
    {
        TotalLength = 0;
        for (int i = 1; i < Points.Count; i++)
        {
            var dx = Points[i].X - Points[i - 1].X;
            var dy = Points[i].Y - Points[i - 1].Y;
            TotalLength += System.Math.Sqrt(dx * dx + dy * dy);
        }
    }
    
    public Point GetPointAtDistance(double distance)
    {
        if (Points.Count < 2) return Points.FirstOrDefault();
        if (distance <= 0) return Points[0];
        if (distance >= TotalLength) return Points[Points.Count - 1];
        
        double accumulated = 0;
        for (int i = 1; i < Points.Count; i++)
        {
            var dx = Points[i].X - Points[i - 1].X;
            var dy = Points[i].Y - Points[i - 1].Y;
            var segmentLength = System.Math.Sqrt(dx * dx + dy * dy);
            
            if (accumulated + segmentLength >= distance)
            {
                var ratio = (distance - accumulated) / segmentLength;
                return new Point(
                    Points[i - 1].X + dx * ratio,
                    Points[i - 1].Y + dy * ratio
                );
            }
            
            accumulated += segmentLength;
        }
        
        return Points[Points.Count - 1];
    }
    
    public double GetDistanceFromStart(Point point, double tolerance = 50)
    {
        double minDistance = double.MaxValue;
        double bestDistance = 0;
        double accumulated = 0;
        
        for (int i = 1; i < Points.Count; i++)
        {
            var dx = Points[i].X - Points[i - 1].X;
            var dy = Points[i].Y - Points[i - 1].Y;
            var segmentLength = System.Math.Sqrt(dx * dx + dy * dy);
            
            if (segmentLength > 0)
            {
                for (double t = 0; t <= 1; t += 0.01)
                {
                    var segmentPoint = new Point(
                        Points[i - 1].X + dx * t,
                        Points[i - 1].Y + dy * t
                    );
                    
                    var distToPoint = System.Math.Sqrt(
                        System.Math.Pow(point.X - segmentPoint.X, 2) + 
                        System.Math.Pow(point.Y - segmentPoint.Y, 2)
                    );
                    
                    if (distToPoint < minDistance)
                    {
                        minDistance = distToPoint;
                        bestDistance = accumulated + segmentLength * t;
                    }
                }
            }
            
            accumulated += segmentLength;
        }
        
        return minDistance <= tolerance ? bestDistance : -1;
    }
}

