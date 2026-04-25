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
	private AudioStreamPlayer _swordAttack;
	private AudioStreamPlayer _noSwordLeft;
	private AudioStreamPlayer _settingsMusic;
	private AudioStreamPlayer _startScreenMusic;
	private AudioStreamPlayer _music;
	private AudioStreamPlayer _gameOverMusic;
	private AudioStreamPlayer _starMusic;
	private AudioStreamPlayer _playerDeath1;
	private AudioStreamPlayer _playerDeath2;
	private AudioStreamPlayer _playerDeath3;
	private RandomNumberGenerator _rng = new RandomNumberGenerator();
	
	//Declaration of a List of Audio Stream Players, which will later be used to switch the music
	private List<AudioStreamPlayer> _musicPlayers;
	private List<AudioStreamPlayer> _fxPlayers;
	
	//Getters for the Audio Player Lists
	public List<AudioStreamPlayer> MusicPlayers => _musicPlayers;
	public List<AudioStreamPlayer> FxPlayers => _fxPlayers;
	
	//Getters for the Music Audio Stream Players
	public AudioStreamPlayer GameMusic => _music;
	public AudioStreamPlayer SettingsMusic => _settingsMusic;
	public AudioStreamPlayer StartScreenMusic => _startScreenMusic;
	public AudioStreamPlayer GameOverMusic => _gameOverMusic;
	public AudioStreamPlayer StarMusic => _starMusic;

	public override void _Ready()
	{
		Instance = this;
		
		//initialising the Audiostream Players
		
		// ====================
		// Enviroment Sounds
		// ====================
		_coin = GetNode<AudioStreamPlayer>("GameFX/CoinSound");
		_enemy = GetNode<AudioStreamPlayer>("GameFX/EnemyDeathSound");
		_checkpoint = GetNode<AudioStreamPlayer>("GameFX/CheckPointSound");


		// ====================
		// Player Sounds
		// ====================
		_swordAttack = GetNode<AudioStreamPlayer>("GameFX/SwordAttackSound");
		_noSwordLeft = GetNode<AudioStreamPlayer>("GameFX/NoSwordLeftSound");
		_jump = GetNode<AudioStreamPlayer>("GameFX/JumpSound");

		_playerDeath1 = GetNode<AudioStreamPlayer>("GameFX/PlayerDeathSound1");
		_playerDeath2 = GetNode<AudioStreamPlayer>("GameFX/PlayerDeathSound2");
		_playerDeath3 = GetNode<AudioStreamPlayer>("GameFX/PlayerDeathSound3");


		// ====================
		// Music
		// ====================
		_music = GetNode<AudioStreamPlayer>("Music/BackGroundMusic");
		_settingsMusic = GetNode<AudioStreamPlayer>("Music/SettingsMusic");
		_startScreenMusic = GetNode<AudioStreamPlayer>("Music/StartScreenMusic");
		_gameOverMusic = GetNode<AudioStreamPlayer>("Music/GameOverMusic");
		_starMusic = GetNode<AudioStreamPlayer>("Music/StarMusic");


		// ====================
		// Menu Sounds
		// ====================
		_menuHover = GetNode<AudioStreamPlayer>("MenuFX/HoverSound");
		_button = GetNode<AudioStreamPlayer>("MenuFX/ButtonSound");
		
		//Parent Paths to iterate
		var musicParent = GetNode<Node>("Music");
		var gameFxParent = GetNode<Node>("GameFX");
		var menuFxParent = GetNode<Node>("MenuFX");
		
		_musicPlayers = new List<AudioStreamPlayer>();
		_fxPlayers = new List<AudioStreamPlayer>();
		
		_rng.Randomize();
		
		
		//Iterating over every object in the "Music" Node and adding them to our list of MusicPlayers
		foreach (Node child in musicParent.GetChildren())
		{
			if (child is AudioStreamPlayer player)
			{
				_musicPlayers.Add(player);
			}
		}
		foreach (Node parent in new[] { gameFxParent, menuFxParent })
		{
			foreach (Node child in parent.GetChildren())
			{
				if (child is AudioStreamPlayer player)
				{
					_fxPlayers.Add(player);
				}
			}
		}

		// Read master volume from settings.cfg straight away – otherwise the main-menu
		// music starts at full volume and gets quieter the moment the player opens
		// settings, which is the bug bartolmay flagged.
		LoadMasterVolumeFromConfig();
	}

	private void LoadMasterVolumeFromConfig()
	{
		var config = new ConfigFile();
		if (config.Load("user://settings.cfg") != Error.Ok) return;
		if (!config.HasSectionKey("audio", "volume")) return;

		double value = (double)config.GetValue("audio", "volume", 100.0);
		float linear = (float)(value / 100.0);
		float db = linear <= 0.001f ? -80f : Mathf.LinearToDb(linear);
		int bus = AudioServer.GetBusIndex("Master");
		AudioServer.SetBusVolumeDb(bus, db);
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
	
	public void PlaySwordAttack()
	{
		_swordAttack.Play();
	}
	
	public void PlayNoSwordLeft()
	{
		_noSwordLeft.Play();
	}
	
	public void PlayPlayerDeath()
	{
		int roll = _rng.RandiRange(1, 3);

		switch (roll)
		{
			case 1:
				_playerDeath1.Play();
				break;
			case 2:
				_playerDeath2.Play();
				break;
			case 3:
				_playerDeath3.Play();
				break;
		}
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
