namespace ZumaWPF.Models;

public class HighScore
{
    public string Username { get; set; }
    public int Score { get; set; }
    public int Level { get; set; }
    public System.DateTime Date { get; set; }
    
    public HighScore(string username, int score, int level)
    {
        Username = username;
        Score = score;
        Level = level;
        Date = System.DateTime.Now;
    }
}

