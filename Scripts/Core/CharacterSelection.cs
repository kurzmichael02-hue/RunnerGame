using Godot;

public partial class CharacterSelection : Node
{
	private Label _selectedLabel;


	private TextureRect _mischaLockIcon;
	
	private Button _mischaButton;

	private Panel _confirmPanel;

	private int _pendingCharacter = -1;

	// button index maps to player char id
	private static readonly int[] CharIds = { 0, 1 };
	private static readonly string[] CharNames = { "Default", "Mischa" };

	public override void _Ready()
	{
		SoundManager.Instance.SwitchMusic(SoundManager.Instance.StartScreenMusic);
		Player.LoadProfile();

		_mischaButton =	GetNode<Button>("HBoxContainer/Button2");

		_selectedLabel = GetNode<Label>("SelectedLabel");
		_mischaLockIcon = GetNode<TextureRect>("HBoxContainer/Button2/LockIcon");
		_confirmPanel =	GetNode<Panel>("ConfirmPanel");
		UpdateCharacterVisuals();
		UpdateLabel();
		
	}

	private void _on_button_pressed() => TrySelect(0);
	private void _on_button_2_pressed() => TrySelect(1);
	
	private void UpdateCharacterVisuals()
	{
		bool unlocked =
			Player.UnlockedCharacters.Contains(1);

		if (unlocked)
		{
			_mischaButton.Modulate = Colors.White;

			_mischaLockIcon.Visible = false;
		}
		else
		{
			_mischaButton.Modulate =
			new Color(0.55f, 0.55f, 0.55f);

			_mischaLockIcon.Visible = true;
		}
	}

	private void TrySelect(int btnIndex)
	{
		SoundManager.Instance.PlayButton();
		int id = CharIds[btnIndex];

		if (!Player.UnlockedCharacters.Contains(id))
		{
			int price = Player.GetCharacterPrice(id);
			if (Player.Coins < price)
			{
				_selectedLabel.Text =
					$"Not enough coins ({Player.Coins}/{price})";

				return;
			}
			_pendingCharacter = id;
			_confirmPanel.Visible = true;
			return;
		}
		Player.SelectCharacter(id);
		UpdateLabel();
	}
	
	private void _on_yes_button_pressed()
	{
		SoundManager.Instance.PlayButton();

		if (_pendingCharacter == -1)
			return;

		if (Player.BuyCharacter(_pendingCharacter))
		{
			Player.SelectCharacter(_pendingCharacter);

			UpdateCharacterVisuals();
			UpdateLabel();
		}

		_confirmPanel.Visible = false;
		_pendingCharacter = -1;
	}
	
	private void _on_no_button_pressed()
	{
		SoundManager.Instance.PlayButton();

		_confirmPanel.Visible = false;

		_pendingCharacter = -1;
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
}
