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

	// State
	private string _listeningAction = null;
	private bool _isLoading = false;

	// Save path
	private const string SAVE_PATH = "user://settings.cfg";

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;

		// Nodes
		_mainMenuButton = GetNode<Button>("MenuPanel/VBoxContainer/Main Menu");
		_volumeSlider   = GetNode<HSlider>("MenuPanel/VBoxContainer/HSlider");

		_jumpBind       = GetNode<Button>("MenuPanel/VBoxContainer/HBoxContainer/JumpBind");
		_moveRightBind  = GetNode<Button>("MenuPanel/VBoxContainer/HBoxContainer2/MoveRightBind");
		_moveLeftBind   = GetNode<Button>("MenuPanel/VBoxContainer/HBoxContainer3/MoveLeftBind");
		_duckBind       = GetNode<Button>("MenuPanel/VBoxContainer/HBoxContainer4/DuckBind");

		if (HasNode("MenuPanel/VBoxContainer/ResetButton"))
			_resetButton = GetNode<Button>("MenuPanel/VBoxContainer/ResetButton");

		// Events
		_mainMenuButton.Pressed += OnMainMenuPressed;
		_volumeSlider.ValueChanged += OnVolumeChanged;

		_jumpBind.Pressed      += () => StartListening("jump");
		_moveRightBind.Pressed += () => StartListening("move_right");
		_moveLeftBind.Pressed  += () => StartListening("move_left");
		_duckBind.Pressed      += () => StartListening("duck");

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

		float normalized = (float)value / 100f;
		ApplyVolume(normalized);

		SaveSettings();
	}

	private void ApplyVolume(float value)
	{
		float db = value <= 0.001f ? -80f : Mathf.LinearToDb(value);

		int bus = AudioServer.GetBusIndex("Master");
		AudioServer.SetBusVolumeDb(bus, db);
	}

	// =========================
	// NAVIGATION
	// =========================
	private void OnMainMenuPressed()
	{
		SaveSettings();
		GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
	}

	// =========================
	// KEY REBINDING
	// =========================
	private void StartListening(string action)
	{
		_listeningAction = action;
	}

	public override void _Input(InputEvent @event)
	{
		if (_listeningAction == null)
			return;

		if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
		{
			InputMap.ActionEraseEvents(_listeningAction);
			InputMap.ActionAddEvent(_listeningAction, keyEvent);

			_listeningAction = null;

			UpdateButtonTexts();
			SaveSettings();
		}
	}

	private void UpdateButtonTexts()
	{
		_jumpBind.Text      = GetKeyName("jump");
		_moveRightBind.Text = GetKeyName("move_right");
		_moveLeftBind.Text  = GetKeyName("move_left");
		_duckBind.Text      = GetKeyName("duck");
	}

	private string GetKeyName(string action)
	{
		var events = InputMap.ActionGetEvents(action);

		if (events.Count > 0 && events[0] is InputEventKey keyEvent)
			return OS.GetKeycodeString(keyEvent.Keycode);

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

		config.Save(SAVE_PATH);
	}

	private void LoadSettings()
	{
		var config = new ConfigFile();

		_isLoading = true;

		if (config.Load(SAVE_PATH) != Error.Ok)
		{
			_volumeSlider.Value = 100; ApplyVolume(1.0f);
			_isLoading = false;
			return;
		}

		float volume = (float)config.GetValue("audio", "volume", 100.0f);
		_volumeSlider.Value = volume;
		ApplyVolume(volume / 100f);

		LoadKey(config, "jump");
		LoadKey(config, "move_right");
		LoadKey(config, "move_left");
		LoadKey(config, "duck");

		_isLoading = false;
	}

	private void SaveKey(ConfigFile config, string action)
	{
		var events = InputMap.ActionGetEvents(action);

		if (events.Count > 0 && events[0] is InputEventKey keyEvent)
			config.SetValue("keys", action, (int)keyEvent.Keycode);
	}

	private void LoadKey(ConfigFile config, string action)
	{
		if (!config.HasSectionKey("keys", action))
		{
			return;
			
		}

		int keycodeInt = (int)config.GetValue("keys", action);
		Key keycode = (Key)keycodeInt;

		InputMap.ActionEraseEvents(action);

		var ev = new InputEventKey();
		ev.Keycode = keycode;

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

		_volumeSlider.Value = 100;
		ApplyVolume(1.0f);
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
		}

		if (key == Key.None) return;

		InputMap.ActionEraseEvents(action);

		var ev = new InputEventKey();
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
