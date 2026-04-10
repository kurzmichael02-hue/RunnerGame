using Godot;

public enum EnemyType { Patrol, Fast, Jumping }

public partial class Enemy : CharacterBody2D
{
	[Export] public EnemyType Type = EnemyType.Patrol;
	[Export] public float Speed = 100f;
	[Export] public float MoveDistance = 200f;

	private bool _isDead = false;
	private int _direction = 1;
	private float _jumpTimer = 0f;
	private Vector2 _startPosition;

	public override void _Ready()
	{
		_startPosition = Position;
		if (Type == EnemyType.Fast) Speed = 220f;

		GetNode<Area2D>("HitBox").BodyEntered += OnHitBoxBodyEntered;
	}

	private void OnHitBoxBodyEntered(Node2D body)
	{
		GD.Print("HitBox entered by: " + body.Name);
		if (_isDead) return;
		if (body is not Player player) return;

		// Falling down onto enemy = stomp
		if (player.Velocity.Y > 0 && player.GlobalPosition.Y < GlobalPosition.Y)
		{
			player.StompedEnemy = true;
			_isDead = true;
			GetNode<Area2D>("HitBox").SetDeferred("monitoring", false);
			GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred("disabled", true);
			SetPhysicsProcess(false);
			QueueFree();
		}
		else
		{
			player.Die();
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_isDead) return;

		Vector2 velocity = Velocity;
		if (!IsOnFloor()) velocity.Y += 1800f * (float)delta;
		velocity.X = Speed * _direction;

		if (Position.X > _startPosition.X + MoveDistance) _direction = -1;
		else if (Position.X < _startPosition.X - MoveDistance) _direction = 1;

		if (Type == EnemyType.Jumping && IsOnFloor())
		{
			_jumpTimer -= (float)delta;
			if (_jumpTimer <= 0f) { velocity.Y = -700f; _jumpTimer = 1.2f; }
		}

		Velocity = velocity;
		MoveAndSlide();
	}
}
