
using Godot;
using System;

public partial class PauseMenu : CanvasLayer
{
	private Button _resumeButton;
	private Button _mainMenuButton;
	private Button _volumeButton;
	private Button _controlsButton;
	private Button _exitButton;
	private Volume _volumeScript;
	
	private Controls _controlsScript;

	private ConfirmationDialog _exitDialog;
	
	private Control _pausePanel;
	private Control _volumePanel;
	private Control _controlsPanel;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		

		GetNode<Control>("MenuPanel").MouseFilter =
			Control.MouseFilterEnum.Stop;

		_resumeButton =
			GetNode<Button>("MenuPanel/VBoxContainer/Resume");

		_mainMenuButton =
			GetNode<Button>("MenuPanel/VBoxContainer/MainMenu");

		_volumeButton =
			GetNode<Button>("MenuPanel/VBoxContainer/Volume");

		_controlsButton =
			GetNode<Button>("MenuPanel/VBoxContainer/Controls");

		_exitButton =
			GetNode<Button>("MenuPanel/VBoxContainer/Exit");

		_exitDialog =
			GetNode<ConfirmationDialog>("ExitConfirmDialog");

		_pausePanel =
	GetNode<Control>("MenuPanel");

		_volumePanel =
			GetNode<Control>("Volume");

		_controlsPanel =
			GetNode<Control>("Controls");
			
		_controlsScript =
			GetNode<Controls>("Controls");
					
		_pausePanel.Visible = true;
		_volumePanel.Visible = false;
		_controlsPanel.Visible = false;
		
		_volumeScript =
			GetNode<Volume>("Volume");
			
		_exitDialog.ProcessMode =
			ProcessModeEnum.Always;

		_exitDialog.Hide();

		_resumeButton.Pressed += OnResumePressed;
		_mainMenuButton.Pressed += OnMainMenuPressed;
		_volumeButton.Pressed += OnVolumePressed;
		_controlsButton.Pressed += OnControlsPressed;
		_exitButton.Pressed += OnExitPressed;

		_exitDialog.Confirmed +=
			OnExitConfirmDialogConfirmed;
			
			
	

		ConnectHoverRecursive(GetNode("MenuPanel"));
		
		
	}

	private void ConnectHoverRecursive(Node node)
	{
		foreach (Node child in node.GetChildren())
		{
			if (child is Button button)
				button.MouseEntered += OnButtonHover;

			ConnectHoverRecursive(child);
		}
	}
private void OnControlsPressed()
{
	SoundManager.Instance.PlayButton();

	_controlsScript.PreviousPanel =
		_pausePanel;

	_pausePanel.Visible = false;
	_volumePanel.Visible = false;
	_controlsPanel.Visible = true;
}


	private void OnButtonHover()
	{
		SoundManager.Instance.PlayMenuHover();
	}

private void OnResumePressed()
{
	SoundManager.Instance.PlayButton();

	SoundManager.Instance.SwitchMusic(
		SoundManager.Instance.GameMusic
	);

	_pausePanel.Visible = true;
	_volumePanel.Visible = false;
	_controlsPanel.Visible = false;



	GetTree().Paused = false;

	Hide();
}
private async void OnMainMenuPressed()
{
	SoundManager.Instance.PlayButton();

	SoundManager.Instance.SwitchMusic(
		SoundManager.Instance.StartScreenMusic
	);

	_pausePanel.Visible = true;
	_volumePanel.Visible = false;
	_controlsPanel.Visible = false;
	


	GetTree().Paused = false;

	Hide();

	await ToSignal(
		GetTree(),
		SceneTree.SignalName.ProcessFrame
	);

	GetTree().ChangeSceneToFile(
		"res://Scenes/Main/MainMenu.tscn"
	);
}
private void OnVolumePressed()
{
	SoundManager.Instance.PlayButton();

	_volumeScript.PreviousPanel =
		_pausePanel;

	_pausePanel.Visible = false;
	_volumePanel.Visible = true;
	_controlsPanel.Visible = false;
}



public override void _ExitTree()
{
	GetTree().Paused = false;
}

	private void OnExitPressed()
	{
		SoundManager.Instance.PlayButton();

		_exitDialog.PopupCentered();
	}

	private void OnExitConfirmDialogConfirmed()
	{
		SoundManager.Instance.PlayButton();

		GetTree().Quit();
	}

	public override void _UnhandledInput(InputEvent @event)
{
	if (!Visible)
		return;

	if (!@event.IsActionPressed("ui_cancel"))
		return;

	GetViewport().SetInputAsHandled();

	// Wenn Volume offen → zurück
	if (_volumePanel.Visible)
	{
		_volumePanel.Visible = false;
		_pausePanel.Visible = true;
		return;
	}

	// Wenn Controls offen → zurück
	if (_controlsPanel.Visible)
	{
		_controlsPanel.Visible = false;
		_pausePanel.Visible = true;
		return;
	}

	// Nur im Hauptmenü pausieren schließen
	OnResumePressed();
}
}
