using Godot;

public partial class Enemy : CharacterBody2D
{
	[Export] public float Speed = 100.0f;
	[Export] public float MoveDistance = 200.0f;

	private Vector2 _startPosition;
	private Vector2 _gravity;
	private int _direction = 1;

	public override void _Ready()
	{
		_startPosition = Position;
		_gravity = new Vector2(0, 980f);
		var hitBox = GetNode<Area2D>("HitBox");
hitBox.BodyEntered += OnBodyEntered;
GD.Print("HitBox found: " + hitBox.Name);
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is Player player)
		{
			player.Die();
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		if (!IsOnFloor())
			velocity += _gravity * (float)delta;

		velocity.X = Speed * _direction;

		if (Position.X > _startPosition.X + MoveDistance)
			_direction = -1;
		else if (Position.X < _startPosition.X - MoveDistance)
			_direction = 1;

		Velocity = velocity;
		MoveAndSlide();
	}
}
