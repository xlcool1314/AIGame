using Godot;
using Godot.Collections;

public partial class GameData : RefCounted
{
    public Dictionary<string, Dictionary> Cards { get; private set; } = new();
    public Dictionary<string, Dictionary> Enemies { get; private set; } = new();
    public Dictionary<string, Dictionary> Decks { get; private set; } = new();

    public void LoadAll()
    {
        Cards = LoadIndexedJson("res://data/cards.json", "cards");
        Enemies = LoadIndexedJson("res://data/enemies.json", "enemies");
        Decks = LoadIndexedJson("res://data/decks.json", "decks");
    }

    public Dictionary GetCard(string cardId) => Cards.TryGetValue(cardId, out var card) ? card : new Dictionary();
    public Dictionary GetEnemy(string enemyId) => Enemies.TryGetValue(enemyId, out var enemy) ? enemy : new Dictionary();
    public Dictionary GetDeck(string deckId) => Decks.TryGetValue(deckId, out var deck) ? deck : new Dictionary();

    private static Dictionary<string, Dictionary> LoadIndexedJson(string path, string key)
    {
        var text = FileAccess.GetFileAsString(path);
        if (string.IsNullOrWhiteSpace(text))
        {
            GD.PushError($"无法读取数据文件: {path}");
            return new Dictionary<string, Dictionary>();
        }

        var json = Json.ParseString(text).AsGodotDictionary();
        if (json.Count == 0 || !json.ContainsKey(key))
        {
            GD.PushError($"JSON 格式不正确: {path}");
            return new Dictionary<string, Dictionary>();
        }

        var result = new Dictionary<string, Dictionary>();
        var entries = json[key].AsGodotArray();

        foreach (var value in entries)
        {
            var item = value.AsGodotDictionary();
            if (!item.ContainsKey("id"))
            {
                continue;
            }

            var id = item["id"].AsString();
            result[id] = item;
        }

        return result;
    }
}
