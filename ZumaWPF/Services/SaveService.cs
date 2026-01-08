using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Xml.Linq;
using ZumaWPF.Models;

namespace ZumaWPF.Services;

public class SaveService
{
    private const string SavePath = "savegame.xml";
    
    public bool HasSaveGame()
    {
        return File.Exists(SavePath);
    }
    
    public SaveData? LoadGame()
    {
        if (!File.Exists(SavePath))
            return null;
        
        try
        {
            var doc = XDocument.Load(SavePath);
            var root = doc.Root ?? throw new Exception("Invalid save file");
            
            var saveData = new SaveData
            {
                Username = root.Element("Username")?.Value ?? "",
                Score = int.Parse(root.Element("Score")?.Value ?? "0"),
                Level = int.Parse(root.Element("Level")?.Value ?? "1"),
                ChainProgress = double.Parse(root.Element("ChainProgress")?.Value ?? "0"),
                NextBallIndex = int.Parse(root.Element("NextBallIndex")?.Value ?? "0")
            };
            
            var ballsElement = root.Element("ChainBalls");
            if (ballsElement != null)
            {
                saveData.ChainBalls = ballsElement.Elements("Ball")
                    .Select(e => new BallData
                    {
                        Color = e.Element("Color")?.Value ?? Colors.Red.ToString(),
                        PositionX = double.Parse(e.Element("PositionX")?.Value ?? "0"),
                        PositionY = double.Parse(e.Element("PositionY")?.Value ?? "0"),
                        Index = int.Parse(e.Element("Index")?.Value ?? "0"),
                        IsDestroyed = bool.Parse(e.Element("IsDestroyed")?.Value ?? "false")
                    })
                    .ToList();
            }
            
            return saveData;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading save: {ex.Message}");
            return null;
        }
    }
    
    public void SaveGame(GameState gameState, string username)
    {
        try
        {
            var doc = new XDocument(
                new XElement("SaveGame",
                    new XElement("Username", username),
                    new XElement("Score", gameState.Score),
                    new XElement("Level", gameState.CurrentLevel),
                    new XElement("ChainProgress", gameState.ChainProgress),
                    new XElement("NextBallIndex", gameState.NextBallIndex),
                    new XElement("ChainBalls",
                        gameState.AllBalls.Select((ball, index) => new XElement("Ball",
                            new XElement("Color", ball.Color.ToString()),
                            new XElement("PositionX", ball.Position.X),
                            new XElement("PositionY", ball.Position.Y),
                            new XElement("Index", index),
                            new XElement("IsDestroyed", ball.IsDestroyed)
                        ))
                    )
                )
            );
            
            doc.Save(SavePath);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving game: {ex.Message}");
        }
    }
    
    public void DeleteSaveGame()
    {
        try
        {
            if (File.Exists(SavePath))
                File.Delete(SavePath);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting save: {ex.Message}");
        }
    }
}

