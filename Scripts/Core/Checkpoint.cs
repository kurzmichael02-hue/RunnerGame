using Godot;

public partial class Checkpoint : Area2D
{
	[Export] public Texture2D InactiveTexture;
	[Export] public Texture2D ActiveTexture;

	private Sprite2D _sprite;
	private bool _activated = false;

	// flag sprite is 32px → render at 96px so it's clearly visible
	private const float InactiveH = 96f;
	// normalize all portrait photos to the same height regardless of source resolution
	private const float ActiveH = 80f;

	public override void _Ready()
	{
		_sprite = GetNode<Sprite2D>("Sprite2D");
		_sprite.Visible = true;
		_sprite.Position = Vector2.Zero;

		if (InactiveTexture != null)
		{
			_sprite.Texture = InactiveTexture;
			_sprite.Scale = ScaleForHeight(InactiveTexture, InactiveH);
		}

		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node body)
	{
		if (_activated) return;
		if (!body.IsInGroup("player")) return;
		_activated = true;

		if (ActiveTexture != null)
		{
			_sprite.Texture = ActiveTexture;
			_sprite.Scale = ScaleForHeight(ActiveTexture, ActiveH);
		}
		else
			_sprite.Modulate = new Color(0.2f, 1f, 0.35f);

		var player = body as Player;
		player?.SetCheckpoint(GlobalPosition);
		SoundManager.Instance.PlayCheckpoint();
	}

	private static Vector2 ScaleForHeight(Texture2D tex, float targetH)
	{
		float h = tex.GetHeight();
		float s = h > 0f ? targetH / h : 1f;
		return Vector2.One * s;
	}
}
