using Godot;

public partial class Coin : Area2D
{
	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
		// "coin" group is used by the magnet power-up to pull coins toward the player
		AddToGroup("coin");
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is Player player)
		{
			player.AddScore(1);
			SoundManager.Instance.PlayCoin();
			QueueFree();
		}
	}
}
