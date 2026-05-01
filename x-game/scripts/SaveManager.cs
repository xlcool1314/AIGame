using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public static class SaveManager
{
    private const string SavePath = "user://save_run.json";
    private const string MetaPath = "user://meta_progress.json";

    public static bool HasSave()
    {
        return FileAccess.FileExists(SavePath);
    }

    public static void SaveRun(RunEngine run)
    {
        var save = RunSaveData.FromRun(run);
        using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
        file.StoreString(JsonSerializer.Serialize(save, new JsonSerializerOptions { WriteIndented = true }));
    }

    public static RunSaveData? LoadRun()
    {
        if (!HasSave())
        {
            return null;
        }

        using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
        var json = file.GetAsText();
        return JsonSerializer.Deserialize<RunSaveData>(json);
    }

    public static MetaProgressData LoadMeta()
    {
        if (!FileAccess.FileExists(MetaPath))
        {
            return new MetaProgressData();
        }

        using var file = FileAccess.Open(MetaPath, FileAccess.ModeFlags.Read);
        var json = file.GetAsText();
        return JsonSerializer.Deserialize<MetaProgressData>(json) ?? new MetaProgressData();
    }

    public static MetaProgressData RecordRun(RunEngine run, bool victory)
    {
        var meta = LoadMeta();
        var earned = run.CalculateMetaEmbers(victory);
        var objectiveBonus = run.IsObjectiveComplete() ? run.ObjectiveEmberReward : 0;
        earned += objectiveBonus;
        meta.TotalEmbers += earned;
        meta.LastEarnedEmbers = earned;
        meta.LastObjectiveBonus = objectiveBonus;
        meta.BestDepth = Math.Max(meta.BestDepth, Math.Max(0, run.CurrentLayerIndex + 1));
        meta.BestScore = Math.Max(meta.BestScore, run.Score);
        meta.RunsCompleted++;
        if (victory)
        {
            meta.Victories++;
        }
        if (run.IsObjectiveComplete() &&
            !string.IsNullOrWhiteSpace(run.ObjectiveId) &&
            !meta.CompletedObjectiveIds.Contains(run.ObjectiveId))
        {
            meta.CompletedObjectiveIds.Add(run.ObjectiveId);
        }

        using var file = FileAccess.Open(MetaPath, FileAccess.ModeFlags.Write);
        file.StoreString(JsonSerializer.Serialize(meta, new JsonSerializerOptions { WriteIndented = true }));
        return meta;
    }

    public static bool IsUnlocked(string unlockId)
    {
        if (string.IsNullOrWhiteSpace(unlockId))
        {
            return true;
        }

        return LoadMeta().UnlockedIds.Contains(unlockId);
    }

    public static bool TryUnlock(UnlockData unlock, out string message)
    {
        var meta = LoadMeta();
        if (meta.UnlockedIds.Contains(unlock.Id))
        {
            message = Localization.Language == Localization.English ? "Already unlocked." : "已经解锁。";
            return false;
        }

        if (!MeetsUnlockRequirements(unlock, meta, out var requirementMessage))
        {
            message = requirementMessage;
            return false;
        }

        if (meta.TotalEmbers < unlock.Cost)
        {
            message = Localization.Language == Localization.English ? "Not enough embers." : "余烬不足。";
            return false;
        }

        meta.TotalEmbers -= unlock.Cost;
        meta.UnlockedIds.Add(unlock.Id);
        SaveMeta(meta);
        message = Localization.Language == Localization.English ? $"Unlocked: {unlock.DisplayTitle()}" : $"已解锁：{unlock.DisplayTitle()}";
        return true;
    }

    public static bool MeetsUnlockRequirements(UnlockData unlock, MetaProgressData meta, out string message)
    {
        var missing = new List<string>();
        if (unlock.RequiredBestDepth > 0 && meta.BestDepth < unlock.RequiredBestDepth)
        {
            missing.Add(Localization.Language == Localization.English
                ? $"best depth {meta.BestDepth}/{unlock.RequiredBestDepth}"
                : $"最深层数 {meta.BestDepth}/{unlock.RequiredBestDepth}");
        }
        if (unlock.RequiredBestScore > 0 && meta.BestScore < unlock.RequiredBestScore)
        {
            missing.Add(Localization.Language == Localization.English
                ? $"best score {meta.BestScore}/{unlock.RequiredBestScore}"
                : $"最高分 {meta.BestScore}/{unlock.RequiredBestScore}");
        }
        if (unlock.RequiredVictories > 0 && meta.Victories < unlock.RequiredVictories)
        {
            missing.Add(Localization.Language == Localization.English
                ? $"victories {meta.Victories}/{unlock.RequiredVictories}"
                : $"胜利 {meta.Victories}/{unlock.RequiredVictories}");
        }
        if (unlock.RequiredRuns > 0 && meta.RunsCompleted < unlock.RequiredRuns)
        {
            missing.Add(Localization.Language == Localization.English
                ? $"runs {meta.RunsCompleted}/{unlock.RequiredRuns}"
                : $"跑局 {meta.RunsCompleted}/{unlock.RequiredRuns}");
        }

        if (missing.Count == 0)
        {
            message = string.Empty;
            return true;
        }

        message = Localization.Language == Localization.English
            ? $"Requirement not met: {string.Join(", ", missing)}"
            : $"条件未达成：{string.Join("，", missing)}";
        return false;
    }

    private static void SaveMeta(MetaProgressData meta)
    {
        using var file = FileAccess.Open(MetaPath, FileAccess.ModeFlags.Write);
        file.StoreString(JsonSerializer.Serialize(meta, new JsonSerializerOptions { WriteIndented = true }));
    }
}

public class RunSaveData
{
    public int PlayerHp { get; set; }
    public int PlayerMaxHp { get; set; }
    public int Shards { get; set; }
    public int CurrentLayerIndex { get; set; }
    public string CurrentRoomNodeId { get; set; } = string.Empty;
    public int RunSeed { get; set; }
    public int LampOil { get; set; }
    public int FogPressure { get; set; }
    public int RoomsCompleted { get; set; }
    public int BattlesWon { get; set; }
    public int MinesCleared { get; set; }
    public int Score { get; set; }
    public string ObjectiveId { get; set; } = string.Empty;
    public string ObjectiveTitle { get; set; } = string.Empty;
    public string ObjectiveTitleEn { get; set; } = string.Empty;
    public string ObjectiveType { get; set; } = string.Empty;
    public int ObjectiveTarget { get; set; }
    public int ObjectiveEmberReward { get; set; }
    public string CharacterId { get; set; } = string.Empty;
    public List<string> DeckCardIds { get; set; } = new();
    public List<ItemStackData> Items { get; set; } = new();
    public List<string> Relics { get; set; } = new();
    public List<string> Log { get; set; } = new();

    public static RunSaveData FromRun(RunEngine run)
    {
        var data = new RunSaveData
        {
            PlayerHp = run.PlayerHp,
            PlayerMaxHp = run.PlayerMaxHp,
            Shards = run.Shards,
            CurrentLayerIndex = run.CurrentLayerIndex,
            CurrentRoomNodeId = run.CurrentRoomNodeId,
            RunSeed = run.RunSeed,
            LampOil = run.LampOil,
            FogPressure = run.FogPressure,
            RoomsCompleted = run.RoomsCompleted,
            BattlesWon = run.BattlesWon,
            MinesCleared = run.MinesCleared,
            Score = run.Score,
            ObjectiveId = run.ObjectiveId,
            ObjectiveTitle = run.ObjectiveTitle,
            ObjectiveTitleEn = run.ObjectiveTitleEn,
            ObjectiveType = run.ObjectiveType,
            ObjectiveTarget = run.ObjectiveTarget,
            ObjectiveEmberReward = run.ObjectiveEmberReward,
            CharacterId = run.CharacterId,
            Relics = new List<string>(run.Relics),
            Log = new List<string>(run.Log)
        };

        foreach (var card in run.PlayerDeck)
        {
            data.DeckCardIds.Add(card.Id);
        }

        foreach (var item in run.Items)
        {
            data.Items.Add(new ItemStackData { ItemId = item.ItemId, Count = item.Count });
        }

        return data;
    }
}

public class MetaProgressData
{
    public int TotalEmbers { get; set; }
    public int LastEarnedEmbers { get; set; }
    public int LastObjectiveBonus { get; set; }
    public int BestDepth { get; set; }
    public int BestScore { get; set; }
    public int RunsCompleted { get; set; }
    public int Victories { get; set; }
    public List<string> UnlockedIds { get; set; } = new();
    public List<string> CompletedObjectiveIds { get; set; } = new();
}
