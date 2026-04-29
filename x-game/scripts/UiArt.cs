using Godot;
using System.Collections.Generic;
using System.Text.Json;

public static class UiArt
{
    private const string ManifestPath = "res://data/art_manifest.json";
    private static ArtManifest? _manifest;

    public static string GetBackgroundPath(string key) => GetValue(Manifest.Backgrounds, key);
    public static string GetFramePath(string key) => GetValue(Manifest.Frames, key);
    public static string GetIconPath(string key) => GetValue(Manifest.Icons, key);
    public static string GetMineTilePath(string key) => GetValue(Manifest.MineTiles, key);

    public static Texture2D? LoadTexture(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !ResourceLoader.Exists(path))
        {
            return null;
        }

        return ResourceLoader.Load<Texture2D>(path);
    }

    public static Texture2D? LoadBackground(string key) => LoadTexture(GetBackgroundPath(key));
    public static Texture2D? LoadIcon(string key) => LoadTexture(GetIconPath(key));

    public static void ApplySceneBackdrop(Control root, string backgroundKey)
    {
        if (root.GetNodeOrNull<TextureRect>("ArtBackdrop") != null)
        {
            return;
        }

        var texture = LoadBackground(backgroundKey);
        if (texture == null)
        {
            return;
        }

        var backdrop = new TextureRect
        {
            Name = "ArtBackdrop",
            Texture = texture,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered,
            Modulate = new Color(0.42f, 0.42f, 0.42f, 1f)
        };
        backdrop.SetAnchorsPreset(Control.LayoutPreset.FullRect);

        var shade = new ColorRect
        {
            Name = "ArtBackdropShade",
            Color = new Color(0.02f, 0.02f, 0.02f, 0.66f),
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        shade.SetAnchorsPreset(Control.LayoutPreset.FullRect);

        root.AddChild(backdrop);
        root.MoveChild(backdrop, 0);
        root.AddChild(shade);
        root.MoveChild(shade, 1);
    }

    private static ArtManifest Manifest
    {
        get
        {
            if (_manifest != null)
            {
                return _manifest;
            }

            if (!FileAccess.FileExists(ManifestPath))
            {
                _manifest = new ArtManifest();
                return _manifest;
            }

            using var file = FileAccess.Open(ManifestPath, FileAccess.ModeFlags.Read);
            var json = file.GetAsText();
            _manifest = JsonSerializer.Deserialize<ArtManifest>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new ArtManifest();
            return _manifest;
        }
    }

    private static string GetValue(Dictionary<string, string> table, string key)
    {
        return table.TryGetValue(key, out var value) ? value : string.Empty;
    }
}

public class ArtManifest
{
    public Dictionary<string, string> Backgrounds { get; set; } = new();
    public Dictionary<string, string> Frames { get; set; } = new();
    public Dictionary<string, string> Icons { get; set; } = new();
    public Dictionary<string, string> MineTiles { get; set; } = new();
}
