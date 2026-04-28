using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public static class SaveManager
{
    private const string SavePath = "user://save_run.json";

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
}

public class RunSaveData
{
    public int PlayerHp { get; set; }
    public int PlayerMaxHp { get; set; }
    public int Shards { get; set; }
    public int CurrentLayerIndex { get; set; }
    public List<string> DeckCardIds { get; set; } = new();
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
            Relics = new List<string>(run.Relics),
            Log = new List<string>(run.Log)
        };

        foreach (var card in run.PlayerDeck)
        {
            data.DeckCardIds.Add(card.Id);
        }

        return data;
    }
}
