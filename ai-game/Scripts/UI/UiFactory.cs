using Godot;

namespace AiGame.UI;

public static class UiFactory
{
    public static Panel CreateCard(Color color)
    {
        var panel = new Panel
        {
            MouseFilter = Control.MouseFilterEnum.Pass,
        };

        var style = new StyleBoxFlat
        {
            BgColor = color,
            BorderColor = color.Lightened(0.16f),
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            ShadowColor = new Color(0, 0, 0, 0.22f),
            ShadowSize = 16,
            ShadowOffset = new Vector2(0, 8),
            CornerRadiusTopLeft = 20,
            CornerRadiusTopRight = 20,
            CornerRadiusBottomLeft = 20,
            CornerRadiusBottomRight = 20,
            ContentMarginLeft = 18,
            ContentMarginTop = 18,
            ContentMarginRight = 18,
            ContentMarginBottom = 18,
        };

        panel.AddThemeStyleboxOverride("panel", style);
        return panel;
    }

    public static Label CreateLabel(string text, int fontSize, Color color)
    {
        var label = new Label
        {
            Text = text,
            AutowrapMode = TextServer.AutowrapMode.Off,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
        };
        label.AddThemeFontSizeOverride("font_size", fontSize);
        label.AddThemeColorOverride("font_color", color);
        return label;
    }

    public static Label CreateWrappedLabel(string text, int fontSize, Color color)
    {
        var label = CreateLabel(text, fontSize, color);
        label.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        return label;
    }

    public static Button CreateButton(string text, Color accent)
    {
        var button = new Button
        {
            Text = text,
            CustomMinimumSize = new Vector2(0, 38),
            FocusMode = Control.FocusModeEnum.All,
        };

        var normal = new StyleBoxFlat
        {
            BgColor = accent,
            CornerRadiusTopLeft = 12,
            CornerRadiusTopRight = 12,
            CornerRadiusBottomLeft = 12,
            CornerRadiusBottomRight = 12,
            ContentMarginLeft = 12,
            ContentMarginTop = 8,
            ContentMarginRight = 12,
            ContentMarginBottom = 8,
        };

        var hover = normal.Duplicate() as StyleBoxFlat ?? new StyleBoxFlat();
        hover.BgColor = accent.Lightened(0.12f);

        var pressed = normal.Duplicate() as StyleBoxFlat ?? new StyleBoxFlat();
        pressed.BgColor = accent.Darkened(0.08f);

        var disabled = normal.Duplicate() as StyleBoxFlat ?? new StyleBoxFlat();
        disabled.BgColor = accent.Darkened(0.45f);

        var focus = normal.Duplicate() as StyleBoxFlat ?? new StyleBoxFlat();
        focus.DrawCenter = false;
        focus.BorderColor = Colors.White;
        focus.BorderWidthLeft = 3;
        focus.BorderWidthTop = 3;
        focus.BorderWidthRight = 3;
        focus.BorderWidthBottom = 3;
        focus.ExpandMarginLeft = 3;
        focus.ExpandMarginTop = 3;
        focus.ExpandMarginRight = 3;
        focus.ExpandMarginBottom = 3;

        button.AddThemeStyleboxOverride("normal", normal);
        button.AddThemeStyleboxOverride("hover", hover);
        button.AddThemeStyleboxOverride("pressed", pressed);
        button.AddThemeStyleboxOverride("disabled", disabled);
        button.AddThemeStyleboxOverride("focus", focus);
        button.AddThemeColorOverride("font_color", Colors.Black);
        button.AddThemeColorOverride("font_disabled_color", new Color(0.12f, 0.12f, 0.12f, 0.55f));
        button.AddThemeFontSizeOverride("font_size", 16);
        return button;
    }
}
