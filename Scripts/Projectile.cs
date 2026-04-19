using Godot;

public partial class Projectile : Area2D
{
	[Export] public Vector2 Velocity = new Vector2(-250, 0);
	[Export] public float Lifetime = 3f;
	private float _elapsed = 0f;

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	public override void _PhysicsProcess(double delta)
	{
		_elapsed += (float)delta;
		// Projectile cleans itself up after its lifetime so we don't leak nodes
		if (_elapsed >= Lifetime)
		{
			QueueFree();
			return;
		}
		Position += Velocity * (float)delta;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is not Player player) return;
		if (player.IsDying) return;
		// Star plows through projectiles – projectile dies, player keeps going
		if (player.StarActive)
		{
			QueueFree();
			return;
		}
		player.Die();
		QueueFree();
	}
}
