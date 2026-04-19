using Godot;
using System.Collections.Generic;

public partial class SoundManager : Node
{
	public static SoundManager Instance;
	
	//Declaration of the Audio Stream Players
	private AudioStreamPlayer _coin;
	private AudioStreamPlayer _enemy;
	private AudioStreamPlayer _checkpoint;
	private AudioStreamPlayer _jump;
	private AudioStreamPlayer _menuHover;
	private AudioStreamPlayer _button;
	private AudioStreamPlayer _settingsMusic;
	private AudioStreamPlayer _startScreenMusic;
	private AudioStreamPlayer _music;
	private AudioStreamPlayer _gameOverMusic;
	
	//Declaration of a List of Audio Stream Players, which will later be used to switch the music
	private List<AudioStreamPlayer> _musicPlayers;
	
	//Getters for the Music Audio Stream Players
	public AudioStreamPlayer GameMusic => _music;
	public AudioStreamPlayer SettingsMusic => _settingsMusic;
	public AudioStreamPlayer StartScreenMusic => _startScreenMusic;
	public AudioStreamPlayer GameOverMusic => _gameOverMusic;

	public override void _Ready()
	{
		Instance = this;
		
		//initialising the Audiostream Players
		_coin = GetNode<AudioStreamPlayer>("CoinSound");
		_enemy = GetNode<AudioStreamPlayer>("EnemyDeathSound");
		_checkpoint = GetNode<AudioStreamPlayer>("CheckPointSound");
		_jump = GetNode<AudioStreamPlayer>("JumpSound");
		_music = GetNode<AudioStreamPlayer>("Music/BackGroundMusic");
		_settingsMusic = GetNode<AudioStreamPlayer>("Music/SettingsMusic");
		_startScreenMusic = GetNode<AudioStreamPlayer>("Music/StartScreenMusic");
		_gameOverMusic = GetNode<AudioStreamPlayer>("Music/GameOverMusic");
		_menuHover = GetNode<AudioStreamPlayer>("MenuFX/HoverSound");
		_button = GetNode<AudioStreamPlayer>("MenuFX/ButtonSound");
		var musicParent = GetNode<Node>("Music");
		
		_musicPlayers = new List<AudioStreamPlayer>();
		//Iterating over every object in the "Music" Node and adding them to our list of MusicPlayers
		foreach (Node child in musicParent.GetChildren())
		{
			if (child is AudioStreamPlayer player)
			{
				_musicPlayers.Add(player);
			}
		}
	}
	
	//Ingame Objects
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
	
	//Menu Sounds
	public void PlayMenuHover()
	{
		_menuHover.Play();
	}
	
	public void PlayButton()
	{
		_button.Play();
	}
	
	//Music  start and stop functions, currently not really needed as we have switch music now
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
	
	//Switch Music Function Stops every music and switches to the Audio Stream Player, which is chosen as the parameter
	public void SwitchMusic(AudioStreamPlayer target)
	{
		foreach (var music in _musicPlayers)
		{
			music.Stop();
		}

		target.Play();
	}
}
