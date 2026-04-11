using Godot;

public partial class Coin : Area2D
{
	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
		AddToGroup("coin");
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is Player player)
		{
			player.AddScore(1);
			QueueFree();
		}
	}
}
