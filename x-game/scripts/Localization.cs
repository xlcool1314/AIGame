using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public static class Localization
{
	public const string Chinese = "zh";
	public const string English = "en";

	public static string Language { get; private set; } = Chinese;

	private const string SettingsPath = "user://settings.cfg";
	private const string TextPath = "res://data/localization.json";
	private static bool _textsLoaded;
	private static readonly HashSet<string> MissingKeys = new();

	private static Dictionary<string, Dictionary<string, string>> Texts = new()
	{
		[Chinese] = new Dictionary<string, string>
		{
			["game_title"] = "怪奇小屋",
			["game_subtitle"] = "父母睡着以后，床下暗门悄悄打开。",
			["new_game"] = "新的夜晚",
			["continue_game"] = "继续探险",
			["settings"] = "设置",
			["back"] = "返回",
			["language"] = "语言",
			["language_value"] = "中文",
			["no_save"] = "暂无可继续的探险记录。",
			["select_entry"] = "从卧室暗门出发",
			["select_next"] = "选择下一个地下房间",
			["route_desc"] = "父母还在熟睡。顺着房间下方的木梯往下选一个房间探索；这一层没进入的门会在夜色里悄悄合上。",
			["route_map_legend"] = "小屋剖面图：当前 / 可进入 / 可预见 / 已错过   战=怪物  探=探索  藏=藏品  ?=怪事  躲=躲藏  换=交换  机=机关",
			["route_current"] = "当前位置",
			["route_available"] = "可进入",
			["route_future"] = "可预见",
			["route_lost"] = "已错过",
			["route_next"] = "后续",
			["route_endpoint"] = "终点",
			["route_layer"] = "地下第 {0} 间",
			["route_entry_layer"] = "卧室暗门",
			["run_complete"] = "今晚的冒险结束",
			["run_complete_desc"] = "再往下已经没有亮着的房间了。",
			["continue_deeper"] = "继续下楼",
			["back_menu"] = "返回主菜单",
			["room_battle"] = "[怪物]",
			["room_mine"] = "[探索]",
			["room_event"] = "[怪事]",
			["room_rest"] = "[躲藏]",
			["room_shop"] = "[交换]",
			["room_complete"] = "[天亮]",
			["mine_summary"] = "翻找房间格子，依据数字避开怪东西，找到安全线索可获得纽扣。",
			["rest_summary"] = "躲进安全角落，恢复勇气，或整理口袋里的小物件。",
			["shop_summary"] = "用纽扣和地下朋友交换卡牌、零食或小道具。",
			["complete_summary"] = "结束今晚的冒险并查看收获。",
			["unknown_room"] = "未知房间。",
			["mine_mode_reveal"] = "模式：翻找",
			["mine_mode_flag"] = "模式：贴纸",
			["mine_status"] = "安全线索: {0}/{1} | 贴纸: {2}/{3} | 惊吓伤害: {4} | 完成奖励: {5} 纽扣",
			["run_status"] = "房间: {0}/{1} | 勇气: {2}/{3} | 纽扣: {4} | 牌组: {5} 张{6}",
			["relics"] = " | 纪念物: {0}",
			["hp"] = "勇气",
			["shards"] = "纽扣",
			["start_items"] = "初始道具",
			["choose_character"] = "选择今晚出门的孩子",
			["start_explore"] = "钻进暗门",
			["cost"] = "消耗",
			["buy_card"] = "交换 {0} - {1} 纽扣",
			["encounter_reward"] = "遇到 {0}，胜利后获得发现。",
		},
		[English] = new Dictionary<string, string>
		{
			["game_title"] = "Odd Little House",
			["game_subtitle"] = "After the parents fall asleep, a trapdoor opens under the bed.",
			["new_game"] = "New Night",
			["continue_game"] = "Continue Adventure",
			["settings"] = "Settings",
			["back"] = "Back",
			["language"] = "Language",
			["language_value"] = "English",
			["no_save"] = "No adventure record found.",
			["select_entry"] = "Leave Through the Bedroom Trapdoor",
			["select_next"] = "Choose the Next Underground Room",
			["route_desc"] = "Your parents are asleep. Pick one room below the house; the doors you skip will quietly shut for the night.",
			["route_map_legend"] = "House Cutaway: Current / Available / Planned / Lost    B=Monster  E=Explore  C=Curio  ?=Oddity  H=Hideout  T=Trade  G=Gadget",
			["route_current"] = "Current",
			["route_available"] = "Available",
			["route_future"] = "Planned",
			["route_lost"] = "Lost",
			["route_next"] = "Next",
			["route_endpoint"] = "End",
			["route_layer"] = "Room {0} Below",
			["route_entry_layer"] = "Bedroom Trapdoor",
			["run_complete"] = "The Night Adventure Ends",
			["run_complete_desc"] = "No lit rooms remain below.",
			["continue_deeper"] = "Go Downstairs",
			["back_menu"] = "Main Menu",
			["room_battle"] = "[Monster]",
			["room_mine"] = "[Explore]",
			["room_event"] = "[Oddity]",
			["room_rest"] = "[Hideout]",
			["room_shop"] = "[Trade]",
			["room_complete"] = "[Dawn]",
			["mine_summary"] = "Search room tiles, use numbers to avoid creepy things, and collect buttons from safe clues.",
			["rest_summary"] = "Hide in a safe corner, recover courage, or sort pocket treasures.",
			["shop_summary"] = "Trade buttons with underground friends for cards, snacks, and tools.",
			["complete_summary"] = "End tonight's adventure and review your finds.",
			["unknown_room"] = "Unknown room.",
			["mine_mode_reveal"] = "Mode: Search",
			["mine_mode_flag"] = "Mode: Sticker",
			["mine_status"] = "Safe Clues: {0}/{1} | Stickers: {2}/{3} | Scare Damage: {4} | Reward: {5} Buttons",
			["run_status"] = "Room: {0}/{1} | Courage: {2}/{3} | Buttons: {4} | Deck: {5} cards{6}",
			["relics"] = " | Keepsakes: {0}",
			["hp"] = "Courage",
			["shards"] = "Buttons",
			["start_items"] = "Starting Items",
			["choose_character"] = "Choose Tonight's Kid",
			["start_explore"] = "Enter the Trapdoor",
			["cost"] = "Cost",
			["buy_card"] = "Trade {0} - {1} Buttons",
			["encounter_reward"] = "Encounter {0}. Win to earn finds.",
		}
	};

	public static void LoadSettings()
	{
		LoadTexts();

		var config = new ConfigFile();
		if (config.Load(SettingsPath) == Error.Ok)
		{
			SetLanguage(config.GetValue("game", "language", Chinese).AsString(), false);
		}
	}

	public static void LoadTexts(bool force = false)
	{
		if (_textsLoaded && !force)
		{
			return;
		}

		_textsLoaded = true;
		if (!FileAccess.FileExists(TextPath))
		{
			return;
		}

		try
		{
			using var file = FileAccess.Open(TextPath, FileAccess.ModeFlags.Read);
			var json = file.GetAsText();
			var loaded = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(json);
			if (loaded != null && loaded.Count > 0)
			{
				Texts = loaded;
			}
		}
		catch (Exception error)
		{
			GD.PushWarning($"Localization load failed: {error.Message}");
		}
	}

	public static void SetLanguage(string language, bool save = true)
	{
		LoadTexts();
		Language = language == English ? English : Chinese;
		if (!save)
		{
			return;
		}

		var config = new ConfigFile();
		config.SetValue("game", "language", Language);
		config.Save(SettingsPath);
	}

	public static string T(string key)
	{
		LoadTexts();
		if (Texts.TryGetValue(Language, out var table) && table.TryGetValue(key, out var value))
		{
			return value;
		}

		if (Texts.TryGetValue(Chinese, out var fallbackTable) && fallbackTable.TryGetValue(key, out var fallback))
		{
			return fallback;
		}

		if (MissingKeys.Add(key))
		{
			GD.PushWarning($"Missing localization key: {key}");
		}

		return key;
	}

	public static string T(string key, params object[] args)
	{
		return string.Format(T(key), args);
	}

	public static string Pick(string zh, string en)
	{
		return Language == English && !string.IsNullOrWhiteSpace(en) ? en : zh;
	}
}
