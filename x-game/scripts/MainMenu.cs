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
    private Button _cardLibraryButton = null!;
    private Button _backButton = null!;
    private Button _unlocksBackButton = null!;
    private Button _cardLibraryBackButton = null!;
    private PanelContainer _settingsPanel = null!;
    private PanelContainer _unlocksPanel = null!;
    private PanelContainer _cardLibraryPanel = null!;
    private Panel _modalOverlay = null!;
    private VBoxContainer _modalStack = null!;
    private Label _modalMessageLabel = null!;
    private VBoxContainer _unlocksList = null!;
    private VBoxContainer _cardLibraryList = null!;
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
        _cardLibraryButton = GetNode<Button>("Root/Margin/MenuLayout/CardLibraryButton");
        _settingsPanel = GetNode<PanelContainer>("Root/Margin/MenuLayout/SettingsPanel");
        _unlocksPanel = GetNode<PanelContainer>("Root/Margin/MenuLayout/UnlocksPanel");
        _cardLibraryPanel = GetNode<PanelContainer>("Root/Margin/MenuLayout/CardLibraryPanel");
        _unlocksList = GetNode<VBoxContainer>("Root/Margin/MenuLayout/UnlocksPanel/UnlocksLayout/UnlocksList");
        _cardLibraryList = GetNode<VBoxContainer>("Root/Margin/MenuLayout/CardLibraryPanel/CardLibraryLayout/CardLibraryScroll/CardLibraryList");
        _languageLabel = GetNode<Label>("Root/Margin/MenuLayout/SettingsPanel/SettingsLayout/LanguageLabel");
        _languageOption = GetNode<OptionButton>("Root/Margin/MenuLayout/SettingsPanel/SettingsLayout/LanguageOption");
        _backButton = GetNode<Button>("Root/Margin/MenuLayout/SettingsPanel/SettingsLayout/BackButton");
        _unlocksBackButton = GetNode<Button>("Root/Margin/MenuLayout/UnlocksPanel/UnlocksLayout/UnlocksBackButton");
        _cardLibraryBackButton = GetNode<Button>("Root/Margin/MenuLayout/CardLibraryPanel/CardLibraryLayout/CardLibraryBackButton");
        _messageLabel = GetNode<Label>("Root/Margin/MenuLayout/MessageLabel");
        BuildModalHost();

        _languageOption.Clear();
        _languageOption.AddItem("中文", 0);
        _languageOption.AddItem("English", 1);
        _languageOption.ItemSelected += OnLanguageSelected;

        _newGameButton.Pressed += OnNewGamePressed;
        _continueButton.Pressed += OnContinuePressed;
        _settingsButton.Pressed += OnSettingsPressed;
        _unlocksButton.Pressed += OnUnlocksPressed;
        _cardLibraryButton.Pressed += OnCardLibraryPressed;
        _backButton.Pressed += OnBackPressed;
        _unlocksBackButton.Pressed += OnUnlocksBackPressed;
        _cardLibraryBackButton.Pressed += OnCardLibraryBackPressed;

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
        ShowSubPage(_settingsPanel);
        _messageLabel.Text = string.Empty;
    }

    private void OnUnlocksPressed()
    {
        ShowSubPage(_unlocksPanel);
        RenderUnlocks();
    }

    private void OnCardLibraryPressed()
    {
        ShowSubPage(_cardLibraryPanel);
        RenderCardLibrary();
    }

    private void OnBackPressed()
    {
        ShowMainPage();
    }

    private void OnUnlocksBackPressed()
    {
        ShowMainPage();
    }

    private void OnCardLibraryBackPressed()
    {
        ShowMainPage();
    }

    private void OnLanguageSelected(long index)
    {
        Localization.SetLanguage(index == 1 ? Localization.English : Localization.Chinese);
        RenderText();
    }

    private void ShowSubPage(PanelContainer activePanel)
    {
        _modalOverlay.Visible = true;
        _settingsPanel.Visible = activePanel == _settingsPanel;
        _unlocksPanel.Visible = activePanel == _unlocksPanel;
        _cardLibraryPanel.Visible = activePanel == _cardLibraryPanel;
    }

    private void ShowMainPage()
    {
        _modalOverlay.Visible = false;
        _settingsPanel.Visible = false;
        _unlocksPanel.Visible = false;
        _cardLibraryPanel.Visible = false;
        RenderText();
    }

    private void SetMainControlsVisible(bool visible)
    {
        _titleLabel.Visible = visible;
        _subtitleLabel.Visible = visible;
        _newGameButton.Visible = visible;
        _continueButton.Visible = visible;
        _settingsButton.Visible = visible;
        _unlocksButton.Visible = visible;
        _cardLibraryButton.Visible = visible;
    }

    private void RenderText()
    {
        _titleLabel.Text = Localization.T("game_title");
        _subtitleLabel.Text = Localization.T("game_subtitle");
        _newGameButton.Text = Localization.T("new_game");
        _continueButton.Text = Localization.T("continue_game");
        _settingsButton.Text = Localization.T("settings");
        _unlocksButton.Text = Localization.Language == Localization.English ? "Unlocks" : "解锁";
        _cardLibraryButton.Text = Localization.Language == Localization.English ? "Card Library" : "卡牌库";
        _languageLabel.Text = Localization.T("language");
        var closeText = Localization.Language == Localization.English ? "Close" : "关闭";
        _backButton.Text = closeText;
        _unlocksBackButton.Text = closeText;
        _cardLibraryBackButton.Text = closeText;
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
        _modalMessageLabel.Text = Localization.Language == Localization.English
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
                _modalMessageLabel.Text = message;
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

    private void RenderCardLibrary()
    {
        ClearBox(_cardLibraryList);
        _modalMessageLabel.Text = Localization.Language == Localization.English
            ? "Cards are data-driven. Pools decide which heroes can find them."
            : "卡牌由数据配置驱动。牌池决定哪些英雄能在奖励和商店中遇到它们。";

        foreach (var card in _gameData.Cards.Cards)
        {
            if (card.UpgradeOnly)
            {
                continue;
            }

            var pools = card.Pools.Count == 0
                ? (Localization.Language == Localization.English ? "All heroes" : "全职业")
                : string.Join(", ", card.Pools);
            var upgrade = string.IsNullOrWhiteSpace(card.UpgradeTo)
                ? (Localization.Language == Localization.English ? "No upgrade" : "无升级")
                : $"{(Localization.Language == Localization.English ? "Upgrades to" : "升级为")} {_gameData.GetCard(card.UpgradeTo).DisplayName()}";
            var unlock = SaveManager.IsUnlocked(card.UnlockId)
                ? string.Empty
                : $"\n{(Localization.Language == Localization.English ? "Locked by" : "解锁需求")} {card.UnlockId}";
            var button = new Button
            {
                Text = $"{FormatCardHeader(card)}\n{card.DisplayDescription()}\n{(Localization.Language == Localization.English ? "Pool" : "牌池")}: {pools} | {upgrade}{unlock}",
                CustomMinimumSize = new Vector2(0, 118),
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
                Disabled = true
            };
            StyleCardButton(button, card);
            _cardLibraryList.AddChild(button);
        }
    }

    private static string FormatCardHeader(CardData card)
    {
        var rarity = card.Rarity switch
        {
            "rare" => Localization.Language == Localization.English ? "Rare" : "稀有",
            "uncommon" => Localization.Language == Localization.English ? "Uncommon" : "进阶",
            _ => Localization.Language == Localization.English ? "Common" : "普通"
        };
        return $"{card.DisplayName()} [{rarity}/{card.Type}] ({Localization.T("cost")} {card.Cost})";
    }

    private void BuildModalHost()
    {
        _modalOverlay = new Panel
        {
            Name = "ModalOverlay",
            Visible = false,
            MouseFilter = MouseFilterEnum.Stop
        };
        _modalOverlay.SetAnchorsPreset(LayoutPreset.FullRect);
        _modalOverlay.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = new Color(0.02f, 0.05f, 0.07f, 0.78f)
        });

        var center = new CenterContainer
        {
            Name = "ModalCenter",
            MouseFilter = MouseFilterEnum.Pass
        };
        center.SetAnchorsPreset(LayoutPreset.FullRect);

        _modalStack = new VBoxContainer
        {
            Name = "ModalStack",
            CustomMinimumSize = new Vector2(780, 0)
        };
        _modalStack.AddThemeConstantOverride("separation", 12);

        MovePanelToModal(_settingsPanel);
        MovePanelToModal(_unlocksPanel);
        MovePanelToModal(_cardLibraryPanel);

        _modalMessageLabel = new Label
        {
            Name = "ModalMessageLabel",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        _modalStack.AddChild(_modalMessageLabel);

        center.AddChild(_modalStack);
        _modalOverlay.AddChild(center);
        GetNode<Panel>("Root").AddChild(_modalOverlay);
    }

    private void MovePanelToModal(PanelContainer panel)
    {
        panel.GetParent()?.RemoveChild(panel);
        _modalStack.AddChild(panel);
    }

    private void ApplyUiStyle()
    {
        GetNode<Panel>("Root").AddThemeStyleboxOverride("panel", MakePanelStyle("101820", "283748", 0));
        _titleLabel.AddThemeColorOverride("font_color", Color.FromHtml("f4f0df"));
        _subtitleLabel.AddThemeColorOverride("font_color", Color.FromHtml("b8c7d5"));
        _languageLabel.AddThemeColorOverride("font_color", Color.FromHtml("dbe6ef"));
        _messageLabel.AddThemeColorOverride("font_color", Color.FromHtml("d6e2ec"));
        _modalMessageLabel.AddThemeColorOverride("font_color", Color.FromHtml("c9d8e5"));
        _settingsPanel.AddThemeStyleboxOverride("panel", MakePanelStyle("182331", "3a5068", 1));
        _unlocksPanel.AddThemeStyleboxOverride("panel", MakePanelStyle("182331", "5b4a2a", 1));
        _cardLibraryPanel.AddThemeStyleboxOverride("panel", MakePanelStyle("182331", "3a5068", 1));
        StyleButton(_newGameButton, Color.FromHtml("315f46"), Color.FromHtml("e7fff1"));
        StyleButton(_continueButton, Color.FromHtml("263f5a"), Color.FromHtml("e4f0ff"));
        StyleButton(_settingsButton, Color.FromHtml("403547"), Color.FromHtml("f0e4ff"));
        StyleButton(_unlocksButton, Color.FromHtml("5b4a2a"), Color.FromHtml("fff1d0"));
        StyleButton(_cardLibraryButton, Color.FromHtml("2f4c54"), Color.FromHtml("e4fbff"));
        StyleButton(_backButton, Color.FromHtml("303946"), Color.FromHtml("eef5ff"));
        StyleButton(_unlocksBackButton, Color.FromHtml("303946"), Color.FromHtml("eef5ff"));
        StyleButton(_cardLibraryBackButton, Color.FromHtml("303946"), Color.FromHtml("eef5ff"));
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

    private static void StyleCardButton(Button button, CardData card)
    {
        var border = card.Rarity switch
        {
            "rare" => Color.FromHtml("d7b45f"),
            "uncommon" => Color.FromHtml("78a8d8"),
            _ => Color.FromHtml("7d8a96")
        };
        var background = card.Type == "attack" ? Color.FromHtml("2b2324") : Color.FromHtml("1d2b35");
        var normal = MakeCardStyle(background, border);
        button.AddThemeStyleboxOverride("normal", normal);
        button.AddThemeStyleboxOverride("disabled", normal);
        button.AddThemeColorOverride("font_color", Color.FromHtml("f3ead7"));
        button.AddThemeColorOverride("font_disabled_color", Color.FromHtml("f3ead7"));
    }

    private static StyleBoxFlat MakeCardStyle(Color background, Color border)
    {
        var style = new StyleBoxFlat
        {
            BgColor = background,
            BorderColor = border,
            CornerRadiusTopLeft = 8,
            CornerRadiusTopRight = 8,
            CornerRadiusBottomLeft = 8,
            CornerRadiusBottomRight = 8,
            ContentMarginLeft = 12,
            ContentMarginTop = 10,
            ContentMarginRight = 12,
            ContentMarginBottom = 10,
            ShadowColor = new Color(0, 0, 0, 0.35f),
            ShadowSize = 4
        };
        style.SetBorderWidthAll(2);
        return style;
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
