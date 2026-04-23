using System;
using System.Text.Json;
using Godot;

namespace AiGame.Data;

public static class JsonLoader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    public static T Load<T>(string resourcePath) where T : new()
    {
        if (!FileAccess.FileExists(resourcePath))
        {
            GD.PushWarning($"Missing data file: {resourcePath}");
            return new T();
        }

        var json = FileAccess.GetFileAsString(resourcePath);
        if (string.IsNullOrWhiteSpace(json))
        {
            GD.PushWarning($"Empty data file: {resourcePath}");
            return new T();
        }

        try
        {
            return JsonSerializer.Deserialize<T>(json, Options) ?? new T();
        }
        catch (Exception ex)
        {
            GD.PushError($"Failed to parse {resourcePath}: {ex.Message}");
            return new T();
        }
    }
}
