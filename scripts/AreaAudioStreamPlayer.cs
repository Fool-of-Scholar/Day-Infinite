using Godot;
using System;

[GlobalClass]
public partial class AreaAudioStreamPlayer : Node
{
    [ExportGroup("Audio Streams")]
    [Export] public AudioStream Phase1 { get; set; }
    [Export] public AudioStream Phase2 { get; set; }
    [Export] public AudioStream Phase3 { get; set; }

    [ExportGroup("Phase Timings (Seconds)")]
    [Export] public float Phase2Time { get; set; } = 240f; // 4 minutes
    [Export] public float Phase3Time { get; set; } = 600f; // 10 minutes (Boss Phase)

    [ExportGroup("Configuration")]
    [Export] public int TargetAreaId { get; set; } = 1;
    [Export] public AudioStreamPlayer MusicPlayer { get; set; }
    [Export] public bool AutoTrackTime { get; set; } = true;

    private float _elapsedTime = 0f;
    private int _currentPhase = -1;

    public override void _Ready()
    {
        // Stop any playing Main Menu BGM from the autoloaded AudioManager
        StopMainMenuBgm();

        ResolveMusicPlayer();

        // Start Phase 1 music immediately on load and loop it
        if (AutoTrackTime && Phase1 != null)
        {
            _currentPhase = 1;
            PlayPhase(1);
        }
    }

    public override void _Process(double delta)
    {
        if (!AutoTrackTime) return;

        _elapsedTime += (float)delta;

        int targetPhase = 1;
        if (_elapsedTime >= Phase3Time)
            targetPhase = 3;
        else if (_elapsedTime >= Phase2Time)
            targetPhase = 2;
        else
            targetPhase = 1;

        if (_currentPhase != targetPhase)
        {
            _currentPhase = targetPhase;
            PlayPhase(_currentPhase);
        }
    }

    /// <summary>
    /// Can also be called externally from game managers or spawners if desired.
    /// </summary>
    public void CheckAndUpdate(int areaId, float runTimeSeconds)
    {
        if (areaId != TargetAreaId) return;

        _elapsedTime = runTimeSeconds;

        int targetPhase = 1;
        if (runTimeSeconds >= Phase3Time)
            targetPhase = 3;
        else if (runTimeSeconds >= Phase2Time)
            targetPhase = 2;
        else
            targetPhase = 1;

        if (_currentPhase != targetPhase)
        {
            _currentPhase = targetPhase;
            PlayPhase(_currentPhase);
        }
    }

    public void PlayPhase(int phase)
    {
        ResolveMusicPlayer();
        if (MusicPlayer == null) return;

        AudioStream streamToPlay = phase switch
        {
            1 => Phase1,
            2 => Phase2,
            3 => Phase3,
            _ => null
        };

        if (streamToPlay == null)
        {
            GD.PrintErr($"[AreaAudio] No audio stream assigned for Phase {phase} (Area {TargetAreaId})");
            return;
        }

        // Enable loop on the stream
        EnableStreamLoop(streamToPlay);

        if (MusicPlayer.Stream != streamToPlay || !MusicPlayer.Playing)
        {
            MusicPlayer.Stream = streamToPlay;
            MusicPlayer.Play();
            GD.Print($"[AreaAudio] Now playing & looping Phase {phase} track for Area {TargetAreaId}");
        }
    }

    private void EnableStreamLoop(AudioStream stream)
    {
        if (stream is AudioStreamMP3 mp3)
        {
            mp3.Loop = true;
        }
        else if (stream is AudioStreamWav wav)
        {
            wav.LoopMode = AudioStreamWav.LoopModeEnum.Forward;
        }
        else if (stream is AudioStreamOggVorbis ogg)
        {
            ogg.Loop = true;
        }
    }

    private void OnMusicFinished()
    {
        // Fail-safe looping if native stream loop isn't supported by the importer
        if (MusicPlayer != null && MusicPlayer.Stream != null)
        {
            MusicPlayer.Play();
        }
    }

    private void StopMainMenuBgm()
    {
        var globalAudio = GetNodeOrNull<AudioManager>("/root/AudioManager");
        if (globalAudio != null)
        {
            globalAudio.StopMusic();
            GD.Print("[AreaAudio] Stopped Main Menu BGM.");
        }
    }

    private void ResolveMusicPlayer()
    {
        if (MusicPlayer != null && IsInstanceValid(MusicPlayer)) return;

        // Auto-search for existing AudioStreamPlayer in parent, children, or siblings
        MusicPlayer = GetNodeOrNull<AudioStreamPlayer>("MusicPlayer")
                   ?? GetNodeOrNull<AudioStreamPlayer>("../AudioStreamPlayer")
                   ?? GetParent()?.GetNodeOrNull<AudioStreamPlayer>("AudioStreamPlayer");

        // Fallback: create one automatically as child
        if (MusicPlayer == null)
        {
            MusicPlayer = new AudioStreamPlayer();
            MusicPlayer.Name = "MusicPlayer";
            AddChild(MusicPlayer);
            GD.Print($"[AreaAudio] Auto-created MusicPlayer child on {Name}");
        }

        MusicPlayer.Finished -= OnMusicFinished;
        MusicPlayer.Finished += OnMusicFinished;
    }
}
