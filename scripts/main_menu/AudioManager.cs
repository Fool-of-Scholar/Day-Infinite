using Godot;
using System;

public partial class AudioManager : Node
{
    private AudioStreamPlayer _bgmAudioPlayer;


    public override void _Ready()
    {
        // Create the audio player programmatically
        _bgmAudioPlayer = new AudioStreamPlayer();
        AddChild(_bgmAudioPlayer);

        // Load your music file from the res:// folder
        // Replace with your actual audio file path
        AudioStream music = GD.Load<AudioStream>("res://assets/music/Dread Pitt - Pyro [NCS Release].mp3");
        _bgmAudioPlayer.Stream = music;
        _bgmAudioPlayer.ProcessMode = ProcessModeEnum.Always;
    }

    public void PlayMusic()
    {
        _bgmAudioPlayer.SetProcess(true);
        // Only play if it isn't already playing
        if (!_bgmAudioPlayer.Playing)
        {
            _bgmAudioPlayer.Play();

        }
        
    }

    public void SetVolumeMusic(float bgmVolume)
    {
        _bgmAudioPlayer.VolumeDb = bgmVolume;
    }
}
