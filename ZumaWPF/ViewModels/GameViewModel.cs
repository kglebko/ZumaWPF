using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using ZumaWPF.Models;
using ZumaWPF.Services;

namespace ZumaWPF.ViewModels;

public class GameViewModel : ViewModelBase
{
    private readonly GameService _gameService;
    private readonly ChainController _chainController;
    private readonly ConfigService _configService;
    private readonly AudioService _audioService;
    private readonly SaveService _saveService;
    private readonly UserService _userService;
    
    private DispatcherTimer _gameTimer;
    private Ball? _flyingBall;
    private Point _mousePosition;
    private double _shootAngle; 
    
    public GameState GameState { get; private set; }
    public List<Level> Levels { get; private set; }
    
    public event Action? GameOver;
    public event Action? Victory;
    public event Action? RequestPause;
    
    public GameViewModel(
        GameService gameService,
        ChainController chainController,
        ConfigService configService,
        AudioService audioService,
        SaveService saveService,
        UserService userService)
    {
        _gameService = gameService;
        _chainController = chainController;
        _configService = configService;
        _audioService = audioService;
        _saveService = saveService;
        _userService = userService;
        
        GameState = new GameState();
        Levels = _gameService.CreateLevels();
        
        _gameTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _gameTimer.Tick += GameTimer_Tick;
    }
    
    public void StartLevel(int levelId)
    {
        var level = Levels.FirstOrDefault(l => l.Id == levelId);
        if (level == null) return;
        
        GameState.CurrentLevel = levelId;
        GameState.ActiveLevel = level;
        GameState.AllBalls = _gameService.GenerateInitialChain(level);
        GameState.Chain = new List<Ball>();
        GameState.NextBallIndex = 0;
        GameState.Score = 0;
        GameState.ChainProgress = 0;
        GameState.IsGameOver = false;
        GameState.IsVictory = false;
        GameState.IsPaused = false;
        
        var shooterPos = new Point(500, 400);
        GameState.Shooter = new Shooter(shooterPos);
        GameState.Shooter.CurrentBall = _gameService.CreateShotBall(shooterPos);
        GameState.Shooter.NextBall = _gameService.CreateShotBall(shooterPos);
        
        _audioService.PlayBackgroundMusic();
        _gameTimer.Start();
        
        OnPropertyChanged(nameof(GameState));
    }
    
    public void LoadSavedGame(SaveData saveData, Level level)
    {
        GameState.CurrentLevel = saveData.Level;
        GameState.ActiveLevel = level;
        GameState.Score = saveData.Score;
        GameState.ChainProgress = saveData.ChainProgress;
        GameState.IsGameOver = false;
        GameState.IsVictory = false;
        GameState.IsPaused = false;
        
        GameState.AllBalls = saveData.ChainBalls.Select(b =>
        {
            var colorObj = ColorConverter.ConvertFromString(b.Color);
            var color = colorObj is Color c ? c : Colors.Red;
            return new Ball(color, new Point(b.PositionX, b.PositionY), _configService.Config.BallRadius)
            {
                Index = b.Index,
                IsDestroyed = b.IsDestroyed
            };
        }).ToList();
        
        GameState.Chain = new List<Ball>();
        var ballSpacing = _configService.Config.BallRadius * 2.2;
        GameState.NextBallIndex = saveData.NextBallIndex;

        for (int i = 0; i < GameState.AllBalls.Count && i < saveData.NextBallIndex; i++)
        {
            var spawnDistance = i * ballSpacing;
            if (GameState.ChainProgress >= spawnDistance)
            {
                var ball = GameState.AllBalls[i];
                if (!ball.IsDestroyed)
                {
                    GameState.Chain.Add(ball);
                }
            }
        }
        
        for (int i = 0; i < GameState.Chain.Count; i++)
        {
            var ballDistance = GameState.ChainProgress - i * ballSpacing;
            if (ballDistance >= 0 && ballDistance <= level.Path.TotalLength)
            {
                GameState.Chain[i].Position = level.Path.GetPointAtDistance(ballDistance);
            }
        }
        
        var shooterPos = new Point(500, 400);
        GameState.Shooter = new Shooter(shooterPos);
        GameState.Shooter.CurrentBall = _gameService.CreateShotBall(shooterPos);
        GameState.Shooter.NextBall = _gameService.CreateShotBall(shooterPos);
        
        _audioService.PlayBackgroundMusic();
        _gameTimer.Start();
        
        OnPropertyChanged(nameof(GameState));
    }
    
    public void UpdateMousePosition(Point position)
    {
        _mousePosition = position;
        if (GameState.Shooter != null)
        {
            var dx = position.X - GameState.Shooter.Position.X;
            var dy = position.Y - GameState.Shooter.Position.Y;
            GameState.Shooter.Angle = Math.Atan2(dy, dx) * 180 / Math.PI;
            OnPropertyChanged(nameof(GameState));
        }
    }
    
    public void Shoot()
    {
        if (GameState.Shooter == null || GameState.Shooter.CurrentBall == null || _flyingBall != null)
            return;
        
        if (GameState.IsPaused || GameState.IsGameOver || GameState.IsVictory)
            return;
        
        _shootAngle = GameState.Shooter.Angle;
        
        _flyingBall = GameState.Shooter.CurrentBall;
        _flyingBall.Position = GameState.Shooter.Position;
        
        GameState.Shooter.CurrentBall = GameState.Shooter.NextBall;
        GameState.Shooter.NextBall = _gameService.CreateShotBall(GameState.Shooter.Position);
        GameState.Shooter.IsShooting = true;
        
        _audioService.PlayShootSound();
        
        OnPropertyChanged(nameof(GameState));
    }
    
    public void Pause()
    {
        if (GameState.IsGameOver || GameState.IsVictory)
            return;
        
        if (!GameState.IsPaused)
        {
            GameState.IsPaused = true;
            _gameTimer.Stop();
            OnPropertyChanged(nameof(GameState));
        }
    }
    
    public void Resume()
    {
        GameState.IsPaused = false;
        _gameTimer.Start();
        _audioService.PlayBackgroundMusic();
        OnPropertyChanged(nameof(GameState));
    }
    
    public void SaveGame()
    {
        if (_userService.CurrentUser != null && GameState.Score > 0)
        {
            _userService.AddHighScore(_userService.CurrentUser.Username, GameState.Score, GameState.CurrentLevel);
        }
    }
    
    private void GameTimer_Tick(object? sender, EventArgs e)
    {
        if (GameState.IsPaused || GameState.IsGameOver || GameState.IsVictory)
            return;
        
        var deltaTime = _gameTimer.Interval.TotalMilliseconds;
        
        _chainController.UpdateChain(GameState, deltaTime);
        
        if (_flyingBall != null)
        {
            var speed = 500.0 * deltaTime / 1000.0;
            var angle = _shootAngle * Math.PI / 180;
            var dx = Math.Cos(angle) * speed;
            var dy = Math.Sin(angle) * speed;
            
            _flyingBall.Position = new Point(
                _flyingBall.Position.X + dx,
                _flyingBall.Position.Y + dy
            );
            
            if (GameState.ActiveLevel != null && _flyingBall != null)
            {
                var minDistance = double.MaxValue;
                Ball? closestBall = null;
                
                foreach (var chainBall in GameState.Chain)
                {
                    if (chainBall.IsDestroyed) continue;
                    
                    var dx2 = _flyingBall.Position.X - chainBall.Position.X;
                    var dy2 = _flyingBall.Position.Y - chainBall.Position.Y;
                    var distance = Math.Sqrt(dx2 * dx2 + dy2 * dy2);
                    
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestBall = chainBall;
                    }
                }
                
                if (closestBall != null && minDistance < _configService.Config.BallRadius * 2)
                {
                    if (_chainController.TryInsertBall(GameState, _flyingBall, closestBall.Position))
                    {
                    }
                    else
                    {
                        _audioService.PlayHitSound();
                    }
                    _flyingBall = null;
                    GameState.Shooter.IsShooting = false;
                }
                else
                {
                    if (_flyingBall.Position.X < -50 || _flyingBall.Position.X > 1050 ||
                        _flyingBall.Position.Y < -50 || _flyingBall.Position.Y > 850)
                    {
                        _flyingBall = null;
                        GameState.Shooter.IsShooting = false;
                    }
                }
            }
        }
        
        if (GameState.IsGameOver)
        {
            _gameTimer.Stop();
            if (_userService.CurrentUser != null)
            {
                _userService.AddHighScore(_userService.CurrentUser.Username, GameState.Score, GameState.CurrentLevel);
                _userService.UpdateUserStats(GameState.Score, GameState.CurrentLevel);
            }
            GameOver?.Invoke();
        }
        
        if (GameState.IsVictory)
        {
            _gameTimer.Stop();
            if (_userService.CurrentUser != null)
            {
                _userService.AddHighScore(_userService.CurrentUser.Username, GameState.Score, GameState.CurrentLevel);
                _userService.UpdateUserStats(GameState.Score, GameState.CurrentLevel);
            }
            Victory?.Invoke();
        }
        
        OnPropertyChanged(nameof(GameState));
    }
    
    public Ball? GetFlyingBall() => _flyingBall;
    
    public void Stop()
    {
        _gameTimer.Stop();
    }
}

public class ViewModelBase : System.ComponentModel.INotifyPropertyChanged
{
    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    
    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
    }
}

