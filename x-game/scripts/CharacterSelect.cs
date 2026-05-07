using Godot;
using System.Collections.Generic;

public partial class CharacterSelect : Control
{
    private readonly GameData _gameData = new();
    private VBoxContainer _characterList = null!;
    private Label _detailLabel = null!;
    private TextureRect _portraitTexture = null!;
    private PanelContainer _portraitFrame = null!;
    private Panel _loadingOverlay = null!;
    private Button _startButton = null!;
    private Button _backButton = null!;
    private Label _loadingLabel = null!;
    private Label _titleLabel = null!;
    private string _selectedCharacterId = "miner";
    private bool _loadingActive;
    private readonly Dictionary<string, bool> _characterUnlocked = new();
    private readonly Dictionary<string, Button> _characterButtons = new();

    public override void _Ready()
    {
        Localization.LoadSettings();
        AddChild(_gameData);
        _gameData.LoadAll();

        _characterList = GetNode<VBoxContainer>("Root/Margin/MainLayout/ContentRow/CharacterScroll/CharacterList");
        _titleLabel = GetNode<Label>("Root/Margin/MainLayout/TitleLabel");
        _detailLabel = GetNode<Label>("Root/Margin/MainLayout/ContentRow/DetailPanel/DetailLayout/DetailLabel");
        _portraitTexture = GetNode<TextureRect>("Root/Margin/MainLayout/ContentRow/DetailPanel/DetailLayout/PortraitFrame/PortraitTexture");
        _portraitFrame = GetNode<PanelContainer>("Root/Margin/MainLayout/ContentRow/DetailPanel/DetailLayout/PortraitFrame");
        _startButton = GetNode<Button>("Root/Margin/MainLayout/ButtonRow/StartButton");
        _backButton = GetNode<Button>("Root/Margin/MainLayout/ButtonRow/BackButton");

        _startButton.Pressed += OnStartPressed;
        _backButton.Pressed += () => ChangeSceneWithLoading(
            "res://scenes/MainMenu.tscn",
            Localization.Language == Localization.English ? "Returning to the bedroom..." : "返回卧室……");

        ApplyUiStyle();
        BuildLoadingOverlay();
        RenderCharacters();
        SelectCharacter(_selectedCharacterId);
    }

    private void RenderCharacters()
    {
        _characterButtons.Clear();
        _characterUnlocked.Clear();
        foreach (var character in _gameData.Characters.Characters)
        {
            var unlocked = SaveManager.IsUnlocked(character.UnlockId);
            var button = new Button
            {
                Text = unlocked
                    ? $"{character.DisplayName()}\n{Localization.T("hp")} {character.MaxHp}  |  {Localization.T("shards")} {character.Shards}\n{character.DisplayDescription()}"
                    : $"🔒 {character.DisplayName()}\n{(Localization.Language == Localization.English ? "Unlock from main menu." : "请在主菜单解锁。")}",
                CustomMinimumSize = new Vector2(420, 112),
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
                Disabled = !unlocked
            };
            var id = character.Id;
            button.Pressed += () => SelectCharacter(id);
            StyleButton(button, Color.FromHtml("263445"), Color.FromHtml("d8e2ee"));
            _characterButtons[id] = button;
            _characterUnlocked[id] = unlocked;
            _characterList.AddChild(button);
        }
    }

    private void SelectCharacter(string characterId)
    {
        _selectedCharacterId = characterId;
        var character = _gameData.GetCharacter(characterId);
        if (!SaveManager.IsUnlocked(character.UnlockId))
        {
            return;
        }
        var items = "";
        foreach (var item in character.StartingItems)
        {
            var itemData = _gameData.GetItem(item.ItemId);
            items += $"\n- {itemData.DisplayName()} x{item.Count}: {itemData.DisplayDescription()}";
        }

        _titleLabel.Text = Localization.T("choose_character");
        _startButton.Text = Localization.T("start_explore");
        _backButton.Text = Localization.T("back");
        _portraitTexture.Texture = LoadTexture(character.ArtPath);
        _detailLabel.Text = $"{character.DisplayName()}\n\n{character.DisplayDescription()}\n\n{Localization.T("hp")}: {character.MaxHp}\n{Localization.T("shards")}: {character.Shards}\n{Localization.T("start_items")}:{items}";
        RefreshSelectionStyles();
    }

    private void OnStartPressed()
    {
        GameSession.SelectedCharacterId = _selectedCharacterId;
        GameSession.LoadRequested = false;
        ChangeSceneWithLoading(
            "res://scenes/BattleScene.tscn",
            Localization.Language == Localization.English ? "Climbing through the trapdoor..." : "钻进床下暗门……");
    }

    private void ApplyUiStyle()
    {
        MistTheme.ApplyRoot(GetNode<Panel>("Root"), "character_select");
        MistTheme.StyleLabel(_titleLabel);
        MistTheme.StyleLabel(_detailLabel);
        MistTheme.StylePanel(GetNode<PanelContainer>("Root/Margin/MainLayout/ContentRow/DetailPanel"), MistPanelVariant.Stone);
        MistTheme.StylePanel(_portraitFrame, MistPanelVariant.Purple);
        MistTheme.StyleButton(_startButton, MistButtonVariant.Primary);
        MistTheme.StyleButton(_backButton, MistButtonVariant.Neutral);
    }

    private void BuildLoadingOverlay()
    {
        _loadingOverlay = new Panel
        {
            Name = "LoadingOverlay",
            Visible = false,
            MouseFilter = MouseFilterEnum.Stop,
            ZIndex = 200
        };
        _loadingOverlay.SetAnchorsPreset(LayoutPreset.FullRect);
        _loadingOverlay.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = new Color(0.02f, 0.04f, 0.06f, 0.84f)
        });

        var center = new CenterContainer
        {
            Name = "LoadingCenter",
            MouseFilter = MouseFilterEnum.Ignore
        };
        center.SetAnchorsPreset(LayoutPreset.FullRect);

        var card = new PanelContainer
        {
            Name = "LoadingCard",
            CustomMinimumSize = new Vector2(420, 132),
            MouseFilter = MouseFilterEnum.Ignore
        };
        card.AddThemeStyleboxOverride("panel", MakePanelStyle("121c28", "8df0bd", 2));

        var layout = new VBoxContainer
        {
            Name = "LoadingLayout",
            MouseFilter = MouseFilterEnum.Ignore
        };
        layout.AddThemeConstantOverride("separation", 10);

        var title = new Label
        {
            Text = Localization.Language == Localization.English ? "Loading" : "载入中",
            HorizontalAlignment = HorizontalAlignment.Center,
            MouseFilter = MouseFilterEnum.Ignore
        };
        title.AddThemeFontSizeOverride("font_size", 24);
        title.AddThemeColorOverride("font_color", Color.FromHtml("f4f0df"));

        _loadingLabel = new Label
        {
            Name = "LoadingLabel",
            HorizontalAlignment = HorizontalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            MouseFilter = MouseFilterEnum.Ignore
        };
        _loadingLabel.AddThemeFontSizeOverride("font_size", 17);
        _loadingLabel.AddThemeColorOverride("font_color", Color.FromHtml("b8c7d5"));

        layout.AddChild(title);
        layout.AddChild(_loadingLabel);
        card.AddChild(layout);
        center.AddChild(card);
        _loadingOverlay.AddChild(center);
        GetNode<Panel>("Root").AddChild(_loadingOverlay);
    }

    private async void ChangeSceneWithLoading(string scenePath, string text)
    {
        if (_loadingActive)
        {
            return;
        }

        _loadingActive = true;
        _loadingLabel.Text = text;
        _loadingOverlay.Visible = true;
        await ToSignal(GetTree().CreateTimer(0.35), SceneTreeTimer.SignalName.Timeout);
        GetTree().ChangeSceneToFile(scenePath);
    }

    private static Texture2D? LoadTexture(string path)
    {
        return UiArt.LoadTexture(path);
    }

    private void RefreshSelectionStyles()
    {
        foreach (var pair in _characterButtons)
        {
            var selected = pair.Key == _selectedCharacterId;
            var unlocked = _characterUnlocked.TryGetValue(pair.Key, out var value) && value;
            StyleButton(pair.Value, !unlocked ? Color.FromHtml("303946") : selected ? Color.FromHtml("315f46") : Color.FromHtml("263445"), selected ? Color.FromHtml("e7fff1") : Color.FromHtml("d8e2ee"));
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
