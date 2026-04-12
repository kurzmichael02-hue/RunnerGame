using Godot;

public partial class GameManager : Node2D
{
	private int _score = 0;
	
	 public override void _Ready()
	{
	SoundManager.Instance.PlayMusic();
	}
	public void AddScore(int amount)
	{
		_score += amount;
		GD.Print("Score: " + _score);
	}
}
