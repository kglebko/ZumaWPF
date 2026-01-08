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
    private readonly AudioService? _audioService;
    
    public ChainController(GameService gameService, ConfigService configService, AudioService? audioService = null)
    {
        _gameService = gameService;
        _configService = configService;
        _audioService = audioService;
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
            var spawnDistance = gameState.NextBallIndex * ballSpacing;
            
            if (gameState.ChainProgress >= spawnDistance)
            {
                var ballToAdd = gameState.AllBalls[gameState.NextBallIndex];
                ballToAdd.Position = path.GetPointAtDistance(0);
                gameState.Chain.Add(ballToAdd);
                gameState.NextBallIndex++;
            }
            else
            {
                break;
            }
        }
        
        // Обновляем позиции ВСЕХ шариков в цепочке каждый кадр
        // Первый шарик (индекс 0) - самый передний, находится на ChainProgress
        // Каждый следующий шарик отстает на ballSpacing
        for (int i = 0; i < gameState.Chain.Count; i++)
        {
            if (gameState.Chain[i].IsDestroyed)
                continue;
            
            var ballDistance = gameState.ChainProgress - i * ballSpacing;
            
            if (ballDistance >= 0 && ballDistance <= path.TotalLength)
            {
                // ВАЖНО: Обновляем позицию каждый кадр, чтобы шарики двигались
                gameState.Chain[i].Position = path.GetPointAtDistance(ballDistance);
            }
            else if (ballDistance > path.TotalLength)
            {
                // Шарик достиг конца пути
                // Проверяем условие победы: набрано >500 очков
                if (gameState.Score > 500)
                {
                    gameState.IsVictory = true;
                }
                else
                {
                    gameState.IsGameOver = true;
                }
                return;
            }
        }
        
        // Удаляем уничтоженные шарики
        var removedCount = gameState.Chain.RemoveAll(b => b.IsDestroyed);
        
        // Если были удалены шарики, проверяем каскадные комбо
        if (removedCount > 0)
        {
            CheckCascadeCombos(gameState);
        }
        
        // Переиндексируем оставшиеся шарики
        for (int i = 0; i < gameState.Chain.Count; i++)
        {
            gameState.Chain[i].Index = i;
        }
        
        // Проверяем условие победы: все шарики уничтожены И набрано >500 очков
        if (gameState.Chain.Count == 0 && gameState.NextBallIndex >= gameState.AllBalls.Count)
        {
            if (gameState.Score > 500)
            {
                gameState.IsVictory = true;
            }
            else
            {
                // Недостаточно очков - продолжаем ждать, пока шарики дойдут до конца
            }
        }
    }
    
    private void CheckCascadeCombos(GameState gameState)
    {
        // Проверяем все возможные комбо после удаления шариков
        bool foundCombo = true;
        
        while (foundCombo)
        {
            foundCombo = false;
            
            for (int i = 0; i < gameState.Chain.Count; i++)
            {
                if (gameState.Chain[i].IsDestroyed) continue;
                
                var comboStart = _gameService.CheckCombinations(gameState.Chain, i);
                if (comboStart.HasValue)
                {
                    var comboSize = 0;
                    var comboColor = gameState.Chain[i].Color;
                    
                    // Подсчитываем размер комбо
                    for (int j = comboStart.Value; j < gameState.Chain.Count; j++)
                    {
                        if (gameState.Chain[j].Color == comboColor && !gameState.Chain[j].IsDestroyed)
                            comboSize++;
                        else
                            break;
                    }
                    
                    if (comboSize >= _configService.Config.MinComboSize)
                    {
                        // Удаляем шарики комбо
                        for (int j = 0; j < comboSize; j++)
                        {
                            if (comboStart.Value + j < gameState.Chain.Count)
                                gameState.Chain[comboStart.Value + j].IsDestroyed = true;
                        }
                        
                        // Начисляем очки
                        var score = _gameService.CalculateScore(comboSize);
                        gameState.Score += score;
                        
                        // Проигрываем звук комбо
                        _audioService?.PlayComboSound();
                        
                        foundCombo = true;
                        break; // Начинаем проверку заново после удаления
                    }
                }
            }
            
            // Удаляем уничтоженные шарики перед следующей проверкой
            if (foundCombo)
            {
                gameState.Chain.RemoveAll(b => b.IsDestroyed);
                
                // Переиндексируем
                for (int i = 0; i < gameState.Chain.Count; i++)
                {
                    gameState.Chain[i].Index = i;
                }
            }
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
        
        // Вычисляем дистанцию ближайшего шарика от начала пути
        var closestBallDistance = gameState.ChainProgress - closestIndex * ballSpacing;
        
        // Вставляем шарик ПОСЛЕ ближайшего
        var insertIndex = closestIndex + 1;
        if (insertIndex > gameState.Chain.Count) insertIndex = gameState.Chain.Count;
        
        // Устанавливаем позицию вставленного шарика на основе дистанции
        shotBall.Position = path.GetPointAtDistance(closestBallDistance);
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
            
            // Проигрываем звук комбо
            _audioService?.PlayComboSound();
            
            // Проверяем каскадные комбо после удаления
            gameState.Chain.RemoveAll(b => b.IsDestroyed);
            CheckCascadeCombos(gameState);
            
            return true;
        }
        
        return false;
    }
}
