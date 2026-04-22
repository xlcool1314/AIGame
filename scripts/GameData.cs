using Godot;
using Godot.Collections;

public partial class GameData : RefCounted
{
    public Dictionary CardIndex { get; private set; } = new();
    public Dictionary EnemyIndex { get; private set; } = new();
    public Dictionary DeckIndex { get; private set; } = new();

    public void LoadAll()
    {
        CardIndex = LoadIndexedJson("res://data/cards.json", "cards");
        EnemyIndex = LoadIndexedJson("res://data/enemies.json", "enemies");
        DeckIndex = LoadIndexedJson("res://data/decks.json", "decks");
    }

    public Dictionary GetCard(string cardId) => GetById(CardIndex, cardId);
    public Dictionary GetEnemy(string enemyId) => GetById(EnemyIndex, enemyId);
    public Dictionary GetDeck(string deckId) => GetById(DeckIndex, deckId);

    private static Dictionary LoadIndexedJson(string path, string listKey)
    {
        var text = FileAccess.GetFileAsString(path);
        if (string.IsNullOrWhiteSpace(text))
        {
            GD.PushError($"无法读取数据文件: {path}");
            return new Dictionary();
        }

        var parsed = Json.ParseString(text);
        if (parsed.VariantType != Variant.Type.Dictionary)
        {
            GD.PushError($"JSON 根节点不是对象: {path}");
            return new Dictionary();
        }

        var root = (Dictionary)parsed;
        if (!root.ContainsKey(listKey) || root[listKey].VariantType != Variant.Type.Array)
        {
            GD.PushError($"JSON 缺少数组字段 {listKey}: {path}");
            return new Dictionary();
        }

        var result = new Dictionary();
        var rows = (Array)root[listKey];
        foreach (var row in rows)
        {
            if (row.VariantType != Variant.Type.Dictionary)
            {
                continue;
            }

            var item = (Dictionary)row;
            if (!item.ContainsKey("id"))
            {
                continue;
            }

            var id = item["id"].AsString();
            result[id] = item;
        }

        return result;
    }

    private static Dictionary GetById(Dictionary index, string id)
    {
        if (index.ContainsKey(id) && index[id].VariantType == Variant.Type.Dictionary)
        {
            return (Dictionary)index[id];
        }

        return new Dictionary();
    }
}
