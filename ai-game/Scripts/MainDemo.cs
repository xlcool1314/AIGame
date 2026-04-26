using System;
using System.Collections.Generic;
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
	private float _clockTime = 18.8f;
	private float _autosaveTimer;
	private string _lastMessage = string.Empty;
	private int _heldCraftedIndex = -1;
	private bool _isDraggingCrafted;
	private int _hoveredOrderIndex = -1;
	private string _hoveredSlot = string.Empty;
	private int _lastKnownOrderCount = -1;
	private int _lastCombatPulse = -1;

	private Control _canvasRoot = null!;
	private ColorRect _ambientBlobA = null!;
	private ColorRect _ambientBlobB = null!;

	private Label _resourceLabel = null!;
	private Label _clockLabel = null!;
	private Label _messageLabel = null!;
	private Label _shiftLabel = null!;
	private Label _servedLabel = null!;
	private Label _specialLabel = null!;
	private Label _comboLabel = null!;
	private Label _heatLabel = null!;
	private Label _eventLabel = null!;
	private Label _eventSubLabel = null!;
	private Label _forgeStatusLabel = null!;
	private Label _heldItemLabel = null!;
	private Label _toastLabel = null!;
	private Label _dragGhost = null!;
	private ProgressBar _brewBar = null!;
	private ProgressBar _heatBar = null!;

	private HBoxContainer _buffList = null!;
	private VBoxContainer _customerQueueList = null!;
	private VBoxContainer _productList = null!;
	private VBoxContainer _craftedList = null!;
	private VBoxContainer _shopList = null!;
	private VBoxContainer _ownedList = null!;
	private VBoxContainer _blessingChoiceList = null!;

	private ScrollContainer _customerScroll = null!;
	private ScrollContainer _productScroll = null!;
	private ScrollContainer _craftedScroll = null!;
	private ScrollContainer _shopScroll = null!;
	private ScrollContainer _ownedScroll = null!;

	private Panel _heroPanel = null!;
	private Panel _customerPanel = null!;
	private Panel _forgePanel = null!;
	private Panel _craftedPanel = null!;
	private Panel _progressPanel = null!;
	private Panel _shopPanel = null!;
	private Panel _ownedPanel = null!;
	private Panel _blessingModal = null!;
	private ColorRect _modalBlocker = null!;

	private Control _stageVisual = null!;
	private readonly List<ProgressBar> _patienceBars = new();
	private readonly List<Label> _patienceTexts = new();
	private readonly List<ProgressBar> _heroAttackBars = new();
	private readonly List<Label> _heroBuffLabels = new();
	private readonly List<ProgressBar> _enemyHpBars = new();
	private readonly List<ProgressBar> _enemyAttackBars = new();
	private readonly List<Label> _enemyHpTexts = new();
	private readonly List<Control> _combatRows = new();
	private readonly List<Control> _enemyCards = new();
	private readonly List<Control> _heroCards = new();

	public override void _UnhandledInput(InputEvent @event)
	{
		if (!_isDraggingCrafted)
		{
			return;
		}

		if (@event is not InputEventMouseButton mouseButton || mouseButton.Pressed)
		{
			return;
		}

		if (mouseButton.ButtonIndex == MouseButton.Right)
		{
			CancelCraftedDrag();
			GetViewport().SetInputAsHandled();
			return;
		}

		if (mouseButton.ButtonIndex != MouseButton.Left)
		{
			return;
		}

		if (_hoveredOrderIndex >= 0)
		{
			TryEquipHeldCraftedTo(_hoveredOrderIndex, _hoveredSlot);
		}
		else
		{
			CancelCraftedDrag();
		}

		GetViewport().SetInputAsHandled();
	}

	private Color _bgColor = new(0.09f, 0.10f, 0.14f);
	private Color _cardColor = new(0.16f, 0.20f, 0.27f);
	private Color _accentColor = new(0.82f, 0.60f, 0.29f);
	private readonly Color _textColor = new(0.94f, 0.93f, 0.90f);
	private readonly Color _mutedTextColor = new(0.70f, 0.74f, 0.78f);
	private readonly Color _positiveColor = new(0.56f, 0.84f, 0.62f);
	private readonly Color _warningColor = new(0.92f, 0.44f, 0.36f);
	private readonly Color _infoColor = new(0.52f, 0.72f, 0.88f);

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
		if (_lastCombatPulse != _state.CombatPulseId)
		{
			_lastCombatPulse = _state.CombatPulseId;
			HandleCombatPulse();
		}
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
		var craftResult = _state.Tick((float)delta, _database);
		_customerTimer += (float)delta;
		_autosaveTimer += (float)delta;
		_clockTime += (float)delta * 0.025f;
		if (_clockTime >= 24f)
		{
			_clockTime -= 24f;
		}

		if (craftResult.HasValue)
		{
			HandleCraftCompleted(craftResult.Value);
		}

		if (_lastCombatPulse != _state.CombatPulseId)
		{
			_lastCombatPulse = _state.CombatPulseId;
			HandleCombatPulse();
		}

		if (_lastKnownOrderCount != _state.Heroes.Count)
		{
			RefreshCustomerQueue();
		}

		if (_state.Heroes.Count < 3 && _customerTimer >= 1.2f)
		{
			_customerTimer = 0f;
			_state.EnsureCustomer(_database);
			RefreshCustomerQueue();
		}

		_state.EnsureNightEvent(_database);

		if (_autosaveTimer >= AutosaveInterval)
		{
			_autosaveTimer = 0f;
			SaveGame(true);
		}

		_resourceLabel.Text = Loc.Format("ui.header.resources", _state.Coins, _state.Aroma, _state.Reputation, MathF.Round(_state.DecorCombatRewardBonus(_database) * 100f));
		_clockLabel.Text = Loc.Format("ui.header.clock", FormatClock(_clockTime));
		_forgeStatusLabel.Text = _state.IsBrewing
			? Loc.Format("ui.forge.progress", ProductName(_database.GetProduct(_state.BrewingProductId) ?? new ProductConfig { DisplayName = _state.BrewingProductId }))
			: Loc.Text("ui.forge.idle");
		_brewBar.Value = _state.BrewingPercent * 100f;
		_heldItemLabel.Text = _heldCraftedIndex >= 0 && _heldCraftedIndex < _state.CraftedItems.Count
			? Loc.Format("ui.rack.holding", CraftedName(_state.CraftedItems[_heldCraftedIndex]))
			: Loc.Text("ui.rack.emptyHand");

		RefreshPatienceBars();
		RefreshProgressLabels();
		RefreshDragGhost();
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

		_canvasRoot = new Control();
		AddChild(_canvasRoot);
		_buffList = new HBoxContainer();

		_ambientBlobA = CreateAmbientBlob(new Color(_accentColor, 0.08f), new Vector2(90, 110), new Vector2(440, 440));
		_ambientBlobB = CreateAmbientBlob(new Color(0.35f, 0.56f, 0.88f, 0.08f), new Vector2(1450, 620), new Vector2(420, 420));
		_canvasRoot.AddChild(_ambientBlobA);
		_canvasRoot.AddChild(_ambientBlobB);

		_canvasRoot.AddChild(BuildHeaderPanel());
		_canvasRoot.AddChild(BuildProgressPanel());
		_canvasRoot.AddChild(BuildCustomerPanel());
		_canvasRoot.AddChild(BuildForgePanel());
		_canvasRoot.AddChild(BuildCraftedPanel());
		_canvasRoot.AddChild(BuildShopPanel());
		_canvasRoot.AddChild(BuildOwnedPanel());
		_canvasRoot.AddChild(BuildToastLabel());
		_canvasRoot.AddChild(BuildDragGhost());
	}

	private Control BuildHeaderPanel()
	{
		var panel = CreateAbsolutePanel(new Rect2(20, 20, 1880, 88), _cardColor);
		panel.AddChild(CreateAbsoluteLabel(Loc.Text("ui.header.state"), 16, _accentColor, new Rect2(24, 14, 240, 22)));
		panel.AddChild(CreateAbsoluteLabel(Loc.Text("content.title", _database.Skin.Title), 29, _textColor, new Rect2(24, 40, 560, 34)));

		_resourceLabel = CreateAbsoluteLabel(string.Empty, 18, _accentColor, new Rect2(670, 24, 690, 28));
		panel.AddChild(_resourceLabel);
		panel.AddChild(CreateAbsoluteWrappedLabel(Loc.Text("ui.header.mode"), 13, _mutedTextColor, new Rect2(670, 54, 610, 18)));

		var languageButton = CreateAbsoluteButton(Loc.Text("ui.header.lang"), new Rect2(1446, 26, 88, 34), true);
		languageButton.Pressed += ToggleLanguage;
		panel.AddChild(languageButton);

		var saveButton = CreateAbsoluteButton(Loc.Text("ui.progress.save"), new Rect2(1544, 26, 88, 34), true);
		saveButton.Pressed += () => SaveGame(true);
		panel.AddChild(saveButton);

		var resetButton = CreateAbsoluteButton(Loc.Text("ui.progress.reset"), new Rect2(1642, 26, 88, 34), true);
		resetButton.Pressed += ResetSaveAndRestart;
		panel.AddChild(resetButton);

		_clockLabel = CreateAbsoluteLabel(string.Empty, 18, _textColor, new Rect2(1740, 28, 120, 26));
		_clockLabel.HorizontalAlignment = HorizontalAlignment.Right;
		panel.AddChild(_clockLabel);
		return panel;
	}

	private Control BuildHeroPanel()
	{
		_heroPanel = CreateAbsolutePanel(new Rect2(20, 126, 900, 390), _cardColor);
		_heroPanel.AddChild(CreateAbsoluteLabel(Loc.Text("ui.hero.title"), 16, _accentColor, new Rect2(24, 18, 180, 22)));
		_heroPanel.AddChild(CreateAbsoluteWrappedLabel(Loc.Text("ui.hero.desc"), 22, _textColor, new Rect2(24, 48, 780, 54)));

		_heroPanel.AddChild(CreateMiniStat(Loc.Text("ui.hero.goal"), Loc.Text("ui.hero.goalValue"), new Rect2(24, 112, 220, 56), _accentColor));
		_heroPanel.AddChild(CreateMiniStat(Loc.Text("ui.hero.path"), Loc.Text("ui.hero.pathValue"), new Rect2(258, 112, 250, 56), _infoColor));
		_heroPanel.AddChild(CreateMiniStat(Loc.Text("ui.hero.growth"), Loc.Text("ui.hero.growthValue"), new Rect2(522, 112, 250, 56), _positiveColor));

		_buffList = new HBoxContainer
		{
			Position = new Vector2(24, 180),
			Size = new Vector2(852, 44),
		};
		_buffList.AddThemeConstantOverride("separation", 8);
		_heroPanel.AddChild(_buffList);

		var stageFrame = CreateAbsolutePanel(new Rect2(24, 234, 852, 132), _cardColor.Darkened(0.12f));
		stageFrame.AddChild(CreateAbsoluteLabel(Loc.Text("ui.hero.stage"), 14, _accentColor, new Rect2(16, 10, 160, 18)));
		_stageVisual = TextureHelper.CreateImageOrFallback(
			_database.Skin.BackgroundTexturePath,
			"Forge",
			new Vector2(820, 86),
			_cardColor,
			_textColor);
		_stageVisual.Position = new Vector2(16, 32);
		_stageVisual.Size = new Vector2(820, 86);
		stageFrame.AddChild(_stageVisual);
		_heroPanel.AddChild(stageFrame);
		return _heroPanel;
	}

	private Control BuildCustomerPanel()
	{
		_customerPanel = CreateAbsolutePanel(new Rect2(20, 292, 900, 682), _cardColor);
		_customerPanel.AddChild(CreateAbsoluteLabel(Loc.Text("ui.customer.title"), 18, _accentColor, new Rect2(24, 18, 220, 24)));
		_customerPanel.AddChild(CreateAbsoluteWrappedLabel(Loc.Text("ui.customer.desc"), 15, _mutedTextColor, new Rect2(24, 48, 812, 36)));

		_customerScroll = new ScrollContainer
		{
			Position = new Vector2(24, 96),
			Size = new Vector2(852, 558),
			HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
			FollowFocus = true,
		};
		_customerPanel.AddChild(_customerScroll);

		_customerQueueList = new VBoxContainer
		{
			CustomMinimumSize = new Vector2(836, 0),
		};
		_customerQueueList.AddThemeConstantOverride("separation", 10);
		_customerScroll.AddChild(_customerQueueList);
		return _customerPanel;
	}

	private Control BuildForgePanel()
	{
		_forgePanel = CreateAbsolutePanel(new Rect2(940, 126, 380, 418), _cardColor);
		_forgePanel.AddChild(CreateAbsoluteLabel(Loc.Text("ui.forge.title"), 16, _accentColor, new Rect2(20, 18, 140, 22)));
		_forgePanel.AddChild(CreateAbsoluteWrappedLabel(Loc.Text("ui.forge.desc"), 14, _mutedTextColor, new Rect2(20, 46, 320, 34)));

		_forgeStatusLabel = CreateAbsoluteLabel(string.Empty, 14, _accentColor, new Rect2(20, 88, 300, 20));
		_forgePanel.AddChild(_forgeStatusLabel);

		_brewBar = BuildMeter();
		_brewBar.Position = new Vector2(20, 116);
		_brewBar.Size = new Vector2(340, 10);
		_forgePanel.AddChild(_brewBar);

		_productScroll = new ScrollContainer
		{
			Position = new Vector2(20, 140),
			Size = new Vector2(340, 256),
			HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
			VerticalScrollMode = ScrollContainer.ScrollMode.Auto,
			FollowFocus = true,
		};
		_forgePanel.AddChild(_productScroll);

		_productList = new VBoxContainer
		{
			CustomMinimumSize = new Vector2(324, 0),
		};
		_productList.AddThemeConstantOverride("separation", 8);
		_productScroll.AddChild(_productList);
		return _forgePanel;
	}

	private Control BuildCraftedPanel()
	{
		_craftedPanel = CreateAbsolutePanel(new Rect2(940, 564, 380, 410), _cardColor);
		_craftedPanel.AddChild(CreateAbsoluteLabel(Loc.Text("ui.rack.title"), 16, _accentColor, new Rect2(20, 18, 160, 22)));
		_craftedPanel.AddChild(CreateAbsoluteWrappedLabel(Loc.Text("ui.rack.desc"), 14, _mutedTextColor, new Rect2(20, 46, 320, 34)));

		_heldItemLabel = CreateAbsoluteLabel(string.Empty, 14, _infoColor, new Rect2(20, 88, 330, 20));
		_craftedPanel.AddChild(_heldItemLabel);

		_craftedScroll = new ScrollContainer
		{
			Position = new Vector2(20, 116),
			Size = new Vector2(340, 274),
			HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
			FollowFocus = true,
		};
		_craftedPanel.AddChild(_craftedScroll);

		_craftedList = new VBoxContainer
		{
			CustomMinimumSize = new Vector2(324, 0),
		};
		_craftedList.AddThemeConstantOverride("separation", 8);
		_craftedScroll.AddChild(_craftedList);
		return _craftedPanel;
	}

	private Control BuildProgressPanel()
	{
		_progressPanel = CreateAbsolutePanel(new Rect2(20, 126, 900, 146), _cardColor.Darkened(0.01f));
		_progressPanel.AddChild(CreateAbsoluteLabel(Loc.Text("ui.progress.title"), 16, _accentColor, new Rect2(20, 18, 160, 22)));

		_shiftLabel = CreateAbsoluteLabel(string.Empty, 20, _textColor, new Rect2(20, 48, 188, 24));
		_progressPanel.AddChild(_shiftLabel);
		_servedLabel = CreateAbsoluteLabel(string.Empty, 15, _mutedTextColor, new Rect2(20, 78, 218, 20));
		_progressPanel.AddChild(_servedLabel);
		_specialLabel = CreateAbsoluteLabel(string.Empty, 15, _accentColor, new Rect2(20, 106, 270, 20));
		_progressPanel.AddChild(_specialLabel);

		_comboLabel = CreateAbsoluteLabel(string.Empty, 15, _infoColor, new Rect2(318, 48, 250, 20));
		_progressPanel.AddChild(_comboLabel);

		_heatLabel = CreateAbsoluteLabel(string.Empty, 15, _accentColor, new Rect2(318, 78, 250, 20));
		_progressPanel.AddChild(_heatLabel);
		_heatBar = BuildMeter();
		_heatBar.Position = new Vector2(318, 108);
		_heatBar.Size = new Vector2(250, 10);
		_progressPanel.AddChild(_heatBar);

		_eventLabel = CreateAbsoluteLabel(string.Empty, 15, _textColor, new Rect2(604, 48, 250, 20));
		_progressPanel.AddChild(_eventLabel);
		_eventSubLabel = CreateAbsoluteWrappedLabel(string.Empty, 13, _mutedTextColor, new Rect2(604, 74, 260, 48));
		_progressPanel.AddChild(_eventSubLabel);
		return _progressPanel;
	}

	private Control BuildShopPanel()
	{
		_shopPanel = CreateAbsolutePanel(new Rect2(1340, 126, 560, 358), _cardColor);
		_shopPanel.AddChild(CreateAbsoluteLabel(Loc.Text("ui.shop.title"), 16, _accentColor, new Rect2(20, 18, 160, 22)));
		_shopPanel.AddChild(CreateAbsoluteWrappedLabel(Loc.Text("ui.shop.desc"), 14, _mutedTextColor, new Rect2(20, 46, 500, 28)));

		_shopScroll = new ScrollContainer
		{
			Position = new Vector2(20, 84),
			Size = new Vector2(520, 254),
			HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
			FollowFocus = true,
		};
		_shopPanel.AddChild(_shopScroll);

		_shopList = new VBoxContainer
		{
			CustomMinimumSize = new Vector2(504, 0),
		};
		_shopList.AddThemeConstantOverride("separation", 8);
		_shopScroll.AddChild(_shopList);
		return _shopPanel;
	}

	private Control BuildOwnedPanel()
	{
		_ownedPanel = CreateAbsolutePanel(new Rect2(1340, 504, 560, 470), _cardColor.Darkened(0.02f));
		_ownedPanel.AddChild(CreateAbsoluteLabel(Loc.Text("ui.owned.title"), 16, _accentColor, new Rect2(20, 18, 220, 22)));
		_ownedPanel.AddChild(CreateAbsoluteWrappedLabel(Loc.Text("ui.owned.desc"), 14, _mutedTextColor, new Rect2(20, 46, 500, 36)));

		_ownedScroll = new ScrollContainer
		{
			Position = new Vector2(20, 94),
			Size = new Vector2(520, 354),
			HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
			FollowFocus = true,
		};
		_ownedPanel.AddChild(_ownedScroll);

		_ownedList = new VBoxContainer
		{
			CustomMinimumSize = new Vector2(504, 0),
		};
		_ownedList.AddThemeConstantOverride("separation", 8);
		_ownedScroll.AddChild(_ownedList);
		return _ownedPanel;
	}

	private Control BuildToastLabel()
	{
		_toastLabel = CreateAbsoluteLabel(string.Empty, 18, _textColor, new Rect2(680, 980, 560, 26));
		_toastLabel.HorizontalAlignment = HorizontalAlignment.Center;
		_toastLabel.Modulate = new Color(1f, 1f, 1f, 0f);
		return _toastLabel;
	}

	private Control BuildDragGhost()
	{
		_dragGhost = CreateAbsoluteLabel(string.Empty, 16, _textColor, new Rect2(0, 0, 220, 28));
		_dragGhost.Visible = false;
		_dragGhost.ZIndex = 12;
		return _dragGhost;
	}

	private Control BuildModalBlocker()
	{
		_modalBlocker = new ColorRect
		{
			Color = new Color(0.02f, 0.03f, 0.05f, 0.62f),
			Position = Vector2.Zero,
			Size = new Vector2(DesignWidth, DesignHeight),
			Visible = false,
			MouseFilter = MouseFilterEnum.Stop,
			ZIndex = 9,
		};
		return _modalBlocker;
	}

	private Control BuildBlessingModal()
	{
		_blessingModal = CreateAbsolutePanel(new Rect2(520, 230, 880, 440), _cardColor.Darkened(0.08f));
		_blessingModal.Visible = false;
		_blessingModal.ZIndex = 10;
		_blessingModal.AddChild(CreateAbsoluteLabel(Loc.Text("ui.blessing.title"), 20, _accentColor, new Rect2(28, 24, 220, 24)));
		_blessingModal.AddChild(CreateAbsoluteWrappedLabel(Loc.Text("ui.blessing.desc"), 17, _textColor, new Rect2(28, 58, 824, 40)));

		_blessingChoiceList = new VBoxContainer
		{
			Position = new Vector2(28, 120),
			Size = new Vector2(824, 288),
		};
		_blessingChoiceList.AddThemeConstantOverride("separation", 12);
		_blessingModal.AddChild(_blessingChoiceList);
		return _blessingModal;
	}

	private Control CreateProductRow(ProductConfig product)
	{
		var row = UiFactory.CreateCard(_cardColor.Lightened(0.03f));
		row.CustomMinimumSize = new Vector2(324, 66);
		BindInteractiveCard(row);

		var icon = TextureHelper.CreateImageOrFallback(product.IconPath, "Item", new Vector2(42, 42), _cardColor, _textColor);
		icon.Position = new Vector2(10, 12);
		icon.Size = new Vector2(42, 42);
		row.AddChild(icon);

		row.AddChild(CreateAbsoluteLabel(ProductName(product), 15, _textColor, new Rect2(62, 8, 168, 19)));
		row.AddChild(CreateAbsoluteLabel(BuildProductStatLine(product), 11, _infoColor, new Rect2(62, 29, 184, 15)));
		row.AddChild(CreateAbsoluteLabel(BuildProductCostLine(product), 11, _mutedTextColor, new Rect2(62, 46, 184, 15)));
		row.TooltipText = BuildProductTooltip(product);

		var button = CreateAbsoluteButton(Loc.Text("ui.button.craft"), new Rect2(250, 17, 62, 32), true);
		button.Disabled = _state.IsBrewing || _state.CraftedRackFull || _state.Coins < product.Cost;
		button.Pressed += () =>
		{
			var result = _state.StartCraft(product, _database);
			RefreshAll(result.Message);
			if (result.Success)
			{
				PlayButtonFeedback(button);
				PlayPanelFlash(_forgePanel);
				ShowFloatingText(Loc.Format("ui.float.cost", product.Cost), new Vector2(1020, 186), _warningColor, 18);
			}
			else
			{
				ShowToast(result.Message);
			}
		};
		row.AddChild(button);
		return row;
	}

	private Control CreateCraftedRow(int index, CraftedItem craftedItem)
	{
		if (_isDraggingCrafted && _heldCraftedIndex == index)
		{
			var placeholder = UiFactory.CreateCard(_cardColor.Darkened(0.04f));
			placeholder.CustomMinimumSize = new Vector2(324, 66);
			placeholder.Modulate = new Color(1f, 1f, 1f, 0.18f);
			placeholder.AddChild(CreateAbsoluteLabel(Loc.Text("ui.rack.slotHeld"), 13, _mutedTextColor, new Rect2(16, 22, 220, 18)));
			return placeholder;
		}

		var product = _database.GetProduct(craftedItem.ProductId);
		var row = UiFactory.CreateCard(index == _heldCraftedIndex ? _cardColor.Lightened(0.10f) : _cardColor.Lightened(0.03f));
		row.CustomMinimumSize = new Vector2(324, 76);
		BindInteractiveCard(row);
		row.MouseFilter = MouseFilterEnum.Stop;
		row.GuiInput += @event =>
		{
			if (@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed)
			{
				BeginCraftedDrag(index, CraftedName(craftedItem));
				GetViewport().SetInputAsHandled();
			}
		};

		if (product != null)
		{
			var icon = TextureHelper.CreateImageOrFallback(product.IconPath, "Ready", new Vector2(40, 40), _cardColor, _textColor);
			icon.Position = new Vector2(10, 13);
			icon.Size = new Vector2(40, 40);
			row.AddChild(icon);
			row.AddChild(CreateAbsoluteLabel(CraftedName(craftedItem), 14, RarityColor(craftedItem), new Rect2(60, 7, 176, 19)));
			row.AddChild(CreateAbsoluteLabel(BuildCraftedStatLine(craftedItem), 11, _infoColor, new Rect2(60, 28, 176, 16)));
			row.TooltipText = BuildCraftedTooltip(craftedItem);
		}

		row.AddChild(CreateAbsoluteWrappedLabel(Loc.Text("ui.rack.tip"), 10, _mutedTextColor, new Rect2(60, 47, 160, 24)));

		var button = CreateAbsoluteButton(index == _heldCraftedIndex ? Loc.Text("ui.button.cancel") : Loc.Text("ui.button.hold"), new Rect2(242, 8, 70, 28), true);
		button.Pressed += () =>
		{
			if (_heldCraftedIndex == index)
			{
				CancelCraftedDrag();
			}
			else
			{
				BeginCraftedDrag(index, CraftedName(craftedItem));
			}

			PlayButtonFeedback(button);
		};
		row.AddChild(button);

		var discardButton = CreateAbsoluteButton(Loc.Text("ui.button.discard"), new Rect2(242, 42, 70, 26), true);
		discardButton.Pressed += () =>
		{
			if (_heldCraftedIndex == index)
			{
				_heldCraftedIndex = -1;
				_isDraggingCrafted = false;
			}

			var result = _state.DiscardCraftedItem(index);
			_hoveredOrderIndex = -1;
			_hoveredSlot = string.Empty;
			RefreshAll(result.Message);
			ShowToast(result.Message);
			PlayButtonFeedback(discardButton);
		};
		row.AddChild(discardButton);
		return row;
	}

	private Control CreateHeroRow(int index, BattleHero hero)
	{
		var customer = _database.GetCustomer(hero.CustomerId);
		var heldMatches = DoesHeldItemMatchHeroSlot(index, _hoveredSlot);
		var hoveredWithDrag = _hoveredOrderIndex == index && _isDraggingCrafted && !string.IsNullOrWhiteSpace(_hoveredSlot);
		var rowColor = hoveredWithDrag
			? (heldMatches ? _positiveColor.Darkened(0.34f) : _warningColor.Darkened(0.38f))
			: _cardColor.Lightened(0.03f);
		var row = UiFactory.CreateCard(rowColor);
		row.CustomMinimumSize = new Vector2(836, 162);
		BindInteractiveCard(row);
		row.MouseFilter = MouseFilterEnum.Pass;
		_combatRows.Add(row);

		var enemy = hero.Enemy;
		var enemyCard = UiFactory.CreateCard(_cardColor.Darkened(0.05f));
		enemyCard.Position = new Vector2(12, 14);
		enemyCard.Size = new Vector2(228, 118);
		row.AddChild(enemyCard);
		_enemyCards.Add(enemyCard);
		enemyCard.AddChild(CreateAbsoluteLabel(Loc.Format("ui.hero.lane", hero.LaneWave), 10, _accentColor, new Rect2(12, 8, 78, 14)));
		if (enemy != null)
		{
			enemyCard.AddChild(CreateAbsoluteLabel(Loc.Format("ui.hero.enemy", enemy.Name, enemy.Attack, enemy.Defense), 12, _textColor, new Rect2(12, 26, 204, 16)));
			enemyCard.AddChild(CreateAbsoluteLabel(Loc.Text("ui.hero.enemyHp"), 10, _mutedTextColor, new Rect2(12, 50, 72, 14)));
			var enemyHp = BuildMeter();
			enemyHp.Position = new Vector2(58, 53);
			enemyHp.Size = new Vector2(110, 8);
			enemyHp.Value = enemy.HpPercent * 100f;
			enemyCard.AddChild(enemyHp);
			_enemyHpBars.Add(enemyHp);
			var enemyHpText = CreateAbsoluteLabel($"{enemy.Hp}/{enemy.MaxHp}", 10, _mutedTextColor, new Rect2(170, 46, 48, 16));
			enemyHpText.HorizontalAlignment = HorizontalAlignment.Right;
			enemyCard.AddChild(enemyHpText);
			_enemyHpTexts.Add(enemyHpText);

			enemyCard.AddChild(CreateAbsoluteLabel(Loc.Format("ui.hero.cast", enemy.AttackSeconds), 10, _warningColor, new Rect2(12, 76, 84, 14)));
			var enemyAttack = BuildMeter();
			enemyAttack.Position = new Vector2(96, 79);
			enemyAttack.Size = new Vector2(110, 8);
			enemyAttack.Value = hero.EnemyAttackPercent * 100f;
			enemyCard.AddChild(enemyAttack);
			_enemyAttackBars.Add(enemyAttack);
		}

		var icon = TextureHelper.CreateImageOrFallback(customer?.SpritePath ?? _database.Skin.DefaultCustomerTexturePath, "Adventurer", new Vector2(58, 58), _cardColor, _textColor);
		icon.Position = new Vector2(258, 20);
		icon.Size = new Vector2(58, 58);
		row.AddChild(icon);
		_heroCards.Add(icon);

		row.AddChild(CreateAbsoluteLabel(customer == null ? hero.CustomerId : CustomerName(customer), 16, _textColor, new Rect2(330, 14, 190, 20)));
		row.AddChild(CreateAbsoluteLabel(Loc.Format("ui.hero.stats", hero.Attack(_database), hero.Defense(_database), hero.MaxHp(_database), hero.AttackSeconds(_database)), 12, _infoColor, new Rect2(330, 37, 280, 16)));
		row.AddChild(CreateAbsoluteWrappedLabel(customer == null ? string.Empty : CustomerLine(customer), 11, _mutedTextColor, new Rect2(330, 58, 280, 28)));
		row.AddChild(CreateAbsoluteLabel(Loc.Text("ui.hero.hp"), 11, _mutedTextColor, new Rect2(258, 92, 52, 16)));

		var patience = BuildMeter();
		patience.Position = new Vector2(306, 96);
		patience.Size = new Vector2(170, 9);
		patience.Value = hero.HpPercent(_database) * 100f;
		row.AddChild(patience);
		_patienceBars.Add(patience);

		var patienceText = CreateAbsoluteLabel($"{hero.Hp}/{hero.MaxHp(_database)}", 11, _mutedTextColor, new Rect2(480, 89, 64, 16));
		patienceText.HorizontalAlignment = HorizontalAlignment.Right;
		row.AddChild(patienceText);
		_patienceTexts.Add(patienceText);

		row.AddChild(CreateAbsoluteLabel(hero.IsFullyEquipped ? Loc.Format("ui.hero.cast", hero.AttackSeconds(_database)) : Loc.Format("ui.hero.waitGearSlots", hero.RequiredSlots.Count), 11, hero.IsFullyEquipped ? _positiveColor : _warningColor, new Rect2(258, 116, 138, 16)));
		var heroAttack = BuildMeter();
		heroAttack.Position = new Vector2(402, 120);
		heroAttack.Size = new Vector2(142, 8);
		heroAttack.Value = hero.AttackPercent(_database) * 100f;
		row.AddChild(heroAttack);
		_heroAttackBars.Add(heroAttack);
		var buffLabel = CreateAbsoluteLabel(BuildHeroBuffLine(hero), 11, hero.ActiveBuffs.Count > 0 ? _positiveColor : _mutedTextColor, new Rect2(258, 136, 350, 16));
		row.AddChild(buffLabel);
		_heroBuffLabels.Add(buffLabel);

		row.AddChild(CreateAbsoluteLabel(Loc.Format("ui.hero.needSlots", string.Join(" / ", hero.RequiredSlots.Select(GameState.SlotToDisplay))), 10, _mutedTextColor, new Rect2(620, 8, 194, 16)));
		for (var slotIndex = 0; slotIndex < hero.RequiredSlots.Count; slotIndex++)
		{
			row.AddChild(CreateHeroSlot(index, hero, hero.RequiredSlots[slotIndex], new Vector2(646, 30 + slotIndex * 42)));
		}
		return row;
	}

	private Control CreateHeroSlot(int heroIndex, BattleHero hero, string slotName, Vector2 position)
	{
		var hoveredWithDrag = _hoveredOrderIndex == heroIndex && _hoveredSlot == slotName && _isDraggingCrafted;
		var heldMatches = DoesHeldItemMatchHeroSlot(heroIndex, slotName);
		var slotColor = hoveredWithDrag
			? (heldMatches ? _positiveColor.Darkened(0.18f) : _warningColor.Darkened(0.20f))
			: _cardColor.Darkened(0.06f);
		var slot = UiFactory.CreateCard(slotColor);
		slot.Position = position;
		slot.Size = new Vector2(168, 34);
		slot.MouseFilter = MouseFilterEnum.Stop;
		slot.MouseEntered += () =>
		{
			_hoveredOrderIndex = heroIndex;
			_hoveredSlot = slotName;
		};
		slot.MouseExited += () =>
		{
			if (_hoveredOrderIndex == heroIndex && _hoveredSlot == slotName)
			{
				_hoveredOrderIndex = -1;
				_hoveredSlot = string.Empty;
			}
		};
		slot.GuiInput += @event =>
		{
			if (_isDraggingCrafted && @event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left && !mouseButton.Pressed)
			{
				TryEquipHeldCraftedTo(heroIndex, slotName);
				GetViewport().SetInputAsHandled();
			}
		};
		var itemId = hero.GetItem(slotName);
		var item = _database.GetProduct(itemId);
		var crafted = hero.GetEquippedItem(slotName);
		if (crafted != null)
		{
			slot.TooltipText = BuildCraftedTooltip(crafted);
		}
		else if (item != null)
		{
			slot.TooltipText = BuildProductTooltip(item);
		}
		var slotTitle = hoveredWithDrag
			? (heldMatches ? Loc.Text("ui.customer.slotReady") : Loc.Text("ui.customer.slotBlocked"))
			: $"{GameState.SlotToDisplay(slotName)}: {(crafted == null ? Loc.Text("ui.hero.emptySlot") : CraftedName(crafted))}";
		var slotTitleColor = hoveredWithDrag ? (heldMatches ? _positiveColor : _warningColor) : _accentColor;
		slot.AddChild(CreateAbsoluteLabel(slotTitle, 11, slotTitleColor, new Rect2(10, 7, 148, 16)));
		return slot;
	}

	private Control CreateDecorRow(DecorConfig decor)
	{
		var row = UiFactory.CreateCard(_cardColor.Lightened(0.03f));
		row.CustomMinimumSize = new Vector2(504, 58);
		BindInteractiveCard(row);

		row.AddChild(CreateAbsoluteLabel(DecorName(decor), 15, _textColor, new Rect2(14, 9, 220, 20)));
		row.AddChild(CreateAbsoluteLabel(BuildDecorEffectLine(decor), 11, _infoColor, new Rect2(14, 32, 230, 16)));
		row.AddChild(CreateAbsoluteLabel(Loc.Format("ui.decor.cost", decor.Cost), 11, _mutedTextColor, new Rect2(248, 32, 90, 16)));

		var button = CreateAbsoluteButton(Loc.Text("ui.button.buy"), new Rect2(400, 14, 82, 32), true);
		button.Disabled = _state.OwnedDecorIds.Contains(decor.Id) || _state.Coins < decor.Cost;
		button.Pressed += () =>
		{
			var result = _state.BuyDecor(decor);
			RefreshAll(result.Message);
			if (result.Success)
			{
				PlayButtonFeedback(button);
				PlayPanelFlash(_shopPanel);
			}
		};
		row.AddChild(button);
		return row;
	}

	private Control CreateBlessingRow(BlessingConfig blessing)
	{
		var row = UiFactory.CreateCard(_cardColor.Lightened(0.02f));
		row.CustomMinimumSize = new Vector2(824, 86);
		BindInteractiveCard(row);

		row.AddChild(CreateAbsoluteLabel(BlessingName(blessing), 18, _accentColor, new Rect2(16, 12, 240, 22)));
		row.AddChild(CreateAbsoluteLabel(blessing.Archetype, 13, _infoColor, new Rect2(16, 38, 180, 18)));
		row.AddChild(CreateAbsoluteWrappedLabel(blessing.Description, 13, _mutedTextColor, new Rect2(210, 14, 430, 40)));

		var button = CreateAbsoluteButton(Loc.Text("ui.button.choose"), new Rect2(684, 22, 110, 40));
		button.Pressed += () =>
		{
			var message = _state.ApplyBlessing(blessing);
			CloseBlessingModal();
			RefreshAll(message);
			ShowToast(message);
			ShowFloatingText(Loc.Text("ui.float.blessing"), new Vector2(760, 220), _accentColor, 24);
			PlayPanelFlash(_heroPanel);
			PlayButtonFeedback(button);
		};
		row.AddChild(button);
		return row;
	}

	private void RefreshAll(string message)
	{
		_lastMessage = message;
		RefreshCustomerQueue();
		RebuildProductList();
		RebuildCraftedList();
		RebuildShopList();
		RefreshOwnedPanel();
		RefreshBuffList();
		RefreshProgressLabels();
		_messageLabel = _messageLabel ?? CreateAbsoluteLabel(string.Empty, 14, _mutedTextColor, new Rect2(0, 0, 0, 0));
		EnsureHeldIndexValid();
		EnsurePrimaryFocus();
	}

	private void RefreshCustomerQueue()
	{
		ClearChildren(_customerQueueList);
		_patienceBars.Clear();
		_patienceTexts.Clear();
		_heroAttackBars.Clear();
		_heroBuffLabels.Clear();
		_enemyHpBars.Clear();
		_enemyAttackBars.Clear();
		_enemyHpTexts.Clear();
		_combatRows.Clear();
		_enemyCards.Clear();
		_heroCards.Clear();

		foreach (var (hero, index) in _state.Heroes.Select((hero, index) => (hero, index)))
		{
			_customerQueueList.AddChild(CreateHeroRow(index, hero));
		}

		_lastKnownOrderCount = _state.Heroes.Count;
	}

	private void RebuildProductList()
	{
		ClearChildren(_productList);
		foreach (var product in _database.GetProductsForRun(_state.ShiftLevel, _state.Reputation))
		{
			_productList.AddChild(CreateProductRow(product));
		}
	}

	private void RebuildCraftedList()
	{
		ClearChildren(_craftedList);
		if (_state.CraftedItems.Count == 0)
		{
			_craftedList.AddChild(UiFactory.CreateWrappedLabel(Loc.Text("ui.rack.none"), 15, _mutedTextColor));
			return;
		}

		for (var i = 0; i < _state.CraftedItems.Count; i++)
		{
			_craftedList.AddChild(CreateCraftedRow(i, _state.CraftedItems[i]));
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

	private void RefreshOwnedPanel()
	{
		ClearChildren(_ownedList);

		_ownedList.AddChild(CreateSectionLabel(Loc.Text("ui.owned.decorSection")));
		if (_state.OwnedDecorIds.Count == 0)
		{
			_ownedList.AddChild(UiFactory.CreateWrappedLabel(Loc.Text("ui.owned.empty"), 15, _mutedTextColor));
		}
		else
		{
			foreach (var decorId in _state.OwnedDecorIds)
			{
				var decor = _database.GetDecor(decorId);
				if (decor == null)
				{
					continue;
				}

				var row = UiFactory.CreateCard(_cardColor.Lightened(0.03f));
				row.CustomMinimumSize = new Vector2(504, 48);
				row.AddChild(CreateAbsoluteLabel(DecorName(decor), 15, _textColor, new Rect2(12, 13, 240, 18)));
				var gain = CreateAbsoluteLabel(BuildDecorEffectLine(decor), 13, _positiveColor, new Rect2(246, 13, 238, 18));
				gain.HorizontalAlignment = HorizontalAlignment.Right;
				row.AddChild(gain);
				_ownedList.AddChild(row);
			}
		}

	}

	private void RefreshBuffList()
	{
		if (_buffList == null)
		{
			return;
		}

		ClearChildren(_buffList);
	}

	private static string BuildDecorEffectLine(DecorConfig decor)
	{
		var craftPercent = decor.PassiveCoinsPerSecond * 0.45f;
		var rewardPercent = decor.PassiveCoinsPerSecond * 0.6f;
		return Loc.Format("ui.decor.effect", craftPercent, rewardPercent);
	}

	private void RefreshProgressLabels()
	{
		var special = _database.GetProduct(_state.TodaySpecialProductId);
		_shiftLabel.Text = Loc.Format("ui.progress.wave", _state.CombatWave);
		_servedLabel.Text = Loc.Format("ui.progress.enemyHp", _state.Enemy.Name, _state.Enemy.Hp, _state.Enemy.MaxHp);
		_specialLabel.Text = special == null ? Loc.Text("ui.progress.special.none") : Loc.Format("ui.progress.special", ProductName(special));
		_comboLabel.Text = Loc.Format("ui.progress.party", _state.Heroes.Count(x => x.Hp > 0), _state.Heroes.Count);
		_heatLabel.Text = Loc.Format("ui.progress.heat", _state.ForgeHeat, GameState.MaxForgeHeat);
		_heatBar.Value = _state.ForgeHeat;

		if (_state.HasActiveEvent)
		{
			_eventLabel.Text = $"{_state.CurrentEventTitle}  {_state.EventProgress}/{_state.EventTargetCount}";
			_eventSubLabel.Text = Loc.Format("ui.progress.event.reward", _state.CurrentEventDescription, _state.EventRewardCoins);
		}
		else
		{
			_eventLabel.Text = Loc.Text("ui.progress.event.wait");
			_eventSubLabel.Text = Loc.Format("ui.progress.event.next", Math.Max(0, _state.NextEventAt - _state.ServedCount));
		}
	}

	private void RefreshPatienceBars()
	{
		for (var i = 0; i < _patienceBars.Count && i < _state.Heroes.Count; i++)
		{
			var hero = _state.Heroes[i];
			_patienceBars[i].Value = hero.HpPercent(_database) * 100f;
			_patienceBars[i].Modulate = hero.HpPercent(_database) < 0.35f ? _warningColor : Colors.White;
			if (i < _patienceTexts.Count)
			{
				_patienceTexts[i].Text = $"{hero.Hp}/{hero.MaxHp(_database)}";
				_patienceTexts[i].Modulate = hero.HpPercent(_database) < 0.35f ? _warningColor : Colors.White;
			}

			if (i < _heroAttackBars.Count)
			{
				_heroAttackBars[i].Value = hero.AttackPercent(_database) * 100f;
				_heroAttackBars[i].Modulate = hero.IsFullyEquipped ? Colors.White : new Color(1f, 1f, 1f, 0.28f);
			}

			if (i < _heroBuffLabels.Count)
			{
				_heroBuffLabels[i].Text = BuildHeroBuffLine(hero);
				_heroBuffLabels[i].Modulate = hero.ActiveBuffs.Count > 0 ? _positiveColor : _mutedTextColor;
			}

			if (i < _enemyAttackBars.Count)
			{
				_enemyAttackBars[i].Value = hero.EnemyAttackPercent * 100f;
				_enemyAttackBars[i].Modulate = hero.IsFullyEquipped ? Colors.White : new Color(1f, 1f, 1f, 0.28f);
			}

			if (hero.Enemy != null && i < _enemyHpBars.Count)
			{
				_enemyHpBars[i].Value = hero.Enemy.HpPercent * 100f;
				_enemyHpBars[i].Modulate = hero.Enemy.HpPercent < 0.35f ? _warningColor : Colors.White;
			}

			if (hero.Enemy != null && i < _enemyHpTexts.Count)
			{
				_enemyHpTexts[i].Text = $"{hero.Enemy.Hp}/{hero.Enemy.MaxHp}";
			}
		}
	}

	private void HandleCraftCompleted(CraftResult result)
	{
		RefreshAll(result.Message);
		PlayPanelFlash(_forgePanel);
		PlayPanelFlash(_craftedPanel);
		PlayStagePulse();
		ShowToast(result.Message);
		ShowFloatingText(Loc.Text("ui.float.ready"), new Vector2(980, 560), _positiveColor, 22);
	}

	private void HandleDeliveryCompleted(DeliveryResult result)
	{
		PlayPanelFlash(_customerPanel);
		PlayPanelFlash(_craftedPanel);
		PlayResourcePulse();
		PlayStagePulse();
		ShowToast(Loc.Format("ui.toast.income", result.Coins, result.Aroma));
		ShowFloatingText(Loc.Format("ui.float.gold", result.Coins), new Vector2(660, 86), _accentColor, 24);
		ShowFloatingText(Loc.Format("ui.float.ember", result.Aroma), new Vector2(790, 86), _infoColor, 20);
		ShowFloatingText(Loc.Format("ui.float.heat", result.HeatGain), new Vector2(1480, 216), _accentColor, 20);

		if (result.FavoriteMatched)
		{
			ShowFloatingText(Loc.Text("ui.float.perfect"), new Vector2(250, 548), _positiveColor, 22);
		}

		if (result.SpecialMatched)
		{
			ShowFloatingText(Loc.Text("ui.float.special"), new Vector2(250, 584), _infoColor, 20);
		}

		if (result.HeatSurged)
		{
			ShowFloatingText(Loc.Format("ui.float.surge", result.HeatSurgeCoins), new Vector2(1450, 250), _accentColor, 26);
			PlayHeatSurge();
		}

		if (result.ShiftAdvanced)
		{
			ShowToast(Loc.Format("ui.toast.shift", _state.ShiftLevel));
		}

		if (!string.IsNullOrWhiteSpace(result.EventMessage))
		{
			ShowToast(result.EventMessage);
		}

	}

	private void HandleEquipCompleted(DeliveryResult result)
	{
		PlayPanelFlash(_customerPanel);
		PlayPanelFlash(_craftedPanel);
		ShowToast(result.Message);
		if (result.HeatGain > 0)
		{
			ShowFloatingText(Loc.Format("ui.float.heat", result.HeatGain), new Vector2(980, 560), _accentColor, 20);
		}
	}

	private void HandleCombatPulse()
	{
		RefreshCustomerQueue();
		RefreshProgressLabels();
		if (!string.IsNullOrWhiteSpace(_state.CombatMessage))
		{
			ShowToast(_state.CombatMessage);
		}

		if (_state.LastCombatDamage > 0)
		{
			var laneIndex = Math.Clamp(_state.LastCombatHeroIndex, 0, 2);
			var laneY = 402 + laneIndex * 160;
			var effectKey = _state.LastCombatEffect switch
			{
				"fireball" => "ui.float.fireball",
				"arrow" => "ui.float.arrow",
				"enemy" => "ui.float.enemy",
				_ => "ui.float.slash",
			};
			var effectX = _state.LastCombatEffect == "enemy" ? 118 : 320;
			var damageX = _state.LastCombatEffect == "enemy" ? 430 : 136;
			ShowFloatingText(Loc.Text(effectKey), new Vector2(effectX, laneY), _infoColor, 20);
			ShowFloatingText(Loc.Format("ui.float.damage", _state.LastCombatDamage), new Vector2(damageX, laneY), _warningColor, 22);
			PlayCombatLaneFeedback(laneIndex, _state.LastCombatEffect);
		}

		if (_state.LastCombatGold > 0)
		{
			ShowFloatingText(Loc.Format("ui.float.gold", _state.LastCombatGold), new Vector2(660, 86), _accentColor, 24);
			PlayResourcePulse();
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
		if (open)
		{
			ShowToast(Loc.Text("ui.toast.blessing"));
			PlayModalPop();
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

	private void RefreshDragGhost()
	{
		if (!_isDraggingCrafted || _heldCraftedIndex < 0 || _heldCraftedIndex >= _state.CraftedItems.Count)
		{
			_dragGhost.Visible = false;
			return;
		}

		var crafted = _state.CraftedItems[_heldCraftedIndex];
		var product = _database.GetProduct(crafted.ProductId);
		_dragGhost.Text = product == null
			? Loc.Format("ui.rack.drag", crafted.ProductId)
			: Loc.Format("ui.rack.dragGear", ProductName(product), GameState.SlotToDisplay(product.Slot));
		_dragGhost.Visible = true;

		var mouse = GetViewport().GetMousePosition();
		var local = (mouse - _canvasRoot.Position) / _canvasRoot.Scale;
		_dragGhost.Position = local + new Vector2(20, 12);
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

	private void ResetSaveAndRestart()
	{
		SaveSystem.Delete();
		_state = new GameState();
		_heldCraftedIndex = -1;
		_hoveredOrderIndex = -1;
		_isDraggingCrafted = false;
		_customerTimer = 0f;
		_autosaveTimer = 0f;
		_state.EnsureTodaySpecial(_database);
		_state.EnsureCustomer(_database);
		_state.EnsureNightEvent(_database);
		RefreshAll(Loc.Text("ui.toast.reset"));
		ShowToast(Loc.Text("ui.toast.reset"));
	}

	private void BeginCraftedDrag(int index, string itemName)
	{
		_heldCraftedIndex = index;
		_isDraggingCrafted = true;
		_hoveredOrderIndex = -1;
		RefreshAll(Loc.Format("ui.rack.pick", itemName));
	}

	private void CancelCraftedDrag()
	{
		_isDraggingCrafted = false;
		_hoveredOrderIndex = -1;
		_hoveredSlot = string.Empty;
		RefreshAll(Loc.Text("ui.rack.cancel"));
	}

	private bool DoesHeldItemMatchHeroSlot(int heroIndex, string slot)
	{
		if (!_isDraggingCrafted || _heldCraftedIndex < 0 || _heldCraftedIndex >= _state.CraftedItems.Count || heroIndex < 0 || heroIndex >= _state.Heroes.Count || string.IsNullOrWhiteSpace(slot))
		{
			return false;
		}

		var product = _database.GetProduct(_state.CraftedItems[_heldCraftedIndex].ProductId);
		return product != null && _state.Heroes[heroIndex].RequiresSlot(slot) && GameState.SlotMatches(product, slot);
	}

	private void TryEquipHeldCraftedTo(int heroIndex, string slot)
	{
		if (_heldCraftedIndex < 0)
		{
			return;
		}

		var result = _state.EquipCraftedItem(_heldCraftedIndex, heroIndex, slot, _database);
		_heldCraftedIndex = -1;
		_hoveredOrderIndex = -1;
		_hoveredSlot = string.Empty;
		_isDraggingCrafted = false;
		RefreshAll(result.Message);

		if (result.Success)
		{
			HandleEquipCompleted(result);
		}
		else
		{
			ShowToast(result.Message);
		}
	}

	private void EnsurePrimaryFocus()
	{
		CallDeferred(nameof(FocusPrimaryButton));
	}

	private void FocusPrimaryButton()
	{
		var root = _heldCraftedIndex >= 0 ? _customerQueueList : _productList;
		var firstButton = FindFirstButton(root);
		firstButton?.GrabFocus();
	}

	private void SetMainUiInteractable(bool enabled)
	{
		SetButtonsDisabled(_canvasRoot, !enabled, _canvasRoot);
		if (enabled)
		{
			return;
		}

		var focused = GetViewport().GuiGetFocusOwner();
		if (focused != null)
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

	private void EnsureHeldIndexValid()
	{
		if (_heldCraftedIndex >= _state.CraftedItems.Count)
		{
			_heldCraftedIndex = -1;
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

	private void OnViewportSizeChanged()
	{
		if (_canvasRoot == null)
		{
			return;
		}

		var viewportSize = GetViewportRect().Size;
		if (viewportSize.X <= 0 || viewportSize.Y <= 0)
		{
			return;
		}

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

		canvasSize = new Vector2(Mathf.Round(canvasSize.X), Mathf.Round(canvasSize.Y));
		var canvasPosition = (viewportSize - canvasSize) * 0.5f;
		canvasPosition = new Vector2(Mathf.Round(canvasPosition.X), Mathf.Round(canvasPosition.Y));

		_canvasRoot.Size = new Vector2(DesignWidth, DesignHeight);
		_canvasRoot.Scale = new Vector2(canvasSize.X / DesignWidth, canvasSize.Y / DesignHeight);
		_canvasRoot.Position = canvasPosition;
	}

	private void AnimateAmbient()
	{
		var time = (float)Time.GetTicksMsec() / 1000f;
		_ambientBlobA.Position = new Vector2(90 + Mathf.Sin(time * 0.42f) * 28f, 110 + Mathf.Cos(time * 0.32f) * 20f);
		_ambientBlobB.Position = new Vector2(1450 + Mathf.Cos(time * 0.28f) * 20f, 620 + Mathf.Sin(time * 0.24f) * 18f);

		if (_stageVisual != null)
		{
			_stageVisual.Modulate = new Color(1f, 1f, 1f, 0.97f + Mathf.Sin(time * 1.05f) * 0.03f);
		}
	}

	private void PlayButtonFeedback(Control control)
	{
		control.Scale = new Vector2(0.96f, 0.96f);
		var tween = CreateTween();
		tween.TweenProperty(control, "scale", Vector2.One, 0.16f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
	}

	private void PlayPanelFlash(Control panel)
	{
		if (panel == null)
		{
			return;
		}

		panel.Modulate = new Color(1f, 1f, 1f, 0.85f);
		var tween = CreateTween();
		tween.TweenProperty(panel, "modulate", Colors.White, 0.22f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
	}

	private void PlayResourcePulse()
	{
		_resourceLabel.Scale = new Vector2(1.03f, 1.03f);
		_resourceLabel.Modulate = new Color(1.18f, 1.08f, 0.86f, 1f);
		var tween = CreateTween();
		tween.SetParallel(true);
		tween.TweenProperty(_resourceLabel, "scale", Vector2.One, 0.18f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
		tween.TweenProperty(_resourceLabel, "modulate", Colors.White, 0.28f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
	}

	private void PlayHeatSurge()
	{
		_progressPanel.Modulate = new Color(1.2f, 1.04f, 0.78f, 1f);
		var tween = CreateTween();
		tween.SetParallel(true);
		tween.TweenProperty(_progressPanel, "modulate", Colors.White, 0.36f).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
		tween.TweenProperty(_heatBar, "scale", new Vector2(1.04f, 1.8f), 0.12f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
		tween.Chain().TweenProperty(_heatBar, "scale", Vector2.One, 0.18f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
	}

	private void PlayModalPop()
	{
		_blessingModal.Scale = new Vector2(0.96f, 0.96f);
		_blessingModal.Modulate = new Color(1f, 1f, 1f, 0f);
		var tween = CreateTween();
		tween.SetParallel(true);
		tween.TweenProperty(_blessingModal, "scale", Vector2.One, 0.18f).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
		tween.TweenProperty(_blessingModal, "modulate", Colors.White, 0.16f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
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
		tween.TweenProperty(_stageVisual, "modulate", new Color(1.06f, 1.04f, 0.98f, 1f), 0.10f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
		tween.Chain().TweenProperty(_stageVisual, "modulate", Colors.White, 0.18f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
	}

	private void PlayCombatLaneFeedback(int laneIndex, string effect)
	{
		if (laneIndex < 0 || laneIndex >= _combatRows.Count)
		{
			return;
		}

		var row = _combatRows[laneIndex];
		row.Modulate = new Color(1.08f, 1.05f, 0.94f, 1f);
		var rowTween = CreateTween();
		rowTween.TweenProperty(row, "modulate", Colors.White, 0.22f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);

		if (effect == "enemy")
		{
			if (laneIndex < _enemyCards.Count)
			{
				NudgeControl(_enemyCards[laneIndex], new Vector2(10, 0), _warningColor.Lightened(0.12f));
			}

			if (laneIndex < _heroCards.Count)
			{
				NudgeControl(_heroCards[laneIndex], new Vector2(0, 5), _warningColor);
			}

			return;
		}

		if (laneIndex < _heroCards.Count)
		{
			NudgeControl(_heroCards[laneIndex], new Vector2(-10, 0), _infoColor.Lightened(0.1f));
		}

		if (laneIndex < _enemyCards.Count)
		{
			NudgeControl(_enemyCards[laneIndex], new Vector2(0, -4), _warningColor);
		}
	}

	private void NudgeControl(Control control, Vector2 offset, Color flashColor)
	{
		var start = control.Position;
		control.Modulate = new Color(flashColor.R, flashColor.G, flashColor.B, 1f);
		var tween = CreateTween();
		tween.SetParallel(true);
		tween.TweenProperty(control, "position", start + offset, 0.08f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
		tween.TweenProperty(control, "modulate", Colors.White, 0.18f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
		tween.Chain().TweenProperty(control, "position", start, 0.14f).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
	}

	private void ShowToast(string text)
	{
		_toastLabel.Text = text;
		_toastLabel.Position = new Vector2(680, 980);
		_toastLabel.Modulate = new Color(1f, 1f, 1f, 0f);

		var tween = CreateTween();
		tween.SetParallel(true);
		tween.TweenProperty(_toastLabel, "modulate", new Color(1f, 1f, 1f, 1f), 0.18f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
		tween.TweenProperty(_toastLabel, "position", new Vector2(680, 966), 0.22f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
		tween.Chain().TweenInterval(0.65f);
		tween.Chain().TweenProperty(_toastLabel, "modulate", new Color(1f, 1f, 1f, 0f), 0.2f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.In);
	}

	private void ShowFloatingText(string text, Vector2 position, Color color, int fontSize = 20)
	{
		var label = CreateAbsoluteLabel(text, fontSize, color, new Rect2(position.X, position.Y, 360, 32));
		label.ZIndex = 20;
		label.Modulate = new Color(color, 0f);
		_canvasRoot.AddChild(label);

		var tween = CreateTween();
		tween.SetParallel(true);
		tween.TweenProperty(label, "position", position + new Vector2(0, -38), 0.62f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
		tween.TweenProperty(label, "modulate", color, 0.12f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
		tween.Chain().TweenProperty(label, "modulate", new Color(color, 0f), 0.24f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.In);
		tween.TweenCallback(Callable.From(label.QueueFree));
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
		tween.TweenProperty(card, "scale", active ? new Vector2(1.01f, 1.01f) : Vector2.One, 0.14f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
		tween.TweenProperty(card, "modulate", active ? new Color(1.04f, 1.04f, 1.04f, 1f) : Colors.White, 0.16f).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
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
		label.ClipText = true;
		return label;
	}

	private Label CreateAbsoluteWrappedLabel(string text, int fontSize, Color color, Rect2 rect)
	{
		var label = UiFactory.CreateWrappedLabel(text, fontSize, color);
		label.Position = rect.Position;
		label.Size = rect.Size;
		label.ClipText = true;
		return label;
	}

	private Button CreateAbsoluteButton(string text, Rect2 rect, bool small = false)
	{
		var button = UiFactory.CreateButton(text, _accentColor);
		button.Position = rect.Position;
		button.Size = rect.Size;
		if (small)
		{
			button.AddThemeFontSizeOverride("font_size", 14);
		}
		return button;
	}

	private Control CreateMiniStat(string title, string value, Rect2 rect, Color accent)
	{
		var panel = CreateAbsolutePanel(rect, _cardColor.Darkened(0.03f));
		panel.AddChild(CreateAbsoluteLabel(title, 12, _mutedTextColor, new Rect2(14, 9, rect.Size.X - 28, 17)));
		panel.AddChild(CreateAbsoluteLabel(value, 17, accent, new Rect2(14, 28, rect.Size.X - 28, 21)));
		return panel;
	}

	private ProgressBar BuildMeter()
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
			BgColor = _cardColor.Darkened(0.16f),
			CornerRadiusTopLeft = 6,
			CornerRadiusTopRight = 6,
			CornerRadiusBottomLeft = 6,
			CornerRadiusBottomRight = 6,
		});
		return bar;
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

	private Control CreateSectionLabel(string text)
	{
		var label = UiFactory.CreateLabel(text, 14, _infoColor);
		label.CustomMinimumSize = new Vector2(490, 24);
		return label;
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

	private static void ClearChildren(Node node)
	{
		foreach (var child in node.GetChildren())
		{
			child.QueueFree();
		}
	}

	private string ProductName(ProductConfig product) => Loc.Content("product", product.Id, "name", product.DisplayName);
	private string CustomerName(CustomerConfig customer) => Loc.Content("customer", customer.Id, "name", customer.DisplayName);
	private string CustomerLine(CustomerConfig customer) => Loc.Content("customer", customer.Id, "line", customer.FirstLine);
	private string DecorName(DecorConfig decor) => Loc.Content("decor", decor.Id, "name", decor.DisplayName);
	private string BlessingName(BlessingConfig blessing) => Loc.Content("blessing", blessing.Id, "name", blessing.DisplayName);

	private string CraftedName(CraftedItem item)
	{
		var product = _database.GetProduct(item.ProductId);
		var baseName = product == null ? item.ProductId : ProductName(product);
		return $"{item.RarityDisplayName} {baseName}";
	}

	private Color RarityColor(CraftedItem item) => item.Rarity switch
	{
		"epic" => new Color(0.86f, 0.62f, 1f),
		"rare" => new Color(0.52f, 0.72f, 1f),
		"fine" => new Color(0.58f, 0.88f, 0.66f),
		_ => _textColor,
	};

	private string BuildCraftedTooltip(CraftedItem item)
	{
		var product = _database.GetProduct(item.ProductId);
		var lines = new List<string>
		{
			CraftedName(item),
			product == null ? string.Empty : $"{Loc.Text("ui.product.slot")}: {GameState.SlotToDisplay(product.Slot)}",
			product == null ? string.Empty : BuildProductStatLine(product),
			BuildCraftedStatLine(item),
		};

		var affixes = BuildCraftedAffixParts(item);
		if (affixes.Count > 0)
		{
			lines.Add(string.Join("  ", affixes));
		}

		if (product != null)
		{
			var buffLine = BuildProductBuffLine(product);
			if (!string.IsNullOrWhiteSpace(buffLine))
			{
				lines.Add(buffLine);
			}
		}

		return string.Join("\n", lines.Where(x => !string.IsNullOrWhiteSpace(x)));
	}

	private string BuildCraftedStatLine(CraftedItem item)
	{
		var product = _database.GetProduct(item.ProductId);
		var parts = new List<string>();
		if (product != null)
		{
			parts.Add(GameState.SlotToDisplay(product.Slot));
		}

		if (item.AttackBonus != 0) parts.Add($"{Loc.Text("ui.stat.attack")} +{item.AttackBonus}");
		if (item.DefenseBonus != 0) parts.Add($"{Loc.Text("ui.stat.defense")} +{item.DefenseBonus}");
		if (item.VitalityBonus != 0) parts.Add($"{Loc.Text("ui.stat.vitality")} +{item.VitalityBonus}");
		if (parts.Count == (product == null ? 0 : 1))
		{
			parts.Add(Loc.Text("ui.item.noRoll"));
		}

		return string.Join("  ", parts);
	}

	private List<string> BuildCraftedAffixParts(CraftedItem item)
	{
		var parts = new List<string>();
		if (MathF.Abs(item.AttackSpeedBonus) > 0.001f) parts.Add($"{Loc.Text("ui.stat.speed")} {-item.AttackSpeedBonus:+0.0;-0.0;0.0}s");
		if (item.ExtraAttackChance > 0.001f) parts.Add($"{Loc.Text("ui.stat.chase")} +{item.ExtraAttackChance * 100f:0}%");
		if (item.CritChance > 0.001f) parts.Add($"{Loc.Text("ui.stat.crit")} +{item.CritChance * 100f:0}%");
		if (item.HealOnHit > 0.001f) parts.Add($"{Loc.Text("ui.stat.leech")} +{item.HealOnHit * 100f:0}%");
		return parts;
	}

	private string BuildProductTooltip(ProductConfig product)
	{
		var lines = new List<string>
		{
			ProductName(product),
			$"{Loc.Text("ui.product.slot")}：{GameState.SlotToDisplay(product.Slot)}",
			BuildProductStatLine(product),
		};

		var affixes = BuildProductAffixParts(product);
		if (affixes.Count > 0)
		{
			lines.Add(string.Join("  ", affixes));
		}

		var buffLine = BuildProductBuffLine(product);
		if (!string.IsNullOrWhiteSpace(buffLine))
		{
			lines.Add(buffLine);
		}

		return string.Join("\n", lines.Where(x => !string.IsNullOrWhiteSpace(x)));
	}

	private string BuildProductStatLine(ProductConfig product)
	{
		var parts = new List<string> { GameState.SlotToDisplay(product.Slot) };
		if (product.Attack != 0) parts.Add($"{Loc.Text("ui.stat.attack")} {(product.Attack > 0 ? "+" : string.Empty)}{product.Attack}");
		if (product.Defense != 0) parts.Add($"{Loc.Text("ui.stat.defense")} {(product.Defense > 0 ? "+" : string.Empty)}{product.Defense}");
		if (product.Vitality != 0) parts.Add($"{Loc.Text("ui.stat.vitality")} {(product.Vitality > 0 ? "+" : string.Empty)}{product.Vitality}");
		return string.Join("  ", parts);
	}

	private List<string> BuildProductAffixParts(ProductConfig product)
	{
		var parts = new List<string>();
		if (MathF.Abs(product.AttackSpeedBonus) > 0.001f)
		{
			var intervalDelta = -product.AttackSpeedBonus;
			parts.Add($"{Loc.Text("ui.stat.speed")} {intervalDelta:+0.0;-0.0;0.0}s");
		}

		if (product.ExtraAttackChance > 0.001f) parts.Add($"{Loc.Text("ui.stat.chase")} {product.ExtraAttackChance * 100f:0}%");
		if (product.CritChance > 0.001f) parts.Add($"{Loc.Text("ui.stat.crit")} {product.CritChance * 100f:0}%");
		if (product.HealOnHit > 0.001f) parts.Add($"{Loc.Text("ui.stat.leech")} {product.HealOnHit * 100f:0}%");
		var buffLine = BuildProductBuffLine(product);
		if (!string.IsNullOrWhiteSpace(buffLine)) parts.Add(buffLine);
		return parts;
	}

	private string BuildProductBuffLine(ProductConfig product)
	{
		if (string.IsNullOrWhiteSpace(product.BuffId) || product.BuffDuration <= 0)
		{
			return string.Empty;
		}

		var name = string.IsNullOrWhiteSpace(product.BuffName) ? ProductName(product) : product.BuffName;
		var buffParts = new List<string>();
		if (product.BuffAttack != 0) buffParts.Add($"{Loc.Text("ui.stat.attack")}+{product.BuffAttack}");
		if (product.BuffDefense != 0) buffParts.Add($"{Loc.Text("ui.stat.defense")}+{product.BuffDefense}");
		if (product.BuffVitality != 0) buffParts.Add($"{Loc.Text("ui.stat.vitality")}+{product.BuffVitality}");
		if (MathF.Abs(product.BuffAttackSpeedBonus) > 0.001f) buffParts.Add($"{Loc.Text("ui.stat.speed")}{-product.BuffAttackSpeedBonus:+0.0;-0.0;0.0}s");
		if (product.BuffExtraAttackChance > 0.001f) buffParts.Add($"{Loc.Text("ui.stat.chase")}+{product.BuffExtraAttackChance * 100f:0}%");
		if (product.BuffCritChance > 0.001f) buffParts.Add($"{Loc.Text("ui.stat.crit")}+{product.BuffCritChance * 100f:0}%");
		if (product.BuffHealOnHit > 0.001f) buffParts.Add($"{Loc.Text("ui.stat.leech")}+{product.BuffHealOnHit * 100f:0}%");
		return Loc.Format("ui.product.buff", name, product.BuffDuration, string.Join("/", buffParts));
	}

	private string BuildHeroBuffLine(BattleHero hero)
	{
		if (hero.ActiveBuffs.Count == 0)
		{
			return Loc.Text("ui.hero.noActiveBuff");
		}

		return Loc.Format(
			"ui.hero.activeBuffs",
			string.Join("  ", hero.ActiveBuffs.Take(3).Select(x => $"{x.DisplayName} {MathF.Ceiling(x.RemainingSeconds):0}s")));
	}

	private string BuildProductCostLine(ProductConfig product)
	{
		var parts = new List<string> { Loc.Format("ui.product.costOnly", product.Cost) };
		parts.AddRange(BuildProductAffixParts(product));
		return string.Join("  ", parts);
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
