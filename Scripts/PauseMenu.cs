using Godot;

public partial class PauseMenu : Control
{
	private HSlider _volumeSlider;
	private Button _jumpBind;
	private Button _moveRightBind;
	private Button _moveLeftBind;
	private Button _duckBind;
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
		_moveRightBind = GetNode<Button>("MenuPanel/VBoxContainer/HBoxContainer2/MoveRightBind");
		_moveLeftBind = GetNode<Button>("MenuPanel/VBoxContainer/HBoxContainer3/MoveLeftBind");
		_duckBind = GetNode<Button>("MenuPanel/VBoxContainer/HBoxContainer4/DuckBind");

		_jumpBind.Pressed += () => StartListening("jump", _jumpBind);
		_moveRightBind.Pressed += () => StartListening("move_right", _moveRightBind);
		_moveLeftBind.Pressed += () => StartListening("move_left", _moveLeftBind);
		_duckBind.Pressed += () => StartListening("duck", _duckBind);
		
		var root = GetNode("MenuPanel");

		foreach (Node child in root.GetChildren())
		{
			foreach (Node subChild in child.GetChildren())
			{
				if (subChild is Button button)
				{
					button.MouseEntered += OnAnyButtonHovered;
				}
			}
		}

		LoadSettings();
		UpdateBindLabels();
	}
	
	private void OnAnyButtonHovered()
	{	
		//GD.Print("hover"); 
		SoundManager.Instance.PlayMenuHover();
	}

	private void StartListening(string action, Button button)
	{
		_listeningAction = action;
		button.Text = "Press a key...";
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

		if (keyEvent.Keycode == Key.Escape)
		{
			CancelListening();
			return;
		}

		foreach (string action in new[] { "jump", "move_right", "move_left", "duck" })
		{
			if (action == _listeningAction) continue;
			foreach (InputEvent existing in InputMap.ActionGetEvents(action))
			{
				if (existing.AsText() == @event.AsText())
				{
					var currentEvents = InputMap.ActionGetEvents(_listeningAction);
					InputMap.ActionEraseEvents(action);
					foreach (var e in currentEvents)
						InputMap.ActionAddEvent(action, e);
				}
			}
		}

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
		_moveRightBind.Text = GetFirstKey("move_right");
		_moveLeftBind.Text = GetFirstKey("move_left");
		_duckBind.Text = GetFirstKey("duck");
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

		foreach (string action in new[] { "jump", "move_right", "move_left", "duck" })
		{
			var events = InputMap.ActionGetEvents(action);
			if (events.Count > 0 && events[0] is InputEventKey key)
				config.SetValue("bindings", action, (int)key.PhysicalKeycode);
		}

		config.SetValue("audio", "volume", _volumeSlider.Value);
		config.Save("user://settings.cfg");
	}

	private void LoadSettings()
	{
		var config = new ConfigFile();
		if (config.Load("user://settings.cfg") != Error.Ok) return;

		foreach (string action in new[] { "jump", "move_right", "move_left", "duck" })
		{
			if (!config.HasSectionKey("bindings", action)) continue;
			int keycode = (int)config.GetValue("bindings", action);
			var keyEvent = new InputEventKey();
			keyEvent.PhysicalKeycode = (Key)keycode;
			InputMap.ActionEraseEvents(action);
			InputMap.ActionAddEvent(action, keyEvent);
		}

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
		SoundManager.Instance.PlayButton();
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
		SoundManager.Instance.PlayButton();
		GetTree().Quit();
	}

	private void OnMainMenuPressed()
	{	
		GetTree().Paused = false;
		SoundManager.Instance.PlayButton();
		GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
	}
}
