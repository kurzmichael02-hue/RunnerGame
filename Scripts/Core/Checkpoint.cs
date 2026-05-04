using Godot;

public partial class Checkpoint : Area2D
{
	// Beide texturen als export – können im editor per instanz gesetzt werden.
	// ActiveTexture ist optional: wenn nicht gesetzt, grüner tint als fallback.
	[Export] public Texture2D InactiveTexture;
	[Export] public Texture2D ActiveTexture;

	private Sprite2D _sprite;
	private bool _activated = false;

	public override void _Ready()
	{
		_sprite = GetNode<Sprite2D>("Sprite2D");
		// Nur überschreiben wenn export gesetzt – sonst bleibt scene-textur
		if (InactiveTexture != null)
			_sprite.Texture = InactiveTexture;

		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node body)
	{
		if (_activated) return;
		if (!body.IsInGroup("player")) return;

		_activated = true;

		// ActiveTexture gesetzt → direkt swappen. Sonst grüner tint (kein extra-asset nötig)
		if (ActiveTexture != null)
			_sprite.Texture = ActiveTexture;
		else
			_sprite.Modulate = new Color(0.2f, 1f, 0.35f);

		var player = body as Player;
		if (player != null)
			player.SetCheckpoint(GlobalPosition);

		SoundManager.Instance.PlayCheckpoint();
	}
}
