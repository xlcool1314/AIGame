using System.Collections.Generic;

namespace AiGame.Data;

public sealed class ProductConfig
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int BasePrice { get; set; }
    public int Cost { get; set; }
    public float BrewSeconds { get; set; }
    public int AromaGain { get; set; }
    public string IconPath { get; set; } = string.Empty;
    public int UnlockShift { get; set; } = 1;
    public int UnlockReputation { get; set; }
    public string Slot { get; set; } = "weapon";
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int Vitality { get; set; }
    public float AttackSpeedBonus { get; set; }
    public float ExtraAttackChance { get; set; }
    public float CritChance { get; set; }
    public float HealOnHit { get; set; }
    public string BuffId { get; set; } = string.Empty;
    public string BuffName { get; set; } = string.Empty;
    public string BuffDescription { get; set; } = string.Empty;
    public float BuffDuration { get; set; }
    public int BuffAttack { get; set; }
    public int BuffDefense { get; set; }
    public int BuffVitality { get; set; }
    public float BuffAttackSpeedBonus { get; set; }
    public float BuffExtraAttackChance { get; set; }
    public float BuffCritChance { get; set; }
    public float BuffHealOnHit { get; set; }
    public List<string> Tags { get; set; } = new();
}

public sealed class CustomerConfig
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string FavoriteProductId { get; set; } = string.Empty;
    public float SpendMultiplier { get; set; } = 1.0f;
    public string SpritePath { get; set; } = string.Empty;
    public string FirstLine { get; set; } = string.Empty;
    public int UnlockShift { get; set; } = 1;
    public int TipCoins { get; set; }
    public int MaxHp { get; set; } = 100;
    public int BaseAttack { get; set; } = 8;
    public int BaseDefense { get; set; } = 2;
    public float BaseAttackSeconds { get; set; } = 2.8f;
}

public sealed class DecorConfig
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int Cost { get; set; }
    public float PassiveCoinsPerSecond { get; set; }
    public string TexturePath { get; set; } = string.Empty;
    public int UnlockShift { get; set; } = 1;
}

public sealed class BlessingConfig
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string EffectType { get; set; } = string.Empty;
    public string Archetype { get; set; } = string.Empty;
    public float Amount { get; set; }
    public int UnlockShift { get; set; } = 1;
}

public sealed class SceneSkinConfig
{
    public string Title { get; set; } = "深夜窗台茶馆";
    public string BackgroundTexturePath { get; set; } = string.Empty;
    public string ClerkTexturePath { get; set; } = string.Empty;
    public string DefaultCustomerTexturePath { get; set; } = string.Empty;
    public string AccentColor { get; set; } = "#d7a86e";
    public string BackgroundColor { get; set; } = "#1a2028";
    public string CardColor { get; set; } = "#243241";
}

public sealed class ProductConfigList
{
    public List<ProductConfig> Items { get; set; } = new();
}

public sealed class CustomerConfigList
{
    public List<CustomerConfig> Items { get; set; } = new();
}

public sealed class DecorConfigList
{
    public List<DecorConfig> Items { get; set; } = new();
}

public sealed class BlessingConfigList
{
    public List<BlessingConfig> Items { get; set; } = new();
}
