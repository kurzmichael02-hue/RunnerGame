using Godot;
using System.Collections.Generic;

public partial class SoundManager : Node
{
	public static SoundManager Instance;

	private AudioStreamPlayer _coin;
	private AudioStreamPlayer _enemy;
	private AudioStreamPlayer _checkpoint;
	private AudioStreamPlayer _music;
	private AudioStreamPlayer _jump;
	private AudioStreamPlayer _settingsMusic;
	private AudioStreamPlayer _startScreenMusic;
	
	private List<AudioStreamPlayer> _musicPlayers;
	
	public AudioStreamPlayer GameMusic => _music;
	public AudioStreamPlayer SettingsMusic => _settingsMusic;
	public AudioStreamPlayer StartScreenMusic => _startScreenMusic;

	public override void _Ready()
	{
		Instance = this;

		_coin = GetNode<AudioStreamPlayer>("CoinSound");
		_enemy = GetNode<AudioStreamPlayer>("EnemyDeathSound");
		_checkpoint = GetNode<AudioStreamPlayer>("CheckPointSound");
		_jump = GetNode<AudioStreamPlayer>("JumpSound");
		_music = GetNode<AudioStreamPlayer>("Music/BackGroundMusic");
		_settingsMusic = GetNode<AudioStreamPlayer>("Music/SettingsMusic");
		_startScreenMusic = GetNode<AudioStreamPlayer>("Music/StartScreenMusic");
		
		var musicParent = GetNode<Node>("Music");

		_musicPlayers = new List<AudioStreamPlayer>();

		foreach (Node child in musicParent.GetChildren())
		{
			if (child is AudioStreamPlayer player)
			{
				_musicPlayers.Add(player);
			}
		}
	}

	public void PlayCoin()
	{
		_coin.Play();
	}

	public void PlayJump()
	{
		_jump.Play();
	}

	public void PlayEnemyDeath()
	{
		_enemy.Play();
	}

	public void PlayCheckpoint()
	{
		_checkpoint.Play();
	}

	public void PlayMusic()
	{
		if (!_music.Playing)
			_music.Play();
	}

	public void StopMusic()
	{
		_music.Stop();
	}
	
	public void PlaySettingsMusic()
	{
		if (!_settingsMusic.Playing)
			_settingsMusic.Play();
	}
	
	public void StopSettingsMusic()
	{
		_settingsMusic.Stop();
	}
	
	public void SwitchMusic(AudioStreamPlayer target)
	{
		foreach (var music in _musicPlayers)
		{
			music.Stop();
		}

		target.Play();
	}
}
