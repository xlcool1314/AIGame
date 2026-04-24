using System;
using System.Collections.Generic;
using System.Text.Json;
using Godot;

namespace AiGame.Systems;

public static class SaveSystem
{
    private const string SavePath = "user://savegame.json";

    public static void Save(GameSaveData data)
    {
        try
        {
            using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
            file.StoreString(JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception ex)
        {
            GD.PushWarning($"Save failed: {ex.Message}");
        }
    }

    public static GameSaveData? Load()
    {
        if (!FileAccess.FileExists(SavePath))
        {
            return null;
        }

        try
        {
            var json = FileAccess.GetFileAsString(SavePath);
            return JsonSerializer.Deserialize<GameSaveData>(json);
        }
        catch (Exception ex)
        {
            GD.PushWarning($"Load failed: {ex.Message}");
            return null;
        }
    }

    public static void Delete()
    {
        try
        {
            if (FileAccess.FileExists(SavePath))
            {
                DirAccess.RemoveAbsolute(ProjectSettings.GlobalizePath(SavePath));
            }
        }
        catch (Exception ex)
        {
            GD.PushWarning($"Delete save failed: {ex.Message}");
        }
    }
}

public sealed class GameSaveData
{
    public string LanguageCode { get; set; } = "zh-CN";
    public int Coins { get; set; }
    public int Aroma { get; set; }
    public int Reputation { get; set; }
    public int ShiftLevel { get; set; }
    public int ServedCount { get; set; }
    public int NextBlessingAt { get; set; }
    public string? CurrentCustomerId { get; set; }
    public string CurrentRequestTag { get; set; } = string.Empty;
    public List<CustomerOrderSaveData> CustomerOrders { get; set; } = new();
    public List<BattleHeroSaveData> Heroes { get; set; } = new();
    public BattleEnemySaveData? Enemy { get; set; }
    public int CombatWave { get; set; } = 1;
    public int SelectedOrderIndex { get; set; }
    public List<CraftedItemSaveData> CraftedItems { get; set; } = new();
    public string BrewingProductId { get; set; } = string.Empty;
    public float BrewingProgress { get; set; }
    public float BrewingDuration { get; set; }
    public float PassiveCoinRemainder { get; set; }
    public List<string> OwnedDecorIds { get; set; } = new();
    public List<string> BlessingIds { get; set; } = new();
    public List<ActiveBlessingSaveData> ActiveBlessings { get; set; } = new();
    public string? TodaySpecialProductId { get; set; }
    public int ComboStreak { get; set; }
    public int BestCombo { get; set; }
    public int TipsEarnedTotal { get; set; }
    public int ForgeHeat { get; set; }
    public int NextEventAt { get; set; }
    public string LastServedTag { get; set; } = string.Empty;
    public string CurrentEventTitle { get; set; } = string.Empty;
    public string CurrentEventDescription { get; set; } = string.Empty;
    public string CurrentEventTargetTag { get; set; } = string.Empty;
    public int CurrentEventTargetCount { get; set; }
    public int CurrentEventProgress { get; set; }
    public int CurrentEventServesRemaining { get; set; }
    public int CurrentEventRewardCoins { get; set; }
    public int CurrentEventRewardAroma { get; set; }
    public int CurrentEventRewardReputation { get; set; }
}

public sealed class CustomerOrderSaveData
{
    public string CustomerId { get; set; } = string.Empty;
    public string RequestTag { get; set; } = string.Empty;
    public float Patience { get; set; }
    public float MaxPatience { get; set; }
}

public sealed class BattleHeroSaveData
{
    public string CustomerId { get; set; } = string.Empty;
    public int Hp { get; set; }
    public string WeaponProductId { get; set; } = string.Empty;
    public string ArmorProductId { get; set; } = string.Empty;
    public string TrinketProductId { get; set; } = string.Empty;
    public List<string> RequiredSlots { get; set; } = new();
    public CraftedItemSaveData? WeaponItem { get; set; }
    public CraftedItemSaveData? ArmorItem { get; set; }
    public CraftedItemSaveData? TrinketItem { get; set; }
    public List<ActiveCombatBuffSaveData> ActiveBuffs { get; set; } = new();
    public BattleEnemySaveData? Enemy { get; set; }
    public int LaneWave { get; set; } = 1;
    public float AttackProgress { get; set; }
    public float EnemyAttackProgress { get; set; }
}

public sealed class BattleEnemySaveData
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Hp { get; set; }
    public int MaxHp { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int Wave { get; set; }
    public float AttackSeconds { get; set; } = 3f;
}

public sealed class ActiveBlessingSaveData
{
    public string BlessingId { get; set; } = string.Empty;
    public int RemainingServes { get; set; }
}

public sealed class ActiveCombatBuffSaveData
{
    public string BuffId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public float RemainingSeconds { get; set; }
    public float DurationSeconds { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int Vitality { get; set; }
    public float AttackSpeedBonus { get; set; }
    public float ExtraAttackChance { get; set; }
    public float CritChance { get; set; }
    public float HealOnHit { get; set; }
}

public sealed class CraftedItemSaveData
{
    public string ItemId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string Rarity { get; set; } = string.Empty;
    public int AttackBonus { get; set; }
    public int DefenseBonus { get; set; }
    public int VitalityBonus { get; set; }
    public float AttackSpeedBonus { get; set; }
    public float ExtraAttackChance { get; set; }
    public float CritChance { get; set; }
    public float HealOnHit { get; set; }
}
