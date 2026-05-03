using Godot;

public partial class PauseMenu : CanvasLayer {
	
	private HSlider _volumeSlider;
	private Button _jumpBind;
	private Button _moveRightBind;
	private Button _moveLeftBind;
	private Button _duckBind;
	private Button _attackBind;
	private string _listeningAction = null;
	private Button _activeBindButton = null;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;

		// Blockiert Klicks auf das Spiel dahinter
		GetNode<Control>("MenuPanel").MouseFilter = Control.MouseFilterEnum.Stop;
		SetProcessInput(true);
		

		GetNode<Button>("MenuPanel/VBoxContainer2/Resume").Pressed += OnResumePressed;
		GetNode<Button>("MenuPanel/VBoxContainer2/Exit").Pressed += OnExitPressed;
		GetNode<Button>("MenuPanel/VBoxContainer2/MainMenu").Pressed += OnMainMenuPressed;

		_volumeSlider = GetNode<HSlider>("MenuPanel/VBoxContainer2/HSlider");
		_volumeSlider.MinValue = 0;
		_volumeSlider.MaxValue = 100;
		_volumeSlider.ValueChanged += OnVolumeChanged;

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
	}

	private void EnableKeyListening()
	{
		SetProcessUnhandledKeyInput(true);
	}

	public override void _Input(InputEvent @event) {
		
		// ESC im Menü → zurück ins Spiel
		if (@event.IsActionPressed("ui_cancel") && Visible)
		{
			if (_listeningAction == null)
			{
				GetViewport().SetInputAsHandled(); //
				OnResumePressed();
				return;
			}
		}
		
		// AB HIER: NUR Keybinding
		if (_listeningAction == null) return;
		if (@event is not InputEventKey keyEvent || !keyEvent.Pressed || keyEvent.Echo) return;

		GetViewport().SetInputAsHandled(); 
		if (keyEvent.PhysicalKeycode == Key.Escape)	{
			CancelListening();
			return;
		}

		// If the new key is already bound to another action, swap them instead of double-binding (#54)
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

		InputMap.ActionEraseEvents(_listeningAction);
		
		var newEvent = new InputEventKey();
		newEvent.Keycode = keyEvent.Keycode;
		newEvent.PhysicalKeycode = keyEvent.PhysicalKeycode;
		InputMap.ActionAddEvent(_listeningAction, newEvent);

		_listeningAction = null;
		_activeBindButton = null;
		SetProcessUnhandledKeyInput(false);
		UpdateBindLabels();
		SaveSettings();
		GetViewport().SetInputAsHandled();
	}

	private void CancelListening()
	{
		_listeningAction = null;
		_activeBindButton = null;
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
			{
				var keycode = key.PhysicalKeycode != Key.None 
					? key.PhysicalKeycode 
					: key.Keycode;
				config.SetValue("bindings", action, (int)keycode);
			}
		}
		config.SetValue("audio", "volume", _volumeSlider.Value);

		var result = config.Save("user://settings.cfg");
	}


	private void LoadSettings()
	{
		var config = new ConfigFile();
		if (config.Load("user://settings.cfg") != Error.Ok) return;

		foreach (string action in new[] { "jump", "move_right", "move_left", "duck", "attack" })
		{
			if (!config.HasSectionKey("bindings", action)) continue;
			int keycode = (int)config.GetValue("bindings", action);
			var keyEvent = new InputEventKey();
			keyEvent.Keycode = (Key)keycode;
			keyEvent.PhysicalKeycode = (Key)keycode;
			InputMap.ActionEraseEvents(action);
			InputMap.ActionAddEvent(action, keyEvent);
		}

		if (config.HasSectionKey("audio", "volume"))
		{
			double volume = (double)config.GetValue("audio", "volume");
			_volumeSlider.Value = volume;
			// linear=0 gibt -inf db, der bus geht dann kaputt. clamp wie settings.cs
			float linear = (float)(volume / 100.0);
			float db = linear <= 0.001f ? -80f : Mathf.LinearToDb(linear);
			AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"), db);
		}
	}

	private void OnResumePressed()
	{
		GetViewport().SetInputAsHandled();

		// GameManager steuert alles!
		GetNode<GameManager>("/root/Node2D").TogglePause();

		SoundManager.Instance.PlayButton();
	}

	private void OnVolumeChanged(double value)
	{
		// linear=0 gibt -inf db, der bus geht dann kaputt - clamp drauf
		float linear = (float)(value / 100.0);
		float db = linear <= 0.001f ? -80f : Mathf.LinearToDb(linear);
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"), db);
		SaveSettings();
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
}
