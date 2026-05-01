using Godot;
using System;

public partial class Settings : Control
{
	// UI
	private Button _mainMenuButton;
	private Button _resetButton;

	private HSlider _volumeSlider;

	private Button _jumpBind;
	private Button _moveRightBind;
	private Button _moveLeftBind;
	private Button _duckBind;
	private Button _attackBind;

	// State
	private string _listeningAction = null;
	private bool _isLoading = false;

	// Save path
	private const string SAVE_PATH = "user://settings.cfg";

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		ConnectHoverRecursive(GetNode("MenuPanel"));
		// Nodes
		_mainMenuButton = GetNode<Button>("MenuPanel/VBoxContainer2/MainMenu");
		_volumeSlider   = GetNode<HSlider>("MenuPanel/VBoxContainer2/HSlider");

		_jumpBind       = GetNode<Button>("MenuPanel/VBoxContainer2/HBoxContainer/JumpBind");
		_moveRightBind  = GetNode<Button>("MenuPanel/VBoxContainer2/HBoxContainer2/MoveRightBind");
		_moveLeftBind   = GetNode<Button>("MenuPanel/VBoxContainer2/HBoxContainer3/MoveLeftBind");
		_duckBind       = GetNode<Button>("MenuPanel/VBoxContainer2/HBoxContainer4/DuckBind");
		_attackBind = GetNodeOrNull<Button>("MenuPanel/VBoxContainer2/HBoxContainer5/AttackBind");

			
		if (HasNode("MenuPanel/VBoxContainer2/ResetButton"))
			_resetButton = GetNode<Button>("MenuPanel/VBoxContainer2/ResetButton"); 

		// Events
		_mainMenuButton.Pressed += OnMainMenuPressed;
		_volumeSlider.ValueChanged += OnVolumeChanged;

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
		_listeningAction = action;
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

		config.SetValue("audio", "volume", _volumeSlider.Value);

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
			_volumeSlider.Value = 100;
			SoundManager.Instance.SetMasterVolume(100); 
			_isLoading = false;
			return;
		}

		float volume = (float)config.GetValue("audio", "volume", 100.0f);

		_volumeSlider.ValueChanged -= OnVolumeChanged;
		_volumeSlider.Value = volume;
		_volumeSlider.ValueChanged += OnVolumeChanged;

		SoundManager.Instance.SetMasterVolume(volume);

		LoadKey(config, "jump");
		LoadKey(config, "move_right");
		LoadKey(config, "move_left");
		LoadKey(config, "duck");
		LoadKey(config, "attack");

		_isLoading = false;
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

		_volumeSlider.ValueChanged -= OnVolumeChanged;
		_volumeSlider.Value = 100;
		_volumeSlider.ValueChanged += OnVolumeChanged;
		SoundManager.Instance.SetMasterVolume(100);
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
}
