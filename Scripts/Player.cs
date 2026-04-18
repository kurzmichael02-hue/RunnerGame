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
	public bool IsDying = false;
	public bool IsSmall = false;
	private CollisionShape2D _standShape;
	private CollisionShape2D _duckShape;
	private bool _shieldActive = false;
	private float _shieldTimer = 0f;
	private float _invincibilityTimer = 0f;
	private float _coyoteTimer = 0f;
	private float _jumpBufferTimer = 0f;
	private bool _isJumping = false;
	private bool _isDucking = false;

	// Magnet
	private bool _magnetActive = false;
	private float _magnetTimer = 0f;
	private float _magnetRadius = 200f;

	public int Lives => _lives;
	public int Score => _score;
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
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;
if (Position.Y > 600f && !IsDying)
	Die();
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
		}

		// Asymmetric gravity – hold jump for higher arc
		float gravity = (velocity.Y < 0 && Input.IsActionPressed("jump") && JumpHoldTimer > 0f)
			? JumpGravity
			: FallGravity;

		if (!IsOnFloor())
			velocity.Y += gravity * dt;
		velocity.Y = Mathf.Min(velocity.Y, MaxFallSpeed);

		// Coyote time
		if (IsOnFloor()) { _coyoteTimer = CoyoteTime; _isJumping = false; }
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
	vel.X = duckDir * MaxSpeed * 0.4f;
	if (!IsOnFloor()) vel.Y += FallGravity * dt;
	vel.Y = Mathf.Min(vel.Y, MaxFallSpeed);
	Velocity = vel;
	MoveAndSlide();
	return;
}

		// Jump buffer
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

		// Horizontal movement
		float direction = 0;
		if (Input.IsActionPressed("move_left")) direction = -1;
		else if (Input.IsActionPressed("move_right")) direction = 1;

		bool isTurning = (direction > 0 && velocity.X < -10) || (direction < 0 && velocity.X > 10);

		if (direction != 0)
		{
			float accel = !IsOnFloor() ? AirAcceleration
						: isTurning    ? TurnAcceleration
						:                Acceleration;
			velocity.X = Mathf.MoveToward(velocity.X, direction * MaxSpeed, accel * dt);
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

		// Invincibility timer after hit
		if (_invincibilityTimer > 0) { _invincibilityTimer -= dt; return; }

		// Shield timer
		if (_shieldActive)
		{
			_shieldTimer -= dt;
			if (_shieldTimer <= 0) _shieldActive = false;
		}
	}

	public void AddScore(int amount)
	{
		_score += amount;
		// Every 100 coins grants an extra life (#41)
		if (_score % 100 == 0)
			_lives++;
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
		IsDying = true;

		_lives--;
		_lives = Mathf.Max(_lives, 0);

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

		var tween = CreateTween();
		tween.SetLoops(6);
		tween.TweenProperty(this, "modulate", new Color(1, 0, 0, 0.3f), 0.1f);
		tween.TweenProperty(this, "modulate", new Color(1, 1, 1, 1f), 0.1f);
	}
}

}

}
