using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public partial class GameData : Node
{
    public CardsConfig Cards { get; private set; } = new();
    public EnemiesConfig Enemies { get; private set; } = new();
    public DecksConfig Decks { get; private set; } = new();

    public void LoadAll()
    {
        Cards = LoadJson<CardsConfig>("res://data/cards.json");
        Enemies = LoadJson<EnemiesConfig>("res://data/enemies.json");
        Decks = LoadJson<DecksConfig>("res://data/decks.json");
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
