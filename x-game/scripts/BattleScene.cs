using Godot;
using System.Text;

public partial class BattleScene : Control
{
    private readonly GameData _gameData = new();
    private readonly BattleEngine _engine = new();

    private Label _statusLabel = null!;
    private Label _intentLabel = null!;
    private VBoxContainer _handBox = null!;
    private RichTextLabel _logLabel = null!;

    public override void _Ready()
    {
        _statusLabel = GetNode<Label>("Root/Margin/MainLayout/StatusLabel");
        _intentLabel = GetNode<Label>("Root/Margin/MainLayout/IntentLabel");
        _handBox = GetNode<VBoxContainer>("Root/Margin/MainLayout/HandPanel/HandList");
        _logLabel = GetNode<RichTextLabel>("Root/Margin/MainLayout/LogPanel/LogText");

        AddChild(_gameData);
        AddChild(_engine);
        _gameData.LoadAll();

        var deck = _gameData.BuildStarterDeck("starter");
        var enemy = _gameData.GetEnemy("slime");
        _engine.StartBattle(deck, enemy);

        GetNode<Button>("Root/Margin/MainLayout/EndTurnButton").Pressed += OnEndTurnPressed;
        Render();
    }

    private void OnEndTurnPressed()
    {
        if (IsBattleFinished())
        {
            return;
        }

        _engine.EndPlayerTurn();
        Render();
    }

    private void OnPlayCardPressed(int index)
    {
        if (IsBattleFinished())
        {
            return;
        }

        _engine.PlayCard(index);
        Render();
    }

    private bool IsBattleFinished()
    {
        return _engine.PlayerHp <= 0 || _engine.EnemyHp <= 0;
    }

    private void Render()
    {
        var intent = _engine.GetCurrentEnemyIntent();
        _statusLabel.Text = $"玩家 HP: {_engine.PlayerHp}/{_engine.PlayerMaxHp} | 格挡: {_engine.PlayerBlock} | 能量: {_engine.Energy}\n" +
                            $"敌人 {_engine.Enemy.Name} HP: {_engine.EnemyHp}/{_engine.Enemy.MaxHp} | 格挡: {_engine.EnemyBlock}";
        _intentLabel.Text = $"敌人下一步: {intent.Name}";

        foreach (var child in _handBox.GetChildren())
        {
            child.QueueFree();
        }

        for (var i = 0; i < _engine.Hand.Count; i++)
        {
            var card = _engine.Hand[i];
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

        var sb = new StringBuilder();
        for (var i = _engine.Log.Count - 1; i >= 0; i--)
        {
            sb.AppendLine(_engine.Log[i]);
        }

        if (_engine.EnemyHp <= 0)
        {
            sb.Insert(0, "\n=== 战斗胜利 ===\n");
        }
        else if (_engine.PlayerHp <= 0)
        {
            sb.Insert(0, "\n=== 战斗失败 ===\n");
        }

        _logLabel.Text = sb.ToString();
    }
}
