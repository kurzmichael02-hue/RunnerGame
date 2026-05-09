using Godot;
using System;

public partial class LevelSelection : Control
{
	
	private ConfirmationDialog _exitDialog;
	
public override void _Ready()
{
	SoundManager.Instance.SwitchMusic(SoundManager.Instance.StartScreenMusic);

	var vbox = GetNode<VBoxContainer>("VBoxContainer");

	foreach (Node child in vbox.GetChildren())
	{
		if (child is Button button)
		{
			button.MouseEntered += OnAnyButtonHovered;
		}
	}
	

	// Start button gets focus so arrow keys + enter work without touching the mouse.
	// Deferred because children aren't fully ready yet inside _Ready.
	GetNode<Button>("VBoxContainer/Start").CallDeferred(Button.MethodName.GrabFocus);
}

private void OnAnyButtonHovered()
{
	SoundManager.Instance.PlayMenuHover();
}

	public override void _Process(double delta)
	{
	}

	// Level 1
	private void _on_level1_pressed()
	{
		
		SoundManager.Instance.PlayButton();
		GetTree().ChangeSceneToFile("res://Scenes/Levels/Level1.tscn");
	}

	// Testlevel
	private void _on_testLevel_pressed()
	{
		SoundManager.Instance.PlayButton();
		GetTree().ChangeSceneToFile("res://Scenes/Levels/TestLevel.tscn");
	}

	// EXIT BUTTON
	private void _on_exit_pressed()
	{
		SoundManager.Instance.PlayButton();
		GetTree().ChangeSceneToFile("res://Scenes/Main/MainMenu.tscn");
	}
	

}
