using Godot;
using System;
using System.Text;

public partial class BattleScene : Control
{
    private readonly GameData _gameData = new();
    private readonly BattleEngine _battle = new();
    private readonly RunEngine _run = new();

    private Label _runStatusLabel = null!;
    private Label _roomTitleLabel = null!;
    private Label _roomDescriptionLabel = null!;
    private PanelContainer _choicePanel = null!;
    private VBoxContainer _choiceList = null!;
    private PanelContainer _minePanel = null!;
    private Label _mineStatusLabel = null!;
    private Button _mineModeButton = null!;
    private GridContainer _mineGrid = null!;
    private PanelContainer _battlePanel = null!;
    private Label _playerCombatLabel = null!;
    private Label _enemyCombatLabel = null!;
    private TextureRect _playerPortrait = null!;
    private TextureRect _enemyPortrait = null!;
    private ProgressBar _playerHpBar = null!;
    private ProgressBar _enemyHpBar = null!;
    private Label _playerBlockLabel = null!;
    private Label _enemyBlockLabel = null!;
    private Label _battleResourceLabel = null!;
    private Label _intentLabel = null!;
    private VBoxContainer _handBox = null!;
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

    private RunRoom? _activeBattleRoom;
    private RunRoom? _activeMineRoom;
    private bool _returnToMineAfterBattle;
    private bool _mineFlagMode;
    private bool _debugVisible;
    private bool _metaRecorded;
    private string _selectedItemId = string.Empty;
    private bool _deckReturnBattle;
    private bool _deckReturnMine;
    private bool _deckReturnReward;
    private bool _deckReturnContinue;

    public override void _Ready()
    {
        Localization.LoadSettings();

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
        _playerPortrait = GetNode<TextureRect>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/BattlePanel/BattleLayout/CombatantRow/PlayerCombatPanel/PlayerCombatLayout/PlayerPortrait");
        _enemyPortrait = GetNode<TextureRect>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/BattlePanel/BattleLayout/CombatantRow/EnemyCombatPanel/EnemyCombatLayout/EnemyPortrait");
        _playerCombatLabel = GetNode<Label>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/BattlePanel/BattleLayout/CombatantRow/PlayerCombatPanel/PlayerCombatLayout/PlayerCombatLabel");
        _enemyCombatLabel = GetNode<Label>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/BattlePanel/BattleLayout/CombatantRow/EnemyCombatPanel/EnemyCombatLayout/EnemyCombatLabel");
        _playerHpBar = GetNode<ProgressBar>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/BattlePanel/BattleLayout/CombatantRow/PlayerCombatPanel/PlayerCombatLayout/PlayerHpBar");
        _enemyHpBar = GetNode<ProgressBar>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/BattlePanel/BattleLayout/CombatantRow/EnemyCombatPanel/EnemyCombatLayout/EnemyHpBar");
        _playerBlockLabel = GetNode<Label>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/BattlePanel/BattleLayout/CombatantRow/PlayerCombatPanel/PlayerCombatLayout/PlayerBlockLabel");
        _enemyBlockLabel = GetNode<Label>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/BattlePanel/BattleLayout/CombatantRow/EnemyCombatPanel/EnemyCombatLayout/EnemyBlockLabel");
        _battleResourceLabel = GetNode<Label>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/BattlePanel/BattleLayout/BattleResourceLabel");
        _intentLabel = GetNode<Label>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/BattlePanel/BattleLayout/IntentPanel/IntentLabel");
        _handBox = GetNode<VBoxContainer>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/BattlePanel/BattleLayout/HandPanel/HandList");
        _endTurnButton = GetNode<Button>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/BattlePanel/BattleLayout/EndTurnButton");
        _deckButton = GetNode<Button>("Root/Margin/MainLayout/ContentSplit/SidePanel/ActionPanel/ActionLayout/DeckButton");
        _rewardPanel = GetNode<PanelContainer>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/RewardPanel");
        _rewardList = GetNode<VBoxContainer>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/RewardPanel/RewardList");
        _continueButton = GetNode<Button>("Root/Margin/MainLayout/ContentSplit/SidePanel/ActionPanel/ActionLayout/ContinueButton");
        _menuButton = GetNode<Button>("Root/Margin/MainLayout/TopBar/TopBarLayout/MenuButton");
        _endPanel = GetNode<PanelContainer>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/EndPanel");
        _endTitleLabel = GetNode<Label>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/EndPanel/EndLayout/EndTitleLabel");
        _endSummaryLabel = GetNode<Label>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/EndPanel/EndLayout/EndSummaryLabel");
        _retryButton = GetNode<Button>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/EndPanel/EndLayout/EndButtonRow/RetryButton");
        _endMenuButton = GetNode<Button>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/EndPanel/EndLayout/EndButtonRow/EndMenuButton");
        _logLabel = GetNode<RichTextLabel>("Root/Margin/MainLayout/ContentSplit/SidePanel/LogPanel/LogText");
        _debugPanel = GetNode<PanelContainer>("Root/Margin/MainLayout/ContentSplit/SidePanel/DebugPanel");
        _debugLabel = GetNode<Label>("Root/Margin/MainLayout/ContentSplit/SidePanel/DebugPanel/DebugLabel");

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
        _continueButton.Pressed += OnContinuePressed;
        _mineModeButton.Pressed += OnMineModePressed;
        _menuButton.Pressed += OnMenuPressed;
        _retryButton.Pressed += OnRetryPressed;
        _endMenuButton.Pressed += OnEndMenuPressed;
        _continueButton.Text = Localization.T("continue_deeper");
        _deckButton.Text = Localization.Language == Localization.English ? "View Deck" : "查看牌组";
        _menuButton.Text = Localization.T("back_menu");
        ApplyUiStyle();

        ShowNextRoomChoices();
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

    private void OnContinuePressed()
    {
        ShowNextRoomChoices();
    }

    private void OnMenuPressed()
    {
        SaveManager.SaveRun(_run);
        GetTree().ChangeSceneToFile("res://scenes/MainMenu.tscn");
    }

    private void OnDeckPressed()
    {
        _deckReturnBattle = _battlePanel.Visible;
        _deckReturnMine = _minePanel.Visible;
        _deckReturnReward = _rewardPanel.Visible;
        _deckReturnContinue = _continueButton.Visible;
        HideInteractivePanels();
        _choicePanel.Visible = true;
        _roomTitleLabel.Text = Localization.Language == Localization.English ? "Current Deck" : "当前牌组";
        _roomDescriptionLabel.Text = Localization.Language == Localization.English
            ? "Cards are grouped by copy. Upgrade and removal happen at camps and shops."
            : "这里显示本局当前牌组。升级和删牌会在营地、商店中进行。";
        RenderDeckList(false, 0, 0);
        AddChoiceButton(Localization.Language == Localization.English ? "Back\nReturn to the current screen." : "返回\n回到当前界面。", RestoreDeckReturn);
        RenderShared();
    }

    private void RestoreDeckReturn()
    {
        HideInteractivePanels();
        if (_deckReturnBattle)
        {
            _battlePanel.Visible = true;
            RenderBattle();
        }
        else if (_deckReturnMine)
        {
            _minePanel.Visible = true;
            RenderMinefield();
        }
        else if (_deckReturnReward)
        {
            ShowBattleReward();
        }
        else if (_deckReturnContinue)
        {
            _continueButton.Visible = true;
        }
        else
        {
            RenderCurrentRoom();
            return;
        }

        RenderShared();
    }

    private void OnRetryPressed()
    {
        GameSession.LoadRequested = false;
        GetTree().ChangeSceneToFile("res://scenes/CharacterSelect.tscn");
    }

    private void OnEndMenuPressed()
    {
        GetTree().ChangeSceneToFile("res://scenes/MainMenu.tscn");
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

        for (var i = 0; i < choices.Count; i++)
        {
            var room = choices[i];
            var button = new Button
            {
                Text = $"{GetRoomIcon(room.Kind)} {room.DisplayTitle()}  {FormatRoomRisk(room)}\n{GetRoomSummary(room)}",
                CustomMinimumSize = new Vector2(0, 82),
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
            StyleButton(button, Color.FromHtml("263445"), Color.FromHtml("d8e2ee"));
            var captured = i;
            button.Pressed += () => EnterNextRoom(captured);
            _choiceList.AddChild(button);
        }

        RenderShared();
    }

    private void EnterNextRoom(int choiceIndex)
    {
        var room = _run.EnterNextRoom(choiceIndex);
        _activeBattleRoom = null;
        _activeMineRoom = null;
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
                StartBattleRoom(room);
                break;
            case "rest":
                RenderRestRoom(room);
                break;
            case "shop":
                RenderShopRoom(room);
                break;
            case "complete":
                ShowEndPanel(true, "抵达矿井深处");
                break;
        }

        RenderShared();
    }

    private static string GetRoomIcon(string kind)
    {
        return kind switch
        {
            "battle" => Localization.T("room_battle"),
            "mine" => Localization.T("room_mine"),
            "event" => Localization.T("room_event"),
            "rest" => Localization.T("room_rest"),
            "shop" => Localization.T("room_shop"),
            "complete" => Localization.T("room_complete"),
            _ => "[未知]"
        };
    }

    private string GetRoomSummary(RunRoom room)
    {
        return room.Kind switch
        {
            "battle" => string.Format(Localization.T("encounter_reward"), _gameData.GetEnemy(room.EnemyId).DisplayName()),
            "mine" => FormatMineSummary(room.MineConfig),
            "event" => _gameData.GetEvent(room.EventId).DisplayDescription(),
            "rest" => Localization.T("rest_summary"),
            "shop" => Localization.T("shop_summary"),
            "complete" => Localization.T("complete_summary"),
            _ => Localization.T("unknown_room")
        };
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
        _run.StartMinefield();
        _mineFlagMode = false;
        _roomTitleLabel.Text = room.DisplayTitle();
        _roomDescriptionLabel.Text = FormatMineIntro(room.MineConfig);
        _minePanel.Visible = true;
        RenderMinefield();
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
            revealResult = _run.RevealMineCell(index);
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
            StartMineMonsterBattle();
            RenderShared();
            return;
        }

        RenderMinefield();
        RenderShared();
    }

    private void StartMineMonsterBattle()
    {
        var enemyId = string.IsNullOrEmpty(_activeMineRoom?.EnemyId) ? "slime" : _activeMineRoom.EnemyId;
        var enemy = _gameData.GetEnemy(enemyId);
        _battle.StartBattle(_run.PlayerDeck, enemy, _run.PlayerMaxHp, _run.PlayerHp);
        SetCombatPortraits(enemy);
        _returnToMineAfterBattle = true;

        _minePanel.Visible = false;
        _battlePanel.Visible = true;
        _roomTitleLabel.Text = $"{Localization.T("room_battle")} {enemy.DisplayName()}";
        _roomDescriptionLabel.Text = "矿格里的怪物拦住了去路。击退它后可以回到当前矿区继续探索。";
        RenderBattle();
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
        _run.ApplyEventChoice(choice, _gameData);
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
        AddChoiceButton("锻造一张牌\n免费升级牌组中一张可升级卡牌。", () => RenderDeckUpgradeChoices(0));
        AddChoiceButton("整备\n获得 12 矿晶和遗物：校准矿灯。", () => OnRestChoicePressed("forge"));
    }

    private void OnRestChoicePressed(string mode)
    {
        _run.Rest(mode);
        _choicePanel.Visible = false;
        _continueButton.Visible = true;
        RenderShared();
    }

    private void RenderShopRoom(RunRoom room)
    {
        _roomTitleLabel.Text = room.DisplayTitle();
        _roomDescriptionLabel.Text = "一盏挂满铜铃的矿灯在轨道旁摇晃。商队愿意收矿晶，也愿意卖一点生路。";
        _choicePanel.Visible = true;

        var reward = _gameData.GetReward(room.RewardId);
        var shopCards = _gameData.BuildRewardChoices(reward, GameSession.SelectedCharacterId, HashCode.Combine(_run.RunSeed, _run.CurrentLayerIndex, "shop"), 4);
        foreach (var card in shopCards)
        {
            AddChoiceButton($"{string.Format(Localization.T("buy_card"), card.DisplayName(), 22)}\n{card.DisplayDescription()}", () => OnBuyCardPressed(card));
        }

        AddChoiceButton("购买治疗 - 16 矿晶\n恢复 18 点生命。", OnBuyHealPressed);
        AddChoiceButton("升级一张牌 - 18 矿晶\n让已有卡牌变成强化版本。", () => RenderDeckUpgradeChoices(18));
        AddChoiceButton("移除一张牌 - 20 矿晶\n精简牌组，提高关键牌上手率。", () => RenderDeckRemoveChoices(20));
        AddChoiceButton("离开商队\n保留矿晶继续深入。", OnLeaveShopPressed);
    }

    private void OnBuyCardPressed(CardData card)
    {
        if (_run.BuyCard(card, 22))
        {
            _choicePanel.Visible = false;
            _continueButton.Visible = true;
        }

        RenderShared();
    }

    private void OnBuyHealPressed()
    {
        if (_run.BuyHeal(18, 16))
        {
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
    }

    private void StartBattleRoom(RunRoom room)
    {
        _activeBattleRoom = room;
        _returnToMineAfterBattle = false;
        var enemy = _gameData.GetEnemy(room.EnemyId);
        _battle.StartBattle(_run.PlayerDeck, enemy, _run.PlayerMaxHp, _run.PlayerHp);
        SetCombatPortraits(enemy);

        _roomTitleLabel.Text = room.DisplayTitle();
        _roomDescriptionLabel.Text = "战斗开始。用手牌削减敌人生命，并观察敌人的下一步意图。";
        _battlePanel.Visible = true;
        RenderBattle();
    }

    private void OnEndTurnPressed()
    {
        if (IsBattleFinished())
        {
            return;
        }

        _battle.EndPlayerTurn();
        ResolveBattleIfFinished();
        RenderBattle();
        RenderShared();
    }

    private void OnPlayCardPressed(int index)
    {
        if (IsBattleFinished())
        {
            return;
        }

        _battle.PlayCard(index);
        ResolveBattleIfFinished();
        RenderBattle();
        RenderShared();
    }

    private void ResolveBattleIfFinished()
    {
        if (_battle.EnemyHp <= 0)
        {
            _run.SyncAfterBattle(_battle.PlayerHp);
            if (_returnToMineAfterBattle)
            {
                _run.ResolveMineMonsterVictory();
                _battlePanel.Visible = false;
                _minePanel.Visible = true;
                _returnToMineAfterBattle = false;
                _roomTitleLabel.Text = _activeMineRoom?.DisplayTitle() ?? _roomTitleLabel.Text;
                _roomDescriptionLabel.Text = "怪物被击退。你可以继续根据线索探索矿区，或在找到出口后深入下一层。";
                RenderMinefield();
            }
            else
            {
                ShowBattleReward();
            }
        }
        else if (_battle.PlayerHp <= 0)
        {
            _run.SyncAfterBattle(0);
            ShowEndPanel(false, "你的矿灯熄灭了。下一次进入矿井前，最好再调整牌组与路线。");
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
        _roomTitleLabel.Text = "战斗胜利";
        _roomDescriptionLabel.Text = "从破碎矿壳里挑选战利品。";

        var reward = _gameData.GetReward(_activeBattleRoom.RewardId);
        ClearBox(_rewardList);

        var cardChoices = _gameData.BuildRewardChoices(reward, GameSession.SelectedCharacterId, HashCode.Combine(_run.RunSeed, _run.CurrentLayerIndex, reward.Id, _run.PlayerDeck.Count), 3);
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
        _run.ApplyReward(reward, card, _activeBattleRoom?.RewardBonus ?? 0);
        _rewardPanel.Visible = false;
        _continueButton.Visible = true;
        RenderShared();
    }

    private bool IsBattleFinished()
    {
        return _battle.PlayerHp <= 0 || _battle.EnemyHp <= 0;
    }

    private void RenderBattle()
    {
        if (!_battlePanel.Visible)
        {
            return;
        }

        var intent = _battle.GetCurrentEnemyIntent();
        var playerStatuses = _battle.PlayerWeak > 0 ? $" | 虚弱 {_battle.PlayerWeak}" : string.Empty;
        var enemyStatuses = $"{(_battle.EnemyWeak > 0 ? $" | 虚弱 {_battle.EnemyWeak}" : string.Empty)}{(_battle.EnemyVulnerable > 0 ? $" | 易伤 {_battle.EnemyVulnerable}" : string.Empty)}";
        _playerHpBar.MaxValue = _battle.PlayerMaxHp;
        _playerHpBar.Value = Math.Max(0, _battle.PlayerHp);
        _enemyHpBar.MaxValue = _battle.Enemy.MaxHp;
        _enemyHpBar.Value = Math.Max(0, _battle.EnemyHp);
        _playerCombatLabel.Text = $"玩家  HP {_battle.PlayerHp}/{_battle.PlayerMaxHp}\n能量 {_battle.Energy}{playerStatuses}";
        _enemyCombatLabel.Text = $"{_battle.Enemy.DisplayName()}  HP {_battle.EnemyHp}/{_battle.Enemy.MaxHp}{enemyStatuses}";
        _playerBlockLabel.Text = $"格挡 {_battle.PlayerBlock}";
        _enemyBlockLabel.Text = $"格挡 {_battle.EnemyBlock}";
        _battleResourceLabel.Text = $"抽牌堆 {_battle.DrawPile.Count} | 弃牌堆 {_battle.DiscardPile.Count} | 手牌 {_battle.Hand.Count}";
        _intentLabel.Text = $"敌人下一步: {_battle.GetIntentPreview()}\n当前意图会在你结束回合后执行。";

        ClearBox(_handBox);
        for (var i = 0; i < _battle.Hand.Count; i++)
        {
            var card = _battle.Hand[i];
            var button = new Button
            {
                Text = $"[{i + 1}] {card.DisplayName()} ({Localization.T("cost")} {card.Cost})\n{card.DisplayDescription()}",
                CustomMinimumSize = new Vector2(0, 70),
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
            StyleButton(button, Color.FromHtml("233548"), Color.FromHtml("d8e2ee"));
            var captured = i;
            button.Pressed += () => OnPlayCardPressed(captured);
            _handBox.AddChild(button);
        }
    }

    private void SetCombatPortraits(EnemyData enemy)
    {
        var character = _gameData.GetCharacter(GameSession.SelectedCharacterId);
        _playerPortrait.Texture = LoadTexture(character.ArtPath);
        _enemyPortrait.Texture = LoadTexture(enemy.ArtPath);
    }

    private static Texture2D? LoadTexture(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !ResourceLoader.Exists(path))
        {
            return null;
        }

        return ResourceLoader.Load<Texture2D>(path);
    }

    private void RenderShared()
    {
        var displayedHp = _battlePanel.Visible ? _battle.PlayerHp : _run.PlayerHp;
        var displayedMaxHp = _battlePanel.Visible ? _battle.PlayerMaxHp : _run.PlayerMaxHp;
        var layerText = $"{Math.Max(_run.CurrentLayerIndex + 1, 0)}/{_run.MapLayers.Count}";
        var relicText = _run.Relics.Count > 0 ? string.Format(Localization.T("relics"), string.Join(", ", _run.Relics)) : string.Empty;
        _runStatusLabel.Text = $"{string.Format(Localization.T("run_status"), layerText, _run.MapLayers.Count, displayedHp, displayedMaxHp, _run.Shards, _run.PlayerDeck.Count, relicText)} | 灯油: {_run.LampOil}/{_run.MaxLampOil} | 雾压: {_run.FogPressure} | 分数: {_run.Score} | 委托: {FormatObjectiveStatus()}";

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
        foreach (var stack in _run.Items)
        {
            if (stack.Count <= 0)
            {
                continue;
            }

            var item = _gameData.GetItem(stack.ItemId);
            var selected = _selectedItemId == item.Id;
            var button = new Button
            {
                Text = $"{item.DisplayName()} x{stack.Count}\n{item.DisplayDescription()}",
                CustomMinimumSize = new Vector2(0, 70),
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
            StyleButton(button, selected ? Color.FromHtml("5b4a2a") : Color.FromHtml("263445"), Color.FromHtml("eef5ff"));
            button.Pressed += () => OnItemPressed(item);
            _itemList.AddChild(button);
        }
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

    private void ShowEndPanel(bool victory, string reason)
    {
        var meta = _metaRecorded ? SaveManager.LoadMeta() : SaveManager.RecordRun(_run, victory);
        _metaRecorded = true;
        HideInteractivePanels();
        _endPanel.Visible = true;
        _roomTitleLabel.Text = victory ? "探索完成" : "探索失败";
        _roomDescriptionLabel.Text = reason;
        _endTitleLabel.Text = victory ? "本次探索完成" : "本次探索结束";
        _endSummaryLabel.Text = $"抵达层数: {Math.Max(_run.CurrentLayerIndex + 1, 0)}\n剩余 HP: {_run.PlayerHp}/{_run.PlayerMaxHp}\n灯油: {_run.LampOil}/{_run.MaxLampOil}  雾压: {_run.FogPressure}\n矿晶: {_run.Shards}  分数: {_run.Score}\n战斗胜利: {_run.BattlesWon}  清理矿区: {_run.MinesCleared}\n委托: {FormatObjectiveStatus()}  委托奖励: {meta.LastObjectiveBonus}\n本局获得余烬: {meta.LastEarnedEmbers}  总余烬: {meta.TotalEmbers}\n最佳深度: {meta.BestDepth}  最佳分数: {meta.BestScore}\n牌组: {_run.PlayerDeck.Count} 张\n遗物: {(_run.Relics.Count == 0 ? "无" : string.Join(", ", _run.Relics))}";
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

    private void AddChoiceButton(string text, System.Action handler)
    {
        var button = new Button
        {
            Text = text,
            CustomMinimumSize = new Vector2(0, 68),
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        StyleButton(button, Color.FromHtml("263445"), Color.FromHtml("d8e2ee"));
        button.Pressed += handler;
        _choiceList.AddChild(button);
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
        AddChoiceButton("返回\n暂时不调整牌组。", RenderCurrentRoom);
        RenderShared();
    }

    private void RenderDeckRemoveChoices(int cost)
    {
        HideInteractivePanels();
        _choicePanel.Visible = true;
        _roomTitleLabel.Text = "移除卡牌";
        _roomDescriptionLabel.Text = "删牌能让核心牌更稳定上手，但会消耗矿晶。";
        RenderDeckList(true, -1, cost);
        AddChoiceButton("返回\n暂时不调整牌组。", RenderCurrentRoom);
        RenderShared();
    }

    private void OnUpgradeCardPicked(int deckIndex, int cost)
    {
        if (_run.UpgradeCard(deckIndex, _gameData, cost))
        {
            _choicePanel.Visible = false;
            _continueButton.Visible = true;
        }

        RenderShared();
    }

    private void OnRemoveCardPicked(int deckIndex, int cost)
    {
        if (_run.RemoveCard(deckIndex, cost))
        {
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

    private void ApplyUiStyle()
    {
        GetNode<Panel>("Root").AddThemeStyleboxOverride("panel", MakePanelStyle("111820", "263445", 0));
        GetNode<PanelContainer>("Root/Margin/MainLayout/TopBar").AddThemeStyleboxOverride("panel", MakePanelStyle("141d27", "2d3c4d", 1));
        GetNode<PanelContainer>("Root/Margin/MainLayout/RoomHeader").AddThemeStyleboxOverride("panel", MakePanelStyle("151f2b", "32445a", 1));
        GetNode<PanelContainer>("Root/Margin/MainLayout/ContentSplit/MainPanel").AddThemeStyleboxOverride("panel", MakePanelStyle("121923", "2a394a", 1));
        _choicePanel.AddThemeStyleboxOverride("panel", MakePanelStyle("18222d", "34465b", 1));
        _itemPanel.AddThemeStyleboxOverride("panel", MakePanelStyle("121b24", "2a3544", 1));
        _minePanel.AddThemeStyleboxOverride("panel", MakePanelStyle("16202a", "38506a", 1));
        _battlePanel.AddThemeStyleboxOverride("panel", MakePanelStyle("171f2b", "3b4b62", 1));
        GetNode<PanelContainer>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/BattlePanel/BattleLayout/CombatantRow/PlayerCombatPanel").AddThemeStyleboxOverride("panel", MakePanelStyle("14243a", "38506a", 1));
        GetNode<PanelContainer>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/BattlePanel/BattleLayout/CombatantRow/EnemyCombatPanel").AddThemeStyleboxOverride("panel", MakePanelStyle("2b1d24", "6b3b4a", 1));
        GetNode<PanelContainer>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/BattlePanel/BattleLayout/IntentPanel").AddThemeStyleboxOverride("panel", MakePanelStyle("2b2634", "5a4d70", 1));
        GetNode<PanelContainer>("Root/Margin/MainLayout/ContentSplit/MainPanel/ContentStack/MinePanel/MineLayout/MineBoardFrame").AddThemeStyleboxOverride("panel", MakePanelStyle("0f151d", "2c3d50", 1));
        _rewardPanel.AddThemeStyleboxOverride("panel", MakePanelStyle("201c2a", "504264", 1));
        _endPanel.AddThemeStyleboxOverride("panel", MakePanelStyle("171f2b", "52627a", 1));
        GetNode<PanelContainer>("Root/Margin/MainLayout/ContentSplit/SidePanel/ActionPanel").AddThemeStyleboxOverride("panel", MakePanelStyle("121b24", "2a3544", 1));
        GetNode<PanelContainer>("Root/Margin/MainLayout/ContentSplit/SidePanel/LogPanel").AddThemeStyleboxOverride("panel", MakePanelStyle("0f141b", "2a3544", 1));
        _debugPanel.AddThemeStyleboxOverride("panel", MakePanelStyle("171720", "5a4d70", 1));
        StyleButton(_continueButton, Color.FromHtml("315f46"), Color.FromHtml("e7fff1"));
        StyleButton(_menuButton, Color.FromHtml("403547"), Color.FromHtml("f0e4ff"));
        StyleButton(_mineModeButton, Color.FromHtml("3a4f68"), Color.FromHtml("e4f0ff"));
        StyleButton(_deckButton, Color.FromHtml("2f4c54"), Color.FromHtml("e4fbff"));
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
        button.AddThemeColorOverride("font_color", fontColor);
        button.AddThemeColorOverride("font_hover_color", fontColor.Lightened(0.08f));
        button.AddThemeColorOverride("font_pressed_color", fontColor);
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
}
