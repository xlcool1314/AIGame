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
    public readonly List<ItemStackData> Items = new();
    public readonly List<string> Relics = new();
    public readonly List<List<RunRoom>> MapLayers = new();
    public MinefieldState? Minefield { get; private set; }
    public readonly List<string> Log = new();

    public void StartRun(GameData gameData)
    {
        StartRun(gameData, GameSession.SelectedCharacterId);
    }

    public void StartRun(GameData gameData, string characterId)
    {
        var character = gameData.GetCharacter(characterId);
        PlayerMaxHp = character.MaxHp;
        PlayerHp = PlayerMaxHp;
        Shards = character.Shards;
        CurrentLayerIndex = -1;
        CurrentRoom = null;

        PlayerDeck.Clear();
        PlayerDeck.AddRange(gameData.BuildStarterDeck(character.DeckId));
        Items.Clear();
        foreach (var item in character.StartingItems)
        {
            Items.Add(new ItemStackData { ItemId = item.ItemId, Count = item.Count });
        }
        Relics.Clear();

        BuildMapLayers(gameData);

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

        Items.Clear();
        foreach (var item in saveData.Items)
        {
            Items.Add(new ItemStackData { ItemId = item.ItemId, Count = item.Count });
        }

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
        Log.Add($"进入第 {CurrentLayerIndex + 1} 层：{CurrentRoom.DisplayTitle()}");
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
        if (result == MineRevealResult.Monster)
        {
            Log.Add("惊动矿穴怪物，战斗开始。");
        }
        else if (result == MineRevealResult.Trap)
        {
            PlayerHp = Math.Max(0, PlayerHp - Minefield.TrapDamage);
            Log.Add($"触发陷阱，失去 {Minefield.TrapDamage} 点生命。");
        }
        else if (result == MineRevealResult.Treasure)
        {
            Shards += 18;
            Log.Add("打开宝箱，获得 18 矿晶。");
        }
        else if (result == MineRevealResult.Ore)
        {
            Shards += 8;
            Log.Add("采集矿石，获得 8 矿晶。");
        }
        else if (result == MineRevealResult.Exit)
        {
            Log.Add("你找到了通往下一层的出口，可以继续探索或立刻深入。");
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

    public bool UseInstantItem(ItemData item)
    {
        if (!ConsumeItem(item.Id))
        {
            return false;
        }

        if (item.UseMode == "instant_heal")
        {
            Heal(item.Value);
            Log.Add($"使用 {item.DisplayName()}，恢复 {item.Value} 点生命。");
            return true;
        }

        return false;
    }

    public bool UseTileItem(ItemData item, int index)
    {
        if (Minefield == null || item.UseMode != "target_tile")
        {
            return false;
        }

        if (!Minefield.CanPreview(index))
        {
            Log.Add("这个矿格不需要探测。");
            return false;
        }

        if (!ConsumeItem(item.Id))
        {
            return false;
        }

        var result = Minefield.Preview(index);
        Log.Add($"探测灯照亮目标：{result}。");
        return true;
    }

    public void SyncAfterBattle(int playerHp)
    {
        PlayerHp = Math.Clamp(playerHp, 0, PlayerMaxHp);
    }

    public void ResolveMineMonsterVictory()
    {
        Shards += 8 + Math.Max(CurrentLayerIndex, 0) * 2;
        Log.Add("矿穴怪物被击退，你获得一些矿晶并继续探勘。");
    }

    public void ApplyReward(RewardData reward, CardData? chosenCard)
    {
        Shards += reward.Shards;
        Heal(reward.Heal);

        if (chosenCard != null)
        {
            PlayerDeck.Add(chosenCard);
            Log.Add($"战利品：获得 {reward.Shards} 矿晶，治疗 {reward.Heal} 点，加入卡牌 {chosenCard.DisplayName()}。");
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
            Log.Add($"矿晶不足，无法购买 {card.DisplayName()}。");
            return false;
        }

        Shards -= cost;
        PlayerDeck.Add(card);
        Log.Add($"花费 {cost} 矿晶购买 {card.DisplayName()}。");
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

        Log.Add(choice.DisplayResult());
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

    private void BuildMapLayers(GameData gameData)
    {
        MapLayers.Clear();
        foreach (var layer in gameData.Layers.Layers)
        {
            var rooms = new List<RunRoom>();
            foreach (var room in layer.Rooms)
            {
                rooms.Add(new RunRoom(room.Kind, room.Title, room.TitleEn, room.EnemyId, room.EventId, room.RewardId));
            }

            MapLayers.Add(rooms);
        }
    }

    private bool ConsumeItem(string itemId)
    {
        foreach (var stack in Items)
        {
            if (stack.ItemId != itemId || stack.Count <= 0)
            {
                continue;
            }

            stack.Count--;
            return true;
        }

        Log.Add("没有可用道具。");
        return false;
    }
}

public class RunRoom
{
    public string Kind { get; }
    public string Title { get; }
    public string EnemyId { get; }
    public string EventId { get; }
    public string RewardId { get; }
    public string TitleEn { get; }

    public RunRoom(string kind, string title, string titleEn = "", string enemyId = "", string eventId = "", string rewardId = "")
    {
        Kind = kind;
        Title = title;
        TitleEn = titleEn;
        EnemyId = enemyId;
        EventId = eventId;
        RewardId = rewardId;
    }

    public string DisplayTitle()
    {
        return Localization.Pick(Title, TitleEn);
    }
}

public class MinefieldState
{
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int DangerCount { get; private set; }
    public int RewardCount { get; private set; }
    public int RewardShards { get; private set; }
    public int TrapDamage { get; private set; } = 8;
    public bool ExitFound { get; private set; }
    public bool IsCleared { get; private set; }
    public readonly List<MineCell> Cells = new();

    public static MinefieldState Create(int width, int height, int mineCount, int rewardShards)
    {
        var state = new MinefieldState
        {
            Width = width,
            Height = height,
            RewardShards = rewardShards
        };

        for (var i = 0; i < width * height; i++)
        {
            state.Cells.Add(new MineCell());
        }

        var entranceIndex = 0;
        var exitIndex = state.Cells.Count - 1;
        state.Cells[entranceIndex].Type = MineTileType.Entrance;
        state.Cells[exitIndex].Type = MineTileType.Exit;

        var random = new Random();
        state.PlaceTiles(random, MineTileType.Monster, Math.Max(2, mineCount / 2));
        state.PlaceTiles(random, MineTileType.Trap, Math.Max(2, mineCount - Math.Max(2, mineCount / 2)));
        state.PlaceTiles(random, MineTileType.Treasure, Math.Max(2, width / 2));
        state.PlaceTiles(random, MineTileType.Ore, Math.Max(4, width));

        for (var i = 0; i < state.Cells.Count; i++)
        {
            state.Cells[i].DangerClue = state.CountAdjacent(i, true);
            state.Cells[i].RewardClue = state.CountAdjacent(i, false);
        }

        state.RevealStartArea();
        state.DangerCount = state.CountTiles(true);
        state.RewardCount = state.CountTiles(false);
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
        if (cell.Type == MineTileType.Exit)
        {
            ExitFound = true;
            return MineRevealResult.Exit;
        }

        if (cell.Type == MineTileType.Monster)
        {
            return MineRevealResult.Monster;
        }

        if (cell.Type == MineTileType.Trap)
        {
            return MineRevealResult.Trap;
        }

        if (cell.Type == MineTileType.Treasure)
        {
            UpdateCleared();
            return MineRevealResult.Treasure;
        }

        if (cell.Type == MineTileType.Ore)
        {
            UpdateCleared();
            return MineRevealResult.Ore;
        }

        if (cell.DangerClue == 0 && cell.RewardClue == 0)
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

    public string Preview(int index)
    {
        if (index < 0 || index >= Cells.Count)
        {
            return "未知";
        }

        var cell = Cells[index];
        cell.IsPreviewed = true;
        return cell.Type switch
        {
            MineTileType.Empty => $"安全格 D{cell.DangerClue}/R{cell.RewardClue}",
            MineTileType.Entrance => "入口",
            MineTileType.Exit => "出口",
            MineTileType.Monster => "怪物",
            MineTileType.Trap => "陷阱",
            MineTileType.Treasure => "宝箱",
            MineTileType.Ore => "矿石",
            _ => "未知"
        };
    }

    public bool CanPreview(int index)
    {
        return index >= 0 && index < Cells.Count && !Cells[index].IsRevealed && !Cells[index].IsPreviewed;
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
                if (cell.IsDanger || cell.IsReward || cell.Type == MineTileType.Exit || cell.IsFlagged || cell.IsRevealed)
                {
                    continue;
                }

                cell.IsRevealed = true;
                if (cell.DangerClue == 0 && cell.RewardClue == 0)
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
            if (!cell.IsDanger && cell.Type != MineTileType.Exit && !cell.IsRevealed)
            {
                return;
            }
        }

        IsCleared = true;
    }

    private void PlaceTiles(Random random, MineTileType type, int count)
    {
        var placed = 0;
        var attempts = 0;
        while (placed < count && attempts < Cells.Count * 20)
        {
            attempts++;
            var index = random.Next(Cells.Count);
            if (Cells[index].Type != MineTileType.Empty || IsStartSafeIndex(index))
            {
                continue;
            }

            Cells[index].Type = type;
            placed++;
        }
    }

    private void RevealStartArea()
    {
        Cells[0].IsRevealed = true;
        foreach (var neighbor in GetNeighbors(0))
        {
            if (!Cells[neighbor].IsDanger)
            {
                Cells[neighbor].IsRevealed = true;
            }
        }
    }

    private bool IsStartSafeIndex(int index)
    {
        if (index == 0 || index == Cells.Count - 1)
        {
            return true;
        }

        var x = index % Width;
        var y = index / Width;
        return x <= 1 && y <= 1;
    }

    private int CountTiles(bool danger)
    {
        var count = 0;
        foreach (var cell in Cells)
        {
            if (danger && cell.IsDanger)
            {
                count++;
            }
            else if (!danger && cell.IsReward)
            {
                count++;
            }
        }

        return count;
    }

    private int CountAdjacent(int index, bool danger)
    {
        var count = 0;
        foreach (var neighbor in GetNeighbors(index))
        {
            if (danger && Cells[neighbor].IsDanger)
            {
                count++;
            }
            else if (!danger && Cells[neighbor].IsReward)
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
    public MineTileType Type { get; set; } = MineTileType.Empty;
    public bool IsRevealed { get; set; }
    public bool IsFlagged { get; set; }
    public bool IsPreviewed { get; set; }
    public int DangerClue { get; set; }
    public int RewardClue { get; set; }
    public bool IsDanger => Type == MineTileType.Monster || Type == MineTileType.Trap;
    public bool IsReward => Type == MineTileType.Treasure || Type == MineTileType.Ore;
}

public enum MineTileType
{
    Empty,
    Entrance,
    Exit,
    Monster,
    Trap,
    Treasure,
    Ore
}

public enum MineRevealResult
{
    NoChange,
    Revealed,
    Monster,
    Trap,
    Treasure,
    Ore,
    Exit,
    Cleared
}
