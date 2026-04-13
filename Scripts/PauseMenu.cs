using Godot;

public partial class PauseMenu : Control
{
	private HSlider _volumeSlider;
	private Button _jumpBind;
	private Button _moveLeftBind;
	private Button _moveRightBind;
	private string _listeningAction = null;
	private bool _justRebound = false;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;

		GetNode<Button>("MenuPanel/VBoxContainer/Resume").Pressed += OnResumePressed;
		GetNode<Button>("MenuPanel/VBoxContainer/Exit").Pressed += OnExitPressed;

		_volumeSlider = GetNode<HSlider>("MenuPanel/VBoxContainer/HSlider");
		_volumeSlider.MinValue = 0;
		_volumeSlider.MaxValue = 100;
		_volumeSlider.Value = 100;
		_volumeSlider.ValueChanged += OnVolumeChanged;

		_jumpBind = GetNode<Button>("MenuPanel/VBoxContainer/HBoxContainer/JumpBind");
		_moveLeftBind = GetNode<Button>("MenuPanel/VBoxContainer/HBoxContainer2/MoveLeftBind");
		_moveRightBind = GetNode<Button>("MenuPanel/VBoxContainer/HBoxContainer3/MoveRightBind");

		_jumpBind.Pressed += () => StartListening("jump", _jumpBind);
		_moveLeftBind.Pressed += () => StartListening("move_left", _moveLeftBind);
		_moveRightBind.Pressed += () => StartListening("move_right", _moveRightBind);
		
		
		LoadKeyBindings();
		UpdateBindLabels();
	}
	
	private void SaveKeyBindings()
{
	var config = new ConfigFile();
	foreach (string action in new[] { "jump", "move_left", "move_right" })
	{
		var events = InputMap.ActionGetEvents(action);
		if (events.Count > 0 && events[0] is InputEventKey key)
			config.SetValue("bindings", action, (int)key.PhysicalKeycode);
	}
	config.Save("user://keybindings.cfg");
}

private void LoadKeyBindings()
{
	var config = new ConfigFile();
	if (config.Load("user://keybindings.cfg") != Error.Ok) return;

	foreach (string action in new[] { "jump", "move_left", "move_right" })
	{
		if (!config.HasSectionKey("bindings", action)) continue;
		int keycode = (int)config.GetValue("bindings", action);
		var keyEvent = new InputEventKey();
		keyEvent.PhysicalKeycode = (Key)keycode;
		InputMap.ActionEraseEvents(action);
		InputMap.ActionAddEvent(action, keyEvent);
	}
	UpdateBindLabels();
}

	private void StartListening(string action, Button button)
	{
		_listeningAction = action;
		button.Text = "Press a key...";
		SetProcessUnhandledKeyInput(true);
	}

	public override void _UnhandledKeyInput(InputEvent @event)
	{
		if (_listeningAction == null) return;
		if (@event is not InputEventKey keyEvent || !keyEvent.Pressed || keyEvent.Echo) return;
		if (_justRebound) return;

		if (keyEvent.Keycode == Key.Escape)
		{
			_listeningAction = null;
			SetProcessUnhandledKeyInput(false);
			UpdateBindLabels();
			SaveKeyBindings();
			return;
		}

		// Swap if another action already uses this key
		foreach (string action in new[] { "jump", "move_left", "move_right" })
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

		_justRebound = true;
		_listeningAction = null;
		SetProcessUnhandledKeyInput(false);
		UpdateBindLabels();
		SaveKeyBindings();
		GetViewport().SetInputAsHandled();
	}

	public override void _Process(double delta)
	{
		_justRebound = false;
	}

	private void UpdateBindLabels()
	{
		_jumpBind.Text = GetFirstKey("jump");
		_moveLeftBind.Text = GetFirstKey("move_left");
		_moveRightBind.Text = GetFirstKey("move_right");
	}

	private string GetFirstKey(string action)
	{
		var events = InputMap.ActionGetEvents(action);
		foreach (var e in events)
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
	}

	private void OnExitPressed()
	{
		GetTree().Quit();
	}

}
