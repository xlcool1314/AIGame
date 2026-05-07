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
	private Button _exitButton = null!;
	private Button _backButton = null!;
	private Button _unlocksBackButton = null!;
	private Button _cardLibraryBackButton = null!;
	private TextureRect _heroTexture = null!;
	private PanelContainer _settingsPanel = null!;
	private PanelContainer _unlocksPanel = null!;
	private PanelContainer _cardLibraryPanel = null!;
	private Panel _modalOverlay = null!;
	private Panel _loadingOverlay = null!;
	private VBoxContainer _modalStack = null!;
	private Label _modalMessageLabel = null!;
	private Label _loadingLabel = null!;
	private VBoxContainer _unlocksList = null!;
	private GridContainer _cardLibraryList = null!;
	private Label _languageLabel = null!;
	private OptionButton _languageOption = null!;
	private Label _messageLabel = null!;
	private bool _loadingActive;

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
		_exitButton = GetNode<Button>("Root/Margin/MenuLayout/ExitButton");
		_heroTexture = GetNode<TextureRect>("Root/HeroTexture");
		_settingsPanel = GetNode<PanelContainer>("Root/Margin/MenuLayout/SettingsPanel");
		_unlocksPanel = GetNode<PanelContainer>("Root/Margin/MenuLayout/UnlocksPanel");
		_cardLibraryPanel = GetNode<PanelContainer>("Root/Margin/MenuLayout/CardLibraryPanel");
		_unlocksList = GetNode<VBoxContainer>("Root/Margin/MenuLayout/UnlocksPanel/UnlocksLayout/UnlocksList");
		_cardLibraryList = GetNode<GridContainer>("Root/Margin/MenuLayout/CardLibraryPanel/CardLibraryLayout/CardLibraryScroll/CardLibraryList");
		_languageLabel = GetNode<Label>("Root/Margin/MenuLayout/SettingsPanel/SettingsLayout/LanguageLabel");
		_languageOption = GetNode<OptionButton>("Root/Margin/MenuLayout/SettingsPanel/SettingsLayout/LanguageOption");
		_backButton = GetNode<Button>("Root/Margin/MenuLayout/SettingsPanel/SettingsLayout/BackButton");
		_unlocksBackButton = GetNode<Button>("Root/Margin/MenuLayout/UnlocksPanel/UnlocksLayout/UnlocksBackButton");
		_cardLibraryBackButton = GetNode<Button>("Root/Margin/MenuLayout/CardLibraryPanel/CardLibraryLayout/CardLibraryBackButton");
		_messageLabel = GetNode<Label>("Root/Margin/MenuLayout/MessageLabel");
		BuildModalHost();
		_heroTexture.Texture = UiArt.LoadBackground("main_menu") ?? _heroTexture.Texture;

		_languageOption.Clear();
		_languageOption.AddItem("中文", 0);
		_languageOption.AddItem("English", 1);
		_languageOption.ItemSelected += OnLanguageSelected;

		_newGameButton.Pressed += OnNewGamePressed;
		_continueButton.Pressed += OnContinuePressed;
		_settingsButton.Pressed += OnSettingsPressed;
		_unlocksButton.Pressed += OnUnlocksPressed;
		_cardLibraryButton.Pressed += OnCardLibraryPressed;
		_exitButton.Pressed += OnExitPressed;
		_backButton.Pressed += OnBackPressed;
		_unlocksBackButton.Pressed += OnUnlocksBackPressed;
		_cardLibraryBackButton.Pressed += OnCardLibraryBackPressed;

		ApplyUiStyle();
		RenderText();
	}

	private void OnNewGamePressed()
	{
		GameSession.LoadRequested = false;
		ChangeSceneWithLoading(
			"res://scenes/CharacterSelect.tscn",
			Localization.T("loading_choose_kid"));
	}

	private void OnContinuePressed()
	{
		if (!SaveManager.HasSave())
		{
			_messageLabel.Text = Localization.T("no_save");
			return;
		}

		GameSession.LoadRequested = true;
		ChangeSceneWithLoading(
			"res://scenes/BattleScene.tscn",
			Localization.T("loading_reopen_trapdoor"));
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

	private void OnExitPressed()
	{
		GetTree().Quit();
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
		_exitButton.Visible = visible;
	}

	private void RenderText()
	{
		_titleLabel.Text = Localization.T("game_title");
		_subtitleLabel.Text = Localization.T("game_subtitle");
		_newGameButton.Text = Localization.T("new_game");
		_continueButton.Text = Localization.T("continue_game");
		_settingsButton.Text = Localization.T("settings");
		_unlocksButton.Text = Localization.T("unlocks");
		_cardLibraryButton.Text = Localization.T("card_library");
		_exitButton.Text = Localization.T("exit_game");
		_languageLabel.Text = Localization.T("language");
		var closeText = Localization.T("close");
		_backButton.Text = closeText;
		_unlocksBackButton.Text = closeText;
		_cardLibraryBackButton.Text = closeText;
		_languageOption.Select(Localization.Language == Localization.English ? 1 : 0);
		var meta = SaveManager.LoadMeta();
		_messageLabel.Text = Localization.T("main_meta", meta.TotalEmbers, meta.BestDepth, meta.BestScore, meta.CompletedObjectiveIds.Count);
	}

	private void RenderUnlocks()
	{
		ClearBox(_unlocksList);
		var meta = SaveManager.LoadMeta();
		_modalMessageLabel.Text = Localization.T("available_stardust", meta.TotalEmbers);

		foreach (var unlock in _gameData.Unlocks.Unlocks)
		{
			var unlocked = meta.UnlockedIds.Contains(unlock.Id);
			var requirementsMet = SaveManager.MeetsUnlockRequirements(unlock, meta, out var requirementText);
			var canBuy = !unlocked && requirementsMet && meta.TotalEmbers >= unlock.Cost;
			var button = new Button
			{
				Text = unlocked
					? $"{unlock.DisplayTitle()}\n{unlock.DisplayDescription()}\n{Localization.T("unlocked")}"
					: $"{unlock.DisplayTitle()} - {unlock.Cost} {Localization.T("stardust")}\n{unlock.DisplayDescription()}{FormatRequirementLine(requirementText)}",
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
		_modalMessageLabel.Text = Localization.T("card_library_desc");

		foreach (var card in _gameData.Cards.Cards)
		{
			if (card.UpgradeOnly)
			{
				continue;
			}

			var pools = card.Pools.Count == 0
				? Localization.T("all_heroes")
				: string.Join(", ", card.Pools);
			var upgrade = string.IsNullOrWhiteSpace(card.UpgradeTo)
				? Localization.T("no_upgrade")
				: $"{Localization.T("upgrades_to")} {_gameData.GetCard(card.UpgradeTo).DisplayName()}";
			var unlock = SaveManager.IsUnlocked(card.UnlockId)
				? string.Empty
				: $"\n{Localization.T("locked_by")} {card.UnlockId}";
			var footer = $"{Localization.T("pool")}: {pools} | {upgrade}{unlock}";
			var cell = CardCell.Create(card, CardCellMode.Deck, true, footer);
			cell.MouseDefaultCursorShape = Control.CursorShape.Arrow;
			_cardLibraryList.AddChild(cell);
		}
	}

	private static string FormatCardHeader(CardData card)
	{
		var rarity = CardCell.FormatRarityName(card.Rarity);
		var type = CardCell.FormatTypeName(card.Type);
		return $"{card.DisplayName()} [{rarity}/{type}] ({Localization.T("cost")} {card.Cost})";
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

		BuildLoadingOverlay();
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
			Text = Localization.T("loading"),
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

	private void MovePanelToModal(PanelContainer panel)
	{
		panel.GetParent()?.RemoveChild(panel);
		_modalStack.AddChild(panel);
	}

	private void ApplyUiStyle()
	{
		GetNode<Panel>("Root").AddThemeStyleboxOverride("panel", MistTheme.PanelStyle(MistPanelVariant.Root));
		MistTheme.StyleLabel(_titleLabel);
		MistTheme.StyleLabel(_subtitleLabel, true);
		MistTheme.StyleLabel(_languageLabel);
		MistTheme.StyleLabel(_messageLabel);
		MistTheme.StyleLabel(_modalMessageLabel, true);
		MistTheme.StylePanel(_settingsPanel, MistPanelVariant.Stone);
		MistTheme.StylePanel(_unlocksPanel, MistPanelVariant.Purple);
		MistTheme.StylePanel(_cardLibraryPanel, MistPanelVariant.Stone);
		MistTheme.StyleButton(_newGameButton, MistButtonVariant.Primary);
		MistTheme.StyleButton(_continueButton, MistButtonVariant.Neutral);
		MistTheme.StyleButton(_settingsButton, MistButtonVariant.Purple);
		MistTheme.StyleButton(_unlocksButton, MistButtonVariant.Gold);
		MistTheme.StyleButton(_cardLibraryButton, MistButtonVariant.Neutral);
		MistTheme.StyleButton(_exitButton, MistButtonVariant.Neutral);
		MistTheme.StyleButton(_backButton, MistButtonVariant.Neutral);
		MistTheme.StyleButton(_unlocksBackButton, MistButtonVariant.Neutral);
		MistTheme.StyleButton(_cardLibraryBackButton, MistButtonVariant.Neutral);
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
