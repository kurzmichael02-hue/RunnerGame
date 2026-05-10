using Godot;

public partial class LevelSelection : Control
{
public override void _Ready()
{
	SoundManager.Instance.SwitchMusic(SoundManager.Instance.StartScreenMusic);

	var vbox = GetNode<VBoxContainer>("VBoxContainer");
	foreach (Node child in vbox.GetChildren())
	{
		if (child is Button btn)
			btn.MouseEntered += OnAnyButtonHovered;
	}

	vbox.GetChild<Button>(0).CallDeferred(Button.MethodName.GrabFocus);
}

private void OnAnyButtonHovered() => SoundManager.Instance.PlayMenuHover();

private void _on_level1_pressed()
{
	Player.CurrentLevelPath = "res://Scenes/Levels/Level1.tscn";
	SoundManager.Instance.PlayButton();
	GetTree().ChangeSceneToFile(Player.CurrentLevelPath);
}

private void _on_testLevel_pressed()
{
	Player.CurrentLevelPath = "res://Scenes/Levels/TestLevel.tscn";
	SoundManager.Instance.PlayButton();
	GetTree().ChangeSceneToFile(Player.CurrentLevelPath);
}

private void _on_exit_pressed()
{
	SoundManager.Instance.PlayButton();
	GetTree().ChangeSceneToFile("res://Scenes/Main/MainMenu.tscn");
}
}
