using Godot;
using System;
using System.Collections.Generic;

public partial class BattleEngine : Node
{
    public int PlayerHp { get; private set; } = 70;
    public int PlayerMaxHp { get; private set; } = 70;
    public int PlayerBlock { get; private set; }
    public int EnemyHp { get; private set; }
    public int EnemyBlock { get; private set; }
    public int Energy { get; private set; }
    public int PlayerWeak { get; private set; }
    public int EnemyWeak { get; private set; }
    public int EnemyVulnerable { get; private set; }

    public EnemyData Enemy { get; private set; } = new();

    public readonly List<CardData> DrawPile = new();
    public readonly List<CardData> DiscardPile = new();
    public readonly List<CardData> Hand = new();
    public readonly List<string> Log = new();

    private int _intentIndex;
    private Random _random = new();

    public void StartBattle(List<CardData> deck, EnemyData enemy)
    {
        StartBattle(deck, enemy, PlayerMaxHp, PlayerMaxHp);
    }

    public void StartBattle(List<CardData> deck, EnemyData enemy, int playerMaxHp, int playerHp)
    {
        Enemy = enemy;
        EnemyHp = enemy.MaxHp;
        EnemyBlock = 0;
        PlayerMaxHp = playerMaxHp;
        PlayerHp = Math.Clamp(playerHp, 1, PlayerMaxHp);
        PlayerBlock = 0;
        PlayerWeak = 0;
        EnemyWeak = 0;
        EnemyVulnerable = 0;

        DrawPile.Clear();
        DiscardPile.Clear();
        Hand.Clear();
        Log.Clear();
        DrawPile.AddRange(deck);
        Shuffle(DrawPile);

        _intentIndex = 0;
        StartPlayerTurn();
        Log.Add($"遭遇敌人：{Enemy.DisplayName()}");
    }

    public void StartPlayerTurn()
    {
        PlayerBlock = 0;
        Energy = 3;
        if (PlayerWeak > 0)
        {
            PlayerWeak--;
        }
        DrawCards(5);
        Log.Add("你的回合开始。");
    }

    public bool PlayCard(int handIndex)
    {
        if (handIndex < 0 || handIndex >= Hand.Count)
        {
            return false;
        }

        var card = Hand[handIndex];
        if (card.Cost > Energy)
        {
            Log.Add($"能量不足，无法使用 {card.DisplayName()}");
            return false;
        }

        Energy -= card.Cost;
        ApplyActions(card.Actions, true, card.DisplayName());
        Hand.RemoveAt(handIndex);
        DiscardPile.Add(card);
        if (EnemyVulnerable > 0)
        {
            EnemyVulnerable--;
        }

        return true;
    }

    public void EndPlayerTurn()
    {
        foreach (var card in Hand)
        {
            DiscardPile.Add(card);
        }
        Hand.Clear();

        EnemyTurn();
        if (EnemyHp > 0)
        {
            StartPlayerTurn();
        }
    }

    public IntentData GetCurrentEnemyIntent()
    {
        if (Enemy.Intents.Count == 0)
        {
            return new IntentData { Name = "待机" };
        }

        return Enemy.Intents[_intentIndex % Enemy.Intents.Count];
    }

    public string GetIntentPreview()
    {
        var intent = GetCurrentEnemyIntent();
        if (intent.Actions.Count == 0)
        {
            return intent.DisplayName();
        }

        var parts = new List<string>();
        foreach (var action in intent.Actions)
        {
            switch (action.Type)
            {
                case "damage":
                    var damage = EnemyWeak > 0 ? Math.Max(0, action.Value - 2) : action.Value;
                    parts.Add($"伤害 {damage}");
                    break;
                case "block":
                    parts.Add($"格挡 {action.Value}");
                    break;
                case "weak":
                    parts.Add($"虚弱 {action.Duration}");
                    break;
                default:
                    parts.Add(action.Type);
                    break;
            }
        }

        return $"{intent.DisplayName()} / {string.Join(" + ", parts)}";
    }

    private void EnemyTurn()
    {
        EnemyBlock = 0;
        var intent = GetCurrentEnemyIntent();
        ApplyActions(intent.Actions, false, $"敌人意图：{intent.DisplayName()}");
        _intentIndex++;
        if (EnemyWeak > 0)
        {
            EnemyWeak--;
        }
    }

    private void DrawCards(int count)
    {
        for (var i = 0; i < count; i++)
        {
            if (DrawPile.Count == 0)
            {
                if (DiscardPile.Count == 0)
                {
                    return;
                }

                DrawPile.AddRange(DiscardPile);
                DiscardPile.Clear();
                Shuffle(DrawPile);
            }

            var top = DrawPile[0];
            DrawPile.RemoveAt(0);
            Hand.Add(top);
        }
    }

    private void ApplyActions(List<CardAction> actions, bool fromPlayer, string source)
    {
        foreach (var action in actions)
        {
            switch (action.Type)
            {
                case "damage":
                    if (fromPlayer)
                    {
                        var incoming = EnemyVulnerable > 0 ? (int)Math.Ceiling(action.Value * 1.5f) : action.Value;
                        var enemyBlock = EnemyBlock;
                        var damage = ResolveDamage(incoming, ref enemyBlock);
                        EnemyBlock = enemyBlock;
                        EnemyHp -= damage;
                        Log.Add($"{source} 造成 {damage} 点伤害。敌人生命 {Math.Max(EnemyHp, 0)}");
                    }
                    else
                    {
                        var incoming = EnemyWeak > 0 ? Math.Max(0, action.Value - 2) : action.Value;
                        var playerBlock = PlayerBlock;
                        var damage = ResolveDamage(incoming, ref playerBlock);
                        PlayerBlock = playerBlock;
                        PlayerHp -= damage;
                        Log.Add($"{source} 对你造成 {damage} 点伤害。玩家生命 {Math.Max(PlayerHp, 0)}");
                    }
                    break;
                case "block":
                    if (fromPlayer)
                    {
                        PlayerBlock += action.Value;
                        Log.Add($"{source} 获得 {action.Value} 点格挡。");
                    }
                    else
                    {
                        EnemyBlock += action.Value;
                        Log.Add($"{source} 获得 {action.Value} 点格挡。");
                    }
                    break;
                case "draw":
                    if (fromPlayer)
                    {
                        DrawCards(action.Value);
                        Log.Add($"{source} 抽 {action.Value} 张牌。");
                    }
                    break;
                case "weak":
                    if (fromPlayer)
                    {
                        EnemyWeak += Math.Max(1, action.Duration);
                        Log.Add($"{source} 使敌人虚弱 {Math.Max(1, action.Duration)} 回合。");
                    }
                    else
                    {
                        PlayerWeak += Math.Max(1, action.Duration);
                        Log.Add($"{source} 使你虚弱 {Math.Max(1, action.Duration)} 回合。");
                    }
                    break;
                case "vulnerable":
                    if (fromPlayer)
                    {
                        EnemyVulnerable += Math.Max(1, action.Duration);
                        Log.Add($"{source} 使敌人易伤 {Math.Max(1, action.Duration)} 回合。");
                    }
                    break;
                default:
                    Log.Add($"未知动作类型: {action.Type}");
                    break;
            }
        }
    }

    private static int ResolveDamage(int incoming, ref int block)
    {
        var blocked = Math.Min(incoming, block);
        block -= blocked;
        return Math.Max(0, incoming - blocked);
    }

    private void Shuffle(List<CardData> deck)
    {
        for (var i = deck.Count - 1; i > 0; i--)
        {
            var j = _random.Next(i + 1);
            (deck[i], deck[j]) = (deck[j], deck[i]);
        }
    }
}
