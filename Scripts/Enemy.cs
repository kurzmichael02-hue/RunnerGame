using Godot;
using System.Collections.Generic;

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
	private readonly HashSet<Player> _overlappingPlayers = new();

	public override void _Ready()
	{
		// Disable physical collision with player, only HitBox handles interaction
		SetCollisionMaskValue(1, true);  // Ground
		_startPosition = Position;
		_direction = GD.Randf() > 0.5f ? 1 : -1;
		if (Type == EnemyType.Fast) Speed = 220f;

		var hitBox = GetNode<Area2D>("HitBox");
		hitBox.BodyEntered += OnBodyEntered;
		hitBox.BodyExited += OnBodyExited;
	}

	private void OnBodyEntered(Node2D body)
{

	if (body is Player player)
		_overlappingPlayers.Add(player);
}

	private void OnBodyExited(Node2D body)
	{
		if (body is Player player)
			_overlappingPlayers.Remove(player);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_isDead) return;

		_damageCooldown -= (float)delta;

		foreach (Player player in _overlappingPlayers)
		{
			if (!IsInstanceValid(player)) continue;

			// Stomp: Player muss fallen UND sein Fuss muss über Enemy Mitte sein
bool stompedFromAbove = player.Velocity.Y > 100f
	&& (player.GlobalPosition.Y + 20f) < GlobalPosition.Y;

			if (stompedFromAbove)
			{
				player.StompedEnemy = true;
				Die();
				return;
			}
			else if (_damageCooldown <= 0f)
			{
				player.Die();
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

	private async void Die()
	{
		_isDead = true;

		GetNode<Area2D>("HitBox").SetDeferred("monitoring", false);
		GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred("disabled", true);
		SetPhysicsProcess(false);
		SoundManager.Instance.PlayEnemyDeath();
		QueueFree();
	}
}
