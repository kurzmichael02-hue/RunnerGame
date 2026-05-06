using Godot;

public partial class PauseMenu : CanvasLayer {

	private Button _jumpBind;
	private Button _moveRightBind;
	private Button _moveLeftBind;
	private Button _duckBind;
	private Button _attackBind;
	private string _listeningAction = null;
	private Button _activeBindButton = null;
	
	private HSlider _masterSlider;
	private HSlider _musicSlider;
	private HSlider _gameFxSlider;
	private HSlider _menuFxSlider;
	
	private bool _masterMuted;
	private float _lastMaster;
	private Button _masterBtn;
	
	private bool _musicMuted;
	private bool _gameFxMuted;
	private bool _menuFxMuted;

	private float _lastMusic;
	private float _lastGameFx;
	private float _lastMenuFx;

	private Button _musicBtn;
	private Button _gameFxBtn;
	private Button _menuFxBtn;
	
	private bool _isLoading = false;

	public override void _Ready()
	{

		ProcessMode = ProcessModeEnum.Always;
		SetProcessInput(true);
		SetProcessUnhandledInput(true);
		GetNode<Control>("MenuPanel").MouseFilter = Control.MouseFilterEnum.Stop;
		
		_masterSlider = GetNode<HSlider>("MenuPanel/VBoxContainer2/HBox1/MasterSlider");
		_musicSlider = GetNode<HSlider>("MenuPanel/VBoxContainer2/HBoxC2/MusicSlider");
		_gameFxSlider = GetNode<HSlider>("MenuPanel/VBoxContainer2/HBoxC3/GameFXSlider");
		_menuFxSlider = GetNode<HSlider>("MenuPanel/VBoxContainer2/HBoxC4/MenuFXSlider");
				
		_masterBtn = GetNode<Button>("MenuPanel/VBoxContainer2/HBox1/MasterButton");
_masterSlider.ValueChanged += (v) =>
{
	if (_isLoading || _masterMuted) return;
	SoundManager.Instance.SetMasterVolume((float)v);
	SaveSettings(); 
};

_musicSlider.ValueChanged += (v) =>
{
	if (_isLoading || _musicMuted) return;
	SoundManager.Instance.SetMusicVolume((float)v);
	SaveSettings();
};

_gameFxSlider.ValueChanged += (v) =>
{
	if (_isLoading || _gameFxMuted) return;
	SoundManager.Instance.SetGameFxVolume((float)v);
	SaveSettings();
};

_menuFxSlider.ValueChanged += (v) =>
{
	if (_isLoading || _menuFxMuted) return;
	SoundManager.Instance.SetMenuFxVolume((float)v);
	SaveSettings();
};

		GetNode<Button>("MenuPanel/VBoxContainer2/Resume").Pressed += OnResumePressed;
		GetNode<Button>("MenuPanel/VBoxContainer2/Exit").Pressed += OnExitPressed;
		GetNode<Button>("MenuPanel/VBoxContainer2/MainMenu").Pressed += OnMainMenuPressed;



		_jumpBind = GetNode<Button>("MenuPanel/VBoxContainer2/HBoxContainer/JumpBind");
		_moveRightBind = GetNode<Button>("MenuPanel/VBoxContainer2/HBoxContainer2/MoveRightBind");
		_moveLeftBind = GetNode<Button>("MenuPanel/VBoxContainer2/HBoxContainer3/MoveLeftBind");
		_duckBind = GetNode<Button>("MenuPanel/VBoxContainer2/HBoxContainer4/DuckBind");
		_attackBind = GetNodeOrNull<Button>("MenuPanel/VBoxContainer2/HBoxContainer5/AttackBind");

		_jumpBind.Pressed += () => StartListening("jump", _jumpBind);
		_moveRightBind.Pressed += () => StartListening("move_right", _moveRightBind);
		_moveLeftBind.Pressed += () => StartListening("move_left", _moveLeftBind);
		_duckBind.Pressed += () => StartListening("duck", _duckBind);
		if (_attackBind != null)
			_attackBind.Pressed += () => StartListening("attack", _attackBind);
			
		_masterBtn.Pressed += ToggleMaster;
		_musicBtn = GetNode<Button>("MenuPanel/VBoxContainer2/HBoxC2/MusicButton");
_gameFxBtn = GetNode<Button>("MenuPanel/VBoxContainer2/HBoxC3/GameFXButton");
_menuFxBtn = GetNode<Button>("MenuPanel/VBoxContainer2/HBoxC4/MenuFXButton");

_musicBtn.Pressed += ToggleMusic;
_gameFxBtn.Pressed += ToggleGameFx;
_menuFxBtn.Pressed += ToggleMenuFx;
		ConnectHoverRecursive(GetNode("MenuPanel"));
		LoadSettings();
		UpdateBindLabels();
	}

	private void ConnectHoverRecursive(Node node)
	{
		foreach (Node child in node.GetChildren())
		{
			if (child is Button button)
			{
				button.MouseEntered += OnAnyButtonHovered;
			}

			// REKURSIV weiter runtergehen
			ConnectHoverRecursive(child);
		}
	}
	
	private void OnAnyButtonHovered()
	{
		if (_listeningAction != null) return;
		SoundManager.Instance.PlayMenuHover();
	}

	private void StartListening(string action, Button button)
	{
		_listeningAction = action;

		// Reset alten Button
		if (_activeBindButton != null)
			UpdateBindLabels();

		_activeBindButton = button;
		button.Text = "Press key...";
		GetViewport().SetInputAsHandled();
	}


public override void _Input(InputEvent @event)
{
	if (!Visible) return;

	// ESC → Menü schließen (nur wenn kein Keybinding aktiv)
	if (@event.IsActionPressed("ui_cancel") && _listeningAction == null)
	{
		GetViewport().SetInputAsHandled();
		OnResumePressed();
		return;
	}

	// Wenn kein Keybinding aktiv → nichts tun
	if (_listeningAction == null) return;

	// Nur echte Key-Inputs
	if (@event is not InputEventKey keyEvent || !keyEvent.Pressed || keyEvent.Echo)
		return;

	GetViewport().SetInputAsHandled();

	// ESC → Keybinding abbrechen
	if (keyEvent.PhysicalKeycode == Key.Escape)
	{
		CancelListening();
		return;
	}

	// Swap-Logik
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

	var newEvent = new InputEventKey
	{
		Keycode = keyEvent.Keycode,
		PhysicalKeycode = keyEvent.PhysicalKeycode
	};

	InputMap.ActionAddEvent(_listeningAction, newEvent);

	_listeningAction = null;
	_activeBindButton = null;

	UpdateBindLabels();
	SaveSettings();
}

	private void CancelListening()
	{
		_listeningAction = null;
		_activeBindButton = null;
		UpdateBindLabels();
	}

	private void UpdateBindLabels()
	{
		_jumpBind.Text = GetFirstKey("jump");
		_moveRightBind.Text = GetFirstKey("move_right");
		_moveLeftBind.Text = GetFirstKey("move_left");
		_duckBind.Text = GetFirstKey("duck");
		if (_attackBind != null) _attackBind.Text = GetFirstKey("attack");
	}

	private string GetFirstKey(string action)
	{
		foreach (var e in InputMap.ActionGetEvents(action))
		{
			if (e is InputEventKey keyEvent)
			{
				var key = keyEvent.PhysicalKeycode != Key.None
					? keyEvent.PhysicalKeycode
					: keyEvent.Keycode;
				return OS.GetKeycodeString(key);
			}
		}
		return "<unbound>";
	}

	private void SaveSettings()
	{
		var config = new ConfigFile();
		// erst laden – sonst überschreiben wir die music/gamefx/menufx-werte
		// die settings.cs gespeichert hat. ohne das load verliert man alles ausser
		// dem master-slider sobald man einmal pause öffnet.
		config.Load("user://settings.cfg");

		foreach (string action in new[] { "jump", "move_right", "move_left", "duck", "attack" })
		{
			var events = InputMap.ActionGetEvents(action);
			if (events.Count > 0 && events[0] is InputEventKey key)
			{
				var keycode = key.PhysicalKeycode != Key.None
					? key.PhysicalKeycode
					: key.Keycode;
				// "keys" statt "bindings" – gleicher key wie settings.cs,
				// sonst liest settings.cs beim nächsten öffnen die alten werte
				config.SetValue("keys", action, (int)keycode);
			}
		}
		config.SetValue("audio", "master", _masterSlider.Value);
		config.SetValue("audio", "music", _musicSlider.Value);
		config.SetValue("audio", "gamefx", _gameFxSlider.Value);
		config.SetValue("audio", "menufx", _menuFxSlider.Value);

		config.Save("user://settings.cfg");
	}


	private void LoadSettings()
	{
		_isLoading = true;
		var config = new ConfigFile();
		if (config.Load("user://settings.cfg") != Error.Ok) return;

		foreach (string action in new[] { "jump", "move_right", "move_left", "duck", "attack" })
		{
			if (!config.HasSectionKey("keys", action)) continue;
			int keycode = (int)config.GetValue("keys", action);
			var keyEvent = new InputEventKey();
			keyEvent.Keycode = (Key)keycode;
			keyEvent.PhysicalKeycode = (Key)keycode;
			InputMap.ActionEraseEvents(action);
			InputMap.ActionAddEvent(action, keyEvent);
		}

if (config.HasSectionKey("audio", "master"))
{
	_masterSlider.Value = (double)config.GetValue("audio", "master");
	SoundManager.Instance.SetMasterVolume((float)_masterSlider.Value);
}

if (config.HasSectionKey("audio", "music"))
{
	_musicSlider.Value = (double)config.GetValue("audio", "music");
	SoundManager.Instance.SetMusicVolume((float)_musicSlider.Value);
}

if (config.HasSectionKey("audio", "gamefx"))
{
	_gameFxSlider.Value = (double)config.GetValue("audio", "gamefx");
	SoundManager.Instance.SetGameFxVolume((float)_gameFxSlider.Value);
}

if (config.HasSectionKey("audio", "menufx"))
{
	_menuFxSlider.Value = (double)config.GetValue("audio", "menufx");
	SoundManager.Instance.SetMenuFxVolume((float)_menuFxSlider.Value);
}
UpdateButtonState(_masterBtn, _masterMuted, "Master Volume", "Master");
UpdateButtonState(_musicBtn, _musicMuted, "Music Volume", "Music");
UpdateButtonState(_gameFxBtn, _gameFxMuted, "Sound Effects", "Sound");
UpdateButtonState(_menuFxBtn, _menuFxMuted, "Menu Sounds", "Menu");

_masterMuted = _masterSlider.Value == 0;
_musicMuted = _musicSlider.Value == 0;
_gameFxMuted = _gameFxSlider.Value == 0;
_menuFxMuted = _menuFxSlider.Value == 0;

_isLoading = false;


}

private void OnResumePressed()
{
	GetViewport().SetInputAsHandled();

	var gm = GetNodeOrNull<GameManager>("/root/Node2D");

	if (gm != null)
	{
		gm.TogglePause();
	}

	SoundManager.Instance.PlayButton();
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




	private void OnExitPressed() {
		SoundManager.Instance.PlayButton();
		QueueFree();
		GetTree().Quit();
	}

	private void OnMainMenuPressed()
	{
		GetTree().Paused = false;
		SoundManager.Instance.PlayButton();
		// WICHTIG: PauseMenu entfernen
		QueueFree();
		GetTree().ChangeSceneToFile("res://Scenes/Main/MainMenu.tscn");
	}
		public override void _UnhandledInput(InputEvent @event)
	{
		_Input(@event);
	}
	
}
