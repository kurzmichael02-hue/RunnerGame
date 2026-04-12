using Godot;

public partial class SoundManager : Node
{
	public static SoundManager Instance;

	private AudioStreamPlayer _coin;
	private AudioStreamPlayer _enemy;
	private AudioStreamPlayer _checkpoint;
	private AudioStreamPlayer _music;
	private AudioStreamPlayer _jump;

	public override void _Ready()
	{
		Instance = this;

		_coin = GetNode<AudioStreamPlayer>("CoinSound");
		_enemy = GetNode<AudioStreamPlayer>("EnemyDeathSound");
		_checkpoint = GetNode<AudioStreamPlayer>("CheckPointSound");
		_music = GetNode<AudioStreamPlayer>("BackGroundMusic");
		_jump = GetNode<AudioStreamPlayer>("JumpSound");
	}

	public void PlayCoin()
	{
		_coin.Play();
	}

	public void PlayJump()
	{
		_jump.Play();
	}

	public void PlayEnemyDeath()
	{
		_enemy.Play();
	}

	public void PlayCheckpoint()
	{
		_checkpoint.Play();
	}

	public void PlayMusic()
	{
		if (!_music.Playing)
			_music.Play();
	}

	public void StopMusic()
	{
		_music.Stop();
	}
}
