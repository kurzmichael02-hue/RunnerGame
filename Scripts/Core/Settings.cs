using Godot;
using System;

public partial class Settings : Control
{
	// UI
	private Button _mainMenuButton;
	private Button _resetButton;

	private Button _activeBindButton = null;

	private Button _jumpBind;
	private Button _moveRightBind;
	private Button _moveLeftBind;
	private Button _duckBind;
	private Button _attackBind;

	// State
	private string _listeningAction = null;
	private bool _isLoading = false;
	
	private HSlider _masterSlider;
	private HSlider _musicSlider;
	private HSlider _gameFxSlider;
	private HSlider _menuFxSlider;
	
	private Button _masterBtn;
	private Button _musicBtn;
	private Button _gameFxBtn;
	private Button _menuFxBtn;

	private bool _masterMuted = false;
	private bool _musicMuted = false;
	private bool _gameFxMuted = false;
	private bool _menuFxMuted = false;

	private float _lastMaster;
	private float _lastMusic;
	private float _lastGameFx;
	private float _lastMenuFx;

	// Save path
	private const string SAVE_PATH = "user://settings.cfg";

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		ConnectHoverRecursive(GetNode("MenuPanel"));
		// Nodes
		
		_mainMenuButton = GetNode<Button>("MenuPanel/VBoxContainer2/MainMenu");
		
		_masterBtn = GetNode<Button>("MenuPanel/VBoxContainer2/HBox1/MasterButton");
		_musicBtn = GetNode<Button>("MenuPanel/VBoxContainer2/HBoxC2/MusicButton");
		_gameFxBtn = GetNode<Button>("MenuPanel/VBoxContainer2/HBoxC3/GameFXButton");
		_menuFxBtn = GetNode<Button>("MenuPanel/VBoxContainer2/HBoxC4/MenuFXButton");
		
		_masterBtn.Pressed += ToggleMaster;
		_musicBtn.Pressed += ToggleMusic;
		_gameFxBtn.Pressed += ToggleGameFx;
		_menuFxBtn.Pressed += ToggleMenuFx;
		
		_masterSlider = GetNode<HSlider>("MenuPanel/VBoxContainer2/HBox1/MasterSlider");
		_musicSlider = GetNode<HSlider>("MenuPanel/VBoxContainer2/HBoxC2/MusicSlider");
		_gameFxSlider = GetNode<HSlider>("MenuPanel/VBoxContainer2/HBoxC3/GameFXSlider");
		_menuFxSlider = GetNode<HSlider>("MenuPanel/VBoxContainer2/HBoxC4/MenuFXSlider");

									
		_jumpBind       = GetNode<Button>("MenuPanel/VBoxContainer2/HBoxContainer/JumpBind");
		_moveRightBind  = GetNode<Button>("MenuPanel/VBoxContainer2/HBoxContainer2/MoveRightBind");
		_moveLeftBind   = GetNode<Button>("MenuPanel/VBoxContainer2/HBoxContainer3/MoveLeftBind");
		_duckBind       = GetNode<Button>("MenuPanel/VBoxContainer2/HBoxContainer4/DuckBind");
		_attackBind = GetNodeOrNull<Button>("MenuPanel/VBoxContainer2/HBoxContainer5/AttackBind");

			
		if (HasNode("MenuPanel/VBoxContainer2/ResetButton"))
			_resetButton = GetNode<Button>("MenuPanel/VBoxContainer2/ResetButton"); 

		// Events
		_mainMenuButton.Pressed += OnMainMenuPressed;
		
		_masterSlider.ValueChanged += (v) => { if (!_isLoading) { SoundManager.Instance.SetMasterVolume((float)v); SaveSettings(); } };
		_musicSlider.ValueChanged += (v) => { if (!_isLoading) { SoundManager.Instance.SetMusicVolume((float)v); SaveSettings(); } };
		_gameFxSlider.ValueChanged += (v) => { if (!_isLoading) { SoundManager.Instance.SetGameFxVolume((float)v); SaveSettings(); } };
		_menuFxSlider.ValueChanged += (v) => { if (!_isLoading) { SoundManager.Instance.SetMenuFxVolume((float)v); SaveSettings(); } };

		_jumpBind.Pressed += () => StartListening("jump", _jumpBind);
		_moveRightBind.Pressed += () => StartListening("move_right", _moveRightBind);
		_moveLeftBind.Pressed += () => StartListening("move_left", _moveLeftBind);
		_duckBind.Pressed += () => StartListening("duck", _duckBind);

		if (_attackBind != null)
			_attackBind.Pressed += () => StartListening("attack", _attackBind);
		
		if (_resetButton != null)
			_resetButton.Pressed += OnResetPressed;

		LoadSettings();
		UpdateButtonTexts();
	}

	// =========================
	// AUDIO (0–100 → 0–1 → dB)
	// =========================
	private void OnVolumeChanged(double value)
	{
		if (_isLoading) return;
		SoundManager.Instance.SetMasterVolume((float)value);
		SaveSettings();
	}

	// =========================
	// NAVIGATION
	// =========================
	private void OnMainMenuPressed()
	{
		SaveSettings();

		// Szenewechsel verzögert → kein Timing-Problem mehr
		CallDeferred(nameof(ChangeToMainMenu));
	}

	private void ChangeToMainMenu()
	{
		if (!IsInsideTree()) return;

		GetTree().ChangeSceneToFile("res://Scenes/Main/MainMenu.tscn");
	}

	// =========================
	// KEY REBINDING
	// =========================
	private void StartListening(string action, Button button)
	{
		// alten Button zurücksetzen
		if (_activeBindButton != null)
			UpdateButtonTexts();

		_listeningAction = action;
		_activeBindButton = button;

		button.Text = "Press key...";
	}


	public override void _Input(InputEvent @event)
	{
		if (_listeningAction == null) return;
		if (@event is not InputEventKey keyEvent || !keyEvent.Pressed || keyEvent.Echo)
			return;
		GetViewport().SetInputAsHandled();

		// ESC → abbrechen
		if (keyEvent.PhysicalKeycode == Key.Escape)
		{
			_listeningAction = null;
			_activeBindButton = null;
			UpdateButtonTexts();
			return;
		}

		// WICHTIG: gleiche Logik wie im PauseMenu (Swap)
		foreach (string action in new[] { "jump", "move_right", "move_left", "duck", "attack" })
		{
			if (action == _listeningAction) continue;

			foreach (InputEvent existing in InputMap.ActionGetEvents(action))
			{
				if (existing is InputEventKey existingKey &&
					existingKey.PhysicalKeycode == keyEvent.PhysicalKeycode)
				{
					var currentEvents = InputMap.ActionGetEvents(_listeningAction);

					InputMap.ActionEraseEvents(action);
					foreach (var e in currentEvents)
						InputMap.ActionAddEvent(action, e);
				}
			}
		}

		// Neue Taste setzen
		InputMap.ActionEraseEvents(_listeningAction);

		var ev = new InputEventKey();
		ev.PhysicalKeycode = keyEvent.PhysicalKeycode;
		ev.Keycode = keyEvent.Keycode;

		InputMap.ActionAddEvent(_listeningAction, ev);

		_listeningAction = null;
		_activeBindButton = null;
		UpdateButtonTexts();
		SaveSettings();
	}
	private void UpdateButtonTexts()
	{
		_jumpBind.Text      = GetKeyName("jump");
		_moveRightBind.Text = GetKeyName("move_right");
		_moveLeftBind.Text  = GetKeyName("move_left");
		_duckBind.Text      = GetKeyName("duck");
		
		if (_attackBind != null)
			_attackBind.Text = GetKeyName("attack");
	}

	private string GetKeyName(string action)
	{
		var events = InputMap.ActionGetEvents(action);

		if (events.Count > 0 && events[0] is InputEventKey keyEvent)
		{
			var key = keyEvent.PhysicalKeycode != Key.None
				? keyEvent.PhysicalKeycode
				: keyEvent.Keycode;

			return OS.GetKeycodeString(key);
		}

		return "None";
	}



	// =========================
	// SAVE / LOAD
	// =========================
	private void SaveSettings()
	{
		var config = new ConfigFile();

		config.SetValue("audio", "master", _masterSlider.Value);
		config.SetValue("audio", "music", _musicSlider.Value);
		config.SetValue("audio", "gamefx", _gameFxSlider.Value);
		config.SetValue("audio", "menufx", _menuFxSlider.Value);

		SaveKey(config, "jump");
		SaveKey(config, "move_right");
		SaveKey(config, "move_left");
		SaveKey(config, "duck");
		SaveKey(config, "attack");
		config.Save(SAVE_PATH);
	}
	
	private void ConnectHoverRecursive(Node node)
	{
		foreach (Node child in node.GetChildren())
		{
			if (child is Button button)
			{
				button.MouseEntered += () =>
				{
					SoundManager.Instance.PlayMenuHover();
				};
			}

			ConnectHoverRecursive(child);
		}
	}
	
	private void LoadSettings()
	{
		var config = new ConfigFile();
		_isLoading = true;

		if (config.Load(SAVE_PATH) != Error.Ok)
		{
			_masterSlider.Value = 100;
			_musicSlider.Value = 100;
			_gameFxSlider.Value = 100;
			_menuFxSlider.Value = 100;

			SoundManager.Instance.SetMasterVolume(100);
			SoundManager.Instance.SetMusicVolume(100);
			SoundManager.Instance.SetGameFxVolume(100);
			SoundManager.Instance.SetMenuFxVolume(100);

			_isLoading = false;
			return;
		}

		_masterSlider.Value = (double)config.GetValue("audio", "master", 100.0);
		_musicSlider.Value = (double)config.GetValue("audio", "music", 100.0);
		_gameFxSlider.Value = (double)config.GetValue("audio", "gamefx", 100.0);
		_menuFxSlider.Value = (double)config.GetValue("audio", "menufx", 100.0);

		SoundManager.Instance.SetMasterVolume((float)_masterSlider.Value);
		SoundManager.Instance.SetMusicVolume((float)_musicSlider.Value);
		SoundManager.Instance.SetGameFxVolume((float)_gameFxSlider.Value);
		SoundManager.Instance.SetMenuFxVolume((float)_menuFxSlider.Value);

		LoadKey(config, "jump");
		LoadKey(config, "move_right");
		LoadKey(config, "move_left");
		LoadKey(config, "duck");
		LoadKey(config, "attack");

		_isLoading = false;
		
UpdateButtonState(_masterBtn, _masterMuted, "Master Volume", "Master");
UpdateButtonState(_musicBtn, _musicMuted, "Music Volume", "Music");
UpdateButtonState(_gameFxBtn, _gameFxMuted, "Sound Effects", "Sound");
UpdateButtonState(_menuFxBtn, _menuFxMuted, "Menu Sounds", "Menu");
	}

	private void SaveKey(ConfigFile config, string action)
	{
		var events = InputMap.ActionGetEvents(action);

		if (events.Count > 0 && events[0] is InputEventKey keyEvent)
		{
			var key = keyEvent.PhysicalKeycode != Key.None
				? keyEvent.PhysicalKeycode
				: keyEvent.Keycode;

			config.SetValue("keys", action, (int)key);
		}
	}

	private void LoadKey(ConfigFile config, string action)
	{
		if (!config.HasSectionKey("keys", action))
		{
			return;
			
		}

		Key key = (Key)(int)config.GetValue("keys", action);

		InputMap.ActionEraseEvents(action);

		var ev = new InputEventKey();
		ev.PhysicalKeycode = key;
		ev.Keycode = key;

		InputMap.ActionAddEvent(action, ev);
	}

	// =========================
	// DEFAULTS
	// =========================
	private void ApplyDefaults()
	{
		SetDefaultKey("jump");
		SetDefaultKey("move_right");
		SetDefaultKey("move_left");
		SetDefaultKey("duck");
		SetDefaultKey("attack");

		_masterSlider.Value = 100;
		_musicSlider.Value = 100;
		_gameFxSlider.Value = 100;
		_menuFxSlider.Value = 100;

		SoundManager.Instance.SetMasterVolume(100);
		SoundManager.Instance.SetMusicVolume(100);
		SoundManager.Instance.SetGameFxVolume(100);
		SoundManager.Instance.SetMenuFxVolume(100);
	}


	private void SetDefaultKey(string action)
	{
		Key key = Key.None;

		switch (action)
		{
			case "jump":       key = Key.Space; break;
			case "move_right": key = Key.D;     break;
			case "move_left":  key = Key.A;     break;
			case "duck":       key = Key.S;     break;
			case "attack":     key = Key.J;     break;
		}

		if (key == Key.None) return;

		InputMap.ActionEraseEvents(action);

		var ev = new InputEventKey();
		ev.PhysicalKeycode = key;
		ev.Keycode = key;

		InputMap.ActionAddEvent(action, ev);
	}

	private void OnResetPressed()
{
	ApplyDefaults();
	UpdateButtonTexts();
	SaveSettings();
}

private void ToggleMaster()
{
	if (!_masterMuted)
	{
		// Werte speichern
		_lastMaster = (float)_masterSlider.Value;
		_lastMusic = (float)_musicSlider.Value;
		_lastGameFx = (float)_gameFxSlider.Value;
		_lastMenuFx = (float)_menuFxSlider.Value;

		// ALLES muten
		_masterSlider.Value = 0;
		_musicSlider.Value = 0;
		_gameFxSlider.Value = 0;
		_menuFxSlider.Value = 0;

		SoundManager.Instance.SetMasterVolume(0);
		SoundManager.Instance.SetMusicVolume(0);
		SoundManager.Instance.SetGameFxVolume(0);
		SoundManager.Instance.SetMenuFxVolume(0);

		// ALLE STATES setzen
		_masterMuted = true;
		_musicMuted = true;
		_gameFxMuted = true;
		_menuFxMuted = true;
	}
	else
	{
		// Werte zurück
		_masterSlider.Value = _lastMaster;
		_musicSlider.Value = _lastMusic;
		_gameFxSlider.Value = _lastGameFx;
		_menuFxSlider.Value = _lastMenuFx;

		SoundManager.Instance.SetMasterVolume(_lastMaster);
		SoundManager.Instance.SetMusicVolume(_lastMusic);
		SoundManager.Instance.SetGameFxVolume(_lastGameFx);
		SoundManager.Instance.SetMenuFxVolume(_lastMenuFx);

		// ALLE STATES zurück
		_masterMuted = false;
		_musicMuted = false;
		_gameFxMuted = false;
		_menuFxMuted = false;
	}

	// UI UPDATE (RICHTIG)
	UpdateButtonState(_masterBtn, _masterMuted, "Master Volume", "Master");
	UpdateButtonState(_musicBtn, _musicMuted, "Music Volume", "Music");
	UpdateButtonState(_gameFxBtn, _gameFxMuted, "Sound Effects", "Sound");
	UpdateButtonState(_menuFxBtn, _menuFxMuted, "Menu Sounds", "Menu");
}



	private void ToggleMusic()
	{
		if (!_musicMuted)
		{
			_lastMusic = (float)_musicSlider.Value;
			_musicSlider.Value = 0;
			SoundManager.Instance.SetMusicVolume(0);
			_musicMuted = true;
		}
		else
		{
			_musicSlider.Value = _lastMusic;
			SoundManager.Instance.SetMusicVolume(_lastMusic);
			_musicMuted = false;
		}
UpdateButtonState(_musicBtn, _musicMuted, "Music Volume", "Music");
	}
	private void ToggleGameFx()
	{
		if (!_gameFxMuted)
		{
			_lastGameFx = (float)_gameFxSlider.Value;
			_gameFxSlider.Value = 0;
			SoundManager.Instance.SetGameFxVolume(0);
			_gameFxMuted = true;
		}
		else
		{
			_gameFxSlider.Value = _lastGameFx;
			SoundManager.Instance.SetGameFxVolume(_lastGameFx);
			_gameFxMuted = false;
		}
UpdateButtonState(_gameFxBtn, _gameFxMuted, "Sound Effects", "Sound");
	}
	private void ToggleMenuFx()
	{
		if (!_menuFxMuted)
		{
			_lastMenuFx = (float)_menuFxSlider.Value;
			_menuFxSlider.Value = 0;
			SoundManager.Instance.SetMenuFxVolume(0);
			_menuFxMuted = true;
		}
		else
		{
			_menuFxSlider.Value = _lastMenuFx;
			SoundManager.Instance.SetMenuFxVolume(_lastMenuFx);
			_menuFxMuted = false;
		}
UpdateButtonState(_menuFxBtn, _menuFxMuted, "Menu Sounds", "Menu");
	}
	
private void UpdateButtonState(Button btn, bool muted, string normalText, string mutedText)
{
	btn.ButtonPressed = muted;

	if (muted)
		btn.Text = $"{mutedText} (Muted)";
	else
		btn.Text = normalText;
}
}
