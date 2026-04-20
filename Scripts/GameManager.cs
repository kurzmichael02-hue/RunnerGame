using Godot;

// Attached to the Game scene root – handles level-wide setup.
// Right now just swaps to the gameplay music when the level loads.
public partial class GameManager : Node2D
{
	public override void _Ready()
	{
		// Fresh run – clear the level-won flag that would otherwise block ESC pause after a retry
		LevelGoal.Reset();
		SoundManager.Instance.SwitchMusic(SoundManager.Instance.GameMusic);
	}
}
