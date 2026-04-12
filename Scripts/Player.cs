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
	private bool _isDying = false;
	private bool _shieldActive = false;
	private float _shieldTimer = 0f;
	private float _invincibilityTimer = 0f;
	private float _coyoteTimer = 0f;
	private float _jumpBufferTimer = 0f;
	private bool _isJumping = false;

	public int Lives => _lives;
	public int Score => _score;
	public bool StompedEnemy = false;
	private Vector2 _checkpointPosition = new Vector2(200, 260);
	
	//Sounds
	private AudioStreamPlayer _jumpFx;

public void SetCheckpoint(Vector2 position)
{
	_checkpointPosition = position;
	GD.Print("Checkpoint saved at: " + position);
}

	public override void _Ready()
	{
		// Don't physically collide with enemies
SetCollisionMaskValue(2, false);
		GD.Print("Lives: " + _lives);
		_jumpFx = GetNode<AudioStreamPlayer>("JumpFx");
		
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;
		JumpHoldTimer = Mathf.Max(JumpHoldTimer - dt, 0f);
		Vector2 velocity = Velocity;
		
		// Magnet – pull nearby coins (#40)
if (_magnetActive)
{
	_magnetTimer -= dt;
	if (_magnetTimer <= 0f)
	{
		_magnetActive = false;
		GD.Print("Magnet expired!");
	}
	else
	{
		// Pull all coins within radius toward player
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

		// Asymmetric gravity
		float gravity;
		if (velocity.Y < 0 && Input.IsActionPressed("jump") && JumpHoldTimer > 0f){
			gravity = JumpGravity;
		}
		else {
			gravity = FallGravity;
		}
		if (!IsOnFloor())
			velocity.Y += gravity * dt;
		velocity.Y = Mathf.Min(velocity.Y, MaxFallSpeed);

		// Coyote time
		if (IsOnFloor()) { _coyoteTimer = CoyoteTime; _isJumping = false; }
		else _coyoteTimer -= dt;

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
			GD.Print(SoundManager.Instance); 
			SoundManager.Instance.PlayJump();
		}

		// Horizontal movement
		float direction = 0;
		if (Input.IsActionPressed("move_left")) direction = -1;
		else if (Input.IsActionPressed("move_right")) direction = 1;

		bool isTurning = (direction > 0 && velocity.X < -10)
					  || (direction < 0 && velocity.X > 10);

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

		// Skip when invincible
		if (_invincibilityTimer > 0) { _invincibilityTimer -= dt; return; }

		// Shield check
		if (_shieldActive)
		{
			_shieldTimer -= dt;
			if (_shieldTimer <= 0) { _shieldActive = false; }
		}
	}

	
	public void AddScore(int amount)
{
	_score += amount;
	// 100 coins = 1 extra life (#41)
	if (_score % 100 == 0)
	{
		_lives++;
		GD.Print("Extra life! Lives: " + _lives);
	}
	GD.Print("Score: " + _score);
}

	public void ActivateShield()
	{
		_shieldActive = true;
		_shieldTimer = 10f;
		GD.Print("Shield activated!");
	}
	
	private bool _magnetActive = false;
private float _magnetTimer = 0f;
private float _magnetRadius = 200f;

public void ActivateMagnet()
{
	_magnetActive = true;
	_magnetTimer = 5f;
	GD.Print("Magnet activated!");
}

	public void Die()
	{
		if (_isDying) return;
		_isDying = true;

		_lives--;
		_lives = Mathf.Max(_lives, 0);
		GD.Print("Lives: " + _lives);

		if (_lives <= 0)
		{
			GD.Print("GAME OVER");
			Visible = false;
			GetTree().Paused = true;
		}
		else
		{
			_invincibilityTimer = 1.5f;
			// Blink effect during invincibility
var tween = CreateTween();
tween.SetLoops(6);
tween.TweenProperty(this, "modulate", new Color(1, 0, 0, 0.3f), 0.1f);
tween.TweenProperty(this, "modulate", new Color(1, 1, 1, 1f), 0.1f);
			Position = _checkpointPosition;
			_isDying = false;
		}
	}
}
