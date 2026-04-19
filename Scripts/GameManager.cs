using Godot;

// Attached to the Game scene root – handles level-wide setup.
// Right now just swaps to the gameplay music when the level loads.
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
