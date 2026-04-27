using Godot;

public partial class PauseMenu : Control
{
	private HSlider _volumeSlider;
	private Button _jumpBind;
	private Button _moveRightBind;
	private Button _moveLeftBind;
	private Button _duckBind;
	private Button _attackBind;
	private string _listeningAction = null;
	// Blockt valuechanged-feedback während loadsettings den slider initial setzt,
	// sonst gibts einen kurzen volume-jump und doppeltes save am gameload
	private bool _isLoading = false;

	public override void _Ready()
	{
		// Always so the pause menu stays responsive while GetTree().Paused is true
		ProcessMode = ProcessModeEnum.Always;

		// When the pause menu pops up, drop focus on Resume so the player can
		// navigate with arrow keys + enter without reaching for the mouse
		VisibilityChanged += OnVisibilityChanged;

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
		_attackBind = GetNodeOrNull<Button>("MenuPanel/VBoxContainer/HBoxContainer5/AttackBind");

		_jumpBind.Pressed += () => StartListening("jump", _jumpBind);
		_moveRightBind.Pressed += () => StartListening("move_right", _moveRightBind);
		_moveLeftBind.Pressed += () => StartListening("move_left", _moveLeftBind);
		_duckBind.Pressed += () => StartListening("duck", _duckBind);
		if (_attackBind != null)
			_attackBind.Pressed += () => StartListening("attack", _attackBind);
		
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
		SoundManager.Instance.PlayMenuHover();
	}

	private void OnVisibilityChanged()
	{
		if (Visible)
			GetNode<Button>("MenuPanel/VBoxContainer/Resume").GrabFocus();
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

		// If the new key is already bound to another action, swap them instead of double-binding (#54)
		foreach (string action in new[] { "jump", "move_right", "move_left", "duck", "attack" })
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

		foreach (string action in new[] { "jump", "move_right", "move_left", "duck", "attack" })
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

		_isLoading = true;
		foreach (string action in new[] { "jump", "move_right", "move_left", "duck", "attack" })
		{
			if (!config.HasSectionKey("bindings", action)) continue;
			int keycode = (int)config.GetValue("bindings", action);
			// 0 = key.none = corrupte save, project.godot defaults beibehalten
			if (keycode <= 0) continue;
			var keyEvent = new InputEventKey();
			keyEvent.PhysicalKeycode = (Key)keycode;
			InputMap.ActionEraseEvents(action);
			InputMap.ActionAddEvent(action, keyEvent);
		}

		if (config.HasSectionKey("audio", "volume"))
		{
			double volume = (double)config.GetValue("audio", "volume");
			_volumeSlider.Value = volume;
			// linear=0 -> -inf db -> bus crashed. clamp wie in settings.cs
			float linear = (float)(volume / 100.0);
			float db = linear <= 0.001f ? -80f : Mathf.LinearToDb(linear);
			AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"), db);
		}
		_isLoading = false;
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
		if (_isLoading) return;
		// gleiche clamp-logik wie load – sonst liefert linear=0 ein -inf db
		float linear = (float)(value / 100.0);
		float db = linear <= 0.001f ? -80f : Mathf.LinearToDb(linear);
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"), db);
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
		GetTree().ChangeSceneToFile("res://Scenes/Main/MainMenu.tscn");
	}
}
