using Godot;

public partial class StarPowerUp : Area2D
{
	private float _time = 0f;

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	public override void _Process(double delta)
	{
		// Breathing pulse – scale in/out so the star catches the eye
		_time += (float)delta;
		float s = 1f + 0.15f * Mathf.Sin(_time * Mathf.Pi * 3f);
		Scale = new Vector2(s, s);
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is not Player player) return;
		player.ActivateStar();
		QueueFree();
	}
}
