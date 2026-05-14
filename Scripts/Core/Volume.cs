using Godot;
using System;

public partial class Volume : Control
{
	private Button _backButton;
	private Button _resetButton;
	public Control PreviousPanel;

	private HSlider _masterSlider;
	private HSlider _musicSlider;
	private HSlider _gameFxSlider;
	private HSlider _menuFxSlider;

	private Button _masterBtn;
	private Button _musicBtn;
	private Button _gameFxBtn;
	private Button _menuFxBtn;

	private bool _isLoading = false;

	private bool _masterMuted = false;
	private bool _musicMuted = false;
	private bool _gameFxMuted = false;
	private bool _menuFxMuted = false;

	private float _lastMaster = 100;
	private float _lastMusic = 100;
	private float _lastGameFx = 100;
	private float _lastMenuFx = 100;

	private const string SAVE_PATH = "user://settings.cfg";

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;

		// =========================
		// BUTTONS
		// =========================

		_backButton = GetNode<Button>("Back");
		_resetButton = GetNode<Button>("Reset");

		_masterBtn = GetNode<Button>("VBoxContainer2/HBox1/MasterButton");
		_musicBtn = GetNode<Button>("VBoxContainer2/HBoxC2/MusicButton");
		_gameFxBtn = GetNode<Button>("VBoxContainer2/HBoxC3/GameFXButton");
		_menuFxBtn = GetNode<Button>("VBoxContainer2/HBoxC4/MenuFXButton");

		// =========================
		// SLIDERS
		// =========================

		_masterSlider = GetNode<HSlider>("VBoxContainer2/HBox1/MasterSlider");
		_musicSlider = GetNode<HSlider>("VBoxContainer2/HBoxC2/MusicSlider");
		_gameFxSlider = GetNode<HSlider>("VBoxContainer2/HBoxC3/GameFXSlider");
		_menuFxSlider = GetNode<HSlider>("VBoxContainer2/HBoxC4/MenuFXSlider");

		// =========================
		// BUTTON EVENTS
		// =========================

		_backButton.Pressed += OnBackPressed;
	_resetButton.Pressed += () =>
{
	SoundManager.Instance.PlayButton();
	OnResetPressed();
};
_masterBtn.Pressed += async () =>
{
	bool wasMuted = _masterMuted;

	if (!wasMuted)
	{
		SoundManager.Instance.PlayButton();

		await ToSignal(
			GetTree().CreateTimer(0.08f),
			SceneTreeTimer.SignalName.Timeout
		);

		ToggleMaster();
	}
	else
	{
		ToggleMaster();

		SoundManager.Instance.PlayButton();
	}
};

_musicBtn.Pressed += () =>
{
	ToggleMusic();
	SoundManager.Instance.PlayButton();
};

_gameFxBtn.Pressed += () =>
{
	SoundManager.Instance.PlayButton();
	ToggleGameFx();
};
_menuFxBtn.Pressed += async () =>
{
	bool wasMuted = _menuFxMuted;

	if (!wasMuted)
	{
		SoundManager.Instance.PlayButton();

		await ToSignal(
			GetTree().CreateTimer(0.08f),
			SceneTreeTimer.SignalName.Timeout
		);

		ToggleMenuFx();
	}
	else
	{
		ToggleMenuFx();
		SoundManager.Instance.PlayButton();
	}
};

		// =========================
		// SLIDER EVENTS
		// =========================

		_masterSlider.ValueChanged += OnMasterChanged;
		_musicSlider.ValueChanged += OnMusicChanged;
		_gameFxSlider.ValueChanged += OnGameFxChanged;
		_menuFxSlider.ValueChanged += OnMenuFxChanged;

		ConnectHoverRecursive(this);

		LoadSettings();
	}

	


	private void OnMasterChanged(double value)
	{
		if (_isLoading)
			return;

		SoundManager.Instance.SetMasterVolume((float)value);

		_masterMuted = value == 0;

		UpdateButtonState(_masterBtn, _masterMuted, "Master Volume", "Master");

		SaveSettings();
	}

	private void OnMusicChanged(double value)
	{
		if (_isLoading)
			return;

		SoundManager.Instance.SetMusicVolume((float)value);

		if (_musicSlider.Editable)
			_musicMuted = value == 0;

		UpdateButtonState(_musicBtn, _musicMuted, "Music Volume", "Music");

		SaveSettings();
	}

	private void OnGameFxChanged(double value)
	{
		if (_isLoading)
			return;

		SoundManager.Instance.SetGameFxVolume((float)value);

		if (_gameFxSlider.Editable)
			_gameFxMuted = value == 0;

		UpdateButtonState(_gameFxBtn, _gameFxMuted, "Sound Effects", "Sound");

		SaveSettings();
	}

	private void OnMenuFxChanged(double value)
	{
		if (_isLoading)
			return;

		SoundManager.Instance.SetMenuFxVolume((float)value);

		if (_menuFxSlider.Editable)
			_menuFxMuted = value == 0;

		UpdateButtonState(_menuFxBtn, _menuFxMuted, "Menu Sounds", "Menu");

		SaveSettings();
	}

	// =========================================================
	// TOGGLE MASTER
	// =========================================================

	private void ToggleMaster()
	{
		if (!_masterMuted)
		{
			_lastMaster = (float)_masterSlider.Value;
			_lastMusic = (float)_musicSlider.Value;
			_lastGameFx = (float)_gameFxSlider.Value;
			_lastMenuFx = (float)_menuFxSlider.Value;

			_isLoading = true;

			_masterSlider.Editable = false;
			_musicSlider.Editable = false;
			_gameFxSlider.Editable = false;
			_menuFxSlider.Editable = false;

			_masterSlider.Value = 0;
			_musicSlider.Value = 0;
			_gameFxSlider.Value = 0;
			_menuFxSlider.Value = 0;

			_isLoading = false;

			SoundManager.Instance.SetMasterVolume(0);
			SoundManager.Instance.SetMusicVolume(0);
			SoundManager.Instance.SetGameFxVolume(0);
			SoundManager.Instance.SetMenuFxVolume(0);

			_masterMuted = true;
			_musicMuted = true;
			_gameFxMuted = true;
			_menuFxMuted = true;
		}
		else
		{
			_isLoading = true;

			_masterSlider.Editable = true;
			_musicSlider.Editable = true;
			_gameFxSlider.Editable = true;
			_menuFxSlider.Editable = true;

			_masterSlider.Value = _lastMaster;
			_musicSlider.Value = _lastMusic;
			_gameFxSlider.Value = _lastGameFx;
			_menuFxSlider.Value = _lastMenuFx;

			_isLoading = false;

			SoundManager.Instance.SetMasterVolume(_lastMaster);
			SoundManager.Instance.SetMusicVolume(_lastMusic);
			SoundManager.Instance.SetGameFxVolume(_lastGameFx);
			SoundManager.Instance.SetMenuFxVolume(_lastMenuFx);

			_masterMuted = false;
			_musicMuted = false;
			_gameFxMuted = false;
			_menuFxMuted = false;
		}

		UpdateAllButtons();

		SaveSettings();
	}

	// =========================================================
	// TOGGLE MUSIC
	// =========================================================

	private void ToggleMusic()
	{
		if (!_musicMuted)
		{
			_lastMusic = (float)_musicSlider.Value;

			_isLoading = true;

			_musicSlider.Editable = false;
			_musicSlider.Value = 0;

			_isLoading = false;

			SoundManager.Instance.SetMusicVolume(0);

			_musicMuted = true;
		}
		else
		{
			_isLoading = true;

			_musicSlider.Editable = true;
			_musicSlider.Value = _lastMusic;

			_isLoading = false;

			SoundManager.Instance.SetMusicVolume(_lastMusic);

			_musicMuted = false;
		}

		UpdateButtonState(_musicBtn, _musicMuted, "Music Volume", "Music");

		SaveSettings();
	}

	// =========================================================
	// TOGGLE GAME FX
	// =========================================================

	private void ToggleGameFx()
	{
		if (!_gameFxMuted)
		{
			_lastGameFx = (float)_gameFxSlider.Value;

			_isLoading = true;

			_gameFxSlider.Editable = false;
			_gameFxSlider.Value = 0;

			_isLoading = false;

			SoundManager.Instance.SetGameFxVolume(0);

			_gameFxMuted = true;
		}
		else
		{
			_isLoading = true;

			_gameFxSlider.Editable = true;
			_gameFxSlider.Value = _lastGameFx;

			_isLoading = false;

			SoundManager.Instance.SetGameFxVolume(_lastGameFx);

			_gameFxMuted = false;
		}

		UpdateButtonState(_gameFxBtn, _gameFxMuted, "Sound Effects", "Sound");

		SaveSettings();
	}

	// =========================================================
	// TOGGLE MENU FX
	// =========================================================

	private void ToggleMenuFx()
	{
		if (!_menuFxMuted)
		{
			_lastMenuFx = (float)_menuFxSlider.Value;

			_isLoading = true;

			_menuFxSlider.Editable = false;
			_menuFxSlider.Value = 0;

			_isLoading = false;

			SoundManager.Instance.SetMenuFxVolume(0);

			_menuFxMuted = true;
		}
		else
		{
			_isLoading = true;

			_menuFxSlider.Editable = true;
			_menuFxSlider.Value = _lastMenuFx;

			_isLoading = false;

			SoundManager.Instance.SetMenuFxVolume(_lastMenuFx);

			_menuFxMuted = false;
		}

		UpdateButtonState(_menuFxBtn, _menuFxMuted, "Menu Sounds", "Menu");

		SaveSettings();
	}

	// =========================================================
	// RESET
	// =========================================================

	private void OnResetPressed()
	{
		ApplyDefaults();

		SaveSettings();
	}

	private void ApplyDefaults()
	{
		_isLoading = true;

		_masterSlider.Editable = true;
		_musicSlider.Editable = true;
		_gameFxSlider.Editable = true;
		_menuFxSlider.Editable = true;

		_masterSlider.Value = 100;
		_musicSlider.Value = 100;
		_gameFxSlider.Value = 100;
		_menuFxSlider.Value = 100;

		_isLoading = false;

		SoundManager.Instance.SetMasterVolume(100);
		SoundManager.Instance.SetMusicVolume(100);
		SoundManager.Instance.SetGameFxVolume(100);
		SoundManager.Instance.SetMenuFxVolume(100);

		_masterMuted = false;
		_musicMuted = false;
		_gameFxMuted = false;
		_menuFxMuted = false;

		_lastMaster = 100;
		_lastMusic = 100;
		_lastGameFx = 100;
		_lastMenuFx = 100;

		UpdateAllButtons();
	}

	// =========================================================
	// SAVE / LOAD
	// =========================================================

	private void SaveSettings()
	{
		var config = new ConfigFile();

		config.Load(SAVE_PATH);

		config.SetValue("audio", "master", _masterSlider.Value);
		config.SetValue("audio", "music", _musicSlider.Value);
		config.SetValue("audio", "gamefx", _gameFxSlider.Value);
		config.SetValue("audio", "menufx", _menuFxSlider.Value);

		config.SetValue("muted", "master", _masterMuted);
		config.SetValue("muted", "music", _musicMuted);
		config.SetValue("muted", "gamefx", _gameFxMuted);
		config.SetValue("muted", "menufx", _menuFxMuted);

		config.SetValue("last", "master", _lastMaster);
		config.SetValue("last", "music", _lastMusic);
		config.SetValue("last", "gamefx", _lastGameFx);
		config.SetValue("last", "menufx", _lastMenuFx);

		config.Save(SAVE_PATH);
	}

	private void LoadSettings()
	{
		var config = new ConfigFile();

		_isLoading = true;

		if (config.Load(SAVE_PATH) != Error.Ok)
		{
			ApplyDefaults();

			_isLoading = false;

			return;
		}

		_masterSlider.Value = (double)config.GetValue("audio", "master", 100.0);
		_musicSlider.Value = (double)config.GetValue("audio", "music", 100.0);
		_gameFxSlider.Value = (double)config.GetValue("audio", "gamefx", 100.0);
		_menuFxSlider.Value = (double)config.GetValue("audio", "menufx", 100.0);

		_masterMuted = (bool)config.GetValue("muted", "master", false);
		_musicMuted = (bool)config.GetValue("muted", "music", false);
		_gameFxMuted = (bool)config.GetValue("muted", "gamefx", false);
		_menuFxMuted = (bool)config.GetValue("muted", "menufx", false);

		_lastMaster = (float)(double)config.GetValue("last", "master", 100.0);
		_lastMusic = (float)(double)config.GetValue("last", "music", 100.0);
		_lastGameFx = (float)(double)config.GetValue("last", "gamefx", 100.0);
		_lastMenuFx = (float)(double)config.GetValue("last", "menufx", 100.0);

		_masterSlider.Editable = !_masterMuted;
		_musicSlider.Editable = !_musicMuted;
		_gameFxSlider.Editable = !_gameFxMuted;
		_menuFxSlider.Editable = !_menuFxMuted;

		SoundManager.Instance.SetMasterVolume((float)_masterSlider.Value);
		SoundManager.Instance.SetMusicVolume((float)_musicSlider.Value);
		SoundManager.Instance.SetGameFxVolume((float)_gameFxSlider.Value);
		SoundManager.Instance.SetMenuFxVolume((float)_menuFxSlider.Value);

		_isLoading = false;

		UpdateAllButtons();
	}

	// =========================================================
	// UI
	// =========================================================

	private void UpdateAllButtons()
	{
		UpdateButtonState(_masterBtn, _masterMuted, "Master Volume", "Master");
		UpdateButtonState(_musicBtn, _musicMuted, "Music Volume", "Music");
		UpdateButtonState(_gameFxBtn, _gameFxMuted, "Sound Effects", "Sound");
		UpdateButtonState(_menuFxBtn, _menuFxMuted, "Menu Sounds", "Menu");
	}

	private void UpdateButtonState(Button btn, bool muted, string normalText, string mutedText)
	{
		btn.ButtonPressed = muted;

		btn.Text = muted
			? $"{mutedText} (Muted)"
			: normalText;
	}

	// =========================================================
	// BACK
	// =========================================================

private void OnBackPressed()
{
	SoundManager.Instance.PlayButton();

	// PauseMenu-Modus
	if (PreviousPanel != null)
	{
		PreviousPanel.Visible = true;
		Visible = false;
		return;
	}

	// Standalone-Modus
	GetTree().ChangeSceneToFile(
		"res://Scenes/Main/Settings.tscn"
	);
}


	// =========================================================
	// HOVER
	// =========================================================

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
}
