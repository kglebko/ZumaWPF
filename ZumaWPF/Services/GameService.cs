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
        
        // Level 1 - Волнистая траектория (ВОКРУГ стрелялки 400,300, НЕ проходит через нее)
        // Центр экрана: 400, 300 - все точки минимум на расстоянии 150 от центра
        // Траектория идет вокруг центра, обходя его слева, снизу, справа
        var level1Path = new GamePath(new List<Point>
        {
            new Point(50, 180),
            new Point(150, 140),
            new Point(250, 180),
            new Point(350, 140),
            new Point(450, 180),
            new Point(550, 140),
            new Point(650, 180),
            new Point(750, 220),

            // Плавный уход вниз справа
            new Point(720, 300),
            new Point(680, 380),
            new Point(600, 450),
            new Point(500, 500),
            new Point(380, 520)
        });

        levels.Add(new Level(
            1,
            "Волна",
            level1Path,
            _configService.Config.DefaultBallSpeed,
            "level1.jpg",
            40
        ));
        
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
        
        // Level 3 - Змейка (ВОКРУГ стрелялки 400,300, НЕ проходит через нее)
        // Центр экрана: 400, 300 - все точки минимум на расстоянии 150 от центра
        // Зигзагообразная траектория, обходящая центр
        var level3Path = new GamePath(new List<Point>
        {
            // Нижняя широкая часть
            new Point(100, 500),
            new Point(700, 500),

            // Средний уровень, большой зигзаг
            new Point(700, 400),
            new Point(100, 400),

            // Верхняя часть
            new Point(100, 200),  // поднимаем выше центра
            new Point(700, 200),

            // Верхняя линия
            new Point(700, 50),
            new Point(100, 50)
        });

        levels.Add(new Level(3, "Змейка", level3Path, _configService.Config.DefaultBallSpeed * 1.5, "level3.jpg", 40));

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
                Index = i,
                OriginalIndex = i // Сохраняем оригинальный индекс из AllBalls
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
    
    /// <summary>
    /// Проверяет все комбинации в цепочке после удаления шариков.
    /// Возвращает индекс начала первой найденной комбинации из 3+ шариков одного цвета подряд.
    /// </summary>
    public int? CheckAllCombinations(List<Ball> chain)
    {
        if (chain.Count < _configService.Config.MinComboSize)
            return null;
        
        // Проходим по всей цепочке и ищем группы из 3+ шариков одного цвета подряд
        for (int i = 0; i <= chain.Count - _configService.Config.MinComboSize; i++)
        {
            if (chain[i].IsDestroyed)
                continue;
            
            var targetColor = chain[i].Color;
            var comboSize = 1;
            var comboEnd = i;
            
            // Проверяем, сколько шариков подряд одного цвета
            while (comboEnd < chain.Count - 1 && !chain[comboEnd + 1].IsDestroyed && chain[comboEnd + 1].Color == targetColor)
            {
                comboEnd++;
                comboSize++;
            }
            
            // Если нашли комбинацию из 3+ шариков, возвращаем индекс начала
            if (comboSize >= _configService.Config.MinComboSize)
            {
                return i;
            }
            
            // Пропускаем уже проверенные шарики
            i = comboEnd;
        }
        
        return null;
    }
    
    public int CalculateScore(int comboSize)
    {
        return comboSize * _configService.Config.PointsPerBall * _configService.Config.ComboMultiplier;
    }
}
