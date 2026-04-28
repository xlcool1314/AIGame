using Godot;
using System;
using System.Collections.Generic;

public partial class RunEngine : Node
{
    public int PlayerHp { get; private set; } = 70;
    public int PlayerMaxHp { get; private set; } = 70;
    public int Shards { get; private set; }
    public int CurrentLayerIndex { get; private set; } = -1;
    public RunRoom? CurrentRoom { get; private set; }

    public readonly List<CardData> PlayerDeck = new();
    public readonly List<string> Relics = new();
    public readonly List<List<RunRoom>> MapLayers = new();
    public MinefieldState? Minefield { get; private set; }
    public readonly List<string> Log = new();

    public void StartRun(GameData gameData)
    {
        PlayerHp = 70;
        PlayerMaxHp = 70;
        Shards = 20;
        CurrentLayerIndex = -1;
        CurrentRoom = null;

        PlayerDeck.Clear();
        PlayerDeck.AddRange(gameData.BuildStarterDeck("starter"));
        Relics.Clear();

        MapLayers.Clear();
        MapLayers.Add(new List<RunRoom>
        {
            new("mine", "浅层矿脉"),
            new("battle", "雾矿史莱姆", enemyId: "slime", rewardId: "basic")
        });
        MapLayers.Add(new List<RunRoom>
        {
            new("mine", "裂隙矿带"),
            new("battle", "晶翼蝠巢", enemyId: "crystal_bat", rewardId: "basic"),
            new("event", "坍塌支道", eventId: "collapsed_tunnel"),
            new("shop", "矿灯商队", rewardId: "shop")
        });
        MapLayers.Add(new List<RunRoom>
        {
            new("rest", "废弃升降机"),
            new("mine", "富矿夹层"),
            new("event", "旧矿工营地", eventId: "old_camp")
        });
        MapLayers.Add(new List<RunRoom>
        {
            new("battle", "晶壳守卫", enemyId: "crystal_guard", rewardId: "elite"),
            new("battle", "矿井监管者", enemyId: "mine_overseer", rewardId: "elite")
        });
        MapLayers.Add(new List<RunRoom>
        {
            new("complete", "矿井深处")
        });

        Log.Clear();
        Log.Add("你点亮矿灯，进入迷雾矿井。");
    }

    public void LoadFromSave(GameData gameData, RunSaveData saveData)
    {
        StartRun(gameData);
        PlayerHp = Math.Clamp(saveData.PlayerHp, 0, saveData.PlayerMaxHp);
        PlayerMaxHp = Math.Max(1, saveData.PlayerMaxHp);
        Shards = Math.Max(0, saveData.Shards);
        CurrentLayerIndex = Math.Clamp(saveData.CurrentLayerIndex, -1, MapLayers.Count - 1);
        CurrentRoom = null;
        Minefield = null;

        PlayerDeck.Clear();
        foreach (var cardId in saveData.DeckCardIds)
        {
            PlayerDeck.Add(gameData.GetCard(cardId));
        }

        if (PlayerDeck.Count == 0)
        {
            PlayerDeck.AddRange(gameData.BuildStarterDeck("starter"));
        }

        Relics.Clear();
        Relics.AddRange(saveData.Relics);

        Log.Clear();
        Log.AddRange(saveData.Log);
        Log.Add("读取存档，矿灯重新亮起。");
    }

    public bool HasNextLayer()
    {
        return CurrentLayerIndex + 1 < MapLayers.Count;
    }

    public IReadOnlyList<RunRoom> GetNextRoomChoices()
    {
        if (!HasNextLayer())
        {
            return Array.Empty<RunRoom>();
        }

        return MapLayers[CurrentLayerIndex + 1];
    }

    public RunRoom EnterNextRoom(int choiceIndex)
    {
        var choices = GetNextRoomChoices();
        if (choices.Count == 0)
        {
            throw new InvalidOperationException("已经没有可进入的房间。");
        }

        var clampedIndex = Math.Clamp(choiceIndex, 0, choices.Count - 1);
        CurrentLayerIndex++;
        CurrentRoom = choices[clampedIndex];
        Minefield = null;
        Log.Add($"进入第 {CurrentLayerIndex + 1} 层：{CurrentRoom.Title}");
        return CurrentRoom;
    }

    public void StartMinefield()
    {
        var layer = Math.Max(CurrentLayerIndex, 0);
        var width = layer >= 2 ? 6 : 5;
        var height = layer >= 2 ? 6 : 5;
        var mineCount = Math.Min(width * height - 6, 4 + layer * 2);
        var reward = 14 + layer * 6;
        Minefield = MinefieldState.Create(width, height, mineCount, reward);
        Log.Add($"开始探勘：{width}x{height} 矿区，疑似雷区 {mineCount} 处。");
    }

    public MineRevealResult RevealMineCell(int index)
    {
        if (Minefield == null)
        {
            return MineRevealResult.NoChange;
        }

        var result = Minefield.Reveal(index);
        if (result == MineRevealResult.HitMine)
        {
            PlayerHp = Math.Max(0, PlayerHp - Minefield.MineDamage);
            Log.Add($"触发暗雷，失去 {Minefield.MineDamage} 点生命。");
        }

        if (Minefield.IsCleared)
        {
            Shards += Minefield.RewardShards;
            Log.Add($"矿区清理完成，获得 {Minefield.RewardShards} 矿晶。");
        }

        return result;
    }

    public void ToggleMineFlag(int index)
    {
        Minefield?.ToggleFlag(index);
    }

    public void SyncAfterBattle(int playerHp)
    {
        PlayerHp = Math.Clamp(playerHp, 0, PlayerMaxHp);
    }

    public void ApplyReward(RewardData reward, CardData? chosenCard)
    {
        Shards += reward.Shards;
        Heal(reward.Heal);

        if (chosenCard != null)
        {
            PlayerDeck.Add(chosenCard);
            Log.Add($"战利品：获得 {reward.Shards} 矿晶，治疗 {reward.Heal} 点，加入卡牌 {chosenCard.Name}。");
        }
        else
        {
            Log.Add($"战利品：获得 {reward.Shards} 矿晶，治疗 {reward.Heal} 点。");
        }
    }

    public bool BuyCard(CardData card, int cost)
    {
        if (Shards < cost)
        {
            Log.Add($"矿晶不足，无法购买 {card.Name}。");
            return false;
        }

        Shards -= cost;
        PlayerDeck.Add(card);
        Log.Add($"花费 {cost} 矿晶购买 {card.Name}。");
        return true;
    }

    public bool BuyHeal(int amount, int cost)
    {
        if (Shards < cost)
        {
            Log.Add("矿晶不足，无法购买治疗。");
            return false;
        }

        Shards -= cost;
        Heal(amount);
        Log.Add($"花费 {cost} 矿晶修理装备，恢复 {amount} 点生命。");
        return true;
    }

    public void ApplyEventChoice(EventChoiceData choice, GameData gameData)
    {
        foreach (var action in choice.Actions)
        {
            ApplyRunAction(action, gameData);
        }

        Log.Add(choice.Result);
    }

    public void Rest(string mode)
    {
        if (mode == "forge")
        {
            Shards += 12;
            Relics.Add("校准矿灯");
            Log.Add("你校准矿灯并整理矿石，获得 12 矿晶和遗物：校准矿灯。");
            return;
        }

        var amount = Math.Max(12, PlayerMaxHp / 3);
        Heal(amount);
        Log.Add($"你在废弃升降机旁短暂休整，恢复 {amount} 点生命。");
    }

    private void ApplyRunAction(RunAction action, GameData gameData)
    {
        switch (action.Type)
        {
            case "heal":
                Heal(action.Value);
                break;
            case "damage":
                PlayerHp = Math.Max(1, PlayerHp - action.Value);
                break;
            case "max_hp":
                PlayerMaxHp += action.Value;
                PlayerHp = Math.Min(PlayerMaxHp, PlayerHp + action.Value);
                break;
            case "shards":
                Shards += action.Value;
                break;
            case "add_card":
                PlayerDeck.Add(gameData.GetCard(action.CardId));
                break;
            case "relic":
                Relics.Add(action.RelicId);
                break;
            default:
                Log.Add($"未知跑局动作: {action.Type}");
                break;
        }
    }

    private void Heal(int amount)
    {
        PlayerHp = Math.Min(PlayerMaxHp, PlayerHp + Math.Max(0, amount));
    }
}

public class RunRoom
{
    public string Kind { get; }
    public string Title { get; }
    public string EnemyId { get; }
    public string EventId { get; }
    public string RewardId { get; }

    public RunRoom(string kind, string title, string enemyId = "", string eventId = "", string rewardId = "")
    {
        Kind = kind;
        Title = title;
        EnemyId = enemyId;
        EventId = eventId;
        RewardId = rewardId;
    }
}

public class MinefieldState
{
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int MineCount { get; private set; }
    public int RewardShards { get; private set; }
    public int MineDamage { get; private set; } = 8;
    public bool IsCleared { get; private set; }
    public readonly List<MineCell> Cells = new();

    public static MinefieldState Create(int width, int height, int mineCount, int rewardShards)
    {
        var state = new MinefieldState
        {
            Width = width,
            Height = height,
            MineCount = mineCount,
            RewardShards = rewardShards
        };

        for (var i = 0; i < width * height; i++)
        {
            state.Cells.Add(new MineCell());
        }

        var random = new Random();
        var placed = 0;
        while (placed < mineCount)
        {
            var index = random.Next(state.Cells.Count);
            if (state.Cells[index].IsMine)
            {
                continue;
            }

            state.Cells[index].IsMine = true;
            placed++;
        }

        for (var i = 0; i < state.Cells.Count; i++)
        {
            state.Cells[i].AdjacentMines = state.CountAdjacentMines(i);
        }

        return state;
    }

    public MineRevealResult Reveal(int index)
    {
        if (index < 0 || index >= Cells.Count || IsCleared)
        {
            return MineRevealResult.NoChange;
        }

        var cell = Cells[index];
        if (cell.IsRevealed || cell.IsFlagged)
        {
            return MineRevealResult.NoChange;
        }

        cell.IsRevealed = true;
        if (cell.IsMine)
        {
            return MineRevealResult.HitMine;
        }

        if (cell.AdjacentMines == 0)
        {
            FloodReveal(index);
        }

        UpdateCleared();
        return IsCleared ? MineRevealResult.Cleared : MineRevealResult.Revealed;
    }

    public void ToggleFlag(int index)
    {
        if (index < 0 || index >= Cells.Count || IsCleared)
        {
            return;
        }

        var cell = Cells[index];
        if (!cell.IsRevealed)
        {
            cell.IsFlagged = !cell.IsFlagged;
        }
    }

    private void FloodReveal(int startIndex)
    {
        var queue = new Queue<int>();
        queue.Enqueue(startIndex);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var neighbor in GetNeighbors(current))
            {
                var cell = Cells[neighbor];
                if (cell.IsMine || cell.IsFlagged || cell.IsRevealed)
                {
                    continue;
                }

                cell.IsRevealed = true;
                if (cell.AdjacentMines == 0)
                {
                    queue.Enqueue(neighbor);
                }
            }
        }
    }

    private void UpdateCleared()
    {
        foreach (var cell in Cells)
        {
            if (!cell.IsMine && !cell.IsRevealed)
            {
                return;
            }
        }

        IsCleared = true;
    }

    private int CountAdjacentMines(int index)
    {
        var count = 0;
        foreach (var neighbor in GetNeighbors(index))
        {
            if (Cells[neighbor].IsMine)
            {
                count++;
            }
        }

        return count;
    }

    private IEnumerable<int> GetNeighbors(int index)
    {
        var x = index % Width;
        var y = index / Width;

        for (var offsetY = -1; offsetY <= 1; offsetY++)
        {
            for (var offsetX = -1; offsetX <= 1; offsetX++)
            {
                if (offsetX == 0 && offsetY == 0)
                {
                    continue;
                }

                var nx = x + offsetX;
                var ny = y + offsetY;
                if (nx >= 0 && nx < Width && ny >= 0 && ny < Height)
                {
                    yield return ny * Width + nx;
                }
            }
        }
    }
}

public class MineCell
{
    public bool IsMine { get; set; }
    public bool IsRevealed { get; set; }
    public bool IsFlagged { get; set; }
    public int AdjacentMines { get; set; }
}

public enum MineRevealResult
{
    NoChange,
    Revealed,
    HitMine,
    Cleared
}
