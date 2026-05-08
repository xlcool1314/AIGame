using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

/// <summary>
/// Unified card display component inspired by Slay the Spire.
/// Vertical card layout: cost orb (top-left), card name (centered),
/// description (auto-wrap), rarity/type tag (bottom).
/// </summary>
public enum CardCellMode
{
	Hand,
	Deck,
	Reward,
	Treasure,
	Shop,
	Upgrade,
	Remove
}

public partial class CardCell : PanelContainer
{
	private const string StylePath = "res://data/card_styles.json";
	private const string CardScenePath = "res://scenes/ui/CardView.tscn";
	private static bool _styleLoaded;
	private static bool _cardSceneChecked;
	private static PackedScene? _cardScene;
	private static readonly Dictionary<string, CardStyleData> CardStyles = new();
	private static readonly Dictionary<string, string> RarityBorders = new();

	private CardData _card = null!;
	private CardCellMode _mode;
	private bool _playable = true;
	private string _footerText = string.Empty;
	private int _index = -1;

	// Child nodes
	private Label _costOrb = null!;
	private Label _nameLabel = null!;
	private Label _descriptionLabel = null!;
	private Label _typeTag = null!;
	private Label _footerLabel = null!;
	private VBoxContainer _contentLayout = null!;
	private PanelContainer _costOrbBg = null!;
	private TextureRect? _fullCardTexture;
	private TextureRect? _frameTexture;
	private TextureRect? _artworkTexture;
	private TextureRect? _typeIconTexture;
	private Label? _placeholderGlyph;
	private bool _uiBuilt;

	// ── Colors ────────────────────────────────────────────────
	// Border / rarity
	private static readonly Color ColorRare      = Color.FromHtml("f5c842");
	private static readonly Color ColorUncommon   = Color.FromHtml("5ba4e8");
	private static readonly Color ColorCommon     = Color.FromHtml("8a929b");
	private static readonly Color ColorCurse      = Color.FromHtml("9b4dca");

	// Background by type
	private static readonly Color ColorBgAttack     = Color.FromHtml("3a1a1a");
	private static readonly Color ColorBgSkill      = Color.FromHtml("1a2a3a");
	private static readonly Color ColorBgCurse      = Color.FromHtml("2a1a30");
	private static readonly Color ColorBgUnplayable = Color.FromHtml("222a30");
	private static readonly Color ColorBgReward     = Color.FromHtml("162030");

	// Unplayable overrides
	private static readonly Color ColorBorderUnplayable = Color.FromHtml("3a424a");
	private static readonly Color ColorFontUnplayable   = Color.FromHtml("6a727a");

	// Text
	private static readonly Color ColorFontName        = Color.FromHtml("f0e8d8");
	private static readonly Color ColorFontDescription = Color.FromHtml("c8c0ae");
	private static readonly Color ColorFontCost        = Color.FromHtml("ffe680");
	private static readonly Color ColorFontCostGreen   = Color.FromHtml("8fc9a8");

	// Cost orb background
	private static readonly Color ColorCostOrbBg      = new Color(0.08f, 0.08f, 0.12f, 0.92f);

	[Signal]
	public delegate void CardClickedEventHandler(CardCell cell);

	[Signal]
	public delegate void CardGuiInputEventHandler(CardCell cell, InputEvent inputEvent);

	private bool _emitClickedOnLeft;
	private Tween? _hoverTween;
	private bool _hovered;
	private int _baseZIndex;

	public CardData Card => _card;
	public int Index => _index;
	public CardCellMode Mode => _mode;

	public override void _Ready()
	{
		MouseDefaultCursorShape = CursorShape.PointingHand;
		BuildUi();
		MouseEntered += OnCardMouseEntered;
		MouseExited += OnCardMouseExited;
		if (_card != null)
		{
			ApplyContent();
			ApplyStyle();
			ApplySize();
		}
	}

	public override void _ExitTree()
	{
		_hoverTween?.Kill();
		_hoverTween = null;
	}

	public void Setup(CardData card, CardCellMode mode, bool playable = true, string footerText = "", int index = -1)
	{
		_card = card;
		_mode = mode;
		_playable = playable;
		_footerText = footerText;
		_index = index;

		if (!IsNodeReady()) return;

		ApplyContent();
		ApplyStyle();
		ApplySize();
	}

	// ── Build UI ──────────────────────────────────────────────
	private void BuildUi()
	{
		if (_uiBuilt)
		{
			return;
		}

		_uiBuilt = true;
		MouseFilter = MouseFilterEnum.Stop;
		if (TryBindSceneUi())
		{
			GuiInput += OnGuiInput;
			return;
		}

		_contentLayout = new VBoxContainer { Name = "ContentLayout" };
		_contentLayout.AddThemeConstantOverride("separation", 2);
		AddChild(_contentLayout);

		// ── Top row: cost orb (overlapping the border) + name ──
		var topRow = new HBoxContainer { Name = "TopRow" };
		topRow.AddThemeConstantOverride("separation", 4);

		// Cost orb: a small circular badge
		_costOrbBg = new PanelContainer { Name = "CostOrbBg" };
		_costOrbBg.CustomMinimumSize = new Vector2(32, 32);
		_costOrbBg.SetAnchorsPreset(Control.LayoutPreset.Center);
		_costOrbBg.AddThemeStyleboxOverride("panel", BuildCostOrbStyle());

		_costOrb = new Label
		{
			Name = "CostOrb",
			Text = "0",
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center
		};
		_costOrb.AddThemeFontSizeOverride("font_size", 17);
		_costOrb.AddThemeColorOverride("font_color", ColorFontCost);
		_costOrbBg.AddChild(_costOrb);
		topRow.AddChild(_costOrbBg);

		// Card name
		_nameLabel = new Label
		{
			Name = "CardName",
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			VerticalAlignment = VerticalAlignment.Center
		};
		_nameLabel.AddThemeFontSizeOverride("font_size", 15);
		topRow.AddChild(_nameLabel);

		_contentLayout.AddChild(topRow);

		// ── Separator ──
		var sep = new HSeparator { Name = "Separator" };
		sep.AddThemeStyleboxOverride("separator", new StyleBoxFlat
		{
			BgColor = new Color(1, 1, 1, 0.1f),
			ContentMarginTop = 0,
			ContentMarginBottom = 0
		});
		_contentLayout.AddChild(sep);

		// ── Description ──
		_descriptionLabel = new Label
		{
			Name = "Description",
			AutowrapMode = TextServer.AutowrapMode.WordSmart,
			SizeFlagsVertical = SizeFlags.ExpandFill
		};
		_descriptionLabel.AddThemeFontSizeOverride("font_size", 13);
		_contentLayout.AddChild(_descriptionLabel);

		// ── Type / rarity tag (bottom) ──
		_typeTag = new Label
		{
			Name = "TypeTag",
			HorizontalAlignment = HorizontalAlignment.Center
		};
		_typeTag.AddThemeFontSizeOverride("font_size", 11);
		_contentLayout.AddChild(_typeTag);

		// ── Footer (price, upgrade cost, etc.) ──
		_footerLabel = new Label
		{
			Name = "Footer",
			AutowrapMode = TextServer.AutowrapMode.WordSmart,
			HorizontalAlignment = HorizontalAlignment.Center,
			Visible = false
		};
		_footerLabel.AddThemeFontSizeOverride("font_size", 12);
		_contentLayout.AddChild(_footerLabel);

		// Input
		GuiInput += OnGuiInput;
	}

	private bool TryBindSceneUi()
	{
		var layout = GetNodeOrNull<VBoxContainer>("CardSurface/ContentMargin/ContentLayout");
		if (layout == null)
		{
			return false;
		}

		_contentLayout = layout;
		_costOrbBg = GetNode<PanelContainer>("CardSurface/ContentMargin/ContentLayout/TopRow/CostOrbBg");
		_costOrb = GetNode<Label>("CardSurface/ContentMargin/ContentLayout/TopRow/CostOrbBg/CostOrb");
		_nameLabel = GetNode<Label>("CardSurface/ContentMargin/ContentLayout/TopRow/CardName");
		_descriptionLabel = GetNode<Label>("CardSurface/ContentMargin/ContentLayout/Description");
		_typeTag = GetNode<Label>("CardSurface/ContentMargin/ContentLayout/TypeTag");
		_footerLabel = GetNode<Label>("CardSurface/ContentMargin/ContentLayout/Footer");
		_fullCardTexture = GetNodeOrNull<TextureRect>("CardSurface/FullCardArt");
		_frameTexture = GetNodeOrNull<TextureRect>("CardSurface/CardFrame");
		_artworkTexture = GetNodeOrNull<TextureRect>("CardSurface/ContentMargin/ContentLayout/ArtFrame/CardArtwork");
		_typeIconTexture = GetNodeOrNull<TextureRect>("CardSurface/ContentMargin/ContentLayout/TopRow/TypeIcon");
		_placeholderGlyph = GetNodeOrNull<Label>("CardSurface/ContentMargin/ContentLayout/ArtFrame/PlaceholderGlyph");
		return true;
	}

	private void OnGuiInput(InputEvent ev)
	{
		EmitSignal(SignalName.CardGuiInput, this, ev);
		if (_emitClickedOnLeft && ev is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
		{
			EmitSignal(SignalName.CardClicked, this);
		}
	}

	private void OnCardMouseEntered()
	{
		if (!CanPlayHoverAnimation())
		{
			return;
		}

		_hovered = true;
		_baseZIndex = ZIndex;
		ZIndex = Math.Max(ZIndex, 20);
		UpdateHoverPivot();
		AnimateHover(new Vector2(1.045f, 1.045f), new Color(1.08f, 1.08f, 1.08f, 1f));
	}

	private void OnCardMouseExited()
	{
		if (!_hovered)
		{
			return;
		}

		_hovered = false;
		if (ZIndex >= 100)
		{
			return;
		}

		ZIndex = _baseZIndex;
		AnimateHover(Vector2.One, Colors.White);
	}

	private bool CanPlayHoverAnimation()
	{
		return _card != null && IsInsideTree() && ZIndex < 100;
	}

	private void UpdateHoverPivot()
	{
		var size = Size.X > 0 && Size.Y > 0 ? Size : CustomMinimumSize;
		if (size.X <= 0 || size.Y <= 0)
		{
			return;
		}

		PivotOffset = _mode == CardCellMode.Hand
			? new Vector2(size.X / 2f, size.Y)
			: size / 2f;
	}

	private void AnimateHover(Vector2 targetScale, Color targetModulate)
	{
		_hoverTween?.Kill();
		_hoverTween = CreateTween();
		_hoverTween.SetParallel(true);
		_hoverTween.TweenProperty(this, "scale", targetScale, 0.1f)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.Out);
		_hoverTween.TweenProperty(this, "modulate", targetModulate, 0.1f)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.Out);
	}

	// ── Apply content ─────────────────────────────────────────
	private void ApplyContent()
	{
		if (_card == null) return;

		_costOrb.Text = _card.Cost.ToString();
		_nameLabel.Text = _card.DisplayName();
		_descriptionLabel.Text = _card.DisplayDescription();
		_typeTag.Text = FormatTypeRarity();

		if (!string.IsNullOrEmpty(_footerText))
		{
			_footerLabel.Text = _footerText;
			_footerLabel.Visible = true;
		}
		else
		{
			_footerLabel.Visible = false;
		}

		TooltipText = $"{_card.DisplayName()} - {_card.DisplayDescription()}";
		ApplyArtwork();

		if (_mode == CardCellMode.Deck)
			MouseDefaultCursorShape = CursorShape.Arrow;

		_emitClickedOnLeft = _mode != CardCellMode.Hand && _mode != CardCellMode.Deck;
	}

	private string FormatTypeRarity()
	{
		var type = FormatTypeName(_card.Type);
		var rarity = FormatRarityName(_card.Rarity);
		return $"{type} · {rarity}";
	}

	public static string FormatTypeName(string type)
	{
		return type switch
		{
			"attack" => Localization.T("card_type_attack"),
			"skill" => Localization.T("card_type_skill"),
			"curse" => Localization.T("card_type_curse"),
			_ => type
		};
	}

	public static string FormatRarityName(string rarity)
	{
		return rarity switch
		{
			"rare" => Localization.T("rarity_rare"),
			"uncommon" => Localization.T("rarity_uncommon"),
			"curse" => Localization.T("rarity_curse"),
			_ => Localization.T("rarity_common")
		};
	}

	// ── Apply style ───────────────────────────────────────────
	private void ApplyStyle()
	{
		if (_card == null) return;

		var visualStyle = ResolveStyle();
		var border = GetBorderColor(visualStyle);
		var background = ParseColor(visualStyle?.Background, GetBackgroundColor());
		var fontColor = ParseColor(visualStyle?.Name, _playable ? ColorFontName : ColorFontUnplayable);
		var descColor = ParseColor(visualStyle?.Description, _playable ? ColorFontDescription : ColorFontUnplayable);

		// Cost orb color
		if (!_playable)
			_costOrb.AddThemeColorOverride("font_color", ParseColor(visualStyle?.Cost, ColorFontUnplayable));
		else if (_card.Cost == 0)
			_costOrb.AddThemeColorOverride("font_color", ColorFontCostGreen);
		else
			_costOrb.AddThemeColorOverride("font_color", ColorFontCost);

		// Name
		_nameLabel.AddThemeColorOverride("font_color", fontColor);

		// Description
		_descriptionLabel.AddThemeColorOverride("font_color", descColor);

		// Type tag
		_typeTag.AddThemeColorOverride("font_color", border.Lightened(0.2f));

		// Footer
		_footerLabel.AddThemeColorOverride("font_color", new Color(0.65f, 0.7f, 0.78f));

		// Card panel style
		var borderWidth = _card.Rarity == "rare" ? 3 : 2;
		var panelStyle = BuildCardStyle(background, border, borderWidth);
		AddThemeStyleboxOverride("panel", panelStyle);
		ApplyArtwork();

		_costOrbBg.AddThemeStyleboxOverride("panel", BuildCostOrbStyle(_card.Rarity == "rare" && _playable ? ColorRare : null));
	}

	private void ApplyArtwork()
	{
		if (_card == null)
		{
			return;
		}

		ApplyOptionalTexture(_fullCardTexture, ResolveFullCardPath());
		ApplyOptionalTexture(_frameTexture, ResolveFramePath());
		var artwork = ResolveArtworkPath();
		ApplyOptionalTexture(_artworkTexture, artwork);
		ApplyOptionalTexture(_typeIconTexture, ResolveIconPath());

		if (_placeholderGlyph != null)
		{
			_placeholderGlyph.Visible = string.IsNullOrWhiteSpace(artwork);
			_placeholderGlyph.Text = _card.Type switch
			{
				"attack" => "ATK",
				"curse" => "HEX",
				_ => "SKL"
			};
			_placeholderGlyph.AddThemeColorOverride("font_color", GetBorderColor().Lightened(0.28f));
		}
	}

	private static void ApplyOptionalTexture(TextureRect? target, string path)
	{
		if (target == null)
		{
			return;
		}

		var texture = UiArt.LoadTexture(path);
		target.Texture = texture;
		target.Visible = texture != null;
	}

	private string ResolveFullCardPath()
	{
		return FirstExisting(_card.FullArtPath, CandidatePaths("res://art/cards/full", _card.Id));
	}

	private string ResolveArtworkPath()
	{
		return FirstExisting(_card.ArtPath, CandidatePaths("res://art/cards/illustrations", _card.Id));
	}

	private string ResolveFramePath()
	{
		var template = string.IsNullOrWhiteSpace(_card.TemplateId) ? string.Empty : _card.TemplateId;
		return FirstExisting(
			_card.FramePath,
			CandidatePaths("res://art/cards/frames", template),
			CandidatePaths("res://art/cards/frames", $"{_card.Type}_{_card.Rarity}"),
			CandidatePaths("res://art/cards/frames", _card.Type),
			CandidatePaths("res://art/cards/frames", "default"));
	}

	private string ResolveIconPath()
	{
		return FirstExisting(
			_card.IconPath,
			CandidatePaths("res://art/cards/icons", _card.Id),
			CandidatePaths("res://art/cards/icons", _card.Type),
			CandidatePaths("res://art/cards/icons", _card.Rarity));
	}

	private static string FirstExisting(params string[][] groups)
	{
		foreach (var group in groups)
		{
			var path = FirstExistingCandidatePaths(group);
			if (!string.IsNullOrWhiteSpace(path))
			{
				return path;
			}
		}

		return string.Empty;
	}

	private static string FirstExisting(string explicitPath, params string[][] groups)
	{
		if (!string.IsNullOrWhiteSpace(explicitPath) && UiArt.ResourceExists(explicitPath))
		{
			return explicitPath;
		}

		return FirstExisting(groups);
	}

	private static string FirstExistingCandidatePaths(IEnumerable<string> candidates)
	{
		foreach (var path in candidates)
		{
			if (!string.IsNullOrWhiteSpace(path) && UiArt.ResourceExists(path))
			{
				return path;
			}
		}

		return string.Empty;
	}

	private static string[] CandidatePaths(string folder, string name)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			return Array.Empty<string>();
		}

		return new[]
		{
			$"{folder}/{name}.png",
			$"{folder}/{name}.webp",
			$"{folder}/{name}.jpg",
			$"{folder}/{name}.jpeg",
			$"{folder}/{name}.svg"
		};
	}

	private Color GetBorderColor()
	{
		return GetBorderColor(ResolveStyle());
	}

	private Color GetBorderColor(CardStyleData? visualStyle)
	{
		if (!_playable) return ColorBorderUnplayable;
		if (!string.IsNullOrWhiteSpace(visualStyle?.Border))
		{
			return ParseColor(visualStyle.Border, ColorCommon);
		}

		LoadStyles();
		if (RarityBorders.TryGetValue(_card.Rarity, out var configured))
		{
			return ParseColor(configured, ColorCommon);
		}

		return _card.Rarity switch
		{
			"rare"     => ColorRare,
			"uncommon" => ColorUncommon,
			"curse"    => ColorCurse,
			_ => ColorCommon
		};
	}

	private Color GetBackgroundColor()
	{
		if (!_playable) return ColorBgUnplayable;

		if (_mode == CardCellMode.Reward || _mode == CardCellMode.Treasure || _mode == CardCellMode.Shop)
			return ColorBgReward;

		return _card.Type switch
		{
			"attack" => ColorBgAttack,
			"curse"  => ColorBgCurse,
			_ => ColorBgSkill
		};
	}

	private CardStyleData? ResolveStyle()
	{
		LoadStyles();
		var styleId = !_playable
			? "unplayable"
			: !string.IsNullOrWhiteSpace(_card.StyleId)
				? _card.StyleId
				: (_mode == CardCellMode.Reward || _mode == CardCellMode.Treasure || _mode == CardCellMode.Shop)
					? "reward"
					: _card.Type;

		return CardStyles.TryGetValue(styleId, out var style) ? style : null;
	}

	private void ApplySize()
	{
		switch (_mode)
		{
			case CardCellMode.Hand:
				CustomMinimumSize = new Vector2(146, 190);
				SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
				_nameLabel.AddThemeFontSizeOverride("font_size", 15);
				_descriptionLabel.AddThemeFontSizeOverride("font_size", 13);
				break;
			case CardCellMode.Deck:
				CustomMinimumSize = new Vector2(146, string.IsNullOrWhiteSpace(_footerText) ? 190 : 214);
				SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
				_nameLabel.AddThemeFontSizeOverride("font_size", 15);
				_descriptionLabel.AddThemeFontSizeOverride("font_size", 13);
				break;
			case CardCellMode.Reward:
			case CardCellMode.Treasure:
				CustomMinimumSize = new Vector2(0, 112);
				SizeFlagsHorizontal = SizeFlags.ExpandFill;
				_nameLabel.AddThemeFontSizeOverride("font_size", 14);
				_descriptionLabel.AddThemeFontSizeOverride("font_size", 12);
				break;
			case CardCellMode.Shop:
				CustomMinimumSize = new Vector2(0, 110);
				SizeFlagsHorizontal = SizeFlags.ExpandFill;
				_nameLabel.AddThemeFontSizeOverride("font_size", 14);
				_descriptionLabel.AddThemeFontSizeOverride("font_size", 12);
				break;
			case CardCellMode.Upgrade:
			case CardCellMode.Remove:
				CustomMinimumSize = new Vector2(0, 95);
				SizeFlagsHorizontal = SizeFlags.ExpandFill;
				_nameLabel.AddThemeFontSizeOverride("font_size", 14);
				_descriptionLabel.AddThemeFontSizeOverride("font_size", 12);
				break;
		}
	}

	private static StyleBoxFlat BuildCardStyle(Color background, Color border, int borderWidth = 2)
	{
		var style = new StyleBoxFlat
		{
			BgColor = background,
			BorderColor = border,
			CornerRadiusTopLeft = 10,
			CornerRadiusTopRight = 10,
			CornerRadiusBottomLeft = 10,
			CornerRadiusBottomRight = 10,
			ContentMarginLeft = 10,
			ContentMarginTop = 8,
			ContentMarginRight = 10,
			ContentMarginBottom = 6,
			ShadowColor = new Color(0, 0, 0, 0.4f),
			ShadowSize = 5
		};
		style.SetBorderWidthAll(borderWidth);
		return style;
	}

	private static StyleBoxFlat BuildCostOrbStyle(Color? border = null)
	{
		var style = new StyleBoxFlat
		{
			BgColor = ColorCostOrbBg,
			BorderColor = border ?? new Color(0, 0, 0, 0),
			CornerRadiusTopLeft = 16,
			CornerRadiusTopRight = 16,
			CornerRadiusBottomLeft = 16,
			CornerRadiusBottomRight = 16,
			ContentMarginLeft = 0,
			ContentMarginTop = 0,
			ContentMarginRight = 0,
			ContentMarginBottom = 0
		};
		style.SetBorderWidthAll(border.HasValue ? 2 : 0);
		return style;
	}

	private static Color ParseColor(string? hex, Color fallback)
	{
		return string.IsNullOrWhiteSpace(hex) ? fallback : Color.FromHtml(hex);
	}

	private static void LoadStyles()
	{
		if (_styleLoaded)
		{
			return;
		}

		_styleLoaded = true;
		if (!FileAccess.FileExists(StylePath))
		{
			return;
		}

		try
		{
			using var file = FileAccess.Open(StylePath, FileAccess.ModeFlags.Read);
			var config = JsonSerializer.Deserialize<CardStyleConfig>(file.GetAsText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
			if (config == null)
			{
				return;
			}

			foreach (var style in config.Styles)
			{
				if (!string.IsNullOrWhiteSpace(style.Id))
				{
					CardStyles[style.Id] = style;
				}
			}

			foreach (var pair in config.RarityBorders)
			{
				RarityBorders[pair.Key] = pair.Value;
			}
		}
		catch (Exception error)
		{
			GD.PushWarning($"Card style load failed: {error.Message}");
		}
	}

	/// <summary>
	/// Factory method: create and configure a CardCell in one call.
	/// </summary>
	public static CardCell Create(CardData card, CardCellMode mode, bool playable = true, string footerText = "", int index = -1)
	{
		var cell = CreateFromScene() ?? new CardCell();
		cell.Setup(card, mode, playable, footerText, index);
		return cell;
	}

	private static CardCell? CreateFromScene()
	{
		if (!_cardSceneChecked)
		{
			_cardSceneChecked = true;
			if (UiArt.ResourceExists(CardScenePath))
			{
				try
				{
					_cardScene = ResourceLoader.Load<PackedScene>(CardScenePath);
				}
				catch (Exception error)
				{
					GD.PushWarning($"Card prefab load failed: {error.Message}");
					_cardScene = null;
				}
			}
		}

		return _cardScene?.Instantiate<CardCell>();
	}
}

public sealed class CardStyleConfig
{
	public List<CardStyleData> Styles { get; set; } = new();
	public Dictionary<string, string> RarityBorders { get; set; } = new();
}

public sealed class CardStyleData
{
	public string Id { get; set; } = string.Empty;
	public string Background { get; set; } = string.Empty;
	public string Border { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public string Cost { get; set; } = string.Empty;
}
