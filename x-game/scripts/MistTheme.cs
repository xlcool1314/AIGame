using Godot;

public enum MistButtonVariant
{
    Neutral,
    Primary,
    Purple,
    Danger,
    Gold,
    Disabled
}

public enum MistPanelVariant
{
    Root,
    Stone,
    Inset,
    Purple,
    Danger,
    Gold
}

public static class MistTheme
{
    public static Color TextMain => Color.FromHtml("ded7c8");
    public static Color TextMuted => Color.FromHtml("9f988c");
    public static Color Purple => Color.FromHtml("7c3bb3");
    public static Color Red => Color.FromHtml("b64a4e");
    public static Color Gold => Color.FromHtml("b79a5b");
    public static Color Green => Color.FromHtml("4f7d5f");

    public static void ApplyRoot(Panel root, string backgroundKey)
    {
        root.AddThemeStyleboxOverride("panel", PanelStyle(MistPanelVariant.Root));
        UiArt.ApplySceneBackdrop(root, backgroundKey);
    }

    public static void StylePanel(PanelContainer panel, MistPanelVariant variant)
    {
        panel.AddThemeStyleboxOverride("panel", PanelStyle(variant));
    }

    public static void StyleButton(Button button, MistButtonVariant variant = MistButtonVariant.Neutral)
    {
        var background = ButtonBackground(variant);
        var font = variant == MistButtonVariant.Disabled ? TextMuted : TextMain;
        button.AddThemeStyleboxOverride("normal", ButtonStyle(background));
        button.AddThemeStyleboxOverride("hover", ButtonStyle(background.Lightened(0.1f)));
        button.AddThemeStyleboxOverride("pressed", ButtonStyle(background.Darkened(0.12f)));
        button.AddThemeStyleboxOverride("disabled", ButtonStyle(background.Darkened(0.25f)));
        button.AddThemeColorOverride("font_color", font);
        button.AddThemeColorOverride("font_hover_color", font.Lightened(0.08f));
        button.AddThemeColorOverride("font_pressed_color", font);
        button.AddThemeColorOverride("font_disabled_color", TextMuted.Darkened(0.25f));
    }

    public static void StyleLabel(Label label, bool muted = false)
    {
        label.AddThemeColorOverride("font_color", muted ? TextMuted : TextMain);
    }

    public static StyleBoxFlat PanelStyle(MistPanelVariant variant)
    {
        var (background, border, borderWidth) = variant switch
        {
            MistPanelVariant.Root => ("0a0b0c", "0a0b0c", 0),
            MistPanelVariant.Inset => ("111111", "2d2b28", 1),
            MistPanelVariant.Purple => ("161117", "6f389e", 2),
            MistPanelVariant.Danger => ("191112", "7d3335", 2),
            MistPanelVariant.Gold => ("181510", "7d673e", 2),
            _ => ("121212", "34302b", 2)
        };

        var style = new StyleBoxFlat
        {
            BgColor = Color.FromHtml(background),
            BorderColor = Color.FromHtml(border),
            CornerRadiusTopLeft = 4,
            CornerRadiusTopRight = 4,
            CornerRadiusBottomLeft = 4,
            CornerRadiusBottomRight = 4,
            ContentMarginLeft = 14,
            ContentMarginTop = 12,
            ContentMarginRight = 14,
            ContentMarginBottom = 12,
            ShadowColor = new Color(0, 0, 0, 0.45f),
            ShadowSize = 5
        };
        style.SetBorderWidthAll(borderWidth);
        return style;
    }

    public static StyleBoxFlat CardStyle(string cardType, string rarity, bool playable = true)
    {
        var border = rarity switch
        {
            "rare" => Gold,
            "uncommon" => Purple.Lightened(0.12f),
            _ => Color.FromHtml("5e5a52")
        };
        var background = cardType == "attack" ? Color.FromHtml("1b1112") : Color.FromHtml("111317");
        if (!playable)
        {
            background = Color.FromHtml("171717");
            border = Color.FromHtml("3a3732");
        }

        var style = PanelStyle(MistPanelVariant.Stone);
        style.BgColor = background;
        style.BorderColor = border;
        style.CornerRadiusTopLeft = 6;
        style.CornerRadiusTopRight = 6;
        style.CornerRadiusBottomLeft = 6;
        style.CornerRadiusBottomRight = 6;
        style.SetBorderWidthAll(2);
        return style;
    }

    private static Color ButtonBackground(MistButtonVariant variant)
    {
        return variant switch
        {
            MistButtonVariant.Primary => Color.FromHtml("3d2f46"),
            MistButtonVariant.Purple => Color.FromHtml("30213c"),
            MistButtonVariant.Danger => Color.FromHtml("3a2022"),
            MistButtonVariant.Gold => Color.FromHtml("3b3120"),
            MistButtonVariant.Disabled => Color.FromHtml("202020"),
            _ => Color.FromHtml("1b1b1b")
        };
    }

    private static StyleBoxFlat ButtonStyle(Color background)
    {
        var style = new StyleBoxFlat
        {
            BgColor = background,
            BorderColor = background.Lightened(0.32f),
            CornerRadiusTopLeft = 4,
            CornerRadiusTopRight = 4,
            CornerRadiusBottomLeft = 4,
            CornerRadiusBottomRight = 4,
            ContentMarginLeft = 12,
            ContentMarginTop = 8,
            ContentMarginRight = 12,
            ContentMarginBottom = 8
        };
        style.SetBorderWidthAll(2);
        return style;
    }
}
