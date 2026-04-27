using Godot;

public enum EnemyType { Patrol, Fast, Jumping, Charger, Shooter }

public partial class Enemy : CharacterBody2D
{
	[Export] public EnemyType Type = EnemyType.Patrol;
	[Export] public float Speed = 100f;
	[Export] public float MoveDistance = 200f;
	[Export] public PackedScene ProjectileScene;
	[Export] public float ShootInterval = 2f;

	private bool _isDead = false;
	private int _direction = 1;
	private float _jumpTimer = 0f;
	private float _damageCooldown = 0f;
	private float _shootTimer = 0f;
	private Vector2 _startPosition;

	public override void _Ready()
{
	// Pausable so ESC freezes enemies with the rest of the world
	ProcessMode = ProcessModeEnum.Pausable;
	// "enemy" group lets the player's attack find us cheaply
	AddToGroup("enemy");
	_startPosition = Position;
	// Random initial patrol direction so not every enemy starts the same way
	_direction = GD.Randf() > 0.5f ? 1 : -1;
		if (Type == EnemyType.Fast) Speed = 220f;
		// Charger sprints when it sees the player, so give it a higher top speed
		if (Type == EnemyType.Charger) Speed = 380f;

		// HitBox war als shapecast2d gecasted, in der scene ist's aber area2d -
		// wurde sowieso nirgends abgefragt, kollision läuft über dx/dy weiter unten
		SetCollisionMaskValue(1, true);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_isDead) return;

		// Enemy patrolled/got knocked into a pit – despawn instead of falling forever
		if (GlobalPosition.Y > 800f)
		{
			QueueFree();
			return;
		}

		_damageCooldown -= (float)delta;

		var playerNode = GetTree().GetFirstNodeInGroup("player") as Player;
		// Skip damage entirely while the player is dying or in i-frames after a respawn,
		// otherwise enemies that happened to be standing on the checkpoint kill instantly
		if (playerNode != null && !playerNode.IsDying && !playerNode.IsInvincible)
		{
			// Split into horizontal + vertical thresholds for cleaner contact detection.
			// dy > 0 = enemy below player, dy < 0 = enemy above player.
			float dx = Mathf.Abs(GlobalPosition.X - playerNode.GlobalPosition.X);
			float dy = GlobalPosition.Y - playerNode.GlobalPosition.Y;
			bool inContact = dx < 48f && Mathf.Abs(dy) < 58f;

			if (inContact && _damageCooldown <= 0f)
			{
				// Star-powered player plows through enemies on contact (#84)
				if (playerNode.StarInvincibilityActive)
				{
					Die();
					return;
				}

				// Player stomps enemy from above (classic Mario stomp) (#88)
				bool playerStomp = playerNode.Velocity.Y > 100f && dy > 6f;
				if (playerStomp)
				{
					playerNode.StompedEnemy = true;
					Die();
					return;
				}

				// Enemy lands on player (jumping enemy falling on top) – player dies.
				// Side / horizontal contact also goes through this branch.
				playerNode.Die();
				_damageCooldown = 1.5f;
				return;
			}
		}

		Vector2 velocity = Velocity;
		if (!IsOnFloor()) velocity.Y += 1800f * (float)delta;

		if (Type == EnemyType.Charger)
		{
			// Lurk-and-lunge – no patrolling, wait until the player is close, then sprint at them
			if (playerNode != null)
			{
				float distX = playerNode.GlobalPosition.X - GlobalPosition.X;
				if (Mathf.Abs(distX) < 350f)
				{
					_direction = distX > 0 ? 1 : -1;
					velocity.X = Speed * _direction;
				}
				else
				{
					// Coast to a stop when out of range, don't slide forever
					velocity.X = Mathf.MoveToward(velocity.X, 0f, Speed * (float)delta);
				}
			}
		}
		else if (Type == EnemyType.Shooter)
		{
			// Stands still and fires projectiles toward the player on an interval
			velocity.X = 0f;
			_shootTimer -= (float)delta;
			if (_shootTimer <= 0f && playerNode != null)
			{
				SpawnProjectile(playerNode);
				_shootTimer = ShootInterval;
			}
		}
		else
		{
			// Patrol – walk back and forth between _startPosition ± MoveDistance
			velocity.X = Speed * _direction;
			if (Position.X > _startPosition.X + MoveDistance) _direction = -1;
			else if (Position.X < _startPosition.X - MoveDistance) _direction = 1;
		}

		// Jumping type fires a jump every 1.2s while on the floor
		if (Type == EnemyType.Jumping && IsOnFloor())
		{
			_jumpTimer -= (float)delta;
			if (_jumpTimer <= 0f) { velocity.Y = -700f; _jumpTimer = 1.2f; }
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	// Public entry for outside damage sources (player sword attack, future projectiles, etc)
	public void Kill()
	{
		if (_isDead) return;
		Die();
	}

	private void Die()
	{
		_isDead = true;
		GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred("disabled", true);
		SetPhysicsProcess(false);
		SoundManager.Instance.PlayEnemyDeath();

		// Squash + fade instead of instant disappear so stomps feel satisfying
		var sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
		var tween = CreateTween();
		if (sprite != null)
			tween.TweenProperty(sprite, "scale:y", sprite.Scale.Y * 0.25f, 0.12f);
		tween.Parallel().TweenProperty(this, "modulate:a", 0f, 0.3f);
		tween.TweenCallback(Callable.From(() => QueueFree()));
	}

	private void SpawnProjectile(Player target)
	{
		if (ProjectileScene == null) return;
		if (ProjectileScene.Instantiate() is not Projectile proj) return;

		// Aim at the player from our current position, constant muzzle speed
		Vector2 dir = (target.GlobalPosition - GlobalPosition).Normalized();
		proj.Velocity = dir * 280f;
		proj.GlobalPosition = GlobalPosition;
		// Add to the scene root so it survives if this enemy gets stomped mid-flight
		GetTree().CurrentScene.AddChild(proj);
	}
	
}
