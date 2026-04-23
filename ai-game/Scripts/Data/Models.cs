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
