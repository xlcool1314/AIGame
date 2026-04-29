using Godot;

public partial class MainMenu : Control
{
    private readonly GameData _gameData = new();
    private Label _titleLabel = null!;
    private Label _subtitleLabel = null!;
    private Button _newGameButton = null!;
    private Button _continueButton = null!;
    private Button _settingsButton = null!;
    private Button _unlocksButton = null!;
    private Button _backButton = null!;
    private Button _unlocksBackButton = null!;
    private PanelContainer _settingsPanel = null!;
    private PanelContainer _unlocksPanel = null!;
    private VBoxContainer _unlocksList = null!;
    private Label _languageLabel = null!;
    private OptionButton _languageOption = null!;
    private Label _messageLabel = null!;

    public override void _Ready()
    {
        Localization.LoadSettings();
        AddChild(_gameData);
        _gameData.LoadAll();

        _titleLabel = GetNode<Label>("Root/Margin/MenuLayout/TitleLabel");
        _subtitleLabel = GetNode<Label>("Root/Margin/MenuLayout/SubtitleLabel");
        _newGameButton = GetNode<Button>("Root/Margin/MenuLayout/NewGameButton");
        _continueButton = GetNode<Button>("Root/Margin/MenuLayout/ContinueButton");
        _settingsButton = GetNode<Button>("Root/Margin/MenuLayout/SettingsButton");
        _unlocksButton = GetNode<Button>("Root/Margin/MenuLayout/UnlocksButton");
        _settingsPanel = GetNode<PanelContainer>("Root/Margin/MenuLayout/SettingsPanel");
        _unlocksPanel = GetNode<PanelContainer>("Root/Margin/MenuLayout/UnlocksPanel");
        _unlocksList = GetNode<VBoxContainer>("Root/Margin/MenuLayout/UnlocksPanel/UnlocksLayout/UnlocksList");
        _languageLabel = GetNode<Label>("Root/Margin/MenuLayout/SettingsPanel/SettingsLayout/LanguageLabel");
        _languageOption = GetNode<OptionButton>("Root/Margin/MenuLayout/SettingsPanel/SettingsLayout/LanguageOption");
        _backButton = GetNode<Button>("Root/Margin/MenuLayout/SettingsPanel/SettingsLayout/BackButton");
        _unlocksBackButton = GetNode<Button>("Root/Margin/MenuLayout/UnlocksPanel/UnlocksLayout/UnlocksBackButton");
        _messageLabel = GetNode<Label>("Root/Margin/MenuLayout/MessageLabel");

        _languageOption.Clear();
        _languageOption.AddItem("中文", 0);
        _languageOption.AddItem("English", 1);
        _languageOption.ItemSelected += OnLanguageSelected;

        _newGameButton.Pressed += OnNewGamePressed;
        _continueButton.Pressed += OnContinuePressed;
        _settingsButton.Pressed += OnSettingsPressed;
        _unlocksButton.Pressed += OnUnlocksPressed;
        _backButton.Pressed += OnBackPressed;
        _unlocksBackButton.Pressed += OnUnlocksBackPressed;

        ApplyUiStyle();
        RenderText();
    }

    private void OnNewGamePressed()
    {
        GameSession.LoadRequested = false;
        GetTree().ChangeSceneToFile("res://scenes/CharacterSelect.tscn");
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
        _unlocksPanel.Visible = false;
        _messageLabel.Text = string.Empty;
    }

    private void OnUnlocksPressed()
    {
        _settingsPanel.Visible = false;
        _unlocksPanel.Visible = true;
        RenderUnlocks();
    }

    private void OnBackPressed()
    {
        _settingsPanel.Visible = false;
    }

    private void OnUnlocksBackPressed()
    {
        _unlocksPanel.Visible = false;
        RenderText();
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
        _unlocksButton.Text = Localization.Language == Localization.English ? "Unlocks" : "解锁";
        _languageLabel.Text = Localization.T("language");
        _backButton.Text = Localization.T("back");
        _unlocksBackButton.Text = Localization.T("back");
        _languageOption.Select(Localization.Language == Localization.English ? 1 : 0);
        var meta = SaveManager.LoadMeta();
        _messageLabel.Text = Localization.Language == Localization.English
            ? $"Embers {meta.TotalEmbers} | Best depth {meta.BestDepth} | Best score {meta.BestScore} | Commissions {meta.CompletedObjectiveIds.Count}"
            : $"余烬 {meta.TotalEmbers} | 最深层数 {meta.BestDepth} | 最高分 {meta.BestScore} | 完成委托 {meta.CompletedObjectiveIds.Count}";
    }

    private void RenderUnlocks()
    {
        ClearBox(_unlocksList);
        var meta = SaveManager.LoadMeta();
        _messageLabel.Text = Localization.Language == Localization.English
            ? $"Available embers: {meta.TotalEmbers}"
            : $"可用余烬：{meta.TotalEmbers}";

        foreach (var unlock in _gameData.Unlocks.Unlocks)
        {
            var unlocked = meta.UnlockedIds.Contains(unlock.Id);
            var requirementsMet = SaveManager.MeetsUnlockRequirements(unlock, meta, out var requirementText);
            var canBuy = !unlocked && requirementsMet && meta.TotalEmbers >= unlock.Cost;
            var button = new Button
            {
                Text = unlocked
                    ? $"{unlock.DisplayTitle()}\n{unlock.DisplayDescription()}\n{(Localization.Language == Localization.English ? "Unlocked" : "已解锁")}"
                    : $"{unlock.DisplayTitle()} - {unlock.Cost} {(Localization.Language == Localization.English ? "Embers" : "余烬")}\n{unlock.DisplayDescription()}{FormatRequirementLine(requirementText)}",
                CustomMinimumSize = new Vector2(0, 76),
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
                Disabled = unlocked || !canBuy
            };
            StyleButton(button, unlocked ? Color.FromHtml("303946") : canBuy ? Color.FromHtml("5b4a2a") : Color.FromHtml("3b3440"), Color.FromHtml("eef5ff"));
            button.Pressed += () =>
            {
                SaveManager.TryUnlock(unlock, out var message);
                _messageLabel.Text = message;
                RenderUnlocks();
            };
            _unlocksList.AddChild(button);
        }
    }

    private static string FormatRequirementLine(string requirementText)
    {
        if (string.IsNullOrWhiteSpace(requirementText))
        {
            return string.Empty;
        }

        return $"\n{requirementText}";
    }

    private void ApplyUiStyle()
    {
        GetNode<Panel>("Root").AddThemeStyleboxOverride("panel", MakePanelStyle("101820", "283748", 0));
        _settingsPanel.AddThemeStyleboxOverride("panel", MakePanelStyle("182331", "3a5068", 1));
        _unlocksPanel.AddThemeStyleboxOverride("panel", MakePanelStyle("182331", "5b4a2a", 1));
        StyleButton(_newGameButton, Color.FromHtml("315f46"), Color.FromHtml("e7fff1"));
        StyleButton(_continueButton, Color.FromHtml("263f5a"), Color.FromHtml("e4f0ff"));
        StyleButton(_settingsButton, Color.FromHtml("403547"), Color.FromHtml("f0e4ff"));
        StyleButton(_unlocksButton, Color.FromHtml("5b4a2a"), Color.FromHtml("fff1d0"));
        StyleButton(_backButton, Color.FromHtml("303946"), Color.FromHtml("eef5ff"));
        StyleButton(_unlocksBackButton, Color.FromHtml("303946"), Color.FromHtml("eef5ff"));
    }

    private static void ClearBox(Container container)
    {
        foreach (var child in container.GetChildren())
        {
            container.RemoveChild(child);
            child.QueueFree();
        }
    }

    private static StyleBoxFlat MakePanelStyle(string background, string border, int borderWidth)
    {
        var style = new StyleBoxFlat
        {
            BgColor = Color.FromHtml(background),
            BorderColor = Color.FromHtml(border),
            CornerRadiusTopLeft = 8,
            CornerRadiusTopRight = 8,
            CornerRadiusBottomLeft = 8,
            CornerRadiusBottomRight = 8,
            ContentMarginLeft = 12,
            ContentMarginTop = 12,
            ContentMarginRight = 12,
            ContentMarginBottom = 12
        };
        style.SetBorderWidthAll(borderWidth);
        return style;
    }

    private static void StyleButton(Button button, Color background, Color fontColor)
    {
        button.AddThemeStyleboxOverride("normal", MakeButtonStyle(background));
        button.AddThemeStyleboxOverride("hover", MakeButtonStyle(background.Lightened(0.12f)));
        button.AddThemeStyleboxOverride("pressed", MakeButtonStyle(background.Darkened(0.12f)));
        button.AddThemeColorOverride("font_color", fontColor);
        button.AddThemeColorOverride("font_hover_color", fontColor.Lightened(0.08f));
    }

    private static StyleBoxFlat MakeButtonStyle(Color background)
    {
        var style = new StyleBoxFlat
        {
            BgColor = background,
            BorderColor = background.Lightened(0.18f),
            CornerRadiusTopLeft = 6,
            CornerRadiusTopRight = 6,
            CornerRadiusBottomLeft = 6,
            CornerRadiusBottomRight = 6,
            ContentMarginLeft = 10,
            ContentMarginTop = 8,
            ContentMarginRight = 10,
            ContentMarginBottom = 8
        };
        style.SetBorderWidthAll(1);
        return style;
    }
}
