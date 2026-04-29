using Godot;
using System;
using System.Collections.Generic;

public partial class BattleEngine : Node
{
    public int PlayerHp { get; private set; } = 70;
    public int PlayerMaxHp { get; private set; } = 70;
    public int PlayerBlock { get; private set; }
    public int Energy { get; private set; }
    public int PlayerWeak { get; private set; }
    public int ThreatLevel { get; private set; }
    public int SelectedEnemyIndex { get; private set; }

    public EnemyData Enemy => CurrentEnemy?.Data ?? new EnemyData();
    public int EnemyHp => CurrentEnemy?.Hp ?? 0;
    public int EnemyMaxHp => CurrentEnemy?.MaxHp ?? 0;
    public int EnemyBlock => CurrentEnemy?.Block ?? 0;
    public int EnemyWeak => CurrentEnemy?.Weak ?? 0;
    public int EnemyVulnerable => CurrentEnemy?.Vulnerable ?? 0;
    public int EnemyStagger => CurrentEnemy?.Stagger ?? 0;
    public int EnemyStaggerLimit => CurrentEnemy?.StaggerLimit ?? 18;
    public int TotalEnemyHp
    {
        get
        {
            var total = 0;
            foreach (var enemy in Enemies)
            {
                total += Math.Max(0, enemy.Hp);
            }

            return total;
        }
    }
    public int TotalEnemyBlock
    {
        get
        {
            var total = 0;
            foreach (var enemy in Enemies)
            {
                total += Math.Max(0, enemy.Block);
            }

            return total;
        }
    }
    public bool AllEnemiesDefeated => GetAliveEnemyCount() == 0;

    public readonly List<BattleEnemyState> Enemies = new();
    public readonly List<CardData> DrawPile = new();
    public readonly List<CardData> DiscardPile = new();
    public readonly List<CardData> Hand = new();
    public readonly List<string> Log = new();

    private readonly Random _random = new();

    private BattleEnemyState? CurrentEnemy
    {
        get
        {
            NormalizeSelectedEnemy();
            return SelectedEnemyIndex >= 0 && SelectedEnemyIndex < Enemies.Count ? Enemies[SelectedEnemyIndex] : null;
        }
    }

    public void StartBattle(List<CardData> deck, EnemyData enemy)
    {
        StartBattle(deck, new List<EnemyData> { enemy }, PlayerMaxHp, PlayerMaxHp, 0);
    }

    public void StartBattle(List<CardData> deck, EnemyData enemy, int playerMaxHp, int playerHp)
    {
        StartBattle(deck, new List<EnemyData> { enemy }, playerMaxHp, playerHp, 0);
    }

    public void StartBattle(List<CardData> deck, EnemyData enemy, int playerMaxHp, int playerHp, int threatLevel)
    {
        StartBattle(deck, new List<EnemyData> { enemy }, playerMaxHp, playerHp, threatLevel);
    }

    public void StartBattle(List<CardData> deck, IReadOnlyList<EnemyData> enemies, int playerMaxHp, int playerHp, int threatLevel)
    {
        ThreatLevel = Math.Clamp(threatLevel, 0, 12);
        PlayerMaxHp = playerMaxHp;
        PlayerHp = Math.Clamp(playerHp, 1, PlayerMaxHp);
        PlayerBlock = 0;
        PlayerWeak = 0;
        SelectedEnemyIndex = 0;

        Enemies.Clear();
        var encounterEnemies = enemies.Count == 0 ? new List<EnemyData> { new() { Name = "未知敌人", NameEn = "Unknown Enemy", MaxHp = 1 } } : enemies;
        var hpBonus = ThreatLevel * (encounterEnemies.Count > 1 ? 3 : 6);
        for (var i = 0; i < encounterEnemies.Count; i++)
        {
            var enemy = encounterEnemies[i];
            Enemies.Add(new BattleEnemyState
            {
                Data = enemy,
                MaxHp = Math.Max(1, enemy.MaxHp + hpBonus),
                Hp = Math.Max(1, enemy.MaxHp + hpBonus),
                StaggerLimit = Math.Max(12, 18 + ThreatLevel * 2 - Math.Max(0, encounterEnemies.Count - 1) * 2)
            });
        }

        DrawPile.Clear();
        DiscardPile.Clear();
        Hand.Clear();
        Log.Clear();
        DrawPile.AddRange(deck);
        Shuffle(DrawPile);

        StartPlayerTurn();
        Log.Add($"遭遇敌群：{FormatEnemyNames()}");
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

    public bool SelectEnemy(int enemyIndex)
    {
        if (enemyIndex < 0 || enemyIndex >= Enemies.Count || !Enemies[enemyIndex].IsAlive)
        {
            return false;
        }

        SelectedEnemyIndex = enemyIndex;
        return true;
    }

    public bool PlayCard(int handIndex)
    {
        return PlayCard(handIndex, SelectedEnemyIndex);
    }

    public bool PlayCard(int handIndex, int targetEnemyIndex)
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

        var targetsEnemy = ActionsTargetEnemy(card.Actions);
        if (targetsEnemy)
        {
            if (!SelectEnemy(targetEnemyIndex))
            {
                Log.Add("需要选择一个仍在战斗中的敌人。");
                return false;
            }
        }

        Energy -= card.Cost;
        ApplyActions(card.Actions, true, card.DisplayName(), targetsEnemy ? SelectedEnemyIndex : -1);
        Hand.RemoveAt(handIndex);
        DiscardPile.Add(card);
        NormalizeSelectedEnemy();
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
        if (!AllEnemiesDefeated && PlayerHp > 0)
        {
            StartPlayerTurn();
        }
    }

    public IntentData GetCurrentEnemyIntent()
    {
        return GetCurrentEnemyIntent(SelectedEnemyIndex);
    }

    public IntentData GetCurrentEnemyIntent(int enemyIndex)
    {
        if (enemyIndex < 0 || enemyIndex >= Enemies.Count || Enemies[enemyIndex].Data.Intents.Count == 0)
        {
            return new IntentData { Name = "待机", NameEn = "Idle" };
        }

        var enemy = Enemies[enemyIndex];
        return enemy.Data.Intents[enemy.IntentIndex % enemy.Data.Intents.Count];
    }

    public string GetIntentPreview()
    {
        var parts = new List<string>();
        for (var i = 0; i < Enemies.Count; i++)
        {
            var enemy = Enemies[i];
            if (!enemy.IsAlive)
            {
                continue;
            }

            parts.Add($"{enemy.Data.DisplayName()}: {FormatIntentPreview(i)}");
        }

        return parts.Count == 0 ? "无行动" : string.Join("\n", parts);
    }

    private string FormatIntentPreview(int enemyIndex)
    {
        var enemy = Enemies[enemyIndex];
        var intent = GetCurrentEnemyIntent(enemyIndex);
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
                    var scaled = ScaleEnemyValue(enemy, action.Value, "damage");
                    var damage = enemy.Weak > 0 ? Math.Max(0, scaled - 2) : scaled;
                    parts.Add($"伤害 {damage}");
                    break;
                case "block":
                    parts.Add($"格挡 {ScaleEnemyValue(enemy, action.Value, "block")}");
                    break;
                case "weak":
                    parts.Add($"虚弱 {action.Duration}");
                    break;
                case "vulnerable":
                    parts.Add($"易伤 {action.Duration}");
                    break;
                case "heal":
                    parts.Add($"治疗 {ScaleEnemyValue(enemy, action.Value, "heal")}");
                    break;
                case "energy":
                    parts.Add($"能量 {action.Value}");
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
        for (var i = 0; i < Enemies.Count; i++)
        {
            var enemy = Enemies[i];
            if (!enemy.IsAlive)
            {
                continue;
            }

            enemy.Block = 0;
            var intent = GetCurrentEnemyIntent(i);
            ApplyActions(intent.Actions, false, $"{enemy.Data.DisplayName()}：{intent.DisplayName()}", i);
            if (PlayerHp <= 0)
            {
                break;
            }

            enemy.IntentIndex++;
            if (enemy.Weak > 0)
            {
                enemy.Weak--;
            }
            if (enemy.Vulnerable > 0)
            {
                enemy.Vulnerable--;
            }
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

    private void ApplyActions(List<CardAction> actions, bool fromPlayer, string source, int enemyIndex)
    {
        foreach (var action in actions)
        {
            var targetEnemy = enemyIndex >= 0 && enemyIndex < Enemies.Count ? Enemies[enemyIndex] : null;
            switch (action.Type)
            {
                case "damage":
                    if (fromPlayer)
                    {
                        if (targetEnemy == null || !targetEnemy.IsAlive)
                        {
                            Log.Add($"{source} 没有可攻击的目标。");
                            break;
                        }

                        var incoming = targetEnemy.Vulnerable > 0 ? (int)Math.Ceiling(action.Value * 1.5f) : action.Value;
                        var block = targetEnemy.Block;
                        var damage = ResolveDamage(incoming, ref block);
                        targetEnemy.Block = block;
                        targetEnemy.Hp -= damage;
                        AddEnemyStagger(targetEnemy, Math.Max(1, damage));
                        Log.Add($"{source} 对 {targetEnemy.Data.DisplayName()} 造成 {damage} 点伤害。目标生命 {Math.Max(targetEnemy.Hp, 0)}");
                        if (targetEnemy.Hp <= 0)
                        {
                            targetEnemy.Block = 0;
                            Log.Add($"{targetEnemy.Data.DisplayName()} 被击倒。");
                        }
                    }
                    else
                    {
                        if (targetEnemy == null || !targetEnemy.IsAlive)
                        {
                            break;
                        }

                        var scaled = ScaleEnemyValue(targetEnemy, action.Value, "damage");
                        var incoming = targetEnemy.Weak > 0 ? Math.Max(0, scaled - 2) : scaled;
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
                    else if (targetEnemy != null && targetEnemy.IsAlive)
                    {
                        var block = ScaleEnemyValue(targetEnemy, action.Value, "block");
                        targetEnemy.Block += block;
                        Log.Add($"{source} 获得 {block} 点格挡。");
                    }
                    break;
                case "draw":
                    if (fromPlayer)
                    {
                        DrawCards(action.Value);
                        Log.Add($"{source} 抽 {action.Value} 张牌。");
                    }
                    break;
                case "heal":
                    if (fromPlayer)
                    {
                        PlayerHp = Math.Min(PlayerMaxHp, PlayerHp + Math.Max(0, action.Value));
                        Log.Add($"{source} 恢复 {action.Value} 点生命。");
                    }
                    else if (targetEnemy != null && targetEnemy.IsAlive)
                    {
                        var heal = ScaleEnemyValue(targetEnemy, action.Value, "heal");
                        targetEnemy.Hp = Math.Min(targetEnemy.MaxHp, targetEnemy.Hp + Math.Max(0, heal));
                        Log.Add($"{source} 恢复 {heal} 点生命。");
                    }
                    break;
                case "energy":
                    if (fromPlayer)
                    {
                        Energy += action.Value;
                        Log.Add($"{source} 获得 {action.Value} 点能量。");
                    }
                    break;
                case "self_damage":
                    if (fromPlayer)
                    {
                        PlayerHp = Math.Max(1, PlayerHp - Math.Max(0, action.Value));
                        Log.Add($"{source} 反噬，失去 {action.Value} 点生命。");
                    }
                    break;
                case "weak":
                    if (fromPlayer)
                    {
                        if (targetEnemy == null || !targetEnemy.IsAlive)
                        {
                            Log.Add($"{source} 没有可影响的目标。");
                            break;
                        }

                        targetEnemy.Weak += Math.Max(1, action.Duration);
                        Log.Add($"{source} 使 {targetEnemy.Data.DisplayName()} 虚弱 {Math.Max(1, action.Duration)} 回合。");
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
                        if (targetEnemy == null || !targetEnemy.IsAlive)
                        {
                            Log.Add($"{source} 没有可影响的目标。");
                            break;
                        }

                        targetEnemy.Vulnerable += Math.Max(1, action.Duration);
                        Log.Add($"{source} 使 {targetEnemy.Data.DisplayName()} 易伤 {Math.Max(1, action.Duration)} 回合。");
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

    private int ScaleEnemyValue(BattleEnemyState enemy, int value, string type)
    {
        if (ThreatLevel <= 0)
        {
            return value;
        }

        return type switch
        {
            "damage" => value + Math.Max(1, (int)Math.Ceiling(ThreatLevel * 0.75f)),
            "block" => value + ThreatLevel * 2,
            "heal" => value + ThreatLevel,
            _ => value
        };
    }

    private void AddEnemyStagger(BattleEnemyState enemy, int amount)
    {
        if (!enemy.IsAlive)
        {
            return;
        }

        enemy.Stagger += amount;
        if (enemy.Stagger < enemy.StaggerLimit)
        {
            return;
        }

        enemy.Stagger = 0;
        enemy.Weak += 1;
        enemy.Vulnerable += 1;
        Log.Add($"{enemy.Data.DisplayName()} 破势，获得 1 回合虚弱和易伤。");
    }

    private int GetAliveEnemyCount()
    {
        var count = 0;
        foreach (var enemy in Enemies)
        {
            if (enemy.IsAlive)
            {
                count++;
            }
        }

        return count;
    }

    private void NormalizeSelectedEnemy()
    {
        if (SelectedEnemyIndex >= 0 && SelectedEnemyIndex < Enemies.Count && Enemies[SelectedEnemyIndex].IsAlive)
        {
            return;
        }

        for (var i = 0; i < Enemies.Count; i++)
        {
            if (Enemies[i].IsAlive)
            {
                SelectedEnemyIndex = i;
                return;
            }
        }

        SelectedEnemyIndex = 0;
    }

    private string FormatEnemyNames()
    {
        var names = new List<string>();
        foreach (var enemy in Enemies)
        {
            names.Add(enemy.Data.DisplayName());
        }

        return string.Join("、", names);
    }

    private static bool ActionsTargetEnemy(List<CardAction> actions)
    {
        foreach (var action in actions)
        {
            if (action.Type is "damage" or "weak" or "vulnerable")
            {
                return true;
            }
        }

        return false;
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

public class BattleEnemyState
{
    public EnemyData Data { get; set; } = new();
    public int Hp { get; set; }
    public int MaxHp { get; set; }
    public int Block { get; set; }
    public int Weak { get; set; }
    public int Vulnerable { get; set; }
    public int Stagger { get; set; }
    public int StaggerLimit { get; set; } = 18;
    public int IntentIndex { get; set; }
    public bool IsAlive => Hp > 0;
}
