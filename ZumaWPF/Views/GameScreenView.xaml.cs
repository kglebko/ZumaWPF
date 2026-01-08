using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using ZumaWPF.Models;
using ZumaWPF.ViewModels;

namespace ZumaWPF.Views;

public partial class GameScreenView : UserControl
{
    private GameViewModel? _viewModel;
    private DispatcherTimer _renderTimer;
    
    public event Action? GameOver;
    public event Action? Victory;
    public event Action? RequestPause;
    
    public GameScreenView()
    {
        InitializeComponent();
        _renderTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _renderTimer.Tick += RenderTimer_Tick;
        
        // Handle ESC key
        KeyDown += GameScreenView_KeyDown;
    }
    
    private void GameScreenView_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            RequestPause?.Invoke();
            e.Handled = true;
        }
    }
    
    private void GameCanvas_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            RequestPause?.Invoke();
            e.Handled = true;
        }
    }
    
    public void SetViewModel(GameViewModel viewModel)
    {
        _viewModel = viewModel;
        _viewModel.GameOver += () => GameOver?.Invoke();
        _viewModel.Victory += () => Victory?.Invoke();
        _viewModel.RequestPause += () => RequestPause?.Invoke();
        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(GameViewModel.GameState))
            {
                UpdateUI();
            }
        };
        _renderTimer.Start();
        UpdateUI();
        GameCanvas.Focus(); // Ensure canvas can receive keyboard events
    }
    
    private void GameCanvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (_viewModel == null) return;
        var position = e.GetPosition(GameCanvas);
        _viewModel.UpdateMousePosition(position);
    }
    
    private void GameCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_viewModel == null) return;
        _viewModel.Shoot();
    }
    
    private void RenderTimer_Tick(object? sender, EventArgs e)
    {
        RenderGame();
    }
    
    private void RenderGame()
    {
        GameCanvas.Children.Clear();
        
        if (_viewModel?.GameState == null || _viewModel.GameState.ActiveLevel == null)
            return;
        
        var state = _viewModel.GameState;
        
        // Draw path
        DrawPath(state.ActiveLevel.Path);
        
        // Draw chain
        foreach (var ball in state.Chain)
        {
            if (!ball.IsDestroyed)
                DrawBall(ball);
        }
        
        // Draw flying ball
        var flyingBall = _viewModel.GetFlyingBall();
        if (flyingBall != null)
        {
            DrawBall(flyingBall);
        }
        
        // Draw shooter
        DrawShooter(state.Shooter);
    }
    
    private void DrawPath(GamePath path)
    {
        if (path.Points.Count < 2) return;
        
        // Draw path shadow/outline for better visibility
        var shadow = new Polyline
        {
            Stroke = new SolidColorBrush(Color.FromArgb(100, 0, 0, 0)),
            StrokeThickness = 22,
            Points = new PointCollection(path.Points)
        };
        GameCanvas.Children.Add(shadow);
        
        var polyline = new Polyline
        {
            Stroke = new SolidColorBrush(Color.FromRgb(139, 195, 74)), // Приятный зеленый цвет
            StrokeThickness = 20,
            Points = new PointCollection(path.Points)
        };
        GameCanvas.Children.Add(polyline);
    }
    
    private void DrawBall(Ball ball)
    {
        var ellipse = new Ellipse
        {
            Width = ball.Radius * 2,
            Height = ball.Radius * 2,
            Fill = new SolidColorBrush(ball.Color),
            Stroke = new SolidColorBrush(Colors.Black),
            StrokeThickness = 2
        };
        
        Canvas.SetLeft(ellipse, ball.Position.X - ball.Radius);
        Canvas.SetTop(ellipse, ball.Position.Y - ball.Radius);
        
        GameCanvas.Children.Add(ellipse);
    }
    
    private void DrawShooter(Shooter shooter)
    {
        // Draw shooter body with better design
        var body = new Ellipse
        {
            Width = 60,
            Height = 60,
            Fill = new SolidColorBrush(Color.FromRgb(76, 175, 80)), // Приятный зеленый
            Stroke = new SolidColorBrush(Color.FromRgb(56, 142, 60)), // Темно-зеленый
            StrokeThickness = 3
        };
        
        Canvas.SetLeft(body, shooter.Position.X - 30);
        Canvas.SetTop(body, shooter.Position.Y - 30);
        GameCanvas.Children.Add(body);
        
        // Draw shooter base/circle
        var baseCircle = new Ellipse
        {
            Width = 80,
            Height = 80,
            Fill = new SolidColorBrush(Color.FromArgb(80, 76, 175, 80)),
            Stroke = new SolidColorBrush(Color.FromRgb(56, 142, 60)),
            StrokeThickness = 2
        };
        
        Canvas.SetLeft(baseCircle, shooter.Position.X - 40);
        Canvas.SetTop(baseCircle, shooter.Position.Y - 40);
        GameCanvas.Children.Insert(0, baseCircle); // Behind main body
        
        // Draw current ball
        if (shooter.CurrentBall != null)
        {
            var currentBall = shooter.CurrentBall;
            currentBall.Position = shooter.Position;
            DrawBall(currentBall);
        }
        
        // Draw next ball indicator (smaller, offset)
        if (shooter.NextBall != null)
        {
            var nextBallPos = new Point(shooter.Position.X - 40, shooter.Position.Y);
            var nextBall = new Ellipse
            {
                Width = shooter.NextBall.Radius * 1.5,
                Height = shooter.NextBall.Radius * 1.5,
                Fill = new SolidColorBrush(shooter.NextBall.Color),
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 1,
                Opacity = 0.7
            };
            
            Canvas.SetLeft(nextBall, nextBallPos.X - shooter.NextBall.Radius * 0.75);
            Canvas.SetTop(nextBall, nextBallPos.Y - shooter.NextBall.Radius * 0.75);
            GameCanvas.Children.Add(nextBall);
        }
        
        // Draw aim line
        var angle = shooter.Angle * Math.PI / 180;
        var lineLength = 100;
        var line = new Line
        {
            X1 = shooter.Position.X,
            Y1 = shooter.Position.Y,
            X2 = shooter.Position.X + Math.Cos(angle) * lineLength,
            Y2 = shooter.Position.Y + Math.Sin(angle) * lineLength,
            Stroke = new SolidColorBrush(Color.FromArgb(180, 255, 255, 255)), // Полупрозрачная белая линия
            StrokeThickness = 2,
            StrokeDashArray = new DoubleCollection { 5, 5 }
        };
        GameCanvas.Children.Add(line);
    }
    
    private void UpdateUI()
    {
        if (_viewModel?.GameState == null) return;
        
        ScoreText.Text = $"Очки: {_viewModel.GameState.Score}";
        LevelText.Text = $"Уровень: {_viewModel.GameState.CurrentLevel}";
    }
    
    public void Stop()
    {
        _renderTimer.Stop();
        _viewModel?.Stop();
    }
    
    public void Pause()
    {
        _viewModel?.Pause();
    }
    
    public void Resume()
    {
        _viewModel?.Resume();
        _renderTimer.Start();
    }
    
    public void SaveGame()
    {
        _viewModel?.SaveGame();
    }
}

