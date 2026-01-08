using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ZumaWPF.Models;
using ZumaWPF.Services;
using ZumaWPF.ViewModels;
using ZumaWPF.Views;

namespace ZumaWPF;

public partial class MainWindow : Window
{
    private readonly ConfigService _configService;
    private readonly UserService _userService;
    private readonly SaveService _saveService;
    private readonly AudioService _audioService;
    private readonly GameService _gameService;
    private readonly ChainController _chainController;

    private GameViewModel? _gameViewModel;

    private MainMenuView? _mainMenuView;
    private LoginView? _loginView;
    private LevelSelectionView? _levelSelectionView;
    private LoadingView? _loadingView;
    private GameScreenView? _gameScreenView;
    private PauseView? _pauseView;
    private GameOverView? _gameOverView;
    private VictoryView? _victoryView;
    private HighScoresView? _highScoresView;
    private ManualView? _manualView;

    public MainWindow()
    {
        InitializeComponent();

        _configService = new ConfigService();
        _userService = new UserService();
        _saveService = new SaveService();
        _audioService = new AudioService();
        _audioService.Initialize();

        _gameService = new GameService(_configService);
        _chainController = new ChainController(_gameService, _configService);

        ShowLoginView(_saveService.HasSaveGame());
    }

    private void ShowLoginView(bool hasSaveGame)
    {
        _loginView = new LoginView();
        _loginView.Login += OnLogin;
        ContentArea.Content = _loginView;
    }

    private void OnLogin(string username)
    {
        if (!_userService.Login(username))
        {
            MessageBox.Show("Ошибка авторизации");
            return;
        }

        ShowMainMenu();
    }

    private void ShowMainMenu()
    {
        _mainMenuView = new MainMenuView();
        _mainMenuView.NewGame += OnNewGame;
        _mainMenuView.Continue += OnContinue;
        _mainMenuView.SelectLevel += OnSelectLevel;
        _mainMenuView.HighScores += OnHighScores;
        _mainMenuView.Manual += OnManual;
        _mainMenuView.Exit += OnExit;
        _mainMenuView.UpdateContinueButton(_saveService.HasSaveGame());

        ContentArea.Content = _mainMenuView;
    }

    private void OnNewGame() => ShowLevelSelection();

    private void OnContinue()
    {
        var saveData = _saveService.LoadGame();
        if (saveData == null) return;

        var level = _gameService.CreateLevels().FirstOrDefault(l => l.Id == saveData.Level);
        if (level == null) return;

        ShowLoadingView(() =>
        {
            _gameViewModel = new GameViewModel(
                _gameService, _chainController, _configService,
                _audioService, _saveService, _userService);

            _gameViewModel.LoadSavedGame(saveData, level);
            ShowGameScreen();
        });
    }

    private void OnSelectLevel() => ShowLevelSelection();

    private void ShowLevelSelection()
    {
        _levelSelectionView = new LevelSelectionView();
        _levelSelectionView.LoadLevels(_gameService.CreateLevels());
        _levelSelectionView.LevelSelected += OnLevelSelected;
        _levelSelectionView.Back += ShowMainMenu;

        ContentArea.Content = _levelSelectionView;
    }

    private void OnLevelSelected(Level level)
    {
        ShowLoadingView(() =>
        {
            _gameViewModel = new GameViewModel(
                _gameService, _chainController, _configService,
                _audioService, _saveService, _userService);

            _gameViewModel.StartLevel(level.Id);
            ShowGameScreen();
        });
    }

    private void ShowLoadingView(Action onComplete)
    {
        _loadingView = new LoadingView();
        ContentArea.Content = _loadingView;

        var timer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };

        timer.Tick += (_, _) =>
        {
            timer.Stop();
            onComplete();
        };

        timer.Start();
    }

    private void ShowGameScreen()
    {
        if (_gameViewModel == null) return;

        _gameScreenView = new GameScreenView();
        _gameScreenView.SetViewModel(_gameViewModel);
        _gameScreenView.GameOver += OnGameOver;
        _gameScreenView.Victory += OnVictory;
        _gameScreenView.RequestPause += OnPauseRequested;

        ContentArea.Content = _gameScreenView;
        KeyDown += MainWindow_KeyDown;
        _gameScreenView.Focus();
    }

    private void MainWindow_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && ContentArea.Content == _gameScreenView)
        {
            OnPauseRequested();
            e.Handled = true;
        }
    }

    private void OnPauseRequested()
    {
        if (_gameScreenView == null || _gameViewModel == null) return;
        if (ContentArea.Content is Grid) return;

        _gameScreenView.Pause();

        _pauseView = new PauseView();
        _pauseView.Resume += OnResume;
        _pauseView.Save += OnSaveGame;
        _pauseView.MainMenu += OnMainMenuFromPause;

        var overlay = new Grid();

        var currentContent = ContentArea.Content as UIElement;
        if (currentContent == null) return;

        ContentArea.Content = null;

        overlay.Children.Add(currentContent);
        overlay.Children.Add(_pauseView);

        ContentArea.Content = overlay;
    }

    private void OnResume()
    {
        if (ContentArea.Content is Grid grid &&
            grid.Children[0] is GameScreenView gameScreen)
        {
            grid.Children.Clear();
            ContentArea.Content = gameScreen;
            gameScreen.Resume();
        }
    }

    private void OnSaveGame()
    {
        _gameViewModel?.SaveGame();
        MessageBox.Show("Игра сохранена");
    }

    private void OnMainMenuFromPause()
    {
        _gameScreenView?.Stop();
        _gameViewModel?.SaveGame();
        _gameViewModel = null;

        KeyDown -= MainWindow_KeyDown;
        ShowMainMenu();
    }

    private void OnGameOver()
    {
        _gameScreenView?.Stop();

        _gameOverView = new GameOverView();
        _gameOverView.SetScore(_gameViewModel!.GameState.Score);
        _gameOverView.Retry += OnRetry;
        _gameOverView.MainMenu += OnMainMenuFromGameOver;

        ContentArea.Content = _gameOverView;
        KeyDown -= MainWindow_KeyDown;
    }

    private void OnVictory()
    {
        _gameScreenView?.Stop();

        _victoryView = new VictoryView();
        _victoryView.SetScore(_gameViewModel!.GameState.Score);
        _victoryView.NextLevel += OnNextLevel;
        _victoryView.MainMenu += OnMainMenuFromGameOver;

        ContentArea.Content = _victoryView;
        KeyDown -= MainWindow_KeyDown;
    }

    private void OnRetry()
    {
        var level = _gameViewModel!.GameState.CurrentLevel;
        _gameViewModel = new GameViewModel(
            _gameService, _chainController, _configService,
            _audioService, _saveService, _userService);

        _gameViewModel.StartLevel(level);
        ShowGameScreen();
    }

    private void OnNextLevel()
    {
        int next = _gameViewModel!.GameState.CurrentLevel + 1;
        if (_gameService.CreateLevels().Any(l => l.Id == next))
        {
            _gameViewModel = new GameViewModel(
                _gameService, _chainController, _configService,
                _audioService, _saveService, _userService);

            _gameViewModel.StartLevel(next);
            ShowGameScreen();
        }
        else
        {
            ShowMainMenu();
        }
    }

    private void OnMainMenuFromGameOver()
    {
        _gameViewModel?.Stop();
        _gameViewModel = null;
        KeyDown -= MainWindow_KeyDown;
        ShowMainMenu();
    }

    private void OnHighScores()
    {
        _highScoresView = new HighScoresView();
        _highScoresView.LoadScores(_userService.GetHighScores(10));
        _highScoresView.Back += ShowMainMenu;

        ContentArea.Content = _highScoresView;
    }

    private void OnManual()
    {
        _manualView = new ManualView();
        _manualView.Back += ShowMainMenu;
        ContentArea.Content = _manualView;
    }

    private void OnExit() => Application.Current.Shutdown();

    protected override void OnClosed(EventArgs e)
    {
        _audioService.Dispose();
        base.OnClosed(e);
    }
}
