using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Xml.Linq;
using ZumaWPF.Models;

namespace ZumaWPF.Services;

public class ConfigService
{
    private const string ConfigPath = "config.xml";
    public GameConfig Config { get; private set; }
    
    public ConfigService()
    {
        Config = new GameConfig();
        LoadConfig();
    }
    
    public void LoadConfig()
    {
        if (!File.Exists(ConfigPath))
        {
            CreateDefaultConfig();
            return;
        }
        
        try
        {
            var doc = XDocument.Load(ConfigPath);
            var root = doc.Root ?? throw new Exception("Invalid config file");
            
            Config.DefaultBallSpeed = double.Parse(root.Element("BallSpeed")?.Value ?? "50");
            Config.PointsPerBall = int.Parse(root.Element("PointsPerBall")?.Value ?? "10");
            Config.ComboMultiplier = int.Parse(root.Element("ComboMultiplier")?.Value ?? "2");
            Config.BallRadius = double.Parse(root.Element("BallRadius")?.Value ?? "20");
            Config.MinComboSize = int.Parse(root.Element("MinComboSize")?.Value ?? "3");
            
            var colorsElement = root.Element("BallColors");
            if (colorsElement != null)
            {
                Config.BallColors = colorsElement.Elements("Color")
                    .Select(e =>
                    {
                        var colorObj = ColorConverter.ConvertFromString(e.Value);
                        return colorObj is Color color ? color : Colors.Red;
                    })
                    .ToList();
            }
        }
        catch
        {
            CreateDefaultConfig();
        }
    }
    
    public void SaveConfig()
    {
        try
        {
            var doc = new XDocument(
                new XElement("GameConfig",
                    new XElement("BallSpeed", Config.DefaultBallSpeed),
                    new XElement("PointsPerBall", Config.PointsPerBall),
                    new XElement("ComboMultiplier", Config.ComboMultiplier),
                    new XElement("BallRadius", Config.BallRadius),
                    new XElement("MinComboSize", Config.MinComboSize),
                    new XElement("BallColors",
                        Config.BallColors.Select(c => new XElement("Color", c.ToString()))
                    )
                )
            );
            
            doc.Save(ConfigPath);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving config: {ex.Message}");
        }
    }
    
    private void CreateDefaultConfig()
    {
        Config = new GameConfig();
        SaveConfig();
    }
}

