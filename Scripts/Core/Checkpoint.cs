using Godot;
using System;

public partial class Checkpoint : Area2D
{
	[Export] public Texture2D InactiveTexture;
	[Export] public Texture2D ActiveTexture;

	private Sprite2D _sprite;
	private bool _activated = false;

	public override void _Ready()
	{
		_sprite = GetNode<Sprite2D>("Sprite2D");

		if (InactiveTexture != null)
			_sprite.Texture = InactiveTexture;

		_sprite.Scale = new Vector2(4.5f, 4.5f);

		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node body)
	{
		if (_activated)
			return;

		if (!body.IsInGroup("player"))
			return;

		_activated = true;

		if (ActiveTexture != null)
			_sprite.Texture = ActiveTexture;
		else
			_sprite.Modulate = new Color(0.2f, 1f, 0.35f);

		_sprite.Scale = new Vector2(0.5f, 0.5f);

		var player = body as Player;

		if (player != null)
		{
			player.SetCheckpoint(GlobalPosition);
		}

		SoundManager.Instance.PlayCheckpoint();
	}
}
