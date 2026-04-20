using Godot;

public partial class HeartPickup : Area2D
{
	private float _time = 0f;

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	public override void _Process(double delta)
	{
		// Gentle bob + pulse so it reads as a pickup, not background decoration
		_time += (float)delta;
		Scale = Vector2.One * (1f + 0.08f * Mathf.Sin(_time * Mathf.Pi * 2f));
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is not Player player) return;
		player.AddLife();
		SoundManager.Instance.PlayCoin();
		QueueFree();
	}
}
