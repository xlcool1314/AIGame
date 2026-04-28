using Godot;
using System;
using System.Collections.Generic;
using System.Text;

public partial class BattleScene : Control
{
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
}
