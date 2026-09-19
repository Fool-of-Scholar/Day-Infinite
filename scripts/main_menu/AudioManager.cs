using Godot;
using System;

public partial class AudioManager : Node
{
    private AudioStreamPlayer _audioPlayer;

    public override void _Ready()
    {
        // Create the audio player programmatically
        _audioPlayer = new AudioStreamPlayer();
        AddChild(_audioPlayer);

        // Load your music file from the res:// folder
        // Replace with your actual audio file path
        AudioStream music = GD.Load<AudioStream>("res://assets/music/Dread Pitt - Pyro [NCS Release].mp3");
        _audioPlayer.Stream = music;
    }

    public void PlayMusic()
    {
        // Only play if it isn't already playing
        if (!_audioPlayer.Playing)
        {
            _audioPlayer.Play();
        }
    }
}
