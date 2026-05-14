

using Godot;
using System;

public partial class Settings : Control
{
	// =========================
	// UI
	// =========================
	private Button _mainMenuButton;
	private Button _volumeButton;
	private Button _controlsButton;
	private Button _exitButton;

	private ConfirmationDialog _exitDialog;

	// =========================
	// STATE
	// =========================
	private bool _changingScene = false;

	// =========================
	// READY
	// =========================
	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;

		// =========================
		// BUTTONS
		// =========================
		_mainMenuButton =
			GetNode<Button>("MenuPanel/VBoxContainer/MainMenu");

		_volumeButton =
			GetNode<Button>("MenuPanel/VBoxContainer/Volume");

		_controlsButton =
			GetNode<Button>("MenuPanel/VBoxContainer/Controls");

		_exitButton =
			GetNode<Button>("MenuPanel/VBoxContainer/Exit");

		// =========================
		// DIALOG
		// =========================
		_exitDialog =
			GetNode<ConfirmationDialog>("ExitConfirmDialog");

		// =========================
		// EVENTS
		// =========================
		_mainMenuButton.Pressed += OnMainMenuPressed;
		_volumeButton.Pressed += OnVolumePressed;
		_controlsButton.Pressed += OnControlsPressed;
		_exitButton.Pressed += OnExitPressed;

		_exitDialog.Confirmed += OnExitConfirmed;



		// =========================
		// HOVER SOUND
		// =========================
		ConnectHoverRecursive(this);
	}

	// =========================
	// MAIN MENU
	// =========================
	private void OnMainMenuPressed()
	{
		ChangeSceneSafe(
			"res://Scenes/Main/MainMenu.tscn"
		);
	}

	// =========================
	// VOLUME
	// =========================
	private void OnVolumePressed()
	{
		ChangeSceneSafe(
			"res://Scenes/Main/Volume.tscn"
		);
	}

	// =========================
	// CONTROLS
	// =========================
	private void OnControlsPressed()
	{
		ChangeSceneSafe(
			"res://Scenes/Main/Controls.tscn"
		);
	}

	// =========================
	// SAFE SCENE CHANGE
	// =========================
	private void ChangeSceneSafe(string path)
	{
		// mehrfaches Klicken verhindern
		if (_changingScene)
			return;

		_changingScene = true;

		// prüfen ob Node noch existiert
		if (!IsInsideTree())
			return;

		// Sound sicher abspielen
		if (SoundManager.Instance != null)
			SoundManager.Instance.PlayButton();

		// verzögert wechseln
		CallDeferred(
			nameof(DeferredSceneChange),
			path
		);
	}

	// =========================
	// DEFERRED CHANGE
	// =========================
	private void DeferredSceneChange(string path)
	{
		// nochmal absichern
		if (!IsInsideTree())
			return;

		SceneTree tree = GetTree();

		if (tree == null)
			return;

		tree.ChangeSceneToFile(path);
	}

	// =========================
	// EXIT
	// =========================
	private void OnExitPressed()
	{
		if (SoundManager.Instance != null)
			SoundManager.Instance.PlayButton();

		_exitDialog.PopupCentered();
	}

	private void OnExitConfirmed()
	{
		SceneTree tree = GetTree();

		if (tree == null)
			return;

		tree.Quit();
	}

	// =========================
	// HOVER SOUND
	// =========================
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
