using System;
using System.IO;
using System.Media;
using System.Windows.Media;

namespace ZumaWPF.Services;

public class AudioService
{
    private MediaPlayer? _backgroundMusic;
    private SoundPlayer? _shootSound;
    private SoundPlayer? _destroySound;
    private bool _musicEnabled = true;
    private bool _soundEnabled = true;
    
    public bool MusicEnabled
    {
        get => _musicEnabled;
        set
        {
            _musicEnabled = value;
            if (_backgroundMusic != null)
            {
                if (value)
                    _backgroundMusic.Play();
                else
                    _backgroundMusic.Pause();
            }
        }
    }
    
    public bool SoundEnabled
    {
        get => _soundEnabled;
        set => _soundEnabled = value;
    }
    
    public void Initialize()
    {
        try
        {
            _backgroundMusic = new MediaPlayer();
            var musicPath = Path.Combine("Resources", "Music", "background.wav");
            if (File.Exists(musicPath))
            {
                _backgroundMusic.Open(new Uri(Path.GetFullPath(musicPath), UriKind.Absolute));
                _backgroundMusic.MediaEnded += (s, e) =>
                {
                    _backgroundMusic?.Stop();
                    _backgroundMusic?.Play();
                };
            }
            
            var shootPath = Path.Combine("Resources", "Sounds", "shoot.wav");
            if (File.Exists(shootPath))
                _shootSound = new SoundPlayer(shootPath);
            
            var destroyPath = Path.Combine("Resources", "Sounds", "destroy.wav");
            if (File.Exists(destroyPath))
                _destroySound = new SoundPlayer(destroyPath);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Audio initialization error: {ex.Message}");
        }
    }
    
    public void PlayBackgroundMusic()
    {
        if (_musicEnabled && _backgroundMusic != null)
        {
            _backgroundMusic.Volume = 0.3;
            _backgroundMusic.Play();
        }
    }
    
    public void StopBackgroundMusic()
    {
        _backgroundMusic?.Stop();
    }
    
    public void PlayShootSound()
    {
        if (_soundEnabled && _shootSound != null)
        {
            try
            {
                _shootSound.Play();
            }
            catch { }
        }
    }
    
    public void PlayDestroySound()
    {
        if (_soundEnabled && _destroySound != null)
        {
            try
            {
                _destroySound.Play();
            }
            catch { }
        }
    }
    
    public void Dispose()
    {
        _backgroundMusic?.Close();
        _shootSound?.Dispose();
        _destroySound?.Dispose();
    }
}

