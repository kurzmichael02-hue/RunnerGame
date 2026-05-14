using Godot;
using System;

public partial class Controls : Control
{

	private Button _resetButton;
	private Button _backButton;

	private Button _jumpBind;
	private Button _moveRightBind;
	private Button _moveLeftBind;
	private Button _duckBind;
	private Button _attackBind;
	public Control PreviousPanel;

	private Button _activeBindButton = null;

	// =========================
	// STATE
	// =========================
	private string _listeningAction = null;

	// =========================
	// SAVE PATH
	// =========================
	private static readonly string SAVE_PATH = "user://settings.cfg";

	// =========================
	// READY
	// =========================
	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		
		_backButton = GetNode<Button>("Back");

		

		_jumpBind = GetNode<Button>("VBoxContainer2/HBoxContainer/JumpBind");
		_moveRightBind = GetNode<Button>("VBoxContainer2/HBoxContainer2/MoveRightBind");
		_moveLeftBind = GetNode<Button>("VBoxContainer2/HBoxContainer3/MoveLeftBind");
		_duckBind = GetNode<Button>("VBoxContainer2/HBoxContainer4/DuckBind");
		_attackBind = GetNode<Button>("VBoxContainer2/HBoxContainer5/AttackBind");

		_resetButton = GetNode<Button>("VBoxContainer2/Reset");

		_backButton.Pressed += OnBackPressed;

		_jumpBind.Pressed += () =>
{
	SoundManager.Instance.PlayButton();
	StartListening("jump", _jumpBind);
};
_moveRightBind.Pressed += () =>
{
	SoundManager.Instance.PlayButton();
	StartListening("move_right", _moveRightBind);
};

_moveLeftBind.Pressed += () =>
{
	SoundManager.Instance.PlayButton();
	StartListening("move_left", _moveLeftBind);
};

_duckBind.Pressed += () =>
{
	SoundManager.Instance.PlayButton();
	StartListening("duck", _duckBind);
};
_attackBind.Pressed += () =>
{
	SoundManager.Instance.PlayButton();
	StartListening("attack", _attackBind);
};
		_resetButton.Pressed += () =>
{
	SoundManager.Instance.PlayButton();
	OnResetPressed();
};

		LoadSettings();
		UpdateButtonTexts();
		ConnectHoverRecursive(this);
	}
private void OnBackPressed()
{
	if (SoundManager.Instance != null)
		SoundManager.Instance.PlayButton();

	// PauseMenu-Modus
	if (PreviousPanel != null)
	{
		PreviousPanel.Visible = true;
		Visible = false;
		return;
	}

	// Standalone Settings-Modus
	GetTree().ChangeSceneToFile(
		"res://Scenes/Main/Settings.tscn"
	);
}


	// =========================
	// START LISTENING
	// =========================
	private void StartListening(string action, Button button)
	{
		if (_activeBindButton != null)
			UpdateButtonTexts();

		_listeningAction = action;
		_activeBindButton = button;

		button.Text = "Press key...";
	}

	// =========================
	// INPUT
	// =========================
	public override void _Input(InputEvent @event)
	{
		if (_listeningAction == null)
			return;

		if (@event is not InputEventKey keyEvent || !keyEvent.Pressed || keyEvent.Echo)
			return;

		GetViewport().SetInputAsHandled();

		// ESC = cancel
		if (keyEvent.PhysicalKeycode == Key.Escape)
		{
			_listeningAction = null;
			_activeBindButton = null;
			UpdateButtonTexts();
			return;
		}

		// Swap duplicate keys
		foreach (string action in new[] { "jump", "move_right", "move_left", "duck", "attack" })
		{
			if (action == _listeningAction)
				continue;

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

		// Set new key
		InputMap.ActionEraseEvents(_listeningAction);

		var ev = new InputEventKey
		{
			PhysicalKeycode = keyEvent.PhysicalKeycode,
			Keycode = keyEvent.Keycode
		};

		InputMap.ActionAddEvent(_listeningAction, ev);
		SoundManager.Instance.PlayButton();

		_listeningAction = null;
		_activeBindButton = null;

		UpdateButtonTexts();
		SaveSettings();
	}

	// =========================
	// UPDATE BUTTON TEXTS
	// =========================
	private void UpdateButtonTexts()
	{
		_jumpBind.Text = GetKeyName("jump");
		_moveRightBind.Text = GetKeyName("move_right");
		_moveLeftBind.Text = GetKeyName("move_left");
		_duckBind.Text = GetKeyName("duck");

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
	// SAVE
	// =========================
	private void SaveSettings()
	{
		var config = new ConfigFile();
		config.Load(SAVE_PATH);

		SaveKey(config, "jump");
		SaveKey(config, "move_right");
		SaveKey(config, "move_left");
		SaveKey(config, "duck");
		SaveKey(config, "attack");

		config.Save(SAVE_PATH);
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

	// =========================
	// LOAD
	// =========================
	public static void LoadSettings()
	{
		var config = new ConfigFile();

		if (config.Load(SAVE_PATH) != Error.Ok)
			return;

		LoadKey(config, "jump");
		LoadKey(config, "move_right");
		LoadKey(config, "move_left");
		LoadKey(config, "duck");
		LoadKey(config, "attack");
	}

	public static void LoadKey(ConfigFile config, string action)
	{
		if (!config.HasSectionKey("keys", action))
			return;

		Key key = (Key)(int)config.GetValue("keys", action);

		InputMap.ActionEraseEvents(action);

		var ev = new InputEventKey
		{
			PhysicalKeycode = key,
			Keycode = key
		};

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
	}

	private void SetDefaultKey(string action)
	{
		Key key = Key.None;

		switch (action)
		{
			case "jump": key = Key.Space; break;
			case "move_right": key = Key.D; break;
			case "move_left": key = Key.A; break;
			case "duck": key = Key.S; break;
			case "attack": key = Key.J; break;
		}

		if (key == Key.None)
			return;

		InputMap.ActionEraseEvents(action);

		var ev = new InputEventKey
		{
			PhysicalKeycode = key,
			Keycode = key
		};

		InputMap.ActionAddEvent(action, ev);
	}

	private void OnResetPressed()
	{
		ApplyDefaults();
		UpdateButtonTexts();
		SaveSettings();
	}
	private void ConnectHoverRecursive(Node node)
{
	foreach (Node child in node.GetChildren())
	{
		if (child is Button button)
		{
			button.MouseEntered += () =>
			{
				if (SoundManager.Instance != null)
					SoundManager.Instance.PlayMenuHover();
			};
		}
		ConnectHoverRecursive(child);
		}
	}
	
}
