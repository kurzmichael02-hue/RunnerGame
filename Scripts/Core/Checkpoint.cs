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
		_sprite.Texture = InactiveTexture;
   		_sprite.Scale = new Vector2(4.5f, 4.5f);
		

		BodyEntered += OnBodyEntered;
	}
	
	private void OnBodyEntered(Node body)
{
	if (_activated)
		return;

	if (body.IsInGroup("player"))
	{
		_activated = true;
		_sprite.Texture = ActiveTexture;
		_sprite.Scale = new Vector2(0.5f, 0.5f);

		var player = body as Player; 

		if (player != null)
		{
			player.SetCheckpoint(GlobalPosition);
		}

		SoundManager.Instance.PlayCheckpoint();
	}
}

}
