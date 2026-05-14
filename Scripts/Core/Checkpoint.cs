using Godot;

public partial class Checkpoint : Area2D
{
	private Sprite2D _sprite;
	private bool _activated = false;

	public override void _Ready()
	{
		_sprite = GetNode<Sprite2D>("Sprite2D");

		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (_activated)
			return;

		if (!body.IsInGroup("player"))
			return;

		_activated = true;

		// Flagge wird grün
		_sprite.Modulate = new Color(0.2f, 1f, 0.35f);

		var player = body as Player;
		player?.SetCheckpoint(GlobalPosition);

		SoundManager.Instance.PlayCheckpoint();
	}
}
