using Godot;
using System;

public partial class AudioManager : Node
{
    public AudioStreamPlayer _bgmAudioPlayer;
    AudioStream music;
    public string _musicSlot = "res://assets/main_menu/sounds/BGMusic Main Menu Final.mp3";

    public override void _Ready()
    {
        // Create the audio player programmatically
        _bgmAudioPlayer = new AudioStreamPlayer();
        AddChild(_bgmAudioPlayer);

        // Load your music file from the res:// folder
        // Replace with your actual audio file path
        music = GD.Load<AudioStream>(_musicSlot);
        _bgmAudioPlayer.ProcessMode = ProcessModeEnum.Always;
        PlayMusic();
    }

    public void PlayMusic()
    {
        _bgmAudioPlayer.Stream = music;
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
