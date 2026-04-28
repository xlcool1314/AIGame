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
    private Label _battleStatusLabel = null!;
    private Label _intentLabel = null!;
    private VBoxContainer _handBox = null!;
    private Button _endTurnButton = null!;
    private PanelContainer _rewardPanel = null!;
    private VBoxContainer _rewardList = null!;
    private Button _continueButton = null!;
    private Button _menuButton = null!;
    private RichTextLabel _logLabel = null!;

    private RunRoom? _activeBattleRoom;
    private bool _mineFlagMode;

    public override void _Ready()
    {
        Localization.LoadSettings();

        _runStatusLabel = GetNode<Label>("Root/Margin/MainLayout/RunStatusLabel");
        _roomTitleLabel = GetNode<Label>("Root/Margin/MainLayout/RoomTitleLabel");
        _roomDescriptionLabel = GetNode<Label>("Root/Margin/MainLayout/RoomDescriptionLabel");
        _choicePanel = GetNode<PanelContainer>("Root/Margin/MainLayout/ChoicePanel");
        _choiceList = GetNode<VBoxContainer>("Root/Margin/MainLayout/ChoicePanel/ChoiceList");
        _minePanel = GetNode<PanelContainer>("Root/Margin/MainLayout/MinePanel");
        _mineStatusLabel = GetNode<Label>("Root/Margin/MainLayout/MinePanel/MineLayout/MineStatusLabel");
        _mineModeButton = GetNode<Button>("Root/Margin/MainLayout/MinePanel/MineLayout/MineModeButton");
        _mineGrid = GetNode<GridContainer>("Root/Margin/MainLayout/MinePanel/MineLayout/MineGrid");
        _battlePanel = GetNode<PanelContainer>("Root/Margin/MainLayout/BattlePanel");
        _battleStatusLabel = GetNode<Label>("Root/Margin/MainLayout/BattlePanel/BattleLayout/BattleStatusLabel");
        _intentLabel = GetNode<Label>("Root/Margin/MainLayout/BattlePanel/BattleLayout/IntentLabel");
        _handBox = GetNode<VBoxContainer>("Root/Margin/MainLayout/BattlePanel/BattleLayout/HandPanel/HandList");
        _endTurnButton = GetNode<Button>("Root/Margin/MainLayout/BattlePanel/BattleLayout/EndTurnButton");
        _rewardPanel = GetNode<PanelContainer>("Root/Margin/MainLayout/RewardPanel");
        _rewardList = GetNode<VBoxContainer>("Root/Margin/MainLayout/RewardPanel/RewardList");
        _continueButton = GetNode<Button>("Root/Margin/MainLayout/ContinueButton");
        _menuButton = GetNode<Button>("Root/Margin/MainLayout/TopButtonRow/MenuButton");
        _logLabel = GetNode<RichTextLabel>("Root/Margin/MainLayout/LogPanel/LogText");

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
        _continueButton.Pressed += OnContinuePressed;
        _mineModeButton.Pressed += OnMineModePressed;
        _menuButton.Pressed += OnMenuPressed;
        _continueButton.Text = Localization.T("continue_deeper");
        _menuButton.Text = Localization.T("back_menu");

        ShowNextRoomChoices();
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
                Text = $"{GetRoomIcon(room.Kind)} {room.Title}\n{GetRoomSummary(room)}",
                CustomMinimumSize = new Vector2(0, 72),
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
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
                _roomTitleLabel.Text = "抵达矿井深处";
                _roomDescriptionLabel.Text = "雾气散开，矿晶在岩壁里发出微光。本轮探索完成。";
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
            "battle" => $"遭遇 {_gameData.GetEnemy(room.EnemyId).Name}，胜利后获得战利品。",
            "mine" => Localization.T("mine_summary"),
            "event" => _gameData.GetEvent(room.EventId).Description,
            "rest" => Localization.T("rest_summary"),
            "shop" => Localization.T("shop_summary"),
            "complete" => Localization.T("complete_summary"),
            _ => Localization.T("unknown_room")
        };
    }

    private void RenderMineRoom(RunRoom room)
    {
        _run.StartMinefield();
        _mineFlagMode = false;
        _roomTitleLabel.Text = room.Title;
        _roomDescriptionLabel.Text = "扫雷探勘开始。数字代表周围八格暗雷数量；用标记模式插旗，翻完所有安全格即可带走矿晶。";
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

        if (_mineFlagMode)
        {
            _run.ToggleMineFlag(index);
        }
        else
        {
            _run.RevealMineCell(index);
        }

        if (_run.PlayerHp <= 0)
        {
            _roomTitleLabel.Text = "探索失败";
            _roomDescriptionLabel.Text = "暗雷耗尽了最后的灯油和体力。";
            HideInteractivePanels();
        }
        else if (_run.Minefield.IsCleared)
        {
            _continueButton.Visible = true;
        }

        RenderMinefield();
        RenderShared();
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
            if (cell.IsRevealed && !cell.IsMine)
            {
                revealedSafe++;
            }
            if (cell.IsFlagged)
            {
                flags++;
            }
        }

        var safeTotal = minefield.Cells.Count - minefield.MineCount;
        _mineStatusLabel.Text = string.Format(Localization.T("mine_status"), revealedSafe, safeTotal, flags, minefield.MineCount, minefield.MineDamage, minefield.RewardShards);

        ClearBox(_mineGrid);
        for (var i = 0; i < minefield.Cells.Count; i++)
        {
            var cell = minefield.Cells[i];
            var text = "?";
            if (cell.IsFlagged && !cell.IsRevealed)
            {
                text = "F";
            }
            else if (cell.IsRevealed && cell.IsMine)
            {
                text = "X";
            }
            else if (cell.IsRevealed)
            {
                text = cell.AdjacentMines == 0 ? " " : cell.AdjacentMines.ToString();
            }

            var button = new Button
            {
                Text = text,
                CustomMinimumSize = new Vector2(52, 52),
                Disabled = minefield.IsCleared || cell.IsRevealed
            };
            var captured = i;
            button.Pressed += () => OnMineCellPressed(captured);
            _mineGrid.AddChild(button);
        }
    }

    private void RenderEventRoom(RunRoom room)
    {
        var mineEvent = _gameData.GetEvent(room.EventId);
        _roomTitleLabel.Text = mineEvent.Title;
        _roomDescriptionLabel.Text = mineEvent.Description;
        _choicePanel.Visible = true;

        ClearBox(_choiceList);
        foreach (var choice in mineEvent.Choices)
        {
            var button = new Button
            {
                Text = choice.Text,
                CustomMinimumSize = new Vector2(0, 56),
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
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
        _roomTitleLabel.Text = room.Title;
        _roomDescriptionLabel.Text = "矿灯在锈蚀铁轨旁轻轻摇晃，你终于有片刻能把呼吸放慢。";
        _choicePanel.Visible = true;

        AddChoiceButton("休息\n恢复一大段生命。", () => OnRestChoicePressed("heal"));
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
        _roomTitleLabel.Text = room.Title;
        _roomDescriptionLabel.Text = "一盏挂满铜铃的矿灯在轨道旁摇晃。商队愿意收矿晶，也愿意卖一点生路。";
        _choicePanel.Visible = true;

        var reward = _gameData.GetReward(room.RewardId);
        foreach (var cardId in reward.CardChoices)
        {
            var card = _gameData.GetCard(cardId);
            AddChoiceButton($"购买 {card.Name} - 22 矿晶\n{card.Description}", () => OnBuyCardPressed(card));
        }

        AddChoiceButton("购买治疗 - 16 矿晶\n恢复 18 点生命。", OnBuyHealPressed);
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
        var enemy = _gameData.GetEnemy(room.EnemyId);
        _battle.StartBattle(_run.PlayerDeck, enemy, _run.PlayerMaxHp, _run.PlayerHp);

        _roomTitleLabel.Text = room.Title;
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
            ShowBattleReward();
        }
        else if (_battle.PlayerHp <= 0)
        {
            _run.SyncAfterBattle(0);
            _roomTitleLabel.Text = "探索失败";
            _roomDescriptionLabel.Text = "你的矿灯熄灭了。下一次进入矿井前，最好再调整牌组与路线。";
            HideInteractivePanels();
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

        foreach (var cardId in reward.CardChoices)
        {
            var card = _gameData.GetCard(cardId);
            var button = new Button
            {
                Text = $"{card.Name} (耗能 {card.Cost})\n{card.Description}",
                CustomMinimumSize = new Vector2(0, 72),
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
            button.Pressed += () => OnRewardPicked(reward, card);
            _rewardList.AddChild(button);
        }

        var skipButton = new Button
        {
            Text = $"跳过卡牌，只获得 {reward.Shards} 矿晶并治疗 {reward.Heal} 点",
            CustomMinimumSize = new Vector2(0, 52),
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        skipButton.Pressed += () => OnRewardPicked(reward, null);
        _rewardList.AddChild(skipButton);
    }

    private void OnRewardPicked(RewardData reward, CardData? card)
    {
        _run.ApplyReward(reward, card);
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
        _battleStatusLabel.Text = $"玩家 HP: {_battle.PlayerHp}/{_battle.PlayerMaxHp} | 格挡: {_battle.PlayerBlock} | 能量: {_battle.Energy}\n" +
                                  $"敌人 {_battle.Enemy.Name} HP: {_battle.EnemyHp}/{_battle.Enemy.MaxHp} | 格挡: {_battle.EnemyBlock}";
        _intentLabel.Text = $"敌人下一步: {intent.Name}";

        ClearBox(_handBox);
        for (var i = 0; i < _battle.Hand.Count; i++)
        {
            var card = _battle.Hand[i];
            var button = new Button
            {
                Text = $"[{i + 1}] {card.Name} (耗能 {card.Cost})\n{card.Description}",
                CustomMinimumSize = new Vector2(0, 70),
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
            var captured = i;
            button.Pressed += () => OnPlayCardPressed(captured);
            _handBox.AddChild(button);
        }
    }

    private void RenderShared()
    {
        var displayedHp = _battlePanel.Visible ? _battle.PlayerHp : _run.PlayerHp;
        var displayedMaxHp = _battlePanel.Visible ? _battle.PlayerMaxHp : _run.PlayerMaxHp;
        var layerText = $"{Math.Max(_run.CurrentLayerIndex + 1, 0)}/{_run.MapLayers.Count}";
        var relicText = _run.Relics.Count > 0 ? string.Format(Localization.T("relics"), string.Join(", ", _run.Relics)) : string.Empty;
        _runStatusLabel.Text = string.Format(Localization.T("run_status"), layerText, _run.MapLayers.Count, displayedHp, displayedMaxHp, _run.Shards, _run.PlayerDeck.Count, relicText);

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
    }

    private void HideInteractivePanels()
    {
        _choicePanel.Visible = false;
        _minePanel.Visible = false;
        _battlePanel.Visible = false;
        _rewardPanel.Visible = false;
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
        button.Pressed += handler;
        _choiceList.AddChild(button);
    }
}
