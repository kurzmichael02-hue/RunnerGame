using Godot;

public partial class GameManager : Node2D
{
	private int _score = 0;
	
	public override void _Ready()
	{
		SoundManager.Instance.SwitchMusic(SoundManager.Instance.GameMusic);
	}
	public void AddScore(int amount)
	{
		_score += amount;
	}
}
