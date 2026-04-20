using Godot;

public partial class Player : CharacterBody2D
{
	// Movement
	[Export] public float MaxSpeed = 500f;
	[Export] public float Acceleration = 800f;
	[Export] public float Deceleration = 900f;
	[Export] public float AirAcceleration = 1200f;
	[Export] public float TurnAcceleration = 3000f;

	// Jump
	[Export] public float JumpVelocity = -600f;
	[Export] public float JumpGravity = 700f;
	[Export] public float FallGravity = 1800f;
	[Export] public float MaxFallSpeed = 800f;
	[Export] public float CoyoteTime = 0.1f;
	[Export] public float JumpBufferTime = 0.12f;
	[Export] public float JumpHoldTime = 0.3f;
	private float JumpHoldTimer = 0f;

	// State
	private int _lives = 3;
	private int _score = 0;
	// Tracks how many 100-point thresholds have already paid out an extra life,
	// so a +5 bonus that jumps over 100 still grants the life instead of missing the modulo
	private int _livesFromScoreGranted = 0;
	public bool IsDying = false;
	public bool IsSmall = false;
	private CollisionShape2D _standShape;
	private CollisionShape2D _duckShape;
	private bool _shieldActive = false;
	private float _shieldTimer = 0f;
	private bool _starActive = false;
	private bool _starInvincibilityActive = false;
	private float _starTimer = 0f;
	private float _starInvincibilityTimer = 0f;
	private float _starHue = 0f;
	public bool StarActive => _starActive;
	public bool StarInvincibilityActive => _starInvincibilityActive;
	private float _invincibilityTimer = 0f;
	private float _coyoteTimer = 0f;
	private float _jumpBufferTimer = 0f;
	private bool _isJumping = false;
	private bool _isDucking = false;
	private bool _doubleJumpUsed = false;
	private int _stompChain = 0;
	// P-Speed (#102) – charges while running on the ground, gives up to +15% max speed.
	// Nerfed twice after tim's feedback: 40%/2.5s -> 25%/3.5s -> 15%/4.5s (500 -> 575 max).
	// Subtle acceleration reward instead of a dash, keeps controls predictable.
	private float _sprintTimer = 0f;
	private float _lastRunDir = 0f;
	public float SprintCharge => Mathf.Clamp(_sprintTimer / 4.5f, 0f, 1f);

	// Sword attack (#45/53-ish) – j/x to swing. Cooldown + movement lockout so you
	// can't just dash-slash through the level (tim's balance note)
	private float _attackCooldown = 0f;
	private float _attackLockout = 0f;
	private const float AttackCooldownMax = 0.6f;
	public float AttackReadiness => 1f - Mathf.Clamp(_attackCooldown / AttackCooldownMax, 0f, 1f);
	private int _facing = 1;
	// Limited sword swings per life (tim wanted it less spammable)
	// Refill via sword pickups in the level, capped at MaxSwordUses
	private int _swordUses = 3;
	private const int MaxSwordUses = 5;
	public int SwordUses => _swordUses;

	// Fire flower (#45): pickup for 10s, every swing also spits a fireball in facing dir
	private bool _fireActive = false;
	private float _fireTimer = 0f;
	public float FireTimeLeft => _fireActive ? _fireTimer : 0f;
	private PackedScene _projectileScene;

	// Camera shake
	private Camera2D _camera;
	private float _shakeTimer = 0f;
	private float _shakeStrength = 0f;

	// Magnet
	private bool _magnetActive = false;
	private float _magnetTimer = 0f;
	private float _magnetRadius = 200f;

	public int Lives => _lives;
	public int Score => _score;
	// Read-only timers for the HUD so it can show which power-ups are still running
	public float ShieldTimeLeft => _shieldActive ? _shieldTimer : 0f;
	public float MagnetTimeLeft => _magnetActive ? _magnetTimer : 0f;
	public float StarTimeLeft => _starActive ? _starTimer : 0f;
	public bool StompedEnemy = false;
	private Vector2 _checkpointPosition = new Vector2(200, 260);

	public void SetCheckpoint(Vector2 position)
	{
		_checkpointPosition = position;
	}

	public override void _Ready()
	{
		AddToGroup("player");
		// Player does not physically collide with enemies (Layer 2)
		SetCollisionMaskValue(2, false);
		_standShape = GetNode<CollisionShape2D>("StandShape");
_duckShape = GetNode<CollisionShape2D>("DuckShape");
_duckShape.Disabled = true;
		_camera = GetNode<Camera2D>("Camera2D");
		// Preload projectile scene so fireballs don't hitch the first time you swing
		_projectileScene = GD.Load<PackedScene>("res://Scenes/projectile.tscn");
	}

	// Kicked by damage events – camera jitters for `duration` seconds at `strength` pixels
	public void Shake(float strength, float duration)
	{
		_shakeStrength = strength;
		_shakeTimer = duration;
	}

	// Sword swing – kills any enemy within a short arc in front of the player.
	// Placeholder visual until we have a real swing sprite from schayan.
	private void Attack()
	{
		_attackCooldown = AttackCooldownMax;
		// Lockout briefly cripples horizontal speed so you can't power-slash on the run
		_attackLockout = 0.25f;
		_swordUses--;
		SpawnSwordSwoosh();
		SoundManager.Instance.PlayJump();

		// Hitbox: 120px reach in facing direction, 85px tall – bigger to compensate for
		// the reduced attack rate and movement lockout (tim's balance note)
		foreach (Node node in GetTree().GetNodesInGroup("enemy"))
		{
			if (node is not Enemy enemy) continue;
			Vector2 toEnemy = enemy.GlobalPosition - GlobalPosition;
			bool sameSide = Mathf.Sign(toEnemy.X) == _facing || Mathf.Abs(toEnemy.X) < 15f;
			if (sameSide && Mathf.Abs(toEnemy.X) < 120f && Mathf.Abs(toEnemy.Y) < 85f)
				enemy.Kill();
		}

		// Same hitbox also deflects incoming enemy projectiles – timing reward,
		// deflected shot flies back and kills whoever fired it (#53-ish)
		foreach (Node node in GetTree().GetNodesInGroup("projectile"))
		{
			if (node is not Projectile proj) continue;
			Vector2 toProj = proj.GlobalPosition - GlobalPosition;
			bool sameSide = Mathf.Sign(toProj.X) == _facing || Mathf.Abs(toProj.X) < 15f;
			if (sameSide && Mathf.Abs(toProj.X) < 120f && Mathf.Abs(toProj.Y) < 85f)
				proj.Deflect();
		}

		// Fire flower active: swing also spits a fireball in the facing direction (#45)
		if (_fireActive)
			SpawnFireball();
		// Little horizontal nudge on swing so the player feels the follow-through
		Shake(2f, 0.05f);
	}

	private void SpawnFireball()
	{
		if (_projectileScene == null) return;
		if (_projectileScene.Instantiate() is not Projectile proj) return;
		// Orange/yellow palette + layer-2 mask so it kills enemies, not the player
		proj.SetAsPlayerShot();
		proj.Velocity = new Vector2(_facing * 450f, 0f);
		proj.GlobalPosition = GlobalPosition + new Vector2(_facing * 28f, 0f);
		GetTree().CurrentScene.AddChild(proj);
	}

	private void SpawnSwordSwoosh()
	{
		var wrap = new Node2D { GlobalPosition = GlobalPosition + new Vector2(_facing * 35f, 0f) };
		var poly = new Polygon2D
		{
			Color = new Color(1f, 1f, 1f, 0.85f),
			// Crescent-ish arc pointing in facing direction
			Polygon = new Vector2[]
			{
				new Vector2(0, -30), new Vector2(15, -18), new Vector2(25, 0),
				new Vector2(15, 18), new Vector2(0, 30), new Vector2(5, 12),
				new Vector2(10, 0), new Vector2(5, -12)
			}
		};
		if (_facing < 0) poly.Scale = new Vector2(-1, 1);
		wrap.AddChild(poly);
		GetTree().CurrentScene.AddChild(wrap);

		var tween = GetTree().CreateTween();
		tween.TweenProperty(poly, "modulate:a", 0f, 0.18f);
		tween.Parallel().TweenProperty(wrap, "scale", new Vector2(1.3f, 1.3f), 0.18f);
		tween.TweenCallback(Callable.From(() => wrap.QueueFree()));
	}

	// Floating chain label above the player, drifts up and fades. Lives on scene root so
	// player movement doesn't drag it along.
	private void SpawnChainPopup(int chain)
	{
		var wrap = new Node2D { GlobalPosition = GlobalPosition + new Vector2(0, -40) };
		var label = new Label
		{
			Text = $"CHAIN x{chain}!",
			Modulate = new Color(1f, 0.5f, 0.1f),
			Position = new Vector2(-45, -20),
			Scale = Vector2.One * 1.8f
		};
		wrap.AddChild(label);
		GetTree().CurrentScene.AddChild(wrap);

		var tween = GetTree().CreateTween();
		tween.TweenProperty(wrap, "global_position", wrap.GlobalPosition + new Vector2(0, -70), 0.7f);
		tween.Parallel().TweenProperty(wrap, "modulate:a", 0f, 0.7f);
		tween.TweenCallback(Callable.From(() => wrap.QueueFree()));
	}
	// Fall into a pit – always costs a life, no shrink animation.
	// Classic Mario: falling ignores power-up state (#105)
	public async void DieFall()
{
	if (IsDying) return;
	IsDying = true;

	// Reset size in case player was small before falling
	IsSmall = false;
	Scale = Vector2.One;
	_standShape.Shape = new RectangleShape2D { Size = new Vector2(30, 60) };

	// Falling into a pit burns the star – tim's request, feels fair because the pit already
	// bypasses star invincibility so keeping it after respawn would be a free ride.
	// Also clear tim's grace-period flag so you can't respawn with lingering i-frames.
	if (_starActive || _starInvincibilityActive)
	{
		_starActive = false;
		_starInvincibilityActive = false;
		Modulate = Colors.White;
		SoundManager.Instance.SwitchMusic(SoundManager.Instance.GameMusic);
	}

	Shake(10f, 0.35f);

	_lives--;
	_lives = Mathf.Max(_lives, 0);

	if (_lives <= 0)
	{
		SaveHighscore(_score);
		Visible = false;
		SetPhysicsProcess(false);
		GetTree().ChangeSceneToFile("res://Scenes/GameOver.tscn");
		return;
	}

	Visible = false;
	SetPhysicsProcess(false);

	var timer = GetTree().CreateTimer(1.0f, true, false, true);
	await ToSignal(timer, SceneTreeTimer.SignalName.Timeout);

	Position = _checkpointPosition;
	Visible = true;
	SetPhysicsProcess(true);
	_invincibilityTimer = 1.5f;
	IsDying = false;
	ResetAirState();

	var tween = CreateTween();
	tween.SetLoops(6);
	tween.TweenProperty(this, "modulate", new Color(1, 0, 0, 0.3f), 0.1f);
	tween.TweenProperty(this, "modulate", new Color(1, 1, 1, 1f), 0.1f);
}

	// Stale coyote/double-jump/chain state from before death would let the player
	// get a phantom mid-air jump or bonus chain right after respawn. Clear everything.
	private void ResetAirState()
	{
		_coyoteTimer = 0f;
		_jumpBufferTimer = 0f;
		_isJumping = false;
		_doubleJumpUsed = false;
		_stompChain = 0;
		_sprintTimer = 0f;
		_lastRunDir = 0f;
		_attackCooldown = 0f;
		_attackLockout = 0f;
		// Refill sword uses per life – pickups carry over during the life itself
		_swordUses = 3;
		Velocity = Vector2.Zero;
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;
if (Position.Y > 600f && !IsDying)
	DieFall();
		JumpHoldTimer = Mathf.Max(JumpHoldTimer - dt, 0f);
		Vector2 velocity = Velocity;

		// Magnet – pull nearby coins toward player (#40)
		if (_magnetActive)
		{
			_magnetTimer -= dt;
			if (_magnetTimer <= 0f)
				_magnetActive = false;
			else
			{
				foreach (Node node in GetTree().GetNodesInGroup("coin"))
				{
					if (node is Area2D coin)
					{
						Vector2 diff = GlobalPosition - coin.GlobalPosition;
						if (diff.Length() < _magnetRadius)
							coin.GlobalPosition += diff.Normalized() * 300f * dt;
					}
				}
			}
		}

		// Bounce after stomping enemy (#89)
		if (StompedEnemy)
		{
			velocity.Y = JumpVelocity * 0.6f;
			StompedEnemy = false;
			Shake(3f, 0.08f);
			// Stomp chain: each consecutive mid-air stomp gives more score, resets on ground
			_stompChain++;
			AddScore(_stompChain * 2);
			// Show a floating "CHAIN x2!" style label so the player notices the bonus
			if (_stompChain >= 2)
				SpawnChainPopup(_stompChain);
		}

		// Asymmetric gravity – hold jump for higher arc
		float gravity = (velocity.Y < 0 && Input.IsActionPressed("jump") && JumpHoldTimer > 0f)
			? JumpGravity
			: FallGravity;

		if (!IsOnFloor())
			velocity.Y += gravity * dt;
		velocity.Y = Mathf.Min(velocity.Y, MaxFallSpeed);

		// Coyote time – small forgiveness window to jump right after walking off a ledge (#18)
		// Double-jump and stomp-chain reset on ground contact – chain means consecutive mid-air stomps
		if (IsOnFloor())
		{
			_coyoteTimer = CoyoteTime;
			_isJumping = false;
			_doubleJumpUsed = false;
			_stompChain = 0;
		}
		else _coyoteTimer -= dt;
		
		// Duck – only on ground, no jumping while ducking
bool wantDuck = Input.IsActionPressed("duck") && IsOnFloor();

if (wantDuck && !_isDucking)
{
	_isDucking = true;
	_jumpBufferTimer = 0f;
	JumpHoldTimer = 0f;
	_standShape.Disabled = true;
	_duckShape.Disabled = false;
}
else if (!Input.IsActionPressed("duck") && _isDucking)
{
	_isDucking = false;
	_standShape.Disabled = false;
	_duckShape.Disabled = true;
}

if (_isDucking)
{
	float duckDir = 0;
	if (Input.IsActionPressed("move_left")) duckDir = -1;
	else if (Input.IsActionPressed("move_right")) duckDir = 1;

	_jumpBufferTimer = 0f;
	Vector2 vel = Velocity;
	// Ducking slows the player to 40% of max speed – feels heavier, harder to dodge
	vel.X = duckDir * MaxSpeed * 0.4f;
	if (!IsOnFloor()) vel.Y += FallGravity * dt;
	vel.Y = Mathf.Min(vel.Y, MaxFallSpeed);
	Velocity = vel;
	MoveAndSlide();
	return;
}

		// Jump buffer – if the player presses jump slightly before landing, we still queue the jump (#18)
		if (Input.IsActionJustPressed("jump")) _jumpBufferTimer = JumpBufferTime;
		else _jumpBufferTimer = Mathf.Max(_jumpBufferTimer - dt, 0f);

		// Short hop on release
		if (Input.IsActionJustReleased("jump") && velocity.Y < 0)
			velocity.Y *= 0.5f;

		// Execute jump
		bool canJump = IsOnFloor() || (_coyoteTimer > 0f && !_isJumping);
		if (_jumpBufferTimer > 0f && canJump)
		{
			velocity.Y = JumpVelocity;
			_jumpBufferTimer = 0f;
			_coyoteTimer = 0f;
			_isJumping = true;
			JumpHoldTimer = JumpHoldTime;
			SoundManager.Instance.PlayJump();
		}
		// Mid-air jump: wall jump takes priority over double jump if the player is on a wall (#98)
		else if (Input.IsActionJustPressed("jump") && !IsOnFloor() && !_isDucking)
		{
			if (IsOnWall())
			{
				// Wall jump: bounce off the wall, refresh the double-jump as reward
				Vector2 wallNormal = GetWallNormal();
				velocity.X = wallNormal.X * MaxSpeed * 0.85f;
				velocity.Y = JumpVelocity * 0.9f;
				_doubleJumpUsed = false;
				_jumpBufferTimer = 0f;
				JumpHoldTimer = JumpHoldTime * 0.8f;
				SoundManager.Instance.PlayJump();
			}
			else if (!_doubleJumpUsed)
			{
				// Double jump – one extra mid-air jump, weaker so it's not game-breaking
				velocity.Y = JumpVelocity * 0.85f;
				_doubleJumpUsed = true;
				_jumpBufferTimer = 0f;
				JumpHoldTimer = JumpHoldTime * 0.6f;
				SoundManager.Instance.PlayJump();
			}
		}

		// Wall slide – when in air against a wall and pressing into it, cap the fall speed
		// so the player has time to wall-jump away (#98)
		if (!IsOnFloor() && IsOnWall() && velocity.Y > 0f)
		{
			Vector2 wallNormal = GetWallNormal();
			bool pressingIntoWall = (wallNormal.X < 0 && Input.IsActionPressed("move_right"))
				|| (wallNormal.X > 0 && Input.IsActionPressed("move_left"));
			if (pressingIntoWall)
				velocity.Y = Mathf.Min(velocity.Y, 120f);
		}

		// Horizontal movement
		float direction = 0;
		if (Input.IsActionPressed("move_left")) direction = -1;
		else if (Input.IsActionPressed("move_right")) direction = 1;

		bool isTurning = (direction > 0 && velocity.X < -10) || (direction < 0 && velocity.X > 10);

		// P-Speed charge: running same direction on the ground builds it, stopping or
		// switching direction dumps it. In-air keeps whatever you built up on the ground.
		if (IsOnFloor())
		{
			if (direction != 0 && direction == _lastRunDir)
				_sprintTimer = Mathf.Min(_sprintTimer + dt, 4.5f);
			else if (direction == 0 || isTurning)
				_sprintTimer = 0f;
		}
		if (direction != 0)
		{
			_lastRunDir = direction;
			_facing = direction > 0 ? 1 : -1;
		}

		// Up to +15% max speed once fully charged (4.5s of clean running) – a nudge, not a dash
		float effectiveMaxSpeed = MaxSpeed * (1f + SprintCharge * 0.15f);

		// Swinging the sword puts a hard brake on you for ~0.25s so you can't
		// dash-slash at full speed through enemies
		if (_attackLockout > 0f)
		{
			_attackLockout -= dt;
			effectiveMaxSpeed = MaxSpeed * 0.3f;
		}

		if (direction != 0)
		{
			float accel = !IsOnFloor() ? AirAcceleration
						: isTurning    ? TurnAcceleration
						:                Acceleration;
			velocity.X = Mathf.MoveToward(velocity.X, direction * effectiveMaxSpeed, accel * dt);
		}
		else
		{
			float decel = IsOnFloor() ? Deceleration : AirAcceleration;
			velocity.X = Mathf.MoveToward(velocity.X, 0f, decel * dt);
		}

		// Left boundary
		if (Position.X < 0)
			Position = new Vector2(0, Position.Y);

		Velocity = velocity;
		MoveAndSlide();

		// Sword attack – j/x fires in the direction we're facing, short cooldown.
		// Ducking disables the swing, you have to stand up first. 3 uses per life,
		// refill via sword pickups.
		if (_attackCooldown > 0f) _attackCooldown -= dt;
		if (Input.IsActionJustPressed("attack") && _attackCooldown <= 0f
			&& !IsDying && !_isDucking && _swordUses > 0)
			Attack();

		// Invincibility timer after hit – just decrements, don't early-return or the
		// shield/star/shake/magnet timers below freeze and stick (camera offset got stuck)
		if (_invincibilityTimer > 0) _invincibilityTimer -= dt;

		// Shield timer
		if (_shieldActive)
		{
			_shieldTimer -= dt;
			if (_shieldTimer <= 0) _shieldActive = false;
		}

		// Fire flower timer
		if (_fireActive)
		{
			_fireTimer -= dt;
			if (_fireTimer <= 0f) _fireActive = false;
		}

		// Camera shake – random jitter for `shakeTimer` seconds, snap back when done
		if (_shakeTimer > 0f)
		{
			_shakeTimer -= dt;
			if (_shakeTimer <= 0f)
			{
				_camera.Offset = Vector2.Zero;
			}
			else
			{
				_camera.Offset = new Vector2(
					(GD.Randf() * 2f - 1f) * _shakeStrength,
					(GD.Randf() * 2f - 1f) * _shakeStrength);
			}
		}

		// Star – cycle hue for the rainbow flash, reset to white when it runs out
		if (_starActive || _starInvincibilityActive)
		{
			_starTimer -= dt;
			_starInvincibilityTimer -= dt;

			if (_starTimer <= 0f && _starActive)
			{
				_starActive = false;
				SoundManager.Instance.SwitchMusic(SoundManager.Instance.GameMusic);
			}

			if (_starInvincibilityTimer <= 0f && _starInvincibilityActive)
			{
				_starInvincibilityActive = false;
			}

			if (_starActive)
			{
				_starHue = (_starHue + dt * 3f) % 1f;
				Modulate = Color.FromHsv(_starHue, 1f, 1f);
			}
			else if (_starInvincibilityActive)
			{
				float alpha = Mathf.Abs(Mathf.Sin(Time.GetTicksMsec() * 0.02f));
				Modulate = new Color(1f, 1f, 1f, alpha);
			}
			else
			{
				Modulate = Colors.White;
			}
		}
	}

	public void AddScore(int amount)
	{
		_score += amount;
		// Every 100 coins grants an extra life (#41). Compare against grant-count instead of
		// modulo so a big bonus (e.g. stomp chain giving +6) still triggers when crossing 100.
		int earned = _score / 100;
		while (_livesFromScoreGranted < earned)
		{
			_livesFromScoreGranted++;
			_lives = Mathf.Min(_lives + 1, 9);
		}
	}

	// Direct life pickup – cap at 9 so the hud label doesn't overflow
	public void AddLife()
	{
		_lives = Mathf.Min(_lives + 1, 9);
	}

	public void AddSwordUse()
	{
		_swordUses = Mathf.Min(_swordUses + 1, MaxSwordUses);
	}

	public void ActivateShield()
	{
		_shieldActive = true;
		_shieldTimer = 10f;
	}

	public void ActivateMagnet()
	{
		_magnetActive = true;
		_magnetTimer = 5f;
	}

	// Fire flower (#45) – 10s of fire attacks. Sword still swings, but every swing
	// also spits a fireball in the facing direction
	public void ActivateFire()
	{
		_fireActive = true;
		_fireTimer = 10f;
	}

	// Star – full invincibility vs enemies for 6 seconds, rainbow tint (#84)
	public void ActivateStar()
	{	
		_starActive = true;
		_starInvincibilityActive = true;

		_starTimer = 6f;
		_starInvincibilityTimer = 6.5f;

		_starHue = 0f;

		SoundManager.Instance.SwitchMusic(SoundManager.Instance.StarMusic);
	}

	public void SaveHighscorePublic() => SaveHighscore(_score);
private void SaveHighscore(int score)
	{
		string path = "user://highscore.dat";
		int best = 0;

		if (FileAccess.FileExists(path))
		{
			using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
			best = (int)(uint)file.Get32();
		}

		if (score > best)
		{
			using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
			file.Store32((uint)score);
		}
	}

	public static int LoadHighscore()
	{
		string path = "user://highscore.dat";
		if (!FileAccess.FileExists(path)) return 0;
		using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
		return (int)(uint)file.Get32();
	}

	public async void Die()
	{
		if (IsDying) return;

		// Star power – full invincibility, any enemy hit is ignored (#84)
		if (_starInvincibilityActive) return;

		// Shield eats one enemy hit, then breaks (#83)
		if (_shieldActive)
		{
			_shieldActive = false;
			_invincibilityTimer = 1.5f;
			var shieldTween = CreateTween();
			shieldTween.TweenProperty(this, "modulate", new Color(0.4f, 1f, 1f, 1f), 0.1f);
			shieldTween.TweenProperty(this, "modulate", Colors.White, 0.3f);
			return;
		}

		IsDying = true;
		Shake(7f, 0.25f);

		if (IsSmall)
		{
		_lives--;
		_lives = Mathf.Max(_lives, 0);
		}

		if (_lives <= 0)
		{
			SaveHighscore(_score);
			Visible = false;
			SetPhysicsProcess(false);
			GetTree().ChangeSceneToFile("res://Scenes/GameOver.tscn");
		}
		else
{
	if (!IsSmall)
	{
		// First hit – shrink player, don't die yet
		IsSmall = true;
		_invincibilityTimer = 1.5f;
		
		// Shrink collision and scale visually
		_standShape.Shape = new RectangleShape2D { Size = new Vector2(30, 30) };
		Scale = new Vector2(0.6f, 0.6f);
		
		// Blink effect
		var tween = CreateTween();
		tween.SetLoops(6);
		tween.TweenProperty(this, "modulate", new Color(1, 0, 0, 0.3f), 0.1f);
		tween.TweenProperty(this, "modulate", new Color(1, 1, 1, 1f), 0.1f);
		
		IsDying = false;
	}
	else
	{
		// Second hit – actually die and respawn
		IsSmall = false;
		Scale = Vector2.One;
		_standShape.Shape = new RectangleShape2D { Size = new Vector2(30, 60) };
		
		Visible = false;
		SetPhysicsProcess(false);

		var timer = GetTree().CreateTimer(1.0f, true, false, true);
		await ToSignal(timer, SceneTreeTimer.SignalName.Timeout);

		Position = _checkpointPosition;
		Visible = true;
		SetPhysicsProcess(true);
		_invincibilityTimer = 1.5f;
		IsDying = false;
		ResetAirState();

		var tween = CreateTween();
		tween.SetLoops(6);
		tween.TweenProperty(this, "modulate", new Color(1, 0, 0, 0.3f), 0.1f);
		tween.TweenProperty(this, "modulate", new Color(1, 1, 1, 1f), 0.1f);
	}
}

}

}
