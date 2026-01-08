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
        
        // Level 1 - Волнистая траектория (НЕ проходит через центр 400,300)
        // Центр экрана: 400, 300 - все точки должны быть минимум на расстоянии 150 от центра
        var level1Path = new GamePath(new List<Point>
        {
            new Point(50, 100),
            new Point(150, 80),
            new Point(250, 100),
            new Point(300, 120),
            new Point(500, 100),
            new Point(550, 80),
            new Point(650, 100),
            new Point(750, 120),
            new Point(700, 200),
            new Point(600, 250),
            new Point(500, 280),
            new Point(300, 320),
            new Point(200, 300),
            new Point(100, 280),
            new Point(50, 350),
            new Point(100, 450),
            new Point(200, 500),
            new Point(300, 520),
            new Point(500, 500),
            new Point(650, 450)
        });
        levels.Add(new Level(1, "Волна", level1Path, _configService.Config.DefaultBallSpeed, "level1.jpg", 40));
        
        // Level 2 - Спираль (НЕ проходит через центр 400,300)
        var level2Path = new GamePath(new List<Point>
        {
            new Point(100, 50),
            new Point(200, 80),
            new Point(300, 100),
            new Point(500, 120),
            new Point(600, 150),
            new Point(700, 200),
            new Point(750, 350),
            new Point(700, 500),
            new Point(600, 550),
            new Point(500, 520),
            new Point(300, 500),
            new Point(200, 450),
            new Point(150, 350),
            new Point(180, 250),
            new Point(250, 200),
            new Point(500, 200),
            new Point(600, 250),
            new Point(650, 350),
            new Point(600, 450)
        });
        levels.Add(new Level(2, "Спираль", level2Path, _configService.Config.DefaultBallSpeed * 1.2, "level2.jpg", 45));
        
        // Level 3 - Зигзаг (НЕ проходит через центр 400,300, не пересекается)
        var level3Path = new GamePath(new List<Point>
        {
            new Point(50, 150),
            new Point(150, 100),
            new Point(250, 150),
            new Point(300, 100),
            new Point(500, 150),
            new Point(550, 100),
            new Point(650, 150),
            new Point(750, 200),
            new Point(700, 300),
            new Point(600, 350),
            new Point(500, 400),
            new Point(300, 450),
            new Point(200, 500),
            new Point(100, 480),
            new Point(80, 350),
            new Point(120, 280),
            new Point(200, 250),
            new Point(300, 280),
            new Point(500, 320),
            new Point(600, 300),
            new Point(680, 250)
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
            // Позиция будет установлена позже при добавлении в цепочку
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
