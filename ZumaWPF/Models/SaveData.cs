using System.Collections.Generic;

namespace ZumaWPF.Models;

public class SaveData
{
    public string Username { get; set; }
    public int Score { get; set; }
    public int Level { get; set; }
    public List<BallData> ChainBalls { get; set; }
    public double ChainProgress { get; set; }
    
    public SaveData()
    {
        ChainBalls = new List<BallData>();
    }
}

public class BallData
{
    public string Color { get; set; }
    public double PositionX { get; set; }
    public double PositionY { get; set; }
    public int Index { get; set; }
}

