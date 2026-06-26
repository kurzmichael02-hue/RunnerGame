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
	private float _jumpTimer = 1.2f;
	private float _damageCooldown = 0f;
	private float _shootTimer = 0f;
	private Vector2 _startPosition;
	// Nach Damage-Hit kurz bewegungslos damit der gegner nicht auf dem player kleben bleibt.
	// Funktioniert auch für Charger/Shooter die sonst meinen direction-flip ignorieren.
	private float _stunTimer = 0f;

	private uint _originalLayer;
	private uint _originalMask;

	// Wenn ein gegner seine start-position zu nah am player-respawn hat → reset
	// komplett skippen (lebende bleiben wo sie sind, tote bleiben tot).
	private const float NoRespawnRadius = 600f;

	// Buffer um das Player-Rect für etwas Großzügigkeit beim contact-check.
	private const float ContactBuffer = 6f;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Pausable;
		AddToGroup("enemy");
		_startPosition = Position;
		_originalLayer = CollisionLayer;
		_originalMask = CollisionMask;

		_direction = GD.Randf() > 0.5f ? 1 : -1;
		if (Type == EnemyType.Jumping)
			_jumpTimer = (float)GD.RandRange(0.2, 1.2);
		if (Type == EnemyType.Fast) Speed = 220f;
		if (Type == EnemyType.Charger) Speed = 380f;

		SetCollisionMaskValue(1, true);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_isDead) return;

		// Pit-fall → endgültig despawn
		if (GlobalPosition.Y > 800f)
		{
			QueueFree();
			return;
		}

		_damageCooldown -= (float)delta;

		var playerNode = GetTree().GetFirstNodeInGroup("player") as Player;
		if (playerNode != null && !playerNode.IsDying)
		{
			Rect2 eRect = GetShapeWorldRect(GetNodeOrNull<CollisionShape2D>("CollisionShape2D"));
			Rect2 pRect = GetShapeWorldRect(GetActivePlayerShape(playerNode));

			// Buffer um player-rect für etwas Großzügigkeit (visual contact)
			if (pRect.Size.X > 0f)
				pRect = pRect.Grow(ContactBuffer);

			bool inContact = eRect.Size.X > 0f && pRect.Size.X > 0f && eRect.Intersects(pRect);

			if (inContact && _damageCooldown <= 0f)
			{
				Vector2 eCenter = eRect.Position + eRect.Size * 0.5f;
				Vector2 pCenter = pRect.Position + pRect.Size * 0.5f;
				float dy = eCenter.Y - pCenter.Y;

				if (playerNode.StarInvincibilityActive) { Die(); return; }

				// Stomp: der Spieler muss von oben auf den Gegner kommen. Ein Stomp
				// setzt voraus, dass der Spieler NICHT auf dem Terrain steht (er steht
				// dann auf dem Gegner, daher !IsOnFloor), nach unten kommt und sein
				// Mittelpunkt ueber dem des Gegners liegt. Ohne den IsOnFloor-Check
				// wuerde ein geduckter Spieler am Boden bei seitlichem Kontakt faelschlich
				// einen Stomp ausloesen.
				float playerBottom = pRect.Position.Y + pRect.Size.Y;
				float enemyTop = eRect.Position.Y;
				bool fromAbove = !playerNode.IsOnFloor()
					&& playerBottom <= enemyTop + eRect.Size.Y * 0.6f;
				bool notRising = playerNode.Velocity.Y > -50f;
				if (fromAbove && notRising && dy > 0f)
				{
					playerNode.StompedEnemy = true;
					Die();
					return;
				}

				if (playerNode.IsInvincible) return;

				bool enemyDrop = !IsOnFloor() && Velocity.Y > 20f && dy < 0f;
				if (enemyDrop)
				{
					playerNode.Shake(8f, 0.2f);
					playerNode.Die();
					_damageCooldown = 1.5f;
					return;
				}

				// Side-hit nur wenn player nicht aktiv fällt — sonst wär's ein verfehlter stomp
				if (Mathf.Abs(playerNode.Velocity.Y) > 80f) return;

				playerNode.Die();
				_damageCooldown = 1.5f;
				float knockDir = playerNode.GlobalPosition.X > GlobalPosition.X ? 1f : -1f;
				playerNode.Velocity = new Vector2(knockDir * 280f, -150f);
				// Gegner bekommt direction weg vom player UND stun damit auch Charger/Shooter
				// kurz pausieren und der player wegkommt.
				_direction = (int)(-knockDir);
				_stunTimer = 1.0f;
				return;
			}
		}

		// Stunned nach damage → keine bewegung, gravity läuft trotzdem.
		if (_stunTimer > 0f)
		{
			_stunTimer -= (float)delta;
			Vector2 stunVel = Velocity;
			stunVel.X = 0f;
			if (!IsOnFloor()) stunVel.Y += 1800f * (float)delta;
			Velocity = stunVel;
			MoveAndSlide();
			return;
		}

		Vector2 velocity = Velocity;
		if (!IsOnFloor()) velocity.Y += 1800f * (float)delta;

		// Push-off-head: wenn der gegner direkt über dem player rumhängt → seitlich wegschieben.
		// Verhindert "gegner klebt auf kopf"-glitches egal welcher typ.
		if (playerNode != null)
		{
			Vector2 toPlayer = playerNode.GlobalPosition - GlobalPosition;
			if (Mathf.Abs(toPlayer.X) < 35f && toPlayer.Y > 10f && toPlayer.Y < 100f)
			{
				float pushDir = toPlayer.X >= 0f ? -1f : 1f;
				velocity.X = pushDir * Speed * 1.5f;
				_direction = (int)pushDir;
				Velocity = velocity;
				MoveAndSlide();
				return;
			}
		}

		if (Type == EnemyType.Charger)
		{
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
					velocity.X = Mathf.MoveToward(velocity.X, 0f, Speed * (float)delta);
				}
			}
		}
		else if (Type == EnemyType.Shooter)
		{
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
			velocity.X = Speed * _direction;
			if (Position.X > _startPosition.X + MoveDistance) _direction = -1;
			else if (Position.X < _startPosition.X - MoveDistance) _direction = 1;
		}

		if (Type == EnemyType.Jumping && IsOnFloor())
		{
			_jumpTimer -= (float)delta;
			if (_jumpTimer <= 0f) { velocity.Y = -700f; _jumpTimer = 1.2f; }
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	// ===== Shape helpers =====

	private static CollisionShape2D GetActivePlayerShape(Player p)
	{
		var stand = p.GetNodeOrNull<CollisionShape2D>("StandShape");
		var duck = p.GetNodeOrNull<CollisionShape2D>("DuckShape");
		if (stand != null && !stand.Disabled) return stand;
		if (duck != null && !duck.Disabled) return duck;
		return stand ?? duck;
	}

	// Welt-AABB für eine CollisionShape — handhabt Rectangle, Circle, Capsule.
	private static Rect2 GetShapeWorldRect(CollisionShape2D shape)
	{
		if (shape?.Shape == null) return new Rect2();
		Transform2D t = shape.GlobalTransform;
		Vector2 size;

		if (shape.Shape is RectangleShape2D rect)
			size = rect.Size;
		else if (shape.Shape is CircleShape2D circle)
			size = new Vector2(circle.Radius * 2f, circle.Radius * 2f);
		else if (shape.Shape is CapsuleShape2D capsule)
			size = new Vector2(capsule.Radius * 2f, capsule.Height);
		else
			return new Rect2();

		size *= t.Scale;
		return new Rect2(t.Origin - size * 0.5f, size);
	}

	// ===== Lifecycle =====

	public void Kill()
	{
		if (_isDead) return;
		Die();
	}

	// Wird vom player aufgerufen wenn er respawnt.
	// Wenn die start-position zu nah am player ist: komplett wegmachen damit der player
	// nicht direkt nach respawn wieder stirbt.
	public void ResetToStart()
	{
		var p = GetTree().GetFirstNodeInGroup("player") as Player;
		if (p != null && (_startPosition - p.GlobalPosition).Length() < NoRespawnRadius)
		{
			QueueFree();
			return;
		}

		Position = _startPosition;
		Velocity = Vector2.Zero;
		_direction = GD.Randf() > 0.5f ? 1 : -1;
		_damageCooldown = 0f;
		_shootTimer = 0f;
		_jumpTimer = Type == EnemyType.Jumping ? (float)GD.RandRange(0.2, 1.2) : 1.2f;

		if (_isDead)
		{
			_isDead = false;
			AddToGroup("enemy");
			CollisionLayer = _originalLayer;
			CollisionMask = _originalMask;
			var col = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
			if (col != null) col.Disabled = false;
			SetPhysicsProcess(true);

			Node2D spriteNode = (Node2D)GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D")
							 ?? GetNodeOrNull<Sprite2D>("Sprite2D");
			if (spriteNode != null) spriteNode.Scale = Vector2.One;
			Modulate = Colors.White;
			Visible = true;
		}
	}

	private void Die()
	{
		if (_isDead) return;
		_isDead = true;

		RemoveFromGroup("enemy");
		CollisionLayer = 0;
		CollisionMask = 0;
		GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred("disabled", true);
		SetPhysicsProcess(false);
		SoundManager.Instance.PlayEnemyDeath();

		Node2D spriteNode = (Node2D)GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D")
						 ?? GetNodeOrNull<Sprite2D>("Sprite2D");
		var tween = CreateTween();
		if (spriteNode != null)
			tween.TweenProperty(spriteNode, "scale:y", 0.25f, 0.12f);
		tween.Parallel().TweenProperty(this, "modulate:a", 0f, 0.3f);
		tween.TweenCallback(Callable.From(() => Visible = false));
	}

	private void SpawnProjectile(Player target)
	{
		if (ProjectileScene == null) return;
		if (ProjectileScene.Instantiate() is not Projectile proj) return;

		Vector2 dir = (target.GlobalPosition - GlobalPosition).Normalized();
		proj.Velocity = dir * 280f;
		proj.GlobalPosition = GlobalPosition;
		GetTree().CurrentScene.AddChild(proj);
	}
}
