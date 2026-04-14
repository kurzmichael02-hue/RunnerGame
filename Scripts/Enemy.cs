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
	private float _damageCooldown = 0f;
	private Vector2 _startPosition;
	private ShapeCast2D _hitBox;

	public override void _Ready()
	{
		_startPosition = Position;
		_direction = GD.Randf() > 0.5f ? 1 : -1;
		if (Type == EnemyType.Fast) Speed = 220f;

		_hitBox = GetNode<ShapeCast2D>("HitBox");
		_hitBox.TargetPosition = Vector2.Zero;
		_hitBox.ExcludeParent = true;
		_hitBox.CollideWithBodies = true;
		_hitBox.CollideWithAreas = false;
		_hitBox.Enabled = true;

		SetCollisionMaskValue(1, true);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_isDead) return;

		_damageCooldown -= (float)delta;

		var playerNode = GetTree().GetFirstNodeInGroup("player") as Player;
		if (playerNode != null && !playerNode.IsDying)
		{
			float dist = GlobalPosition.DistanceTo(playerNode.GlobalPosition);
			if (dist < 60f && _damageCooldown <= 0f)
			{
				bool stompedFromAbove = playerNode.Velocity.Y > 100f
					&& (playerNode.GlobalPosition.Y + 20f) < GlobalPosition.Y;

				if (stompedFromAbove)
				{
					playerNode.StompedEnemy = true;
					Die();
					return;
				}

				playerNode.Die();
				_damageCooldown = 1.5f;
				return;
			}
		}

		// Movement
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

	private void Die()
	{
		_isDead = true;
		_hitBox.Enabled = false;
		GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred("disabled", true);
		SetPhysicsProcess(false);
		SoundManager.Instance.PlayEnemyDeath();
		QueueFree();
	}
}
