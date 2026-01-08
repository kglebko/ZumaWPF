using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ZumaWPF.Models;

namespace ZumaWPF.Services;

public class UserService
{
    private const string UsersPath = "users.xml";
    private const string HighScoresPath = "highscores.xml";
    private User? _currentUser;
    
    public User? CurrentUser => _currentUser;
    
    public List<User> GetAllUsers()
    {
        if (!File.Exists(UsersPath))
            return new List<User>();
        
        try
        {
            var doc = XDocument.Load(UsersPath);
            return doc.Root?.Elements("User")
                .Select(e => new User(e.Element("Username")?.Value ?? "")
                {
                    BestScore = int.Parse(e.Element("BestScore")?.Value ?? "0"),
                    CurrentLevel = int.Parse(e.Element("CurrentLevel")?.Value ?? "1"),
                    TotalScore = int.Parse(e.Element("TotalScore")?.Value ?? "0")
                })
                .ToList() ?? new List<User>();
        }
        catch
        {
            return new List<User>();
        }
    }
    
    public bool Login(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return false;
        
        var users = GetAllUsers();
        _currentUser = users.FirstOrDefault(u => u.Username == username);
        
        if (_currentUser == null)
        {
            _currentUser = new User(username);
            users.Add(_currentUser);
        }
        
        SaveUsers(users);
        return true;
    }
    
    public void UpdateUserStats(int score, int level)
    {
        if (_currentUser == null) return;
        
        _currentUser.TotalScore += score;
        if (score > _currentUser.BestScore)
            _currentUser.BestScore = score;
        if (level > _currentUser.CurrentLevel)
            _currentUser.CurrentLevel = level;
        
        var users = GetAllUsers();
        var existingUser = users.FirstOrDefault(u => u.Username == _currentUser.Username);
        if (existingUser != null)
        {
            existingUser.BestScore = _currentUser.BestScore;
            existingUser.CurrentLevel = _currentUser.CurrentLevel;
            existingUser.TotalScore = _currentUser.TotalScore;
        }
        else
        {
            users.Add(_currentUser);
        }
        
        SaveUsers(users);
    }
    
    private void SaveUsers(List<User> users)
    {
        try
        {
            var doc = new XDocument(
                new XElement("Users",
                    users.Select(u => new XElement("User",
                        new XElement("Username", u.Username),
                        new XElement("BestScore", u.BestScore),
                        new XElement("CurrentLevel", u.CurrentLevel),
                        new XElement("TotalScore", u.TotalScore)
                    ))
                )
            );
            
            doc.Save(UsersPath);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving users: {ex.Message}");
        }
    }
    
    public List<HighScore> GetHighScores(int count = 10)
    {
        if (!File.Exists(HighScoresPath))
            return new List<HighScore>();
        
        try
        {
            var doc = XDocument.Load(HighScoresPath);
            return doc.Root?.Elements("HighScore")
                .Select(e => new HighScore(
                    e.Element("Username")?.Value ?? "",
                    int.Parse(e.Element("Score")?.Value ?? "0"),
                    int.Parse(e.Element("Level")?.Value ?? "1")
                )
                {
                    Date = DateTime.Parse(e.Element("Date")?.Value ?? DateTime.Now.ToString())
                })
                .OrderByDescending(h => h.Score)
                .ThenByDescending(h => h.Level)
                .Take(count)
                .ToList() ?? new List<HighScore>();
        }
        catch
        {
            return new List<HighScore>();
        }
    }
    
    public void AddHighScore(string username, int score, int level)
    {
        var scores = GetHighScores(100).ToList();
        scores.Add(new HighScore(username, score, level));
        
        try
        {
            var doc = new XDocument(
                new XElement("HighScores",
                    scores.OrderByDescending(s => s.Score)
                        .ThenByDescending(s => s.Level)
                        .Take(50)
                        .Select(s => new XElement("HighScore",
                            new XElement("Username", s.Username),
                            new XElement("Score", s.Score),
                            new XElement("Level", s.Level),
                            new XElement("Date", s.Date.ToString("o"))
                        ))
                )
            );
            
            doc.Save(HighScoresPath);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving high scores: {ex.Message}");
        }
    }
}

