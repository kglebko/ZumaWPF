using System.Windows.Controls;
using System.Windows;
using ZumaWPF.Services;

namespace ZumaWPF.Views;

public partial class SettingsView : UserControl
{
    private readonly AudioService _audioService;
    
    public event System.Action? Back;
    
    public SettingsView(AudioService audioService)
    {
        InitializeComponent();
        _audioService = audioService;
        
        // Устанавливаем текущие значения
        MusicCheckBox.IsChecked = _audioService?.MusicEnabled ?? true;
        SoundCheckBox.IsChecked = _audioService?.SoundEnabled ?? true;
    }
    
    private void MusicCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        if (_audioService == null) return;
        _audioService.MusicEnabled = true;
    }
    
    private void MusicCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        if (_audioService == null) return;
        _audioService.MusicEnabled = false;
    }
    
    private void SoundCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        if (_audioService == null) return;
        _audioService.SoundEnabled = true;
    }
    
    private void SoundCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        if (_audioService == null) return;
        _audioService.SoundEnabled = false;
    }
    
    private void Back_Click(object sender, RoutedEventArgs e)
    {
        Back?.Invoke();
    }
}

