using Godot;

public partial class PauseMenu : Control
{
	private HSlider _volumeSlider;
	private Button _jumpBind;
	private Button _moveLeftBind;
	private Button _moveRightBind;
	private string _listeningAction = null;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;

		GetNode<Button>("MenuPanel/VBoxContainer/Resume").Pressed += OnResumePressed;
		GetNode<Button>("MenuPanel/VBoxContainer/Exit").Pressed += OnExitPressed;
		GetNode<Button>("MenuPanel/VBoxContainer/Main Menu").Pressed += OnMainMenuPressed;

		_volumeSlider = GetNode<HSlider>("MenuPanel/VBoxContainer/HSlider");
		_volumeSlider.MinValue = 0;
		_volumeSlider.MaxValue = 100;
		_volumeSlider.ValueChanged += OnVolumeChanged;

		_jumpBind = GetNode<Button>("MenuPanel/VBoxContainer/HBoxContainer/JumpBind");
		_moveLeftBind = GetNode<Button>("MenuPanel/VBoxContainer/HBoxContainer2/MoveLeftBind");
		_moveRightBind = GetNode<Button>("MenuPanel/VBoxContainer/HBoxContainer3/MoveRightBind");

		_jumpBind.Pressed += () => StartListening("jump", _jumpBind);
		_moveLeftBind.Pressed += () => StartListening("move_left", _moveLeftBind);
		_moveRightBind.Pressed += () => StartListening("move_right", _moveRightBind);

		LoadSettings();
		UpdateBindLabels();
	}

	private void StartListening(string action, Button button)
	{
		_listeningAction = action;
		button.Text = "Press a key...";
		// Block all input until next frame so the click doesn't register as a bind
		SetProcessUnhandledKeyInput(false);
		CallDeferred(nameof(EnableKeyListening));
	}

	private void EnableKeyListening()
	{
		SetProcessUnhandledKeyInput(true);
	}

	public override void _UnhandledKeyInput(InputEvent @event)
	{
		if (_listeningAction == null) return;
		if (@event is not InputEventKey keyEvent || !keyEvent.Pressed || keyEvent.Echo) return;

		// ESC cancels rebinding without saving
		if (keyEvent.Keycode == Key.Escape)
		{
			CancelListening();
			return;
		}

		// Swap if this key is already used by another action
		foreach (string action in new[] { "jump", "move_left", "move_right" })
		{
			if (action == _listeningAction) continue;
			foreach (InputEvent existing in InputMap.ActionGetEvents(action))
			{
				if (existing.AsText() == @event.AsText())
				{
					// Give this action the current binding of _listeningAction
					var currentEvents = InputMap.ActionGetEvents(_listeningAction);
					InputMap.ActionEraseEvents(action);
					foreach (var e in currentEvents)
						InputMap.ActionAddEvent(action, e);
				}
			}
		}

		// Assign new key
		InputMap.ActionEraseEvents(_listeningAction);
		InputMap.ActionAddEvent(_listeningAction, @event);

		_listeningAction = null;
		SetProcessUnhandledKeyInput(false);
		UpdateBindLabels();
		SaveSettings();
		GetViewport().SetInputAsHandled();
	}

	private void CancelListening()
	{
		_listeningAction = null;
		SetProcessUnhandledKeyInput(false);
		UpdateBindLabels();
	}

	private void UpdateBindLabels()
	{
		_jumpBind.Text = GetFirstKey("jump");
		_moveLeftBind.Text = GetFirstKey("move_left");
		_moveRightBind.Text = GetFirstKey("move_right");
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

		// Save key bindings
		foreach (string action in new[] { "jump", "move_left", "move_right" })
		{
			var events = InputMap.ActionGetEvents(action);
			if (events.Count > 0 && events[0] is InputEventKey key)
				config.SetValue("bindings", action, (int)key.PhysicalKeycode);
		}

		// Save volume
		config.SetValue("audio", "volume", _volumeSlider.Value);
		config.Save("user://settings.cfg");
	}

	private void LoadSettings()
	{
		var config = new ConfigFile();
		if (config.Load("user://settings.cfg") != Error.Ok) return;

		// Load key bindings
		foreach (string action in new[] { "jump", "move_left", "move_right" })
		{
			if (!config.HasSectionKey("bindings", action)) continue;
			int keycode = (int)config.GetValue("bindings", action);
			var keyEvent = new InputEventKey();
			keyEvent.PhysicalKeycode = (Key)keycode;
			InputMap.ActionEraseEvents(action);
			InputMap.ActionAddEvent(action, keyEvent);
		}

		// Load volume
		if (config.HasSectionKey("audio", "volume"))
		{
			double volume = (double)config.GetValue("audio", "volume");
			_volumeSlider.Value = volume;
			float db = Mathf.LinearToDb((float)(volume / 100.0));
			AudioServer.SetBusVolumeDb(0, db);
		}
	}

	private void OnResumePressed()
	{
		GetTree().Paused = false;
		Visible = false;
		SoundManager.Instance.SwitchMusic(SoundManager.Instance.GameMusic);
	}

	private void OnVolumeChanged(double value)
	{
		float db = Mathf.LinearToDb((float)(value / 100.0));
		AudioServer.SetBusVolumeDb(0, db);
		SaveSettings();
	}

	private void OnExitPressed()
	{
		GetTree().Quit();
	}

	private void OnMainMenuPressed()
	{
		GetTree().Paused = false;
		GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
	}
}
