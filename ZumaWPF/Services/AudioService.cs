using System;
using System.IO;
using System.Windows.Media;

namespace ZumaWPF.Services;

public class AudioService
{
    private MediaPlayer? _backgroundMusic;
    private MediaPlayer? _shootSound;
    private MediaPlayer? _hitSound;
    private MediaPlayer? _comboSound;
    private MediaPlayer? _resultSound;
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
            var musicPath = Path.Combine("Assets", "Sounds", "music.mp3");
            if (File.Exists(musicPath))
            {
                _backgroundMusic.Open(new Uri(Path.GetFullPath(musicPath), UriKind.Absolute));
                _backgroundMusic.MediaEnded += (s, e) =>
                {
                    _backgroundMusic?.Stop();
                    _backgroundMusic?.Play();
                };
            }
            
            _shootSound = new MediaPlayer();
            var shootPath = Path.Combine("Assets", "Sounds", "shot.mp3");
            if (File.Exists(shootPath))
            {
                _shootSound.Open(new Uri(Path.GetFullPath(shootPath), UriKind.Absolute));
            }
            
            _hitSound = new MediaPlayer();
            var hitPath = Path.Combine("Assets", "Sounds", "boom.mp3");
            if (File.Exists(hitPath))
            {
                _hitSound.Open(new Uri(Path.GetFullPath(hitPath), UriKind.Absolute));
            }
            
            _comboSound = new MediaPlayer();
            var comboPath = Path.Combine("Assets", "Sounds", "like.mp3");
            if (File.Exists(comboPath))
            {
                _comboSound.Open(new Uri(Path.GetFullPath(comboPath), UriKind.Absolute));
            }
            _resultSound = new MediaPlayer();
            if (File.Exists(comboPath))
            {
                _resultSound.Open(new Uri(Path.GetFullPath(comboPath), UriKind.Absolute));
            }
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
                _shootSound.Stop();
                _shootSound.Position = TimeSpan.Zero;
                _shootSound.Volume = 0.5;
                _shootSound.Play();
            }
            catch { }
        }
    }
    
    public void PlayHitSound()
    {
        if (_soundEnabled && _hitSound != null)
        {
            try
            {
                _hitSound.Stop();
                _hitSound.Position = TimeSpan.Zero;
                _hitSound.Volume = 0.4;
                _hitSound.Play();
            }
            catch { }
        }
    }
    
    public void PlayComboSound()
    {
        if (_soundEnabled && _comboSound != null)
        {
            try
            {
                _comboSound.Stop();
                _comboSound.Position = TimeSpan.Zero;
                _comboSound.Volume = 0.6;
                _comboSound.Play();
            }
            catch { }
        }
    }

    public void PlayResultSound()
    {
        if (_soundEnabled && _resultSound != null)
        {
            try
            {
                _resultSound.Stop();
                _resultSound.Position = TimeSpan.Zero;
                _resultSound.Volume = 0.7;
                _resultSound.Play();
            }
            catch { }
        }
    }
    
    public void Dispose()
    {
        _backgroundMusic?.Close();
        _shootSound?.Close();
        _hitSound?.Close();
        _comboSound?.Close();
        _resultSound?.Close();
    }
}
