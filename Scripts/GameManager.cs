using Godot;

public partial class GameManager : Node2D
{
	private int _score = 0;

	public void AddScore(int amount)
	{
		_score += amount;
		GD.Print("Score: " + _score);
	}
}
