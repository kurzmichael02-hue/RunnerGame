using Godot;

public partial class Checkpoint : Area2D
{
	[Export] public Texture2D InactiveTexture;
	[Export] public Texture2D ActiveTexture;

	[Export] public Vector2 InactiveScale = new Vector2(4.5f, 4.5f);
	[Export] public Vector2 ActiveScale = new Vector2(0.5f, 0.5f);

	private Sprite2D _sprite;
	private bool _activated = false;

	public override void _Ready()
	{
		_sprite = GetNode<Sprite2D>("Sprite2D");

		if (InactiveTexture != null)
			_sprite.Texture = InactiveTexture;

		_sprite.Scale = InactiveScale;

		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (_activated)
			return;

		if (!body.IsInGroup("player"))
			return;

		_activated = true;

		if (ActiveTexture != null)
			_sprite.Texture = ActiveTexture;

		_sprite.Scale = ActiveScale;

		if (body is Player player)
		{
			player.SetCheckpoint(GlobalPosition);
		}

		SoundManager.Instance.PlayCheckpoint();
	}
}
