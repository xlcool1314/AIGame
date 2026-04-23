using Godot;

namespace AiGame.UI;

public static class TextureHelper
{
    public static Texture2D? TryLoad(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        return ResourceLoader.Exists(path) ? GD.Load<Texture2D>(path) : null;
    }

    public static Control CreateImageOrFallback(string path, string fallbackText, Vector2 size, Color bgColor, Color textColor)
    {
        var texture = TryLoad(path);
        if (texture != null)
        {
            var textureRect = new TextureRect
            {
                Texture = texture,
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                CustomMinimumSize = size,
                MouseFilter = Control.MouseFilterEnum.Ignore,
            };
            return textureRect;
        }

        var panel = UiFactory.CreateCard(bgColor.Darkened(0.15f));
        panel.CustomMinimumSize = size;
        panel.MouseFilter = Control.MouseFilterEnum.Ignore;

        var label = UiFactory.CreateLabel(fallbackText, 18, textColor);
        label.HorizontalAlignment = HorizontalAlignment.Center;
        label.VerticalAlignment = VerticalAlignment.Center;
        label.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        label.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        panel.AddChild(label);
        return panel;
    }
}
