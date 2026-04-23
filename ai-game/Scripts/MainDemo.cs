using System;
using System.Linq;
using AiGame.Data;
using AiGame.Systems;
using AiGame.UI;
using Godot;

namespace AiGame;

public partial class MainDemo : Control
{
	private const float DesignWidth = 1920f;
	private const float DesignHeight = 1080f;
	private const float AutosaveInterval = 30f;

	private GameDatabase _database = null!;
	private GameState _state = null!;

	private float _customerTimer;
	private float _clockTime = 21.3f;
	private float _autosaveTimer;
	private string _lastMessage = string.Empty;

	private Control _canvasRoot = null!;
	private ColorRect _ambientBlobA = null!;
	private ColorRect _ambientBlobB = null!;

	private Label _resourceLabel = null!;
	private Label _messageLabel = null!;
	private Label _clockLabel = null!;
	private Label _customerName = null!;
	private Label _customerLine = null!;
	private Label _toastLabel = null!;
	private Label _shiftLabel = null!;
	private Label _servedLabel = null!;
	private Label _blessingLabel = null!;
	private Label _specialLabel = null!;
	private Label _comboLabel = null!;
	private Label _eventLabel = null!;
	private Label _eventSubLabel = null!;
	private Label _brewLabel = null!;
	private ProgressBar _brewBar = null!;
	private HBoxContainer _buffList = null!;
	private readonly System.Collections.Generic.List<ProgressBar> _patienceBars = new();

	private VBoxContainer _productList = null!;
	private VBoxContainer _customerQueueList = null!;
	private VBoxContainer _shopList = null!;
	private VBoxContainer _decorOwnedList = null!;
	private VBoxContainer _blessingChoiceList = null!;

	private ScrollContainer _productScroll = null!;
	private ScrollContainer _shopScroll = null!;
	private ScrollContainer _ownedScroll = null!;

	private ProgressBar _guestTimerBar = null!;
	private Control _customerVisual = null!;
	private Control _stageVisual = null!;
	private Panel _customerCard = null!;
	private Panel _shopPanel = null!;
	private Panel _ownedDecorPanel = null!;
	private Panel _productPanel = null!;
	private Panel _progressPanel = null!;
	private Panel _blessingModal = null!;
	private ColorRect _modalBlocker = null!;

	private Color _bgColor = new(0.09f, 0.11f, 0.15f);
	private Color _cardColor = new(0.14f, 0.19f, 0.25f);
	private Color _accentColor = new(0.84f, 0.66f, 0.43f);
	private readonly Color _textColor = new(0.95f, 0.94f, 0.90f);
	private readonly Color _mutedTextColor = new(0.77f, 0.80f, 0.84f);
	private readonly Color _positiveColor = new(0.60f, 0.86f, 0.66f);
	private readonly Color _warningColor = new(0.95f, 0.55f, 0.38f);
	private readonly Color _infoColor = new(0.54f, 0.74f, 0.86f);

	public override void _Ready()
	{
		EnsureInputActions();
		_database = GameDatabase.Load();
		_state = new GameState();

		var save = SaveSystem.Load();
		Loc.Set(save?.LanguageCode);
		if (save != null)
		{
			_state.LoadFromSave(save, _database);
		}

		_bgColor = ParseColor(_database.Skin.BackgroundColor, _bgColor);
		_cardColor = ParseColor(_database.Skin.CardColor, _cardColor);
		_accentColor = ParseColor(_database.Skin.AccentColor, _accentColor);

		BuildUi();
		GetViewport().SizeChanged += OnViewportSizeChanged;
		OnViewportSizeChanged();

		_state.EnsureTodaySpecial(_database);
		_state.EnsureCustomer(_database);
		_state.EnsureNightEvent(_database);

		RefreshAll(Loc.Text("ui.message.start"));
	}

	public override void _Notification(int what)
	{
		if (what == NotificationWMCloseRequest || what == NotificationExitTree)
		{
			SaveGame(false);
		}

		if (what == NotificationExitTree && GetViewport() != null)
		{
			GetViewport().SizeChanged -= OnViewportSizeChanged;
		}
	}

	public override void _Process(double delta)
	{
		var completedSale = _state.Tick((float)delta, _database);
		_customerTimer += (float)delta;
		_autosaveTimer += (float)delta;
		_clockTime += (float)delta * 0.03f;

		if (_clockTime >= 24f)
		{
			_clockTime -= 24f;
		}

		if (completedSale.HasValue)
		{
			_customerTimer = 0f;
			HandleCompletedSale(completedSale.Value);
		}

		if (_state.CustomerOrders.Count < 3 && _customerTimer >= 1.5f)
		{
			_customerTimer = 0f;
			_state.EnsureCustomer(_database);
			RefreshCustomerPanel();
		}

		_state.EnsureNightEvent(_database);

		if (_autosaveTimer >= AutosaveInterval)
		{
			_autosaveTimer = 0f;
			SaveGame(true);
		}

		_resourceLabel.Text = Loc.Current == GameLanguage.Chinese
			? $"金币 {_state.Coins}    茶香 {_state.Aroma}    声望 {_state.Reputation}    被动 {MathF.Round(_state.PassiveCoinsPerSecond(_database), 1)}/秒"
			: $"Coins {_state.Coins}    Aroma {_state.Aroma}    Rep {_state.Reputation}    Passive {MathF.Round(_state.PassiveCoinsPerSecond(_database), 1)}/s";

		_clockLabel.Text = Loc.Format("ui.header.clock", FormatClock(_clockTime));
		_guestTimerBar.Value = _state.CustomerOrders.Count < 3 ? Math.Min(_customerTimer / 1.5f, 1f) * 100f : 100f;
		RefreshBrewStatus();
		RefreshPatienceBars();
		RefreshProgressLabels();
		AnimateAmbient();
	}

	private void BuildUi()
	{
		ClearChildren(this);
		SetAnchorsPreset(LayoutPreset.FullRect);

		var background = new ColorRect
		{
			Color = _bgColor,
			AnchorRight = 1,
			AnchorBottom = 1,
			MouseFilter = MouseFilterEnum.Ignore,
		};
		AddChild(background);

		_canvasRoot = new Control
		{
			MouseFilter = MouseFilterEnum.Pass,
		};
		AddChild(_canvasRoot);

		_ambientBlobA = CreateAmbientBlob(new Color(_accentColor, 0.10f), new Vector2(120, 160), new Vector2(420, 420));
		_ambientBlobB = CreateAmbientBlob(new Color(0.42f, 0.66f, 0.78f, 0.08f), new Vector2(1500, 720), new Vector2(360, 360));
		_canvasRoot.AddChild(_ambientBlobA);
		_canvasRoot.AddChild(_ambientBlobB);

		_canvasRoot.AddChild(BuildHeaderPanel());
		_canvasRoot.AddChild(BuildHeroPanel());
		_canvasRoot.AddChild(BuildCustomerPanel());
		_canvasRoot.AddChild(BuildProductPanel());
		_canvasRoot.AddChild(BuildProgressPanel());
		_canvasRoot.AddChild(BuildShopPanel());
		_canvasRoot.AddChild(BuildOwnedDecorPanel());
		_canvasRoot.AddChild(BuildBottomPanel());
		_canvasRoot.AddChild(BuildToastLabel());
		_canvasRoot.AddChild(BuildModalBlocker());
		_canvasRoot.AddChild(BuildBlessingModal());
	}

	private Control BuildModalBlocker()
	{
		_modalBlocker = new ColorRect
		{
			Color = new Color(0.02f, 0.03f, 0.05f, 0.58f),
			Position = Vector2.Zero,
			Size = new Vector2(DesignWidth, DesignHeight),
			Visible = false,
			MouseFilter = MouseFilterEnum.Stop,
			ZIndex = 9,
			FocusMode = FocusModeEnum.All,
		};
		return _modalBlocker;
	}

	private Control BuildHeaderPanel()
	{
		var panel = CreateAbsolutePanel(new Rect2(20, 20, 1880, 92), _cardColor);
		panel.AddChild(CreateAbsoluteLabel(Loc.Text("ui.header.state"), 16, _accentColor, new Rect2(24, 14, 220, 24)));
		panel.AddChild(CreateAbsoluteLabel(Loc.Text("content.title", _database.Skin.Title), 30, _textColor, new Rect2(24, 34, 620, 38)));

		_resourceLabel = CreateAbsoluteLabel(string.Empty, 18, _accentColor, new Rect2(660, 18, 760, 28));
		panel.AddChild(_resourceLabel);
		panel.AddChild(CreateAbsoluteWrappedLabel(Loc.Text("ui.header.mode"), 14, _mutedTextColor, new Rect2(660, 46, 430, 22)));
		panel.AddChild(CreateAbsoluteWrappedLabel(Loc.Text("ui.header.pad"), 14, _mutedTextColor, new Rect2(660, 66, 300, 18)));

		var languageButton = CreateAbsoluteButton(Loc.Text("ui.header.lang"), new Rect2(1490, 48, 90, 32), true);
		languageButton.Pressed += ToggleLanguage;
		panel.AddChild(languageButton);

		_clockLabel = CreateAbsoluteLabel(string.Empty, 18, _textColor, new Rect2(1660, 22, 200, 24));
		_clockLabel.HorizontalAlignment = HorizontalAlignment.Right;
		panel.AddChild(_clockLabel);
		return panel;
	}

	private Control BuildHeroPanel()
	{
		var heroPanel = CreateAbsolutePanel(new Rect2(20, 128, 1230, 430), _cardColor);
		heroPanel.AddChild(CreateAbsoluteLabel(Loc.Text("ui.hero.title"), 16, _accentColor, new Rect2(24, 18, 180, 22)));
		heroPanel.AddChild(CreateAbsoluteWrappedLabel(Loc.Text("ui.hero.desc"), 24, _textColor, new Rect2(24, 48, 860, 44)));

		heroPanel.AddChild(CreateMiniStat(Loc.Text("ui.hero.goal"), Loc.Text("ui.hero.goalValue"), new Rect2(24, 112, 220, 58), _accentColor));
		heroPanel.AddChild(CreateMiniStat(Loc.Text("ui.hero.path"), Loc.Text("ui.hero.pathValue"), new Rect2(256, 112, 250, 58), new Color(0.54f, 0.74f, 0.86f)));
		heroPanel.AddChild(CreateMiniStat(Loc.Text("ui.hero.growth"), Loc.Text("ui.hero.growthValue"), new Rect2(518, 112, 250, 58), new Color(0.67f, 0.83f, 0.72f)));

		_buffList = new HBoxContainer
		{
			Position = new Vector2(790, 116),
			Size = new Vector2(410, 54),
		};
		_buffList.AddThemeConstantOverride("separation", 8);
		heroPanel.AddChild(_buffList);

		var stageFrame = CreateAbsolutePanel(new Rect2(24, 182, 886, 206), _cardColor.Darkened(0.16f));
		heroPanel.AddChild(stageFrame);
		stageFrame.AddChild(CreateAbsoluteLabel(Loc.Text("ui.hero.stage"), 15, _accentColor, new Rect2(18, 12, 120, 20)));

		_stageVisual = TextureHelper.CreateImageOrFallback(
			_database.Skin.BackgroundTexturePath,
			"Background",
			new Vector2(850, 132),
			_cardColor,
			_textColor);
		_stageVisual.Position = new Vector2(18, 42);
		_stageVisual.Size = new Vector2(850, 132);
		stageFrame.AddChild(_stageVisual);

		_guestTimerBar = BuildGuestTimerBar();
		_guestTimerBar.Position = new Vector2(18, 178);
		_guestTimerBar.Size = new Vector2(850, 10);
		stageFrame.AddChild(_guestTimerBar);

		var clerkPanel = CreateAbsolutePanel(new Rect2(930, 182, 276, 206), _cardColor.Lightened(0.04f));
		heroPanel.AddChild(clerkPanel);
		clerkPanel.AddChild(CreateAbsoluteLabel(Loc.Text("ui.hero.clerk"), 15, _accentColor, new Rect2(18, 12, 120, 20)));

		var clerkVisual = TextureHelper.CreateImageOrFallback(
			_database.Skin.ClerkTexturePath,
			"Clerk",
			new Vector2(240, 124),
			_cardColor,
			_textColor);
		clerkVisual.Position = new Vector2(18, 42);
		clerkVisual.Size = new Vector2(240, 124);
		clerkPanel.AddChild(clerkVisual);
		clerkPanel.AddChild(CreateAbsoluteWrappedLabel(Loc.Text("ui.hero.clerkDesc"), 14, _mutedTextColor, new Rect2(18, 172, 236, 24)));
		return heroPanel;
	}

	private Control BuildCustomerPanel()
	{
		_customerCard = CreateAbsolutePanel(new Rect2(20, 578, 500, 430), _cardColor);
		_customerCard.AddChild(CreateAbsoluteLabel(Loc.Current == GameLanguage.Chinese ? "等候客人" : "Guest Queue", 16, _accentColor, new Rect2(18, 16, 160, 22)));

		_customerVisual = TextureHelper.CreateImageOrFallback(
			_database.Skin.DefaultCustomerTexturePath,
			"Guest",
			new Vector2(104, 84),
			_cardColor,
			_textColor);
		_customerVisual.Position = new Vector2(18, 48);
		_customerVisual.Size = new Vector2(104, 84);
		_customerCard.AddChild(_customerVisual);

		_customerName = CreateAbsoluteLabel(Loc.Text("ui.customer.waitName"), 20, _textColor, new Rect2(140, 54, 320, 26));
		_customerCard.AddChild(_customerName);

		_customerLine = CreateAbsoluteWrappedLabel(Loc.Text("ui.customer.waitLine"), 14, _mutedTextColor, new Rect2(140, 86, 330, 60));
		_customerCard.AddChild(_customerLine);

		var queueScroll = new ScrollContainer
		{
			Position = new Vector2(18, 158),
			Size = new Vector2(464, 250),
			HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
			FollowFocus = true,
		};
		_customerCard.AddChild(queueScroll);

		_customerQueueList = new VBoxContainer
		{
			CustomMinimumSize = new Vector2(448, 0),
		};
		_customerQueueList.AddThemeConstantOverride("separation", 8);
		queueScroll.AddChild(_customerQueueList);
		return _customerCard;
	}

	private Control BuildProductPanel()
	{
		_productPanel = CreateAbsolutePanel(new Rect2(540, 578, 710, 430), _cardColor);
		_productPanel.AddChild(CreateAbsoluteLabel(Loc.Text("ui.products.title"), 16, _accentColor, new Rect2(18, 16, 160, 22)));
		_productPanel.AddChild(CreateAbsoluteWrappedLabel(Loc.Text("ui.products.desc"), 15, _mutedTextColor, new Rect2(18, 46, 470, 24)));

		_brewLabel = CreateAbsoluteLabel(string.Empty, 14, _accentColor, new Rect2(438, 46, 250, 20));
		_productPanel.AddChild(_brewLabel);

		_brewBar = BuildGuestTimerBar();
		_brewBar.Position = new Vector2(438, 68);
		_brewBar.Size = new Vector2(236, 8);
		_productPanel.AddChild(_brewBar);

		_productScroll = new ScrollContainer
		{
			Position = new Vector2(18, 90),
			Size = new Vector2(674, 320),
			HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
			VerticalScrollMode = ScrollContainer.ScrollMode.Auto,
			FollowFocus = true,
		};
		_productPanel.AddChild(_productScroll);

		_productList = new VBoxContainer
		{
			CustomMinimumSize = new Vector2(658, 0),
		};
		_productList.AddThemeConstantOverride("separation", 8);
		_productScroll.AddChild(_productList);
		return _productPanel;
	}

	private Control BuildProgressPanel()
	{
		_progressPanel = CreateAbsolutePanel(new Rect2(1270, 128, 630, 176), _cardColor);
		_progressPanel.AddChild(CreateAbsoluteLabel(Loc.Text("ui.progress.title"), 16, _accentColor, new Rect2(18, 16, 140, 22)));

		_shiftLabel = CreateAbsoluteLabel(string.Empty, 20, _textColor, new Rect2(18, 48, 230, 24));
		_progressPanel.AddChild(_shiftLabel);

		_servedLabel = CreateAbsoluteLabel(string.Empty, 16, _mutedTextColor, new Rect2(18, 78, 240, 20));
		_progressPanel.AddChild(_servedLabel);

		_blessingLabel = CreateAbsoluteLabel(string.Empty, 16, _mutedTextColor, new Rect2(18, 102, 240, 20));
		_progressPanel.AddChild(_blessingLabel);

		_specialLabel = CreateAbsoluteLabel(string.Empty, 16, _accentColor, new Rect2(270, 48, 260, 20));
		_progressPanel.AddChild(_specialLabel);

		_comboLabel = CreateAbsoluteLabel(string.Empty, 16, _mutedTextColor, new Rect2(270, 78, 260, 20));
		_progressPanel.AddChild(_comboLabel);

		_eventLabel = CreateAbsoluteLabel(string.Empty, 16, _textColor, new Rect2(270, 102, 300, 20));
		_progressPanel.AddChild(_eventLabel);

		_eventSubLabel = CreateAbsoluteWrappedLabel(string.Empty, 14, _mutedTextColor, new Rect2(18, 132, 490, 24));
		_progressPanel.AddChild(_eventSubLabel);

		var saveButton = CreateAbsoluteButton(Loc.Text("ui.progress.save"), new Rect2(532, 126, 80, 34), true);
		saveButton.Pressed += () => SaveGame(true);
		_progressPanel.AddChild(saveButton);
		return _progressPanel;
	}

	private Control BuildShopPanel()
	{
		_shopPanel = CreateAbsolutePanel(new Rect2(1270, 322, 630, 230), _cardColor);
		_shopPanel.AddChild(CreateAbsoluteLabel(Loc.Text("ui.shop.title"), 16, _accentColor, new Rect2(18, 16, 140, 22)));
		_shopPanel.AddChild(CreateAbsoluteWrappedLabel(Loc.Text("ui.shop.desc"), 15, _mutedTextColor, new Rect2(18, 44, 594, 24)));

		_shopScroll = new ScrollContainer
		{
			Position = new Vector2(18, 78),
			Size = new Vector2(594, 134),
			HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
			FollowFocus = true,
		};
		_shopPanel.AddChild(_shopScroll);

		_shopList = new VBoxContainer
		{
			CustomMinimumSize = new Vector2(578, 0),
		};
		_shopList.AddThemeConstantOverride("separation", 10);
		_shopScroll.AddChild(_shopList);
		return _shopPanel;
	}

	private Control BuildOwnedDecorPanel()
	{
		_ownedDecorPanel = CreateAbsolutePanel(new Rect2(1270, 570, 630, 444), _cardColor.Darkened(0.03f));
		_ownedDecorPanel.AddChild(CreateAbsoluteLabel(Loc.Current == GameLanguage.Chinese ? "资产与祝福" : "Assets & Blessings", 16, _accentColor, new Rect2(18, 16, 180, 22)));
		_ownedDecorPanel.AddChild(CreateAbsoluteWrappedLabel(Loc.Current == GameLanguage.Chinese ? "装饰提供被动收益；祝福决定当前流派效果。" : "Decor gives passive income. Blessings define your current build.", 15, _mutedTextColor, new Rect2(18, 44, 594, 24)));

		_ownedScroll = new ScrollContainer
		{
			Position = new Vector2(18, 78),
			Size = new Vector2(594, 348),
			HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
			FollowFocus = true,
		};
		_ownedDecorPanel.AddChild(_ownedScroll);

		_decorOwnedList = new VBoxContainer
		{
			CustomMinimumSize = new Vector2(578, 0),
		};
		_decorOwnedList.AddThemeConstantOverride("separation", 8);
		_ownedScroll.AddChild(_decorOwnedList);
		return _ownedDecorPanel;
	}

	private Control BuildBottomPanel()
	{
		var panel = CreateAbsolutePanel(new Rect2(20, 1022, 1880, 18), _cardColor.Darkened(0.02f));
		_messageLabel = CreateAbsoluteWrappedLabel(string.Empty, 13, _textColor, new Rect2(16, 0, 1848, 18));
		panel.AddChild(_messageLabel);
		return panel;
	}

	private Control BuildToastLabel()
	{
		_toastLabel = CreateAbsoluteLabel(string.Empty, 18, _textColor, new Rect2(720, 944, 480, 28));
		_toastLabel.HorizontalAlignment = HorizontalAlignment.Center;
		_toastLabel.Modulate = new Color(1f, 1f, 1f, 0f);
		return _toastLabel;
	}

	private Control BuildBlessingModal()
	{
		_blessingModal = CreateAbsolutePanel(new Rect2(520, 240, 880, 420), _cardColor.Darkened(0.08f));
		_blessingModal.Visible = false;
		_blessingModal.ZIndex = 10;
		_blessingModal.AddChild(CreateAbsoluteLabel(Loc.Text("ui.blessing.title"), 20, _accentColor, new Rect2(28, 24, 200, 24)));
		_blessingModal.AddChild(CreateAbsoluteWrappedLabel(Loc.Text("ui.blessing.desc"), 18, _textColor, new Rect2(28, 60, 824, 38)));

		_blessingChoiceList = new VBoxContainer
		{
			Position = new Vector2(28, 124),
			Size = new Vector2(824, 252),
		};
		_blessingChoiceList.AddThemeConstantOverride("separation", 12);
		_blessingModal.AddChild(_blessingChoiceList);

		return _blessingModal;
	}

	private Panel CreateAbsolutePanel(Rect2 rect, Color color)
	{
		var panel = UiFactory.CreateCard(color);
		panel.Position = rect.Position;
		panel.Size = rect.Size;
		return panel;
	}

	private Label CreateAbsoluteLabel(string text, int fontSize, Color color, Rect2 rect)
	{
		var label = UiFactory.CreateLabel(text, fontSize, color);
		label.Position = rect.Position;
		label.Size = rect.Size;
		return label;
	}

	private Label CreateAbsoluteWrappedLabel(string text, int fontSize, Color color, Rect2 rect)
	{
		var label = UiFactory.CreateWrappedLabel(text, fontSize, color);
		label.Position = rect.Position;
		label.Size = rect.Size;
		return label;
	}

	private Button CreateAbsoluteButton(string text, Rect2 rect, bool small = false)
	{
		var button = UiFactory.CreateButton(text, _accentColor);
		button.Position = rect.Position;
		button.Size = rect.Size;
		if (small)
		{
			button.AddThemeFontSizeOverride("font_size", 15);
		}
		return button;
	}

	private Control CreateMiniStat(string title, string value, Rect2 rect, Color accent)
	{
		var panel = CreateAbsolutePanel(rect, _cardColor.Darkened(0.03f));
		panel.AddChild(CreateAbsoluteLabel(title, 13, _mutedTextColor, new Rect2(14, 10, rect.Size.X - 28, 18)));
		panel.AddChild(CreateAbsoluteLabel(value, 18, accent, new Rect2(14, 28, rect.Size.X - 28, 22)));
		return panel;
	}

	private ProgressBar BuildGuestTimerBar()
	{
		var bar = new ProgressBar
		{
			MinValue = 0,
			MaxValue = 100,
			Value = 0,
			ShowPercentage = false,
			MouseFilter = MouseFilterEnum.Ignore,
		};

		bar.AddThemeStyleboxOverride("fill", new StyleBoxFlat
		{
			BgColor = _accentColor,
			CornerRadiusTopLeft = 6,
			CornerRadiusTopRight = 6,
			CornerRadiusBottomLeft = 6,
			CornerRadiusBottomRight = 6,
		});

		bar.AddThemeStyleboxOverride("background", new StyleBoxFlat
		{
			BgColor = _cardColor.Darkened(0.12f),
			CornerRadiusTopLeft = 6,
			CornerRadiusTopRight = 6,
			CornerRadiusBottomLeft = 6,
			CornerRadiusBottomRight = 6,
		});

		return bar;
	}

	private Control CreateProductRow(ProductConfig product)
	{
		var row = UiFactory.CreateCard(_cardColor.Lightened(0.03f));
		row.CustomMinimumSize = new Vector2(718, 58);
		BindInteractiveCard(row);

		var icon = TextureHelper.CreateImageOrFallback(product.IconPath, ProductName(product), new Vector2(34, 34), _cardColor, _textColor);
		icon.Position = new Vector2(10, 7);
		icon.Size = new Vector2(34, 34);
		row.AddChild(icon);

		var expectedProfit = Math.Max(0, product.BasePrice - product.Cost);
		row.AddChild(CreateAbsoluteLabel(ProductName(product), 16, _textColor, new Rect2(54, 6, 250, 20)));
		row.AddChild(CreateAbsoluteLabel(Loc.Current == GameLanguage.Chinese ? $"成本 {product.Cost}" : $"Cost {product.Cost}", 13, _warningColor, new Rect2(54, 28, 82, 18)));
		row.AddChild(CreateAbsoluteLabel(Loc.Current == GameLanguage.Chinese ? $"售价 {product.BasePrice}" : $"Price {product.BasePrice}", 13, _mutedTextColor, new Rect2(140, 28, 90, 18)));
		row.AddChild(CreateAbsoluteLabel(Loc.Current == GameLanguage.Chinese ? $"基础利润 +{expectedProfit}" : $"Base +{expectedProfit}", 13, _positiveColor, new Rect2(232, 28, 120, 18)));

		var tagText = string.Join(" / ", product.Tags.Take(2).Select(GameState.TagToDisplay));
		var tagColor = _state.CurrentRequestTag.Length > 0 && product.Tags.Contains(_state.CurrentRequestTag)
			? _positiveColor
			: _infoColor;
		row.AddChild(CreateAbsoluteLabel(tagText, 13, tagColor, new Rect2(368, 18, 170, 18)));

		var button = CreateAbsoluteButton(Loc.Current == GameLanguage.Chinese ? "制作" : "Brew", new Rect2(618, 7, 90, 34), true);
		button.Disabled = _state.IsBrewing || !_state.HasSelectedOrder || _state.Coins < product.Cost;
		button.Pressed += () =>
		{
			var result = _state.StartBrew(product);
			RefreshAll(result.Message);
			PlayButtonFeedback(button);
			PlayPanelFlash(_productPanel);
			PlayStagePulse();
			ShowToast(result.Message);
		};
		row.AddChild(button);
		return row;
	}

	private Control CreateDecorRow(DecorConfig decor)
	{
		var row = UiFactory.CreateCard(_cardColor.Lightened(0.03f));
		row.CustomMinimumSize = new Vector2(578, 68);
		BindInteractiveCard(row);

		var icon = TextureHelper.CreateImageOrFallback(decor.TexturePath, DecorName(decor), new Vector2(44, 44), _cardColor, _textColor);
		icon.Position = new Vector2(12, 12);
		icon.Size = new Vector2(44, 44);
		row.AddChild(icon);

		row.AddChild(CreateAbsoluteLabel(DecorName(decor), 17, _textColor, new Rect2(70, 10, 250, 22)));
		row.AddChild(CreateAbsoluteLabel(Loc.Format("ui.decor.cost", decor.Cost), 14, _accentColor, new Rect2(70, 31, 140, 18)));
		row.AddChild(CreateAbsoluteLabel(Loc.Format("ui.decor.passive", decor.PassiveCoinsPerSecond), 13, _mutedTextColor, new Rect2(70, 48, 220, 16)));

		var button = CreateAbsoluteButton(Loc.Text("ui.button.buy"), new Rect2(472, 17, 86, 34), true);
		button.Pressed += () =>
		{
			var result = _state.BuyDecor(decor);
			RefreshAll(result.Message);
			PlayButtonFeedback(button);
			PlayPanelFlash(_shopPanel);
			PlayPanelFlash(_ownedDecorPanel);
			if (result.Success)
			{
				PlayResourcePulse();
				PlayStagePulse();
				ShowToast(Loc.Format("ui.toast.bought", DecorName(decor)));
				RebuildShopList();
			}
		};
		row.AddChild(button);
		return row;
	}

	private Control CreateBlessingRow(BlessingConfig blessing)
	{
		var row = UiFactory.CreateCard(_cardColor.Lightened(0.02f));
		row.CustomMinimumSize = new Vector2(824, 72);
		BindInteractiveCard(row);

		row.AddChild(CreateAbsoluteLabel(BlessingName(blessing), 18, _accentColor, new Rect2(18, 10, 260, 22)));
		row.AddChild(CreateAbsoluteLabel(blessing.Archetype, 13, new Color(0.70f, 0.86f, 0.80f), new Rect2(18, 34, 180, 18)));
		row.AddChild(CreateAbsoluteWrappedLabel(blessing.Description, 15, _textColor, new Rect2(190, 22, 430, 24)));

		var button = CreateAbsoluteButton(Loc.Text("ui.button.pick"), new Rect2(704, 18, 92, 34), true);
		button.Pressed += () =>
		{
			var message = _state.ApplyBlessing(blessing);
			CloseBlessingModal();
			RebuildAllLists();
			RefreshAll(message);
			PlayPanelFlash(_progressPanel);
			PlayStagePulse();
			ShowToast(BlessingName(blessing));
			SaveGame(false);
		};
		row.AddChild(button);
		return row;
	}

	private void RefreshAll(string message)
	{
		_lastMessage = message;
		_messageLabel.Text = message;
		RefreshCustomerPanel();
		RefreshBuffList();
		RefreshOwnedDecors();
		RefreshProgressLabels();
		RebuildProductList();
		RebuildShopList();
		EnsurePrimaryFocus();
	}

	private void RebuildAllLists()
	{
		RebuildProductList();
		RebuildShopList();
		RefreshOwnedDecors();
		RefreshProgressLabels();
		EnsurePrimaryFocus();
	}

	private void RebuildProductList()
	{
		ClearChildren(_productList);
		foreach (var product in _database.GetProductsForRun(_state.ShiftLevel, _state.Reputation))
		{
			_productList.AddChild(CreateProductRow(product));
		}
	}

	private void RebuildShopList()
	{
		ClearChildren(_shopList);
		foreach (var decor in _database.GetDecorsForRun(_state.ShiftLevel))
		{
			if (_state.OwnedDecorIds.Contains(decor.Id))
			{
				continue;
			}

			_shopList.AddChild(CreateDecorRow(decor));
		}
	}

	private void RefreshCustomerPanel()
	{
		ClearChildren(_customerQueueList);
		_patienceBars.Clear();

		if (_state.CustomerOrders.Count == 0)
		{
			_customerName.Text = Loc.Text("ui.customer.waitName");
			_customerLine.Text = Loc.Text("ui.customer.waitLine");
			ReplaceCustomerVisual(_database.Skin.DefaultCustomerTexturePath, "Guest");
			return;
		}

		var selected = _state.SelectedOrder;
		var selectedCustomer = selected == null ? null : _database.GetCustomer(selected.CustomerId);
		if (selectedCustomer != null)
		{
			_customerName.Text = CustomerName(selectedCustomer);
			var requestText = string.IsNullOrWhiteSpace(selected!.RequestTag)
			? string.Empty
			: Loc.Current == GameLanguage.Chinese
				? $"\n需求：{GameState.TagToDisplay(selected.RequestTag)}"
				: $"\nRequest: {GameState.TagToDisplay(selected.RequestTag)}";
			_customerLine.Text = CustomerLine(selectedCustomer) + requestText;
			ReplaceCustomerVisual(selectedCustomer.SpritePath, CustomerName(selectedCustomer));
		}

		for (var i = 0; i < _state.CustomerOrders.Count; i++)
		{
			_customerQueueList.AddChild(CreateCustomerOrderRow(i, _state.CustomerOrders[i]));
		}

		PlayCustomerArrivalAnimation();
	}

	private Control CreateCustomerOrderRow(int index, CustomerOrder order)
	{
		var customer = _database.GetCustomer(order.CustomerId);
		var row = UiFactory.CreateCard(index == _state.SelectedOrderIndex ? _cardColor.Lightened(0.10f) : _cardColor.Lightened(0.03f));
		row.CustomMinimumSize = new Vector2(448, 78);
		BindInteractiveCard(row);

		var name = customer == null ? order.CustomerId : CustomerName(customer);
		row.AddChild(CreateAbsoluteLabel($"{index + 1}. {name}", 15, index == _state.SelectedOrderIndex ? _accentColor : _textColor, new Rect2(12, 8, 250, 20)));
		row.AddChild(CreateAbsoluteLabel(Loc.Current == GameLanguage.Chinese ? $"需求 {GameState.TagToDisplay(order.RequestTag)}" : $"Need {GameState.TagToDisplay(order.RequestTag)}", 13, _infoColor, new Rect2(12, 32, 160, 18)));
		row.AddChild(CreateAbsoluteLabel(Loc.Current == GameLanguage.Chinese ? "耐心" : "Patience", 12, _mutedTextColor, new Rect2(176, 32, 54, 18)));

		var patience = new ProgressBar
		{
			Position = new Vector2(226, 36),
			Size = new Vector2(104, 8),
			MinValue = 0,
			MaxValue = 100,
			Value = order.PatiencePercent * 100f,
			ShowPercentage = false,
		};
		row.AddChild(patience);
		_patienceBars.Add(patience);

		var button = CreateAbsoluteButton(Loc.Current == GameLanguage.Chinese ? "选择" : "Pick", new Rect2(354, 20, 76, 32), true);
		button.Disabled = _state.IsBrewing;
		button.Pressed += () =>
		{
			_state.SelectOrder(index);
			RefreshAll(Loc.Current == GameLanguage.Chinese ? $"已选择第 {index + 1} 位客人。" : $"Selected guest #{index + 1}.");
			PlayButtonFeedback(button);
		};
		row.AddChild(button);
		return row;
	}

	private void RefreshPatienceBars()
	{
		for (var i = 0; i < _patienceBars.Count && i < _state.CustomerOrders.Count; i++)
		{
			_patienceBars[i].Value = _state.CustomerOrders[i].PatiencePercent * 100f;
			_patienceBars[i].Modulate = _state.CustomerOrders[i].PatiencePercent < 0.35f
				? _warningColor
				: Colors.White;
		}
	}

	private void RefreshBuffList()
	{
		if (_buffList == null)
		{
			return;
		}

		ClearChildren(_buffList);
		if (_state.ActiveBlessings.Count == 0)
		{
			_buffList.AddChild(CreateAbsoluteLabel(Loc.Current == GameLanguage.Chinese ? "暂无临时祝福" : "No temporary buffs", 13, _mutedTextColor, new Rect2(0, 0, 180, 24)));
			return;
		}

		foreach (var active in _state.ActiveBlessings.Take(5))
		{
			var blessing = _database.GetBlessing(active.BlessingId);
			if (blessing == null)
			{
				continue;
			}

			var button = UiFactory.CreateButton($"{BlessingName(blessing)} {active.RemainingServes}", _accentColor);
			button.CustomMinimumSize = new Vector2(92, 40);
			button.TooltipText = $"{BlessingName(blessing)}\n{blessing.Description}\n{(Loc.Current == GameLanguage.Chinese ? "剩余出杯" : "Serves left")}: {active.RemainingServes}";
			_buffList.AddChild(button);
		}
	}

	private void RefreshBrewStatus()
	{
		if (_brewLabel == null || _brewBar == null)
		{
			return;
		}

		if (!_state.IsBrewing)
		{
			_brewLabel.Text = Loc.Current == GameLanguage.Chinese ? "吧台空闲" : "Counter idle";
			_brewBar.Value = 0;
			return;
		}

		var product = _database.GetProduct(_state.BrewingProductId);
		var productName = product == null ? _state.BrewingProductId : ProductName(product);
		_brewLabel.Text = Loc.Current == GameLanguage.Chinese
			? $"制作中：{productName}"
			: $"Brewing: {productName}";
		_brewBar.Value = _state.BrewingPercent * 100f;
	}

	private void HandleCompletedSale(SaleResult result)
	{
		RefreshAll(result.Message);
		PlayPanelFlash(_customerCard);
		PlayPanelFlash(_productPanel);
		PlayResourcePulse();
		PlayStagePulse();
		ShowToast(Loc.Format("ui.toast.income", result.Coins, result.Aroma));

		if (result.ShiftAdvanced)
		{
			ShowToast(Loc.Format("ui.toast.shift", _state.ShiftLevel));
			RebuildAllLists();
		}

		if (!string.IsNullOrWhiteSpace(result.EventMessage))
		{
			ShowToast(result.EventMessage);
		}

		if (result.BlessingReady)
		{
			OpenBlessingModal();
		}
	}

	private void RefreshOwnedDecors()
	{
		ClearChildren(_decorOwnedList);
		if (_state.OwnedDecorIds.Count == 0)
		{
			_decorOwnedList.AddChild(UiFactory.CreateWrappedLabel(Loc.Text("ui.owned.empty"), 15, _mutedTextColor));
		}
		else
		{
			_decorOwnedList.AddChild(CreateSectionLabel(Loc.Current == GameLanguage.Chinese ? "装饰收益" : "Decor Income"));
			foreach (var decorId in _state.OwnedDecorIds)
			{
				var decor = _database.GetDecor(decorId);
				if (decor == null)
				{
					continue;
				}

				var row = UiFactory.CreateCard(_cardColor.Lightened(0.03f));
				row.CustomMinimumSize = new Vector2(578, 48);
				row.AddChild(CreateAbsoluteLabel(DecorName(decor), 16, _textColor, new Rect2(12, 13, 340, 22)));

				var gain = CreateAbsoluteLabel(Loc.Format("ui.decor.passive", decor.PassiveCoinsPerSecond), 15, _positiveColor, new Rect2(380, 13, 178, 22));
				gain.HorizontalAlignment = HorizontalAlignment.Right;
				row.AddChild(gain);
				_decorOwnedList.AddChild(row);
			}
		}

		_decorOwnedList.AddChild(CreateSectionLabel(Loc.Current == GameLanguage.Chinese ? "已获得祝福" : "Active Blessings"));
		if (_state.ActiveBlessings.Count == 0)
		{
			_decorOwnedList.AddChild(UiFactory.CreateWrappedLabel(Loc.Current == GameLanguage.Chinese ? "暂无临时祝福。服务客人后会经常出现三选一。" : "No temporary buffs. Serve guests to trigger choices often.", 15, _mutedTextColor));
			return;
		}

		foreach (var active in _state.ActiveBlessings)
		{
			var blessing = _database.GetBlessing(active.BlessingId);
			if (blessing == null)
			{
				continue;
			}

			var row = UiFactory.CreateCard(_cardColor.Lightened(0.035f));
			row.CustomMinimumSize = new Vector2(578, 68);
			row.AddChild(CreateAbsoluteLabel(BlessingName(blessing), 16, _accentColor, new Rect2(12, 9, 240, 20)));
			row.AddChild(CreateAbsoluteLabel($"{blessing.Archetype} / {active.RemainingServes}", 13, _infoColor, new Rect2(390, 9, 168, 18)));
			row.AddChild(CreateAbsoluteWrappedLabel(blessing.Description, 13, _mutedTextColor, new Rect2(12, 34, 540, 24)));
			_decorOwnedList.AddChild(row);
		}
	}

	private Control CreateSectionLabel(string text)
	{
		var label = UiFactory.CreateLabel(text, 14, _infoColor);
		label.CustomMinimumSize = new Vector2(560, 24);
		return label;
	}

	private void RefreshProgressLabels()
	{
		var specialProduct = _database.GetProduct(_state.TodaySpecialProductId);
		_shiftLabel.Text = Loc.Format("ui.progress.shift", _state.ShiftLevel);
		_servedLabel.Text = Loc.Format("ui.progress.served", _state.ServedCount);
		_blessingLabel.Text = Loc.Format("ui.progress.blessings", _state.ActiveBlessings.Count);
		_specialLabel.Text = specialProduct == null
			? Loc.Text("ui.progress.special.none")
			: Loc.Format("ui.progress.special", ProductName(specialProduct));
		_comboLabel.Text = Loc.Format("ui.progress.combo", _state.ComboStreak, _state.BestCombo);

		if (_state.HasActiveEvent)
		{
			_eventLabel.Text = $"{_state.CurrentEventTitle}  {_state.EventProgress}/{_state.EventTargetCount}";
			_eventSubLabel.Text = Loc.Format("ui.progress.event.reward", _state.CurrentEventDescription, _state.EventRewardCoins);
		}
		else
		{
			_eventLabel.Text = Loc.Text("ui.progress.event.wait");
			_eventSubLabel.Text = _state.HasBlessingChoice
				? Loc.Text("ui.progress.event.pending")
				: Loc.Format("ui.progress.event.next", _state.NextEventAt - _state.ServedCount);
		}
	}

	private void OpenBlessingModal()
	{
		ClearChildren(_blessingChoiceList);
		var choices = _state.RollBlessingChoices(_database);
		foreach (var blessing in choices)
		{
			_blessingChoiceList.AddChild(CreateBlessingRow(blessing));
		}

		var open = choices.Count > 0;
		_blessingModal.Visible = open;
		_modalBlocker.Visible = open;
		SetMainUiInteractable(!open);
		if (choices.Count > 0)
		{
			ShowToast(Loc.Text("ui.toast.blessing"));
			CallDeferred(nameof(FocusPrimaryButton));
		}
	}

	private void CloseBlessingModal()
	{
		_blessingModal.Visible = false;
		_modalBlocker.Visible = false;
		SetMainUiInteractable(true);
		CallDeferred(nameof(FocusPrimaryButton));
	}

	private void ReplaceCustomerVisual(string path, string fallbackText)
	{
		var newVisual = TextureHelper.CreateImageOrFallback(path, fallbackText, new Vector2(150, 126), _cardColor, _textColor);
		newVisual.Position = new Vector2(18, 50);
		newVisual.Size = new Vector2(150, 126);
		newVisual.MouseFilter = MouseFilterEnum.Ignore;
		_customerVisual.ReplaceBy(newVisual);
		_customerVisual = newVisual;
	}

	private void ToggleLanguage()
	{
		Loc.Toggle();
		BuildUi();
		OnViewportSizeChanged();
		RefreshAll(string.IsNullOrWhiteSpace(_lastMessage) ? Loc.Text("ui.message.start") : _lastMessage);
		SaveGame(false);
	}

	private void SaveGame(bool showToast)
	{
		var data = _state.ToSaveData();
		data.LanguageCode = Loc.Code;
		SaveSystem.Save(data);
		if (showToast)
		{
			ShowToast(Loc.Text("ui.toast.autosave"));
		}
	}

	private void EnsurePrimaryFocus()
	{
		CallDeferred(nameof(FocusPrimaryButton));
	}

	private void FocusPrimaryButton()
	{
		var firstButton = FindFirstButton(_blessingModal.Visible ? _blessingChoiceList : _productList);
		firstButton?.GrabFocus();
	}

	private void SetMainUiInteractable(bool enabled)
	{
		SetButtonsDisabled(_canvasRoot, !enabled, _blessingModal);

		if (enabled)
		{
			return;
		}

		var focused = GetViewport().GuiGetFocusOwner();
		if (focused != null && !_blessingModal.IsAncestorOf(focused))
		{
			focused.ReleaseFocus();
		}
	}

	private static void SetButtonsDisabled(Node node, bool disabled, Node exceptRoot)
	{
		if (node != exceptRoot && node is Button button)
		{
			button.Disabled = disabled;
		}

		foreach (var child in node.GetChildren())
		{
			if (child == exceptRoot)
			{
				SetButtonsDisabled(child, false, exceptRoot);
				continue;
			}

			SetButtonsDisabled(child, disabled, exceptRoot);
		}
	}

	private void EnsureInputActions()
	{
		EnsureActionKey("ui_accept", Key.Enter);
		EnsureActionKey("ui_accept", Key.Space);
		EnsureJoyButton("ui_accept", JoyButton.A);
		EnsureJoyButton("ui_cancel", JoyButton.B);
	}

	private static void EnsureActionKey(string action, Key key)
	{
		if (!InputMap.HasAction(action))
		{
			InputMap.AddAction(action);
		}

		foreach (var existing in InputMap.ActionGetEvents(action))
		{
			if (existing is InputEventKey keyEvent && keyEvent.PhysicalKeycode == key)
			{
				return;
			}
		}

		InputMap.ActionAddEvent(action, new InputEventKey { PhysicalKeycode = key });
	}

	private static void EnsureJoyButton(string action, JoyButton joyButton)
	{
		if (!InputMap.HasAction(action))
		{
			InputMap.AddAction(action);
		}

		foreach (var existing in InputMap.ActionGetEvents(action))
		{
			if (existing is InputEventJoypadButton joyEvent && joyEvent.ButtonIndex == joyButton)
			{
				return;
			}
		}

		InputMap.ActionAddEvent(action, new InputEventJoypadButton { ButtonIndex = joyButton });
	}

	private static Button? FindFirstButton(Node root)
	{
		foreach (var child in root.GetChildren())
		{
			if (child is Button button)
			{
				return button;
			}

			var nested = FindFirstButton(child);
			if (nested != null)
			{
				return nested;
			}
		}

		return null;
	}

	private void OnViewportSizeChanged()
	{
		if (_canvasRoot == null)
		{
			return;
		}

		var viewportSize = GetViewportRect().Size;
		const float targetAspect = DesignWidth / DesignHeight;
		var viewportAspect = viewportSize.X / viewportSize.Y;

		Vector2 canvasSize;
		if (viewportAspect > targetAspect)
		{
			canvasSize = new Vector2(viewportSize.Y * targetAspect, viewportSize.Y);
		}
		else
		{
			canvasSize = new Vector2(viewportSize.X, viewportSize.X / targetAspect);
		}

		_canvasRoot.Size = new Vector2(DesignWidth, DesignHeight);
		_canvasRoot.Scale = new Vector2(canvasSize.X / DesignWidth, canvasSize.Y / DesignHeight);
		_canvasRoot.Position = (viewportSize - canvasSize) * 0.5f;
	}

	private ColorRect CreateAmbientBlob(Color color, Vector2 position, Vector2 size)
	{
		return new ColorRect
		{
			Color = color,
			Position = position,
			CustomMinimumSize = size,
			MouseFilter = MouseFilterEnum.Ignore,
		};
	}

	private void AnimateAmbient()
	{
		var time = (float)Time.GetTicksMsec() / 1000f;
		_ambientBlobA.Position = new Vector2(120 + Mathf.Sin(time * 0.4f) * 24f, 160 + Mathf.Cos(time * 0.3f) * 18f);
		_ambientBlobB.Position = new Vector2(1500 + Mathf.Cos(time * 0.32f) * 18f, 720 + Mathf.Sin(time * 0.26f) * 20f);

		if (_stageVisual != null)
		{
			_stageVisual.Modulate = new Color(1f, 1f, 1f, 0.97f + Mathf.Sin(time * 1.15f) * 0.03f);
		}
	}

	private void PlayCustomerArrivalAnimation()
	{
		_customerCard.Scale = new Vector2(0.985f, 0.985f);
		_customerVisual.Modulate = new Color(1f, 1f, 1f, 0.4f);

		var tween = CreateTween();
		tween.SetParallel(true);
		tween.TweenProperty(_customerCard, "scale", Vector2.One, 0.22f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
		tween.TweenProperty(_customerVisual, "modulate", Colors.White, 0.28f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
	}

	private void PlayButtonFeedback(Control button)
	{
		button.Scale = new Vector2(0.96f, 0.96f);
		var tween = CreateTween();
		tween.TweenProperty(button, "scale", Vector2.One, 0.16f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
	}

	private void PlayResourcePulse()
	{
		_resourceLabel.Scale = new Vector2(1.03f, 1.03f);
		_resourceLabel.Modulate = _accentColor.Lightened(0.18f);
		var tween = CreateTween();
		tween.SetParallel(true);
		tween.TweenProperty(_resourceLabel, "scale", Vector2.One, 0.18f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
		tween.TweenProperty(_resourceLabel, "modulate", _accentColor, 0.22f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
	}

	private void PlayPanelFlash(Control panel)
	{
		panel.Modulate = new Color(1f, 1f, 1f, 0.82f);
		var tween = CreateTween();
		tween.TweenProperty(panel, "modulate", Colors.White, 0.22f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
	}

	private void PlayStagePulse()
	{
		if (_stageVisual == null)
		{
			return;
		}

		_stageVisual.Scale = new Vector2(0.992f, 0.992f);
		var tween = CreateTween();
		tween.SetParallel(true);
		tween.TweenProperty(_stageVisual, "scale", Vector2.One, 0.22f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
		tween.TweenProperty(_stageVisual, "modulate", new Color(1.08f, 1.04f, 0.98f, 1f), 0.10f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
		tween.Chain().TweenProperty(_stageVisual, "modulate", Colors.White, 0.18f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
	}

	private void BindInteractiveCard(Control card)
	{
		card.MouseEntered += () => AnimateCardState(card, true);
		card.MouseExited += () => AnimateCardState(card, false);
		card.FocusEntered += () => AnimateCardState(card, true);
		card.FocusExited += () => AnimateCardState(card, false);
	}

	private void AnimateCardState(Control card, bool active)
	{
		var tween = CreateTween();
		tween.SetParallel(true);
		tween.TweenProperty(card, "scale", active ? new Vector2(1.01f, 1.01f) : Vector2.One, 0.14f)
			.SetTrans(Tween.TransitionType.Cubic)
			.SetEase(Tween.EaseType.Out);
		tween.TweenProperty(card, "modulate", active ? new Color(1.04f, 1.04f, 1.04f, 1f) : Colors.White, 0.16f)
			.SetTrans(Tween.TransitionType.Cubic)
			.SetEase(Tween.EaseType.Out);
	}

	private void ShowToast(string text)
	{
		_toastLabel.Text = text;
		_toastLabel.Position = new Vector2(720, 944);
		_toastLabel.Modulate = new Color(1f, 1f, 1f, 0f);

		var tween = CreateTween();
		tween.SetParallel(true);
		tween.TweenProperty(_toastLabel, "modulate", new Color(1f, 1f, 1f, 1f), 0.18f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
		tween.TweenProperty(_toastLabel, "position", new Vector2(720, 930), 0.22f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
		tween.Chain().TweenInterval(0.55f);
		tween.Chain().TweenProperty(_toastLabel, "modulate", new Color(1f, 1f, 1f, 0f), 0.22f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.In);
	}

	private string ProductName(ProductConfig product) => Loc.Content("product", product.Id, "name", product.DisplayName);
	private string DecorName(DecorConfig decor) => Loc.Content("decor", decor.Id, "name", decor.DisplayName);
	private string BlessingName(BlessingConfig blessing) => Loc.Content("blessing", blessing.Id, "name", blessing.DisplayName);
	private string CustomerName(CustomerConfig customer) => Loc.Content("customer", customer.Id, "name", customer.DisplayName);
	private string CustomerLine(CustomerConfig customer) => Loc.Content("customer", customer.Id, "line", customer.FirstLine);

	private static void ClearChildren(Node node)
	{
		foreach (var child in node.GetChildren())
		{
			child.QueueFree();
		}
	}

	private static string FormatClock(float value)
	{
		var hour = (int)MathF.Floor(value);
		var minute = (int)MathF.Floor((value - hour) * 60f);
		return $"{hour:00}:{minute:00}";
	}

	private static Color ParseColor(string value, Color fallback)
	{
		return Color.HtmlIsValid(value) ? Color.FromString(value, fallback) : fallback;
	}
}
