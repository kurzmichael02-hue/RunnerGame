using Godot;

public partial class GameManager : Node2D
{
	private int _score = 0;
	private AudioStreamPlayer _music;
	
	 public override void _Ready()
	{
		_music = GetNode<AudioStreamPlayer>("BackgroundMusic");
		_music.Play();
	}
	public void AddScore(int amount)
	{
		_score += amount;
		GD.Print("Score: " + _score);
	}
}
