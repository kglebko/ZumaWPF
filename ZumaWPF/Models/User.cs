namespace ZumaWPF.Models;

public class User
{
    public string Username { get; set; }
    public int BestScore { get; set; }
    public int CurrentLevel { get; set; }
    public int TotalScore { get; set; }
    
    public User(string username)
    {
        Username = username;
        BestScore = 0;
        CurrentLevel = 1;
        TotalScore = 0;
    }
}

