using Godot;
using System;
using System.Collections.Generic;
using System.Text;

public partial class BattleScene : Control
{
    private readonly GameData _gameData = new();
    private readonly BattleEngine _battle = new();
    private readonly RunEngine _run = new();

    private VBoxContainer _mainLayout = null!;
    private HBoxContainer _contentSplit = null!;
    private HBoxContainer _topBarLayout = null!;
    private VBoxContainer _sidePanel = null!;
    private VBoxContainer _rightPanel = null!;
    private PanelContainer _topBarPanel = null!;
    private PanelContainer _roomHeaderPanel = null!;
    private PanelContainer _mainPanel = null!;
    private PanelContainer _actionPanel = null!;
    private PanelContainer _logPanel = null!;
    private PanelContainer _intentPanel = null!;
    private PanelContainer _mineBoardFrame = null!;
    private Label _runStatusLabel = null!;
    private Label _hudLayerLabel = null!;
    private Label _hudHpLabel = null!;
    private Label _hudShardsLabel = null!;
    private Label _hudDeckLabel = null!;
    private Label _hudLampLabel = null!;
    private Label _hudFogLabel = null!;
    private Label _hudScoreLabel = null!;
    private Label _hudObjectiveLabel = null!;
    private Label _roomTitleLabel = null!;
    private Label _roomDescriptionLabel = null!;
    private PanelContainer _choicePanel = null!;
    private VBoxContainer _choiceList = null!;
    private PanelContainer _minePanel = null!;
    private Label _mineStatusLabel = null!;
    private Button _mineModeButton = null!;
    private GridContainer _mineGrid = null!;
    private PanelContainer _battlePanel = null!;
    private PanelContainer _playerCombatPanel = null!;
    private PanelContainer _enemyCombatPanel = null!;
    private VBoxContainer _enemyCombatLayout = null!;
    private HBoxContainer _enemyTargetRow = null!;
    private Label _playerCombatLabel = null!;
    private Label _enemyCombatLabel = null!;
    private TextureRect _playerPortrait = null!;
    private TextureRect _enemyPortrait = null!;
    private ProgressBar _playerHpBar = null!;
    private ProgressBar _enemyHpBar = null!;
    private Label _playerBlockLabel = null!;
    private Label _enemyBlockLabel = null!;
    private Label _battleResourceLabel = null!;
    private PanelContainer _statusHelpPanel = null!;
    private Label _statusHelpLabel = null!;
    private Label _intentLabel = null!;
    private HBoxContainer _handBox = null!;
    private Button _endTurnButton = null!;
    private Button _deckButton = null!;
    private PanelContainer _rewardPanel = null!;
    private VBoxContainer _rewardList = null!;
    private Button _continueButton = null!;
    private Button _menuButton = null!;
    private PanelContainer _itemPanel = null!;
    private VBoxContainer _itemList = null!;
    private PanelContainer _endPanel = null!;
    private Label _endTitleLabel = null!;
    private Label _endSummaryLabel = null!;
    private Button _retryButton = null!;
    private Button _endMenuButton = null!;
    private RichTextLabel _logLabel = null!;
    private PanelContainer _debugPanel = null!;
    private Label _debugLabel = null!;
    private Panel _modalOverlay = null!;
    private Panel _loadingOverlay = null!;
    private PanelContainer _deckModalPanel = null!;
    private VBoxContainer _deckModalList = null!;
    private Button _deckModalCloseButton = null!;
    private Label _loadingLabel = null!;
    private PanelContainer _dragHintPanel = null!;
    private Label _dragHintLabel = null!;
    private PanelContainer _mineRevealTipPanel = null!;
    private Label _mineRevealTipLabel = null!;
    private Line2D _dragLine = null!;

    private const float RouteMapWidth = 960f;
    private const float RouteNodeWidth = 126f;
    private const float RouteNodeHeight = 78f;
    private const float RouteLayerGap = 108f;
    private const float RouteMapSidePadding = 136f;
    private const float RouteMapTopPadding = 44f;
    private const float RouteMapBottomPadding = 70f;

    private RunRoom? _activeBattleRoom;
    private RunRoom? _activeMineRoom;
    private RunRoom? _activeCalibrationRoom;
    private bool _returnToMineAfterBattle;
    private bool _mineFlagMode;
    private bool _debugVisible;
    private bool _metaRecorded;
    private bool _loadingActive;
    private bool _tipPlaying;
    private string _selectedItemId = string.Empty;
    private readonly Queue<string> _tipQueue = new();
    private readonly Dictionary<int, Button> _enemyTargetButtons = new();
    private Button? _aimingCardButton;
    private int _aimingCardIndex = -1;
    private Vector2 _dragLineStart;
    private bool _dragTargetsEnemy;
    private Container? _choiceButtonTarget;
    private readonly List<CalibrationNode> _calibrationNodes = new();
    private readonly HashSet<int> _selectedCalibrationNodes = new();
    private Label? _calibrationSummaryLabel;
    private GridContainer? _calibrationGrid;

    public override void _Ready()
    {
        Localization.LoadSettings();

        _mainLayout = GetNode<VBoxContainer>("Root/Margin/MainLayout");
        _topBarPanel = GetNode<PanelContainer>("Root/Margin/MainLayout/TopBar");
        _topBarLayout = GetNode<HBoxContainer>("Root/Margin/MainLayout/TopBar/TopBarLayout");
        _roomHeaderPanel = GetNode<PanelContainer>("Root/Margin/MainLayout/RoomHeader");
        _contentSplit = GetNode<HBoxContainer>("Root/Margin/MainLayout/ContentSplit");
        _mainPanel = GetNode<PanelContainer>("Root/Margin/MainLayout/ContentSplit/MainPanel");
        _sidePanel = GetNode<VBoxContainer>("Root/Margin/MainLayout/ContentSplit/SidePanel");
        _runStatusLabel = GetNode<Label>("Root/Margin/MainLayout/TopBar/TopBarLayout/RunStatusLabel");
        _itemPanel = GetNode<PanelContainer>("Root/Margin/MainLayout/ContentSplit/SidePanel/ItemPanel");
        _itemList = GetNode<VBoxContainer>("Root/Margin/MainLayout/ContentSplit/SidePanel/ItemPanel/ItemList");
        _roomTitleLabel = GetNode<Label>("Root/Margin/MainLayout/RoomHeader/RoomHeaderLayout/RoomTitleLabel");
        _roomDescriptionLabel = GetNode<Label>("Root/Margin/MainLayout/RoomHeader/RoomHeaderLayout/RoomDescriptionLabel");
        _choicePanel = GetNode<PanelContainer>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/ChoicePanel");
        _choiceList = GetNode<VBoxContainer>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/ChoicePanel/ChoiceList");
        _minePanel = GetNode<PanelContainer>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/MinePanel");
        _mineStatusLabel = GetNode<Label>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/MinePanel/MineLayout/MineHeader/MineStatusLabel");
        _mineModeButton = GetNode<Button>("Root/Margin/MainLayout/ContentSplit/SidePanel/ActionPanel/ActionLayout/MineModeButton");
        _mineGrid = GetNode<GridContainer>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/MinePanel/MineLayout/MineBoardFrame/MineBoardCenter/MineGrid");
        _battlePanel = GetNode<PanelContainer>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/BattlePanel");
        _playerCombatPanel = GetNode<PanelContainer>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/BattlePanel/BattleLayout/CombatantRow/PlayerCombatPanel");
        _enemyCombatPanel = GetNode<PanelContainer>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/BattlePanel/BattleLayout/CombatantRow/EnemyCombatPanel");
        _enemyCombatLayout = GetNode<VBoxContainer>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/BattlePanel/BattleLayout/CombatantRow/EnemyCombatPanel/EnemyCombatLayout");
        _playerPortrait = GetNode<TextureRect>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/BattlePanel/BattleLayout/CombatantRow/PlayerCombatPanel/PlayerCombatLayout/PlayerPortrait");
        _enemyPortrait = GetNode<TextureRect>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/BattlePanel/BattleLayout/CombatantRow/EnemyCombatPanel/EnemyCombatLayout/EnemyPortrait");
        _playerCombatLabel = GetNode<Label>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/BattlePanel/BattleLayout/CombatantRow/PlayerCombatPanel/PlayerCombatLayout/PlayerCombatLabel");
        _enemyCombatLabel = GetNode<Label>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/BattlePanel/BattleLayout/CombatantRow/EnemyCombatPanel/EnemyCombatLayout/EnemyCombatLabel");
        _playerHpBar = GetNode<ProgressBar>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/BattlePanel/BattleLayout/CombatantRow/PlayerCombatPanel/PlayerCombatLayout/PlayerHpBar");
        _enemyHpBar = GetNode<ProgressBar>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/BattlePanel/BattleLayout/CombatantRow/EnemyCombatPanel/EnemyCombatLayout/EnemyHpBar");
        _playerBlockLabel = GetNode<Label>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/BattlePanel/BattleLayout/CombatantRow/PlayerCombatPanel/PlayerCombatLayout/PlayerBlockLabel");
        _enemyBlockLabel = GetNode<Label>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/BattlePanel/BattleLayout/CombatantRow/EnemyCombatPanel/EnemyCombatLayout/EnemyBlockLabel");
        _battleResourceLabel = GetNode<Label>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/BattlePanel/BattleLayout/BattleResourceLabel");
        _intentPanel = GetNode<PanelContainer>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/BattlePanel/BattleLayout/IntentPanel");
        _intentLabel = GetNode<Label>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/BattlePanel/BattleLayout/IntentPanel/IntentLabel");
        _handBox = GetNode<HBoxContainer>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/BattlePanel/BattleLayout/HandPanel/HandList");
        _endTurnButton = GetNode<Button>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/BattlePanel/BattleLayout/EndTurnButton");
        _deckButton = GetNode<Button>("Root/Margin/MainLayout/ContentSplit/SidePanel/ActionPanel/ActionLayout/DeckButton");
        _rewardPanel = GetNode<PanelContainer>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/RewardPanel");
        _rewardList = GetNode<VBoxContainer>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/RewardPanel/RewardList");
        _continueButton = GetNode<Button>("Root/Margin/MainLayout/ContentSplit/SidePanel/ActionPanel/ActionLayout/ContinueButton");
        _menuButton = GetNode<Button>("Root/Margin/MainLayout/TopBar/TopBarLayout/MenuButton");
        _actionPanel = GetNode<PanelContainer>("Root/Margin/MainLayout/ContentSplit/SidePanel/ActionPanel");
        _endPanel = GetNode<PanelContainer>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/EndPanel");
        _endTitleLabel = GetNode<Label>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/EndPanel/EndLayout/EndTitleLabel");
        _endSummaryLabel = GetNode<Label>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/EndPanel/EndLayout/EndSummaryLabel");
        _retryButton = GetNode<Button>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/EndPanel/EndLayout/EndButtonRow/RetryButton");
        _endMenuButton = GetNode<Button>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/EndPanel/EndLayout/EndButtonRow/EndMenuButton");
        _mineBoardFrame = GetNode<PanelContainer>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/MinePanel/MineLayout/MineBoardFrame");
        _logPanel = GetNode<PanelContainer>("Root/Margin/MainLayout/ContentSplit/SidePanel/LogPanel");
        _logLabel = GetNode<RichTextLabel>("Root/Margin/MainLayout/ContentSplit/SidePanel/LogPanel/LogText");
        _debugPanel = GetNode<PanelContainer>("Root/Margin/MainLayout/ContentSplit/SidePanel/DebugPanel");
        _debugLabel = GetNode<Label>("Root/Margin/MainLayout/ContentSplit/SidePanel/DebugPanel/DebugLabel");
        BuildRunLayout();
        BuildModalHost();
        BuildBattleStatusHelp();
        BuildEnemyTargetRow();

        AddChild(_gameData);
        AddChild(_battle);
        AddChild(_run);

        _gameData.LoadAll();
        if (GameSession.LoadRequested)
        {
            var saveData = SaveManager.LoadRun();
            if (saveData != null)
            {
                _run.LoadFromSave(_gameData, saveData);
            }
            else
            {
                _run.StartRun(_gameData);
            }

            GameSession.LoadRequested = false;
        }
        else
        {
            _run.StartRun(_gameData);
        }

        _endTurnButton.Pressed += OnEndTurnPressed;
        _deckButton.Pressed += OnDeckPressed;
        _deckModalCloseButton.Pressed += CloseDeckModal;
        _continueButton.Pressed += OnContinuePressed;
        _mineModeButton.Pressed += OnMineModePressed;
        _menuButton.Pressed += OnMenuPressed;
        _retryButton.Pressed += OnRetryPressed;
        _endMenuButton.Pressed += OnEndMenuPressed;
        _playerCombatPanel.GuiInput += OnPlayerCombatGuiInput;
        _continueButton.Text = Localization.T("continue_deeper");
        _deckButton.Text = Localization.Language == Localization.English ? "View Deck" : "查看牌组";
        _menuButton.Text = Localization.T("back_menu");
        ApplyUiStyle();

        ShowNextRoomChoices();
    }

    public override void _Input(InputEvent @event)
    {
        if (_aimingCardIndex >= 0 && @event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Right)
        {
            CancelAimingCard();
            GetViewport().SetInputAsHandled();
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is not InputEventKey keyEvent || !keyEvent.Pressed || keyEvent.Echo || keyEvent.Keycode != Key.F3)
        {
            return;
        }

        _debugVisible = !_debugVisible;
        _debugPanel.Visible = _debugVisible;
        RenderDebug();
        GetViewport().SetInputAsHandled();
    }

    public override void _Process(double delta)
    {
        if (_aimingCardButton == null || _aimingCardIndex < 0 || !GodotObject.IsInstanceValid(_aimingCardButton))
        {
            return;
        }

        UpdateAimingLine(GetGlobalMousePosition());
        UpdateAimingFeedback(GetGlobalMousePosition());
    }

    private void OnContinuePressed()
    {
        if (_battlePanel.Visible || _modalOverlay.Visible || _loadingActive)
        {
            return;
        }

        RunLoadingTransition(
            Localization.Language == Localization.English ? "Plotting the next route..." : "整理下一段路线……",
            ShowNextRoomChoices);
    }

    private void OnMenuPressed()
    {
        SaveManager.SaveRun(_run);
        ChangeSceneWithLoading(
            "res://scenes/MainMenu.tscn",
            Localization.Language == Localization.English ? "Returning to camp..." : "返回营地……");
    }

    private void OnDeckPressed()
    {
        ShowDeckModal();
    }

    private void ShowDeckModal()
    {
        _modalOverlay.Visible = true;
        ClearBox(_deckModalList);

        for (var i = 0; i < _run.PlayerDeck.Count; i++)
        {
            var card = _run.PlayerDeck[i];
            var button = new Button
            {
                Text = $"{i + 1}. {FormatBattleCardText(card)}",
                CustomMinimumSize = new Vector2(0, 112),
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
                Disabled = true
            };
            StyleCardButton(button, card, true);
            _deckModalList.AddChild(button);
        }
    }

    private void CloseDeckModal()
    {
        _modalOverlay.Visible = false;
    }

    private void BuildRunLayout()
    {
        _mainLayout.AddThemeConstantOverride("separation", 10);
        _contentSplit.AddThemeConstantOverride("separation", 12);

        _sidePanel.CustomMinimumSize = new Vector2(286, 0);
        _sidePanel.SizeFlagsVertical = SizeFlags.ExpandFill;
        _contentSplit.MoveChild(_sidePanel, 0);
        _mainPanel.SizeFlagsHorizontal = SizeFlags.ExpandFill;

        _rightPanel = new VBoxContainer
        {
            Name = "RightPanel",
            CustomMinimumSize = new Vector2(372, 0),
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        _rightPanel.AddThemeConstantOverride("separation", 10);
        _contentSplit.AddChild(_rightPanel);

        MoveToPanelColumn(_roomHeaderPanel, _rightPanel);
        MoveToPanelColumn(_debugPanel, _rightPanel);
        MoveToPanelColumn(_logPanel, _rightPanel);
        _logPanel.SizeFlagsVertical = SizeFlags.ExpandFill;
        _debugPanel.SizeFlagsVertical = SizeFlags.ShrinkBegin;
        _roomHeaderPanel.SizeFlagsVertical = SizeFlags.ShrinkBegin;
        _itemPanel.CustomMinimumSize = new Vector2(0, 330);
        _itemList.AddThemeConstantOverride("separation", 8);
        _actionPanel.CustomMinimumSize = new Vector2(0, 180);
        _mineModeButton.CustomMinimumSize = new Vector2(0, 58);
        _deckButton.CustomMinimumSize = new Vector2(0, 58);
        _continueButton.CustomMinimumSize = new Vector2(0, 64);
        _logLabel.CustomMinimumSize = new Vector2(0, 360);
        _roomTitleLabel.AddThemeFontSizeOverride("font_size", 24);

        BuildHudBar();
        BuildInventoryScroll();
    }

    private void BuildInventoryScroll()
    {
        if (_itemList.GetParent() is not Container parent || parent is ScrollContainer)
        {
            return;
        }

        parent.RemoveChild(_itemList);
        var scroll = new ScrollContainer
        {
            Name = "InventoryScroll",
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        _itemList.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        scroll.AddChild(_itemList);
        parent.AddChild(scroll);
    }

    private static void MoveToPanelColumn(Control node, Container target)
    {
        node.GetParent()?.RemoveChild(node);
        target.AddChild(node);
    }

    private void BuildHudBar()
    {
        _topBarPanel.CustomMinimumSize = new Vector2(0, 76);
        _topBarLayout.AddThemeConstantOverride("separation", 8);
        _runStatusLabel.Visible = false;

        _topBarLayout.RemoveChild(_menuButton);
        _hudLayerLabel = AddHudCell("HudLayer", 96);
        _hudHpLabel = AddHudCell("HudHp", 150);
        _hudShardsLabel = AddHudCell("HudShards", 132);
        _hudDeckLabel = AddHudCell("HudDeck", 112);
        _hudLampLabel = AddHudCell("HudLamp", 124);
        _hudFogLabel = AddHudCell("HudFog", 144);
        _hudScoreLabel = AddHudCell("HudScore", 134);
        _hudObjectiveLabel = AddHudCell("HudObjective", 300, true);
        _topBarLayout.AddChild(_menuButton);
        _menuButton.CustomMinimumSize = new Vector2(72, 52);
    }

    private Label AddHudCell(string name, float minWidth, bool expand = false)
    {
        var cell = new PanelContainer
        {
            Name = $"{name}Cell",
            CustomMinimumSize = new Vector2(minWidth, 56),
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        if (expand)
        {
            cell.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        }

        cell.AddThemeStyleboxOverride("panel", MistTheme.PanelStyle(MistPanelVariant.Inset));
        var label = new Label
        {
            Name = name,
            VerticalAlignment = VerticalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        label.AddThemeFontSizeOverride("font_size", 16);
        label.AddThemeColorOverride("font_color", MistTheme.TextMain);
        cell.AddChild(label);
        _topBarLayout.AddChild(cell);
        return label;
    }

    private void BuildEnemyTargetRow()
    {
        _enemyPortrait.Visible = false;
        _enemyHpBar.Visible = false;
        _enemyBlockLabel.Visible = false;

        _enemyTargetRow = new HBoxContainer
        {
            Name = "EnemyTargetRow",
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        _enemyTargetRow.AddThemeConstantOverride("separation", 10);
        _enemyCombatLayout.AddChild(_enemyTargetRow);
    }

    private void BuildBattleStatusHelp()
    {
        _statusHelpPanel = new PanelContainer
        {
            Name = "StatusHelpPanel",
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };

        _statusHelpLabel = new Label
        {
            Name = "StatusHelpLabel",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        _statusHelpLabel.AddThemeFontSizeOverride("font_size", 15);
        _statusHelpPanel.AddChild(_statusHelpLabel);

        var battleLayout = _battleResourceLabel.GetParent();
        battleLayout.AddChild(_statusHelpPanel);
        battleLayout.MoveChild(_statusHelpPanel, _battleResourceLabel.GetIndex() + 1);
    }

    private void OnRetryPressed()
    {
        GameSession.LoadRequested = false;
        ChangeSceneWithLoading(
            "res://scenes/CharacterSelect.tscn",
            Localization.Language == Localization.English ? "Preparing a new expedition..." : "准备新的探索……");
    }

    private void OnEndMenuPressed()
    {
        ChangeSceneWithLoading(
            "res://scenes/MainMenu.tscn",
            Localization.Language == Localization.English ? "Returning to camp..." : "返回营地……");
    }

    private void ShowNextRoomChoices()
    {
        HideInteractivePanels();

        var choices = _run.GetNextRoomChoices();
        if (choices.Count == 0)
        {
            _roomTitleLabel.Text = Localization.T("run_complete");
            _roomDescriptionLabel.Text = Localization.T("run_complete_desc");
            RenderShared();
            return;
        }

        SaveManager.SaveRun(_run);
        _roomTitleLabel.Text = _run.CurrentLayerIndex < 0 ? Localization.T("select_entry") : Localization.T("select_next");
        _roomDescriptionLabel.Text = Localization.T("route_desc");
        _choicePanel.Visible = true;
        RenderRouteMap(choices);

        RenderShared();
        ShowGameTip(Localization.Language == Localization.English
            ? $"Route ready: {choices.Count} reachable paths."
            : $"路线已更新：{choices.Count} 条可进入路线。");
    }

    private void EnterNextRoom(int choiceIndex)
    {
        var room = _run.EnterNextRoom(choiceIndex);
        _activeBattleRoom = null;
        _activeMineRoom = null;
        _activeCalibrationRoom = null;
        _returnToMineAfterBattle = false;
        HideInteractivePanels();

        switch (room.Kind)
        {
            case "mine":
                RenderMineRoom(room);
                break;
            case "event":
                RenderEventRoom(room);
                break;
            case "battle":
            case "elite":
                StartBattleRoom(room);
                break;
            case "treasure":
                RenderTreasureRoom(room);
                break;
            case "rest":
                RenderRestRoom(room);
                break;
            case "shop":
                RenderShopRoom(room);
                break;
            case "calibration":
                RenderCalibrationRoom(room);
                break;
            case "complete":
                ShowEndPanel(true, "抵达矿井深处");
                break;
        }

        RenderShared();
        ShowGameTip(Localization.Language == Localization.English
            ? $"{GetMapNodeIcon(room.Kind)} {room.DisplayTitle()}\n{FormatRouteCost(room)}"
            : $"{GetMapNodeIcon(room.Kind)} {room.DisplayTitle()}\n{FormatRouteCost(room)}");
    }

    private static string GetRoomIcon(string kind)
    {
        return kind switch
        {
            "battle" => Localization.T("room_battle"),
            "elite" => Localization.Language == Localization.English ? "[Elite]" : "[精英]",
            "treasure" => Localization.Language == Localization.English ? "[Cache]" : "[宝藏]",
            "mine" => Localization.T("room_mine"),
            "event" => Localization.T("room_event"),
            "rest" => Localization.T("room_rest"),
            "shop" => Localization.T("room_shop"),
            "calibration" => Localization.Language == Localization.English ? "[Tune]" : "[校准]",
            "complete" => Localization.T("room_complete"),
            _ => "[未知]"
        };
    }

    private string GetRoomSummary(RunRoom room)
    {
        return room.Kind switch
        {
            "battle" => $"{string.Format(Localization.T("encounter_reward"), _gameData.GetEnemy(room.EnemyId).DisplayName())} {FormatThreatPreview(room)}",
            "elite" => $"{(Localization.Language == Localization.English ? "Elite battle. Higher pressure, stronger spoils." : "精英战。压力更高，但战利品更强。")} {FormatThreatPreview(room)}",
            "treasure" => Localization.Language == Localization.English ? "Choose a cache reward: relic, card, or supplies." : "选择一份宝藏补给：遗物、卡牌或工具。",
            "mine" => FormatMineSummary(room.MineConfig),
            "event" => _gameData.GetEvent(room.EventId).DisplayDescription(),
            "rest" => Localization.T("rest_summary"),
            "shop" => Localization.T("shop_summary"),
            "calibration" => Localization.Language == Localization.English
                ? "Tune a lamp array. Choose 3 nodes for a mix of supplies, oil, fog control, cards, and risk."
                : "校准矿灯阵列。选择 3 个节点，组合矿晶、灯油、净雾、卡牌与过载风险。",
            "complete" => Localization.T("complete_summary"),
            _ => Localization.T("unknown_room")
        };
    }

    private void RenderRouteMap(IReadOnlyList<RunRoom> choices)
    {
        ClearBox(_choiceList);

        AddRouteMapHeader(choices);

        var mapFrame = new PanelContainer
        {
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        mapFrame.AddThemeStyleboxOverride("panel", MakePanelStyle("121c28", "40566d", 1));

        var scroll = new ScrollContainer
        {
            SizeFlagsVertical = SizeFlags.ExpandFill,
            CustomMinimumSize = new Vector2(0, 640)
        };

        var mapCanvas = new Control
        {
            CustomMinimumSize = new Vector2(RouteMapWidth, GetRouteMapHeight()),
            SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };

        var availableRoute = BuildAvailableRouteSet();
        AddRouteLayerBands(mapCanvas);
        AddRouteEntryGuide(mapCanvas, availableRoute);
        AddRouteConnections(mapCanvas, availableRoute);
        AddRouteNodes(mapCanvas, choices, availableRoute);

        scroll.AddChild(mapCanvas);
        mapFrame.AddChild(scroll);
        _choiceList.AddChild(mapFrame);
        var scrollTarget = GetRouteScrollTarget();
        GetTree().CreateTimer(0.01).Timeout += () => scroll.ScrollVertical = scrollTarget;
    }

    private void AddRouteMapHeader(IReadOnlyList<RunRoom> choices)
    {
        var header = new HBoxContainer
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        header.AddThemeConstantOverride("separation", 8);

        var title = new Label
        {
            Text = Localization.Language == Localization.English
                ? $"Route Map · choose 1 of {choices.Count}"
                : $"路线图 · 从 {choices.Count} 条可进入路线中选择 1 条",
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            VerticalAlignment = VerticalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        title.AddThemeColorOverride("font_color", Color.FromHtml("f4f0df"));
        title.AddThemeFontSizeOverride("font_size", 18);
        header.AddChild(title);

        AddRouteLegendChip(header, "战", "Battle", Color.FromHtml("b8505d"));
        AddRouteLegendChip(header, "矿", "Survey", Color.FromHtml("5f89b8"));
        AddRouteLegendChip(header, "宝", "Cache", Color.FromHtml("d7b45f"));
        AddRouteLegendChip(header, "?", "Event", Color.FromHtml("9c8ec2"));
        AddRouteLegendChip(header, "校", "Tune", Color.FromHtml("78d69b"));
        _choiceList.AddChild(header);
    }

    private static void AddRouteLegendChip(Container parent, string zh, string en, Color color)
    {
        var chip = new PanelContainer
        {
            CustomMinimumSize = new Vector2(Localization.Language == Localization.English ? 70 : 44, 28)
        };
        chip.AddThemeStyleboxOverride("panel", MakeRouteLegendChipStyle(color));

        var label = new Label
        {
            Text = Localization.Language == Localization.English ? en : zh,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        label.AddThemeColorOverride("font_color", color.Lightened(0.38f));
        label.AddThemeFontSizeOverride("font_size", 13);
        chip.AddChild(label);
        parent.AddChild(chip);
    }

    private void AddRouteLayerBands(Control mapCanvas)
    {
        for (var layerIndex = 0; layerIndex < _run.MapLayers.Count; layerIndex++)
        {
            var y = GetRouteLayerY(layerIndex);
            var band = new ColorRect
            {
                Color = layerIndex == _run.CurrentLayerIndex + 1
                    ? new Color(0.16f, 0.24f, 0.30f, 0.58f)
                    : layerIndex % 2 == 0 ? new Color(0.08f, 0.13f, 0.18f, 0.36f) : new Color(0.06f, 0.10f, 0.15f, 0.28f),
                Position = new Vector2(22, y - 14),
                Size = new Vector2(RouteMapWidth - 44, RouteNodeHeight + 28),
                MouseFilter = MouseFilterEnum.Ignore,
                ZIndex = -2
            };
            mapCanvas.AddChild(band);

            var label = new Label
            {
                Text = FormatRouteLayerTitle(layerIndex),
                Position = new Vector2(18, y + 18),
                Size = new Vector2(106, 42),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                MouseFilter = MouseFilterEnum.Ignore,
                ZIndex = 3
            };
            label.AddThemeColorOverride("font_color", layerIndex == _run.CurrentLayerIndex + 1 ? Color.FromHtml("f4f0df") : Color.FromHtml("71849a"));
            label.AddThemeFontSizeOverride("font_size", 13);
            mapCanvas.AddChild(label);
        }
    }

    private void AddRouteConnections(Control mapCanvas, HashSet<string> availableRoute)
    {
        for (var layerIndex = 0; layerIndex < _run.MapLayers.Count - 1; layerIndex++)
        {
            foreach (var room in _run.MapLayers[layerIndex])
            {
                foreach (var nextNodeId in room.NextNodeIds)
                {
                    var next = FindMapRoom(nextNodeId);
                    if (next == null)
                    {
                        continue;
                    }

                    var active = room.NodeId == _run.CurrentRoomNodeId || _run.IsRoomReachable(room) || availableRoute.Contains(room.NodeId);
                    var forward = availableRoute.Contains(next.NodeId);
                    AddRouteConnectionLine(mapCanvas, room, next, active && forward);
                }
            }
        }
    }

    private void AddRouteEntryGuide(Control mapCanvas, HashSet<string> availableRoute)
    {
        if (_run.MapLayers.Count == 0)
        {
            return;
        }

        var origin = new Vector2(RouteMapWidth / 2f, GetRouteMapHeight() - 24f);
        foreach (var room in _run.MapLayers[0])
        {
            var active = _run.CurrentLayerIndex < 0 && (_run.IsRoomReachable(room) || availableRoute.Contains(room.NodeId));
            AddRouteGuideLine(mapCanvas, origin, GetRouteNodeEdgePoint(room, origin), active);
        }

        var marker = new Label
        {
            Text = Localization.Language == Localization.English ? "Entry" : "入口",
            Position = origin - new Vector2(44, 20),
            Size = new Vector2(88, 28),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            MouseFilter = MouseFilterEnum.Ignore,
            ZIndex = 1
        };
        marker.AddThemeColorOverride("font_color", _run.CurrentLayerIndex < 0 ? Color.FromHtml("f4f0df") : Color.FromHtml("71849a"));
        marker.AddThemeFontSizeOverride("font_size", 14);
        mapCanvas.AddChild(marker);
    }

    private void AddRouteConnectionLine(Control mapCanvas, RunRoom fromRoom, RunRoom toRoom, bool active)
    {
        var fromCenter = GetRouteNodeCenter(fromRoom);
        var toCenter = GetRouteNodeCenter(toRoom);
        AddRouteGuideLine(
            mapCanvas,
            GetRouteNodeEdgePoint(fromRoom, toCenter),
            GetRouteNodeEdgePoint(toRoom, fromCenter),
            active);
    }

    private void AddRouteGuideLine(Control mapCanvas, Vector2 from, Vector2 to, bool active)
    {
        var mid = (from + to) / 2f + new Vector2(0, active ? -10f : -6f);
        var shadow = new Line2D
        {
            Width = active ? 7f : 4f,
            DefaultColor = active ? new Color(0.20f, 0.60f, 0.45f, 0.30f) : new Color(0.06f, 0.10f, 0.14f, 0.52f),
            Antialiased = true,
            ZIndex = -1
        };
        shadow.AddPoint(from);
        shadow.AddPoint(mid);
        shadow.AddPoint(to);
        mapCanvas.AddChild(shadow);

        var line = new Line2D
        {
            Width = active ? 3.5f : 2f,
            DefaultColor = GetRouteLineColor(active),
            Antialiased = true,
            ZIndex = 0
        };
        line.AddPoint(from);
        line.AddPoint(mid);
        line.AddPoint(to);
        mapCanvas.AddChild(line);
    }

    private void AddRouteNodes(Control mapCanvas, IReadOnlyList<RunRoom> choices, HashSet<string> availableRoute)
    {
        for (var layerIndex = 0; layerIndex < _run.MapLayers.Count; layerIndex++)
        {
            foreach (var room in _run.MapLayers[layerIndex])
            {
                var selectable = _run.IsRoomReachable(room);
                var current = room.NodeId == _run.CurrentRoomNodeId;
                var past = room.LayerIndex < _run.CurrentLayerIndex || (room.LayerIndex == _run.CurrentLayerIndex && !current);
                var onRoute = current || selectable || availableRoute.Contains(room.NodeId);
                var button = new Button
                {
                    Text = FormatRouteNodeText(room, current, selectable, past, onRoute),
                    Position = GetRouteNodePosition(room),
                    Size = new Vector2(RouteNodeWidth, RouteNodeHeight),
                    AutowrapMode = TextServer.AutowrapMode.WordSmart,
                    TooltipText = BuildRouteNodeTooltip(room, current, selectable, past, onRoute),
                    Disabled = !selectable,
                    ZIndex = 2
                };
                button.AddThemeFontSizeOverride("font_size", 13);
                StyleRouteNodeButton(button, room, current, selectable, past, onRoute);
                if (selectable)
                {
                    var captured = FindChoiceIndex(choices, room.NodeId);
                    var loadingText = BuildRoomLoadingText(room);
                    button.Pressed += () => RunLoadingTransition(loadingText, () => EnterNextRoom(captured));
                }

                mapCanvas.AddChild(button);
            }
        }
    }

    private float GetRouteMapHeight()
    {
        return RouteMapTopPadding + RouteMapBottomPadding + RouteNodeHeight + Math.Max(0, _run.MapLayers.Count - 1) * RouteLayerGap;
    }

    private int GetRouteScrollTarget()
    {
        var focusLayer = Math.Clamp(_run.CurrentLayerIndex < 0 ? 0 : _run.CurrentLayerIndex, 0, Math.Max(0, _run.MapLayers.Count - 1));
        return Math.Max(0, (int)(GetRouteLayerY(focusLayer) - 360f));
    }

    private float GetRouteLayerY(int layerIndex)
    {
        return GetRouteMapHeight() - RouteMapBottomPadding - RouteNodeHeight - layerIndex * RouteLayerGap;
    }

    private Vector2 GetRouteNodePosition(RunRoom room)
    {
        var layer = _run.MapLayers[room.LayerIndex];
        var usableWidth = RouteMapWidth - RouteMapSidePadding * 2f;
        var x = layer.Count <= 1
            ? RouteMapWidth / 2f
            : RouteMapSidePadding + room.ColumnIndex * (usableWidth / Math.Max(1, layer.Count - 1));
        return new Vector2(x - RouteNodeWidth / 2f, GetRouteLayerY(room.LayerIndex));
    }

    private Vector2 GetRouteNodeCenter(RunRoom room)
    {
        return GetRouteNodePosition(room) + new Vector2(RouteNodeWidth / 2f, RouteNodeHeight / 2f);
    }

    private Vector2 GetRouteNodeEdgePoint(RunRoom room, Vector2 toward)
    {
        var center = GetRouteNodeCenter(room);
        var delta = toward - center;
        if (delta.LengthSquared() <= 0.001f)
        {
            return center;
        }

        var xScale = Math.Abs(delta.X) < 0.001f ? float.PositiveInfinity : RouteNodeWidth / 2f / Math.Abs(delta.X);
        var yScale = Math.Abs(delta.Y) < 0.001f ? float.PositiveInfinity : RouteNodeHeight / 2f / Math.Abs(delta.Y);
        return center + delta * Math.Min(xScale, yScale);
    }

    private static Color GetRouteLineColor(bool active)
    {
        return active ? Color.FromHtml("8df0bd") : new Color(0.44f, 0.55f, 0.67f, 0.64f);
    }

    private HashSet<string> BuildAvailableRouteSet()
    {
        var result = new HashSet<string>();
        var queue = new Queue<RunRoom>();
        foreach (var choice in _run.GetNextRoomChoices())
        {
            queue.Enqueue(choice);
        }

        while (queue.Count > 0)
        {
            var room = queue.Dequeue();
            if (!result.Add(room.NodeId))
            {
                continue;
            }

            foreach (var nextNodeId in room.NextNodeIds)
            {
                var next = FindMapRoom(nextNodeId);
                if (next != null)
                {
                    queue.Enqueue(next);
                }
            }
        }

        return result;
    }

    private RunRoom? FindMapRoom(string nodeId)
    {
        foreach (var layer in _run.MapLayers)
        {
            foreach (var room in layer)
            {
                if (room.NodeId == nodeId)
                {
                    return room;
                }
            }
        }

        return null;
    }

    private string FormatRouteLayerTitle(int layerIndex)
    {
        var title = layerIndex == 0 ? Localization.T("route_entry_layer") : string.Format(Localization.T("route_layer"), layerIndex + 1);
        if (layerIndex == _run.CurrentLayerIndex)
        {
            return $"{title} · {Localization.T("route_current")}";
        }

        if (layerIndex == _run.CurrentLayerIndex + 1)
        {
            return $"{title} · {Localization.T("route_available")}";
        }

        return title;
    }

    private string FormatRouteNodeText(RunRoom room, bool current, bool selectable, bool past, bool onRoute)
    {
        var marker = current ? "●" : selectable ? "◆" : past || !onRoute ? "·" : "◇";
        var title = TrimRouteTitle(room.DisplayTitle(), Localization.Language == Localization.English ? 13 : 8);
        return $"{marker} {GetMapNodeIcon(room.Kind)}\n{title}\n{FormatCompactRouteCost(room)}";
    }

    private string BuildRouteNodeTooltip(RunRoom room, bool current, bool selectable, bool past, bool onRoute)
    {
        var state = GetRouteNodeStateText(current, selectable, past, onRoute);
        return $"{room.DisplayTitle()}\n{state}\n{FormatRouteCost(room)}\n{FormatRouteNodeSummary(room)}\n{FormatRouteNext(room)}";
    }

    private static string GetRouteNodeStateText(bool current, bool selectable, bool past, bool onRoute)
    {
        return current
            ? Localization.T("route_current")
            : selectable
                ? Localization.T("route_available")
                : past || !onRoute
                    ? Localization.T("route_lost")
                    : Localization.T("route_future");
    }

    private static string TrimRouteTitle(string title, int maxChars)
    {
        if (title.Length <= maxChars)
        {
            return title;
        }

        return $"{title[..Math.Max(1, maxChars - 1)]}…";
    }

    private static string FormatCompactRouteCost(RunRoom room)
    {
        var bonus = room.RewardBonus > 0 ? $" +{room.RewardBonus}%" : string.Empty;
        return Localization.Language == Localization.English
            ? $"R{room.Risk} / O-{room.LampCost}{bonus}"
            : $"险{room.Risk} / 油-{room.LampCost}{bonus}";
    }

    private string FormatRouteNodeSummary(RunRoom room)
    {
        return room.Kind switch
        {
            "battle" => _gameData.GetEnemy(room.EnemyId).DisplayName(),
            "elite" => Localization.Language == Localization.English ? $"Elite: {_gameData.GetEnemy(room.EnemyId).DisplayName()}" : $"精英：{_gameData.GetEnemy(room.EnemyId).DisplayName()}",
            "treasure" => Localization.Language == Localization.English ? "Relic / card / supplies" : "遗物 / 卡牌 / 补给",
            "mine" => $"{room.MineConfig.Width}x{room.MineConfig.Height} D{room.MineConfig.Monsters + room.MineConfig.Traps}/R{room.MineConfig.Treasures + room.MineConfig.Ores}",
            "event" => Localization.Language == Localization.English ? "Choice event" : "事件抉择",
            "rest" => Localization.Language == Localization.English ? "Recover / forge" : "恢复 / 锻造",
            "shop" => Localization.Language == Localization.English ? "Cards / healing" : "卡牌 / 治疗",
            "calibration" => Localization.Language == Localization.English ? "Pick 3 lamp nodes" : "选择 3 个灯阵节点",
            "complete" => Localization.T("route_endpoint"),
            _ => Localization.T("unknown_room")
        };
    }

    private static string FormatRouteCost(RunRoom room)
    {
        var bonus = room.RewardBonus > 0 ? $" +{room.RewardBonus}%" : string.Empty;
        return Localization.Language == Localization.English
            ? $"R{room.Risk}  Oil -{room.LampCost}{bonus}"
            : $"险{room.Risk}  灯油 -{room.LampCost}{bonus}";
    }

    private string FormatRouteNext(RunRoom room)
    {
        if (room.NextNodeIds.Count == 0)
        {
            return $"{Localization.T("route_next")}: {Localization.T("route_endpoint")}";
        }

        var icons = new List<string>();
        foreach (var nextNodeId in room.NextNodeIds)
        {
            var next = FindMapRoom(nextNodeId);
            if (next != null)
            {
                icons.Add(GetMapNodeIcon(next.Kind));
            }
        }

        return $"{Localization.T("route_next")}: {string.Join(" ", icons)}";
    }

    private static int FindChoiceIndex(IReadOnlyList<RunRoom> choices, string nodeId)
    {
        for (var i = 0; i < choices.Count; i++)
        {
            if (choices[i].NodeId == nodeId)
            {
                return i;
            }
        }

        return 0;
    }

    private static string GetMapNodeIcon(string kind)
    {
        if (Localization.Language == Localization.English)
        {
            return kind switch
            {
                "battle" => "B",
                "elite" => "E",
                "treasure" => "$",
                "mine" => "M",
                "event" => "?",
                "rest" => "R",
                "shop" => "S",
                "calibration" => "T",
                "complete" => "END",
                _ => "?"
            };
        }

        return kind switch
        {
            "battle" => "战",
            "mine" => "矿",
            "elite" => "精",
            "treasure" => "宝",
            "event" => "?",
            "rest" => "休",
            "shop" => "商",
            "calibration" => "校",
            "complete" => "终",
            _ => "?"
        };
    }

    private static string BuildRoomLoadingText(RunRoom room)
    {
        if (Localization.Language == Localization.English)
        {
            return room.Kind switch
            {
                "mine" => "Lighting the survey grid...",
                "battle" or "elite" => "Entering combat...",
                "treasure" => "Opening the cache...",
                "event" => "Reading the tunnel signs...",
                "rest" => "Securing a quiet stop...",
                "shop" => "Finding the lamp caravan...",
                "calibration" => "Warming the lamp array...",
                "complete" => "Crossing the final gate...",
                _ => "Moving deeper..."
            };
        }

        return room.Kind switch
        {
            "mine" => "点亮勘探网格……",
            "battle" or "elite" => "进入战斗……",
            "treasure" => "开启补给箱……",
            "event" => "辨认支道痕迹……",
            "rest" => "抵达安全歇脚点……",
            "shop" => "寻找矿灯商队……",
            "calibration" => "预热矿灯阵列……",
            "complete" => "穿过最终矿门……",
            _ => "继续深入……"
        };
    }

    private string FormatThreatPreview(RunRoom room)
    {
        var threat = CalculateThreatLevel(room, false);
        var fogCards = GetBattleFogCardCount();
        return Localization.Language == Localization.English
            ? $"Threat {threat}. Fog cards: {fogCards}."
            : $"威胁 {threat}。浓雾卡 {fogCards}。";
    }

    private static string FormatRoomRisk(RunRoom room)
    {
        var risk = room.Risk switch
        {
            1 => Localization.Language == Localization.English ? "Safe" : "稳妥",
            2 => Localization.Language == Localization.English ? "Risk" : "风险",
            3 => Localization.Language == Localization.English ? "High Risk" : "高危",
            _ => Localization.Language == Localization.English ? "Critical" : "极危"
        };
        var bonus = room.RewardBonus > 0 ? $" +{room.RewardBonus}% reward" : string.Empty;
        if (Localization.Language != Localization.English)
        {
            bonus = room.RewardBonus > 0 ? $" +{room.RewardBonus}%奖励" : string.Empty;
        }

        return $"[{risk} | 灯油-{room.LampCost}{bonus}]";
    }

    private static string FormatMineSummary(MineRoomConfig config)
    {
        if (Localization.Language == Localization.English)
        {
            return $"{config.Width}x{config.Height} survey board. Threats: {config.Monsters + config.Traps}; rewards: {config.Treasures + config.Ores}; clear reward: {config.ClearReward} shards.";
        }

        return $"{config.Width}x{config.Height} 探勘棋盘。危险：{config.Monsters + config.Traps}；资源：{config.Treasures + config.Ores}；清理奖励：{config.ClearReward} 矿晶。";
    }

    private static string FormatMineIntro(MineRoomConfig config)
    {
        if (Localization.Language == Localization.English)
        {
            return $"Survey started. Numbers show nearby danger and reward clues. Board {config.Width}x{config.Height}, monsters {config.Monsters}, traps {config.Traps}, resources {config.Treasures + config.Ores}.";
        }

        return $"扫雷探勘开始。数字同时提示周围危险与资源。棋盘 {config.Width}x{config.Height}，怪物 {config.Monsters}，陷阱 {config.Traps}，资源 {config.Treasures + config.Ores}。";
    }

    private void RenderMineRoom(RunRoom room)
    {
        _activeMineRoom = room;
        _run.StartMinefield(_gameData);
        _mineFlagMode = false;
        _roomTitleLabel.Text = room.DisplayTitle();
        _roomDescriptionLabel.Text = FormatMineIntro(room.MineConfig);
        _minePanel.Visible = true;
        RenderMinefield();
        ShowGameTip(Localization.Language == Localization.English
            ? $"Survey started\n{room.MineConfig.Width}x{room.MineConfig.Height} grid, watch danger clues."
            : $"开始勘探\n{room.MineConfig.Width}x{room.MineConfig.Height} 网格，留意危险线索。");
    }

    private void OnMineModePressed()
    {
        _mineFlagMode = !_mineFlagMode;
        RenderMinefield();
    }

    private void OnMineCellPressed(int index)
    {
        if (_run.Minefield == null || _run.Minefield.IsCleared || _run.PlayerHp <= 0)
        {
            return;
        }

        var revealResult = MineRevealResult.NoChange;
        var tipPosition = GetMineCellTipPosition(index);
        var beforeHp = _run.PlayerHp;
        var beforeShards = _run.Shards;
        var beforeFog = _run.FogPressure;
        if (_mineFlagMode)
        {
            _run.ToggleMineFlag(index);
        }
        else if (!string.IsNullOrEmpty(_selectedItemId))
        {
            var item = _gameData.GetItem(_selectedItemId);
            _run.UseTileItem(item, index);
            _selectedItemId = string.Empty;
        }
        else
        {
            revealResult = _run.RevealMineCell(index, _gameData);
        }

        if (revealResult != MineRevealResult.NoChange)
        {
            ShowMineRevealTip(revealResult, index, tipPosition, beforeHp, beforeShards, beforeFog);
        }

        if (_run.PlayerHp <= 0)
        {
            ShowEndPanel(false, "暗雷耗尽了最后的灯油和体力。");
        }
        else if (_run.Minefield.IsCleared)
        {
            _continueButton.Visible = true;
        }
        else if (_run.Minefield.ExitFound)
        {
            _continueButton.Visible = true;
        }

        if (revealResult == MineRevealResult.Monster)
        {
            RunLoadingTransition(
                Localization.Language == Localization.English ? "A monster breaks from the dark..." : "暗处的怪物扑出……",
                StartMineMonsterBattle);
            RenderMinefield();
            RenderShared();
            return;
        }

        RenderMinefield();
        RenderShared();
    }

    private void StartMineMonsterBattle()
    {
        _continueButton.Visible = false;
        var enemyId = string.IsNullOrEmpty(_activeMineRoom?.EnemyId) ? "slime" : _activeMineRoom.EnemyId;
        var enemies = BuildEncounterEnemies(_activeMineRoom, true, enemyId);
        _battle.StartBattle(BuildBattleDeck(), enemies, _run.PlayerMaxHp, _run.PlayerHp, CalculateThreatLevel(_activeMineRoom, true));
        ApplyBattleStartRelics();
        SetCombatPortraits();
        _returnToMineAfterBattle = true;

        _minePanel.Visible = false;
        _battlePanel.Visible = true;
        _roomTitleLabel.Text = $"{Localization.T("room_battle")} {FormatEncounterNames(enemies)}";
        _roomDescriptionLabel.Text = $"矿格里的怪物拦住了去路。威胁等级 {_battle.ThreatLevel}，击退它后可以回到当前矿区继续探索。";
        RenderBattle();
    }

    private Vector2 GetMineCellTipPosition(int index)
    {
        if (index >= 0 && index < _mineGrid.GetChildCount() && _mineGrid.GetChild(index) is Control cell)
        {
            return cell.GlobalPosition + cell.Size / 2f;
        }

        return _mineBoardFrame.GlobalPosition + _mineBoardFrame.Size / 2f;
    }

    private void ShowMineRevealTip(MineRevealResult result, int index, Vector2 globalPosition, int beforeHp, int beforeShards, int beforeFog)
    {
        ShowGameTip(BuildMineRevealTipText(result, index, beforeHp, beforeShards, beforeFog));
    }

    private string BuildMineRevealTipText(MineRevealResult result, int index, int beforeHp, int beforeShards, int beforeFog)
    {
        var title = result switch
        {
            MineRevealResult.Monster => Localization.Language == Localization.English ? "Monster!" : "惊动怪物！",
            MineRevealResult.Trap => Localization.Language == Localization.English ? "Trap Triggered" : "触发陷阱",
            MineRevealResult.Treasure => Localization.Language == Localization.English ? "Cache Opened" : "打开宝箱",
            MineRevealResult.Ore => Localization.Language == Localization.English ? "Ore Collected" : "采集矿石",
            MineRevealResult.Exit => Localization.Language == Localization.English ? "Exit Found" : "发现出口",
            MineRevealResult.Cleared => Localization.Language == Localization.English ? "Survey Cleared" : "矿区清理完成",
            _ => Localization.Language == Localization.English ? "Tile Revealed" : "翻开矿格"
        };

        var details = new List<string>();
        var cell = _run.Minefield != null && index >= 0 && index < _run.Minefield.Cells.Count ? _run.Minefield.Cells[index] : null;
        if (cell != null && result is MineRevealResult.Revealed or MineRevealResult.Cleared)
        {
            details.Add(Localization.Language == Localization.English
                ? $"Clues: danger {cell.DangerClue}, resource {cell.RewardClue}."
                : $"线索：危险 {cell.DangerClue}，资源 {cell.RewardClue}。");
        }

        var hpDelta = _run.PlayerHp - beforeHp;
        var shardDelta = _run.Shards - beforeShards;
        var fogDelta = _run.FogPressure - beforeFog;
        if (hpDelta != 0)
        {
            details.Add($"HP {FormatSigned(hpDelta)}");
        }
        if (shardDelta != 0)
        {
            details.Add(Localization.Language == Localization.English ? $"Shards {FormatSigned(shardDelta)}" : $"矿晶 {FormatSigned(shardDelta)}");
        }
        if (fogDelta != 0)
        {
            details.Add(Localization.Language == Localization.English ? $"Fog {FormatSigned(fogDelta)}" : $"雾压 {FormatSigned(fogDelta)}");
        }
        if (_run.Minefield?.IsCleared == true && result != MineRevealResult.Cleared)
        {
            details.Add(Localization.Language == Localization.English ? "Board cleared bonus gained." : "已获得清理奖励。");
        }

        return details.Count == 0 ? title : $"{title}\n{string.Join("  ", details)}";
    }

    private static string FormatSigned(int value)
    {
        return value > 0 ? $"+{value}" : value.ToString();
    }

    private void RenderMinefield()
    {
        var minefield = _run.Minefield;
        if (minefield == null)
        {
            return;
        }

        _mineModeButton.Text = _mineFlagMode ? Localization.T("mine_mode_flag") : Localization.T("mine_mode_reveal");
        _mineGrid.Columns = minefield.Width;

        var revealedSafe = 0;
        var flags = 0;
        foreach (var cell in minefield.Cells)
        {
            if (cell.IsRevealed && !cell.IsDanger && cell.Type != MineTileType.Exit)
            {
                revealedSafe++;
            }
            if (cell.IsFlagged)
            {
                flags++;
            }
        }

        var safeTotal = minefield.Cells.Count - minefield.DangerCount - 1;
        _mineStatusLabel.Text = string.Format(Localization.T("mine_status"), revealedSafe, safeTotal, flags, minefield.DangerCount, minefield.TrapDamage, minefield.RewardShards);

        ClearBox(_mineGrid);
        for (var i = 0; i < minefield.Cells.Count; i++)
        {
            var cell = minefield.Cells[i];
            var text = "?";
            if (cell.IsFlagged && !cell.IsRevealed)
            {
                text = "F";
            }
            else if (cell.IsPreviewed && !cell.IsRevealed)
            {
                text = GetPreviewText(cell);
            }
            else if (cell.IsRevealed && cell.Type == MineTileType.Entrance)
            {
                text = "IN";
            }
            else if (cell.IsRevealed && cell.Type == MineTileType.Exit)
            {
                text = "OUT";
            }
            else if (cell.IsRevealed && cell.Type == MineTileType.Monster)
            {
                text = "M";
            }
            else if (cell.IsRevealed && cell.Type == MineTileType.Trap)
            {
                text = "T";
            }
            else if (cell.IsRevealed && cell.Type == MineTileType.Treasure)
            {
                text = "BOX";
            }
            else if (cell.IsRevealed && cell.Type == MineTileType.Ore)
            {
                text = "ORE";
            }
            else if (cell.IsRevealed)
            {
                text = cell.DangerClue == 0 && cell.RewardClue == 0 ? " " : $"D{cell.DangerClue}\nR{cell.RewardClue}";
            }

            var button = new Button
            {
                Text = text,
                CustomMinimumSize = new Vector2(72, 72),
                Disabled = minefield.IsCleared || cell.IsRevealed
            };
            StyleMineButton(button, cell);
            var captured = i;
            button.Pressed += () => OnMineCellPressed(captured);
            button.GuiInput += inputEvent => OnMineCellGuiInput(inputEvent, captured);
            _mineGrid.AddChild(button);
        }
    }

    private void OnMineCellGuiInput(InputEvent inputEvent, int index)
    {
        if (inputEvent is not InputEventMouseButton mouse || !mouse.Pressed || mouse.ButtonIndex != MouseButton.Right)
        {
            return;
        }

        _run.ToggleMineFlag(index);
        RenderMinefield();
        RenderShared();
    }

    private void RenderEventRoom(RunRoom room)
    {
        var mineEvent = _gameData.GetEvent(room.EventId);
        _roomTitleLabel.Text = mineEvent.DisplayTitle();
        _roomDescriptionLabel.Text = mineEvent.DisplayDescription();
        _choicePanel.Visible = true;

        ClearBox(_choiceList);
        foreach (var choice in mineEvent.Choices)
        {
            var button = new Button
            {
                Text = choice.DisplayText(),
                CustomMinimumSize = new Vector2(0, 56),
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
            StyleButton(button, Color.FromHtml("263445"), Color.FromHtml("d8e2ee"));
            button.Pressed += () => OnEventChoicePressed(choice);
            _choiceList.AddChild(button);
        }
    }

    private void OnEventChoicePressed(EventChoiceData choice)
    {
        var before = CaptureRunTipSnapshot();
        _run.ApplyEventChoice(choice, _gameData);
        _choicePanel.Visible = false;
        _continueButton.Visible = true;
        RenderShared();
        ShowGameTip(Localization.Language == Localization.English ? "Choice resolved." : "抉择已生效。");
        ShowRunDeltaTips(before);
    }

    private void RenderTreasureRoom(RunRoom room)
    {
        _roomTitleLabel.Text = room.DisplayTitle();
        _roomDescriptionLabel.Text = Localization.Language == Localization.English
            ? "An old locked cache sits between two rails. Take one package and move on."
            : "旧锁箱卡在两条铁轨之间。选择一份补给，然后继续深入。";
        _choicePanel.Visible = true;

        var reward = _gameData.GetReward(string.IsNullOrWhiteSpace(room.RewardId) ? "cache" : room.RewardId);
        var before = CaptureRunTipSnapshot();
        _run.GainShards(reward.Shards, Localization.Language == Localization.English ? "Cache" : "宝藏");
        ShowRunDeltaTips(before);

        var relicChoices = _gameData.BuildRelicChoices(reward, _run.Relics, HashCode.Combine(_run.RunSeed, _run.CurrentLayerIndex, room.NodeId, "cache_relic"), 2);
        foreach (var relic in relicChoices)
        {
            AddChoiceButton($"{(Localization.Language == Localization.English ? "Take relic" : "带走遗物")}：{relic.DisplayName()}\n{relic.DisplayDescription()}", () => OnTreasureRelicPicked(relic));
        }

        var cardChoices = _gameData.BuildRewardChoices(reward, _run.CharacterId, HashCode.Combine(_run.RunSeed, _run.CurrentLayerIndex, room.NodeId, "cache_card"), 2);
        foreach (var card in cardChoices)
        {
            AddChoiceButton($"{(Localization.Language == Localization.English ? "Take card" : "记录卡牌")}：{FormatCardHeader(card)}\n{card.DisplayDescription()}", () => OnTreasureCardPicked(card));
        }

        AddChoiceButton(Localization.Language == Localization.English
            ? "Take survey supplies\nGain Probe Lamp x2, Bandage x1, and reduce fog pressure by 1."
            : "拿走勘探补给\n获得探测灯 x2、绷带 x1，并降低 1 点雾压。",
            OnTreasureSupplyPicked);
    }

    private void OnTreasureRelicPicked(RelicData relic)
    {
        var before = CaptureRunTipSnapshot();
        _run.AddRelic(relic);
        ShowGameTip(Localization.Language == Localization.English
            ? $"Relic acquired\n{relic.DisplayName()}: {relic.DisplayDescription()}"
            : $"获得遗物\n{relic.DisplayName()}：{relic.DisplayDescription()}");
        ShowRunDeltaTips(before);
        FinishTreasureRoom();
    }

    private void OnTreasureCardPicked(CardData card)
    {
        var before = CaptureRunTipSnapshot();
        _run.AddCardReward(card, Localization.Language == Localization.English ? "Cache" : "宝藏");
        ShowGameTip(Localization.Language == Localization.English
            ? $"Card recorded\n{card.DisplayName()}: {card.DisplayDescription()}"
            : $"记录卡牌\n{card.DisplayName()}：{card.DisplayDescription()}");
        ShowRunDeltaTips(before);
        FinishTreasureRoom();
    }

    private void OnTreasureSupplyPicked()
    {
        var before = CaptureRunTipSnapshot();
        _run.AddItemReward(_gameData.GetItem("probe_lamp"), 2, Localization.Language == Localization.English ? "Cache" : "宝藏");
        _run.AddItemReward(_gameData.GetItem("bandage"), 1, Localization.Language == Localization.English ? "Cache" : "宝藏");
        _run.ReduceFogPressure(1);
        ShowGameTip(Localization.Language == Localization.English ? "Supplies packed." : "补给已装入背包。");
        ShowRunDeltaTips(before);
        FinishTreasureRoom();
    }

    private void FinishTreasureRoom()
    {
        _choicePanel.Visible = false;
        _continueButton.Visible = true;
        RenderShared();
    }

    private void RenderRestRoom(RunRoom room)
    {
        _roomTitleLabel.Text = room.DisplayTitle();
        _roomDescriptionLabel.Text = "矿灯在锈蚀铁轨旁轻轻摇晃，你终于有片刻能把呼吸放慢。";
        _choicePanel.Visible = true;

        AddChoiceButton("休息\n恢复一大段生命。", () => OnRestChoicePressed("heal"));
        AddChoiceButton(Localization.Language == Localization.English ? "Calibrate lamp\nRestore 25 oil and reduce fog pressure by 3." : "校准矿灯\n恢复 25 灯油，并降低 3 点雾压。", () => OnRestChoicePressed("calibrate"));
        AddChoiceButton("锻造一张牌\n免费升级牌组中一张可升级卡牌。", () => RunLoadingTransition(
            Localization.Language == Localization.English ? "Opening the forge..." : "展开锻造台……",
            () => RenderDeckUpgradeChoices(0)));
        AddChoiceButton("整备\n获得 12 矿晶和遗物：校准矿灯。", () => OnRestChoicePressed("forge"));
    }

    private void OnRestChoicePressed(string mode)
    {
        var before = CaptureRunTipSnapshot();
        _run.Rest(mode, _gameData);
        _choicePanel.Visible = false;
        _continueButton.Visible = true;
        RenderShared();
        ShowGameTip(mode switch
        {
            "heal" => Localization.Language == Localization.English ? "Rest complete." : "休息完成。",
            "calibrate" => Localization.Language == Localization.English ? "Lamp calibrated." : "矿灯已校准。",
            "forge" => Localization.Language == Localization.English ? "Field kit prepared." : "整备完成。",
            _ => Localization.Language == Localization.English ? "Camp action complete." : "营地行动完成。"
        });
        ShowRunDeltaTips(before);
    }

    private void RenderShopRoom(RunRoom room)
    {
        _roomTitleLabel.Text = room.DisplayTitle();
        _roomDescriptionLabel.Text = "一盏挂满铜铃的矿灯在轨道旁摇晃。商队愿意收矿晶，也愿意卖一点生路。";
        _choicePanel.Visible = true;

        var shopGrid = CreateShopGrid();
        _choiceList.AddChild(shopGrid);
        _choiceButtonTarget = shopGrid;

        var reward = _gameData.GetReward(room.RewardId);
        var shopCards = _gameData.BuildRewardChoices(reward, _run.CharacterId, HashCode.Combine(_run.RunSeed, _run.CurrentLayerIndex, "shop"), 4);
        foreach (var card in shopCards)
        {
            AddShopChoiceButton(shopGrid, $"{string.Format(Localization.T("buy_card"), card.DisplayName(), 22)}\n{card.DisplayDescription()}", () => OnBuyCardPressed(card), "card");
        }

        var shopRelics = _gameData.BuildRelicChoices(reward, _run.Relics, HashCode.Combine(_run.RunSeed, _run.CurrentLayerIndex, "shop_relic"), 2);
        foreach (var relic in shopRelics)
        {
            AddChoiceButton($"{(Localization.Language == Localization.English ? "Buy relic" : "购买遗物")} {relic.DisplayName()} - 34 矿晶\n{relic.DisplayDescription()}", () => OnBuyRelicPressed(relic));
        }

        AddChoiceButton("购买治疗 - 16 矿晶\n恢复 18 点生命。", OnBuyHealPressed);
        AddChoiceButton(Localization.Language == Localization.English ? "Buy Probe Lamp x2 - 14 Shards\nPreview hidden mine tiles before committing." : "购买探测灯 x2 - 14 矿晶\n在翻开矿格前先预览隐藏内容。", () => OnBuyItemPressed("probe_lamp", 2, 14));
        AddChoiceButton(Localization.Language == Localization.English ? "Buy Smoke Marker - 12 Shards\nPreview a tile; dangerous tiles are flagged automatically." : "购买烟雾标记器 - 12 矿晶\n预览 1 个矿格；若为危险格会自动标记。", () => OnBuyItemPressed("smoke_marker", 1, 12));
        AddChoiceButton("升级一张牌 - 18 矿晶\n让已有卡牌变成强化版本。", () => RunLoadingTransition(
            Localization.Language == Localization.English ? "Opening the forge..." : "展开锻造台……",
            () => RenderDeckUpgradeChoices(18)));
        AddChoiceButton("移除一张牌 - 20 矿晶\n精简牌组，提高关键牌上手率。", () => RunLoadingTransition(
            Localization.Language == Localization.English ? "Sorting the deck..." : "整理牌组……",
            () => RenderDeckRemoveChoices(20)));
        AddChoiceButton("离开商队\n保留矿晶继续深入。", OnLeaveShopPressed);
        _choiceButtonTarget = null;
    }

    private void OnBuyCardPressed(CardData card)
    {
        var before = CaptureRunTipSnapshot();
        if (_run.BuyCard(card, 22))
        {
            ShowGameTip(Localization.Language == Localization.English
                ? $"Purchased card\n{card.DisplayName()}"
                : $"购买卡牌\n{card.DisplayName()}");
            ShowRunDeltaTips(before);
            _choicePanel.Visible = false;
            _continueButton.Visible = true;
        }

        RenderShared();
    }

    private void OnBuyRelicPressed(RelicData relic)
    {
        var before = CaptureRunTipSnapshot();
        if (_run.BuyRelic(relic, 34))
        {
            ShowGameTip(Localization.Language == Localization.English
                ? $"Purchased relic\n{relic.DisplayName()}: {relic.DisplayDescription()}"
                : $"购买遗物\n{relic.DisplayName()}：{relic.DisplayDescription()}");
            ShowRunDeltaTips(before);
            _choicePanel.Visible = false;
            _continueButton.Visible = true;
        }

        RenderShared();
    }

    private void OnBuyHealPressed()
    {
        var before = CaptureRunTipSnapshot();
        if (_run.BuyHeal(18, 16))
        {
            ShowGameTip(Localization.Language == Localization.English ? "Treatment complete." : "治疗完成。");
            ShowRunDeltaTips(before);
            _choicePanel.Visible = false;
            _continueButton.Visible = true;
        }

        RenderShared();
    }

    private void OnBuyItemPressed(string itemId, int count, int cost)
    {
        var item = _gameData.GetItem(itemId);
        var before = CaptureRunTipSnapshot();
        if (_run.BuyItem(item, count, cost))
        {
            ShowGameTip(Localization.Language == Localization.English
                ? $"Purchased item\n{item.DisplayName()} x{count}"
                : $"购买道具\n{item.DisplayName()} x{count}");
            ShowRunDeltaTips(before);
            _choicePanel.Visible = false;
            _continueButton.Visible = true;
        }

        RenderShared();
    }

    private void OnLeaveShopPressed()
    {
        _run.Log.Add("你离开矿灯商队，继续向下。");
        _choicePanel.Visible = false;
        _continueButton.Visible = true;
        RenderShared();
        ShowGameTip(Localization.Language == Localization.English ? "Left the caravan." : "已离开矿灯商队。");
    }

    private void RenderCalibrationRoom(RunRoom room)
    {
        _activeCalibrationRoom = room;
        _selectedCalibrationNodes.Clear();
        BuildCalibrationNodes(room);
        _roomTitleLabel.Text = room.DisplayTitle();
        _roomDescriptionLabel.Text = Localization.Language == Localization.English
            ? "A cracked lamp array still hums. Select exactly 3 nodes to route power through. Stable sets are safe; greedy sets can surge."
            : "一组裂纹矿灯阵仍在低鸣。选择 3 个节点接入回路。稳态组合更安全，贪心组合可能过载。";
        _choicePanel.Visible = true;
        RenderCalibrationChoices();
    }

    private void BuildCalibrationNodes(RunRoom room)
    {
        var templates = new List<CalibrationNode>
        {
            new("stable_crystal", "稳定晶灯", "Stable Crystal", "稳态 / 净雾", "Stable / Fog", shards: 8, fogReduction: 1, tags: new[] { "stable", "fog" }),
            new("ore_prism", "富矿棱镜", "Ore Prism", "富矿 / 微量雾噪", "Ore / Minor fog", shards: 24, fogGain: 1, tags: new[] { "ore", "risk" }),
            new("oil_wick", "储油灯芯", "Oil Wick", "灯油 / 稳态", "Oil / Stable", oil: 22, tags: new[] { "oil", "stable" }),
            new("survey_map", "测绘星图", "Survey Map", "卡牌 / 稳态", "Card / Stable", cardId: "mapping_flare", tags: new[] { "card", "stable" }),
            new("rift_resonator", "裂隙谐振器", "Rift Resonator", "富矿 / 过载", "Ore / Surge", shards: 16, damage: 5, cardId: "crack_finder", tags: new[] { "ore", "risk", "card" }),
            new("mist_lens", "净雾透镜", "Mist Lens", "净雾 / 灯油", "Fog / Oil", oil: 8, fogReduction: 2, tags: new[] { "fog", "oil" })
        };

        _calibrationNodes.Clear();
        var offset = Math.Abs(HashCode.Combine(_run.RunSeed, room.NodeId, room.LayerIndex)) % templates.Count;
        for (var i = 0; i < templates.Count; i++)
        {
            _calibrationNodes.Add(templates[(i + offset) % templates.Count]);
        }
    }

    private void RenderCalibrationChoices()
    {
        ClearBox(_choiceList);
        _calibrationSummaryLabel = new Label
        {
            Text = BuildCalibrationPreviewText(),
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        _calibrationSummaryLabel.AddThemeColorOverride("font_color", Color.FromHtml("d8e2ee"));
        _choiceList.AddChild(_calibrationSummaryLabel);

        _calibrationGrid = new GridContainer
        {
            Columns = 2,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        _calibrationGrid.AddThemeConstantOverride("h_separation", 12);
        _calibrationGrid.AddThemeConstantOverride("v_separation", 12);
        _choiceList.AddChild(_calibrationGrid);

        for (var i = 0; i < _calibrationNodes.Count; i++)
        {
            var node = _calibrationNodes[i];
            var selected = _selectedCalibrationNodes.Contains(i);
            var button = new Button
            {
                Text = $"{(selected ? "◆ " : "◇ ")}{node.DisplayName()}\n{node.DisplayHint()}\n{FormatCalibrationNodePreview(node)}",
                CustomMinimumSize = new Vector2(0, 118),
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
                Disabled = !selected && _selectedCalibrationNodes.Count >= 3
            };
            StyleButton(button, selected ? Color.FromHtml("5b4a2a") : Color.FromHtml("263445"), selected ? Color.FromHtml("fff1d0") : Color.FromHtml("d8e2ee"));
            var captured = i;
            button.Pressed += () => ToggleCalibrationNode(captured);
            _calibrationGrid.AddChild(button);
        }

        var confirm = new Button
        {
            Text = Localization.Language == Localization.English ? "Start Calibration" : "启动校准",
            CustomMinimumSize = new Vector2(0, 58),
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            Disabled = _selectedCalibrationNodes.Count != 3
        };
        StyleButton(confirm, Color.FromHtml("315f46"), Color.FromHtml("e7fff1"));
        confirm.Pressed += CompleteCalibration;
        _choiceList.AddChild(confirm);

        AddChoiceButton(Localization.Language == Localization.English ? "Bypass\nLeave the lamp array untouched." : "绕过\n不触碰这组矿灯阵。", BypassCalibration);
    }

    private void ToggleCalibrationNode(int index)
    {
        if (_selectedCalibrationNodes.Contains(index))
        {
            _selectedCalibrationNodes.Remove(index);
        }
        else if (_selectedCalibrationNodes.Count < 3)
        {
            _selectedCalibrationNodes.Add(index);
        }

        RenderCalibrationChoices();
    }

    private void CompleteCalibration()
    {
        var outcome = CalculateCalibrationOutcome();
        var before = CaptureRunTipSnapshot();
        _run.ApplyCalibrationOutcome(outcome.Shards, outcome.Oil, outcome.FogReduction, outcome.Heal, outcome.CardId, outcome.Damage, outcome.FogGain, _gameData);
        _roomDescriptionLabel.Text = BuildCalibrationResultText(outcome);
        _choicePanel.Visible = false;
        _continueButton.Visible = true;
        RenderShared();
        ShowGameTip(Localization.Language == Localization.English ? "Calibration complete." : "校准完成。");
        ShowRunDeltaTips(before);
    }

    private void BypassCalibration()
    {
        _run.Log.Add(Localization.Language == Localization.English ? "You bypassed the lamp array." : "你绕过了矿灯阵。");
        _choicePanel.Visible = false;
        _continueButton.Visible = true;
        RenderShared();
        ShowGameTip(Localization.Language == Localization.English ? "Lamp array bypassed." : "已绕过矿灯阵。");
    }

    private CalibrationOutcome CalculateCalibrationOutcome()
    {
        var outcome = new CalibrationOutcome();
        var tagCounts = new Dictionary<string, int>();
        foreach (var index in _selectedCalibrationNodes)
        {
            var node = _calibrationNodes[index];
            outcome.Shards += node.Shards;
            outcome.Oil += node.Oil;
            outcome.FogReduction += node.FogReduction;
            outcome.Heal += node.Heal;
            outcome.Damage += node.Damage;
            outcome.FogGain += node.FogGain;
            if (string.IsNullOrWhiteSpace(outcome.CardId) && !string.IsNullOrWhiteSpace(node.CardId))
            {
                outcome.CardId = node.CardId;
            }

            foreach (var tag in node.Tags)
            {
                tagCounts[tag] = tagCounts.TryGetValue(tag, out var count) ? count + 1 : 1;
            }
        }

        if (tagCounts.GetValueOrDefault("stable") >= 2)
        {
            outcome.Oil += 8;
            outcome.FogReduction += 1;
            outcome.Notes.Add(Localization.Language == Localization.English ? "Stable circuit: oil +8, fog -1." : "稳态回路：灯油 +8，雾压 -1。");
        }
        if (tagCounts.GetValueOrDefault("ore") >= 2)
        {
            outcome.Shards += 16;
            outcome.Notes.Add(Localization.Language == Localization.English ? "Ore resonance: shards +16." : "富矿共振：矿晶 +16。");
        }
        if (tagCounts.GetValueOrDefault("oil") >= 2)
        {
            outcome.Oil += 12;
            outcome.Notes.Add(Localization.Language == Localization.English ? "Oil loop: oil +12." : "储油回路：灯油 +12。");
        }
        if (tagCounts.GetValueOrDefault("fog") >= 2)
        {
            outcome.FogReduction += 1;
            outcome.Notes.Add(Localization.Language == Localization.English ? "Mist lock: fog -1." : "净雾锁定：雾压 -1。");
        }
        if (tagCounts.GetValueOrDefault("risk") >= 2)
        {
            outcome.Damage += 6;
            outcome.FogGain += 1;
            outcome.Notes.Add(Localization.Language == Localization.English ? "Surge backlash: HP -6, fog +1." : "过载反震：生命 -6，雾压 +1。");
        }

        return outcome;
    }

    private string BuildCalibrationPreviewText()
    {
        var count = _selectedCalibrationNodes.Count;
        var header = Localization.Language == Localization.English
            ? $"Selected {count}/3. Combine tags for bonuses; two surge nodes cause backlash."
            : $"已选择 {count}/3。组合同类节点可触发额外收益；两个过载节点会反震。";
        if (count == 0)
        {
            return header;
        }

        var outcome = CalculateCalibrationOutcome();
        return $"{header}\n{BuildCalibrationResultText(outcome)}";
    }

    private string BuildCalibrationResultText(CalibrationOutcome outcome)
    {
        var parts = new List<string>();
        if (outcome.Shards > 0)
        {
            parts.Add(Localization.Language == Localization.English ? $"Shards +{outcome.Shards}" : $"矿晶 +{outcome.Shards}");
        }
        if (outcome.Oil > 0)
        {
            parts.Add(Localization.Language == Localization.English ? $"Oil +{outcome.Oil}" : $"灯油 +{outcome.Oil}");
        }
        if (outcome.FogReduction > 0)
        {
            parts.Add(Localization.Language == Localization.English ? $"Fog -{outcome.FogReduction}" : $"雾压 -{outcome.FogReduction}");
        }
        if (outcome.Heal > 0)
        {
            parts.Add($"HP +{outcome.Heal}");
        }
        if (!string.IsNullOrWhiteSpace(outcome.CardId))
        {
            parts.Add(Localization.Language == Localization.English ? $"Card: {_gameData.GetCard(outcome.CardId).DisplayName()}" : $"卡牌：{_gameData.GetCard(outcome.CardId).DisplayName()}");
        }
        if (outcome.Damage > 0)
        {
            parts.Add($"HP -{outcome.Damage}");
        }
        if (outcome.FogGain > 0)
        {
            parts.Add(Localization.Language == Localization.English ? $"Fog +{outcome.FogGain}" : $"雾压 +{outcome.FogGain}");
        }

        var summary = parts.Count == 0
            ? Localization.Language == Localization.English ? "No output yet." : "暂无输出。"
            : string.Join("  |  ", parts);
        return outcome.Notes.Count == 0 ? summary : $"{summary}\n{string.Join("\n", outcome.Notes)}";
    }

    private static string FormatCalibrationNodePreview(CalibrationNode node)
    {
        var parts = new List<string>();
        if (node.Shards > 0)
        {
            parts.Add(Localization.Language == Localization.English ? $"+{node.Shards} shards" : $"+{node.Shards} 矿晶");
        }
        if (node.Oil > 0)
        {
            parts.Add(Localization.Language == Localization.English ? $"+{node.Oil} oil" : $"+{node.Oil} 灯油");
        }
        if (node.FogReduction > 0)
        {
            parts.Add(Localization.Language == Localization.English ? $"-{node.FogReduction} fog" : $"-{node.FogReduction} 雾压");
        }
        if (!string.IsNullOrWhiteSpace(node.CardId))
        {
            parts.Add(Localization.Language == Localization.English ? "card signal" : "卡牌信号");
        }
        if (node.Damage > 0)
        {
            parts.Add($"-{node.Damage} HP");
        }
        if (node.FogGain > 0)
        {
            parts.Add(Localization.Language == Localization.English ? $"+{node.FogGain} fog" : $"+{node.FogGain} 雾压");
        }

        return string.Join("  ", parts);
    }

    private void StartBattleRoom(RunRoom room)
    {
        _continueButton.Visible = false;
        _activeBattleRoom = room;
        _returnToMineAfterBattle = false;
        var enemies = BuildEncounterEnemies(room, false, room.EnemyId);
        _battle.StartBattle(BuildBattleDeck(), enemies, _run.PlayerMaxHp, _run.PlayerHp, CalculateThreatLevel(room, false));
        ApplyBattleStartRelics();
        SetCombatPortraits();

        _roomTitleLabel.Text = room.DisplayTitle();
        _roomDescriptionLabel.Text = $"战斗开始。威胁等级 {_battle.ThreatLevel}，敌群：{FormatEncounterNames(enemies)}。优先击杀目标会直接改变下一回合压力。";
        _battlePanel.Visible = true;
        RenderBattle();
        ShowGameTip(Localization.Language == Localization.English
            ? $"Combat started\nThreat {_battle.ThreatLevel}: {FormatEncounterNames(enemies)}"
            : $"战斗开始\n威胁等级 {_battle.ThreatLevel}：{FormatEncounterNames(enemies)}");
    }

    private void ApplyBattleStartRelics()
    {
        _battle.SetPlayerModifiers(
            _run.GetRunEffectValue(_gameData, "player_damage_bonus"),
            _run.GetRunEffectValue(_gameData, "player_block_bonus"),
            _run.GetRunEffectValue(_gameData, "self_damage_reduction"));
        _battle.GainBlock(_run.GetRunEffectValue(_gameData, "battle_start_block"));
        _battle.GainEnergy(_run.GetRunEffectValue(_gameData, "battle_start_energy"));
        _battle.DrawExtraCards(_run.GetRunEffectValue(_gameData, "battle_start_draw"));
    }

    private List<EnemyData> BuildEncounterEnemies(RunRoom? room, bool fromMine, string fallbackEnemyId)
    {
        var enemies = new List<EnemyData>();
        if (room != null)
        {
            foreach (var enemyId in room.EnemyIds)
            {
                if (!string.IsNullOrWhiteSpace(enemyId))
                {
                    enemies.Add(_gameData.GetEnemy(enemyId));
                }
            }
        }

        if (enemies.Count == 0 && !string.IsNullOrWhiteSpace(fallbackEnemyId))
        {
            enemies.Add(_gameData.GetEnemy(fallbackEnemyId));
        }

        if (!fromMine && enemies.Count == 1 && room != null && room.Kind == "elite")
        {
            enemies.Add(_gameData.GetEnemy(room.Risk >= 4 ? "crystal_guard" : "lamp_mite"));
        }
        else if (!fromMine && enemies.Count == 1 && room != null && room.Risk >= 3)
        {
            enemies.Add(_gameData.GetEnemy(room.Risk >= 4 ? "crystal_guard" : "lamp_mite"));
        }
        else if (fromMine && enemies.Count == 1 && room != null && room.Risk >= 4)
        {
            enemies.Add(_gameData.GetEnemy("lamp_mite"));
        }

        if (enemies.Count == 0)
        {
            enemies.Add(_gameData.GetEnemy("slime"));
        }

        return enemies;
    }

    private List<CardData> BuildBattleDeck()
    {
        var deck = new List<CardData>(_run.PlayerDeck);
        var fogCards = GetBattleFogCardCount();
        if (fogCards <= 0)
        {
            return deck;
        }

        var fogCard = _gameData.GetCard("dense_fog");
        for (var i = 0; i < fogCards; i++)
        {
            deck.Add(fogCard);
        }

        return deck;
    }

    private int GetBattleFogCardCount()
    {
        var fromPressure = _run.FogPressure / 3;
        var lowOil = _run.LampOil <= Math.Max(1, _run.MaxLampOil / 4) ? 1 : 0;
        return Math.Clamp(fromPressure + lowOil, 0, 5);
    }

    private static string FormatEncounterNames(IReadOnlyList<EnemyData> enemies)
    {
        var names = new List<string>();
        foreach (var enemy in enemies)
        {
            names.Add(enemy.DisplayName());
        }

        return string.Join("、", names);
    }

    private int CalculateThreatLevel(RunRoom? room, bool fromMine)
    {
        var depth = Math.Max(0, _run.CurrentLayerIndex);
        var risk = Math.Max(1, room?.Risk ?? 1);
        var depthThreat = depth / 2;
        var fogThreat = _run.FogPressure / 3;
        var lowOilThreat = _run.LampOil <= Math.Max(1, _run.MaxLampOil / 4) ? 1 : 0;
        var eliteThreat = room?.Kind == "elite" ? 2 : 0;
        var threat = depthThreat + risk - 1 + fogThreat + lowOilThreat + eliteThreat;
        if (fromMine)
        {
            threat = Math.Max(0, threat - 1);
        }

        return Math.Clamp(threat, 0, 12);
    }

    private void OnEndTurnPressed()
    {
        if (IsBattleFinished())
        {
            return;
        }

        var beforePlayerHp = _battle.PlayerHp;
        var beforeEnemyHp = _battle.TotalEnemyHp;
        var beforeEnemyBlock = _battle.TotalEnemyBlock;
        _battle.EndPlayerTurn();
        ResolveBattleIfFinished();
        if (_battle.PlayerHp < beforePlayerHp)
        {
            AnimateCombatFeedback(_playerPortrait, new Vector2(-18, 0), Color.FromHtml("ffb1b8"));
        }
        if (_battle.TotalEnemyHp > beforeEnemyHp || _battle.TotalEnemyBlock > beforeEnemyBlock)
        {
            AnimateCombatFeedback(GetEnemyFeedbackTarget(_battle.SelectedEnemyIndex), new Vector2(10, 0), Color.FromHtml("d8e2ee"));
        }
        RenderBattle();
        RenderShared();
    }

    private void OnPlayCardPressed(int index, int targetEnemyIndex = -1)
    {
        if (IsBattleFinished())
        {
            return;
        }

        if (index < 0 || index >= _battle.Hand.Count)
        {
            return;
        }

        var card = _battle.Hand[index];
        var targetsEnemy = CardTargetsEnemy(card);
        var resolvedTarget = targetEnemyIndex >= 0 ? targetEnemyIndex : _battle.SelectedEnemyIndex;
        var beforeTargetHp = targetsEnemy && resolvedTarget >= 0 && resolvedTarget < _battle.Enemies.Count
            ? _battle.Enemies[resolvedTarget].Hp
            : _battle.EnemyHp;
        var beforePlayerHp = _battle.PlayerHp;
        var beforePlayerBlock = _battle.PlayerBlock;
        if (!_battle.PlayCard(index, resolvedTarget))
        {
            RenderBattle();
            RenderShared();
            return;
        }

        if (targetsEnemy && resolvedTarget >= 0 && resolvedTarget < _battle.Enemies.Count && _battle.Enemies[resolvedTarget].Hp < beforeTargetHp)
        {
            AnimateCombatFeedback(GetEnemyFeedbackTarget(resolvedTarget), new Vector2(18, 0), Color.FromHtml("ffb1b8"));
        }
        if (_battle.PlayerBlock > beforePlayerBlock || _battle.PlayerHp > beforePlayerHp)
        {
            AnimateCombatFeedback(_playerPortrait, new Vector2(-10, 0), Color.FromHtml("bdf7d4"));
        }
        CancelAimingCard(false);
        ResolveBattleIfFinished();
        RenderBattle();
        RenderShared();
    }

    private void OnCardGuiInput(InputEvent inputEvent, Button button, int handIndex)
    {
        if (IsBattleFinished() || handIndex < 0 || handIndex >= _battle.Hand.Count)
        {
            return;
        }

        if (inputEvent is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
        {
            SelectAimingCard(button, handIndex);
        }
    }

    private void SelectAimingCard(Button button, int handIndex)
    {
        CancelAimingCard(false);
        _aimingCardButton = button;
        _aimingCardIndex = handIndex;
        _dragTargetsEnemy = CardTargetsEnemy(_battle.Hand[handIndex]);
        _dragLineStart = button.GlobalPosition + button.Size * new Vector2(0.5f, 0.06f);
        button.ZIndex = 100;
        button.Scale = new Vector2(1.08f, 1.08f);
        button.Modulate = Color.FromHtml("ffffff");
        ShowAimingFeedback(_battle.Hand[handIndex]);
        UpdateAimingLine(GetGlobalMousePosition());
        UpdateAimingFeedback(GetGlobalMousePosition());
    }

    private void CancelAimingCard(bool animate = true)
    {
        if (_aimingCardButton != null && GodotObject.IsInstanceValid(_aimingCardButton))
        {
            _aimingCardButton.ZIndex = 0;
            _aimingCardButton.Scale = Vector2.One;
            if (animate)
            {
                AnimateInvalidDrop(_aimingCardButton);
            }
        }

        _aimingCardButton = null;
        _aimingCardIndex = -1;
        HideAimingFeedback();
    }

    private void TryPlayAimedCardOnEnemy(int enemyIndex)
    {
        if (_aimingCardIndex < 0 || _aimingCardIndex >= _battle.Hand.Count)
        {
            _battle.SelectEnemy(enemyIndex);
            RenderBattle();
            return;
        }

        var card = _battle.Hand[_aimingCardIndex];
        if (!CardTargetsEnemy(card))
        {
            AnimateInvalidDrop(_aimingCardButton ?? GetEnemyFeedbackTarget(enemyIndex));
            return;
        }

        if (card.Cost > _battle.Energy)
        {
            AnimateInvalidDrop(_aimingCardButton ?? GetEnemyFeedbackTarget(enemyIndex));
            return;
        }

        OnPlayCardPressed(_aimingCardIndex, enemyIndex);
    }

    private void TryPlayAimedCardOnPlayer()
    {
        if (_aimingCardIndex < 0 || _aimingCardIndex >= _battle.Hand.Count)
        {
            return;
        }

        var card = _battle.Hand[_aimingCardIndex];
        if (CardTargetsEnemy(card) || card.Cost > _battle.Energy)
        {
            AnimateInvalidDrop((Control?)_aimingCardButton ?? _playerCombatPanel);
            return;
        }

        OnPlayCardPressed(_aimingCardIndex, -1);
    }

    private int FindEnemyTargetAt(Vector2 globalPosition)
    {
        foreach (var pair in _enemyTargetButtons)
        {
            if (pair.Value.GetGlobalRect().HasPoint(globalPosition))
            {
                return pair.Key;
            }
        }

        return -1;
    }

    private bool IsValidAimingTarget(CardData card, Vector2 globalPosition)
    {
        if (card.Cost > _battle.Energy)
        {
            return false;
        }

        var wantsEnemy = CardTargetsEnemy(card);
        return wantsEnemy ? FindEnemyTargetAt(globalPosition) >= 0 : _playerCombatPanel.GetGlobalRect().HasPoint(globalPosition);
    }

    private static bool CardTargetsEnemy(CardData card)
    {
        foreach (var action in card.Actions)
        {
            if (action.Type is "damage" or "weak" or "vulnerable")
            {
                return true;
            }
        }

        return false;
    }

    private void AnimateInvalidDrop(Control button)
    {
        var originalModulate = button.Modulate;
        var tween = CreateTween();
        tween.TweenProperty(button, "modulate", Color.FromHtml("ff8f8f"), 0.05f);
        tween.TweenProperty(button, "modulate", originalModulate, 0.12f);
    }

    private void ShowAimingFeedback(CardData card)
    {
        var targetText = CardTargetsEnemy(card)
            ? (Localization.Language == Localization.English ? "Click an enemy to play" : "点击敌人释放")
            : (Localization.Language == Localization.English ? "Click yourself to play" : "点击自己释放");
        _dragHintLabel.Text = $"{card.DisplayName()} - {targetText}";
        _dragHintPanel.Visible = true;
        StyleCombatPanels(!_dragTargetsEnemy, _dragTargetsEnemy);
    }

    private void UpdateAimingFeedback(Vector2 globalMousePosition)
    {
        if (_aimingCardIndex < 0 || _aimingCardIndex >= _battle.Hand.Count)
        {
            return;
        }

        var valid = IsValidAimingTarget(_battle.Hand[_aimingCardIndex], globalMousePosition);
        _dragHintLabel.AddThemeColorOverride("font_color", valid ? Color.FromHtml("bdf7d4") : Color.FromHtml("f4f0df"));
    }

    private void HideAimingFeedback()
    {
        _dragHintPanel.Visible = false;
        _dragLine.Visible = false;
        StyleCombatPanels(false, false);
    }

    private void UpdateAimingLine(Vector2 globalMousePosition)
    {
        if (_aimingCardButton == null || _aimingCardIndex < 0 || _aimingCardIndex >= _battle.Hand.Count)
        {
            return;
        }

        var root = GetNode<Panel>("Root");
        _dragLine.Visible = true;
        var rootOrigin = root.GlobalPosition;
        _dragLineStart = _aimingCardButton.GlobalPosition + _aimingCardButton.Size * new Vector2(0.5f, 0.06f);
        _dragLine.Points = new[]
        {
            _dragLineStart - rootOrigin,
            globalMousePosition - rootOrigin
        };
        _dragLine.DefaultColor = IsValidAimingTarget(_battle.Hand[_aimingCardIndex], globalMousePosition)
            ? Color.FromHtml("78d69b")
            : Color.FromHtml("d7b45f");
    }

    private void AnimateCombatFeedback(Control target, Vector2 offset, Color flashColor)
    {
        var originalPosition = target.Position;
        var originalModulate = target.Modulate;
        var tween = CreateTween();
        tween.SetParallel(true);
        tween.TweenProperty(target, "position", originalPosition + offset, 0.08f).SetTrans(Tween.TransitionType.Sine);
        tween.TweenProperty(target, "modulate", flashColor, 0.08f);
        tween.SetParallel(false);
        tween.TweenProperty(target, "position", originalPosition, 0.12f).SetTrans(Tween.TransitionType.Sine);
        tween.TweenProperty(target, "modulate", originalModulate, 0.12f);
    }

    private void ResolveBattleIfFinished()
    {
        if (_battle.AllEnemiesDefeated)
        {
            _run.SyncAfterBattle(_battle.PlayerHp);
            if (_returnToMineAfterBattle)
            {
                RunLoadingTransition(
                    Localization.Language == Localization.English ? "Returning to the survey grid..." : "返回勘探网格……",
                    () =>
                    {
                        _run.ResolveMineMonsterVictory(_gameData);
                        _battlePanel.Visible = false;
                        _minePanel.Visible = true;
                        _returnToMineAfterBattle = false;
                        _roomTitleLabel.Text = _activeMineRoom?.DisplayTitle() ?? _roomTitleLabel.Text;
                        _roomDescriptionLabel.Text = Localization.Language == Localization.English
                            ? "The monster is driven off. Keep reading the clues, or move deeper after finding the exit."
                            : "怪物被击退。你可以继续根据线索探索矿区，或在找到出口后深入下一层。";
                        RenderMinefield();
                        RenderShared();
                        ShowGameTip(Localization.Language == Localization.English
                            ? "Monster defeated\nSurvey route reopened."
                            : "怪物已击退\n勘探路线恢复。");
                    });
            }
            else
            {
                RunLoadingTransition(
                    Localization.Language == Localization.English ? "Sorting the spoils..." : "清点战利品……",
                    ShowBattleReward);
            }
        }
        else if (_battle.PlayerHp <= 0)
        {
            _run.SyncAfterBattle(0);
            RunLoadingTransition(
                Localization.Language == Localization.English ? "Recording the failed expedition..." : "记录本次探索……",
                () => ShowEndPanel(false, "你的矿灯熄灭了。下一次进入矿井前，最好再调整牌组与路线。"));
        }
    }

    private void ShowBattleReward()
    {
        if (_activeBattleRoom == null)
        {
            return;
        }

        _battlePanel.Visible = false;
        _rewardPanel.Visible = true;
        _roomTitleLabel.Text = Localization.Language == Localization.English ? "Battle Won" : "战斗胜利";
        _roomDescriptionLabel.Text = Localization.Language == Localization.English
            ? "Choose one spoil. Relics change the rest of this run."
            : "选择一项战利品。遗物会改变本局后续路线、战斗与资源压力。";
        ShowGameTip(Localization.Language == Localization.English
            ? "Battle won\nChoose a spoil."
            : "战斗胜利\n选择一项战利品。");

        var reward = _gameData.GetReward(_activeBattleRoom.RewardId);
        ClearBox(_rewardList);

        var cardChoiceCount = 3 + Math.Max(0, _run.GetRunEffectValue(_gameData, "reward_card_bonus"));
        var cardChoices = _gameData.BuildRewardChoices(
            reward,
            _run.CharacterId,
            HashCode.Combine(_run.RunSeed, _run.CurrentLayerIndex, reward.Id, _run.PlayerDeck.Count),
            cardChoiceCount);

        foreach (var card in cardChoices)
        {
            var button = new Button
            {
                Text = $"{FormatCardHeader(card)}\n{card.DisplayDescription()}",
                CustomMinimumSize = new Vector2(0, 74),
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
            StyleButton(button, Color.FromHtml("233548"), Color.FromHtml("d8e2ee"));
            button.Pressed += () => OnRewardPickedV2(reward, card, null);
            _rewardList.AddChild(button);
        }

        var relicChoices = _gameData.BuildRelicChoices(
            reward,
            _run.Relics,
            HashCode.Combine(_run.RunSeed, _run.CurrentLayerIndex, reward.Id, "relic"),
            2);
        foreach (var relic in relicChoices)
        {
            var button = new Button
            {
                Text = FormatRelicReward(relic),
                CustomMinimumSize = new Vector2(0, 74),
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
            StyleButton(button, Color.FromHtml("392d4b"), Color.FromHtml("f0e4ff"));
            button.Pressed += () => OnRewardPickedV2(reward, null, relic);
            _rewardList.AddChild(button);
        }

        var totalBonusPercent = (_activeBattleRoom?.RewardBonus ?? 0) + _run.GetRunEffectValue(_gameData, "reward_shards_percent");
        var shardPreview = reward.Shards + (reward.Shards * Math.Max(0, totalBonusPercent) / 100);
        var skipText = Localization.Language == Localization.English
            ? $"Skip picks: gain {shardPreview} shards and heal {reward.Heal}."
            : $"跳过选择：获得 {shardPreview} 矿晶并治疗 {reward.Heal} 点。";
        var skipButton = new Button
        {
            Text = skipText,
            CustomMinimumSize = new Vector2(0, 52),
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        StyleButton(skipButton, Color.FromHtml("3a3140"), Color.FromHtml("ead7f7"));
        skipButton.Pressed += () => OnRewardPickedV2(reward, null, null);
        _rewardList.AddChild(skipButton);
    }

    private void OnRewardPickedV2(RewardData reward, CardData? card, RelicData? relic)
    {
        var before = CaptureRunTipSnapshot();
        _run.ApplyRewardWithRelic(reward, card, relic, _gameData, _activeBattleRoom?.RewardBonus ?? 0);
        if (card != null)
        {
            ShowGameTip(Localization.Language == Localization.English
                ? $"Card reward\n{card.DisplayName()}: {card.DisplayDescription()}"
                : $"卡牌奖励\n{card.DisplayName()}：{card.DisplayDescription()}");
        }
        if (relic != null)
        {
            ShowGameTip(Localization.Language == Localization.English
                ? $"Relic reward\n{relic.DisplayName()}: {relic.DisplayDescription()}"
                : $"遗物奖励\n{relic.DisplayName()}：{relic.DisplayDescription()}");
        }
        ShowRunDeltaTips(before);
        _rewardPanel.Visible = false;
        _continueButton.Visible = true;
        RenderShared();
    }

    private static string FormatRelicReward(RelicData relic)
    {
        var rarity = relic.Rarity switch
        {
            "rare" => Localization.Language == Localization.English ? "Rare Relic" : "稀有遗物",
            "uncommon" => Localization.Language == Localization.English ? "Uncommon Relic" : "罕见遗物",
            _ => Localization.Language == Localization.English ? "Common Relic" : "普通遗物"
        };

        return $"{rarity} | {relic.DisplayName()}\n{relic.DisplayDescription()}";
    }

    private void ShowBattleRewardLegacy()
    {
        if (_activeBattleRoom == null)
        {
            return;
        }

        _battlePanel.Visible = false;
        _rewardPanel.Visible = true;
        _roomTitleLabel.Text = "战斗胜利";
        _roomDescriptionLabel.Text = "从破碎矿壳里挑选战利品。";

        var reward = _gameData.GetReward(_activeBattleRoom.RewardId);
        ClearBox(_rewardList);

        var cardChoices = _gameData.BuildRewardChoices(reward, _run.CharacterId, HashCode.Combine(_run.RunSeed, _run.CurrentLayerIndex, reward.Id, _run.PlayerDeck.Count), 3);
        foreach (var card in cardChoices)
        {
            var button = new Button
            {
                Text = $"{FormatCardHeader(card)}\n{card.DisplayDescription()}",
                CustomMinimumSize = new Vector2(0, 72),
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
            StyleButton(button, Color.FromHtml("233548"), Color.FromHtml("d8e2ee"));
            button.Pressed += () => OnRewardPicked(reward, card);
            _rewardList.AddChild(button);
        }

        var skipButton = new Button
        {
            Text = $"跳过卡牌，只获得 {reward.Shards} 矿晶并治疗 {reward.Heal} 点",
            CustomMinimumSize = new Vector2(0, 52),
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        StyleButton(skipButton, Color.FromHtml("3a3140"), Color.FromHtml("ead7f7"));
        skipButton.Pressed += () => OnRewardPicked(reward, null);
        _rewardList.AddChild(skipButton);
    }

    private void OnRewardPicked(RewardData reward, CardData? card)
    {
        var before = CaptureRunTipSnapshot();
        _run.ApplyReward(reward, card, _activeBattleRoom?.RewardBonus ?? 0);
        if (card != null)
        {
            ShowGameTip(Localization.Language == Localization.English
                ? $"Card reward\n{card.DisplayName()}: {card.DisplayDescription()}"
                : $"卡牌奖励\n{card.DisplayName()}：{card.DisplayDescription()}");
        }
        ShowRunDeltaTips(before);
        _rewardPanel.Visible = false;
        _continueButton.Visible = true;
        RenderShared();
    }

    private bool IsBattleFinished()
    {
        return _battle.PlayerHp <= 0 || _battle.AllEnemiesDefeated;
    }

    private void RenderBattle()
    {
        if (!_battlePanel.Visible)
        {
            return;
        }

        var playerStatuses = _battle.PlayerWeak > 0 ? $" | 虚弱 {_battle.PlayerWeak}" : string.Empty;
        _playerHpBar.MaxValue = _battle.PlayerMaxHp;
        _playerHpBar.Value = Math.Max(0, _battle.PlayerHp);
        _playerCombatLabel.Text = $"玩家  HP {_battle.PlayerHp}/{_battle.PlayerMaxHp}\n能量 {_battle.Energy}{playerStatuses}";
        _playerCombatPanel.TooltipText = BuildPlayerStatusTooltip();
        _enemyCombatLabel.Text = $"敌群  存活 {CountAliveEnemies()}/{_battle.Enemies.Count}\n威胁 {_battle.ThreatLevel} | 当前目标：{_battle.Enemy.DisplayName()}";
        _playerBlockLabel.Text = $"格挡 {_battle.PlayerBlock}";
        _battleResourceLabel.Text = $"抽牌堆 {_battle.DrawPile.Count} | 弃牌堆 {_battle.DiscardPile.Count} | 手牌 {_battle.Hand.Count}";
        _statusHelpLabel.Text = BuildStatusHelpText();
        _intentPanel.Visible = false;

        RenderEnemyTargets();

        ClearBox(_handBox);
        for (var i = 0; i < _battle.Hand.Count; i++)
        {
            var card = _battle.Hand[i];
            var button = new Button
            {
                Text = FormatBattleCardText(card),
                CustomMinimumSize = new Vector2(150, 188),
                SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
            StyleCardButton(button, card, card.Cost <= _battle.Energy);
            button.TooltipText = BuildCardTooltip(card);
            var captured = i;
            button.GuiInput += inputEvent => OnCardGuiInput(inputEvent, button, captured);
            _handBox.AddChild(button);
        }

        CallDeferred(nameof(LayoutHandFan));
    }

    private int CountAliveEnemies()
    {
        var count = 0;
        foreach (var enemy in _battle.Enemies)
        {
            if (enemy.IsAlive)
            {
                count++;
            }
        }

        return count;
    }

    private void RenderEnemyTargets()
    {
        ClearBox(_enemyTargetRow);
        _enemyTargetButtons.Clear();
        for (var i = 0; i < _battle.Enemies.Count; i++)
        {
            var enemy = _battle.Enemies[i];
            var selected = i == _battle.SelectedEnemyIndex;
            var button = new Button
            {
                Text = FormatEnemyTargetText(enemy, i, selected),
                CustomMinimumSize = new Vector2(176, 232),
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
                Disabled = !enemy.IsAlive
            };
            StyleEnemyTargetButton(button, enemy, selected);
            button.TooltipText = BuildEnemyStatusTooltip(enemy);
            var captured = i;
            button.Pressed += () => OnEnemyTargetPressed(captured);
            _enemyTargetButtons[captured] = button;
            _enemyTargetRow.AddChild(button);
        }
    }

    private string FormatEnemyTargetText(BattleEnemyState enemy, int index, bool selected)
    {
        var marker = selected ? "▶ " : string.Empty;
        if (!enemy.IsAlive)
        {
            return $"{enemy.Data.DisplayName()}\nHP 0/{enemy.MaxHp}\n已击倒";
        }

        var status = FormatEnemyStatusLine(enemy);
        var intentPreview = _battle.GetEnemyIntentPreview(index);
        var counterHint = BuildEnemyCounterHint(_battle.GetCurrentEnemyIntent(index));
        return $"{marker}{enemy.Data.DisplayName()}\nHP {Math.Max(0, enemy.Hp)}/{enemy.MaxHp}  格挡 {enemy.Block}\n破势 {enemy.Stagger}/{enemy.StaggerLimit}{status}\n下一步: {intentPreview}\n应对: {counterHint}";
    }

    private static string FormatEnemyStatusLine(BattleEnemyState enemy)
    {
        var parts = new List<string>();
        if (enemy.Weak > 0)
        {
            parts.Add($"虚弱{enemy.Weak}");
        }
        if (enemy.Vulnerable > 0)
        {
            parts.Add($"易伤{enemy.Vulnerable}");
        }

        return parts.Count == 0 ? string.Empty : $"\n状态 {string.Join(" / ", parts)}";
    }

    private static string BuildEnemyCounterHint(IntentData intent)
    {
        var hasDamage = false;
        var hasControl = false;
        var hasDefense = false;
        var hasHeal = false;
        foreach (var action in intent.Actions)
        {
            switch (action.Type)
            {
                case "damage":
                case "damage_all":
                    hasDamage = true;
                    break;
                case "weak":
                case "weak_all":
                case "vulnerable":
                case "vulnerable_all":
                    hasControl = true;
                    break;
                case "block":
                case "block_per_enemy":
                    hasDefense = true;
                    break;
                case "heal":
                    hasHeal = true;
                    break;
            }
        }

        if (hasDamage && hasControl)
        {
            return Localization.Language == Localization.English ? "Block first or burst it down." : "优先格挡，能击杀就先击杀。";
        }
        if (hasDamage)
        {
            return Localization.Language == Localization.English ? "Prepare block or finish it." : "准备格挡，或抢先击杀。";
        }
        if (hasHeal)
        {
            return Localization.Language == Localization.English ? "Focus fire before it recovers." : "优先集火，避免回血拖长战斗。";
        }
        if (hasDefense)
        {
            return Localization.Language == Localization.English ? "Consider switching target." : "它要防御，可考虑转火。";
        }
        if (hasControl)
        {
            return Localization.Language == Localization.English ? "Plan around the debuff." : "注意负面状态，保留解场节奏。";
        }

        return Localization.Language == Localization.English ? "Low immediate threat." : "当前威胁较低。";
    }

    private string BuildStatusHelpText()
    {
        return Localization.Language == Localization.English
            ? "Status: Weak reduces outgoing attack damage by 2. Vulnerable takes 50% more player attack damage. Status duration drops at the end of that unit's turn."
            : "状态说明：虚弱会使造成的攻击伤害 -2；易伤会使受到的玩家攻击伤害 +50%。状态在其回合结束时减少 1。";
    }

    private string BuildPlayerStatusTooltip()
    {
        if (_battle.PlayerWeak <= 0)
        {
            return Localization.Language == Localization.English
                ? "No active status."
                : "当前没有状态。";
        }

        return Localization.Language == Localization.English
            ? $"Weak {_battle.PlayerWeak}: your outgoing attack damage is reduced by 2. Duration drops at the end of your turn."
            : $"虚弱 {_battle.PlayerWeak}：你造成的攻击伤害减少 2。持续回合在你的回合结束时减少。";
    }

    private string BuildEnemyStatusTooltip(BattleEnemyState enemy)
    {
        var lines = new List<string>();
        if (enemy.Weak > 0)
        {
            lines.Add(Localization.Language == Localization.English
                ? $"Weak {enemy.Weak}: outgoing attack damage is reduced by 2."
                : $"虚弱 {enemy.Weak}：造成的攻击伤害减少 2。");
        }

        if (enemy.Vulnerable > 0)
        {
            lines.Add(Localization.Language == Localization.English
                ? $"Vulnerable {enemy.Vulnerable}: takes 50% more attack damage from your cards."
                : $"易伤 {enemy.Vulnerable}：受到你的卡牌攻击伤害提高 50%。");
        }

        if (enemy.IsAlive)
        {
            lines.Add(Localization.Language == Localization.English
                ? "Status duration drops at the end of this enemy's turn."
                : "状态持续回合在该敌人的回合结束时减少。");
        }

        return lines.Count == 0
            ? Localization.Language == Localization.English ? "No active status." : "当前没有状态。"
            : string.Join("\n", lines);
    }

    private string BuildCardTooltip(CardData card)
    {
        var lines = new List<string> { $"{card.DisplayName()} - {card.DisplayDescription()}" };
        foreach (var action in card.Actions)
        {
            switch (action.Type)
            {
                case "weak":
                    lines.Add(Localization.Language == Localization.English
                        ? "Weak: reduces the target's outgoing attack damage by 2."
                        : "虚弱：使目标造成的攻击伤害减少 2。");
                    break;
                case "vulnerable":
                    lines.Add(Localization.Language == Localization.English
                        ? "Vulnerable: target takes 50% more attack damage from your cards."
                        : "易伤：使目标受到你的卡牌攻击伤害提高 50%。");
                    break;
            }
        }

        return string.Join("\n", lines);
    }

    private void OnEnemyTargetPressed(int enemyIndex)
    {
        if (_aimingCardIndex >= 0)
        {
            TryPlayAimedCardOnEnemy(enemyIndex);
            return;
        }

        _battle.SelectEnemy(enemyIndex);
        RenderBattle();
    }

    private void OnPlayerCombatGuiInput(InputEvent inputEvent)
    {
        if (inputEvent is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left } && _aimingCardIndex >= 0)
        {
            TryPlayAimedCardOnPlayer();
        }
    }

    private Control GetEnemyFeedbackTarget(int enemyIndex)
    {
        return _enemyTargetButtons.TryGetValue(enemyIndex, out var button) && GodotObject.IsInstanceValid(button)
            ? button
            : _enemyCombatPanel;
    }

    private void SetCombatPortraits()
    {
        var character = _gameData.GetCharacter(GameSession.SelectedCharacterId);
        _playerPortrait.Texture = UiArt.LoadTexture(character.ArtPath);
    }

    private void LayoutHandFan()
    {
        var count = _handBox.GetChildCount();
        if (count <= 0)
        {
            return;
        }

        var middle = (count - 1) / 2f;
        for (var i = 0; i < count; i++)
        {
            if (_handBox.GetChild(i) is not Control cardControl)
            {
                continue;
            }

            var offsetFromMiddle = i - middle;
            var normalized = middle <= 0 ? 0 : offsetFromMiddle / middle;
            cardControl.PivotOffset = cardControl.Size / 2f;
            cardControl.RotationDegrees = normalized * 9f;
            cardControl.Position += new Vector2(0, Math.Abs(normalized) * 24f);
        }
    }

    private static Texture2D? LoadTexture(string path)
    {
        return UiArt.LoadTexture(path);
    }

    private void RenderShared()
    {
        var displayedHp = _battlePanel.Visible ? _battle.PlayerHp : _run.PlayerHp;
        var displayedMaxHp = _battlePanel.Visible ? _battle.PlayerMaxHp : _run.PlayerMaxHp;
        RenderHud(displayedHp, displayedMaxHp);

        var sb = new StringBuilder();
        for (var i = _battle.Log.Count - 1; i >= 0; i--)
        {
            sb.AppendLine(_battle.Log[i]);
        }

        if (_battle.Log.Count > 0 && _run.Log.Count > 0)
        {
            sb.AppendLine();
        }

        for (var i = _run.Log.Count - 1; i >= 0; i--)
        {
            sb.AppendLine(_run.Log[i]);
        }

        _logLabel.Text = sb.ToString();
        RenderItems();
        RenderDebug();
    }

    private void RenderHud(int displayedHp, int displayedMaxHp)
    {
        var currentLayer = Math.Max(_run.CurrentLayerIndex + 1, 0);
        var isEnglish = Localization.Language == Localization.English;
        _hudLayerLabel.Text = $"{(isEnglish ? "Layer" : "层数")}\n{currentLayer}/{_run.MapLayers.Count}";
        _hudHpLabel.Text = $"HP\n{displayedHp}/{displayedMaxHp}";
        _hudShardsLabel.Text = $"{(isEnglish ? "Shards" : "矿晶")}\n{_run.Shards}";
        _hudDeckLabel.Text = $"{(isEnglish ? "Deck" : "牌组")}\n{_run.PlayerDeck.Count}";
        _hudLampLabel.Text = $"{(isEnglish ? "Oil" : "灯油")}\n{_run.LampOil}/{_run.MaxLampOil}";
        _hudFogLabel.Text = $"{(isEnglish ? "Fog" : "雾压")}\n{FormatFogDots()}";
        _hudScoreLabel.Text = $"{(isEnglish ? "Score" : "分数")}\n{_run.Score}";
        _hudObjectiveLabel.Text = $"{(isEnglish ? "Objective" : "目标")}\n{FormatObjectiveStatus()}";

        _hudHpLabel.AddThemeColorOverride("font_color", displayedHp <= displayedMaxHp / 3 ? Color.FromHtml("ff9aa8") : MistTheme.TextMain);
        _hudShardsLabel.AddThemeColorOverride("font_color", Color.FromHtml("b58aff"));
        _hudLampLabel.AddThemeColorOverride("font_color", _run.LampOil <= _run.MaxLampOil / 4 ? Color.FromHtml("ffcf88") : MistTheme.TextMain);
        _hudFogLabel.AddThemeColorOverride("font_color", Color.FromHtml("b58aff"));
        _hudObjectiveLabel.AddThemeColorOverride("font_color", _run.IsObjectiveComplete() ? Color.FromHtml("bdf7d4") : MistTheme.TextMain);
    }

    private string FormatFogDots()
    {
        var filled = Math.Clamp(_run.FogPressure, 0, 8);
        return $"{new string('●', filled)}{new string('○', 8 - filled)}";
    }

    private void RenderDebug()
    {
        if (!_debugVisible)
        {
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("Debug");
        sb.AppendLine($"Run seed: {_run.RunSeed}");
        sb.AppendLine($"Layer: {Math.Max(_run.CurrentLayerIndex + 1, 0)}/{_run.MapLayers.Count}");
        sb.AppendLine($"Room: {_run.CurrentRoom?.Kind ?? "-"} / {_run.CurrentRoom?.DisplayTitle() ?? "-"}");
        sb.AppendLine($"HP: {_run.PlayerHp}/{_run.PlayerMaxHp}  Shards: {_run.Shards}");
        sb.AppendLine($"Lamp: {_run.LampOil}/{_run.MaxLampOil}  Fog: {_run.FogPressure}  Score: {_run.Score}");
        sb.AppendLine($"Deck: {_run.PlayerDeck.Count}  Items: {_run.Items.Count}");

        if (_run.Minefield != null)
        {
            var mine = _run.Minefield;
            sb.AppendLine();
            sb.AppendLine($"Mine seed: {mine.Seed}");
            sb.AppendLine($"Board: {mine.Width}x{mine.Height}");
            sb.AppendLine($"Visible: {mine.CountRevealed()}/{mine.Cells.Count}  Flags: {mine.CountFlags()}");
            sb.AppendLine($"Monster: {mine.CountType(MineTileType.Monster)}  Trap: {mine.CountType(MineTileType.Trap)}");
            sb.AppendLine($"Treasure: {mine.CountType(MineTileType.Treasure)}  Ore: {mine.CountType(MineTileType.Ore)}");
            sb.AppendLine($"Clear reward: {mine.RewardShards}  Trap dmg: {mine.TrapDamage}");
        }

        if (_battlePanel.Visible)
        {
            sb.AppendLine();
            sb.AppendLine($"Battle HP: {_battle.PlayerHp}/{_battle.PlayerMaxHp} vs {_battle.EnemyHp}/{_battle.Enemy.MaxHp}");
            sb.AppendLine($"Energy: {_battle.Energy}  Hand: {_battle.Hand.Count}");
        }

        _debugLabel.Text = sb.ToString();
    }

    private string FormatObjectiveStatus()
    {
        if (string.IsNullOrWhiteSpace(_run.ObjectiveId))
        {
            return Localization.Language == Localization.English ? "None" : "无";
        }

        var state = _run.IsObjectiveComplete()
            ? (Localization.Language == Localization.English ? "Done" : "完成")
            : $"{Math.Min(_run.GetObjectiveProgress(), _run.ObjectiveTarget)}/{_run.ObjectiveTarget}";
        return $"{_run.DisplayObjectiveTitle()} {state} (+{_run.ObjectiveEmberReward})";
    }

    private void RenderItems()
    {
        ClearBox(_itemList);
        AddInventoryHeader(Localization.Language == Localization.English ? "Relics" : "遗物");
        if (_run.Relics.Count == 0)
        {
            AddInventoryEmpty(Localization.Language == Localization.English ? "No relics yet." : "暂无遗物。");
        }
        else
        {
            foreach (var relicId in _run.Relics)
            {
                AddRelicInfoButton(relicId);
            }
        }

        AddInventoryHeader(Localization.Language == Localization.English ? "Items" : "道具");
        var hasItems = false;
        foreach (var stack in _run.Items)
        {
            if (stack.Count <= 0)
            {
                continue;
            }

            hasItems = true;
            var item = _gameData.GetItem(stack.ItemId);
            var selected = _selectedItemId == item.Id;
            var button = new Button
            {
                Text = $"{item.DisplayName()} x{stack.Count}\n{item.DisplayDescription()}",
                CustomMinimumSize = new Vector2(0, 70),
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
            button.TooltipText = $"{item.DisplayName()} x{stack.Count}\n{item.DisplayDescription()}\n{FormatItemUseHint(item)}";
            StyleButton(button, selected ? Color.FromHtml("5b4a2a") : Color.FromHtml("263445"), Color.FromHtml("eef5ff"));
            button.Pressed += () => OnItemPressed(item);
            _itemList.AddChild(button);
        }

        if (!hasItems)
        {
            AddInventoryEmpty(Localization.Language == Localization.English ? "No usable items." : "暂无可用道具。");
        }
    }

    private void AddInventoryHeader(string text)
    {
        var label = new Label
        {
            Text = text,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        label.AddThemeFontSizeOverride("font_size", 17);
        label.AddThemeColorOverride("font_color", Color.FromHtml("f4f0df"));
        _itemList.AddChild(label);
    }

    private void AddInventoryEmpty(string text)
    {
        var label = new Label
        {
            Text = text,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        label.AddThemeColorOverride("font_color", Color.FromHtml("8f9aa8"));
        _itemList.AddChild(label);
    }

    private void AddRelicInfoButton(string relicId)
    {
        try
        {
            var relic = _gameData.GetRelic(relicId);
            var button = new Button
            {
                Text = $"{FormatRelicRarity(relic)} | {relic.DisplayName()}\n{relic.DisplayDescription()}",
                CustomMinimumSize = new Vector2(0, 78),
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
                TooltipText = BuildRelicTooltip(relic)
            };
            StyleButton(button, GetRelicBackground(relic), Color.FromHtml("f0e4ff"));
            button.Pressed += () => ShowRelicDetails(relic);
            _itemList.AddChild(button);
        }
        catch (InvalidOperationException)
        {
            AddInventoryEmpty(relicId);
        }
    }

    private void ShowRelicDetails(RelicData relic)
    {
        _roomTitleLabel.Text = Localization.Language == Localization.English ? "Relic Details" : "遗物详情";
        _roomDescriptionLabel.Text = BuildRelicTooltip(relic);
    }

    private static string FormatRelicRarity(RelicData relic)
    {
        return relic.Rarity switch
        {
            "rare" => Localization.Language == Localization.English ? "Rare" : "稀有",
            "uncommon" => Localization.Language == Localization.English ? "Uncommon" : "罕见",
            _ => Localization.Language == Localization.English ? "Common" : "普通"
        };
    }

    private static Color GetRelicBackground(RelicData relic)
    {
        return relic.Rarity switch
        {
            "rare" => Color.FromHtml("5b4a2a"),
            "uncommon" => Color.FromHtml("392d4b"),
            _ => Color.FromHtml("303946")
        };
    }

    private string BuildRelicTooltip(RelicData relic)
    {
        var lines = new List<string>
        {
            $"{FormatRelicRarity(relic)} | {relic.DisplayName()}",
            relic.DisplayDescription()
        };

        foreach (var effect in relic.Effects)
        {
            lines.Add(FormatRelicEffect(effect));
        }

        return string.Join("\n", lines);
    }

    private static string FormatRelicEffect(RelicEffectData effect)
    {
        var value = effect.Value;
        return effect.Type switch
        {
            "battle_start_block" => Localization.Language == Localization.English ? $"Battle start: block +{value}." : $"战斗开始：格挡 +{value}。",
            "battle_start_draw" => Localization.Language == Localization.English ? $"Battle start: draw +{value}." : $"战斗开始：抽牌 +{value}。",
            "battle_start_energy" => Localization.Language == Localization.English ? $"First turn: energy +{value}." : $"首回合：能量 +{value}。",
            "trap_damage_reduction" => Localization.Language == Localization.English ? $"Trap damage -{value}." : $"陷阱伤害 -{value}。",
            "reward_shards_percent" => Localization.Language == Localization.English ? $"Battle reward shards +{value}%." : $"战斗奖励矿晶 +{value}%。",
            "reward_card_bonus" => Localization.Language == Localization.English ? $"Card reward choices +{value}." : $"卡牌奖励选项 +{value}。",
            "mine_start_preview" => Localization.Language == Localization.English ? $"Preview {value} mine tile at survey start." : $"进入矿区时预览 {value} 个矿格。",
            "post_battle_fog_reduction" => Localization.Language == Localization.English ? $"After battle: fog pressure -{value}." : $"战斗后：雾压 -{value}。",
            "post_battle_lamp" => Localization.Language == Localization.English ? $"After battle: oil +{value}." : $"战斗后：灯油 +{value}。",
            "max_lamp" => Localization.Language == Localization.English ? $"Max oil +{value}." : $"灯油上限 +{value}。",
            "player_damage_bonus" => Localization.Language == Localization.English ? $"Player attack damage +{value}." : $"玩家攻击伤害 +{value}。",
            "player_block_bonus" => Localization.Language == Localization.English ? $"Player block from cards +{value}." : $"卡牌获得格挡 +{value}。",
            "self_damage_reduction" => Localization.Language == Localization.English ? $"Self damage -{value}." : $"自伤 -{value}。",
            _ => Localization.Language == Localization.English ? $"{effect.Type}: {value}" : $"{effect.Type}：{value}"
        };
    }

    private static string FormatItemUseHint(ItemData item)
    {
        return item.UseMode switch
        {
            "instant_heal" => Localization.Language == Localization.English ? "Click to use immediately." : "点击后立即使用。",
            "target_tile" => Localization.Language == Localization.English ? "Click to select, then choose a mine tile." : "点击选中后，再选择一个矿格。",
            _ => Localization.Language == Localization.English ? "Item effect is passive or contextual." : "该道具效果会在特定场景中生效。"
        };
    }

    private void OnItemPressed(ItemData item)
    {
        if (item.UseMode == "instant_heal")
        {
            _run.UseInstantItem(item);
        }
        else if (item.UseMode == "target_tile")
        {
            _selectedItemId = _selectedItemId == item.Id ? string.Empty : item.Id;
        }

        RenderMinefield();
        RenderShared();
    }

    private string FormatRelicList()
    {
        if (_run.Relics.Count == 0)
        {
            return Localization.Language == Localization.English ? "None" : "无";
        }

        var names = new List<string>();
        foreach (var relicId in _run.Relics)
        {
            try
            {
                names.Add(_gameData.GetRelic(relicId).DisplayName());
            }
            catch (InvalidOperationException)
            {
                names.Add(relicId);
            }
        }

        return string.Join(", ", names);
    }

    private void ShowEndPanel(bool victory, string reason)
    {
        var meta = _metaRecorded ? SaveManager.LoadMeta() : SaveManager.RecordRun(_run, victory);
        _metaRecorded = true;
        HideInteractivePanels();
        _endPanel.Visible = true;
        _roomTitleLabel.Text = victory ? "探索完成" : "探索失败";
        _roomDescriptionLabel.Text = reason;
        _endTitleLabel.Text = victory ? "本次探索完成" : "本次探索结束";
        _endSummaryLabel.Text = $"抵达层数: {Math.Max(_run.CurrentLayerIndex + 1, 0)}\n剩余 HP: {_run.PlayerHp}/{_run.PlayerMaxHp}\n灯油: {_run.LampOil}/{_run.MaxLampOil}  雾压: {_run.FogPressure}\n矿晶: {_run.Shards}  分数: {_run.Score}\n战斗胜利: {_run.BattlesWon}  清理矿区: {_run.MinesCleared}\n委托: {FormatObjectiveStatus()}  委托奖励: {meta.LastObjectiveBonus}\n本局获得余烬: {meta.LastEarnedEmbers}  总余烬: {meta.TotalEmbers}\n最佳深度: {meta.BestDepth}  最佳分数: {meta.BestScore}\n牌组: {_run.PlayerDeck.Count} 张\n遗物: {FormatRelicList()}";
        RenderShared();
    }

    private void HideInteractivePanels()
    {
        _choicePanel.Visible = false;
        _minePanel.Visible = false;
        _battlePanel.Visible = false;
        _rewardPanel.Visible = false;
        _endPanel.Visible = false;
        _continueButton.Visible = false;
        ClearBox(_choiceList);
        ClearBox(_mineGrid);
        ClearBox(_handBox);
        ClearBox(_rewardList);
    }

    private static void ClearBox(Container container)
    {
        foreach (var child in container.GetChildren())
        {
            container.RemoveChild(child);
            child.QueueFree();
        }
    }

    private GridContainer CreateShopGrid()
    {
        var grid = new GridContainer
        {
            Name = "ShopGrid",
            Columns = 2,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        grid.AddThemeConstantOverride("h_separation", 12);
        grid.AddThemeConstantOverride("v_separation", 12);
        return grid;
    }

    private void AddChoiceButton(string text, System.Action handler)
    {
        var button = new Button
        {
            Text = text,
            CustomMinimumSize = _choiceButtonTarget == null ? new Vector2(0, 68) : new Vector2(340, 132),
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        StyleButton(button, Color.FromHtml("263445"), Color.FromHtml("d8e2ee"));
        button.Pressed += handler;
        (_choiceButtonTarget ?? _choiceList).AddChild(button);
    }

    private void AddShopChoiceButton(GridContainer grid, string text, System.Action handler, string category)
    {
        var button = new Button
        {
            Text = text,
            CustomMinimumSize = new Vector2(340, 132),
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };

        var background = category switch
        {
            "card" => Color.FromHtml("233548"),
            "relic" => Color.FromHtml("392d4b"),
            "tool" => Color.FromHtml("2f4c54"),
            "service" => Color.FromHtml("3a3140"),
            "exit" => Color.FromHtml("403547"),
            _ => Color.FromHtml("263445")
        };
        var font = category == "relic" || category == "exit"
            ? Color.FromHtml("f0e4ff")
            : Color.FromHtml("d8e2ee");
        StyleButton(button, background, font);
        button.Pressed += handler;
        grid.AddChild(button);
    }

    private void RenderDeckList(bool interactive, int upgradeCost, int removeCost)
    {
        ClearBox(_choiceList);
        for (var i = 0; i < _run.PlayerDeck.Count; i++)
        {
            var card = _run.PlayerDeck[i];
            var text = $"{i + 1}. {FormatCardHeader(card)}\n{card.DisplayDescription()}";
            if (interactive && upgradeCost >= 0)
            {
                text += string.IsNullOrWhiteSpace(card.UpgradeTo)
                    ? "\n已到当前最高等级。"
                    : $"\n升级费用：{upgradeCost} 矿晶";
            }
            else if (interactive && removeCost >= 0)
            {
                text += $"\n移除费用：{removeCost} 矿晶";
            }

            var button = new Button
            {
                Text = text,
                CustomMinimumSize = new Vector2(0, 82),
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
                Disabled = !interactive || (upgradeCost >= 0 && string.IsNullOrWhiteSpace(card.UpgradeTo))
            };
            StyleButton(button, Color.FromHtml("263445"), Color.FromHtml("d8e2ee"));
            var captured = i;
            if (interactive && upgradeCost >= 0)
            {
                button.Pressed += () => OnUpgradeCardPicked(captured, upgradeCost);
            }
            else if (interactive && removeCost >= 0)
            {
                button.Pressed += () => OnRemoveCardPicked(captured, removeCost);
            }

            _choiceList.AddChild(button);
        }
    }

    private void RenderDeckUpgradeChoices(int cost)
    {
        HideInteractivePanels();
        _choicePanel.Visible = true;
        _roomTitleLabel.Text = cost <= 0 ? "锻造卡牌" : "升级卡牌";
        _roomDescriptionLabel.Text = "选择一张拥有强化版本的卡牌。强化后的卡牌会直接进入本局牌组。";
        RenderDeckList(true, cost, -1);
        AddChoiceButton("返回\n暂时不调整牌组。", () => RunLoadingTransition(
            Localization.Language == Localization.English ? "Returning to the room..." : "返回当前房间……",
            RenderCurrentRoom));
        RenderShared();
    }

    private void RenderDeckRemoveChoices(int cost)
    {
        HideInteractivePanels();
        _choicePanel.Visible = true;
        _roomTitleLabel.Text = "移除卡牌";
        _roomDescriptionLabel.Text = "删牌能让核心牌更稳定上手，但会消耗矿晶。";
        RenderDeckList(true, -1, cost);
        AddChoiceButton("返回\n暂时不调整牌组。", () => RunLoadingTransition(
            Localization.Language == Localization.English ? "Returning to the room..." : "返回当前房间……",
            RenderCurrentRoom));
        RenderShared();
    }

    private void OnUpgradeCardPicked(int deckIndex, int cost)
    {
        var before = CaptureRunTipSnapshot();
        var oldCard = _run.PlayerDeck[deckIndex];
        if (_run.UpgradeCard(deckIndex, _gameData, cost))
        {
            var newCard = _run.PlayerDeck[Math.Clamp(deckIndex, 0, _run.PlayerDeck.Count - 1)];
            ShowGameTip(Localization.Language == Localization.English
                ? $"Card upgraded\n{oldCard.DisplayName()} -> {newCard.DisplayName()}"
                : $"卡牌已强化\n{oldCard.DisplayName()} -> {newCard.DisplayName()}");
            ShowRunDeltaTips(before);
            _choicePanel.Visible = false;
            _continueButton.Visible = true;
        }

        RenderShared();
    }

    private void OnRemoveCardPicked(int deckIndex, int cost)
    {
        var before = CaptureRunTipSnapshot();
        var removed = _run.PlayerDeck[deckIndex];
        if (_run.RemoveCard(deckIndex, cost))
        {
            ShowGameTip(Localization.Language == Localization.English
                ? $"Card removed\n{removed.DisplayName()}"
                : $"卡牌已移除\n{removed.DisplayName()}");
            ShowRunDeltaTips(before);
            _choicePanel.Visible = false;
            _continueButton.Visible = true;
        }

        RenderShared();
    }

    private void RenderCurrentRoom()
    {
        HideInteractivePanels();
        var room = _run.CurrentRoom;
        if (room == null)
        {
            ShowNextRoomChoices();
            return;
        }

        if (room.Kind == "rest")
        {
            RenderRestRoom(room);
        }
        else if (room.Kind == "shop")
        {
            RenderShopRoom(room);
        }
        else
        {
            ShowNextRoomChoices();
        }

        RenderShared();
    }

    private RunTipSnapshot CaptureRunTipSnapshot()
    {
        var items = new Dictionary<string, int>();
        foreach (var stack in _run.Items)
        {
            items[stack.ItemId] = items.TryGetValue(stack.ItemId, out var count) ? count + stack.Count : stack.Count;
        }

        return new RunTipSnapshot
        {
            Hp = _run.PlayerHp,
            Shards = _run.Shards,
            LampOil = _run.LampOil,
            FogPressure = _run.FogPressure,
            Score = _run.Score,
            DeckCount = _run.PlayerDeck.Count,
            RelicCount = _run.Relics.Count,
            Items = items
        };
    }

    private void ShowRunDeltaTips(RunTipSnapshot before)
    {
        var lines = new List<string>();
        AddDeltaLine(lines, Localization.Language == Localization.English ? "HP" : "生命", _run.PlayerHp - before.Hp);
        AddDeltaLine(lines, Localization.Language == Localization.English ? "Shards" : "矿晶", _run.Shards - before.Shards);
        AddDeltaLine(lines, Localization.Language == Localization.English ? "Oil" : "灯油", _run.LampOil - before.LampOil);
        AddDeltaLine(lines, Localization.Language == Localization.English ? "Fog" : "雾压", _run.FogPressure - before.FogPressure);
        AddDeltaLine(lines, Localization.Language == Localization.English ? "Score" : "分数", _run.Score - before.Score);

        if (lines.Count > 0)
        {
            ShowGameTip(string.Join("\n", lines));
        }

        if (_run.PlayerDeck.Count > before.DeckCount)
        {
            for (var i = before.DeckCount; i < _run.PlayerDeck.Count; i++)
            {
                var card = _run.PlayerDeck[i];
                ShowGameTip(Localization.Language == Localization.English
                    ? $"New card\n{card.DisplayName()}"
                    : $"新增卡牌\n{card.DisplayName()}");
            }
        }

        if (_run.Relics.Count > before.RelicCount)
        {
            for (var i = before.RelicCount; i < _run.Relics.Count; i++)
            {
                var relic = _gameData.GetRelic(_run.Relics[i]);
                ShowGameTip(Localization.Language == Localization.English
                    ? $"New relic\n{relic.DisplayName()}"
                    : $"新增遗物\n{relic.DisplayName()}");
            }
        }

        foreach (var stack in _run.Items)
        {
            var oldCount = before.Items.TryGetValue(stack.ItemId, out var count) ? count : 0;
            var delta = stack.Count - oldCount;
            if (delta > 0)
            {
                var item = _gameData.GetItem(stack.ItemId);
                ShowGameTip(Localization.Language == Localization.English
                    ? $"Item gained\n{item.DisplayName()} x{delta}"
                    : $"获得道具\n{item.DisplayName()} x{delta}");
            }
        }
    }

    private static void AddDeltaLine(List<string> lines, string label, int delta)
    {
        if (delta != 0)
        {
            lines.Add($"{label} {FormatSigned(delta)}");
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

    private static string FormatBattleCardText(CardData card)
    {
        var rarity = card.Rarity switch
        {
            "rare" => Localization.Language == Localization.English ? "Rare" : "稀有",
            "uncommon" => Localization.Language == Localization.English ? "Uncommon" : "进阶",
            _ => Localization.Language == Localization.English ? "Common" : "普通"
        };
        var type = card.Type switch
        {
            "attack" => Localization.Language == Localization.English ? "Attack" : "攻击",
            "skill" => Localization.Language == Localization.English ? "Skill" : "技能",
            _ => card.Type
        };
        return $"[{card.Cost}] {card.DisplayName()}\n{type} / {rarity}\n{card.DisplayDescription()}";
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
            BgColor = new Color(0.02f, 0.05f, 0.07f, 0.76f)
        });

        var center = new CenterContainer
        {
            Name = "ModalCenter",
            MouseFilter = MouseFilterEnum.Pass
        };
        center.SetAnchorsPreset(LayoutPreset.FullRect);

        _deckModalPanel = new PanelContainer
        {
            Name = "DeckModalPanel",
            CustomMinimumSize = new Vector2(760, 620)
        };

        var layout = new VBoxContainer
        {
            Name = "DeckModalLayout"
        };
        layout.AddThemeConstantOverride("separation", 12);

        var title = new Label
        {
            Text = Localization.Language == Localization.English ? "Current Deck" : "当前牌组",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        title.AddThemeFontSizeOverride("font_size", 28);
        title.AddThemeColorOverride("font_color", Color.FromHtml("f4f0df"));

        var description = new Label
        {
            Text = Localization.Language == Localization.English
                ? "This is your current run deck. Upgrade and removal happen at camps and shops."
                : "这里展示当前跑局牌组。升级和删牌会在营地、商店中进行。",
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        description.AddThemeColorOverride("font_color", Color.FromHtml("b8c7d5"));

        var scroll = new ScrollContainer
        {
            Name = "DeckModalScroll",
            CustomMinimumSize = new Vector2(0, 460),
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        _deckModalList = new VBoxContainer
        {
            Name = "DeckModalList",
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        _deckModalList.AddThemeConstantOverride("separation", 8);

        _deckModalCloseButton = new Button
        {
            Text = Localization.Language == Localization.English ? "Close" : "关闭",
            CustomMinimumSize = new Vector2(0, 52)
        };

        scroll.AddChild(_deckModalList);
        layout.AddChild(title);
        layout.AddChild(description);
        layout.AddChild(scroll);
        layout.AddChild(_deckModalCloseButton);
        _deckModalPanel.AddChild(layout);
        center.AddChild(_deckModalPanel);
        _modalOverlay.AddChild(center);
        GetNode<Panel>("Root").AddChild(_modalOverlay);

        _dragHintPanel = new PanelContainer
        {
            Name = "DragHintPanel",
            Visible = false,
            MouseFilter = MouseFilterEnum.Ignore
        };
        _dragHintPanel.SetAnchorsPreset(LayoutPreset.CenterTop);
        _dragHintPanel.Position = new Vector2(-190, 22);
        _dragHintPanel.CustomMinimumSize = new Vector2(380, 46);
        _dragHintLabel = new Label
        {
            Name = "DragHintLabel",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        _dragHintPanel.AddChild(_dragHintLabel);
        GetNode<Panel>("Root").AddChild(_dragHintPanel);

        _mineRevealTipPanel = new PanelContainer
        {
            Name = "MineRevealTipPanel",
            Visible = false,
            MouseFilter = MouseFilterEnum.Ignore,
            CustomMinimumSize = new Vector2(290, 70),
            ZIndex = 120
        };
        _mineRevealTipLabel = new Label
        {
            Name = "MineRevealTipLabel",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            MouseFilter = MouseFilterEnum.Ignore
        };
        _mineRevealTipLabel.AddThemeFontSizeOverride("font_size", 18);
        _mineRevealTipPanel.AddChild(_mineRevealTipLabel);
        GetNode<Panel>("Root").AddChild(_mineRevealTipPanel);

        _dragLine = new Line2D
        {
            Name = "DragAimLine",
            Visible = false,
            Width = 4,
            DefaultColor = Color.FromHtml("d7b45f"),
            ZIndex = 80
        };
        GetNode<Panel>("Root").AddChild(_dragLine);

        BuildLoadingOverlay();
    }

    private void BuildLoadingOverlay()
    {
        _loadingOverlay = new Panel
        {
            Name = "LoadingOverlay",
            Visible = false,
            MouseFilter = MouseFilterEnum.Stop,
            ZIndex = 220
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

    private async void RunLoadingTransition(string text, Action action)
    {
        if (_loadingActive)
        {
            return;
        }

        _loadingActive = true;
        ShowLoading(text);
        await ToSignal(GetTree().CreateTimer(0.28), SceneTreeTimer.SignalName.Timeout);
        try
        {
            action();
        }
        finally
        {
            await ToSignal(GetTree().CreateTimer(0.18), SceneTreeTimer.SignalName.Timeout);
            HideLoading();
            _loadingActive = false;
        }
    }

    private async void ChangeSceneWithLoading(string scenePath, string text)
    {
        if (_loadingActive)
        {
            return;
        }

        _loadingActive = true;
        ShowLoading(text);
        await ToSignal(GetTree().CreateTimer(0.35), SceneTreeTimer.SignalName.Timeout);
        GetTree().ChangeSceneToFile(scenePath);
    }

    private void ShowLoading(string text)
    {
        CancelAimingCard();
        _loadingLabel.Text = text;
        _loadingOverlay.Modulate = Colors.White;
        _loadingOverlay.Visible = true;
    }

    private void HideLoading()
    {
        _loadingOverlay.Visible = false;
    }

    private void ShowGameTip(string text)
    {
        _tipQueue.Enqueue(text);
        if (!_tipPlaying)
        {
            PlayNextGameTip();
        }
    }

    private async void PlayNextGameTip()
    {
        if (_tipPlaying || _tipQueue.Count == 0)
        {
            return;
        }

        _tipPlaying = true;
        var text = _tipQueue.Dequeue();
        var root = GetNode<Panel>("Root");
        var tip = new PanelContainer
        {
            Name = "CenterTip",
            MouseFilter = MouseFilterEnum.Ignore,
            CustomMinimumSize = new Vector2(820, 58),
            ZIndex = 190
        };
        tip.AddThemeStyleboxOverride("panel", MakePanelStyle("121c28", "8df0bd", 1));

        var label = new Label
        {
            Text = text.Replace("\n", "    |    "),
            MouseFilter = MouseFilterEnum.Ignore,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        label.AddThemeFontSizeOverride("font_size", 18);
        label.AddThemeColorOverride("font_color", Color.FromHtml("f4f0df"));
        tip.AddChild(label);

        root.AddChild(tip);

        var width = Math.Min(920f, Math.Max(520f, root.Size.X - 120f));
        var height = 58f;
        var target = new Vector2((root.Size.X - width) / 2f, Math.Clamp(root.Size.Y * 0.28f, 84f, Math.Max(84f, root.Size.Y - 170f)));
        tip.Size = new Vector2(width, height);
        tip.Position = target + new Vector2(0, -54f);
        tip.Modulate = new Color(1f, 1f, 1f, 0f);

        var tween = CreateTween();
        tween.TweenProperty(tip, "position", target, 0.22f).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
        tween.Parallel().TweenProperty(tip, "modulate:a", 1.0f, 0.18f);
        tween.TweenInterval(1.15f);
        tween.TweenProperty(tip, "position", target + new Vector2(0, -16f), 0.45f).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
        tween.Parallel().TweenProperty(tip, "modulate:a", 0.0f, 0.45f);
        tween.TweenCallback(Callable.From(tip.QueueFree));
        await ToSignal(tween, Tween.SignalName.Finished);
        _tipPlaying = false;
        PlayNextGameTip();
    }

    private void ShowGameTipAt(string text, Vector2 globalPosition, bool centerOnPoint = true)
    {
        ShowGameTip(text);
    }

    private void ApplyUiStyle()
    {
        MistTheme.ApplyRoot(GetNode<Panel>("Root"), "run");
        ApplyLabelColor(_runStatusLabel, "d6e2ec");
        ApplyLabelColor(_roomTitleLabel, "f4f0df");
        ApplyLabelColor(_roomDescriptionLabel, "b8c7d5");
        ApplyLabelColor(_mineStatusLabel, "d6e2ec");
        ApplyLabelColor(_playerCombatLabel, "e7fff1");
        ApplyLabelColor(_enemyCombatLabel, "ffe2e6");
        ApplyLabelColor(_playerBlockLabel, "d8e2ee");
        ApplyLabelColor(_enemyBlockLabel, "ffd9df");
        ApplyLabelColor(_battleResourceLabel, "b8c7d5");
        ApplyLabelColor(_statusHelpLabel, "d8e2ee");
        ApplyLabelColor(_intentLabel, "ead7f7");
        ApplyLabelColor(_endTitleLabel, "f4f0df");
        ApplyLabelColor(_endSummaryLabel, "d6e2ec");
        ApplyLabelColor(_debugLabel, "d6e2ec");
        _logLabel.AddThemeColorOverride("default_color", Color.FromHtml("b8c7d5"));
        MistTheme.StylePanel(_topBarPanel, MistPanelVariant.Stone);
        MistTheme.StylePanel(_roomHeaderPanel, MistPanelVariant.Stone);
        MistTheme.StylePanel(_mainPanel, MistPanelVariant.Inset);
        MistTheme.StylePanel(_choicePanel, MistPanelVariant.Stone);
        MistTheme.StylePanel(_itemPanel, MistPanelVariant.Stone);
        MistTheme.StylePanel(_minePanel, MistPanelVariant.Stone);
        MistTheme.StylePanel(_battlePanel, MistPanelVariant.Stone);
        MistTheme.StylePanel(_statusHelpPanel, MistPanelVariant.Inset);
        MistTheme.StylePanel(_intentPanel, MistPanelVariant.Purple);
        MistTheme.StylePanel(_mineBoardFrame, MistPanelVariant.Inset);
        MistTheme.StylePanel(_rewardPanel, MistPanelVariant.Purple);
        MistTheme.StylePanel(_endPanel, MistPanelVariant.Stone);
        MistTheme.StylePanel(_actionPanel, MistPanelVariant.Stone);
        MistTheme.StylePanel(_logPanel, MistPanelVariant.Inset);
        MistTheme.StylePanel(_debugPanel, MistPanelVariant.Purple);
        MistTheme.StylePanel(_deckModalPanel, MistPanelVariant.Stone);
        MistTheme.StylePanel(_dragHintPanel, MistPanelVariant.Purple);
        MistTheme.StylePanel(_mineRevealTipPanel, MistPanelVariant.Purple);
        _dragHintLabel.AddThemeColorOverride("font_color", Color.FromHtml("f4f0df"));
        _mineRevealTipLabel.AddThemeColorOverride("font_color", Color.FromHtml("f4f0df"));
        StyleCombatPanels(false, false);
        StyleButton(_continueButton, Color.FromHtml("315f46"), Color.FromHtml("e7fff1"));
        StyleButton(_menuButton, Color.FromHtml("403547"), Color.FromHtml("f0e4ff"));
        StyleButton(_mineModeButton, Color.FromHtml("3a4f68"), Color.FromHtml("e4f0ff"));
        StyleButton(_deckButton, Color.FromHtml("2f4c54"), Color.FromHtml("e4fbff"));
        StyleButton(_deckModalCloseButton, Color.FromHtml("303946"), Color.FromHtml("eef5ff"));
        StyleButton(_retryButton, Color.FromHtml("315f46"), Color.FromHtml("e7fff1"));
        StyleButton(_endMenuButton, Color.FromHtml("403547"), Color.FromHtml("f0e4ff"));
        StyleProgressBar(_playerHpBar, Color.FromHtml("2f9d68"));
        StyleProgressBar(_enemyHpBar, Color.FromHtml("b8505d"));
    }

    private static StyleBoxFlat MakePanelStyle(string background, string border, int borderWidth)
    {
        var style = new StyleBoxFlat
        {
            BgColor = Color.FromHtml(background),
            BorderColor = Color.FromHtml(border),
            CornerRadiusTopLeft = 6,
            CornerRadiusTopRight = 6,
            CornerRadiusBottomLeft = 6,
            CornerRadiusBottomRight = 6,
            ContentMarginLeft = 10,
            ContentMarginTop = 10,
            ContentMarginRight = 10,
            ContentMarginBottom = 10
        };
        style.SetBorderWidthAll(borderWidth);
        return style;
    }

    private static void StyleButton(Button button, Color background, Color fontColor)
    {
        var normal = MakeButtonStyle(background);
        var hover = MakeButtonStyle(background.Lightened(0.12f));
        var pressed = MakeButtonStyle(background.Darkened(0.12f));
        button.AddThemeStyleboxOverride("normal", normal);
        button.AddThemeStyleboxOverride("hover", hover);
        button.AddThemeStyleboxOverride("pressed", pressed);
        button.AddThemeStyleboxOverride("disabled", MakeButtonStyle(background.Darkened(0.24f)));
        button.AddThemeColorOverride("font_color", fontColor);
        button.AddThemeColorOverride("font_hover_color", fontColor.Lightened(0.08f));
        button.AddThemeColorOverride("font_pressed_color", fontColor);
        button.AddThemeColorOverride("font_disabled_color", fontColor.Darkened(0.35f));
    }

    private static void StyleRouteNodeButton(Button button, RunRoom room, bool current, bool selectable, bool past, bool onRoute)
    {
        var kindColor = GetRouteKindColor(room.Kind);
        var background = kindColor.Darkened(0.66f);
        var border = kindColor.Darkened(0.08f);
        var font = kindColor.Lightened(0.45f);
        var borderWidth = 2;

        if (past || !onRoute)
        {
            background = Color.FromHtml("1b2027");
            border = Color.FromHtml("303944");
            font = Color.FromHtml("667382");
        }
        else if (current)
        {
            background = Color.FromHtml("5b4a2a");
            border = Color.FromHtml("f0cf87");
            font = Color.FromHtml("fff1d0");
            borderWidth = 3;
        }
        else if (selectable)
        {
            background = kindColor.Darkened(room.Risk >= 3 ? 0.50f : 0.58f);
            border = kindColor.Lightened(0.25f);
            font = Color.FromHtml("e7fff1");
            borderWidth = 3;
        }
        else
        {
            background = kindColor.Darkened(0.72f);
            border = kindColor.Darkened(0.30f);
            font = Color.FromHtml("b9c9d8");
        }

        var normal = MakeRouteNodeStyle(background, border, borderWidth);
        var hover = MakeRouteNodeStyle(background.Lightened(0.08f), border.Lightened(0.12f), borderWidth);
        var pressed = MakeRouteNodeStyle(background.Darkened(0.08f), border, borderWidth);
        button.AddThemeStyleboxOverride("normal", normal);
        button.AddThemeStyleboxOverride("hover", hover);
        button.AddThemeStyleboxOverride("pressed", pressed);
        button.AddThemeStyleboxOverride("disabled", normal);
        button.AddThemeColorOverride("font_color", font);
        button.AddThemeColorOverride("font_hover_color", font.Lightened(0.08f));
        button.AddThemeColorOverride("font_pressed_color", font);
        button.AddThemeColorOverride("font_disabled_color", font);
    }

    private static Color GetRouteKindColor(string kind)
    {
        return kind switch
        {
            "battle" => Color.FromHtml("d36a72"),
            "elite" => Color.FromHtml("ff9aa8"),
            "mine" => Color.FromHtml("72a7d8"),
            "treasure" => Color.FromHtml("d7b45f"),
            "event" => Color.FromHtml("a99bd3"),
            "rest" => Color.FromHtml("78d69b"),
            "shop" => Color.FromHtml("66d4df"),
            "calibration" => Color.FromHtml("8df0bd"),
            "complete" => Color.FromHtml("f4f0df"),
            _ => Color.FromHtml("8f9aa8")
        };
    }

    private static void StyleEnemyTargetButton(Button button, BattleEnemyState enemy, bool selected)
    {
        var background = enemy.IsAlive
            ? selected ? Color.FromHtml("3d2830") : Color.FromHtml("241d25")
            : Color.FromHtml("20242b");
        var border = enemy.IsAlive
            ? selected ? Color.FromHtml("ff9aa8") : Color.FromHtml("6b3b4a")
            : Color.FromHtml("384351");
        var font = enemy.IsAlive ? Color.FromHtml("ffe2e6") : Color.FromHtml("667382");
        var normal = MakeCardStyle(background, border);
        var hover = MakeCardStyle(background.Lightened(0.08f), border.Lightened(0.12f));
        var pressed = MakeCardStyle(background.Darkened(0.08f), border);
        button.AddThemeStyleboxOverride("normal", normal);
        button.AddThemeStyleboxOverride("hover", hover);
        button.AddThemeStyleboxOverride("pressed", pressed);
        button.AddThemeStyleboxOverride("disabled", normal);
        button.AddThemeColorOverride("font_color", font);
        button.AddThemeColorOverride("font_hover_color", font.Lightened(0.08f));
        button.AddThemeColorOverride("font_pressed_color", font);
        button.AddThemeColorOverride("font_disabled_color", font);
    }

    private static void StyleCardButton(Button button, CardData card, bool playable)
    {
        var border = card.Rarity switch
        {
            "rare" => Color.FromHtml("d7b45f"),
            "uncommon" => Color.FromHtml("78a8d8"),
            _ => Color.FromHtml("7d8a96")
        };
        var background = card.Type == "attack" ? Color.FromHtml("2b2324") : Color.FromHtml("1d2b35");
        if (!playable)
        {
            background = Color.FromHtml("252b32");
            border = Color.FromHtml("4a5664");
        }

        var normal = MakeCardStyle(background, border);
        var hover = MakeCardStyle(background.Lightened(0.08f), border.Lightened(0.12f));
        var pressed = MakeCardStyle(background.Darkened(0.08f), border);
        var font = playable ? Color.FromHtml("f3ead7") : Color.FromHtml("8f9aa8");
        button.AddThemeStyleboxOverride("normal", normal);
        button.AddThemeStyleboxOverride("hover", hover);
        button.AddThemeStyleboxOverride("pressed", pressed);
        button.AddThemeStyleboxOverride("disabled", normal);
        button.AddThemeColorOverride("font_color", font);
        button.AddThemeColorOverride("font_hover_color", font.Lightened(0.08f));
        button.AddThemeColorOverride("font_pressed_color", font);
        button.AddThemeColorOverride("font_disabled_color", font);
    }

    private void StyleCombatPanels(bool highlightPlayer, bool highlightEnemy)
    {
        _playerCombatPanel.AddThemeStyleboxOverride("panel", MakePanelStyle(
            highlightPlayer ? "1f3b31" : "14243a",
            highlightPlayer ? "78d69b" : "38506a",
            highlightPlayer ? 3 : 1));
        _enemyCombatPanel.AddThemeStyleboxOverride("panel", MakePanelStyle(
            highlightEnemy ? "3d2229" : "2b1d24",
            highlightEnemy ? "ff7f8f" : "6b3b4a",
            highlightEnemy ? 3 : 1));
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

    private static StyleBoxFlat MakeRouteNodeStyle(Color background, Color border, int borderWidth)
    {
        var style = new StyleBoxFlat
        {
            BgColor = background,
            BorderColor = border,
            CornerRadiusTopLeft = 7,
            CornerRadiusTopRight = 7,
            CornerRadiusBottomLeft = 7,
            CornerRadiusBottomRight = 7,
            ContentMarginLeft = 7,
            ContentMarginTop = 7,
            ContentMarginRight = 7,
            ContentMarginBottom = 7,
            ShadowColor = new Color(0, 0, 0, 0.26f),
            ShadowSize = 3
        };
        style.SetBorderWidthAll(borderWidth);
        return style;
    }

    private static StyleBoxFlat MakeRouteLegendChipStyle(Color color)
    {
        var style = new StyleBoxFlat
        {
            BgColor = color.Darkened(0.68f),
            BorderColor = color.Darkened(0.12f),
            CornerRadiusTopLeft = 6,
            CornerRadiusTopRight = 6,
            CornerRadiusBottomLeft = 6,
            CornerRadiusBottomRight = 6,
            ContentMarginLeft = 8,
            ContentMarginTop = 4,
            ContentMarginRight = 8,
            ContentMarginBottom = 4
        };
        style.SetBorderWidthAll(1);
        return style;
    }

    private static void ApplyLabelColor(Label label, string color)
    {
        label.AddThemeColorOverride("font_color", Color.FromHtml(color));
    }

    private static StyleBoxFlat MakeButtonStyle(Color background)
    {
        var style = new StyleBoxFlat
        {
            BgColor = background,
            CornerRadiusTopLeft = 5,
            CornerRadiusTopRight = 5,
            CornerRadiusBottomLeft = 5,
            CornerRadiusBottomRight = 5,
            ContentMarginLeft = 8,
            ContentMarginTop = 6,
            ContentMarginRight = 8,
            ContentMarginBottom = 6
        };
        style.SetBorderWidthAll(1);
        style.BorderColor = background.Lightened(0.18f);
        return style;
    }

    private static void StyleProgressBar(ProgressBar bar, Color fill)
    {
        var background = new StyleBoxFlat
        {
            BgColor = Color.FromHtml("0d1218"),
            CornerRadiusTopLeft = 5,
            CornerRadiusTopRight = 5,
            CornerRadiusBottomLeft = 5,
            CornerRadiusBottomRight = 5
        };
        var foreground = new StyleBoxFlat
        {
            BgColor = fill,
            CornerRadiusTopLeft = 5,
            CornerRadiusTopRight = 5,
            CornerRadiusBottomLeft = 5,
            CornerRadiusBottomRight = 5
        };
        bar.AddThemeStyleboxOverride("background", background);
        bar.AddThemeStyleboxOverride("fill", foreground);
    }

    private static void StyleMineButton(Button button, MineCell cell)
    {
        if (!cell.IsRevealed)
        {
            var hiddenColor = cell.IsPreviewed ? Color.FromHtml("334e68") : Color.FromHtml("253242");
            StyleButton(button, cell.IsFlagged ? Color.FromHtml("5b4a2a") : hiddenColor, Color.FromHtml("eef5ff"));
            return;
        }

        var color = cell.Type switch
        {
            MineTileType.Monster => Color.FromHtml("6b2d35"),
            MineTileType.Trap => Color.FromHtml("74412b"),
            MineTileType.Treasure => Color.FromHtml("2b5d74"),
            MineTileType.Ore => Color.FromHtml("466335"),
            MineTileType.Exit => Color.FromHtml("2e6a52"),
            MineTileType.Entrance => Color.FromHtml("38506a"),
            _ => Color.FromHtml("d8dee8")
        };
        var font = cell.Type == MineTileType.Empty ? Color.FromHtml("1e2a36") : Color.FromHtml("fff7e6");
        StyleButton(button, color, font);
    }

    private static string GetPreviewText(MineCell cell)
    {
        return cell.Type switch
        {
            MineTileType.Empty => $"D{cell.DangerClue}\nR{cell.RewardClue}",
            MineTileType.Entrance => "IN",
            MineTileType.Exit => "OUT",
            MineTileType.Monster => "M?",
            MineTileType.Trap => "T?",
            MineTileType.Treasure => "BOX?",
            MineTileType.Ore => "ORE?",
            _ => "?"
        };
    }

    private sealed class CalibrationNode
    {
        public CalibrationNode(string id, string name, string nameEn, string hint, string hintEn, int shards = 0, int oil = 0, int fogReduction = 0, int heal = 0, string cardId = "", int damage = 0, int fogGain = 0, string[]? tags = null)
        {
            Id = id;
            Name = name;
            NameEn = nameEn;
            Hint = hint;
            HintEn = hintEn;
            Shards = shards;
            Oil = oil;
            FogReduction = fogReduction;
            Heal = heal;
            CardId = cardId;
            Damage = damage;
            FogGain = fogGain;
            Tags = tags == null ? new List<string>() : new List<string>(tags);
        }

        public string Id { get; }
        public string Name { get; }
        public string NameEn { get; }
        public string Hint { get; }
        public string HintEn { get; }
        public int Shards { get; }
        public int Oil { get; }
        public int FogReduction { get; }
        public int Heal { get; }
        public string CardId { get; }
        public int Damage { get; }
        public int FogGain { get; }
        public List<string> Tags { get; }

        public string DisplayName() => Localization.Pick(Name, NameEn);
        public string DisplayHint() => Localization.Pick(Hint, HintEn);
    }

    private sealed class CalibrationOutcome
    {
        public int Shards { get; set; }
        public int Oil { get; set; }
        public int FogReduction { get; set; }
        public int Heal { get; set; }
        public string CardId { get; set; } = string.Empty;
        public int Damage { get; set; }
        public int FogGain { get; set; }
        public List<string> Notes { get; } = new();
    }

    private sealed class RunTipSnapshot
    {
        public int Hp { get; set; }
        public int Shards { get; set; }
        public int LampOil { get; set; }
        public int FogPressure { get; set; }
        public int Score { get; set; }
        public int DeckCount { get; set; }
        public int RelicCount { get; set; }
        public Dictionary<string, int> Items { get; set; } = new();
    }
}
