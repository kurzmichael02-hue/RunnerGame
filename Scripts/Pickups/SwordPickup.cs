using Godot;

public partial class SwordPickup : Area2D
{
	private float _time = 0f;

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	public override void _Process(double delta)
	{
		// Subtle rotating sway so the pickup stands out
		_time += (float)delta;
		Rotation = 0.18f * Mathf.Sin(_time * Mathf.Pi * 1.6f);
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is not Player player) return;
		player.AddSwordUse();
		SoundManager.Instance.PlayCoin();
		QueueFree();
	}
}
