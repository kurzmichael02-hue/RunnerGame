using Godot;

public partial class LevelGoal : Area2D
{
	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is not Player) return;

		GD.Print("Level complete!");
		GetTree().Paused = true;
	}
}
