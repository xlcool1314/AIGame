using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public partial class GameData : Node
{
    public CardsConfig Cards { get; private set; } = new();
    public EnemiesConfig Enemies { get; private set; } = new();
    public DecksConfig Decks { get; private set; } = new();
    public EventsConfig Events { get; private set; } = new();
    public RewardsConfig Rewards { get; private set; } = new();
    public CharactersConfig Characters { get; private set; } = new();
    public ItemsConfig Items { get; private set; } = new();
    public LayersConfig Layers { get; private set; } = new();
    public UnlocksConfig Unlocks { get; private set; } = new();
    public ObjectivesConfig Objectives { get; private set; } = new();
    public RelicsConfig Relics { get; private set; } = new();

    public void LoadAll()
    {
        Cards = LoadJson<CardsConfig>("res://data/cards.json");
        Enemies = LoadJson<EnemiesConfig>("res://data/enemies.json");
        Decks = LoadJson<DecksConfig>("res://data/decks.json");
        Events = LoadJson<EventsConfig>("res://data/events.json");
        Rewards = LoadJson<RewardsConfig>("res://data/rewards.json");
        Characters = LoadJson<CharactersConfig>("res://data/characters.json");
        Items = LoadJson<ItemsConfig>("res://data/items.json");
        Layers = LoadJson<LayersConfig>("res://data/layers.json");
        Unlocks = LoadJson<UnlocksConfig>("res://data/unlocks.json");
        Objectives = LoadJson<ObjectivesConfig>("res://data/objectives.json");
        Relics = LoadJson<RelicsConfig>("res://data/relics.json");
    }

    public CardData GetCard(string cardId)
    {
        foreach (var card in Cards.Cards)
        {
            if (card.Id == cardId)
            {
                return card;
            }
        }

        throw new InvalidOperationException($"未找到卡牌: {cardId}");
    }

    public EnemyData GetEnemy(string enemyId)
    {
        foreach (var enemy in Enemies.Enemies)
        {
            if (enemy.Id == enemyId)
            {
                return enemy;
            }
        }

        throw new InvalidOperationException($"未找到敌人: {enemyId}");
    }

    public MineEventData GetEvent(string eventId)
    {
        foreach (var mineEvent in Events.Events)
        {
            if (mineEvent.Id == eventId)
            {
                return mineEvent;
            }
        }

        throw new InvalidOperationException($"未找到事件: {eventId}");
    }

    public RewardData GetReward(string rewardId)
    {
        foreach (var reward in Rewards.Rewards)
        {
            if (reward.Id == rewardId)
            {
                return reward;
            }
        }

        throw new InvalidOperationException($"未找到奖励: {rewardId}");
    }

    public ObjectiveData GetObjective(string objectiveId)
    {
        foreach (var objective in Objectives.Objectives)
        {
            if (objective.Id == objectiveId)
            {
                return objective;
            }
        }

        throw new InvalidOperationException($"未找到委托: {objectiveId}");
    }

    public RelicData GetRelic(string relicId)
    {
        foreach (var relic in Relics.Relics)
        {
            if (relic.Id == relicId)
            {
                return relic;
            }
        }

        throw new InvalidOperationException($"未找到遗物: {relicId}");
    }

    public CharacterData GetCharacter(string characterId)
    {
        foreach (var character in Characters.Characters)
        {
            if (character.Id == characterId)
            {
                return character;
            }
        }

        throw new InvalidOperationException($"未找到角色: {characterId}");
    }

    public ItemData GetItem(string itemId)
    {
        foreach (var item in Items.Items)
        {
            if (item.Id == itemId)
            {
                return item;
            }
        }

        throw new InvalidOperationException($"未找到道具: {itemId}");
    }

    public List<CardData> BuildStarterDeck(string deckId)
    {
        foreach (var deck in Decks.Decks)
        {
            if (deck.Id != deckId)
            {
                continue;
            }

            var result = new List<CardData>();
            foreach (var cardId in deck.Cards)
            {
                result.Add(GetCard(cardId));
            }

            return result;
        }

        throw new InvalidOperationException($"未找到卡组: {deckId}");
    }

    public List<CardData> BuildRewardChoices(RewardData reward, string characterId, int seed, int count = 3)
    {
        var character = GetCharacter(characterId);
        var candidates = new List<CardData>();
        foreach (var cardId in reward.CardChoices)
        {
            var card = GetCard(cardId);
            if (IsCardEligibleForReward(card, character))
            {
                candidates.Add(card);
            }
        }

        foreach (var card in Cards.Cards)
        {
            if (!candidates.Contains(card) && IsCardEligibleForReward(card, character))
            {
                candidates.Add(card);
            }
        }

        var result = new List<CardData>();
        var random = new Random(seed);
        while (result.Count < count && candidates.Count > 0)
        {
            var totalWeight = 0;
            foreach (var card in candidates)
            {
                totalWeight += GetRarityWeight(card.Rarity);
            }

            var roll = random.Next(Math.Max(1, totalWeight));
            for (var i = 0; i < candidates.Count; i++)
            {
                roll -= GetRarityWeight(candidates[i].Rarity);
                if (roll >= 0)
                {
                    continue;
                }

                result.Add(candidates[i]);
                candidates.RemoveAt(i);
                break;
            }
        }

        return result;
    }

    public List<RelicData> BuildRelicChoices(RewardData reward, IReadOnlyCollection<string> ownedRelics, int seed, int count = 2)
    {
        var candidates = new List<RelicData>();
        foreach (var relicId in reward.RelicChoices)
        {
            var relic = GetRelic(relicId);
            if (IsRelicEligibleForReward(relic, ownedRelics))
            {
                candidates.Add(relic);
            }
        }

        foreach (var relic in Relics.Relics)
        {
            if (!ContainsRelicData(candidates, relic.Id) && IsRelicEligibleForReward(relic, ownedRelics))
            {
                candidates.Add(relic);
            }
        }

        var result = new List<RelicData>();
        var random = new Random(seed);
        while (result.Count < count && candidates.Count > 0)
        {
            var totalWeight = 0;
            foreach (var relic in candidates)
            {
                totalWeight += GetRarityWeight(relic.Rarity);
            }

            var roll = random.Next(Math.Max(1, totalWeight));
            for (var i = 0; i < candidates.Count; i++)
            {
                roll -= GetRarityWeight(candidates[i].Rarity);
                if (roll >= 0)
                {
                    continue;
                }

                result.Add(candidates[i]);
                candidates.RemoveAt(i);
                break;
            }
        }

        return result;
    }

    private static bool IsCardEligibleForReward(CardData card, CharacterData character)
    {
        if (card.UpgradeOnly || !SaveManager.IsUnlocked(card.UnlockId))
        {
            return false;
        }

        if (card.Pools.Count == 0)
        {
            return true;
        }

        foreach (var pool in card.Pools)
        {
            if (pool == "neutral" || pool == character.Id || character.CardPools.Contains(pool))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsRelicEligibleForReward(RelicData relic, IReadOnlyCollection<string> ownedRelics)
    {
        return !ContainsRelicId(ownedRelics, relic.Id) && SaveManager.IsUnlocked(relic.UnlockId);
    }

    private static bool ContainsRelicId(IReadOnlyCollection<string> relicIds, string relicId)
    {
        foreach (var ownedRelicId in relicIds)
        {
            if (ownedRelicId == relicId)
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsRelicData(List<RelicData> relics, string relicId)
    {
        foreach (var relic in relics)
        {
            if (relic.Id == relicId)
            {
                return true;
            }
        }

        return false;
    }

    private static int GetRarityWeight(string rarity)
    {
        return rarity switch
        {
            "rare" => 12,
            "uncommon" => 28,
            _ => 60
        };
    }

    private static T LoadJson<T>(string path)
    {
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        var json = file.GetAsText();
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var data = JsonSerializer.Deserialize<T>(json, options);
        return data ?? throw new InvalidOperationException($"读取失败: {path}");
    }
}

public class CardsConfig
{
    public List<CardData> Cards { get; set; } = new();
}

public class EnemiesConfig
{
    public List<EnemyData> Enemies { get; set; } = new();
}

public class DecksConfig
{
    public List<DeckData> Decks { get; set; } = new();
}

public class EventsConfig
{
    public List<MineEventData> Events { get; set; } = new();
}

public class RewardsConfig
{
    public List<RewardData> Rewards { get; set; } = new();
}

public class CharactersConfig
{
    public List<CharacterData> Characters { get; set; } = new();
}

public class ItemsConfig
{
    public List<ItemData> Items { get; set; } = new();
}

public class LayersConfig
{
    public List<LayerData> Layers { get; set; } = new();
}

public class UnlocksConfig
{
    public List<UnlockData> Unlocks { get; set; } = new();
}

public class ObjectivesConfig
{
    public List<ObjectiveData> Objectives { get; set; } = new();
}

public class RelicsConfig
{
    public List<RelicData> Relics { get; set; } = new();
}

public class ObjectiveData
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Target { get; set; }
    public int EmberReward { get; set; }

    public string DisplayTitle() => Localization.Pick(Title, TitleEn);
    public string DisplayDescription() => Localization.Pick(Description, DescriptionEn);
}

public class UnlockData
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public int Cost { get; set; }
    public int RequiredBestDepth { get; set; }
    public int RequiredBestScore { get; set; }
    public int RequiredVictories { get; set; }
    public int RequiredRuns { get; set; }

    public string DisplayTitle() => Localization.Pick(Title, TitleEn);
    public string DisplayDescription() => Localization.Pick(Description, DescriptionEn);
}

public class LayerData
{
    public List<RoomData> Rooms { get; set; } = new();
}

public class RoomData
{
    public string Kind { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string EnemyId { get; set; } = string.Empty;
    public List<string> EnemyIds { get; set; } = new();
    public string EventId { get; set; } = string.Empty;
    public string RewardId { get; set; } = string.Empty;
    public int Risk { get; set; } = 1;
    public int LampCost { get; set; } = 8;
    public int RewardBonus { get; set; }
    public MineRoomConfig MineConfig { get; set; } = new();
}

public class MineRoomConfig
{
    public int Width { get; set; } = 5;
    public int Height { get; set; } = 5;
    public int Monsters { get; set; } = 2;
    public int Traps { get; set; } = 2;
    public int Treasures { get; set; } = 2;
    public int Ores { get; set; } = 5;
    public int ClearReward { get; set; } = 14;
    public int TrapDamage { get; set; } = 8;
}

public class DeckData
{
    public string Id { get; set; } = string.Empty;
    public List<string> Cards { get; set; } = new();
}

public class CardData
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string UnlockId { get; set; } = string.Empty;
    public int Cost { get; set; }
    public string Description { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string Rarity { get; set; } = "common";
    public string Type { get; set; } = "skill";
    public string UpgradeTo { get; set; } = string.Empty;
    public bool UpgradeOnly { get; set; }
    public List<string> Pools { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public List<CardAction> Actions { get; set; } = new();

    public string DisplayName() => Localization.Pick(Name, NameEn);
    public string DisplayDescription() => Localization.Pick(Description, DescriptionEn);
}

public class CardAction
{
    public string Type { get; set; } = string.Empty;
    public int Value { get; set; }
    public int Duration { get; set; } = 1;
}

public class EnemyData
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string ArtPath { get; set; } = string.Empty;
    public int MaxHp { get; set; }
    public List<IntentData> Intents { get; set; } = new();

    public string DisplayName() => Localization.Pick(Name, NameEn);
}

public class IntentData
{
    public string Name { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public List<CardAction> Actions { get; set; } = new();

    public string DisplayName() => Localization.Pick(Name, NameEn);
}

public class MineEventData
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public List<EventChoiceData> Choices { get; set; } = new();

    public string DisplayTitle() => Localization.Pick(Title, TitleEn);
    public string DisplayDescription() => Localization.Pick(Description, DescriptionEn);
}

public class EventChoiceData
{
    public string Text { get; set; } = string.Empty;
    public string TextEn { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public string ResultEn { get; set; } = string.Empty;
    public List<RunAction> Actions { get; set; } = new();

    public string DisplayText() => Localization.Pick(Text, TextEn);
    public string DisplayResult() => Localization.Pick(Result, ResultEn);
}

public class RelicData
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string Rarity { get; set; } = "common";
    public string UnlockId { get; set; } = string.Empty;
    public List<RelicEffectData> Effects { get; set; } = new();

    public string DisplayName() => Localization.Pick(Name, NameEn);
    public string DisplayDescription() => Localization.Pick(Description, DescriptionEn);
}

public class RelicEffectData
{
    public string Type { get; set; } = string.Empty;
    public int Value { get; set; }
}

public class RewardData
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public int Shards { get; set; }
    public int Heal { get; set; }
    public List<string> CardChoices { get; set; } = new();
    public List<string> RelicChoices { get; set; } = new();

    public string DisplayTitle() => Localization.Pick(Title, TitleEn);
}

public class RunAction
{
    public string Type { get; set; } = string.Empty;
    public int Value { get; set; }
    public string CardId { get; set; } = string.Empty;
    public string RelicId { get; set; } = string.Empty;
}

public class CharacterData
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string ArtPath { get; set; } = string.Empty;
    public string UnlockId { get; set; } = string.Empty;
    public int MaxHp { get; set; }
    public int Shards { get; set; }
    public string DeckId { get; set; } = "starter";
    public List<string> CardPools { get; set; } = new();
    public List<ItemStackData> StartingItems { get; set; } = new();
    public List<RelicEffectData> Effects { get; set; } = new();

    public string DisplayName() => Localization.Pick(Name, NameEn);
    public string DisplayDescription() => Localization.Pick(Description, DescriptionEn);
}

public class ItemData
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string UseMode { get; set; } = string.Empty;
    public int Value { get; set; }

    public string DisplayName() => Localization.Pick(Name, NameEn);
    public string DisplayDescription() => Localization.Pick(Description, DescriptionEn);
}

public class ItemStackData
{
    public string ItemId { get; set; } = string.Empty;
    public int Count { get; set; }
}
