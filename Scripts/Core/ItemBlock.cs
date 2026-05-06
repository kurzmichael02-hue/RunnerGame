using Godot;

// ?-Block (Mario-Stil): player springt von unten dagegen → item spawnt, block wird "leer".
// Braucht als Kinder: Sprite2D, CollisionShape2D, BottomArea (Area2D mit CollisionShape).
// Schayans Texturen: animierterblock.png (aktiv) und blockdead.png (verbraucht).
public partial class ItemBlock : StaticBody2D
{
	// Welches item aus dem block kommt – im editor per instanz wählbar
	public enum BlockContent { Coin, Growth, Heart }
	[Export] public BlockContent Content = BlockContent.Growth;

	// Texturen – im editor setzen oder der code lädt sie direkt
	[Export] public Texture2D ActiveTexture;
	[Export] public Texture2D DeadTexture;

	private Sprite2D _sprite;
	private bool _used = false;

	// Szenen-Pfade für die items die spawnen
	private const string CoinScene   = "res://Scenes/Pickups/coin.tscn";
	private const string GrowthScene = "res://Scenes/Pickups/growth_pickup.tscn";
	private const string HeartScene  = "res://Scenes/Pickups/heart_pickup.tscn";

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Pausable;
		_sprite = GetNode<Sprite2D>("Sprite2D");

		// texturen aus export oder als fallback direkt laden
		if (ActiveTexture != null) _sprite.Texture = ActiveTexture;

		// BottomArea: kleine Area2D direkt unter dem block – erkennt wenn player aufspringt
		var bottomArea = GetNodeOrNull<Area2D>("BottomArea");
		if (bottomArea != null)
			bottomArea.BodyEntered += OnBottomAreaEntered;
	}

	private void OnBottomAreaEntered(Node2D body)
	{
		if (_used) return;
		if (body is not Player player) return;
		// nur auslösen wenn player nach oben springt (kopf trifft block von unten)
		if (player.Velocity.Y >= 0f) return;

		Hit(player);
	}

	private async void Hit(Player player)
	{
		_used = true;

		// block "hüpft" kurz nach oben dann zurück – klassisches mario-feel
		var startPos = Position;
		var tween = CreateTween();
		tween.TweenProperty(this, "position", startPos + new Vector2(0, -8f), 0.06f);
		tween.TweenProperty(this, "position", startPos, 0.08f);
		await ToSignal(tween, Tween.SignalName.Finished);

		// textur wechseln zu "leer"
		if (DeadTexture != null)
			_sprite.Texture = DeadTexture;
		else
			_sprite.Modulate = new Color(0.5f, 0.4f, 0.25f); // fallback: dunkler tönung

		// item direkt über dem block spawnen
		SpawnItem();
		SoundManager.Instance.PlayCoin(); // TODO: eigenen block-sound wenn vorhanden
	}

	private void SpawnItem()
	{
		string path = Content switch
		{
			BlockContent.Coin   => CoinScene,
			BlockContent.Growth => GrowthScene,
			BlockContent.Heart  => HeartScene,
			_                   => GrowthScene
		};

		var scene = GD.Load<PackedScene>(path);
		if (scene == null) return;

		var item = scene.Instantiate<Node2D>();
		// item spawnt 40px über dem block damit es sichtbar auftaucht
		item.GlobalPosition = GlobalPosition + new Vector2(0, -40f);
		GetTree().CurrentScene.AddChild(item);
	}
}
