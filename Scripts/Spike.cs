using Godot;

public partial class Spike : Area2D
{
	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is not Player player) return;
		if (player.IsDying) return;

		// Spikes always cost a life, even when big – like falling off the map (#105)
		player.CallDeferred(nameof(Player.DieFall));
	}
}
