using Godot;

public partial class Player : CharacterBody2D
{
	// Movement
	[Export] public float MaxSpeed = 440f;         // vorher 500f – langsamer auf benutzeranforderung
	[Export] public float Acceleration = 650f;     // vorher 800f – bewegung startet träger
	[Export] public float Deceleration = 900f;
	[Export] public float AirAcceleration = 1000f; // vorher 1200f – luftkontrolle etwas reduziert
	[Export] public float TurnAcceleration = 2500f; // vorher 3000f – richtungswechsel weicher
	

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
	// Zählt wie viele 100er-stufen schon ein extra-leben ausgezahlt haben.
	// Vergleich gegen den counter (statt _score % 100), damit ein bonus der
	// von 96 auf 102 springt das leben trotzdem auslöst – siehe AddScore in Player.Profile.cs.
	private int _livesFromScoreGranted = 0;

	// Statics für coins, charakter-auswahl und highscore stehen in Player.Profile.cs

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
	// Read-only flag for enemies / hazards so they skip damage during respawn i-frames
	public bool IsInvincible => _invincibilityTimer > 0f;
	private float _coyoteTimer = 0f;
	private float _jumpBufferTimer = 0f;
	// wie lange der player nach einem wall-jump nicht nochmal an wände springen darf
	private float _wallJumpCooldown = 0f;
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
	private AnimatedSprite2D _anim;
	private AnimatedSprite2D _attackSprite;
	private AnimatedSprite2D _duckSprite;
	
	// Sword + fire flower fields liegen in Player.Combat.cs

	// Camera shake
	private Camera2D _camera;
	private float _shakeTimer = 0f;
	private float _shakeStrength = 0f;

	// Dust: tracks floor transitions so we can poof on landing
	private bool _wasOnFloor = true;

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
		_projectileScene = GD.Load<PackedScene>("res://Scenes/level_objects/projectile.tscn");

		// Coins + selected character come from user://profile.cfg
		LoadProfile();
		
		// GetNodeOrNull damit's nicht crasht wenn schayan/maksym mal die scene
		// umbaut und das spritenode unbenennt – updateanimation hat sowieso null-check
		_anim = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
		_attackSprite = GetNode<AnimatedSprite2D>("AttackSprite");
		_duckSprite = GetNode<AnimatedSprite2D>("DuckSprite");
		
		ApplyCharacterTexture();
		
		_duckSprite.Visible = false;
		_anim.Play("still");
		_attackSprite.Visible = false;

		CreateBoundaryWalls();
	}

	// Unsichtbare wand-StaticBody2D ganz am rand des world space — links bei X=0
	// (terrain in Level1 startet ungefähr da) und rechts X=7270 (kurz hinter goal X=7168).
	// Echte physik statt position-clamp damit der player wirklich blockiert wird.
	private void CreateBoundaryWalls()
	{
		var parent = GetParent();
		if (parent == null) return;
		parent.CallDeferred(Node.MethodName.AddChild, MakeWall(-30));
		parent.CallDeferred(Node.MethodName.AddChild, MakeWall(7270));
	}

	private static StaticBody2D MakeWall(float x)
	{
		var wall = new StaticBody2D { Position = new Vector2(x, 0), CollisionLayer = 1 };
		var shape = new CollisionShape2D
		{
			Shape = new RectangleShape2D { Size = new Vector2(20, 2000) }
		};
		wall.AddChild(shape);
		return wall;
	}
	private void UpdateAnimation(float direction)
{
		if (_anim == null || _attackSprite == null || _duckSprite == null) return;
		
		
		// DUCK (höchste Priorität)
	if (_isDucking)
	{
		_anim.Visible = false;
		_duckSprite.Visible = true;

		_duckSprite.FlipH = _facing < 0;

		if (_duckSprite.Animation != "duck")
			_duckSprite.Play("duck");

		return;
	}

	// zurück zu normal
	_duckSprite.Visible = false;
	_anim.Visible = true;
	
	
		if (_attackLockout > 0f)
		{
			_anim.Visible = false;
			_attackSprite.Visible = true;

			_attackSprite.FlipH = _facing < 0;

			if (_attackSprite.Animation != "attack")
				_attackSprite.Play("attack");

			return;
		}

		_attackSprite.Visible = false;
		_anim.Visible = true;

	// In der Luft
	if (!IsOnFloor())
	{
		_anim.FlipH = direction < 0;

		// Springt nach oben
		if (Velocity.Y < 0)
		{
			if (_anim.Animation != "jump_high")
				_anim.Play("jump_high");
		}
		// Fällt nach unten
		else
		{
			if (_anim.Animation != "jump_down")
				_anim.Play("jump_down");
		}

		return;
	}

	// Am Boden
	if (direction != 0)
	{
		_anim.FlipH = direction < 0;

		if (_anim.Animation != "wallk")
			_anim.Play("wallk");
	}
	else
	{
		if (_anim.Animation != "still")
			_anim.Play("still");
	}
}

	// ApplyCharacterTexture, LoadProfile etc. liegen in Player.Profile.cs

	// Kicked by damage events – camera jitters for `duration` seconds at `strength` pixels
	public void Shake(float strength, float duration)
	{
		_shakeStrength = strength;
		_shakeTimer = duration;
	}

	// Alle lebenden gegner auf ihre start-position zurücksetzen. Wird beim respawn
	// gerufen damit kein gegner direkt über dem checkpoint hängt und insta-killt.
	private void ResetEnemiesToStart()
	{
		foreach (Node n in GetTree().GetNodesInGroup("enemy"))
		{
			if (n is Enemy e) e.ResetToStart();
		}
	}

	// Check ob der player aufstehen kann ohne in einem block über ihm festzustecken.
	// GlobalTransform vom standShape benutzen damit player-scale (z.b. wenn klein/IsSmall)
	// korrekt einberechnet wird — sonst denkt der check der player ist groß und blockt das aufstehen.
	private bool CanStandUp()
	{
		if (_standShape?.Shape is not RectangleShape2D standRect) return true;
		if (_duckShape?.Shape is not RectangleShape2D duckRect) return true;

		// Nur den Raum ueber dem geduckten Kopf pruefen, nicht die volle Stand-Form.
		// Wuerde man die ganze Stand-Form testen, ragt sie unten in den Boden und der
		// Check schlaegt immer fehl -> der Spieler koennte nie wieder aufstehen.
		Vector2 scale = GlobalScale;
		float standTop = _standShape.GlobalPosition.Y - standRect.Size.Y * scale.Y * 0.5f;
		float duckTop = _duckShape.GlobalPosition.Y - duckRect.Size.Y * scale.Y * 0.5f;
		float gap = duckTop - standTop;
		if (gap <= 1f) return true; // kaum hoehenunterschied -> aufstehen immer erlaubt

		// schmale box knapp ueber dem geduckten kopf, deckt nur die aufricht-hoehe ab
		var boxSize = new Vector2(standRect.Size.X * scale.X * 0.6f, gap);
		var center = new Vector2(_standShape.GlobalPosition.X, standTop + gap * 0.5f);
		var query = new PhysicsShapeQueryParameters2D
		{
			Shape = new RectangleShape2D { Size = boxSize },
			Transform = new Transform2D(0f, center),
			CollisionMask = CollisionMask,
			Exclude = new Godot.Collections.Array<Godot.Rid> { GetRid() }
		};
		var hits = GetWorld2D().DirectSpaceState.IntersectShape(query, 1);
		return hits.Count == 0;
	}


	// Pilz-Power-Up: player wächst wieder auf normale größe.
	// Macht nur was wenn der player gerade klein ist (IsSmall=true).
	public void Grow()
	{
		if (!IsSmall) return;
		IsSmall = false;
		Scale = Vector2.One;
		_standShape.Shape = new RectangleShape2D { Size = new Vector2(30, 90) };
		// kurze i-frames damit der player nicht sofort wieder getroffen wird
		_invincibilityTimer = 1.0f;
		// grünes blinken als visuelles feedback
		var tween = CreateTween();
		tween.SetLoops(3);
		tween.TweenProperty(this, "modulate", new Color(0.4f, 1f, 0.4f, 1f), 0.1f);
		tween.TweenProperty(this, "modulate", Colors.White, 0.1f);
	}
	// Attack / SpawnSwordSwoosh / SpawnFireball / PlayNoSwordFlash / AddSwordUse
	// liegen in Player.Combat.cs

	private void SpawnLandingDust()
	{
		// Little white poof at the player's feet, expands and fades in 0.25s
		var wrap = new Node2D { GlobalPosition = GlobalPosition + new Vector2(0, 35f) };
		var poly = new Polygon2D
		{
			Color = new Color(1f, 1f, 1f, 0.65f),
			Polygon = new Vector2[]
			{
				new Vector2(-14, 0), new Vector2(-10, -6), new Vector2(-4, -8),
				new Vector2(4, -8), new Vector2(10, -6), new Vector2(14, 0)
			}
		};
		wrap.AddChild(poly);
		GetTree().CurrentScene.AddChild(wrap);

		var tween = GetTree().CreateTween();
		tween.TweenProperty(wrap, "scale", Vector2.One * 1.8f, 0.25f);
		tween.Parallel().TweenProperty(wrap, "modulate:a", 0f, 0.25f);
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
	_standShape.Shape = new RectangleShape2D { Size = new Vector2(30, 90) };

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
	
	SoundManager.Instance.PlayPlayerDeath();
	
	_lives--;
	_lives = Mathf.Max(_lives, 0);

	if (_lives <= 0)
	{
		LastRunScore = _score;
		SaveHighscore(_score);
		Visible = false;
		SetPhysicsProcess(false);
		GetTree().ChangeSceneToFile("res://Scenes/Main/GameOver.tscn");
		return;
	}

	Visible = false;
	SetPhysicsProcess(false);

	var timer = GetTree().CreateTimer(1.0f, true, false, true);
	await ToSignal(timer, SceneTreeTimer.SignalName.Timeout);

	Position = _checkpointPosition;
	ResetEnemiesToStart();
	Visible = true;
	SetPhysicsProcess(true);
	// invincibility = enemy grace period damit beide synchron enden
	_invincibilityTimer = 3f;
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

		// Duck-State zuruecksetzen, sonst respawnt der Spieler geduckt und haengt fest,
		// wenn er im Moment des Todes geduckt war.
		_isDucking = false;
		if (_standShape != null) _standShape.Disabled = false;
		if (_duckShape != null) _duckShape.Disabled = true;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (GetTree().Paused) return;
		float direction = 0;
		if (Input.IsActionPressed("move_left")) direction = -1;
		else if (Input.IsActionPressed("move_right")) direction = 1;

		
	UpdateAnimation(direction);
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
	// nur aufstehen wenn über dem Player platz ist — sonst bleibt der player
	// ducked weil sonst die stand-collision in einen block über uns reinrutscht
	// und der player für immer im block feststeckt.
	if (CanStandUp())
	{
		_isDucking = false;
		_standShape.Disabled = false;
		_duckShape.Disabled = true;
	}
}

if (_isDucking)
{
	float duckDir = 0;
	if (Input.IsActionPressed("move_left")) duckDir = -1;
	else if (Input.IsActionPressed("move_right")) duckDir = 1;

	// facing auch beim ducken updaten, sonst flipt der duck-sprite nicht
	// wenn der player die richtung wechselt während er geduckt ist.
	if (duckDir != 0)
		_facing = duckDir > 0 ? 1 : -1;

	_jumpBufferTimer = 0f;
	Vector2 vel = Velocity;
	
	//Slowing down the player or making him slide when ducking depending on if he's on a slope or not
	if (IsOnFloor())
	{
		Vector2 normal = GetFloorNormal();

		float slopeAngle = Mathf.RadToDeg(
			Mathf.Acos(normal.Dot(Vector2.Up))
		);

		// Only slide on actual slopes
		if (slopeAngle > 10f)
		{
			// Determine downhill direction
			float slideDirection = normal.X;

			// Slide speed depends on slope steepness
		float slideSpeed = slopeAngle * 220f;

		vel.X = slideDirection * slideSpeed;
	}
	
	// Ducking slows the player to 40% of max speed – feels heavier, harder to dodge
	else 
	{
		vel.X = duckDir * MaxSpeed * 0.4f;
	}
}
else
{
	vel.X = duckDir * MaxSpeed * 0.4f;
}
	if (!IsOnFloor()) vel.Y += FallGravity * dt;
	vel.Y = Mathf.Min(vel.Y, MaxFallSpeed);
	Velocity = vel;
	MoveAndSlide();
	// timer trotzdem ticken sonst hängt alles solange er duckt
	UpdateTimers(dt);
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
		if (IsOnWall() && _wallJumpCooldown <= 0f)
			{
				// Wall jump: mehr laterale kraft, runtergedreht auf 0.6 weil der player vorher zu hoch sprang.
				// cooldown 0.5s verhindert spam – player muss sich erst von wand wegbewegen.
				Vector2 wallNormal = GetWallNormal();
				velocity.X = wallNormal.X * MaxSpeed * 1.0f;
				velocity.Y = JumpVelocity * 0.6f;
				_doubleJumpUsed = false;
				_jumpBufferTimer = 0f;
				JumpHoldTimer = JumpHoldTime * 0.7f;
				_wallJumpCooldown = 0.5f;
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
		//float direction = 0;
		//if (Input.IsActionPressed("move_left")) direction = -1;
		//else if (Input.IsActionPressed("move_right")) direction = 1;

		bool isTurning = (direction > 0 && velocity.X < -10) || (direction < 0 && velocity.X > 10);

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
		
		//Decrese speed if on slope
		if (IsOnFloor())
		{
			Vector2 normal = GetFloorNormal();

			// Angel of the Slope 
			float slopeAngle = Mathf.RadToDeg(
				Mathf.Acos(normal.Dot(Vector2.Up))
			);

			// The steeper the slower
			float slopeFactor = Mathf.Clamp(
				1f - (slopeAngle / 45f),
				0.45f,
				1f
			);

			// Check if player is going up
			bool movingUpSlope =
				(direction > 0 && normal.X < 0) ||
				(direction < 0 && normal.X > 0);

			if (movingUpSlope)
				effectiveMaxSpeed *= slopeFactor;
		}
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

		Velocity = velocity;
		MoveAndSlide();

		// Landing dust – only on the airborne→grounded transition, scales with fall speed.
		// Feels weightier than landing silently, cheap to render (single polygon).
		bool onFloorNow = IsOnFloor();
		if (!_wasOnFloor && onFloorNow)
		{
			float fall = Mathf.Abs(velocity.Y);
			if (fall > 50f) SpawnLandingDust();
		}
		_wasOnFloor = onFloorNow;

		// Sword attack – j/x fires in the direction we're facing, short cooldown.
		// Ducking disables the swing, you have to stand up first. 3 uses per life,
		// refill via sword pickups.
		if (_attackCooldown > 0f) _attackCooldown -= dt;
		if (Input.IsActionJustPressed("attack") && !IsDying && !_isDucking)
		{
			if (_attackCooldown <= 0f && _swordUses > 0)
			{
				Attack();
			}
			else
			{
				// Tim's no-sword-left sfx covers both: out of swings, or still on cooldown
				Shake(2f, 0.35f);
				PlayNoSwordFlash();
				SoundManager.Instance.PlayNoSwordLeft();
			}
				
		}

		UpdateTimers(dt);
	}

	// timer-tick. extra methode weil der duck-branch sonst returned und alles
	// einfriert (shake, star, shield). bug, gefixt.
	private void UpdateTimers(float dt)
	{
		// Invincibility timer after hit – just decrements, don't early-return or the
		// shield/star/shake/magnet timers below freeze and stick (camera offset got stuck)
		if (_invincibilityTimer > 0) _invincibilityTimer -= dt;
		if (_wallJumpCooldown > 0f) _wallJumpCooldown -= dt;

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

		// No Sword left – kurzes rotes flackern. nur den modulate während des flash-windows
		// setzen, sonst überschreibt der frame-reset die die()/diefall-blink-tweens
		if (_noSwordFlashTimer > 0f)
		{
			_noSwordFlashTimer -= dt;
			if (_noSwordFlashTimer > 0f)
				Modulate = new Color(1f, 0.3f, 0.3f, 1f);
			else if (!_starActive && !_starInvincibilityActive && !IsDying)
				Modulate = Colors.White;
		}
		// Star – cycle hue for the rainbow flash, reset to white when it runs out
		if (_starActive || _starInvincibilityActive)
		{
			_starTimer -= dt;
			_starInvincibilityTimer -= dt;

			//if (_starTimer <= 0f && _starActive)
			//{
				//_starActive = false;
				//SoundManager.Instance.SwitchMusic(SoundManager.Instance.GameMusic);
			//}

			if (_starTimer <= 0f && _starActive)
			{
				_starActive = false;

				if (!LevelGoal.LevelCompleted)
				{
					SoundManager.Instance.SwitchMusic(
						SoundManager.Instance.GameMusic
					);
				}
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

	// AddScore + AddLife + AddCoin liegen in Player.Profile.cs
	// AddSwordUse liegt in Player.Combat.cs

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

	// SaveHighscore / LoadHighscore / LoadProfile / SaveProfile / GetCharacterPrice
	// / BuyCharacter / SelectCharacter / AddCoin liegen in Player.Profile.cs

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

		//if (IsSmall)
		//{
		//_lives--;
		//_lives = Mathf.Max(_lives, 0);
		//}

		if (_lives <= 1)
		{	
			SoundManager.Instance.PlayPlayerDeath();
			LastRunScore = _score;
			SaveHighscore(_score);
			Visible = false;
			SetPhysicsProcess(false);
			GetTree().ChangeSceneToFile("res://Scenes/Main/GameOver.tscn");
		}
		else
{
	if (!IsSmall)
	{
		// First hit – shrink player, don't die yet
		IsSmall = true;
		// kurze i-frames damit player nicht direkt nochmal stirbt, aber nicht so lange
		// dass man durch andere gegner durchläuft
		_invincibilityTimer = 0.5f;

		// Shrink collision and scale visually
		_standShape.Shape = new RectangleShape2D { Size = new Vector2(30, 90) };
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
		_lives--;
		_lives = Mathf.Max(_lives, 0);
		// Second hit – actually die and respawn
		SoundManager.Instance.PlayPlayerDeath();
		IsSmall = false;
		Scale = Vector2.One;
		_standShape.Shape = new RectangleShape2D { Size = new Vector2(30, 90) };
		
		Visible = false;
		SetPhysicsProcess(false);

		var timer = GetTree().CreateTimer(1.0f, true, false, true);
		await ToSignal(timer, SceneTreeTimer.SignalName.Timeout);

		Position = _checkpointPosition;
		ResetEnemiesToStart();
		Visible = true;
		SetPhysicsProcess(true);
		// kurze i-frames nach respawn, damit man nicht direkt wieder stirbt aber auch nicht zu lange wartet
		_invincibilityTimer = 0.5f;
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
