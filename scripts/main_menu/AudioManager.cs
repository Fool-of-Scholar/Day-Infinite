using Godot;
using System;

public partial class AudioManager : Node
{
    public static AudioManager Instance { get; private set; }

    private AudioStreamPlayer _bgmAudioPlayer;

    [Export] public AudioStream DefaultPickupSound { get; set; }

    
    private const int PickupPoolSize = 8;
    private readonly AudioStreamPlayer[] _pickupPlayers = new AudioStreamPlayer[PickupPoolSize];
    private int _pickupPoolIndex = 0;

    // Experience Gem / Battery Pitch Ramping
    private ulong _lastExpPickupTimeMs = 0;
    private int _expComboCount = 0;
    private const float ExpBasePitch = 0.95f;
    private const float ExpPitchStep = 0.04f;
    private const float ExpMaxPitch = 2.0f;
    private const ulong ExpComboResetMs = 400; // Reset streak if no gem collected for 0.4s
    private const ulong ExpMinIntervalMs = 30; // Min 30ms between sounds to avoid clipping
    private ulong _lastExpSoundTimeMs = 0;

    // Scrap Pickup Pitch Ramping
    private ulong _lastScrapPickupTimeMs = 0;
    private int _scrapComboCount = 0;
    private const float ScrapBasePitch = 1.05f;
    private const float ScrapPitchStep = 0.035f;
    private const float ScrapMaxPitch = 1.8f;
    private const ulong ScrapComboResetMs = 350;
    private const ulong ScrapMinIntervalMs = 30;
    private ulong _lastScrapSoundTimeMs = 0;

    // Health Battery Pickup Pitch Ramping (Distinct, uplifting tone)
    private ulong _lastHealthPickupTimeMs = 0;
    private int _healthComboCount = 0;
    private const float HealthBasePitch = 1.25f;
    private const float HealthPitchStep = 0.06f;
    private const float HealthMaxPitch = 2.0f;
    private const ulong HealthComboResetMs = 500;
    private const ulong HealthMinIntervalMs = 30;
    private ulong _lastHealthSoundTimeMs = 0;

    public override void _EnterTree()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public override void _Ready()
    {
        // 1. Background Music Player
        _bgmAudioPlayer = new AudioStreamPlayer
        {
            Name = "BGMAudioPlayer",
            ProcessMode = ProcessModeEnum.Always
        };
        AddChild(_bgmAudioPlayer);

        AudioStream music = GD.Load<AudioStream>("res://assets/Sounds&Music/BGMusic Main Menu Final.mp3");
        _bgmAudioPlayer.Stream = music;

        // 2. Load Default Pickup Sound
        if (DefaultPickupSound == null)
        {
            DefaultPickupSound = GD.Load<AudioStream>("res://assets/Sounds&Music/Battery PickUp Sfx Export 1.wav");
        }

        // 3. Create AudioStreamPlayer Pool for Pickup SFX
        for (int i = 0; i < PickupPoolSize; i++)
        {
            var player = new AudioStreamPlayer
            {
                Name = $"PickupPlayer_{i}",
                Bus = "SFX",
                ProcessMode = ProcessModeEnum.Always
            };
            AddChild(player);
            _pickupPlayers[i] = player;
        }
    }

    // --- Static Public API for Pickups ---

    public static void PlayExperiencePickup(AudioStream customStream = null, float volumeDb = -3.0f)
    {
        Instance?.PlayExperiencePickupInternal(customStream, volumeDb);
    }

    public static void PlayScrapPickup(AudioStream customStream = null, float volumeDb = -3.0f)
    {
        Instance?.PlayScrapPickupInternal(customStream, volumeDb);
    }

    public static void PlayHealthPickup(AudioStream customStream = null, float volumeDb = -1.0f)
    {
        Instance?.PlayHealthPickupInternal(customStream, volumeDb);
    }

    // --- Internal Implementation ---

    public void PlayExperiencePickupInternal(AudioStream customStream = null, float volumeDb = -3.0f)
    {
        ulong now = Time.GetTicksMsec();

        // Check if combo reset time expired
        if (now - _lastExpPickupTimeMs > ExpComboResetMs)
        {
            _expComboCount = 0;
        }
        _lastExpPickupTimeMs = now;
        _expComboCount++;

        // Throttle playback rate to prevent speaker clipping on simultaneous pickups
        if (now - _lastExpSoundTimeMs < ExpMinIntervalMs)
        {
            return;
        }
        _lastExpSoundTimeMs = now;

        // Smooth pitch escalation (Vampire Survivors musical arpeggio feel)
        float pitch = Mathf.Min(ExpBasePitch + (_expComboCount * ExpPitchStep), ExpMaxPitch);

        PlayPooledSound(customStream ?? DefaultPickupSound, pitch, volumeDb);
    }

    public void PlayScrapPickupInternal(AudioStream customStream = null, float volumeDb = -3.0f)
    {
        ulong now = Time.GetTicksMsec();

        if (now - _lastScrapPickupTimeMs > ScrapComboResetMs)
        {
            _scrapComboCount = 0;
        }
        _lastScrapPickupTimeMs = now;
        _scrapComboCount++;

        if (now - _lastScrapSoundTimeMs < ScrapMinIntervalMs)
        {
            return;
        }
        _lastScrapSoundTimeMs = now;

        float pitch = Mathf.Min(ScrapBasePitch + (_scrapComboCount * ScrapPitchStep), ScrapMaxPitch);

        PlayPooledSound(customStream ?? DefaultPickupSound, pitch, volumeDb);
    }

    public void PlayHealthPickupInternal(AudioStream customStream = null, float volumeDb = -1.0f)
    {
        ulong now = Time.GetTicksMsec();

        if (now - _lastHealthPickupTimeMs > HealthComboResetMs)
        {
            _healthComboCount = 0;
        }
        _lastHealthPickupTimeMs = now;
        _healthComboCount++;

        if (now - _lastHealthSoundTimeMs < HealthMinIntervalMs)
        {
            return;
        }
        _lastHealthSoundTimeMs = now;

        float pitch = Mathf.Min(HealthBasePitch + (_healthComboCount * HealthPitchStep), HealthMaxPitch);

        PlayPooledSound(customStream ?? DefaultPickupSound, pitch, volumeDb);
    }

    private void PlayPooledSound(AudioStream stream, float pitch, float volumeDb)
    {
        if (stream == null) return;

        var player = _pickupPlayers[_pickupPoolIndex];
        _pickupPoolIndex = (_pickupPoolIndex + 1) % PickupPoolSize;

        player.Stream = stream;
        player.PitchScale = pitch;
        player.VolumeDb = volumeDb;
        player.Play();
    }

    // --- Music Controls ---

    public void PlayMusic()
    {
        if (_bgmAudioPlayer == null) return;
        _bgmAudioPlayer.SetProcess(true);
        if (!_bgmAudioPlayer.Playing)
        {
            _bgmAudioPlayer.Play();
        }
    }

    public void StopMusic()
    {
        if (_bgmAudioPlayer != null && _bgmAudioPlayer.Playing)
        {
            _bgmAudioPlayer.Stop();
        }
    }

    public void SetVolumeMusic(float bgmVolume)
    {
        if (_bgmAudioPlayer != null)
        {
            _bgmAudioPlayer.VolumeDb = bgmVolume;
        }
    }

    public void SetVolumeSFX(float sfxVolume)
    {
        int busIdx = AudioServer.GetBusIndex("SFX");
        if (busIdx >= 0)
        {
            AudioServer.SetBusVolumeDb(busIdx, sfxVolume);
        }
    }
}
