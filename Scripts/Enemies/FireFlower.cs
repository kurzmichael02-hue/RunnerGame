using Godot;

public partial class FireFlower : Area2D
{
	private float _time = 0f;

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	public override void _Process(double delta)
	{
		// Gentle sway so it stands out as a pickup
		_time += (float)delta;
		float s = 1f + 0.1f * Mathf.Sin(_time * Mathf.Pi * 2.5f);
		Scale = new Vector2(s, s);
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is not Player player) return;
		player.ActivateFire();
		QueueFree();
	}
}
