using Godot;
using Godot.Collections;

public partial class BattleScene : Control
{
    private GameData _gameData = null!;
    private BattleEngine _battle = null!;

    private Label _playerStatusLabel = null!;
    private Label _enemyStatusLabel = null!;
    private Label _deckStatusLabel = null!;
    private HBoxContainer _handContainer = null!;
    private RichTextLabel _logText = null!;
    private Button _endTurnButton = null!;

    public override void _Ready()
    {
        _playerStatusLabel = GetNode<Label>("MarginContainer/RootVBox/HeaderPanel/HeaderVBox/PlayerStatus");
        _enemyStatusLabel = GetNode<Label>("MarginContainer/RootVBox/HeaderPanel/HeaderVBox/EnemyStatus");
        _deckStatusLabel = GetNode<Label>("MarginContainer/RootVBox/HeaderPanel/HeaderVBox/DeckStatus");
        _handContainer = GetNode<HBoxContainer>("MarginContainer/RootVBox/HandPanel/HandPadding/HandCards");
        _logText = GetNode<RichTextLabel>("MarginContainer/RootVBox/LogPanel/LogPadding/BattleLog");
        _endTurnButton = GetNode<Button>("MarginContainer/RootVBox/BottomBar/EndTurnButton");

        _gameData = new GameData();
        _gameData.LoadAll();

        _battle = new BattleEngine();
        _battle.CombatLog += AppendLog;
        _battle.StateChanged += RefreshUi;
        _battle.CombatFinished += OnCombatFinished;

        _endTurnButton.Pressed += OnEndTurnPressed;

        _battle.Setup(_gameData, "starter", "green_slime");
        RefreshUi();
    }

    private void RefreshUi()
    {
        var state = _battle.GetState();

        _playerStatusLabel.Text = $"玩家 HP: {state["player_hp"]}/{state["player_max_hp"]}   格挡: {state["player_block"]}   能量: {state["player_energy"]}";
        _enemyStatusLabel.Text = $"{state["enemy_name"]} HP: {state["enemy_hp"]}/{state["enemy_max_hp"]}   格挡: {state["enemy_block"]}   意图: {state["enemy_intent"]}";
        _deckStatusLabel.Text = $"抽牌堆: {state["draw_count"]}   弃牌堆: {state["discard_count"]}";

        foreach (Node child in _handContainer.GetChildren())
        {
            child.QueueFree();
        }

        var handCards = state["hand"].AsGodotArray<string>();
        for (var i = 0; i < handCards.Count; i++)
        {
            var idx = i;
            var cardId = handCards[i];
            var card = _battle.GetCardView(cardId);

            var cardButton = new Button
            {
                CustomMinimumSize = new Vector2(220, 180),
                Text = $"{card.GetValueOrDefault("name", cardId)} ({card.GetValueOrDefault("cost", 0)})\n\n{card.GetValueOrDefault("description", "")}",
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
                Alignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Disabled = state["combat_over"].AsBool()
            };

            var normalStyle = new StyleBoxFlat
            {
                BgColor = new Color("2A3347"),
                BorderColor = new Color("6FA8FF"),
                BorderWidthBottom = 2,
                BorderWidthTop = 2,
                BorderWidthLeft = 2,
                BorderWidthRight = 2,
                CornerRadiusBottomLeft = 14,
                CornerRadiusBottomRight = 14,
                CornerRadiusTopLeft = 14,
                CornerRadiusTopRight = 14,
                ContentMarginLeft = 14,
                ContentMarginTop = 10
            };
            cardButton.AddThemeStyleboxOverride("normal", normalStyle);
            cardButton.AddThemeStyleboxOverride("hover", normalStyle.Duplicate() as StyleBox);
            cardButton.AddThemeColorOverride("font_color", new Color("E6EEFF"));

            cardButton.Pressed += () => _battle.PlayCard(idx);
            _handContainer.AddChild(cardButton);
        }

        _endTurnButton.Disabled = state["combat_over"].AsBool();
    }

    private void OnEndTurnPressed()
    {
        _battle.EndTurn();
    }

    private void AppendLog(string message)
    {
        _logText.AppendText(message + "\n");
        _logText.ScrollToLine(_logText.GetLineCount());
    }

    private void OnCombatFinished(string result)
    {
        AppendLog(result == "win" ? "你赢了！可扩展到奖励结算 / 下一个房间。" : "你输了！可扩展到重开 / 结算页面。");
    }
}
