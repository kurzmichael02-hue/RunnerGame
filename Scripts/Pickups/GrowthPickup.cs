using Godot;

// Pilz-Pickup: lässt den player wieder auf normale größe wachsen wenn er gerade klein ist.
// Nur relevant nach dem ersten treffer (IsSmall=true). Wenn der player schon groß ist,
// fliegt der pilz trotzdem weg – kein extra-leben oder bonus.
public partial class GrowthPickup : Area2D
{
	private float _time = 0f;

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	public override void _Process(double delta)
	{
		// Hüpft sanft auf und ab damit man sieht dass es ein pickup ist
		_time += (float)delta;
		Position = new Vector2(Position.X, Position.Y + Mathf.Sin(_time * 4f) * 0.3f);
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
