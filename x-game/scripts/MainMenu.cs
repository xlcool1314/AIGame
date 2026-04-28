using Godot;

public partial class MainMenu : Control
{
    private Label _titleLabel = null!;
    private Label _subtitleLabel = null!;
    private Button _newGameButton = null!;
    private Button _continueButton = null!;
    private Button _settingsButton = null!;
    private Button _backButton = null!;
    private PanelContainer _settingsPanel = null!;
    private Label _languageLabel = null!;
    private OptionButton _languageOption = null!;
    private Label _messageLabel = null!;

    public override void _Ready()
    {
        Localization.LoadSettings();

        _titleLabel = GetNode<Label>("Root/Margin/MenuLayout/TitleLabel");
        _subtitleLabel = GetNode<Label>("Root/Margin/MenuLayout/SubtitleLabel");
        _newGameButton = GetNode<Button>("Root/Margin/MenuLayout/NewGameButton");
        _continueButton = GetNode<Button>("Root/Margin/MenuLayout/ContinueButton");
        _settingsButton = GetNode<Button>("Root/Margin/MenuLayout/SettingsButton");
        _settingsPanel = GetNode<PanelContainer>("Root/Margin/MenuLayout/SettingsPanel");
        _languageLabel = GetNode<Label>("Root/Margin/MenuLayout/SettingsPanel/SettingsLayout/LanguageLabel");
        _languageOption = GetNode<OptionButton>("Root/Margin/MenuLayout/SettingsPanel/SettingsLayout/LanguageOption");
        _backButton = GetNode<Button>("Root/Margin/MenuLayout/SettingsPanel/SettingsLayout/BackButton");
        _messageLabel = GetNode<Label>("Root/Margin/MenuLayout/MessageLabel");

        _languageOption.Clear();
        _languageOption.AddItem("中文", 0);
        _languageOption.AddItem("English", 1);
        _languageOption.ItemSelected += OnLanguageSelected;

        _newGameButton.Pressed += OnNewGamePressed;
        _continueButton.Pressed += OnContinuePressed;
        _settingsButton.Pressed += OnSettingsPressed;
        _backButton.Pressed += OnBackPressed;

        RenderText();
    }

    private void OnNewGamePressed()
    {
        GameSession.LoadRequested = false;
        GetTree().ChangeSceneToFile("res://scenes/BattleScene.tscn");
    }

    private void OnContinuePressed()
    {
        if (!SaveManager.HasSave())
        {
            _messageLabel.Text = Localization.T("no_save");
            return;
        }

        GameSession.LoadRequested = true;
        GetTree().ChangeSceneToFile("res://scenes/BattleScene.tscn");
    }

    private void OnSettingsPressed()
    {
        _settingsPanel.Visible = true;
        _messageLabel.Text = string.Empty;
    }

    private void OnBackPressed()
    {
        _settingsPanel.Visible = false;
    }

    private void OnLanguageSelected(long index)
    {
        Localization.SetLanguage(index == 1 ? Localization.English : Localization.Chinese);
        RenderText();
    }

    private void RenderText()
    {
        _titleLabel.Text = Localization.T("game_title");
        _subtitleLabel.Text = Localization.T("game_subtitle");
        _newGameButton.Text = Localization.T("new_game");
        _continueButton.Text = Localization.T("continue_game");
        _settingsButton.Text = Localization.T("settings");
        _languageLabel.Text = Localization.T("language");
        _backButton.Text = Localization.T("back");
        _languageOption.Select(Localization.Language == Localization.English ? 1 : 0);
    }
}
