using Godot;

// Pilz-Pickup: lässt den player wieder auf normale größe wachsen wenn er gerade klein ist.
// Nur relevant nach dem ersten treffer (IsSmall=true). Wenn der player schon groß ist,
// fliegt der pilz trotzdem weg – kein extra-leben oder bonus.
public partial class GrowthPickup : Area2D
{
	private float _time = 0f;
	private float _baseY;
	private bool _initialOverlapChecked = false;

	public override void _Ready()
	{
		_baseY = Position.Y;
		BodyEntered += OnBodyEntered;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!_initialOverlapChecked)
		{
			_initialOverlapChecked = true;
			foreach (var body in GetOverlappingBodies())
				OnBodyEntered(body);
		}

		// Auch proximity-check — falls die CollisionShape kleiner ist als der visuelle
		// sprite (also man sieht sich berühren aber die rect's überlappen nicht).
		var player = GetTree().GetFirstNodeInGroup("player") as Player;
		if (player != null && (player.GlobalPosition - GlobalPosition).Length() < 50f)
			OnBodyEntered(player);
	}

	public override void _Process(double delta)
	{
		// Bob um feste Ausgangsposition – kein kumulativer Drift
		_time += (float)delta;
		Position = new Vector2(Position.X, _baseY + Mathf.Sin(_time * 4f) * 5f);
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is not Player player) return;

		// Wachsen: nur wenn player gerade geschrumpft ist
		if (player.IsSmall)
		{
			player.Grow();
			SoundManager.Instance.PlayCoin(); // TODO: eigenen wachstums-sound einfügen wenn vorhanden
		}
		// auch wenn player groß ist: pilz verschwindet (einmalverwendung)
		QueueFree();
	}
}
