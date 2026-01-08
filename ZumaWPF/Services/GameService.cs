using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using ZumaWPF.Models;

namespace ZumaWPF.Services;

public class GameService
{
    private readonly ConfigService _configService;
    private Random _random = new Random();
    
    public GameService(ConfigService configService)
    {
        _configService = configService;
    }
    
    public List<Level> CreateLevels()
    {
        var levels = new List<Level>();
        
        // Центр экрана: 500, 400 (ширина 1000, высота 800)
        // Траектория должна идти ВОКРУГ центра, не проходя через него
        // Минимальное расстояние от центра: 150 пикселей
        
        // Level 1 - Волна (простая закорючка вокруг центра)
        var level1Path = new GamePath(new List<Point>
        {
            new Point(100, 200),
            new Point(200, 150),
            new Point(300, 180),
            new Point(400, 160),
            new Point(600, 160),
            new Point(700, 180),
            new Point(800, 150),
            new Point(900, 200),
            new Point(850, 300),
            new Point(750, 400),
            new Point(600, 500),
            new Point(400, 550),
            new Point(250, 500),
            new Point(150, 400),
            new Point(100, 300)
        });
        levels.Add(new Level(1, "Волна", level1Path, _configService.Config.DefaultBallSpeed, "level1.jpg", 40));
        
        // Level 2 - Спираль (не очень сложная, вокруг центра)
        var level2Path = new GamePath(new List<Point>
        {
            new Point(150, 100),
            new Point(300, 80),
            new Point(500, 80),
            new Point(700, 80),
            new Point(850, 100),
            new Point(900, 200),
            new Point(900, 300),
            new Point(850, 400),
            new Point(750, 500),
            new Point(600, 600),
            new Point(400, 650),
            new Point(250, 600),
            new Point(150, 500),
            new Point(100, 400),
            new Point(100, 300),
            new Point(100, 200)
        });
        levels.Add(new Level(2, "Спираль", level2Path, _configService.Config.DefaultBallSpeed * 1.2, "level2.jpg", 45));
        
        // Level 3 - Змейка (как в примере, но с координатами для 1000x800)
        var level3Path = new GamePath(new List<Point>
        {
            // Нижняя широкая часть
            new Point(150, 650),
            new Point(850, 650),
            
            // Средний уровень, большой зигзаг
            new Point(850, 550),
            new Point(150, 550),
            
            // Верхняя часть
            new Point(150, 350),
            new Point(850, 350),
            
            // Верхняя линия
            new Point(850, 100),
            new Point(150, 100)
        });
        levels.Add(new Level(3, "Змейка", level3Path, _configService.Config.DefaultBallSpeed * 1.5, "level3.jpg", 50));
        
        return levels;
    }
    
    public Color GetRandomBallColor()
    {
        var colors = _configService.Config.BallColors;
        return colors[_random.Next(colors.Count)];
    }
    
    public List<Ball> GenerateInitialChain(Level level)
    {
        var chain = new List<Ball>();
        var ballSpacing = _configService.Config.BallRadius * 2.2;
        
        // Генерируем все шарики заранее с фиксированными цветами
        for (int i = 0; i < level.InitialBallCount; i++)
        {
            var color = GetRandomBallColor();
            var ball = new Ball(color, new Point(0, 0), _configService.Config.BallRadius)
            {
                Index = i
            };
            chain.Add(ball);
        }
        
        return chain;
    }
    
    public Ball CreateShotBall(Point shooterPosition)
    {
        return new Ball(GetRandomBallColor(), shooterPosition, _configService.Config.BallRadius);
    }
    
    public int? CheckCombinations(List<Ball> chain, int insertIndex)
    {
        if (insertIndex < 0 || insertIndex >= chain.Count)
            return null;
        
        var targetColor = chain[insertIndex].Color;
        var comboStart = insertIndex;
        var comboEnd = insertIndex;
        
        // Проверяем влево
        while (comboStart > 0 && !chain[comboStart - 1].IsDestroyed && chain[comboStart - 1].Color == targetColor)
            comboStart--;
        
        // Проверяем вправо
        while (comboEnd < chain.Count - 1 && !chain[comboEnd + 1].IsDestroyed && chain[comboEnd + 1].Color == targetColor)
            comboEnd++;
        
        var comboSize = comboEnd - comboStart + 1;
        
        if (comboSize >= _configService.Config.MinComboSize)
        {
            return comboStart;
        }
        
        return null;
    }
    
    public int CalculateScore(int comboSize)
    {
        return comboSize * _configService.Config.PointsPerBall * _configService.Config.ComboMultiplier;
    }
}
