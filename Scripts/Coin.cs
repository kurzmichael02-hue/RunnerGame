using Godot;

public partial class Coin : Area2D
{
	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is Player)
		{
			GameManager gameManager = GetNode<GameManager>("/root/Node2D");
			gameManager.AddScore(1);
			QueueFree();
		}
	}
}
