using Godot;

public partial class CharacterSelection : Node
{
	private ConfirmationDialog _exitDialog;
	private Label _selectedLabel;

	// button index maps to player char id
	private static readonly int[] CharIds = { 0, 1 };
	private static readonly string[] CharNames = { "Default", "Mischa" };

	public override void _Ready()
	{
		SoundManager.Instance.SwitchMusic(SoundManager.Instance.StartScreenMusic);
		Player.LoadProfile();
		_exitDialog = GetNode<ConfirmationDialog>("ExitConfirmDialog");
		_selectedLabel = GetNode<Label>("SelectedLabel");
		UpdateLabel();
	}

	public override void _Process(double delta) { }

	private void _on_button_pressed() => TrySelect(0);
	private void _on_button_2_pressed() => TrySelect(1);

	private void TrySelect(int btnIndex)
	{
		SoundManager.Instance.PlayButton();
		int id = CharIds[btnIndex];

		if (!Player.UnlockedCharacters.Contains(id))
		{
			int price = Player.GetCharacterPrice(id);
			if (price < 0 || !Player.BuyCharacter(id))
			{
				_selectedLabel.Text = $"not enough coins ({Player.Coins}/{price})";
				return;
			}
		}

		Player.SelectCharacter(id);
		UpdateLabel();
	}

	private void UpdateLabel()
	{
		string name = CharNames[Player.SelectedCharacter < CharNames.Length ? Player.SelectedCharacter : 0];
		_selectedLabel.Text = $"Selected: {name}  |  coins: {Player.Coins}";
	}

	private void _on_main_menu_pressed()
	{
		SoundManager.Instance.PlayButton();
		GetTree().ChangeSceneToFile("res://Scenes/Main/MainMenu.tscn");
	}

	private void _on_exit_pressed()
	{
		SoundManager.Instance.PlayButton();
		_exitDialog.PopupCentered();
	}

	private void _on_exit_confirmed() => GetTree().Quit();
}
