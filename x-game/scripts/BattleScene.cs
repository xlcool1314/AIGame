using Godot;
using System;
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
=======
using System.Collections.Generic;
>>>>>>> theirs
=======
using System.Collections.Generic;
>>>>>>> theirs
=======
using System.Collections.Generic;
>>>>>>> theirs
=======
using System.Collections.Generic;
>>>>>>> theirs
=======
using System.Collections.Generic;
>>>>>>> theirs
using System.Text;

public partial class BattleScene : Control
{
<<<<<<< ours
	private readonly GameData _gameData = new();
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
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
=======
=======
>>>>>>> theirs
=======
>>>>>>> theirs
=======
>>>>>>> theirs
	private readonly BattleEngine _engine = new();
	private readonly Random _random = new();

	private readonly List<string> _encounterIds = new() { "slime", "mushroom_guard" };
	private readonly List<CardData> _runDeck = new();
	private RelicData? _activeRelic;

	private int _encounterIndex;
	private int _runPlayerHp;
	private bool _rewardPending;
	private bool _eventPending;
	private bool _isEliteBattle;
	private string _nextEncounterId = "mushroom_guard";
	private float _pulseTime;

	private Label _statusLabel = null!;
	private Label _mapLabel = null!;
	private Label _routeHintLabel = null!;
	private Button _nodeStart = null!;
	private Button _nodeBattle1 = null!;
	private Button _nodeEvent = null!;
	private Button _nodeCampPath = null!;
	private Button _nodeElitePath = null!;
	private Label _arrow4 = null!;
	private Label _intentLabel = null!;
	private Label _pilesLabel = null!;
	private Label _statusEffectLabel = null!;
	private Label _relicLabel = null!;
	private VBoxContainer _handBox = null!;
	private RichTextLabel _logLabel = null!;

	private PanelContainer _rewardPanel = null!;
	private Label _rewardTitle = null!;
	private HBoxContainer _rewardChoices = null!;
	private Button _nextBattleButton = null!;
	private PanelContainer _eventPanel = null!;
	private Label _eventTitle = null!;
	private Button _campButton = null!;
	private Button _mysteryButton = null!;

	public override void _Ready()
	{
		_statusLabel = GetNode<Label>("Root/Margin/MainLayout/TopPanel/TopContent/StatusLabel");
		_mapLabel = GetNode<Label>("Root/Margin/MainLayout/TopPanel/TopContent/MapLabel");
		_routeHintLabel = GetNode<Label>("Root/Margin/MainLayout/TopPanel/TopContent/RouteHintLabel");
		_nodeStart = GetNode<Button>("Root/Margin/MainLayout/TopPanel/TopContent/MapNodeRow/NodeStart");
		_nodeBattle1 = GetNode<Button>("Root/Margin/MainLayout/TopPanel/TopContent/MapNodeRow/NodeBattle1");
		_nodeEvent = GetNode<Button>("Root/Margin/MainLayout/TopPanel/TopContent/MapNodeRow/NodeEvent");
		_nodeCampPath = GetNode<Button>("Root/Margin/MainLayout/TopPanel/TopContent/MapNodeRow/NodeCampPath");
		_nodeElitePath = GetNode<Button>("Root/Margin/MainLayout/TopPanel/TopContent/MapNodeRow/NodeElitePath");
		_arrow4 = GetNode<Label>("Root/Margin/MainLayout/TopPanel/TopContent/MapNodeRow/Arrow4");
		_intentLabel = GetNode<Label>("Root/Margin/MainLayout/Body/LeftColumn/IntentPanel/IntentLabel");
		_pilesLabel = GetNode<Label>("Root/Margin/MainLayout/Body/LeftColumn/PilesLabel");
		_statusEffectLabel = GetNode<Label>("Root/Margin/MainLayout/Body/LeftColumn/StatusEffectsLabel");
		_relicLabel = GetNode<Label>("Root/Margin/MainLayout/Body/LeftColumn/RelicLabel");
		_handBox = GetNode<VBoxContainer>("Root/Margin/MainLayout/Body/LeftColumn/HandPanel/HandScroll/HandList");
		_logLabel = GetNode<RichTextLabel>("Root/Margin/MainLayout/Body/RightColumn/LogPanel/LogText");

		_rewardPanel = GetNode<PanelContainer>("Root/Margin/MainLayout/Body/LeftColumn/RewardPanel");
		_rewardTitle = GetNode<Label>("Root/Margin/MainLayout/Body/LeftColumn/RewardPanel/RewardContent/RewardTitle");
		_rewardChoices = GetNode<HBoxContainer>("Root/Margin/MainLayout/Body/LeftColumn/RewardPanel/RewardContent/RewardChoices");
		_nextBattleButton = GetNode<Button>("Root/Margin/MainLayout/Body/LeftColumn/RewardPanel/RewardContent/NextBattleButton");
		_eventPanel = GetNode<PanelContainer>("Root/Margin/MainLayout/Body/LeftColumn/EventPanel");
		_eventTitle = GetNode<Label>("Root/Margin/MainLayout/Body/LeftColumn/EventPanel/EventContent/EventTitle");
		_campButton = GetNode<Button>("Root/Margin/MainLayout/Body/LeftColumn/EventPanel/EventContent/CampButton");
		_mysteryButton = GetNode<Button>("Root/Margin/MainLayout/Body/LeftColumn/EventPanel/EventContent/MysteryButton");

		AddChild(_gameData);
		AddChild(_engine);
		_gameData.LoadAll();

		ApplyUiTheme();

		GetNode<Button>("Root/Margin/MainLayout/Body/LeftColumn/EndTurnButton").Pressed += OnEndTurnPressed;
		_nextBattleButton.Pressed += OnNextBattlePressed;
		_campButton.Pressed += OnCampSelected;
		_mysteryButton.Pressed += OnMysterySelected;
		_nodeCampPath.Pressed += OnCampSelected;
		_nodeElitePath.Pressed += OnMysterySelected;
		_nodeCampPath.MouseEntered += () => SetRouteHint("营地线：更稳健，先回复 12 HP，再进入普通战。");
		_nodeElitePath.MouseEntered += () => SetRouteHint("精英线：风险更高，但可拿到更优奖励（四选一）。");
		_nodeCampPath.MouseExited += ClearRouteHint;
		_nodeElitePath.MouseExited += ClearRouteHint;
		SetProcess(true);

		StartRun();
	}

	private void StartRun()
	{
		_runDeck.Clear();
		_runDeck.AddRange(_gameData.BuildStarterDeck("starter"));
		_encounterIndex = 0;
		_runPlayerHp = _engine.PlayerMaxHp;
		_rewardPending = false;
		_eventPending = false;
		_isEliteBattle = false;
		_nextEncounterId = "mushroom_guard";
		_rewardPanel.Visible = false;
		_eventPanel.Visible = false;
		RollStarterRelic();

		StartEncounter(_encounterIds[_encounterIndex]);
	}

	private void RollStarterRelic()
	{
		var relics = _gameData.GetAllRelics();
		if (relics.Count == 0)
		{
			_activeRelic = null;
			return;
		}

		_activeRelic = relics[_random.Next(relics.Count)];
		ApplyRelicToEngine(_activeRelic);
	}

	private void ApplyRelicToEngine(RelicData relic)
	{
		_engine.BonusEnergyPerTurn = 0;
		_engine.BonusBlockPerTurn = 0;
		_engine.BonusPlayerDamage = 0;

		switch (relic.Type)
		{
			case "bonus_energy":
				_engine.BonusEnergyPerTurn = relic.Value;
				break;
			case "bonus_block":
				_engine.BonusBlockPerTurn = relic.Value;
				break;
			case "bonus_damage":
				_engine.BonusPlayerDamage = relic.Value;
				break;
		}
	}

	private void StartEncounter(string enemyId)
	{
		var enemy = _gameData.GetEnemy(enemyId);
		_isEliteBattle = enemyId == "crystal_golem";
		_engine.StartBattle(new List<CardData>(_runDeck), enemy, _runPlayerHp);
		Render();
	}

	private void ApplyUiTheme()
	{
		var bg = GetNode<Panel>("Root");
		bg.SelfModulate = new Color("#151a24");

		var title = GetNode<Label>("Root/Margin/MainLayout/TopPanel/TopContent/TitleLabel");
		title.AddThemeColorOverride("font_color", new Color("#e8d7a8"));
		_mapLabel.AddThemeColorOverride("font_color", new Color("#9cc7ff"));
		_routeHintLabel.AddThemeColorOverride("font_color", new Color("#c2d3ee"));
		_nodeCampPath.AddThemeStyleboxOverride("normal", MakePanelStyle("#2f6f8f"));
		_nodeCampPath.AddThemeStyleboxOverride("hover", MakePanelStyle("#3c87ac"));
		_nodeElitePath.AddThemeStyleboxOverride("normal", MakePanelStyle("#6b4a93"));
		_nodeElitePath.AddThemeStyleboxOverride("hover", MakePanelStyle("#8360af"));
		_nodeCampPath.TooltipText = "营地线：低风险，先回血再打普通遭遇。";
		_nodeElitePath.TooltipText = "精英线：高风险，挑战精英获取更好奖励。";

		var endTurnButton = GetNode<Button>("Root/Margin/MainLayout/Body/LeftColumn/EndTurnButton");
		endTurnButton.AddThemeColorOverride("font_color", new Color("#f6f8ff"));
		endTurnButton.AddThemeColorOverride("font_hover_color", new Color("#ffffff"));
		endTurnButton.AddThemeStyleboxOverride("normal", MakePanelStyle("#4960d8"));
		endTurnButton.AddThemeStyleboxOverride("hover", MakePanelStyle("#6076ea"));
		endTurnButton.AddThemeStyleboxOverride("pressed", MakePanelStyle("#3249bc"));

		_nextBattleButton.AddThemeStyleboxOverride("normal", MakePanelStyle("#3a9968"));
		_nextBattleButton.AddThemeStyleboxOverride("hover", MakePanelStyle("#49b37a"));
		_campButton.AddThemeStyleboxOverride("normal", MakePanelStyle("#2f6f8f"));
		_campButton.AddThemeStyleboxOverride("hover", MakePanelStyle("#3c87ac"));
		_mysteryButton.AddThemeStyleboxOverride("normal", MakePanelStyle("#6b4a93"));
		_mysteryButton.AddThemeStyleboxOverride("hover", MakePanelStyle("#8360af"));
	}

	private static StyleBoxFlat MakePanelStyle(string colorHex)
	{
		var style = new StyleBoxFlat
		{
			BgColor = new Color(colorHex),
			CornerRadiusTopLeft = 10,
			CornerRadiusTopRight = 10,
			CornerRadiusBottomLeft = 10,
			CornerRadiusBottomRight = 10,
		};
		style.SetContentMarginAll(8);
		return style;
	}

	private void OnEndTurnPressed()
	{
		if (IsBattleFinished() || _rewardPending)
		{
			return;
		}

		_engine.EndPlayerTurn();
		Render();
	}

	private void OnPlayCardPressed(int index)
	{
		if (IsBattleFinished() || _rewardPending)
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
>>>>>>> theirs
=======
>>>>>>> theirs
=======
>>>>>>> theirs
=======
>>>>>>> theirs
		{
			return;
		}

<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
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
=======
=======
>>>>>>> theirs
=======
>>>>>>> theirs
=======
>>>>>>> theirs
		_engine.PlayCard(index);
		Render();
	}

	private void OnPickReward(CardData card)
	{
		_runDeck.Add(card);
		_rewardTitle.Text = $"已获得：{card.Name}（当前牌组 {_runDeck.Count} 张）";

		foreach (var child in _rewardChoices.GetChildren())
		{
			child.QueueFree();
		}

		_nextBattleButton.Disabled = false;
	}

	private void OnNextBattlePressed()
	{
		_rewardPanel.Visible = false;
		_rewardPending = false;

		_encounterIndex++;
		if (_encounterIndex >= _encounterIds.Count)
		{
			_logLabel.Text = "\n=== 你已通关当前矿井样章 ===\n\n" + _logLabel.Text;
			return;
		}

		if (_encounterIndex == 1)
		{
			_eventPending = true;
			_eventPanel.Visible = true;
			_eventTitle.Text = "你在矿道岔路遇到一处休整点，要怎么做？\n- 营地：稳定推进到普通遭遇\n- 祭坛：进入精英遭遇，奖励更好";
			return;
		}

		StartEncounter(_nextEncounterId);
	}

	private void OnCampSelected()
	{
		if (!_eventPending)
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
>>>>>>> theirs
=======
>>>>>>> theirs
=======
>>>>>>> theirs
		{
			return;
		}

<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
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
=======
>>>>>>> theirs
		{
			return;
		}

<<<<<<< ours
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
=======
=======
>>>>>>> theirs
=======
>>>>>>> theirs
=======
>>>>>>> theirs
		_runPlayerHp = Math.Min(_engine.PlayerMaxHp, _runPlayerHp + 12);
		_eventTitle.Text = $"你在营地恢复了生命，当前 HP: {_runPlayerHp}";
		_nextEncounterId = "mushroom_guard";
		_eventPending = false;
		_eventPanel.Visible = false;
		StartEncounter(_nextEncounterId);
	}

	private void OnMysterySelected()
	{
		if (!_eventPending)
		{
			return;
		}

		RollStarterRelic();
		_eventTitle.Text = $"你触发神秘祭坛，遗物变更为：{_activeRelic?.Name ?? "无"}，并惊动了精英守卫。";
		_nextEncounterId = "crystal_golem";
		_eventPending = false;
		_eventPanel.Visible = false;
		StartEncounter(_nextEncounterId);
	}

	private void ShowRewards()
	{
		_rewardPending = true;
		_rewardPanel.Visible = true;
		_nextBattleButton.Disabled = true;
		_rewardTitle.Text = "选择一张奖励卡加入你的牌组";

		foreach (var child in _rewardChoices.GetChildren())
		{
			child.QueueFree();
		}

		var pool = _gameData.GetAllCards();
		var rewardOptions = _isEliteBattle ? 4 : 3;
		for (var i = 0; i < rewardOptions && pool.Count > 0; i++)
		{
			var pick = _random.Next(pool.Count);
			var card = pool[pick];
			pool.RemoveAt(pick);

			var button = new Button
			{
				Text = $"{card.Name}\n耗能:{card.Cost}\n{card.Description}",
				CustomMinimumSize = new Vector2(220, 110),
				AutowrapMode = TextServer.AutowrapMode.WordSmart,
			};
			button.AddThemeStyleboxOverride("normal", MakePanelStyle("#3a2f4d"));
			button.AddThemeStyleboxOverride("hover", MakePanelStyle("#4a3d63"));

			button.Pressed += () => OnPickReward(card);
			_rewardChoices.AddChild(button);
		}
	}

	private bool IsBattleFinished()
	{
		return _engine.PlayerHp <= 0 || _engine.EnemyHp <= 0;
	}

	private void Render()
	{
		var intent = _engine.GetCurrentEnemyIntent();
		var tier = _isEliteBattle ? "精英" : "普通";
		_statusLabel.Text = $"关卡 {_encounterIndex + 1}/{_encounterIds.Count} ({tier}) | 第 {_engine.Turn} 回合 | 能量: {_engine.Energy}\n" +
							$"玩家 HP: {_engine.PlayerHp}/{_engine.PlayerMaxHp}  持久HP:{_runPlayerHp}  格挡: {_engine.PlayerBlock}  牌组总数:{_runDeck.Count}\n" +
							$"敌人 {_engine.Enemy.Name} HP: {_engine.EnemyHp}/{_engine.Enemy.MaxHp}  格挡: {_engine.EnemyBlock}";

		_intentLabel.Text = $"敌人下一步：{intent.Name}";
		_mapLabel.Text = BuildMapPreview();
		_nodeCampPath.Visible = _eventPending;
		_nodeElitePath.Visible = _eventPending;
		_arrow4.Visible = _eventPending;
		_nodeCampPath.Disabled = !_eventPending;
		_nodeElitePath.Disabled = !_eventPending;
		UpdateMapNodeVisuals();
		_pilesLabel.Text = $"抽牌堆: {_engine.DrawPile.Count}  弃牌堆: {_engine.DiscardPile.Count}  手牌: {_engine.Hand.Count}";
		_statusEffectLabel.Text =
			$"玩家状态 -> 弱化:{_engine.PlayerWeak} 易伤:{_engine.PlayerVulnerable}\n" +
			$"敌人状态 -> 弱化:{_engine.EnemyWeak} 易伤:{_engine.EnemyVulnerable}";
		_relicLabel.Text = _activeRelic == null
			? "遗物：无"
			: $"遗物：{_activeRelic.Name}（{_activeRelic.Description}）";

		foreach (var child in _handBox.GetChildren())
		{
			child.QueueFree();
		}

		for (var i = 0; i < _engine.Hand.Count; i++)
		{
			var card = _engine.Hand[i];
			var button = new Button
			{
				Text = $"[{i + 1}] {card.Name}  耗能:{card.Cost}\n{card.Description}",
				CustomMinimumSize = new Vector2(0, 76),
				AutowrapMode = TextServer.AutowrapMode.WordSmart,
			};
			button.AddThemeStyleboxOverride("normal", MakePanelStyle("#2a3244"));
			button.AddThemeStyleboxOverride("hover", MakePanelStyle("#36435f"));
			button.AddThemeColorOverride("font_color", new Color("#f0f4ff"));

			var captured = i;
			button.Pressed += () => OnPlayCardPressed(captured);
			_handBox.AddChild(button);
		}

		var sb = new StringBuilder();
		for (var i = _engine.Log.Count - 1; i >= 0; i--)
		{
			sb.AppendLine(_engine.Log[i]);
		}

		if (_engine.EnemyHp <= 0)
		{
			_runPlayerHp = _engine.PlayerHp;
			sb.Insert(0, "\n=== 战斗胜利 ===\n");
			if (!_rewardPending)
			{
				ShowRewards();
			}
		}
		else if (_engine.PlayerHp <= 0)
		{
			sb.Insert(0, "\n=== 战斗失败 ===\n");
			_rewardPanel.Visible = false;
			_rewardPending = false;
		}

		_logLabel.Text = sb.ToString();
	}

	private string BuildMapPreview()
	{
		if (_encounterIndex == 0 && !_eventPending)
		{
			return "路线：起点 -> 首战(史莱姆) -> 事件岔路 -> 二战(普通/精英)";
		}

		if (_eventPending)
		{
			return "路线：起点 -> 首战完成 -> [营地=>普通战] / [祭坛=>精英战]";
		}

		if (_encounterIndex >= 1)
		{
			var branch = _isEliteBattle ? "祭坛精英线" : "营地稳健线";
			return $"路线：当前处于{branch} -> 终点";
		}

		return "路线：生成中...";
	}

	private void UpdateMapNodeVisuals()
	{
		ApplyNodeStyle(_nodeStart, "起点", "done");

		if (_encounterIndex == 0)
		{
			ApplyNodeStyle(_nodeBattle1, "首战", "current");
			ApplyNodeStyle(_nodeEvent, "岔路", "locked");
		}
		else if (_eventPending)
		{
			ApplyNodeStyle(_nodeBattle1, "首战", "done");
			ApplyNodeStyle(_nodeEvent, "岔路", "current");
		}
		else
		{
			ApplyNodeStyle(_nodeBattle1, "首战", "done");
			ApplyNodeStyle(_nodeEvent, "岔路", "done");
		}

		if (_eventPending)
		{
			ApplyNodeStyle(_nodeCampPath, "营地线", "choice");
			ApplyNodeStyle(_nodeElitePath, "精英线", "choice");
		}
		else if (_encounterIndex >= 1)
		{
			ApplyNodeStyle(_nodeCampPath, "营地线", _isEliteBattle ? "locked" : "current");
			ApplyNodeStyle(_nodeElitePath, "精英线", _isEliteBattle ? "current" : "locked");
		}
	}

	private static void ApplyNodeStyle(Button button, string text, string state)
	{
		button.Text = text;

		switch (state)
		{
			case "done":
				button.Modulate = new Color("#8eb39d");
				break;
			case "current":
				button.Modulate = new Color("#ffd479");
				break;
			case "choice":
				button.Modulate = new Color("#d4b8ff");
				break;
			default:
				button.Modulate = new Color("#708090");
				break;
		}

		button.Scale = Vector2.One;
	}

	private void SetRouteHint(string text)
	{
		_routeHintLabel.Text = $"提示：{text}";
	}

	private void ClearRouteHint()
	{
		_routeHintLabel.Text = "提示：将鼠标悬停在路线节点上查看风险与收益。";
	}

	public override void _Process(double delta)
	{
		_pulseTime += (float)delta;
		var current = GetCurrentNodeButton();
		if (current == null)
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
>>>>>>> theirs
=======
>>>>>>> theirs
=======
>>>>>>> theirs
=======
>>>>>>> theirs
		{
			return;
		}

<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
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
=======
=======
>>>>>>> theirs
=======
>>>>>>> theirs
=======
>>>>>>> theirs
		var pulse = 1.0f + 0.05f * Mathf.Sin(_pulseTime * 6f);
		current.Scale = new Vector2(pulse, pulse);
	}

	private Button? GetCurrentNodeButton()
	{
		if (_encounterIndex == 0)
		{
			return _nodeBattle1;
		}

		if (_eventPending)
		{
			return _nodeEvent;
		}

		if (_encounterIndex >= 1)
		{
			return _isEliteBattle ? _nodeElitePath : _nodeCampPath;
		}

		return null;
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
>>>>>>> theirs
=======
>>>>>>> theirs
=======
>>>>>>> theirs
=======
>>>>>>> theirs
	}
=======
    private readonly GameData _gameData = new();
    private readonly BattleEngine _engine = new();
    private readonly Random _random = new();

    private readonly List<string> _encounterIds = new() { "slime", "mushroom_guard" };
    private readonly List<CardData> _runDeck = new();
    private RelicData? _activeRelic;

    private int _encounterIndex;
    private int _runPlayerHp;
    private bool _rewardPending;
    private bool _eventPending;
    private bool _isEliteBattle;
    private string _nextEncounterId = "mushroom_guard";
    private float _pulseTime;

    private Label _statusLabel = null!;
    private Label _mapLabel = null!;
    private Label _routeHintLabel = null!;
    private Button _nodeStart = null!;
    private Button _nodeBattle1 = null!;
    private Button _nodeEvent = null!;
    private Button _nodeCampPath = null!;
    private Button _nodeElitePath = null!;
    private Label _arrow4 = null!;
    private Label _intentLabel = null!;
    private Label _pilesLabel = null!;
    private Label _statusEffectLabel = null!;
    private Label _relicLabel = null!;
    private VBoxContainer _handBox = null!;
    private RichTextLabel _logLabel = null!;

    private PanelContainer _rewardPanel = null!;
    private Label _rewardTitle = null!;
    private HBoxContainer _rewardChoices = null!;
    private Button _nextBattleButton = null!;
    private PanelContainer _eventPanel = null!;
    private Label _eventTitle = null!;
    private Button _campButton = null!;
    private Button _mysteryButton = null!;

    public override void _Ready()
    {
        _statusLabel = GetNode<Label>("Root/Margin/MainLayout/TopPanel/TopContent/StatusLabel");
        _mapLabel = GetNode<Label>("Root/Margin/MainLayout/TopPanel/TopContent/MapLabel");
        _routeHintLabel = GetNode<Label>("Root/Margin/MainLayout/TopPanel/TopContent/RouteHintLabel");
        _nodeStart = GetNode<Button>("Root/Margin/MainLayout/TopPanel/TopContent/MapNodeRow/NodeStart");
        _nodeBattle1 = GetNode<Button>("Root/Margin/MainLayout/TopPanel/TopContent/MapNodeRow/NodeBattle1");
        _nodeEvent = GetNode<Button>("Root/Margin/MainLayout/TopPanel/TopContent/MapNodeRow/NodeEvent");
        _nodeCampPath = GetNode<Button>("Root/Margin/MainLayout/TopPanel/TopContent/MapNodeRow/NodeCampPath");
        _nodeElitePath = GetNode<Button>("Root/Margin/MainLayout/TopPanel/TopContent/MapNodeRow/NodeElitePath");
        _arrow4 = GetNode<Label>("Root/Margin/MainLayout/TopPanel/TopContent/MapNodeRow/Arrow4");
        _intentLabel = GetNode<Label>("Root/Margin/MainLayout/Body/LeftColumn/IntentPanel/IntentLabel");
        _pilesLabel = GetNode<Label>("Root/Margin/MainLayout/Body/LeftColumn/PilesLabel");
        _statusEffectLabel = GetNode<Label>("Root/Margin/MainLayout/Body/LeftColumn/StatusEffectsLabel");
        _relicLabel = GetNode<Label>("Root/Margin/MainLayout/Body/LeftColumn/RelicLabel");
        _handBox = GetNode<VBoxContainer>("Root/Margin/MainLayout/Body/LeftColumn/HandPanel/HandScroll/HandList");
        _logLabel = GetNode<RichTextLabel>("Root/Margin/MainLayout/Body/RightColumn/LogPanel/LogText");

        _rewardPanel = GetNode<PanelContainer>("Root/Margin/MainLayout/Body/LeftColumn/RewardPanel");
        _rewardTitle = GetNode<Label>("Root/Margin/MainLayout/Body/LeftColumn/RewardPanel/RewardContent/RewardTitle");
        _rewardChoices = GetNode<HBoxContainer>("Root/Margin/MainLayout/Body/LeftColumn/RewardPanel/RewardContent/RewardChoices");
        _nextBattleButton = GetNode<Button>("Root/Margin/MainLayout/Body/LeftColumn/RewardPanel/RewardContent/NextBattleButton");
        _eventPanel = GetNode<PanelContainer>("Root/Margin/MainLayout/Body/LeftColumn/EventPanel");
        _eventTitle = GetNode<Label>("Root/Margin/MainLayout/Body/LeftColumn/EventPanel/EventContent/EventTitle");
        _campButton = GetNode<Button>("Root/Margin/MainLayout/Body/LeftColumn/EventPanel/EventContent/CampButton");
        _mysteryButton = GetNode<Button>("Root/Margin/MainLayout/Body/LeftColumn/EventPanel/EventContent/MysteryButton");

        AddChild(_gameData);
        AddChild(_engine);
        _gameData.LoadAll();

        ApplyUiTheme();

        GetNode<Button>("Root/Margin/MainLayout/Body/LeftColumn/EndTurnButton").Pressed += OnEndTurnPressed;
        _nextBattleButton.Pressed += OnNextBattlePressed;
        _campButton.Pressed += OnCampSelected;
        _mysteryButton.Pressed += OnMysterySelected;
        _nodeCampPath.Pressed += OnCampSelected;
        _nodeElitePath.Pressed += OnMysterySelected;
        _nodeCampPath.MouseEntered += () => SetRouteHint("营地线：更稳健，先回复 12 HP，再进入普通战。");
        _nodeElitePath.MouseEntered += () => SetRouteHint("精英线：风险更高，但可拿到更优奖励（四选一）。");
        _nodeCampPath.MouseExited += ClearRouteHint;
        _nodeElitePath.MouseExited += ClearRouteHint;
        SetProcess(true);

        StartRun();
    }

    private void StartRun()
    {
        _runDeck.Clear();
        _runDeck.AddRange(_gameData.BuildStarterDeck("starter"));
        _encounterIndex = 0;
        _runPlayerHp = _engine.PlayerMaxHp;
        _rewardPending = false;
        _eventPending = false;
        _isEliteBattle = false;
        _nextEncounterId = "mushroom_guard";
        _rewardPanel.Visible = false;
        _eventPanel.Visible = false;
        RollStarterRelic();

        StartEncounter(_encounterIds[_encounterIndex]);
    }

    private void RollStarterRelic()
    {
        var relics = _gameData.GetAllRelics();
        if (relics.Count == 0)
        {
            _activeRelic = null;
            return;
        }

        _activeRelic = relics[_random.Next(relics.Count)];
        ApplyRelicToEngine(_activeRelic);
    }

    private void ApplyRelicToEngine(RelicData relic)
    {
        _engine.BonusEnergyPerTurn = 0;
        _engine.BonusBlockPerTurn = 0;
        _engine.BonusPlayerDamage = 0;

        switch (relic.Type)
        {
            case "bonus_energy":
                _engine.BonusEnergyPerTurn = relic.Value;
                break;
            case "bonus_block":
                _engine.BonusBlockPerTurn = relic.Value;
                break;
            case "bonus_damage":
                _engine.BonusPlayerDamage = relic.Value;
                break;
        }
    }

    private void StartEncounter(string enemyId)
    {
        var enemy = _gameData.GetEnemy(enemyId);
        _isEliteBattle = enemyId == "crystal_golem";
        _engine.StartBattle(new List<CardData>(_runDeck), enemy, _runPlayerHp);
        Render();
    }

    private void ApplyUiTheme()
    {
        var bg = GetNode<Panel>("Root");
        bg.SelfModulate = new Color("#151a24");

        var title = GetNode<Label>("Root/Margin/MainLayout/TopPanel/TopContent/TitleLabel");
        title.AddThemeColorOverride("font_color", new Color("#e8d7a8"));
        _mapLabel.AddThemeColorOverride("font_color", new Color("#9cc7ff"));
        _routeHintLabel.AddThemeColorOverride("font_color", new Color("#c2d3ee"));
        _nodeCampPath.AddThemeStyleboxOverride("normal", MakePanelStyle("#2f6f8f"));
        _nodeCampPath.AddThemeStyleboxOverride("hover", MakePanelStyle("#3c87ac"));
        _nodeElitePath.AddThemeStyleboxOverride("normal", MakePanelStyle("#6b4a93"));
        _nodeElitePath.AddThemeStyleboxOverride("hover", MakePanelStyle("#8360af"));
        _nodeCampPath.TooltipText = "营地线：低风险，先回血再打普通遭遇。";
        _nodeElitePath.TooltipText = "精英线：高风险，挑战精英获取更好奖励。";

        var endTurnButton = GetNode<Button>("Root/Margin/MainLayout/Body/LeftColumn/EndTurnButton");
        endTurnButton.AddThemeColorOverride("font_color", new Color("#f6f8ff"));
        endTurnButton.AddThemeColorOverride("font_hover_color", new Color("#ffffff"));
        endTurnButton.AddThemeStyleboxOverride("normal", MakePanelStyle("#4960d8"));
        endTurnButton.AddThemeStyleboxOverride("hover", MakePanelStyle("#6076ea"));
        endTurnButton.AddThemeStyleboxOverride("pressed", MakePanelStyle("#3249bc"));

        _nextBattleButton.AddThemeStyleboxOverride("normal", MakePanelStyle("#3a9968"));
        _nextBattleButton.AddThemeStyleboxOverride("hover", MakePanelStyle("#49b37a"));
        _campButton.AddThemeStyleboxOverride("normal", MakePanelStyle("#2f6f8f"));
        _campButton.AddThemeStyleboxOverride("hover", MakePanelStyle("#3c87ac"));
        _mysteryButton.AddThemeStyleboxOverride("normal", MakePanelStyle("#6b4a93"));
        _mysteryButton.AddThemeStyleboxOverride("hover", MakePanelStyle("#8360af"));
    }

    private static StyleBoxFlat MakePanelStyle(string colorHex)
    {
        var style = new StyleBoxFlat
        {
            BgColor = new Color(colorHex),
            CornerRadiusTopLeft = 10,
            CornerRadiusTopRight = 10,
            CornerRadiusBottomLeft = 10,
            CornerRadiusBottomRight = 10,
        };
        style.SetContentMarginAll(8);
        return style;
    }

    private void OnEndTurnPressed()
    {
        if (IsBattleFinished() || _rewardPending)
        {
            return;
        }

        _engine.EndPlayerTurn();
        Render();
    }

    private void OnPlayCardPressed(int index)
    {
        if (IsBattleFinished() || _rewardPending)
        {
            return;
        }

        _engine.PlayCard(index);
        Render();
    }

    private void OnPickReward(CardData card)
    {
        _runDeck.Add(card);
        _rewardTitle.Text = $"已获得：{card.Name}（当前牌组 {_runDeck.Count} 张）";

        foreach (var child in _rewardChoices.GetChildren())
        {
            child.QueueFree();
        }

        _nextBattleButton.Disabled = false;
    }

    private void OnNextBattlePressed()
    {
        _rewardPanel.Visible = false;
        _rewardPending = false;

        _encounterIndex++;
        if (_encounterIndex >= _encounterIds.Count)
        {
            _logLabel.Text = "\n=== 你已通关当前矿井样章 ===\n\n" + _logLabel.Text;
            return;
        }

        if (_encounterIndex == 1)
        {
            _eventPending = true;
            _eventPanel.Visible = true;
            _eventTitle.Text = "你在矿道岔路遇到一处休整点，要怎么做？\n- 营地：稳定推进到普通遭遇\n- 祭坛：进入精英遭遇，奖励更好";
            return;
        }

        StartEncounter(_nextEncounterId);
    }

    private void OnCampSelected()
    {
        if (!_eventPending)
        {
            return;
        }

        _runPlayerHp = Math.Min(_engine.PlayerMaxHp, _runPlayerHp + 12);
        _eventTitle.Text = $"你在营地恢复了生命，当前 HP: {_runPlayerHp}";
        _nextEncounterId = "mushroom_guard";
        _eventPending = false;
        _eventPanel.Visible = false;
        StartEncounter(_nextEncounterId);
    }

    private void OnMysterySelected()
    {
        if (!_eventPending)
        {
            return;
        }

        RollStarterRelic();
        _eventTitle.Text = $"你触发神秘祭坛，遗物变更为：{_activeRelic?.Name ?? "无"}，并惊动了精英守卫。";
        _nextEncounterId = "crystal_golem";
        _eventPending = false;
        _eventPanel.Visible = false;
        StartEncounter(_nextEncounterId);
    }

    private void ShowRewards()
    {
        _rewardPending = true;
        _rewardPanel.Visible = true;
        _nextBattleButton.Disabled = true;
        _rewardTitle.Text = "选择一张奖励卡加入你的牌组";

        foreach (var child in _rewardChoices.GetChildren())
        {
            child.QueueFree();
        }

        var pool = _gameData.GetAllCards();
        var rewardOptions = _isEliteBattle ? 4 : 3;
        for (var i = 0; i < rewardOptions && pool.Count > 0; i++)
        {
            var pick = _random.Next(pool.Count);
            var card = pool[pick];
            pool.RemoveAt(pick);

            var button = new Button
            {
                Text = $"{card.Name}\n耗能:{card.Cost}\n{card.Description}",
                CustomMinimumSize = new Vector2(220, 110),
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
            };
            button.AddThemeStyleboxOverride("normal", MakePanelStyle("#3a2f4d"));
            button.AddThemeStyleboxOverride("hover", MakePanelStyle("#4a3d63"));

            button.Pressed += () => OnPickReward(card);
            _rewardChoices.AddChild(button);
        }
    }

    private bool IsBattleFinished()
    {
        return _engine.PlayerHp <= 0 || _engine.EnemyHp <= 0;
    }

    private void Render()
    {
        var intent = _engine.GetCurrentEnemyIntent();
        var tier = _isEliteBattle ? "精英" : "普通";
        _statusLabel.Text = $"关卡 {_encounterIndex + 1}/{_encounterIds.Count} ({tier}) | 第 {_engine.Turn} 回合 | 能量: {_engine.Energy}\n" +
                            $"玩家 HP: {_engine.PlayerHp}/{_engine.PlayerMaxHp}  持久HP:{_runPlayerHp}  格挡: {_engine.PlayerBlock}  牌组总数:{_runDeck.Count}\n" +
                            $"敌人 {_engine.Enemy.Name} HP: {_engine.EnemyHp}/{_engine.Enemy.MaxHp}  格挡: {_engine.EnemyBlock}";

        _intentLabel.Text = $"敌人下一步：{intent.Name}";
        _mapLabel.Text = BuildMapPreview();
        _nodeCampPath.Visible = _eventPending;
        _nodeElitePath.Visible = _eventPending;
        _arrow4.Visible = _eventPending;
        _nodeCampPath.Disabled = !_eventPending;
        _nodeElitePath.Disabled = !_eventPending;
        UpdateMapNodeVisuals();
        _pilesLabel.Text = $"抽牌堆: {_engine.DrawPile.Count}  弃牌堆: {_engine.DiscardPile.Count}  手牌: {_engine.Hand.Count}";
        _statusEffectLabel.Text =
            $"玩家状态 -> 弱化:{_engine.PlayerWeak} 易伤:{_engine.PlayerVulnerable}\n" +
            $"敌人状态 -> 弱化:{_engine.EnemyWeak} 易伤:{_engine.EnemyVulnerable}";
        _relicLabel.Text = _activeRelic == null
            ? "遗物：无"
            : $"遗物：{_activeRelic.Name}（{_activeRelic.Description}）";

        foreach (var child in _handBox.GetChildren())
        {
            child.QueueFree();
        }

        for (var i = 0; i < _engine.Hand.Count; i++)
        {
            var card = _engine.Hand[i];
            var button = new Button
            {
                Text = $"[{i + 1}] {card.Name}  耗能:{card.Cost}\n{card.Description}",
                CustomMinimumSize = new Vector2(0, 76),
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
            };
            button.AddThemeStyleboxOverride("normal", MakePanelStyle("#2a3244"));
            button.AddThemeStyleboxOverride("hover", MakePanelStyle("#36435f"));
            button.AddThemeColorOverride("font_color", new Color("#f0f4ff"));

            var captured = i;
            button.Pressed += () => OnPlayCardPressed(captured);
            _handBox.AddChild(button);
        }

        var sb = new StringBuilder();
        for (var i = _engine.Log.Count - 1; i >= 0; i--)
        {
            sb.AppendLine(_engine.Log[i]);
        }

        if (_engine.EnemyHp <= 0)
        {
            _runPlayerHp = _engine.PlayerHp;
            sb.Insert(0, "\n=== 战斗胜利 ===\n");
            if (!_rewardPending)
            {
                ShowRewards();
            }
        }
        else if (_engine.PlayerHp <= 0)
        {
            sb.Insert(0, "\n=== 战斗失败 ===\n");
            _rewardPanel.Visible = false;
            _rewardPending = false;
        }

        _logLabel.Text = sb.ToString();
    }

    private string BuildMapPreview()
    {
        if (_encounterIndex == 0 && !_eventPending)
        {
            return "路线：起点 -> 首战(史莱姆) -> 事件岔路 -> 二战(普通/精英)";
        }

        if (_eventPending)
        {
            return "路线：起点 -> 首战完成 -> [营地=>普通战] / [祭坛=>精英战]";
        }

        if (_encounterIndex >= 1)
        {
            var branch = _isEliteBattle ? "祭坛精英线" : "营地稳健线";
            return $"路线：当前处于{branch} -> 终点";
        }

        return "路线：生成中...";
    }

    private void UpdateMapNodeVisuals()
    {
        ApplyNodeStyle(_nodeStart, "起点", "done");

        if (_encounterIndex == 0)
        {
            ApplyNodeStyle(_nodeBattle1, "首战", "current");
            ApplyNodeStyle(_nodeEvent, "岔路", "locked");
        }
        else if (_eventPending)
        {
            ApplyNodeStyle(_nodeBattle1, "首战", "done");
            ApplyNodeStyle(_nodeEvent, "岔路", "current");
        }
        else
        {
            ApplyNodeStyle(_nodeBattle1, "首战", "done");
            ApplyNodeStyle(_nodeEvent, "岔路", "done");
        }

        if (_eventPending)
        {
            ApplyNodeStyle(_nodeCampPath, "营地线", "choice");
            ApplyNodeStyle(_nodeElitePath, "精英线", "choice");
        }
        else if (_encounterIndex >= 1)
        {
            ApplyNodeStyle(_nodeCampPath, "营地线", _isEliteBattle ? "locked" : "current");
            ApplyNodeStyle(_nodeElitePath, "精英线", _isEliteBattle ? "current" : "locked");
        }
    }

    private static void ApplyNodeStyle(Button button, string text, string state)
    {
        button.Text = text;

        switch (state)
        {
            case "done":
                button.Modulate = new Color("#8eb39d");
                break;
            case "current":
                button.Modulate = new Color("#ffd479");
                break;
            case "choice":
                button.Modulate = new Color("#d4b8ff");
                break;
            default:
                button.Modulate = new Color("#708090");
                break;
        }

        button.Scale = Vector2.One;
    }

    private void SetRouteHint(string text)
    {
        _routeHintLabel.Text = $"提示：{text}";
    }

    private void ClearRouteHint()
    {
        _routeHintLabel.Text = "提示：将鼠标悬停在路线节点上查看风险与收益。";
    }

    public override void _Process(double delta)
    {
        _pulseTime += (float)delta;
        var current = GetCurrentNodeButton();
        if (current == null)
        {
            return;
        }

        var pulse = 1.0f + 0.05f * Mathf.Sin(_pulseTime * 6f);
        current.Scale = new Vector2(pulse, pulse);
    }

    private Button? GetCurrentNodeButton()
    {
        if (_encounterIndex == 0)
        {
            return _nodeBattle1;
        }

        if (_eventPending)
        {
            return _nodeEvent;
        }

        if (_encounterIndex >= 1)
        {
            return _isEliteBattle ? _nodeElitePath : _nodeCampPath;
        }

        return null;
    }
>>>>>>> theirs
}
