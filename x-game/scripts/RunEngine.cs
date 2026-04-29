using Godot;
using System;
using System.Collections.Generic;

public partial class RunEngine : Node
{
    public int PlayerHp { get; private set; } = 70;
    public int PlayerMaxHp { get; private set; } = 70;
    public int Shards { get; private set; }
    public int CurrentLayerIndex { get; private set; } = -1;
    public int RunSeed { get; private set; }
    public int LampOil { get; private set; } = 100;
    public int MaxLampOil { get; private set; } = 100;
    public int FogPressure { get; private set; }
    public int RoomsCompleted { get; private set; }
    public int BattlesWon { get; private set; }
    public int MinesCleared { get; private set; }
    public int Score { get; private set; }
    public string ObjectiveId { get; private set; } = string.Empty;
    public string ObjectiveTitle { get; private set; } = string.Empty;
    public string ObjectiveTitleEn { get; private set; } = string.Empty;
    public string ObjectiveType { get; private set; } = string.Empty;
    public int ObjectiveTarget { get; private set; }
    public int ObjectiveEmberReward { get; private set; }
    public string CurrentRoomNodeId { get; private set; } = string.Empty;
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
        RunSeed = Random.Shared.Next(1, int.MaxValue);
        MaxLampOil = SaveManager.IsUnlocked("start_lamp_plus") ? 115 : 100;
        LampOil = MaxLampOil;
        FogPressure = 0;
        RoomsCompleted = 0;
        BattlesWon = 0;
        MinesCleared = 0;
        Score = 0;
        CurrentRoomNodeId = string.Empty;
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
        SelectObjective(gameData);

        Log.Clear();
        Log.Add("你点亮矿灯，进入迷雾矿井。");
    }

    public void LoadFromSave(GameData gameData, RunSaveData saveData)
    {
        StartRun(gameData);
        RunSeed = saveData.RunSeed > 0 ? saveData.RunSeed : Random.Shared.Next(1, int.MaxValue);
        PlayerHp = Math.Clamp(saveData.PlayerHp, 0, saveData.PlayerMaxHp);
        PlayerMaxHp = Math.Max(1, saveData.PlayerMaxHp);
        Shards = Math.Max(0, saveData.Shards);
        CurrentLayerIndex = Math.Clamp(saveData.CurrentLayerIndex, -1, MapLayers.Count - 1);
        LampOil = Math.Clamp(saveData.LampOil <= 0 ? 70 : saveData.LampOil, 0, MaxLampOil);
        FogPressure = Math.Max(0, saveData.FogPressure);
        RoomsCompleted = Math.Max(0, saveData.RoomsCompleted);
        BattlesWon = Math.Max(0, saveData.BattlesWon);
        MinesCleared = Math.Max(0, saveData.MinesCleared);
        Score = Math.Max(0, saveData.Score);
        CurrentRoomNodeId = saveData.CurrentRoomNodeId ?? string.Empty;
        ObjectiveId = saveData.ObjectiveId;
        ObjectiveTitle = saveData.ObjectiveTitle;
        ObjectiveTitleEn = saveData.ObjectiveTitleEn;
        ObjectiveType = saveData.ObjectiveType;
        ObjectiveTarget = saveData.ObjectiveTarget;
        ObjectiveEmberReward = saveData.ObjectiveEmberReward;
        if (string.IsNullOrWhiteSpace(ObjectiveId))
        {
            SelectObjective(gameData);
        }
        BuildMapLayers(gameData);
        CurrentRoom = FindRoomByNodeId(CurrentRoomNodeId);
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

        if (string.IsNullOrWhiteSpace(CurrentRoomNodeId) || CurrentRoom == null)
        {
            return MapLayers[CurrentLayerIndex + 1];
        }

        var nextRooms = new List<RunRoom>();
        foreach (var nodeId in CurrentRoom.NextNodeIds)
        {
            var room = FindRoomByNodeId(nodeId);
            if (room != null)
            {
                nextRooms.Add(room);
            }
        }

        return nextRooms;
    }

    public bool IsRoomReachable(RunRoom room)
    {
        if (!HasNextLayer() || room.LayerIndex != CurrentLayerIndex + 1)
        {
            return false;
        }

        foreach (var choice in GetNextRoomChoices())
        {
            if (choice.NodeId == room.NodeId)
            {
                return true;
            }
        }

        return false;
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
        CurrentRoomNodeId = CurrentRoom.NodeId;
        Minefield = null;
        ApplyRoomPressure(CurrentRoom);
        Log.Add($"进入第 {CurrentLayerIndex + 1} 层：{CurrentRoom.DisplayTitle()}");
        return CurrentRoom;
    }

    public void StartMinefield()
    {
        var config = CurrentRoom?.MineConfig ?? new MineRoomConfig();
        var seed = HashCode.Combine(RunSeed, CurrentLayerIndex, CurrentRoom?.Kind ?? string.Empty, CurrentRoom?.TitleEn ?? string.Empty);
        Minefield = MinefieldState.Create(config, seed);
        var dangerCount = config.Monsters + config.Traps;
        Log.Add($"开始探勘：{config.Width}x{config.Height} 矿区，疑似雷区 {dangerCount} 处。");
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
            MinesCleared++;
            Score += 18 + Minefield.RewardShards;
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
        BattlesWon++;
        Score += 12 + Math.Max(CurrentLayerIndex, 0) * 4;
        Shards += 8 + Math.Max(CurrentLayerIndex, 0) * 2;
        Log.Add("矿穴怪物被击退，你获得一些矿晶并继续探勘。");
    }

    public void ApplyReward(RewardData reward, CardData? chosenCard, int bonusPercent = 0)
    {
        var shardReward = reward.Shards + (reward.Shards * Math.Max(0, bonusPercent) / 100);
        BattlesWon++;
        Score += 25 + shardReward + Math.Max(0, bonusPercent);
        Shards += shardReward;
        Heal(reward.Heal);

        if (chosenCard != null)
        {
            PlayerDeck.Add(chosenCard);
            Log.Add($"战利品：获得 {shardReward} 矿晶，治疗 {reward.Heal} 点，加入卡牌 {chosenCard.DisplayName()}。");
        }
        else
        {
            Log.Add($"战利品：获得 {shardReward} 矿晶，治疗 {reward.Heal} 点。");
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

    public bool UpgradeCard(int deckIndex, GameData gameData, int cost = 0)
    {
        if (deckIndex < 0 || deckIndex >= PlayerDeck.Count)
        {
            return false;
        }

        var card = PlayerDeck[deckIndex];
        if (string.IsNullOrWhiteSpace(card.UpgradeTo))
        {
            Log.Add($"{card.DisplayName()} 暂时没有可用升级。");
            return false;
        }

        if (Shards < cost)
        {
            Log.Add($"矿晶不足，无法升级 {card.DisplayName()}。");
            return false;
        }

        Shards -= cost;
        var upgraded = gameData.GetCard(card.UpgradeTo);
        PlayerDeck[deckIndex] = upgraded;
        Log.Add($"牌组整备：{card.DisplayName()} 升级为 {upgraded.DisplayName()}。");
        return true;
    }

    public bool RemoveCard(int deckIndex, int cost)
    {
        if (deckIndex < 0 || deckIndex >= PlayerDeck.Count || PlayerDeck.Count <= 1)
        {
            return false;
        }

        if (Shards < cost)
        {
            Log.Add("矿晶不足，无法精简牌组。");
            return false;
        }

        Shards -= cost;
        var removed = PlayerDeck[deckIndex];
        PlayerDeck.RemoveAt(deckIndex);
        Log.Add($"牌组整备：移除 {removed.DisplayName()}。");
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
            RestoreLamp(18);
            Relics.Add("校准矿灯");
            Log.Add("你校准矿灯并整理矿石，获得 12 矿晶、18 灯油和遗物：校准矿灯。");
            return;
        }

        var amount = Math.Max(12, PlayerMaxHp / 3);
        Heal(amount);
        RestoreLamp(10);
        Log.Add($"你在废弃升降机旁短暂休整，恢复 {amount} 点生命和 10 灯油。");
    }

    public int CalculateMetaEmbers(bool victory)
    {
        var depth = Math.Max(0, CurrentLayerIndex + 1);
        var ember = depth * 3 + BattlesWon * 2 + MinesCleared * 3 + Shards / 20 + Score / 60;
        if (victory)
        {
            ember += 18;
        }

        return Math.Max(1, ember);
    }

    public string DisplayObjectiveTitle()
    {
        return Localization.Pick(ObjectiveTitle, ObjectiveTitleEn);
    }

    public int GetObjectiveProgress()
    {
        return ObjectiveType switch
        {
            "depth" => Math.Max(0, CurrentLayerIndex + 1),
            "battles" => BattlesWon,
            "mines" => MinesCleared,
            "score" => Score,
            "shards" => Shards,
            _ => 0
        };
    }

    public bool IsObjectiveComplete()
    {
        return !string.IsNullOrWhiteSpace(ObjectiveId) && GetObjectiveProgress() >= ObjectiveTarget;
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
        for (var layerIndex = 0; layerIndex < gameData.Layers.Layers.Count; layerIndex++)
        {
            var layer = gameData.Layers.Layers[layerIndex];
            var rooms = new List<RunRoom>();
            for (var roomIndex = 0; roomIndex < layer.Rooms.Count; roomIndex++)
            {
                var room = layer.Rooms[roomIndex];
                rooms.Add(new RunRoom(
                    $"L{layerIndex}R{roomIndex}",
                    layerIndex,
                    roomIndex,
                    room.Kind,
                    room.Title,
                    room.TitleEn,
                    room.EnemyId,
                    room.EnemyIds,
                    room.EventId,
                    room.RewardId,
                    room.MineConfig,
                    room.Risk,
                    room.LampCost,
                    room.RewardBonus));
            }

            MapLayers.Add(rooms);
        }

        BuildMapConnections();
    }

    private void BuildMapConnections()
    {
        for (var layerIndex = 0; layerIndex < MapLayers.Count - 1; layerIndex++)
        {
            var currentLayer = MapLayers[layerIndex];
            var nextLayer = MapLayers[layerIndex + 1];
            if (currentLayer.Count == 0 || nextLayer.Count == 0)
            {
                continue;
            }

            var random = new Random(HashCode.Combine(RunSeed, layerIndex, 701));
            for (var i = 0; i < currentLayer.Count; i++)
            {
                var room = currentLayer[i];
                var mapped = currentLayer.Count == 1
                    ? nextLayer.Count / 2
                    : (int)Math.Round(i * (nextLayer.Count - 1) / (float)Math.Max(1, currentLayer.Count - 1));
                AddConnection(room, nextLayer[Math.Clamp(mapped, 0, nextLayer.Count - 1)]);

                if (nextLayer.Count > 1)
                {
                    var branchDirection = random.Next(0, 2) == 0 ? -1 : 1;
                    var branchIndex = Math.Clamp(mapped + branchDirection, 0, nextLayer.Count - 1);
                    if (branchIndex != mapped && random.NextDouble() < 0.72)
                    {
                        AddConnection(room, nextLayer[branchIndex]);
                    }
                }
            }

            for (var nextIndex = 0; nextIndex < nextLayer.Count; nextIndex++)
            {
                if (HasIncomingConnection(currentLayer, nextLayer[nextIndex].NodeId))
                {
                    continue;
                }

                var sourceIndex = nextLayer.Count == 1
                    ? 0
                    : (int)Math.Round(nextIndex * (currentLayer.Count - 1) / (float)Math.Max(1, nextLayer.Count - 1));
                AddConnection(currentLayer[Math.Clamp(sourceIndex, 0, currentLayer.Count - 1)], nextLayer[nextIndex]);
            }
        }
    }

    private static void AddConnection(RunRoom from, RunRoom to)
    {
        if (!from.NextNodeIds.Contains(to.NodeId))
        {
            from.NextNodeIds.Add(to.NodeId);
        }
    }

    private static bool HasIncomingConnection(List<RunRoom> sourceLayer, string nodeId)
    {
        foreach (var room in sourceLayer)
        {
            if (room.NextNodeIds.Contains(nodeId))
            {
                return true;
            }
        }

        return false;
    }

    private RunRoom? FindRoomByNodeId(string nodeId)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
        {
            return null;
        }

        foreach (var layer in MapLayers)
        {
            foreach (var room in layer)
            {
                if (room.NodeId == nodeId)
                {
                    return room;
                }
            }
        }

        return null;
    }

    private void SelectObjective(GameData gameData)
    {
        if (gameData.Objectives.Objectives.Count == 0)
        {
            ObjectiveId = string.Empty;
            ObjectiveTitle = string.Empty;
            ObjectiveTitleEn = string.Empty;
            ObjectiveType = string.Empty;
            ObjectiveTarget = 0;
            ObjectiveEmberReward = 0;
            return;
        }

        var random = new Random(HashCode.Combine(RunSeed, GameSession.SelectedCharacterId));
        var objective = gameData.Objectives.Objectives[random.Next(gameData.Objectives.Objectives.Count)];
        ObjectiveId = objective.Id;
        ObjectiveTitle = objective.Title;
        ObjectiveTitleEn = objective.TitleEn;
        ObjectiveType = objective.Type;
        ObjectiveTarget = objective.Target;
        ObjectiveEmberReward = objective.EmberReward;
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

    private void ApplyRoomPressure(RunRoom room)
    {
        RoomsCompleted++;
        var layerPressure = Math.Max(0, CurrentLayerIndex) * 2;
        var riskPressure = Math.Max(1, room.Risk) * 3;
        var lampCost = Math.Max(0, room.LampCost + layerPressure + riskPressure);
        LampOil -= lampCost;
        FogPressure += Math.Max(0, room.Risk - 1) + Math.Max(0, CurrentLayerIndex / 2);
        Score += Math.Max(1, room.Risk) * 6;

        if (LampOil >= 0)
        {
            Log.Add($"矿灯消耗 {lampCost}。剩余灯油 {LampOil}/{MaxLampOil}。");
            return;
        }

        var damage = Math.Min(Math.Max(0, PlayerHp - 1), Math.Abs(LampOil) + FogPressure);
        PlayerHp = Math.Max(1, PlayerHp - damage);
        LampOil = 0;
        Log.Add($"灯油见底，雾压造成 {damage} 点伤害。雾压 {FogPressure}。");
    }

    private void RestoreLamp(int amount)
    {
        LampOil = Math.Min(MaxLampOil, LampOil + Math.Max(0, amount));
    }
}

public class RunRoom
{
    public string NodeId { get; }
    public int LayerIndex { get; }
    public int ColumnIndex { get; }
    public string Kind { get; }
    public string Title { get; }
    public string EnemyId { get; }
    public List<string> EnemyIds { get; }
    public string EventId { get; }
    public string RewardId { get; }
    public string TitleEn { get; }
    public MineRoomConfig MineConfig { get; }
    public int Risk { get; }
    public int LampCost { get; }
    public int RewardBonus { get; }
    public List<string> NextNodeIds { get; } = new();

    public RunRoom(string nodeId, int layerIndex, int columnIndex, string kind, string title, string titleEn = "", string enemyId = "", List<string>? enemyIds = null, string eventId = "", string rewardId = "", MineRoomConfig? mineConfig = null, int risk = 1, int lampCost = 8, int rewardBonus = 0)
    {
        NodeId = nodeId;
        LayerIndex = layerIndex;
        ColumnIndex = columnIndex;
        Kind = kind;
        Title = title;
        TitleEn = titleEn;
        EnemyId = enemyId;
        EnemyIds = enemyIds == null ? new List<string>() : new List<string>(enemyIds);
        EventId = eventId;
        RewardId = rewardId;
        MineConfig = mineConfig ?? new MineRoomConfig();
        Risk = Math.Clamp(risk, 1, 4);
        LampCost = Math.Max(0, lampCost);
        RewardBonus = Math.Max(0, rewardBonus);
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
    public int Seed { get; private set; }
    public bool ExitFound { get; private set; }
    public bool IsCleared { get; private set; }
    public readonly List<MineCell> Cells = new();

    public static MinefieldState Create(MineRoomConfig config, int seed)
    {
        var width = Math.Clamp(config.Width, 4, 8);
        var height = Math.Clamp(config.Height, 4, 8);
        var maxPayload = Math.Max(0, width * height - 6);
        var monsters = Math.Clamp(config.Monsters, 0, maxPayload);
        var traps = Math.Clamp(config.Traps, 0, Math.Max(0, maxPayload - monsters));
        var treasures = Math.Clamp(config.Treasures, 0, Math.Max(0, maxPayload - monsters - traps));
        var ores = Math.Clamp(config.Ores, 0, Math.Max(0, maxPayload - monsters - traps - treasures));
        var state = new MinefieldState
        {
            Width = width,
            Height = height,
            RewardShards = Math.Max(0, config.ClearReward),
            TrapDamage = Math.Max(1, config.TrapDamage),
            Seed = seed
        };

        for (var i = 0; i < width * height; i++)
        {
            state.Cells.Add(new MineCell());
        }

        var entranceIndex = 0;
        var exitIndex = state.Cells.Count - 1;
        state.Cells[entranceIndex].Type = MineTileType.Entrance;
        state.Cells[exitIndex].Type = MineTileType.Exit;

        var random = new Random(seed);
        state.PlaceTiles(random, MineTileType.Monster, monsters);
        state.PlaceTiles(random, MineTileType.Trap, traps);
        state.PlaceTiles(random, MineTileType.Treasure, treasures);
        state.PlaceTiles(random, MineTileType.Ore, ores);

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

    public int CountType(MineTileType type)
    {
        var count = 0;
        foreach (var cell in Cells)
        {
            if (cell.Type == type)
            {
                count++;
            }
        }

        return count;
    }

    public int CountRevealed()
    {
        var count = 0;
        foreach (var cell in Cells)
        {
            if (cell.IsRevealed)
            {
                count++;
            }
        }

        return count;
    }

    public int CountFlags()
    {
        var count = 0;
        foreach (var cell in Cells)
        {
            if (cell.IsFlagged)
            {
                count++;
            }
        }

        return count;
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
