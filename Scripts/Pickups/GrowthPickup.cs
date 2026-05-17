using Godot;

// Pilz-Pickup: lässt den player wieder auf normale größe wachsen wenn er gerade klein ist.
// Nur relevant nach dem ersten treffer (IsSmall=true). Wenn der player schon groß ist,
// fliegt der pilz trotzdem weg – kein extra-leben oder bonus.
public partial class GrowthPickup : Area2D
{
	private float _time = 0f;
	private float _baseY;

	public override void _Ready()
	{
		_baseY = Position.Y;
		BodyEntered += OnBodyEntered;
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
