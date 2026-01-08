using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ZumaWPF.Models;

namespace ZumaWPF.Services;

public class ChainController
{
    private readonly GameService _gameService;
    private readonly ConfigService _configService;
    
    public ChainController(GameService gameService, ConfigService configService)
    {
        _gameService = gameService;
        _configService = configService;
    }
    
    public void UpdateChain(GameState gameState, double deltaTime)
    {
        if (gameState.ActiveLevel == null)
            return;
        
        if (gameState.IsPaused || gameState.IsGameOver || gameState.IsVictory)
            return;
        
        var speed = gameState.ActiveLevel.BallSpeed * deltaTime / 1000.0;
        var ballSpacing = _configService.Config.BallRadius * 2.2;
        var path = gameState.ActiveLevel.Path;
        
        // Увеличиваем прогресс движения цепочки
        gameState.ChainProgress += speed;
        
        // Добавляем новые шарики в КОНЕЦ цепочки (они появляются в начале пути)
        while (gameState.NextBallIndex < gameState.AllBalls.Count)
        {
            // Вычисляем, когда должен появиться следующий шарик
            // Шарики появляются один за другим с интервалом ballSpacing
            var spawnDistance = gameState.NextBallIndex * ballSpacing;
            
            // Если прогресс достиг точки спавна этого шарика
            if (gameState.ChainProgress >= spawnDistance)
            {
                var ballToAdd = gameState.AllBalls[gameState.NextBallIndex];
                // Шарик появляется в начале пути (дистанция 0)
                ballToAdd.Position = path.GetPointAtDistance(0);
                // Добавляем в КОНЕЦ цепочки (последний элемент)
                gameState.Chain.Add(ballToAdd);
                gameState.NextBallIndex++;
            }
            else
            {
                break;
            }
        }
        
        // Обновляем позиции всех шариков в цепочке
        // Первый шарик (индекс 0) - самый передний, находится на ChainProgress
        // Каждый следующий шарик отстает на ballSpacing
        for (int i = 0; i < gameState.Chain.Count; i++)
        {
            if (gameState.Chain[i].IsDestroyed)
                continue;
            
            // Вычисляем дистанцию этого шарика от начала пути
            // Индекс 0 - самый передний (ChainProgress)
            // Индекс 1 - отстает на ballSpacing
            // Индекс 2 - отстает на 2*ballSpacing и т.д.
            var ballDistance = gameState.ChainProgress - i * ballSpacing;
            
            if (ballDistance >= 0 && ballDistance <= path.TotalLength)
            {
                // Обновляем позицию шарика на траектории
                var newPosition = path.GetPointAtDistance(ballDistance);
                gameState.Chain[i].Position = newPosition;
            }
            else if (ballDistance > path.TotalLength)
            {
                // Шарик достиг конца пути - проигрыш
                gameState.IsGameOver = true;
                return;
            }
        }
        
        // Удаляем уничтоженные шарики
        gameState.Chain.RemoveAll(b => b.IsDestroyed);
        
        // Переиндексируем оставшиеся шарики
        for (int i = 0; i < gameState.Chain.Count; i++)
        {
            gameState.Chain[i].Index = i;
        }
        
        // Проверяем условие победы
        if (gameState.Chain.Count == 0 && gameState.NextBallIndex >= gameState.AllBalls.Count)
        {
            gameState.IsVictory = true;
        }
    }
    
    public bool TryInsertBall(GameState gameState, Ball shotBall, Point hitPoint)
    {
        if (gameState.ActiveLevel == null || gameState.Chain.Count == 0)
            return false;
        
        var ballSpacing = _configService.Config.BallRadius * 2.2;
        var path = gameState.ActiveLevel.Path;
        
        // Находим ближайший шарик к точке попадания
        int closestIndex = -1;
        double minDistance = double.MaxValue;
        
        for (int i = 0; i < gameState.Chain.Count; i++)
        {
            if (gameState.Chain[i].IsDestroyed) continue;
            
            var dx = hitPoint.X - gameState.Chain[i].Position.X;
            var dy = hitPoint.Y - gameState.Chain[i].Position.Y;
            var dist = Math.Sqrt(dx * dx + dy * dy);
            
            if (dist < minDistance)
            {
                minDistance = dist;
                closestIndex = i;
            }
        }
        
        // Проверяем, достаточно ли близко попадание
        if (closestIndex < 0 || minDistance > _configService.Config.BallRadius * 3)
            return false;
        
        // Вставляем шарик ПОСЛЕ ближайшего (все шарики за ним сдвигаются назад)
        var insertIndex = closestIndex + 1;
        if (insertIndex > gameState.Chain.Count) insertIndex = gameState.Chain.Count;
        
        // Устанавливаем позицию вставленного шарика (на позиции ближайшего)
        shotBall.Position = gameState.Chain[closestIndex].Position;
        gameState.Chain.Insert(insertIndex, shotBall);
        
        // Переиндексируем
        for (int i = 0; i < gameState.Chain.Count; i++)
        {
            gameState.Chain[i].Index = i;
        }
        
        // Проверяем комбинации
        var comboStart = _gameService.CheckCombinations(gameState.Chain, insertIndex);
        if (comboStart.HasValue)
        {
            var comboSize = 0;
            var comboColor = gameState.Chain[insertIndex].Color;
            
            // Подсчитываем размер комбо
            for (int i = comboStart.Value; i < gameState.Chain.Count; i++)
            {
                if (gameState.Chain[i].Color == comboColor && !gameState.Chain[i].IsDestroyed)
                    comboSize++;
                else
                    break;
            }
            
            // Удаляем шарики комбо
            for (int i = 0; i < comboSize; i++)
            {
                if (comboStart.Value + i < gameState.Chain.Count)
                    gameState.Chain[comboStart.Value + i].IsDestroyed = true;
            }
            
            // Начисляем очки
            var score = _gameService.CalculateScore(comboSize);
            gameState.Score += score;
            
            return true;
        }
        
        return false;
    }
}
