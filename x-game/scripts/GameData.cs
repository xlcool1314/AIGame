using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public partial class GameData : Node
{
    public CardsConfig Cards { get; private set; } = new();
    public EnemiesConfig Enemies { get; private set; } = new();
    public DecksConfig Decks { get; private set; } = new();
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
    public EventsConfig Events { get; private set; } = new();
    public RewardsConfig Rewards { get; private set; } = new();
=======
    public RelicsConfig Relics { get; private set; } = new();
>>>>>>> theirs
=======
    public RelicsConfig Relics { get; private set; } = new();
>>>>>>> theirs
=======
    public RelicsConfig Relics { get; private set; } = new();
>>>>>>> theirs
=======
    public RelicsConfig Relics { get; private set; } = new();
>>>>>>> theirs

    public void LoadAll()
    {
        Cards = LoadJson<CardsConfig>("res://data/cards.json");
        Enemies = LoadJson<EnemiesConfig>("res://data/enemies.json");
        Decks = LoadJson<DecksConfig>("res://data/decks.json");
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
        Events = LoadJson<EventsConfig>("res://data/events.json");
        Rewards = LoadJson<RewardsConfig>("res://data/rewards.json");
=======
=======
>>>>>>> theirs
=======
>>>>>>> theirs
=======
>>>>>>> theirs
        Relics = LoadJson<RelicsConfig>("res://data/relics.json");
    }

    public List<CardData> GetAllCards()
    {
        return new List<CardData>(Cards.Cards);
    }

    public List<RelicData> GetAllRelics()
    {
        return new List<RelicData>(Relics.Relics);
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

<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
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

=======
>>>>>>> theirs
=======
>>>>>>> theirs
=======
>>>>>>> theirs
=======
>>>>>>> theirs
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

<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
public class EventsConfig
{
    public List<MineEventData> Events { get; set; } = new();
}

public class RewardsConfig
{
    public List<RewardData> Rewards { get; set; } = new();
=======
public class RelicsConfig
{
    public List<RelicData> Relics { get; set; } = new();
>>>>>>> theirs
=======
public class RelicsConfig
{
    public List<RelicData> Relics { get; set; } = new();
>>>>>>> theirs
=======
public class RelicsConfig
{
    public List<RelicData> Relics { get; set; } = new();
>>>>>>> theirs
=======
public class RelicsConfig
{
    public List<RelicData> Relics { get; set; } = new();
>>>>>>> theirs
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
    public int Cost { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<CardAction> Actions { get; set; } = new();
}

public class CardAction
{
    public string Type { get; set; } = string.Empty;
    public int Value { get; set; }
}

public class EnemyData
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int MaxHp { get; set; }
    public List<IntentData> Intents { get; set; } = new();
}

public class IntentData
{
    public string Name { get; set; } = string.Empty;
    public List<CardAction> Actions { get; set; } = new();
}

<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
public class MineEventData
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<EventChoiceData> Choices { get; set; } = new();
}

public class EventChoiceData
{
    public string Text { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public List<RunAction> Actions { get; set; } = new();
}

public class RewardData
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Shards { get; set; }
    public int Heal { get; set; }
    public List<string> CardChoices { get; set; } = new();
}

public class RunAction
{
    public string Type { get; set; } = string.Empty;
    public int Value { get; set; }
    public string CardId { get; set; } = string.Empty;
    public string RelicId { get; set; } = string.Empty;
=======
=======
>>>>>>> theirs
=======
>>>>>>> theirs
=======
>>>>>>> theirs
public class RelicData
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Value { get; set; }
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
}
