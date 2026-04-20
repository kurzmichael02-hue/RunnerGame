using Godot;

public partial class Projectile : Area2D
{
	[Export] public Vector2 Velocity = new Vector2(-250, 0);
	[Export] public float Lifetime = 3f;
	private float _elapsed = 0f;
	private bool _deflected = false;

	public override void _Ready()
	{
		// Group so the sword swing can find projectiles to deflect without scene walks
		AddToGroup("projectile");
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

	// Sword hit flips the projectile around and re-targets it at enemies.
	// Lifetime resets so a well-timed block can actually make the shot count.
	public void Deflect()
	{
		if (_deflected) return;
		_deflected = true;
		Velocity = -Velocity;
		_elapsed = 0f;
		SetCollisionMaskValue(1, false);
		SetCollisionMaskValue(2, true);
		var poly = GetNodeOrNull<Polygon2D>("Polygon2D");
		if (poly != null) poly.Color = new Color(0.2f, 0.9f, 1f);
		var core = GetNodeOrNull<Polygon2D>("Core");
		if (core != null) core.Color = new Color(1f, 1f, 1f);
	}

	// Called when a fire-flower-boosted player shoots a fireball: hits enemies only,
	// warm orange/yellow palette so it reads different from the red enemy shot
	public void SetAsPlayerShot()
	{
		_deflected = true;
		SetCollisionMaskValue(1, false);
		SetCollisionMaskValue(2, true);
		var poly = GetNodeOrNull<Polygon2D>("Polygon2D");
		if (poly != null) poly.Color = new Color(1f, 0.5f, 0.1f);
		var core = GetNodeOrNull<Polygon2D>("Core");
		if (core != null) core.Color = new Color(1f, 1f, 0.4f);
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is Player player)
		{
			if (player.IsDying) return;
			// Star plows through projectiles – projectile dies, player keeps going
			if (player.StarActive)
			{
				QueueFree();
				return;
			}
			player.Die();
			QueueFree();
			return;
		}
		// A deflected projectile kills enemies on contact – only possible after deflect
		// because collision mask for layer 2 is off by default
		if (body is Enemy enemy)
		{
			enemy.Kill();
			QueueFree();
		}
	}
}
