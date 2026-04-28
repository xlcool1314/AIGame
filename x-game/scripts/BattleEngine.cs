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
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours

    public EnemyData Enemy { get; private set; } = new();
=======
=======
>>>>>>> theirs
=======
>>>>>>> theirs
=======
>>>>>>> theirs
=======
>>>>>>> theirs
    public int Turn { get; private set; }

    public int PlayerWeak { get; private set; }
    public int EnemyWeak { get; private set; }
    public int PlayerVulnerable { get; private set; }
    public int EnemyVulnerable { get; private set; }

    public EnemyData Enemy { get; private set; } = new();
    public int BonusEnergyPerTurn { get; set; }
    public int BonusBlockPerTurn { get; set; }
    public int BonusPlayerDamage { get; set; }
<<<<<<< ours
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
=======
>>>>>>> theirs

    public readonly List<CardData> DrawPile = new();
    public readonly List<CardData> DiscardPile = new();
    public readonly List<CardData> Hand = new();
    public readonly List<string> Log = new();

    private int _intentIndex;
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
    private Random _random = new();

    public void StartBattle(List<CardData> deck, EnemyData enemy)
    {
        StartBattle(deck, enemy, PlayerMaxHp, PlayerMaxHp);
    }

    public void StartBattle(List<CardData> deck, EnemyData enemy, int playerMaxHp, int playerHp)
=======
    private readonly Random _random = new();

    public void StartBattle(List<CardData> deck, EnemyData enemy, int? playerHpOverride = null)
>>>>>>> theirs
=======
    private readonly Random _random = new();

    public void StartBattle(List<CardData> deck, EnemyData enemy, int? playerHpOverride = null)
>>>>>>> theirs
=======
    private readonly Random _random = new();

    public void StartBattle(List<CardData> deck, EnemyData enemy, int? playerHpOverride = null)
>>>>>>> theirs
=======
    private readonly Random _random = new();

    public void StartBattle(List<CardData> deck, EnemyData enemy, int? playerHpOverride = null)
>>>>>>> theirs
=======
    private readonly Random _random = new();

    public void StartBattle(List<CardData> deck, EnemyData enemy, int? playerHpOverride = null)
>>>>>>> theirs
    {
        Enemy = enemy;
        EnemyHp = enemy.MaxHp;
        EnemyBlock = 0;
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
        PlayerMaxHp = playerMaxHp;
        PlayerHp = Math.Clamp(playerHp, 1, PlayerMaxHp);
        PlayerBlock = 0;

=======
=======
>>>>>>> theirs
=======
>>>>>>> theirs
=======
>>>>>>> theirs
=======
>>>>>>> theirs
        PlayerHp = Math.Clamp(playerHpOverride ?? PlayerMaxHp, 1, PlayerMaxHp);
        PlayerBlock = 0;

        Turn = 0;
        PlayerWeak = 0;
        EnemyWeak = 0;
        PlayerVulnerable = 0;
        EnemyVulnerable = 0;

<<<<<<< ours
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
=======
>>>>>>> theirs
        DrawPile.Clear();
        DiscardPile.Clear();
        Hand.Clear();
        Log.Clear();
        DrawPile.AddRange(deck);
        Shuffle(DrawPile);

        _intentIndex = 0;
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
        StartPlayerTurn();
        Log.Add($"遭遇敌人：{Enemy.Name}");
=======
        Log.Add($"遭遇敌人：{Enemy.Name}");
        StartPlayerTurn();
>>>>>>> theirs
=======
        Log.Add($"遭遇敌人：{Enemy.Name}");
        StartPlayerTurn();
>>>>>>> theirs
=======
        Log.Add($"遭遇敌人：{Enemy.Name}");
        StartPlayerTurn();
>>>>>>> theirs
=======
        Log.Add($"遭遇敌人：{Enemy.Name}");
        StartPlayerTurn();
>>>>>>> theirs
=======
        Log.Add($"遭遇敌人：{Enemy.Name}");
        StartPlayerTurn();
>>>>>>> theirs
    }

    public void StartPlayerTurn()
    {
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
        PlayerBlock = 0;
        Energy = 3;
        DrawCards(5);
        Log.Add("你的回合开始。");
=======
=======
>>>>>>> theirs
=======
>>>>>>> theirs
=======
>>>>>>> theirs
=======
>>>>>>> theirs
        Turn++;
        PlayerBlock = BonusBlockPerTurn;
        Energy = 3 + BonusEnergyPerTurn;
        DrawCards(5);
        Log.Add($"你的第 {Turn} 回合开始。弱化:{PlayerWeak} 易伤:{PlayerVulnerable}");
        if (BonusBlockPerTurn > 0)
        {
            Log.Add($"遗物效果：回合开始获得 {BonusBlockPerTurn} 格挡。");
        }
<<<<<<< ours
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
=======
>>>>>>> theirs
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
            Log.Add($"能量不足，无法使用 {card.Name}");
            return false;
        }

        Energy -= card.Cost;
        ApplyActions(card.Actions, true, card.Name);
        Hand.RemoveAt(handIndex);
        DiscardPile.Add(card);
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours

=======
>>>>>>> theirs
=======
>>>>>>> theirs
=======
>>>>>>> theirs
=======
>>>>>>> theirs
=======
>>>>>>> theirs
        return true;
    }

    public void EndPlayerTurn()
    {
        foreach (var card in Hand)
        {
            DiscardPile.Add(card);
        }
        Hand.Clear();

<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
        EnemyTurn();
        if (EnemyHp > 0)
=======
        TickStatusAfterPlayerTurn();
        EnemyTurn();
        if (EnemyHp > 0 && PlayerHp > 0)
>>>>>>> theirs
=======
        TickStatusAfterPlayerTurn();
        EnemyTurn();
        if (EnemyHp > 0 && PlayerHp > 0)
>>>>>>> theirs
=======
        TickStatusAfterPlayerTurn();
        EnemyTurn();
        if (EnemyHp > 0 && PlayerHp > 0)
>>>>>>> theirs
=======
        TickStatusAfterPlayerTurn();
        EnemyTurn();
        if (EnemyHp > 0 && PlayerHp > 0)
>>>>>>> theirs
=======
        TickStatusAfterPlayerTurn();
        EnemyTurn();
        if (EnemyHp > 0 && PlayerHp > 0)
>>>>>>> theirs
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

    private void EnemyTurn()
    {
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
=======
=======
>>>>>>> theirs
=======
>>>>>>> theirs
=======
>>>>>>> theirs
=======
>>>>>>> theirs
        if (EnemyHp <= 0 || PlayerHp <= 0)
        {
            return;
        }

<<<<<<< ours
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
=======
>>>>>>> theirs
        EnemyBlock = 0;
        var intent = GetCurrentEnemyIntent();
        ApplyActions(intent.Actions, false, $"敌人意图：{intent.Name}");
        _intentIndex++;
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
=======
        TickStatusAfterEnemyTurn();
>>>>>>> theirs
=======
        TickStatusAfterEnemyTurn();
>>>>>>> theirs
=======
        TickStatusAfterEnemyTurn();
>>>>>>> theirs
=======
        TickStatusAfterEnemyTurn();
>>>>>>> theirs
=======
        TickStatusAfterEnemyTurn();
>>>>>>> theirs
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
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
=======
                Log.Add("抽牌堆耗尽，洗牌后继续抽牌。");
>>>>>>> theirs
=======
                Log.Add("抽牌堆耗尽，洗牌后继续抽牌。");
>>>>>>> theirs
=======
                Log.Add("抽牌堆耗尽，洗牌后继续抽牌。");
>>>>>>> theirs
=======
                Log.Add("抽牌堆耗尽，洗牌后继续抽牌。");
>>>>>>> theirs
=======
                Log.Add("抽牌堆耗尽，洗牌后继续抽牌。");
>>>>>>> theirs
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
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
                    if (fromPlayer)
                    {
                        var enemyBlock = EnemyBlock;
                        var damage = ResolveDamage(action.Value, ref enemyBlock);
                        EnemyBlock = enemyBlock;
                        EnemyHp -= damage;
                        Log.Add($"{source} 造成 {damage} 点伤害。敌人生命 {Math.Max(EnemyHp, 0)}");
                    }
                    else
                    {
                        var playerBlock = PlayerBlock;
                        var damage = ResolveDamage(action.Value, ref playerBlock);
                        PlayerBlock = playerBlock;
                        PlayerHp -= damage;
                        Log.Add($"{source} 对你造成 {damage} 点伤害。玩家生命 {Math.Max(PlayerHp, 0)}");
                    }
=======
                    ApplyDamageAction(action.Value, fromPlayer, source);
>>>>>>> theirs
=======
                    ApplyDamageAction(action.Value, fromPlayer, source);
>>>>>>> theirs
=======
                    ApplyDamageAction(action.Value, fromPlayer, source);
>>>>>>> theirs
=======
                    ApplyDamageAction(action.Value, fromPlayer, source);
>>>>>>> theirs
=======
                    ApplyDamageAction(action.Value, fromPlayer, source);
>>>>>>> theirs
                    break;
                case "block":
                    if (fromPlayer)
                    {
                        PlayerBlock += action.Value;
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
                        Log.Add($"{source} 获得 {action.Value} 点格挡。");
=======
>>>>>>> theirs
=======
>>>>>>> theirs
=======
>>>>>>> theirs
=======
>>>>>>> theirs
=======
>>>>>>> theirs
                    }
                    else
                    {
                        EnemyBlock += action.Value;
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
                        Log.Add($"{source} 获得 {action.Value} 点格挡。");
                    }
=======
                    }
                    Log.Add($"{source} 获得 {action.Value} 点格挡。");
>>>>>>> theirs
=======
                    }
                    Log.Add($"{source} 获得 {action.Value} 点格挡。");
>>>>>>> theirs
=======
                    }
                    Log.Add($"{source} 获得 {action.Value} 点格挡。");
>>>>>>> theirs
=======
                    }
                    Log.Add($"{source} 获得 {action.Value} 点格挡。");
>>>>>>> theirs
=======
                    }
                    Log.Add($"{source} 获得 {action.Value} 点格挡。");
>>>>>>> theirs
                    break;
                case "draw":
                    if (fromPlayer)
                    {
                        DrawCards(action.Value);
                        Log.Add($"{source} 抽 {action.Value} 张牌。");
                    }
                    break;
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
=======
=======
>>>>>>> theirs
=======
>>>>>>> theirs
=======
>>>>>>> theirs
=======
>>>>>>> theirs
                case "heal":
                    if (fromPlayer)
                    {
                        PlayerHp = Math.Min(PlayerMaxHp, PlayerHp + action.Value);
                        Log.Add($"{source} 回复 {action.Value} 点生命。");
                    }
                    else
                    {
                        EnemyHp = Math.Min(Enemy.MaxHp, EnemyHp + action.Value);
                        Log.Add($"{source} 回复 {action.Value} 点生命。");
                    }
                    break;
                case "energy":
                    if (fromPlayer)
                    {
                        Energy += action.Value;
                        Log.Add($"{source} 获得 {action.Value} 点能量。");
                    }
                    break;
                case "apply_weak":
                    if (fromPlayer)
                    {
                        EnemyWeak += action.Value;
                        Log.Add($"{source} 施加 {action.Value} 层弱化给敌人。");
                    }
                    else
                    {
                        PlayerWeak += action.Value;
                        Log.Add($"{source} 施加 {action.Value} 层弱化给你。");
                    }
                    break;
                case "apply_vulnerable":
                    if (fromPlayer)
                    {
                        EnemyVulnerable += action.Value;
                        Log.Add($"{source} 施加 {action.Value} 层易伤给敌人。");
                    }
                    else
                    {
                        PlayerVulnerable += action.Value;
                        Log.Add($"{source} 施加 {action.Value} 层易伤给你。");
                    }
                    break;
<<<<<<< ours
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
=======
>>>>>>> theirs
                default:
                    Log.Add($"未知动作类型: {action.Type}");
                    break;
            }
        }
    }

<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
=======
=======
>>>>>>> theirs
=======
>>>>>>> theirs
=======
>>>>>>> theirs
=======
>>>>>>> theirs
    private void ApplyDamageAction(int value, bool fromPlayer, string source)
    {
        if (fromPlayer)
        {
            var boosted = value + BonusPlayerDamage;
            var actual = ComputeModifiedDamage(boosted, attackerWeak: PlayerWeak, defenderVulnerable: EnemyVulnerable);
            var damage = ResolveDamage(actual, ref EnemyBlock);
            EnemyHp = Math.Max(0, EnemyHp - damage);
            Log.Add($"{source} 造成 {damage} 点伤害。敌人生命 {EnemyHp}");
        }
        else
        {
            var actual = ComputeModifiedDamage(value, attackerWeak: EnemyWeak, defenderVulnerable: PlayerVulnerable);
            var damage = ResolveDamage(actual, ref PlayerBlock);
            PlayerHp = Math.Max(0, PlayerHp - damage);
            Log.Add($"{source} 对你造成 {damage} 点伤害。玩家生命 {PlayerHp}");
        }
    }

    private static int ComputeModifiedDamage(int raw, int attackerWeak, int defenderVulnerable)
    {
        var damage = raw;

        if (attackerWeak > 0)
        {
            damage = (int)Math.Ceiling(damage * 0.75f);
        }

        if (defenderVulnerable > 0)
        {
            damage = (int)Math.Ceiling(damage * 1.5f);
        }

        return Math.Max(0, damage);
    }

    private void TickStatusAfterPlayerTurn()
    {
        if (PlayerWeak > 0)
        {
            PlayerWeak--;
        }

        if (PlayerVulnerable > 0)
        {
            PlayerVulnerable--;
        }
    }

    private void TickStatusAfterEnemyTurn()
    {
        if (EnemyWeak > 0)
        {
            EnemyWeak--;
        }

        if (EnemyVulnerable > 0)
        {
            EnemyVulnerable--;
        }
    }

<<<<<<< ours
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
=======
>>>>>>> theirs
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
