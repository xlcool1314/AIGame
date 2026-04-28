using Godot;
using System.Collections.Generic;

public static class Localization
{
    public const string Chinese = "zh";
    public const string English = "en";

    public static string Language { get; private set; } = Chinese;

    private const string SettingsPath = "user://settings.cfg";

    private static readonly Dictionary<string, Dictionary<string, string>> Texts = new()
    {
        [Chinese] = new Dictionary<string, string>
        {
            ["game_title"] = "迷雾矿井",
            ["game_subtitle"] = "扫雷探勘 + 卡牌战斗原型",
            ["new_game"] = "新游戏",
            ["continue_game"] = "读档",
            ["settings"] = "设置",
            ["back"] = "返回",
            ["language"] = "语言",
            ["language_value"] = "中文",
            ["no_save"] = "暂无可读取的存档。",
            ["select_entry"] = "选择入口",
            ["select_next"] = "选择下一层",
            ["route_desc"] = "矿道在雾里分岔。选择一个房间继续推进，本层未选择的路线会被迷雾吞没。",
            ["run_complete"] = "矿井探索完成",
            ["run_complete_desc"] = "没有更深的路径了。",
            ["continue_deeper"] = "继续深入",
            ["back_menu"] = "返回主菜单",
            ["room_battle"] = "[战斗]",
            ["room_mine"] = "[探勘]",
            ["room_event"] = "[事件]",
            ["room_rest"] = "[休整]",
            ["room_shop"] = "[商店]",
            ["room_complete"] = "[终点]",
            ["mine_summary"] = "翻开矿格，依据数字避开暗雷，清理安全格可获得矿晶。",
            ["rest_summary"] = "恢复生命，或整理矿晶并获得临时遗物。",
            ["shop_summary"] = "花费矿晶购买卡牌或治疗。",
            ["complete_summary"] = "结束本次探索并查看成果。",
            ["unknown_room"] = "未知房间。",
            ["mine_mode_reveal"] = "模式：翻开",
            ["mine_mode_flag"] = "模式：标记",
            ["mine_status"] = "安全格: {0}/{1} | 标记: {2}/{3} | 踩雷伤害: {4} | 完成奖励: {5} 矿晶",
            ["run_status"] = "层数: {0}/{1} | HP: {2}/{3} | 矿晶: {4} | 牌组: {5} 张{6}",
            ["relics"] = " | 遗物: {0}",
            ["hp"] = "生命",
            ["shards"] = "矿晶",
            ["start_items"] = "初始道具",
            ["choose_character"] = "选择角色",
            ["start_explore"] = "开始探索",
            ["cost"] = "耗能",
            ["buy_card"] = "购买 {0} - {1} 矿晶",
            ["encounter_reward"] = "遭遇 {0}，胜利后获得战利品。",
        },
        [English] = new Dictionary<string, string>
        {
            ["game_title"] = "Mist Mine",
            ["game_subtitle"] = "Minesweeper Exploration + Card Battles",
            ["new_game"] = "New Game",
            ["continue_game"] = "Load Game",
            ["settings"] = "Settings",
            ["back"] = "Back",
            ["language"] = "Language",
            ["language_value"] = "English",
            ["no_save"] = "No save file found.",
            ["select_entry"] = "Choose Entrance",
            ["select_next"] = "Choose Next Layer",
            ["route_desc"] = "The mine splits in the fog. Choose one room to continue; the unchosen paths will vanish.",
            ["run_complete"] = "Expedition Complete",
            ["run_complete_desc"] = "There are no deeper paths.",
            ["continue_deeper"] = "Continue",
            ["back_menu"] = "Main Menu",
            ["room_battle"] = "[Battle]",
            ["room_mine"] = "[Survey]",
            ["room_event"] = "[Event]",
            ["room_rest"] = "[Rest]",
            ["room_shop"] = "[Shop]",
            ["room_complete"] = "[End]",
            ["mine_summary"] = "Reveal tiles, use numbers to avoid mines, and clear safe tiles for shards.",
            ["rest_summary"] = "Recover health, or prepare gear for shards and a relic record.",
            ["shop_summary"] = "Spend shards on cards or healing.",
            ["complete_summary"] = "Finish the expedition and review your gains.",
            ["unknown_room"] = "Unknown room.",
            ["mine_mode_reveal"] = "Mode: Reveal",
            ["mine_mode_flag"] = "Mode: Flag",
            ["mine_status"] = "Safe: {0}/{1} | Flags: {2}/{3} | Mine Damage: {4} | Reward: {5} Shards",
            ["run_status"] = "Layer: {0}/{1} | HP: {2}/{3} | Shards: {4} | Deck: {5} cards{6}",
            ["relics"] = " | Relics: {0}",
            ["hp"] = "HP",
            ["shards"] = "Shards",
            ["start_items"] = "Starting Items",
            ["choose_character"] = "Choose Character",
            ["start_explore"] = "Start Expedition",
            ["cost"] = "Cost",
            ["buy_card"] = "Buy {0} - {1} Shards",
            ["encounter_reward"] = "Encounter {0}. Win to earn spoils.",
        }
    };

    public static void LoadSettings()
    {
        var config = new ConfigFile();
        if (config.Load(SettingsPath) == Error.Ok)
        {
            SetLanguage(config.GetValue("game", "language", Chinese).AsString(), false);
        }
    }

    public static void SetLanguage(string language, bool save = true)
    {
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
        if (Texts.TryGetValue(Language, out var table) && table.TryGetValue(key, out var value))
        {
            return value;
        }

        return Texts[Chinese].TryGetValue(key, out var fallback) ? fallback : key;
    }

    public static string Pick(string zh, string en)
    {
        return Language == English && !string.IsNullOrWhiteSpace(en) ? en : zh;
    }
}
