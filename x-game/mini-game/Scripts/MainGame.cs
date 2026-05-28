#nullable enable

using Godot;
using System;
using System.Collections.Generic;

public partial class MainGame : Node2D
{
    private const int ScreenWidth = 1920;
    private const int ScreenHeight = 1080;
    private const int WavesPerSector = 8;
    private const int SectorCount = 5;
    private const int TotalWaves = WavesPerSector * SectorCount;
    private const float PlayerRadius = 24.0f;
    private const float EnemyBulletRadius = 8.0f;
    private const float PlayerBulletRadius = 7.0f;
    private const float SampleRate = 44100.0f;
    private const int MaxEnemies = 96;
    private const int MaxShots = 480;
    private const int MaxPickups = 260;
    private const int MaxParticles = 520;
    private const int MaxDamageTexts = 72;
    private const int MaxPoolSize = 900;
    private const int PlayerTrailCapacity = 14;
    private const int ShotTrailCapacity = 4;
    private const float PolaritySwitchCooldownBase = 2.65f;
    private const float PolaritySwitchCooldownMin = 1.75f;
    private const float EnemyBulletSpeedStartScale = 0.54f;
    private const float EnemyBulletSpeedEndScale = 0.84f;
    private const int EnemyBulletCapStart = 72;
    private const int EnemyBulletCapEnd = 240;
    private const float CruiseChargeMax = 100.0f;
    private const float CruiseGrazeRadius = 58.0f;
    private const float AssaultBurstMin = 1.45f;
    private const float AssaultBurstMax = 4.4f;
    private const float EnemyTelegraphLead = 0.58f;
    private const float EnemyOverheatBase = 1.55f;
    private const float UiHairline = 0.8f;
    private const float UiStroke = 1.05f;
    private const float UiAccentStroke = 1.3f;
    private const int VesperUnlockWave = 8;
    private const int VesperUnlockKills = 120;
    private const int KairoUnlockWave = 14;
    private const int KairoUnlockPickups = 220;
    private const int KairoUnlockRuns = 3;
    private const int SolUnlockWave = 22;
    private const int SolUnlockBosses = 3;
    private const int SolUnlockPerfectWaves = 10;

    private static readonly Vector2 ScreenCenter = new(ScreenWidth * 0.5f, ScreenHeight * 0.5f);
    private static readonly Rect2 Arena = new(new Vector2(84.0f, 86.0f), new Vector2(1752.0f, 884.0f));
    private static readonly Color Void = new(0.012f, 0.014f, 0.028f);
    private static readonly Color Ink = new(0.025f, 0.028f, 0.055f);
    private static readonly Color Cyan = new(0.12f, 0.86f, 1.0f);
    private static readonly Color Gold = new(1.0f, 0.62f, 0.16f);
    private static readonly Color Rose = new(1.0f, 0.18f, 0.43f);
    private static readonly Color Jade = new(0.28f, 1.0f, 0.62f);
    private static readonly Color Violet = new(0.58f, 0.35f, 1.0f);
    private static readonly Color Paper = new(0.92f, 0.96f, 1.0f);
    private static readonly Color Steel = new(0.42f, 0.48f, 0.54f);
    private static readonly Color Graphite = new(0.075f, 0.092f, 0.105f);
    private static readonly Color Panel = new(0.11f, 0.135f, 0.15f);
    private static readonly Color GridLine = new(0.34f, 0.42f, 0.46f);
    private static readonly Color PolarityBlue = new(0.2f, 0.72f, 0.92f);
    private static readonly Color PolarityAmber = new(0.96f, 0.72f, 0.2f);
    private static readonly Color XpGreen = new(0.68f, 0.86f, 0.18f);
    private static readonly Color PickupBlue = new(0.3f, 0.58f, 0.88f);
    private static readonly Color AlertRed = new(1.0f, 0.28f, 0.16f);
    private static readonly Color EnemyFireRed = new(1.0f, 0.16f, 0.10f);
    private static readonly string[] UpgradeHighlightTerms =
    {
        "Star Dust",
        "opposite-polarity",
        "invulnerable",
        "regenerates",
        "ultimate",
        "stronger",
        "protects",
        "restore",
        "restores",
        "repairs",
        "repair",
        "launches",
        "clears",
        "convert",
        "charge",
        "damage",
        "bullets",
        "pierce",
        "drones",
        "energy",
        "shield",
        "reroll",
        "pickup",
        "pickups",
        "enemies",
        "faster",
        "larger",
        "cheaper",
        "wider",
        "volley",
        "volleys",
        "shots",
        "beams",
        "rays",
        "clear",
        "cycle",
        "opposite",
        "longer",
        "extra",
        "more",
        "adds",
        "cost",
        "hull",
        "fire",
        "dash",
        "slow",
        "boss",
        "elites",
        "自动攻击",
        "蓄能时间",
        "伤害",
        "生命",
        "能量",
        "射速",
        "子弹",
        "穿透",
        "无人机",
        "反色",
        "火力",
        "弹道",
        "蓄能",
        "弹幕",
        "保护",
        "回响",
        "齐射",
        "清除",
        "转化",
        "概率",
        "修复",
        "压制",
        "消耗",
        "上限",
        "速度",
        "环绕",
        "冲刺",
        "移动",
        "拾取",
        "敌人",
        "减速",
        "大招",
        "回复",
        "恢复",
        "重抽",
        "星尘",
        "精英",
        "护盾",
        "无敌",
        "范围",
        "降低",
        "提高",
        "增加",
        "额外",
        "更多",
        "更快",
        "更高",
        "更少",
        "更强",
        "更远",
        "更久",
        "更宽",
        "更便宜",
        "立即",
        "掉落",
    };

    private static readonly SectorInfo[] Sectors =
    {
        new("sector.0.name", "sector.0.trait", Cyan),
        new("sector.1.name", "sector.1.trait", new Color(0.42f, 0.9f, 1.0f)),
        new("sector.2.name", "sector.2.trait", Jade),
        new("sector.3.name", "sector.3.trait", Violet),
        new("sector.4.name", "sector.4.trait", Rose),
    };

    private const string MetaSavePath = "user://astra_fracture_meta.cfg";

    private static readonly MetaUpgradeDef[] MetaUpgrades =
    {
        new(MetaUpgradeId.HullPlating, "meta.hull.title", "meta.hull.body", Rose, 7, 50, 14),
        new(MetaUpgradeId.ReactorSeed, "meta.energy.title", "meta.energy.body", Cyan, 6, 55, 16),
        new(MetaUpgradeId.FocusLens, "meta.weapon.title", "meta.weapon.body", Gold, 6, 65, 18),
        new(MetaUpgradeId.DriftEngine, "meta.engine.title", "meta.engine.body", Violet, 6, 50, 16),
        new(MetaUpgradeId.SalvageRig, "meta.salvage.title", "meta.salvage.body", Jade, 5, 80, 24),
        new(MetaUpgradeId.StarterChart, "meta.chart.title", "meta.chart.body", new Color(0.72f, 0.82f, 1.0f), 3, 110, 42),
        new(MetaUpgradeId.RepairProtocol, "meta.repair.title", "meta.repair.body", new Color(0.4f, 1.0f, 0.66f), 5, 70, 20),
        new(MetaUpgradeId.AegisMatrix, "meta.aegis.title", "meta.aegis.body", new Color(0.64f, 0.82f, 1.0f), 4, 85, 26),
        new(MetaUpgradeId.NovaCatalyst, "meta.nova.title", "meta.nova.body", new Color(1.0f, 0.54f, 0.22f), 5, 75, 22),
        new(MetaUpgradeId.DroneDock, "meta.drone.title", "meta.drone.body", PickupBlue, 4, 90, 28),
        new(MetaUpgradeId.PolarityTuner, "meta.tuner.title", "meta.tuner.body", PolarityAmber, 4, 80, 25),
        new(MetaUpgradeId.DeepSurvey, "meta.survey.title", "meta.survey.body", new Color(0.76f, 0.62f, 1.0f), 4, 95, 30),
    };

    private static readonly Dictionary<string, LocalizedText> Texts = new()
    {
        ["wake"] = new("WAKE", "觉醒"),
        ["choir.core.event"] = new("THE CHOIR CORE", "合唱核心"),
        ["wave.intro"] = new("WAVE {0}", "第 {0} 波"),
        ["sector.enter"] = new("SECTOR {0}: {1}", "第 {0} 章：{1}"),
        ["sector.cleared"] = new("{0} CLEARED", "通过：{0}"),
        ["sector.0.name"] = new("Lumen Shoal", "光滩星区"),
        ["sector.0.trait"] = new("Calm opening field. Learn Cruise, Assault, and enemy overheat windows.", "第一章没有环境危险。练习巡航、突击和敌人过热窗口。"),
        ["sector.1.name"] = new("Glass Reef", "玻璃星区"),
        ["sector.1.trait"] = new("Warning beams cut across the arena before firing.", "光束会先显示预警线，随后造成伤害。"),
        ["sector.2.name"] = new("Verdant Grave", "孢子星区"),
        ["sector.2.trait"] = new("Spores drift through space. Enemies arrive heavier, but repairs are more common.", "敌人血量更高，但修复掉落更多。"),
        ["sector.3.name"] = new("Clock Cathedral", "时钟星区"),
        ["sector.3.trait"] = new("Time shears the arena. Bullets pulse faster and slower in alternating bars.", "环境光束角度更复杂，弹幕也更密。"),
        ["sector.4.name"] = new("Solar Wound", "太阳裂口"),
        ["sector.4.trait"] = new("Final sector. Elite enemies and solar lances try to end the pattern.", "最终章。精英敌人更多，Boss 更强。"),
        ["repair"] = new("REPAIR", "修复"),
        ["language.changed"] = new("LANGUAGE: ENGLISH", "当前语言：中文"),
        ["language.hint"] = new("LANGUAGE: ENGLISH  [L]", "中文  [L切换]"),
        ["menu.start"] = new("START EXPEDITION", "开始远征"),
        ["menu.meta"] = new("PERMANENT UPGRADES", "永久升级"),
        ["menu.language"] = new("SWITCH LANGUAGE", "切换语言"),
        ["menu.settings"] = new("SETTINGS", "设置"),
        ["menu.pilot"] = new("PILOT", "角色"),
        ["menu.tip"] = new("Click START or press Enter. Spend Star Dust in Permanent Upgrades between runs.", "点击“开始远征”或按 Enter。每局结束后可用星尘购买永久升级。"),
        ["boss.choir_core"] = new("CHOIR CORE", "合唱核心"),
        ["boss.sector"] = new("{0} CORE", "{0} Boss"),
        ["wave.enemy.focus"] = new("PRIMARY: {0} - {1}", "主敌：{0} - {1}"),
        ["wave.enemy.support"] = new("SUPPORT: {0}", "辅助：{0}"),
        ["wave.enemy.new"] = new("NEW ENEMY: {0}", "新敌人：{0}"),
        ["enemy.chaser.name"] = new("Chaser", "追猎者"),
        ["enemy.chaser.role"] = new("rushes you and fires simple aimed shots", "直线追击，发射简单瞄准弹"),
        ["enemy.weaver.name"] = new("Weaver", "织弹者"),
        ["enemy.weaver.role"] = new("moves sideways and fires small fans", "横向游走，发射小扇形弹"),
        ["enemy.turret.name"] = new("Turret", "炮台"),
        ["enemy.turret.role"] = new("keeps distance and creates ring patterns", "保持距离，制造环形弹幕"),
        ["enemy.splitter.name"] = new("Splitter", "分裂体"),
        ["enemy.splitter.role"] = new("breaks into smaller attackers when destroyed", "被击破后会分裂出小敌人"),
        ["enemy.lance.name"] = new("Lance", "长枪手"),
        ["enemy.lance.role"] = new("fires fast heavy shots from long range", "远距离发射高速重弹"),
        ["enemy.mine.name"] = new("Mine", "星雷"),
        ["enemy.mine.role"] = new("drifts slowly and bursts into radial bullets", "缓慢漂移，爆出放射弹幕"),
        ["enemy.shard.name"] = new("Shard", "碎片"),
        ["enemy.shard.role"] = new("fast harasser with quick paired shots", "高速骚扰，连续发射小弹"),
        ["enemy.warden.name"] = new("Warden", "守望者"),
        ["enemy.warden.role"] = new("summons support units while suppressing space", "召唤援兵并压制空间"),
        ["enemy.drifter.name"] = new("Drifter", "弧行者"),
        ["enemy.drifter.role"] = new("curves around you and shoots from angles", "绕弧移动，从侧面发射弹幕"),
        ["enemy.bulwark.name"] = new("Bulwark", "壁垒"),
        ["enemy.bulwark.role"] = new("slow armored target that anchors the wave", "慢速高血量，是这一波的核心目标"),
        ["enemy.siren.name"] = new("Siren", "鸣标"),
        ["enemy.siren.role"] = new("cycles its core and releases slow red rings", "切换核心状态，释放慢速红色环弹"),
        ["enemy.harrier.name"] = new("Harrier", "掠袭者"),
        ["enemy.harrier.role"] = new("dives quickly, then fires short bursts", "高速突进，停顿后短促连射"),
        ["hud.hull"] = new("HULL", "船体"),
        ["hud.energy"] = new("ENERGY", "能量"),
        ["hud.dash"] = new("DASH", "冲刺"),
        ["hud.settings"] = new("SETTINGS", "设置"),
        ["hud.wave"] = new("WAVE {0}/40", "波次 {0}/40"),
        ["hud.sector"] = new("SECTOR {0}/5", "章节 {0}/5"),
        ["hud.wave.label"] = new("WAVE", "波次"),
        ["hud.sector.label"] = new("SECTOR", "章节"),
        ["hud.score"] = new("SCORE {0:000000}", "分数 {0:000000}"),
        ["hud.score.label"] = new("SCORE", "分数"),
        ["hud.combo.label"] = new("COMBO", "连击"),
        ["hud.build"] = new("BUILD", "构筑"),
        ["hud.objectives"] = new("EXPEDITION GOALS", "远征目标"),
        ["hud.cyan_resonance"] = new("CRUISE MODE", "巡航态"),
        ["hud.gold_resonance"] = new("ASSAULT MODE", "突击态"),
        ["hud.resonance_ready"] = new("SHIFT READY", "切换就绪"),
        ["hud.resonance_cooldown"] = new("SHIFT {0:0.0}s", "切换 {0:0.0}秒"),
        ["hud.cruise_charge"] = new("CHARGE", "蓄势"),
        ["hud.assault_window"] = new("BURST {0:0.0}s", "爆发 {0:0.0}秒"),
        ["hud.controls"] = new("WASD MOVE  MOUSE AIM  AUTO FIRE  RMB/SHIFT DASH  SPACE STANCE SHIFT  F/E ULTIMATE  L LANGUAGE", "WASD移动  鼠标瞄准  自动射击  右键/Shift冲刺  空格切换姿态  F/E大招  L语言"),
        ["title.astra"] = new("ASTRA", "星穹"),
        ["title.fracture"] = new("FRACTURE", "裂隙"),
        ["title.subtitle"] = new("a stance-shifting arcade roguelite built in Godot C#", "Godot C# 制作的姿态切换弹幕 Roguelite"),
        ["title.body"] = new("Red fire is always danger. Cruise through pressure, build charge, then shift into Assault during enemy overheat windows.", "红色弹幕永远危险。巡航态穿过压力并蓄势，敌人过热时切入突击态爆发。"),
        ["pilot.astra.name"] = new("Astra", "星棱"),
        ["pilot.astra.body"] = new("Balanced prism bolts. Easy to read, easy to grow.", "均衡棱镜弹。稳定、清晰，适合熟悉流程。"),
        ["pilot.astra.weapon"] = new("Prism Bolts", "棱镜连射"),
        ["pilot.vesper.name"] = new("Vesper", "暮轨"),
        ["pilot.vesper.body"] = new("Slow heavy rail shots pierce targets and reward aim.", "低频重炮，穿透敌人，适合精准瞄准。"),
        ["pilot.vesper.weapon"] = new("Rail Lance", "轨道长枪"),
        ["pilot.kairo.name"] = new("Kairo", "环序"),
        ["pilot.kairo.body"] = new("Starts with drones and a softer primary pulse.", "开局拥有无人机，主武器较弱但自动压制强。"),
        ["pilot.kairo.weapon"] = new("Drone Net", "无人机网"),
        ["pilot.sol.name"] = new("Sol", "日冕"),
        ["pilot.sol.body"] = new("Wide scatter fire and deeper energy reserves.", "宽角散射，能量更厚，适合近中距离压场。"),
        ["pilot.sol.weapon"] = new("Corona Scatter", "日冕散射"),
        ["pilot.unlock.free"] = new("Unlocked", "已解锁"),
        ["pilot.unlock.wave8"] = new("Reach wave 8", "到达第 8 波解锁"),
        ["pilot.unlock.wave16"] = new("Reach wave 16", "到达第 16 波解锁"),
        ["pilot.unlock.wave24"] = new("Reach wave 24", "到达第 24 波解锁"),
        ["title.start"] = new("PRESS ENTER / CLICK", "按 ENTER / 点击开始"),
        ["title.won_once"] = new("Choir Core fractured once. It remembers.", "合唱核心曾被击碎。它记得你。"),
        ["upgrade.title"] = new("CHOOSE A NEW RESONANCE", "选择一个升级"),
        ["upgrade.hint"] = new("1 / 2 / 3 / 4 or click a card. R rerolls once.", "按 1 / 2 / 3 / 4 或点击卡牌。R 可重抽一次。"),
        ["choice.instant"] = new("Instant", "立即生效"),
        ["choice.tactic"] = new("Tactic", "战术"),
        ["choice.risk"] = new("Risk", "风险"),
        ["choice.contract"] = new("Contract", "契约"),
        ["choice.meta"] = new("Map", "路线"),
        ["end.victory.title"] = new("CORE FRACTURED", "核心已碎裂"),
        ["end.defeat.title"] = new("SIGNAL LOST", "信号丢失"),
        ["end.score"] = new("FINAL SCORE {0:000000}", "最终分数 {0:000000}"),
        ["end.victory.body"] = new("The starfield exhales. Your pattern survives.", "你击败了全部 Boss，完成了这次航行。"),
        ["end.defeat.body"] = new("The Choir rewinds the arena. Tune again.", "这次失败了。调整升级选择，再试一次。"),
        ["end.restart"] = new("ENTER / CLICK TO RESTART", "ENTER / 点击重新开始"),
        ["end.reward"] = new("STAR DUST +{0}   REACHED WAVE {1}/40", "获得星尘 +{0}   到达波次 {1}/40"),
        ["end.objective_bonus"] = new("GOAL BONUS +{0}", "目标奖励 +{0}"),
        ["end.meta_hint"] = new("Press Esc for title, then spend Star Dust in Permanent Upgrades.", "按 Esc 回主界面，然后在永久升级中使用星尘。"),
        ["objective.complete"] = new("GOAL COMPLETE +{0} DUST", "目标完成 +{0} 星尘"),
        ["objective.clean_wave"] = new("CLEAN WAVE +ENERGY", "无伤清波 +能量"),
        ["meta.title"] = new("STAR VAULT", "星尘工坊"),
        ["meta.subtitle"] = new("Permanent upgrades are a long route across many expeditions.", "永久升级需要多次远征逐步推进。"),
        ["meta.dust"] = new("Star Dust", "星尘"),
        ["meta.wallet"] = new("STAR DUST {0}", "星尘 {0}"),
        ["meta.best"] = new("BEST WAVE {0}/40   BEST SCORE {1:000000}   RUNS {2}", "最高波次 {0}/40   最高分 {1:000000}   出航 {2}"),
        ["meta.open_hint"] = new("Press U or click Permanent Upgrades.", "按 U 或点击“永久升级”。"),
        ["meta.buy_hint"] = new("Click any node to buy. Keys 1-9 buy the first nine nodes. Esc returns to title.", "点击任意节点购买。按 1-9 可购买前九个节点。Esc 返回主界面。"),
        ["meta.back"] = new("BACK", "返回"),
        ["meta.cost"] = new("COST {0}", "花费 {0}"),
        ["meta.max"] = new("MAX", "已满"),
        ["meta.rank"] = new("Rank {0}/{1}", "等级 {0}/{1}"),
        ["meta.bought"] = new("UPGRADED", "升级完成"),
        ["meta.short"] = new("NEED {0} MORE", "还差 {0}"),
        ["settings.title"] = new("SETTINGS", "设置"),
        ["settings.subtitle"] = new("Pause the run, adjust language, or read the combat guide.", "暂停游戏，切换语言，或查看玩法说明。"),
        ["settings.guide"] = new("GAME GUIDE", "游戏说明"),
        ["settings.main_menu"] = new("MAIN MENU", "回到主界面"),
        ["settings.delete_save"] = new("DELETE SAVE", "删除存档"),
        ["settings.delete_confirm"] = new("CLICK AGAIN TO DELETE", "再次点击确认删除"),
        ["settings.delete_warning"] = new("Clears Star Dust, permanent upgrades, unlocks, and records.", "清空星尘、永久升级、角色解锁和纪录。"),
        ["settings.delete_notice"] = new("SAVE DATA CLEARED", "存档已删除"),
        ["settings.resume"] = new("RESUME", "继续游戏"),
        ["settings.back"] = new("BACK", "返回"),
        ["ultimate.astra"] = new("PRISM NOVA", "棱镜星爆"),
        ["ultimate.vesper"] = new("RAIL JUDGMENT", "轨道裁决"),
        ["ultimate.kairo"] = new("SWARM OVERRIDE", "蜂群覆写"),
        ["ultimate.sol"] = new("CORONA FLARE", "日冕耀斑"),
        ["guide.title"] = new("GAME GUIDE", "游戏说明"),
        ["guide.subtitle"] = new("Core rules are collected here so the battle HUD can stay clean.", "核心规则集中放在这里，战斗界面保持清爽。"),
        ["meta.hull.title"] = new("Hull Plating", "船体加固"),
        ["meta.hull.body"] = new("Start every run with more maximum hull.", "每级提高开局最大生命。"),
        ["meta.energy.title"] = new("Reactor Seed", "能量核心"),
        ["meta.energy.body"] = new("Start with more energy and a larger energy cap.", "提高开局能量和能量上限。"),
        ["meta.weapon.title"] = new("Focus Lens", "聚焦透镜"),
        ["meta.weapon.body"] = new("Weapon damage starts higher before any draft choices.", "每局开局就提高武器伤害。"),
        ["meta.engine.title"] = new("Drift Engine", "推进引擎"),
        ["meta.engine.body"] = new("Move faster and dash harder from wave one.", "开局移动更快，冲刺更强。"),
        ["meta.salvage.title"] = new("Salvage Rig", "回收装置"),
        ["meta.salvage.body"] = new("Earn more Star Dust and pull pickups from farther away.", "每局星尘更多，拾取范围更大。"),
        ["meta.chart.title"] = new("Starter Chart", "开局星图"),
        ["meta.chart.body"] = new("Gain extra rerolls on every upgrade screen.", "每次升级选择时获得更多重抽。"),
        ["meta.repair.title"] = new("Repair Protocol", "维修协议"),
        ["meta.repair.body"] = new("Repair drops become more common and calm moments slowly restore hull.", "修复掉落更多，脱战后会缓慢回血。"),
        ["meta.aegis.title"] = new("Aegis Matrix", "护盾矩阵"),
        ["meta.aegis.body"] = new("Incoming damage is reduced before temporary shields or repairs.", "降低受到的伤害，让后期容错更高。"),
        ["meta.nova.title"] = new("Nova Catalyst", "大招催化"),
        ["meta.nova.body"] = new("Ultimates cost less energy and each run starts with more charge.", "大招耗能降低，开局能量更多。"),
        ["meta.drone.title"] = new("Drone Dock", "无人机坞"),
        ["meta.drone.body"] = new("Begin runs with support drones. Kairo turns this into a stronger swarm.", "开局获得支援无人机。环序会形成更强蜂群。"),
        ["meta.tuner.title"] = new("Stance Tuner", "姿态调谐"),
        ["meta.tuner.body"] = new("Cruise charge and overheat-window hits return more energy.", "巡航蓄势和过热窗口命中会获得更多能量。"),
        ["meta.survey.title"] = new("Deep Survey", "深空测绘"),
        ["meta.survey.body"] = new("Earn a modest Star Dust bonus and read the opening waves more safely.", "略微提高星尘收益，并让开局节奏更稳。"),
        ["rank"] = new("Rank {0}", "等级 {0}"),
        ["upgrade.prism.title"] = new("Prism Array", "多重射击"),
        ["upgrade.prism.body"] = new("Adds another parallel beam. Assault windows bloom harder.", "多发一颗子弹。突击窗口爆发更强。"),
        ["upgrade.rail.title"] = new("Rail Heart", "强力核心"),
        ["upgrade.rail.body"] = new("Raises weapon damage and trims charge delay. Simple, brutal, beautiful.", "提高子弹伤害，并略微提高射速。"),
        ["upgrade.coolant.title"] = new("Coolant Lattice", "冷却装置"),
        ["upgrade.coolant.body"] = new("Faster fire cycle and a larger energy vessel for nova turns.", "射击更快，能量上限更高。"),
        ["upgrade.kinetic.title"] = new("Kinetic Bloom", "机动强化"),
        ["upgrade.kinetic.body"] = new("Dash harder, drift faster, and carve bullets out of the air.", "移动和冲刺更快。"),
        ["upgrade.gravity.title"] = new("Gravity Well", "拾取范围"),
        ["upgrade.gravity.body"] = new("Pull pickups from farther away and thicken enemy time.", "拾取物吸得更远，敌人略微变慢。"),
        ["upgrade.vital.title"] = new("Vital Shell", "生命护盾"),
        ["upgrade.vital.body"] = new("Increases hull integrity and repairs a large chunk immediately.", "提高最大生命，并立即回复生命。"),
        ["upgrade.leech.title"] = new("Resonance Leech", "修复掉落"),
        ["upgrade.leech.body"] = new("Kills can seed repairs. Cruise play becomes a survival engine.", "击败敌人有更高概率掉落修复。"),
        ["upgrade.wisp.title"] = new("Moon Wisp", "自动浮游炮"),
        ["upgrade.wisp.body"] = new("Adds an orbiting shard that searches nearby targets and fires.", "增加一个自动攻击附近敌人的环绕碎片。"),
        ["upgrade.rift.title"] = new("Rift Needle", "穿透弹"),
        ["upgrade.rift.body"] = new("Shots become thin piercing lances with more velocity and bite.", "子弹变成速度更快的穿透弹。"),
        ["upgrade.mirror.title"] = new("Mirror Skin", "减伤装甲"),
        ["upgrade.mirror.body"] = new("Reduces incoming damage and flashes absorbed force outward.", "减少受到的伤害。"),
        ["upgrade.nova.title"] = new("Ultimate Capacitor", "大招强化"),
        ["upgrade.nova.body"] = new("Ultimate skills cost less energy and your vessel holds more charge.", "大招消耗更少，能量上限更高。"),
        ["upgrade.storm.title"] = new("Stance Storm", "姿态反击"),
        ["upgrade.storm.body"] = new("Switching stance releases counterfire and shortens stance cooldown.", "切换姿态时向四周发射子弹，并缩短姿态冷却。"),
        ["upgrade.comet.title"] = new("Comet Trail", "冲刺强化"),
        ["upgrade.comet.body"] = new("Dash impact damage rises and the dash clears a wider lane.", "冲刺伤害更高，清除更大范围的弹幕。"),
        ["upgrade.aegis.title"] = new("Aegis Bloom", "自动回血"),
        ["upgrade.aegis.body"] = new("Slowly regenerates hull while you avoid damage.", "一段时间不受伤会缓慢回血。"),
        ["upgrade.echo.title"] = new("Quantum Echo", "额外射击"),
        ["upgrade.echo.body"] = new("Shots can echo into a second ghost lance.", "射击时有概率额外发射一发穿透弹。"),
        ["upgrade.solar.title"] = new("Overheat Thesis", "破绽专精"),
        ["upgrade.solar.body"] = new("Assault hits scale harder against overheated elites and cores.", "突击命中过热精英和 Boss 时伤害更高。"),
        ["upgrade.repair.title"] = new("Emergency Repair", "紧急维修"),
        ["upgrade.repair.body"] = new("Repair hull immediately and gain a little max hull.", "立即回复生命，并少量提高最大生命。"),
        ["upgrade.overdrive.title"] = new("One-Wave Overdrive", "单波过载"),
        ["upgrade.overdrive.body"] = new("Next wave: much higher damage. Also gain energy now.", "下一波伤害大幅提高，并立即获得能量。"),
        ["upgrade.glass.title"] = new("Glass Cannon", "玻璃大炮"),
        ["upgrade.glass.body"] = new("Permanent damage up, but max hull goes down.", "永久提高伤害，但降低最大生命。"),
        ["upgrade.bounty.title"] = new("Bounty Contract", "赏金契约"),
        ["upgrade.bounty.body"] = new("Next wave has more enemies, but they are worth more score and drops.", "下一波敌人更多，但分数和掉落更多。"),
        ["upgrade.transmute.title"] = new("Bullet Transmute", "弹幕转化"),
        ["upgrade.transmute.body"] = new("Clear enemy bullets now and convert them into energy.", "立刻清除敌方子弹，并转化为能量。"),
        ["upgrade.map.title"] = new("Harmonic Map", "升级地图"),
        ["upgrade.map.body"] = new("Gain one extra reroll on future upgrade screens.", "之后的升级界面多一次重抽机会。"),
        ["upgrade.astra.refraction.title"] = new("Refraction Lattice", "折射阵列"),
        ["upgrade.astra.refraction.body"] = new("Astra gains extra prism lanes without losing clarity.", "星棱增加额外弹道，保持稳定火力。"),
        ["upgrade.astra.wake.title"] = new("Prism Wake", "棱光余波"),
        ["upgrade.astra.wake.body"] = new("Prism shots hit harder and cycle a little faster.", "棱镜弹伤害提高，射击节奏略快。"),
        ["upgrade.vesper.charge.title"] = new("Capacitor Spine", "蓄能脊柱"),
        ["upgrade.vesper.charge.body"] = new("Rail lance damage rises and charge time shrinks.", "轨道长枪伤害提高，蓄能时间缩短。"),
        ["upgrade.vesper.fork.title"] = new("Split Rail", "分裂轨道"),
        ["upgrade.vesper.fork.body"] = new("Rail shots add narrow side lances.", "轨道炮追加两侧副枪线。"),
        ["upgrade.kairo.bay.title"] = new("Drone Bay", "无人机舱"),
        ["upgrade.kairo.bay.body"] = new("Kairo launches more orbiting drones.", "环序增加更多环绕无人机。"),
        ["upgrade.kairo.sync.title"] = new("Swarm Sync", "蜂群同步"),
        ["upgrade.kairo.sync.body"] = new("Drones and pulses fire faster and hit harder.", "无人机和脉冲弹射速、伤害提高。"),
        ["upgrade.sol.bloom.title"] = new("Corona Bloom", "日冕绽放"),
        ["upgrade.sol.bloom.body"] = new("Sol scatter fire gains more rays and wider control.", "日冕散射弹数量增加，压制范围更大。"),
        ["upgrade.sol.forge.title"] = new("Solar Forge", "太阳熔炉"),
        ["upgrade.sol.forge.body"] = new("More energy, cheaper ultimate skills, and stronger scatter impact.", "能量更多，大招更便宜，散射伤害提高。"),
        ["upgrade.astra.nova.title"] = new("Nova Bloom", "星爆棱镜"),
        ["upgrade.astra.nova.body"] = new("Prism Nova clears a larger field and launches more blades.", "棱镜星爆范围更大，并发射更多棱镜弹。"),
        ["upgrade.astra.twin.title"] = new("Twin Refraction", "双相折射"),
        ["upgrade.astra.twin.body"] = new("Prism Nova adds an inner echo ring with stronger pierce.", "棱镜星爆追加内圈回响，穿透更强。"),
        ["upgrade.vesper.judgment.title"] = new("Judgment Coil", "裁决线圈"),
        ["upgrade.vesper.judgment.body"] = new("Rail Judgment becomes wider, cheaper, and more lethal.", "轨道裁决更宽、更便宜，伤害更高。"),
        ["upgrade.vesper.sever.title"] = new("Sever Line", "裂轨余震"),
        ["upgrade.vesper.sever.body"] = new("Rail Judgment creates parallel aftershock beams.", "轨道裁决会生成平行余震光束。"),
        ["upgrade.kairo.override.title"] = new("Override Matrix", "覆写矩阵"),
        ["upgrade.kairo.override.body"] = new("Swarm Override launches more drones and stronger shots.", "蜂群覆写展开更多无人机，弹幕更强。"),
        ["upgrade.kairo.relay.title"] = new("Relay Protocol", "接力协议"),
        ["upgrade.kairo.relay.body"] = new("Swarm Override fires extra volleys and improves drone pierce.", "蜂群覆写增加齐射轮次，并提高无人机穿透。"),
        ["upgrade.sol.flare.title"] = new("Flare Core", "耀斑核心"),
        ["upgrade.sol.flare.body"] = new("Corona Flare expands farther and burns harder.", "日冕耀斑范围更大，伤害更高。"),
        ["upgrade.sol.mantle.title"] = new("Radiant Mantle", "光冕护层"),
        ["upgrade.sol.mantle.body"] = new("Corona Flare restores more hull and protects you longer.", "日冕耀斑回复更多生命，并提供更久保护。"),
        ["upgrade.unknown.title"] = new("Unknown", "未知升级"),
        ["upgrade.unknown.body"] = new("Mystery signal.", "未知效果。"),
    };

    private enum GameMode
    {
        Title,
        Meta,
        Playing,
        Upgrade,
        GameOver,
        Victory,
        Settings,
        Guide,
    }

    private enum GameLanguage
    {
        English,
        Chinese,
    }

    private enum PilotKind
    {
        Astra,
        Vesper,
        Kairo,
        Sol,
    }

    private enum MetaUpgradeId
    {
        HullPlating,
        ReactorSeed,
        FocusLens,
        DriftEngine,
        SalvageRig,
        StarterChart,
        RepairProtocol,
        AegisMatrix,
        NovaCatalyst,
        DroneDock,
        PolarityTuner,
        DeepSurvey,
    }

    private readonly struct LocalizedText
    {
        public LocalizedText(string english, string chinese)
        {
            English = english;
            Chinese = chinese;
        }

        public readonly string English;
        public readonly string Chinese;
    }

    private readonly struct SectorInfo
    {
        public SectorInfo(string nameKey, string traitKey, Color accent)
        {
            NameKey = nameKey;
            TraitKey = traitKey;
            Accent = accent;
        }

        public readonly string NameKey;
        public readonly string TraitKey;
        public readonly Color Accent;
    }

    private readonly struct MetaUpgradeDef
    {
        public MetaUpgradeDef(MetaUpgradeId id, string titleKey, string bodyKey, Color accent, int maxRank, int baseCost, int stepCost)
        {
            Id = id;
            TitleKey = titleKey;
            BodyKey = bodyKey;
            Accent = accent;
            MaxRank = maxRank;
            BaseCost = baseCost;
            StepCost = stepCost;
        }

        public readonly MetaUpgradeId Id;
        public readonly string TitleKey;
        public readonly string BodyKey;
        public readonly Color Accent;
        public readonly int MaxRank;
        public readonly int BaseCost;
        public readonly int StepCost;
    }

    private enum EnemyKind
    {
        Chaser,
        Weaver,
        Turret,
        Splitter,
        Lance,
        Mine,
        Shard,
        Warden,
        Drifter,
        Bulwark,
        Siren,
        Harrier,
        Boss,
    }

    private enum RunObjectiveKind
    {
        ReachWave,
        PerfectWaves,
        DefeatEnemies,
        AbsorbBullets,
        CollectPickups,
        BestCombo,
        DefeatBosses,
    }

    private enum PickupKind
    {
        Dust,
        Energy,
        Repair,
    }

    private enum UpgradeId
    {
        PrismArray,
        RailHeart,
        CoolantLattice,
        KineticBloom,
        GravityWell,
        VitalShell,
        ResonanceLeech,
        MoonWisp,
        RiftNeedle,
        MirrorSkin,
        NovaCapacitor,
        PolarityStorm,
        CometTrail,
        AegisBloom,
        QuantumEcho,
        SolarThesis,
        EmergencyRepair,
        OneWaveOverdrive,
        GlassCannon,
        BountyContract,
        BulletTransmute,
        HarmonicMap,
        AstraRefraction,
        AstraPrismWake,
        VesperCapacitor,
        VesperSplitRail,
        KairoDroneBay,
        KairoSwarmSync,
        SolCoronaBloom,
        SolSolarForge,
        AstraNovaBloom,
        AstraTwinRefraction,
        VesperJudgmentCoil,
        VesperSeverLine,
        KairoOverrideMatrix,
        KairoRelayProtocol,
        SolFlareCore,
        SolRadiantMantle,
    }

    private sealed class Enemy
    {
        public EnemyKind Kind;
        public Vector2 Pos;
        public Vector2 Vel;
        public float Radius;
        public float Hp;
        public float MaxHp;
        public float Cooldown;
        public float Overheat;
        public float OverheatMax;
        public float Phase;
        public float SpawnPulse = 1.0f;
        public float ContactTimer;
        public int Polarity;
        public int Value;
        public int SplitDepth;
        public bool Elite;
        public float Armor;
    }

    private sealed class Shot
    {
        public Vector2 Pos;
        public Vector2 Prev;
        public Vector2 Vel;
        public float Radius;
        public float Damage;
        public float Life;
        public float MaxLife;
        public Vector2 Trail0;
        public Vector2 Trail1;
        public Vector2 Trail2;
        public Vector2 Trail3;
        public int TrailCount;
        public int Polarity;
        public bool FromPlayer;
        public int Pierce;
        public bool Rift;
        public bool Grazed;
    }

    private sealed class Pickup
    {
        public PickupKind Kind;
        public Vector2 Pos;
        public Vector2 Vel;
        public float Radius;
        public float Life;
    }

    private sealed class Particle
    {
        public Vector2 Pos;
        public Vector2 Vel;
        public Color Color;
        public float Size;
        public float Life;
        public float MaxLife;
        public float Spin;
    }

    private sealed class DamageText
    {
        public Vector2 Pos;
        public string Text = string.Empty;
        public Color Color;
        public float Life;
        public float MaxLife;
        public float Size;
    }

    private sealed class HazardLine
    {
        public Vector2 A;
        public Vector2 B;
        public Color Color;
        public float Life;
        public float MaxLife;
        public float Warmup;
        public float Width;
        public float Damage;
        public int Polarity;
    }

    private sealed class Star
    {
        public Vector2 Pos;
        public float Radius;
        public float Twinkle;
        public float Depth;
        public Color Color;
    }

    private sealed class Nebula
    {
        public Vector2 Pos;
        public float Radius;
        public Color Color;
        public float Drift;
    }

    private sealed class UpgradeCard
    {
        public UpgradeId Id;
        public string Title = string.Empty;
        public string Tag = string.Empty;
        public string Body = string.Empty;
        public Color Accent;
        public Rect2 Rect;
    }

    private readonly struct RichTextSegment
    {
        public RichTextSegment(string text, bool highlight)
        {
            Text = text;
            Highlight = highlight;
        }

        public string Text { get; }
        public bool Highlight { get; }
    }

    private sealed class RunObjective
    {
        public RunObjectiveKind Kind;
        public int Target;
        public int Progress;
        public int RewardDust;
        public bool Completed;
    }

    private sealed class PendingSpawn
    {
        public EnemyKind Kind;
        public int Polarity;
        public float RewardBoost;
    }

    private sealed class SfxVoice
    {
        public float Age;
        public float Life;
        public float Frequency;
        public float Sweep;
        public float Volume;
        public float Noise;
        public int Wave;
    }

    private readonly RandomNumberGenerator _rng = new();
    private readonly List<Enemy> _enemies = new();
    private readonly List<Shot> _shots = new();
    private readonly List<Pickup> _pickups = new();
    private readonly List<Particle> _particles = new();
    private readonly List<DamageText> _damageTexts = new();
    private readonly List<HazardLine> _hazards = new();
    private readonly List<Star> _stars = new();
    private readonly List<Nebula> _nebulas = new();
    private readonly List<UpgradeCard> _upgradeChoices = new();
    private readonly List<UpgradeId> _upgradeOrder = new();
    private readonly List<RunObjective> _runObjectives = new();
    private readonly List<SfxVoice> _voices = new();
    private readonly Vector2[] _playerTrail = new Vector2[PlayerTrailCapacity];
    private readonly Queue<PendingSpawn> _pendingSpawns = new();
    private readonly Dictionary<UpgradeId, int> _upgradeRanks = new();
    private readonly Dictionary<MetaUpgradeId, int> _metaRanks = new();
    private readonly Stack<Enemy> _enemyPool = new();
    private readonly Stack<Shot> _shotPool = new();
    private readonly Stack<Pickup> _pickupPool = new();
    private readonly Stack<Particle> _particlePool = new();
    private readonly Stack<DamageText> _damageTextPool = new();
    private int _activeEnemyBullets;
    private float _visualPressure;

    private GameMode _mode = GameMode.Title;
    private GameMode _settingsReturnMode = GameMode.Title;
    private PilotKind _selectedPilot = PilotKind.Astra;
    private PilotKind _runPilot = PilotKind.Astra;
    private Vector2 _playerPos = ScreenCenter;
    private Vector2 _playerVel = Vector2.Zero;
    private Vector2 _aimDir = Vector2.Right;
    private float _playerHp = 120.0f;
    private float _playerMaxHp = 120.0f;
    private float _energy = 35.0f;
    private float _maxEnergy = 100.0f;
    private float _hudHullValue = 1.0f;
    private float _hudHullTrail = 1.0f;
    private float _hudEnergyValue = 0.35f;
    private float _hudEnergyTrail = 0.35f;
    private float _hudDashValue = 1.0f;
    private float _hudDashTrail = 1.0f;
    private float _fireTimer;
    private float _dashTimer;
    private float _dashCooldown;
    private float _invulnTimer;
    private float _polarityCooldown;
    private float _polarityCooldownMax = PolaritySwitchCooldownBase;
    private float _polarityDenyTextCooldown;
    private int _playerPolarity;
    private int _wave;
    private int _score;
    private int _combo;
    private float _comboTimer;
    private float _waveClearTimer;
    private float _time;
    private float _shake;
    private float _flash;
    private float _slowMo = 1.0f;
    private float _playerTrailTimer;
    private float _spawnDirector;
    private float _waveSpawnTimer;
    private float _bossPatternTimer;
    private float _sectorHazardTimer;
    private float _deleteSaveConfirmTimer;
    private float _deleteSaveNoticeTimer;
    private float _timeSinceHit;
    private float _musicClock;
    private float _noiseSeed;
    private float _absorbTextCooldown;
    private float _counterTextCooldown;
    private float _polarityTipTimer;
    private float _cruiseCharge;
    private float _assaultBurstTimer;
    private float _assaultPower = 1.0f;
    private bool _wonOnce;
    private bool _runRewardGranted;
    private GameLanguage _language = GameLanguage.English;
    private Font? _uiFont;

    private int _starDust;
    private int _lifetimeDust;
    private int _bestScore;
    private int _bestWave;
    private int _runsCompleted;
    private int _careerKills;
    private int _careerPickups;
    private int _careerBossKills;
    private int _careerPerfectWaves;
    private int _lastDustEarned;
    private int _lastRunWave;
    private int _lastObjectiveBonusDust;
    private int _runObjectiveBonusDust;
    private int _runWavesCleared;
    private int _runPerfectWaves;
    private int _runKills;
    private int _runAbsorbs;
    private int _runPickups;
    private int _runBossKills;
    private int _runBestCombo;
    private bool _waveTookDamage;
    private int _playerTrailCount;

    private int _multiShot = 1;
    private int _orbiters;
    private float _orbiterFireTimer;
    private float _damageMultiplier = 1.0f;
    private float _fireInterval = 0.22f;
    private float _playerSpeed = 420.0f;
    private float _dashPower = 1120.0f;
    private float _pickupMagnet = 155.0f;
    private float _enemySlow = 1.0f;
    private float _leechChance;
    private float _mirrorReduction = 1.0f;
    private float _absorbEfficiency = 1.0f;
    private float _novaCost = 70.0f;
    private float _critMultiplier = 1.55f;
    private float _dashDamage = 70.0f;
    private float _aegisRegen;
    private float _echoChance;
    private float _nextWaveDamageBoost = 1.0f;
    private float _nextWaveRewardBoost = 1.0f;
    private int _polarityStorm;
    private int _nextWaveBonusEnemies;
    private int _baseRerolls = 1;
    private int _rerollsRemaining;
    private int _astraRefraction;
    private int _astraWake;
    private int _vesperCharge;
    private int _vesperFork;
    private int _kairoDroneBay;
    private int _kairoSync;
    private int _solBloom;
    private int _solForge;
    private int _astraNovaBloom;
    private int _astraTwinRefraction;
    private int _vesperJudgmentCoil;
    private int _vesperSeverLine;
    private int _kairoOverrideMatrix;
    private int _kairoRelayProtocol;
    private int _solFlareCore;
    private int _solRadiantMantle;
    private bool _riftNeedle;

    private bool _lastStart;
    private bool _lastToggle;
    private bool _lastNova;
    private bool _lastDash;
    private bool _lastClick;
    private bool _lastRestart;
    private bool _lastOne;
    private bool _lastTwo;
    private bool _lastThree;
    private bool _lastFour;
    private bool _lastFive;
    private bool _lastSix;
    private bool _lastSeven;
    private bool _lastEight;
    private bool _lastNine;
    private bool _lastReroll;
    private bool _lastLanguage;
    private bool _lastMeta;
    private bool _lastBack;

    private AudioStreamPlayer? _musicPlayer;
    private AudioStreamGeneratorPlayback? _musicPlayback;

    public override void _Ready()
    {
        _rng.Randomize();
        _noiseSeed = _rng.RandfRange(-1000.0f, 1000.0f);
        _language = DetectLanguage();
        _uiFont = new SystemFont
        {
            FontNames = new[] { "Microsoft YaHei UI", "Microsoft YaHei", "Noto Sans CJK SC", "Noto Sans SC", "Arial Unicode MS", "Segoe UI" },
            MultichannelSignedDistanceField = true,
        };
        GenerateBackdrop();
        SetupAudio();
        LoadMetaProgress();
        ResetTitle();
        DisplayServer.WindowSetTitle("穿越星际 - 1920x1080 Godot C#");
        SetProcess(true);
    }

    public override void _ExitTree()
    {
        _voices.Clear();
        _musicPlayback = null;
    }

    public override void _Process(double delta)
    {
        float dt = Mathf.Min((float)delta, 0.033f);
        _time += dt;
        FillAudio();
        UpdateLanguageToggle();

        switch (_mode)
        {
            case GameMode.Title:
                UpdateTitle(dt);
                break;
            case GameMode.Meta:
                UpdateMeta(dt);
                break;
            case GameMode.Playing:
                UpdatePlaying(dt);
                break;
            case GameMode.Upgrade:
                UpdateUpgrade(dt);
                break;
            case GameMode.GameOver:
            case GameMode.Victory:
                UpdateEndScreen(dt);
                break;
            case GameMode.Settings:
                UpdateSettings(dt);
                break;
            case GameMode.Guide:
                UpdateGuide(dt);
                break;
        }

        UpdateHudBarEasing(dt);
        GameMode trailMode = (_mode == GameMode.Settings || _mode == GameMode.Guide) ? _settingsReturnMode : _mode;
        if (IsRunViewMode(trailMode))
        {
            UpdatePlayerTrail(dt);
        }
        else
        {
            ResetPlayerTrail(_playerPos);
        }
        _shake = Approach(_shake, 0.0f, dt * 9.0f);
        _flash = Approach(_flash, 0.0f, dt * 2.8f);
        _visualPressure = CalculateVisualPressure();
        QueueRedraw();
        CaptureButtons();
    }

    public override void _Draw()
    {
        GameMode visibleMode = (_mode == GameMode.Settings || _mode == GameMode.Guide) ? _settingsReturnMode : _mode;
        bool titleView = visibleMode == GameMode.Title;

        if (titleView)
        {
            DrawTitleBackdrop();
        }
        else
        {
            DrawBackdrop();
            DrawArenaFrame();

            foreach (HazardLine hazard in _hazards)
            {
                DrawHazard(hazard);
            }

            foreach (Particle particle in _particles)
            {
                DrawParticle(particle);
            }

            foreach (Pickup pickup in _pickups)
            {
                DrawPickup(pickup);
            }

            foreach (Shot shot in _shots)
            {
                DrawShot(shot);
            }

            foreach (Enemy enemy in _enemies)
            {
                DrawEnemy(enemy);
            }
        }

        if (IsRunViewMode(visibleMode))
        {
            DrawPlayer();
            DrawHud();
        }

        foreach (DamageText damageText in _damageTexts)
        {
            float t = Mathf.Clamp(damageText.Life / damageText.MaxLife, 0.0f, 1.0f);
            DrawText(damageText.Text, damageText.Pos + ShakeOffset(), (int)damageText.Size, Alpha(damageText.Color, t), HorizontalAlignment.Center, 220.0f, true, 4);
        }

        if (visibleMode == GameMode.Title)
        {
            DrawTitle();
        }
        else if (visibleMode == GameMode.Meta)
        {
            DrawMeta();
        }
        else if (visibleMode == GameMode.Upgrade)
        {
            DrawUpgrade();
        }
        else if (visibleMode == GameMode.GameOver)
        {
            DrawEndScreen(false);
        }
        else if (visibleMode == GameMode.Victory)
        {
            DrawEndScreen(true);
        }

        if (_mode == GameMode.Settings)
        {
            DrawSettings();
        }
        else if (_mode == GameMode.Guide)
        {
            DrawGuide();
        }

        if (_flash > 0.01f)
        {
            DrawRect(new Rect2(Vector2.Zero, new Vector2(ScreenWidth, ScreenHeight)), Alpha(Paper, 0.18f * _flash), true);
        }
    }

    private void ResetTitle()
    {
        _mode = GameMode.Title;
        _settingsReturnMode = GameMode.Title;
        _playerPos = ScreenCenter;
        _playerVel = Vector2.Zero;
        ResetPlayerTrail(_playerPos);
        ClearShots();
        ClearEnemies();
        ClearPickups();
        ClearParticles();
        ClearDamageTexts();
        _hazards.Clear();
        _upgradeChoices.Clear();
        _voices.Clear();
    }

    private Enemy? AddEnemy()
    {
        if (_enemies.Count >= MaxEnemies)
        {
            return null;
        }

        Enemy enemy = _enemyPool.Count > 0 ? _enemyPool.Pop() : new Enemy();
        _enemies.Add(enemy);
        return enemy;
    }

    private Shot? AddShot(bool fromPlayer)
    {
        if (!fromPlayer && ActiveEnemyBulletCount() >= EnemyBulletCap())
        {
            return null;
        }

        if (_shots.Count >= MaxShots && !MakeShotRoom(fromPlayer))
        {
            return null;
        }

        Shot shot = _shotPool.Count > 0 ? _shotPool.Pop() : new Shot();
        shot.FromPlayer = fromPlayer;
        shot.Rift = false;
        shot.Pierce = 0;
        shot.TrailCount = 0;
        shot.Grazed = false;
        _shots.Add(shot);
        if (!fromPlayer)
        {
            _activeEnemyBullets++;
        }
        return shot;
    }

    private bool MakeShotRoom(bool incomingPlayerShot)
    {
        for (int i = 0; i < _shots.Count; i++)
        {
            if (!_shots[i].FromPlayer)
            {
                RemoveShotAt(i);
                return true;
            }
        }

        if (!incomingPlayerShot)
        {
            return false;
        }

        if (_shots.Count > 0)
        {
            RemoveShotAt(0);
            return true;
        }
        return true;
    }

    private Pickup? AddPickup()
    {
        if (_pickups.Count >= MaxPickups)
        {
            RemovePickupAt(0);
        }

        Pickup pickup = _pickupPool.Count > 0 ? _pickupPool.Pop() : new Pickup();
        _pickups.Add(pickup);
        return pickup;
    }

    private Particle AddParticleObject()
    {
        if (_particles.Count >= MaxParticles)
        {
            RemoveParticleAt(0);
        }

        Particle particle = _particlePool.Count > 0 ? _particlePool.Pop() : new Particle();
        _particles.Add(particle);
        return particle;
    }

    private DamageText AddDamageTextObject()
    {
        if (_damageTexts.Count >= MaxDamageTexts)
        {
            RemoveDamageTextAt(0);
        }

        DamageText text = _damageTextPool.Count > 0 ? _damageTextPool.Pop() : new DamageText();
        _damageTexts.Add(text);
        return text;
    }

    private void RemoveShotAt(int index)
    {
        Shot shot = _shots[index];
        if (!shot.FromPlayer)
        {
            _activeEnemyBullets = Math.Max(0, _activeEnemyBullets - 1);
        }
        int last = _shots.Count - 1;
        _shots[index] = _shots[last];
        _shots.RemoveAt(last);
        RecycleShot(shot);
    }

    private void RemovePickupAt(int index)
    {
        Pickup pickup = _pickups[index];
        int last = _pickups.Count - 1;
        _pickups[index] = _pickups[last];
        _pickups.RemoveAt(last);
        RecyclePickup(pickup);
    }

    private void RemoveParticleAt(int index)
    {
        Particle particle = _particles[index];
        int last = _particles.Count - 1;
        _particles[index] = _particles[last];
        _particles.RemoveAt(last);
        RecycleParticle(particle);
    }

    private void RemoveDamageTextAt(int index)
    {
        DamageText text = _damageTexts[index];
        int last = _damageTexts.Count - 1;
        _damageTexts[index] = _damageTexts[last];
        _damageTexts.RemoveAt(last);
        RecycleDamageText(text);
    }

    private bool DetachEnemy(Enemy enemy)
    {
        int index = _enemies.IndexOf(enemy);
        if (index < 0)
        {
            return false;
        }

        int last = _enemies.Count - 1;
        _enemies[index] = _enemies[last];
        _enemies.RemoveAt(last);
        return true;
    }

    private void ClearEnemies()
    {
        _pendingSpawns.Clear();
        _waveSpawnTimer = 0.0f;
        for (int i = 0; i < _enemies.Count; i++)
        {
            RecycleEnemy(_enemies[i]);
        }
        _enemies.Clear();
    }

    private void ClearShots()
    {
        for (int i = 0; i < _shots.Count; i++)
        {
            RecycleShot(_shots[i]);
        }
        _shots.Clear();
        _activeEnemyBullets = 0;
    }

    private void ClearPickups()
    {
        for (int i = 0; i < _pickups.Count; i++)
        {
            RecyclePickup(_pickups[i]);
        }
        _pickups.Clear();
    }

    private void ClearParticles()
    {
        for (int i = 0; i < _particles.Count; i++)
        {
            RecycleParticle(_particles[i]);
        }
        _particles.Clear();
    }

    private void ClearDamageTexts()
    {
        for (int i = 0; i < _damageTexts.Count; i++)
        {
            RecycleDamageText(_damageTexts[i]);
        }
        _damageTexts.Clear();
    }

    private void RecycleEnemy(Enemy enemy)
    {
        if (_enemyPool.Count < MaxPoolSize)
        {
            _enemyPool.Push(enemy);
        }
    }

    private void RecycleShot(Shot shot)
    {
        shot.TrailCount = 0;
        if (_shotPool.Count < MaxPoolSize)
        {
            _shotPool.Push(shot);
        }
    }

    private void ResetPlayerTrail(Vector2 pos)
    {
        for (int i = 0; i < _playerTrail.Length; i++)
        {
            _playerTrail[i] = pos;
        }

        _playerTrailCount = 1;
        _playerTrailTimer = 0.0f;
    }

    private void UpdatePlayerTrail(float dt)
    {
        if (_playerTrailCount <= 0)
        {
            ResetPlayerTrail(_playerPos);
            return;
        }

        _playerTrailTimer -= dt;
        float minDistance = _dashTimer > 0.0f ? 5.0f : 9.0f;
        if (_playerTrailTimer > 0.0f && _playerPos.DistanceSquaredTo(_playerTrail[0]) < minDistance * minDistance)
        {
            return;
        }

        for (int i = Math.Min(_playerTrailCount, PlayerTrailCapacity - 1); i > 0; i--)
        {
            _playerTrail[i] = _playerTrail[i - 1];
        }

        _playerTrail[0] = _playerPos;
        _playerTrailCount = Math.Min(PlayerTrailCapacity, _playerTrailCount + 1);
        _playerTrailTimer = _dashTimer > 0.0f ? 0.008f : 0.022f;
    }

    private static void ResetShotTrail(Shot shot, Vector2 pos)
    {
        shot.Trail0 = pos;
        shot.Trail1 = pos;
        shot.Trail2 = pos;
        shot.Trail3 = pos;
        shot.TrailCount = 1;
    }

    private static void PushShotTrail(Shot shot, Vector2 pos)
    {
        shot.Trail3 = shot.Trail2;
        shot.Trail2 = shot.Trail1;
        shot.Trail1 = shot.Trail0;
        shot.Trail0 = pos;
        shot.TrailCount = Math.Min(ShotTrailCapacity, shot.TrailCount + 1);
    }

    private void RecyclePickup(Pickup pickup)
    {
        if (_pickupPool.Count < MaxPoolSize)
        {
            _pickupPool.Push(pickup);
        }
    }

    private void RecycleParticle(Particle particle)
    {
        if (_particlePool.Count < MaxPoolSize)
        {
            _particlePool.Push(particle);
        }
    }

    private void RecycleDamageText(DamageText text)
    {
        text.Text = string.Empty;
        if (_damageTextPool.Count < MaxPoolSize)
        {
            _damageTextPool.Push(text);
        }
    }

    private void LoadMetaProgress()
    {
        _metaRanks.Clear();
        ConfigFile config = new();
        if (config.Load(MetaSavePath) != Error.Ok)
        {
            return;
        }

        _starDust = Mathf.Max(0, ReadConfigInt(config, "meta", "star_dust", 0));
        _lifetimeDust = Mathf.Max(0, ReadConfigInt(config, "meta", "lifetime_dust", 0));
        _bestScore = Mathf.Max(0, ReadConfigInt(config, "stats", "best_score", 0));
        _bestWave = Mathf.Clamp(ReadConfigInt(config, "stats", "best_wave", 0), 0, TotalWaves);
        _runsCompleted = Mathf.Max(0, ReadConfigInt(config, "stats", "runs_completed", 0));
        _wonOnce = ReadConfigInt(config, "stats", "won_once", 0) > 0;
        _careerKills = Mathf.Max(0, ReadConfigInt(config, "career", "kills", 0));
        _careerPickups = Mathf.Max(0, ReadConfigInt(config, "career", "pickups", 0));
        _careerBossKills = Mathf.Max(0, ReadConfigInt(config, "career", "boss_kills", 0));
        _careerPerfectWaves = Mathf.Max(0, ReadConfigInt(config, "career", "perfect_waves", 0));
        string pilotName = ReadConfigString(config, "settings", "pilot", PilotKind.Astra.ToString());
        if (Enum.TryParse(pilotName, out PilotKind loadedPilot) && IsPilotUnlocked(loadedPilot))
        {
            _selectedPilot = loadedPilot;
        }
        else
        {
            _selectedPilot = PilotKind.Astra;
        }

        foreach (MetaUpgradeDef def in MetaUpgrades)
        {
            int rank = Mathf.Clamp(ReadConfigInt(config, "upgrades", def.Id.ToString(), 0), 0, def.MaxRank);
            if (rank > 0)
            {
                _metaRanks[def.Id] = rank;
            }
        }
    }

    private void SaveMetaProgress()
    {
        ConfigFile config = new();
        config.SetValue("meta", "star_dust", _starDust);
        config.SetValue("meta", "lifetime_dust", _lifetimeDust);
        config.SetValue("stats", "best_score", _bestScore);
        config.SetValue("stats", "best_wave", _bestWave);
        config.SetValue("stats", "runs_completed", _runsCompleted);
        config.SetValue("stats", "won_once", _wonOnce ? 1 : 0);
        config.SetValue("career", "kills", _careerKills);
        config.SetValue("career", "pickups", _careerPickups);
        config.SetValue("career", "boss_kills", _careerBossKills);
        config.SetValue("career", "perfect_waves", _careerPerfectWaves);
        config.SetValue("settings", "pilot", _selectedPilot.ToString());

        foreach (MetaUpgradeDef def in MetaUpgrades)
        {
            config.SetValue("upgrades", def.Id.ToString(), MetaRank(def.Id));
        }

        Error error = config.Save(MetaSavePath);
        if (error != Error.Ok)
        {
            AddText($"SAVE ERROR {error}", ScreenCenter + new Vector2(0.0f, -250.0f), Rose, 24.0f);
        }
    }

    private void DeleteSaveData()
    {
        _metaRanks.Clear();
        _starDust = 0;
        _lifetimeDust = 0;
        _bestScore = 0;
        _bestWave = 0;
        _runsCompleted = 0;
        _careerKills = 0;
        _careerPickups = 0;
        _careerBossKills = 0;
        _careerPerfectWaves = 0;
        _wonOnce = false;
        _selectedPilot = PilotKind.Astra;
        _lastDustEarned = 0;
        _lastRunWave = 0;
        _lastObjectiveBonusDust = 0;
        SaveMetaProgress();

        ResetTitle();
        _mode = GameMode.Settings;
        _settingsReturnMode = GameMode.Title;
        _deleteSaveConfirmTimer = 0.0f;
        _deleteSaveNoticeTimer = 3.2f;
        AddText(T("settings.delete_notice"), ScreenCenter + new Vector2(0.0f, -210.0f), Jade, 26.0f);
    }

    private static int ReadConfigInt(ConfigFile config, string section, string key, int fallback)
    {
        return config.GetValue(section, key, fallback).AsInt32();
    }

    private static string ReadConfigString(ConfigFile config, string section, string key, string fallback)
    {
        return config.GetValue(section, key, fallback).AsString();
    }

    private void AwardMetaProgress(bool victory)
    {
        if (_runRewardGranted)
        {
            return;
        }

        _runRewardGranted = true;
        int reachedWave = Mathf.Clamp(_wave, 0, TotalWaves);
        int sectorBonus = Mathf.Clamp(CurrentSectorIndex(), 0, SectorCount - 1) * 3;
        int baseDust = 10 + Mathf.RoundToInt(reachedWave * 1.35f) + _score / 9000 + sectorBonus + (victory ? 45 : 0);
        float dustBonus = 1.0f + MetaRank(MetaUpgradeId.SalvageRig) * 0.045f + MetaRank(MetaUpgradeId.DeepSurvey) * 0.035f;
        int earned = Mathf.Max(10, Mathf.RoundToInt(baseDust * dustBonus)) + _runObjectiveBonusDust;

        _lastDustEarned = earned;
        _lastRunWave = reachedWave;
        _lastObjectiveBonusDust = _runObjectiveBonusDust;
        _starDust += earned;
        _lifetimeDust += earned;
        _runsCompleted++;
        _bestScore = Math.Max(_bestScore, _score);
        _bestWave = Math.Max(_bestWave, reachedWave);
        _careerKills += _runKills;
        _careerPickups += _runPickups;
        _careerBossKills += _runBossKills;
        _careerPerfectWaves += _runPerfectWaves;
        SaveMetaProgress();
        AddText(Tf("end.reward", earned, reachedWave), ScreenCenter + new Vector2(0.0f, -220.0f), Gold, 28.0f);
    }

    private void StartRun()
    {
        if (!IsPilotUnlocked(_selectedPilot))
        {
            _selectedPilot = PilotKind.Astra;
        }

        _mode = GameMode.Playing;
        _settingsReturnMode = GameMode.Playing;
        _runPilot = _selectedPilot;
        _wave = 0;
        _score = 0;
        _combo = 0;
        _comboTimer = 0.0f;
        _playerPos = ScreenCenter;
        _playerVel = Vector2.Zero;
        ResetPlayerTrail(_playerPos);
        int hullRank = MetaRank(MetaUpgradeId.HullPlating);
        int reactorRank = MetaRank(MetaUpgradeId.ReactorSeed);
        int lensRank = MetaRank(MetaUpgradeId.FocusLens);
        int engineRank = MetaRank(MetaUpgradeId.DriftEngine);
        int salvageRank = MetaRank(MetaUpgradeId.SalvageRig);
        int chartRank = MetaRank(MetaUpgradeId.StarterChart);
        int repairRank = MetaRank(MetaUpgradeId.RepairProtocol);
        int aegisRank = MetaRank(MetaUpgradeId.AegisMatrix);
        int novaRank = MetaRank(MetaUpgradeId.NovaCatalyst);
        int droneRank = MetaRank(MetaUpgradeId.DroneDock);
        int tunerRank = MetaRank(MetaUpgradeId.PolarityTuner);
        int surveyRank = MetaRank(MetaUpgradeId.DeepSurvey);
        _playerMaxHp = 140.0f + hullRank * 8.0f;
        _playerHp = _playerMaxHp;
        _maxEnergy = 100.0f + reactorRank * 8.0f;
        _energy = Mathf.Min(_maxEnergy, 60.0f + reactorRank * 6.0f + novaRank * 4.0f);
        _fireTimer = 0.0f;
        _dashTimer = 0.0f;
        _dashCooldown = 0.0f;
        SnapHudBars();
        _invulnTimer = 1.8f;
        _playerPolarity = 0;
        _polarityCooldown = 0.0f;
        _polarityDenyTextCooldown = 0.0f;
        _waveClearTimer = 0.0f;
        _bossPatternTimer = 0.0f;
        _spawnDirector = 0.0f;
        _sectorHazardTimer = 4.0f;
        _timeSinceHit = 99.0f;
        _absorbTextCooldown = 0.0f;
        _counterTextCooldown = 0.0f;
        _polarityTipTimer = 4.2f;
        _cruiseCharge = 0.0f;
        _assaultBurstTimer = 0.0f;
        _assaultPower = 1.0f;
        _slowMo = 1.0f;
        _multiShot = 1;
        _orbiters = 0;
        _orbiterFireTimer = 0.22f;
        _damageMultiplier = 1.0f + lensRank * 0.04f;
        _fireInterval = 0.22f;
        _playerSpeed = 420.0f + engineRank * 12.0f;
        _dashPower = 1120.0f + engineRank * 28.0f;
        _pickupMagnet = 155.0f + salvageRank * 14.0f + surveyRank * 7.0f;
        _enemySlow = 1.0f - surveyRank * 0.01f;
        _leechChance = repairRank * 0.025f;
        _mirrorReduction = 1.0f - aegisRank * 0.025f;
        _absorbEfficiency = 1.0f + tunerRank * 0.055f;
        _novaCost = 70.0f - reactorRank * 1.5f - novaRank * 2.4f;
        _critMultiplier = 1.55f + tunerRank * 0.035f;
        _dashDamage = 70.0f;
        _aegisRegen = repairRank * 0.08f;
        _echoChance = 0.0f;
        _nextWaveDamageBoost = 1.0f;
        _nextWaveRewardBoost = 1.0f;
        _polarityStorm = 0;
        _polarityCooldownMax = PolarityCooldownFor(tunerRank, _polarityStorm);
        _nextWaveBonusEnemies = 0;
        _baseRerolls = 1 + chartRank + (surveyRank >= 4 ? 1 : 0);
        _rerollsRemaining = 0;
        _astraRefraction = 0;
        _astraWake = 0;
        _vesperCharge = 0;
        _vesperFork = 0;
        _kairoDroneBay = 0;
        _kairoSync = 0;
        _solBloom = 0;
        _solForge = 0;
        _astraNovaBloom = 0;
        _astraTwinRefraction = 0;
        _vesperJudgmentCoil = 0;
        _vesperSeverLine = 0;
        _kairoOverrideMatrix = 0;
        _kairoRelayProtocol = 0;
        _solFlareCore = 0;
        _solRadiantMantle = 0;
        _riftNeedle = false;
        _runRewardGranted = false;
        _lastDustEarned = 0;
        _lastRunWave = 0;
        _lastObjectiveBonusDust = 0;
        _runObjectiveBonusDust = 0;
        _runWavesCleared = 0;
        _runPerfectWaves = 0;
        _runKills = 0;
        _runAbsorbs = 0;
        _runPickups = 0;
        _runBossKills = 0;
        _runBestCombo = 0;
        _waveTookDamage = false;
        _upgradeRanks.Clear();
        _upgradeOrder.Clear();
        SetupRunObjectives();
        ApplyPilotBaseline();
        if (droneRank > 0)
        {
            int metaDrones = 1 + (droneRank - 1) / 2;
            _orbiters = Math.Min(6, Math.Max(_orbiters, metaDrones + (_runPilot == PilotKind.Kairo ? 1 : 0)));
        }
        SnapHudBars();
        ClearShots();
        ClearEnemies();
        ClearPickups();
        ClearParticles();
        ClearDamageTexts();
        _hazards.Clear();
        _upgradeChoices.Clear();
        Burst(ScreenCenter, Cyan, 64, 680.0f, 2.2f);
        AddText(T("wake"), ScreenCenter + new Vector2(0.0f, -90.0f), Cyan, 42.0f);
        PlaySfx(220.0f, 0.9f, 0.22f, 0.36f, 0.05f, 1);
        BeginNextWave();
    }

    private void ApplyPilotBaseline()
    {
        switch (_runPilot)
        {
            case PilotKind.Vesper:
                _fireInterval = 0.58f;
                _damageMultiplier += 0.18f;
                _playerSpeed -= 18.0f;
                break;
            case PilotKind.Kairo:
                _fireInterval = 0.34f;
                _damageMultiplier *= 0.84f;
                _orbiters = 2;
                _orbiterFireTimer = 0.28f;
                _pickupMagnet += 60.0f;
                break;
            case PilotKind.Sol:
                _fireInterval = 0.48f;
                _maxEnergy += 24.0f;
                _energy = Mathf.Clamp(_energy + 24.0f, 0.0f, _maxEnergy);
                _novaCost = Mathf.Max(48.0f, _novaCost - 10.0f);
                _playerMaxHp += 10.0f;
                _playerHp += 10.0f;
                break;
            default:
                break;
        }
    }

    private void BeginNextWave()
    {
        _wave++;
        _waveClearTimer = 0.0f;
        _waveTookDamage = false;
        _spawnDirector = 0.0f;
        _waveSpawnTimer = 0.0f;
        _pendingSpawns.Clear();
        _bossPatternTimer = 0.0f;
        _sectorHazardTimer = Mathf.Max(2.5f, 8.0f - CurrentSectorIndex());
        for (int i = _shots.Count - 1; i >= 0; i--)
        {
            if (!_shots[i].FromPlayer)
            {
                RemoveShotAt(i);
            }
        }

        int sector = CurrentSectorIndex();
        int waveInSector = CurrentWaveInSector();
        SectorInfo info = CurrentSector();

        if (waveInSector == 1)
        {
            AddText(Tf("sector.enter", sector + 1, T(info.NameKey)), ScreenCenter + new Vector2(0.0f, -250.0f), info.Accent, 36.0f);
            AddText(T(info.TraitKey), ScreenCenter + new Vector2(0.0f, -202.0f), Paper, 22.0f);
        }

        if (waveInSector == WavesPerSector)
        {
            SpawnBoss();
            AddText(Tf("boss.sector", T(info.NameKey)), ScreenCenter + new Vector2(0.0f, -180.0f), info.Accent, 44.0f);
            PlaySfx(72.0f, 0.5f, 1.6f, 0.46f, 0.3f, 0);
            return;
        }

        int budget = 3 + waveInSector * 2 + sector * 4 + _nextWaveBonusEnemies;
        float rewardBoost = _nextWaveRewardBoost;
        EnemyKind primaryKind = WavePrimaryEnemyKind(sector, waveInSector);
        EnemyKind supportKind = WaveSupportEnemyKind(sector, waveInSector);
        _nextWaveBonusEnemies = 0;
        _nextWaveRewardBoost = 1.0f;
        int openingBatch = Math.Min(budget, Math.Max(3, 3 + sector + waveInSector / 3));
        for (int i = 0; i < budget; i++)
        {
            EnemyKind kind = SelectEnemyKind(i, sector, waveInSector, primaryKind, supportKind);
            PendingSpawn spawn = new()
            {
                Kind = kind,
                Polarity = i % 2,
                RewardBoost = rewardBoost,
            };

            if (i < openingBatch)
            {
                SpawnPendingEnemy(spawn);
            }
            else
            {
                _pendingSpawns.Enqueue(spawn);
            }
        }

        AddText(Tf("wave.intro", _wave), ScreenCenter + new Vector2(0.0f, -210.0f), info.Accent, 42.0f);
        AddWaveEnemyCallout(primaryKind, supportKind, info.Accent);
        ShowWaveTutorial();
        PlaySfx(120.0f + waveInSector * 18.0f + sector * 22.0f, 0.4f, 0.36f, 0.32f, 0.08f, 1);
    }

    private void ShowWaveTutorial()
    {
        string tutorial = _wave switch
        {
            1 => TutorialText(1),
            2 => TutorialText(2),
            3 => TutorialText(3),
            9 => TutorialText(9),
            _ => string.Empty,
        };

        if (tutorial.Length > 0)
        {
            AddText(tutorial, ScreenCenter + new Vector2(0.0f, -162.0f), Paper, 23.0f);
        }
    }

    private void UpdateWaveSpawns(float dt)
    {
        if (_pendingSpawns.Count == 0)
        {
            return;
        }

        if (_enemies.Count == 0)
        {
            _waveSpawnTimer = 0.0f;
        }
        else
        {
            _waveSpawnTimer -= dt;
        }

        if (_waveSpawnTimer > 0.0f)
        {
            return;
        }

        int sector = CurrentSectorIndex();
        int spawnCount = (_enemies.Count < 8 + sector * 2 && _pendingSpawns.Count > 3) ? 2 : 1;
        if (_visualPressure > 0.82f || _enemies.Count > 34 + sector * 3)
        {
            spawnCount = 1;
            _waveSpawnTimer = 0.55f;
            return;
        }

        for (int i = 0; i < spawnCount && _pendingSpawns.Count > 0; i++)
        {
            SpawnPendingEnemy(_pendingSpawns.Dequeue());
        }

        float progress = RunProgress01();
        _waveSpawnTimer = Mathf.Lerp(1.25f, 0.72f, progress) + _rng.RandfRange(-0.12f, 0.16f);
    }

    private void SpawnPendingEnemy(PendingSpawn spawn)
    {
        Enemy? enemy = SpawnEnemy(spawn.Kind, RandomArenaEdge(), spawn.Polarity);
        if (enemy != null)
        {
            enemy.Value = (int)(enemy.Value * spawn.RewardBoost);
        }
    }

    private void SetupRunObjectives()
    {
        _runObjectives.Clear();

        int nextMilestone = Mathf.Clamp(((_bestWave / 8) + 1) * 8, 8, TotalWaves);
        AddRunObjective(RunObjectiveKind.ReachWave, nextMilestone, 24 + nextMilestone * 2);

        int perfectTarget = _bestWave < 16 ? 2 : _bestWave < 32 ? 3 : 4;
        AddRunObjective(RunObjectiveKind.PerfectWaves, perfectTarget, 34 + perfectTarget * 16);

        switch (_selectedPilot)
        {
            case PilotKind.Vesper:
                AddRunObjective(RunObjectiveKind.BestCombo, _bestWave < 16 ? 24 : 38, 58 + CurrentSectorIndex() * 6);
                break;
            case PilotKind.Kairo:
                AddRunObjective(RunObjectiveKind.CollectPickups, _bestWave < 16 ? 120 : 180, 54 + MetaRank(MetaUpgradeId.SalvageRig) * 4);
                break;
            case PilotKind.Sol:
                AddRunObjective(RunObjectiveKind.DefeatEnemies, _bestWave < 16 ? 90 : 145, 56 + MetaRank(MetaUpgradeId.FocusLens) * 4);
                break;
            default:
                AddRunObjective(RunObjectiveKind.AbsorbBullets, _bestWave < 16 ? 52 : 86, 52 + MetaRank(MetaUpgradeId.PolarityTuner) * 4);
                break;
        }
    }

    private void AddRunObjective(RunObjectiveKind kind, int target, int rewardDust)
    {
        _runObjectives.Add(new RunObjective
        {
            Kind = kind,
            Target = Math.Max(1, target),
            RewardDust = Math.Max(1, rewardDust),
        });
    }

    private void OnWaveCleared()
    {
        _runWavesCleared++;
        SetObjectiveProgress(RunObjectiveKind.ReachWave, _wave);

        if (!_waveTookDamage)
        {
            _runPerfectWaves++;
            AddObjectiveProgress(RunObjectiveKind.PerfectWaves, 1);
            _energy = Mathf.Clamp(_energy + 10.0f, 0.0f, _maxEnergy);
            _playerHp = Mathf.Clamp(_playerHp + 4.0f, 0.0f, _playerMaxHp);
            AddText(T("objective.clean_wave"), ScreenCenter + new Vector2(0.0f, -118.0f), Jade, 22.0f);
        }
    }

    private void AddObjectiveProgress(RunObjectiveKind kind, int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        foreach (RunObjective objective in _runObjectives)
        {
            if (objective.Kind == kind && !objective.Completed)
            {
                objective.Progress = Math.Min(objective.Target, objective.Progress + amount);
                CheckObjectiveComplete(objective);
            }
        }
    }

    private void SetObjectiveProgress(RunObjectiveKind kind, int progress)
    {
        foreach (RunObjective objective in _runObjectives)
        {
            if (objective.Kind == kind && !objective.Completed)
            {
                objective.Progress = Math.Min(objective.Target, Math.Max(objective.Progress, progress));
                CheckObjectiveComplete(objective);
            }
        }
    }

    private void CheckObjectiveComplete(RunObjective objective)
    {
        if (objective.Completed || objective.Progress < objective.Target)
        {
            return;
        }

        objective.Completed = true;
        _runObjectiveBonusDust += objective.RewardDust;
        AddText(Tf("objective.complete", objective.RewardDust), ScreenCenter + new Vector2(0.0f, -92.0f), Gold, 24.0f);
        Burst(ScreenCenter + new Vector2(0.0f, -80.0f), Gold, 30, 340.0f, 0.9f);
        PlaySfx(520.0f, 180.0f, 0.2f, 0.24f, 0.02f, 1);
    }

    private void RefreshRunBestCombo()
    {
        if (_combo <= _runBestCombo)
        {
            return;
        }

        _runBestCombo = _combo;
        SetObjectiveProgress(RunObjectiveKind.BestCombo, _runBestCombo);
    }

    private void SpawnBoss()
    {
        int sector = CurrentSectorIndex();
        float threat = ThreatLevel();
        Enemy? boss = AddEnemy();
        if (boss == null)
        {
            return;
        }

        boss.Kind = EnemyKind.Boss;
        boss.Pos = new Vector2(ScreenWidth * 0.5f, Arena.Position.Y + 148.0f);
        boss.Vel = Vector2.Zero;
        boss.Radius = 82.0f + sector * 8.0f;
        boss.Hp = 1500.0f + sector * 1000.0f + threat * 45.0f;
        boss.MaxHp = boss.Hp;
        boss.Cooldown = 1.0f;
        boss.Overheat = 0.0f;
        boss.OverheatMax = 1.0f;
        boss.Phase = 0.0f;
        boss.SpawnPulse = 1.0f;
        boss.ContactTimer = 0.0f;
        boss.Polarity = sector % 2;
        boss.Value = 8000 + sector * 5000;
        boss.SplitDepth = 0;
        boss.Elite = true;
        boss.Armor = 1.0f + sector * 0.08f;
        Burst(boss.Pos, CurrentSector().Accent, 100 + sector * 24, 760.0f + sector * 80.0f, 2.8f);
        _shake = 1.0f;
    }

    private Enemy? SpawnEnemy(EnemyKind kind, Vector2 pos, int polarity, int splitDepth = 0)
    {
        Enemy? enemy = AddEnemy();
        if (enemy == null)
        {
            return null;
        }

        float threat = ThreatLevel();
        int sector = CurrentSectorIndex();
        float radius = kind switch
        {
            EnemyKind.Chaser => 25.0f,
            EnemyKind.Weaver => 29.0f,
            EnemyKind.Turret => 33.0f,
            EnemyKind.Splitter => 34.0f,
            EnemyKind.Lance => 31.0f,
            EnemyKind.Mine => 26.0f,
            EnemyKind.Shard => 22.0f,
            EnemyKind.Warden => 42.0f,
            EnemyKind.Drifter => 27.0f,
            EnemyKind.Bulwark => 40.0f,
            EnemyKind.Siren => 32.0f,
            EnemyKind.Harrier => 24.0f,
            _ => 30.0f,
        };

        float hp = kind switch
        {
            EnemyKind.Chaser => 42.0f + threat * 8.0f,
            EnemyKind.Weaver => 56.0f + threat * 9.0f,
            EnemyKind.Turret => 86.0f + threat * 12.0f,
            EnemyKind.Splitter => 78.0f + threat * 10.0f,
            EnemyKind.Lance => 92.0f + threat * 13.0f,
            EnemyKind.Mine => 48.0f + threat * 7.0f,
            EnemyKind.Shard => 38.0f + threat * 7.5f,
            EnemyKind.Warden => 150.0f + threat * 18.0f,
            EnemyKind.Drifter => 64.0f + threat * 8.5f,
            EnemyKind.Bulwark => 170.0f + threat * 18.0f,
            EnemyKind.Siren => 95.0f + threat * 11.0f,
            EnemyKind.Harrier => 54.0f + threat * 9.0f,
            _ => 70.0f,
        };

        float eliteChance = kind == EnemyKind.Boss || splitDepth > 0 || sector == 0 ? 0.0f : Mathf.Clamp(sector * 0.04f + CurrentWaveInSector() * 0.004f, 0.0f, 0.2f);
        bool elite = _rng.Randf() < eliteChance;
        if (elite)
        {
            radius *= 1.16f;
            hp *= 2.35f;
        }

        enemy.Kind = kind;
        enemy.Pos = pos;
        enemy.Vel = Vector2.Zero;
        enemy.Radius = radius;
        enemy.Hp = hp;
        enemy.MaxHp = hp;
        enemy.Cooldown = _rng.RandfRange(0.35f, 1.45f);
        enemy.Overheat = 0.0f;
        enemy.OverheatMax = 1.0f;
        enemy.Phase = _rng.RandfRange(0.0f, Mathf.Tau);
        enemy.SpawnPulse = 1.0f;
        enemy.ContactTimer = 0.0f;
        enemy.Polarity = polarity;
        enemy.Value = (int)((100 + _wave * 32 + (int)kind * 45) * (elite ? 2.8f : 1.0f));
        enemy.SplitDepth = splitDepth;
        enemy.Elite = elite;
        enemy.Armor = elite ? 1.18f + sector * 0.06f : 1.0f;
        if (kind == EnemyKind.Bulwark)
        {
            enemy.Armor += 0.22f;
        }
        Burst(pos, PolarityColor(polarity), 12, 220.0f, 0.9f);
        return enemy;
    }

    private void UpdateTitle(float dt)
    {
        UpdateParticles(dt);
        UpdateDamageTexts(dt);
        Vector2 mouse = GetGlobalMousePosition();
        bool startKey = KeyDown(Key.Enter);
        bool metaKey = KeyDown(Key.U);
        bool click = Input.IsMouseButtonPressed(MouseButton.Left) && !_lastClick;
        if (click)
        {
            for (int i = 0; i < PilotCount(); i++)
            {
                PilotKind pilot = PilotFromIndex(i);
                if (PilotCardRect(i).HasPoint(mouse))
                {
                    if (IsPilotUnlocked(pilot))
                    {
                        _selectedPilot = pilot;
                        SaveMetaProgress();
                        PlaySfx(360.0f + i * 55.0f, 70.0f, 0.14f, 0.2f, 0.02f, 1);
                    }
                    else
                    {
                        AddText(PilotUnlockText(pilot), ScreenCenter + new Vector2(0.0f, 210.0f), Rose, 22.0f);
                        PlaySfx(120.0f, -20.0f, 0.12f, 0.16f, 0.05f, 0);
                    }
                    return;
                }
            }
        }

        if ((startKey && !_lastStart) || (click && StartButtonRect().HasPoint(mouse)))
        {
            StartRun();
        }
        else if ((metaKey && !_lastMeta) || (click && MetaButtonRect().HasPoint(mouse)))
        {
            _mode = GameMode.Meta;
            PlaySfx(360.0f, 80.0f, 0.18f, 0.2f, 0.02f, 1);
        }
        else if (click && TitleSettingsButtonRect().HasPoint(mouse))
        {
            OpenSettings(GameMode.Title);
        }
    }

    private void UpdateSettings(float dt)
    {
        UpdateParticles(dt);
        UpdateDamageTexts(dt);
        _deleteSaveConfirmTimer = Mathf.Max(0.0f, _deleteSaveConfirmTimer - dt);
        _deleteSaveNoticeTimer = Mathf.Max(0.0f, _deleteSaveNoticeTimer - dt);
        Vector2 mouse = GetGlobalMousePosition();
        bool click = Input.IsMouseButtonPressed(MouseButton.Left) && !_lastClick;

        if ((KeyDown(Key.Escape) && !_lastBack) || (click && SettingsBackButtonRect().HasPoint(mouse)))
        {
            _deleteSaveConfirmTimer = 0.0f;
            CloseSettings();
            return;
        }

        if (click && SettingsGuideButtonRect().HasPoint(mouse))
        {
            _deleteSaveConfirmTimer = 0.0f;
            _mode = GameMode.Guide;
            PlaySfx(420.0f, 120.0f, 0.16f, 0.2f, 0.02f, 1);
            return;
        }

        if (click && SettingsLanguageButtonRect().HasPoint(mouse))
        {
            _deleteSaveConfirmTimer = 0.0f;
            ToggleLanguage();
            return;
        }

        if (click && SettingsDeleteSaveButtonRect().HasPoint(mouse))
        {
            if (_deleteSaveConfirmTimer > 0.0f)
            {
                DeleteSaveData();
                PlaySfx(160.0f, -120.0f, 0.24f, 0.24f, 0.06f, 1);
            }
            else
            {
                _deleteSaveConfirmTimer = 3.8f;
                PlaySfx(280.0f, -40.0f, 0.14f, 0.18f, 0.03f, 1);
            }
            return;
        }

        if (IsRunViewMode(_settingsReturnMode) && click && SettingsMainMenuButtonRect().HasPoint(mouse))
        {
            _deleteSaveConfirmTimer = 0.0f;
            ResetTitle();
            PlaySfx(180.0f, -80.0f, 0.18f, 0.2f, 0.04f, 1);
            return;
        }
    }

    private void UpdateGuide(float dt)
    {
        UpdateParticles(dt);
        UpdateDamageTexts(dt);
        Vector2 mouse = GetGlobalMousePosition();
        bool click = Input.IsMouseButtonPressed(MouseButton.Left) && !_lastClick;

        if ((KeyDown(Key.Escape) && !_lastBack) || (click && GuideBackButtonRect().HasPoint(mouse)))
        {
            _mode = GameMode.Settings;
            PlaySfx(300.0f, -80.0f, 0.12f, 0.18f, 0.02f, 1);
        }
    }

    private void OpenSettings(GameMode returnMode)
    {
        _settingsReturnMode = returnMode;
        _mode = GameMode.Settings;
        _deleteSaveConfirmTimer = 0.0f;
        PlaySfx(360.0f, 80.0f, 0.14f, 0.18f, 0.02f, 1);
    }

    private void CloseSettings()
    {
        _mode = _settingsReturnMode;
        PlaySfx(240.0f, -60.0f, 0.12f, 0.16f, 0.02f, 1);
    }

    private void UpdateMeta(float dt)
    {
        UpdateParticles(dt);
        UpdateDamageTexts(dt);
        Vector2 mouse = GetGlobalMousePosition();
        bool click = Input.IsMouseButtonPressed(MouseButton.Left) && !_lastClick;

        if ((KeyDown(Key.Escape) && !_lastBack) || (click && MetaBackButtonRect().HasPoint(mouse)))
        {
            ResetTitle();
            return;
        }

        for (int i = 0; i < MetaUpgrades.Length; i++)
        {
            if (MetaHotkeyPressed(i) || (click && MetaUpgradeRect(i).HasPoint(mouse)))
            {
                TryBuyMetaUpgrade(MetaUpgrades[i]);
                return;
            }
        }
    }

    private bool MetaHotkeyPressed(int index)
    {
        return index switch
        {
            0 => KeyDown(Key.Key1) && !_lastOne,
            1 => KeyDown(Key.Key2) && !_lastTwo,
            2 => KeyDown(Key.Key3) && !_lastThree,
            3 => KeyDown(Key.Key4) && !_lastFour,
            4 => KeyDown(Key.Key5) && !_lastFive,
            5 => KeyDown(Key.Key6) && !_lastSix,
            6 => KeyDown(Key.Key7) && !_lastSeven,
            7 => KeyDown(Key.Key8) && !_lastEight,
            8 => KeyDown(Key.Key9) && !_lastNine,
            _ => false,
        };
    }

    private void TryBuyMetaUpgrade(MetaUpgradeDef def)
    {
        int rank = MetaRank(def.Id);
        if (rank >= def.MaxRank)
        {
            AddText(T("meta.max"), ScreenCenter + new Vector2(0.0f, -250.0f), def.Accent, 28.0f);
            PlaySfx(180.0f, -20.0f, 0.14f, 0.16f, 0.04f, 0);
            return;
        }

        int cost = MetaUpgradeCost(def, rank);
        if (_starDust < cost)
        {
            AddText(Tf("meta.short", cost - _starDust), ScreenCenter + new Vector2(0.0f, -250.0f), Rose, 28.0f);
            PlaySfx(110.0f, -18.0f, 0.16f, 0.18f, 0.06f, 0);
            return;
        }

        _starDust -= cost;
        _metaRanks[def.Id] = rank + 1;
        SaveMetaProgress();
        Burst(ScreenCenter + new Vector2(0.0f, 40.0f), def.Accent, 48, 420.0f, 1.2f);
        AddText(T("meta.bought"), ScreenCenter + new Vector2(0.0f, -250.0f), def.Accent, 28.0f);
        PlaySfx(420.0f, 220.0f, 0.24f, 0.26f, 0.02f, 1);
    }

    private void UpdatePlaying(float dt)
    {
        Vector2 mouse = GetGlobalMousePosition();
        bool click = Input.IsMouseButtonPressed(MouseButton.Left) && !_lastClick;
        if ((click && HudSettingsButtonRect().HasPoint(mouse)) || (KeyDown(Key.Escape) && !_lastBack))
        {
            OpenSettings(GameMode.Playing);
            return;
        }

        float gameDt = dt * _slowMo;
        _slowMo = Approach(_slowMo, 1.0f, dt * 1.5f);
        _energy = Mathf.Clamp(_energy + dt * 3.0f, 0.0f, _maxEnergy);
        _fireTimer -= gameDt;
        _dashTimer -= gameDt;
        _dashCooldown -= gameDt;
        _invulnTimer -= gameDt;
        _polarityCooldown = Mathf.Max(0.0f, _polarityCooldown - gameDt);
        _polarityDenyTextCooldown -= gameDt;
        _absorbTextCooldown -= gameDt;
        _counterTextCooldown -= gameDt;
        _polarityTipTimer -= gameDt;
        _assaultBurstTimer = Mathf.Max(0.0f, _assaultBurstTimer - gameDt);
        if (_playerPolarity == 0)
        {
            _cruiseCharge = Mathf.Max(0.0f, _cruiseCharge - gameDt * 1.2f);
        }
        _comboTimer -= gameDt;
        _timeSinceHit += gameDt;
        if (_comboTimer <= 0.0f)
        {
            _combo = 0;
        }
        if (_aegisRegen > 0.0f && _timeSinceHit > 4.0f && _playerHp > 0.0f)
        {
            _playerHp = Mathf.Clamp(_playerHp + _aegisRegen * gameDt, 0.0f, _playerMaxHp);
        }

        UpdatePlayer(gameDt);
        UpdateWaveSpawns(gameDt);
        UpdateEnemies(gameDt);
        UpdateShots(gameDt);
        UpdatePickups(gameDt);
        UpdateParticles(dt);
        UpdateDamageTexts(dt);
        UpdateSectorHazards(gameDt);
        ResolveCombat(gameDt);
        UpdateOrbiters(gameDt);

        if (_enemies.Count == 0 && _pendingSpawns.Count == 0)
        {
            _waveClearTimer += gameDt;
            if (_waveClearTimer > 1.0f)
            {
                _nextWaveDamageBoost = 1.0f;
                OnWaveCleared();
                if (_wave >= TotalWaves)
                {
                    WinRun();
                }
                else
                {
                    OpenUpgradeChoice();
                }
            }
        }
    }

    private void UpdatePlayer(float dt)
    {
        Vector2 move = Vector2.Zero;
        if (KeyDown(Key.A) || KeyDown(Key.Left))
        {
            move.X -= 1.0f;
        }
        if (KeyDown(Key.D) || KeyDown(Key.Right))
        {
            move.X += 1.0f;
        }
        if (KeyDown(Key.W) || KeyDown(Key.Up))
        {
            move.Y -= 1.0f;
        }
        if (KeyDown(Key.S) || KeyDown(Key.Down))
        {
            move.Y += 1.0f;
        }
        if (move.LengthSquared() > 1.0f)
        {
            move = move.Normalized();
        }

        Vector2 mouse = GetGlobalMousePosition();
        Vector2 aim = mouse - _playerPos;
        if (aim.LengthSquared() > 0.01f)
        {
            _aimDir = aim.Normalized();
        }

        bool toggle = KeyDown(Key.Space);
        if (toggle && !_lastToggle)
        {
            if (_polarityCooldown <= 0.0f)
            {
                TogglePolarity();
            }
            else if (_polarityDenyTextCooldown <= 0.0f)
            {
                AddText(PolarityCooldownText(), _playerPos + new Vector2(0.0f, -88.0f), Alpha(Paper, 0.76f), 18.0f);
                _polarityDenyTextCooldown = 0.45f;
                PlaySfx(180.0f, -90.0f, 0.08f, 0.12f, 0.02f, 1);
            }
        }

        bool nova = KeyDown(Key.F) || KeyDown(Key.E);
        if (nova && !_lastNova && _energy >= _novaCost)
        {
            CastUltimate();
        }

        bool dash = KeyDown(Key.Shift) || Input.IsMouseButtonPressed(MouseButton.Right);
        if (dash && !_lastDash && _dashCooldown <= 0.0f)
        {
            StartDash(move.LengthSquared() > 0.0f ? move.Normalized() : _aimDir);
        }

        float speed = _playerSpeed;
        if (_dashTimer > 0.0f)
        {
            speed = _dashPower;
            move = _aimDir;
            _invulnTimer = Mathf.Max(_invulnTimer, 0.05f);
            ClearBulletsNear(_playerPos, 62.0f + (_dashDamage - 70.0f) * 0.24f, true);
        }

        _playerVel = _playerVel.Lerp(move * speed, 1.0f - Mathf.Exp(-dt * 14.0f));
        _playerPos += _playerVel * dt;
        _playerPos = ClampToArena(_playerPos, PlayerRadius + 6.0f);

        if (_enemies.Count > 0 && _fireTimer <= 0.0f)
        {
            FirePlayerShot();
            _fireTimer = _fireInterval;
        }

        if (_playerHp <= 0.0f)
        {
            LoseRun();
        }
    }

    private void TogglePolarity()
    {
        int previous = _playerPolarity;
        _playerPolarity = 1 - _playerPolarity;
        _polarityCooldown = _polarityCooldownMax;
        _polarityTipTimer = 2.4f;
        _energy = Mathf.Clamp(_energy + 8.0f, 0.0f, _maxEnergy);

        if (previous == 0 && _playerPolarity == 1)
        {
            float charge01 = CruiseCharge01();
            _assaultPower = 1.15f + charge01 * 0.62f;
            _assaultBurstTimer = Mathf.Lerp(AssaultBurstMin, AssaultBurstMax, charge01);
            _cruiseCharge = 0.0f;
            if (charge01 > 0.18f)
            {
                AddText(AssaultText(_assaultPower), _playerPos + new Vector2(0.0f, -112.0f), PolarityAmber, 20.0f);
            }
        }
        else if (previous == 1 && _playerPolarity == 0)
        {
            float clearRadius = 110.0f + Mathf.Clamp(_assaultPower - 1.0f, 0.0f, 0.8f) * 120.0f;
            ClearBulletsNear(_playerPos, clearRadius, true);
            _invulnTimer = Mathf.Max(_invulnTimer, 0.22f);
            _assaultBurstTimer = 0.0f;
            _assaultPower = 1.0f;
            AddText(CruiseText(), _playerPos + new Vector2(0.0f, -112.0f), PolarityBlue, 20.0f);
        }

        Burst(_playerPos, PolarityColor(_playerPolarity), 34, 400.0f, 1.0f);
        AddText(PolarityTipText(), _playerPos + new Vector2(0.0f, -86.0f), PolarityColor(_playerPolarity), 22.0f);
        if (_polarityStorm > 0)
        {
            FirePolarityStorm();
        }
        PlaySfx(_playerPolarity == 0 ? 560.0f : 390.0f, 0.65f, 0.12f, 0.22f, 0.02f, 2);
    }

    private void CastUltimate()
    {
        _energy -= _novaCost;
        _slowMo = 0.42f;
        _shake = 1.0f;
        _flash = 0.86f;
        Color color = PolarityColor(_playerPolarity);
        AddText(UltimateName(_runPilot), _playerPos + new Vector2(0.0f, -104.0f), PilotAccent(_runPilot), 30.0f);

        switch (_runPilot)
        {
            case PilotKind.Vesper:
                CastVesperUltimate(color);
                break;
            case PilotKind.Kairo:
                CastKairoUltimate(color);
                break;
            case PilotKind.Sol:
                CastSolUltimate(color);
                break;
            default:
                CastAstraUltimate(color);
                break;
        }
    }

    private void CastAstraUltimate(Color color)
    {
        Burst(_playerPos, color, 118, 920.0f, 1.75f);
        ClearBulletsNear(_playerPos, 500.0f + _astraNovaBloom * 45.0f, true);

        int count = 24 + _astraRefraction * 4 + _astraNovaBloom * 5;
        for (int i = 0; i < count; i++)
        {
            float angle = i * Mathf.Tau / count + _time * 0.12f;
            Vector2 dir = Vector2.Right.Rotated(angle);
            int polarity = i % 2;
            SpawnPlayerShot(_playerPos + dir * 44.0f, dir, 1280.0f, 5.2f, (24.0f + _astraWake * 3.0f + _astraNovaBloom * 3.0f) * _damageMultiplier, 0.86f, 2 + _astraRefraction / 2 + _astraTwinRefraction, true, polarity);
        }

        if (_astraTwinRefraction > 0)
        {
            int echoCount = 8 + _astraTwinRefraction * 4;
            for (int i = 0; i < echoCount; i++)
            {
                float angle = -i * Mathf.Tau / echoCount + _time * 0.2f;
                Vector2 dir = Vector2.Right.Rotated(angle);
                SpawnPlayerShot(_playerPos + dir * 24.0f, dir, 960.0f, 6.0f, (18.0f + _astraTwinRefraction * 4.0f) * _damageMultiplier, 1.02f, _astraTwinRefraction, true, 1 - i % 2);
            }
        }

        for (int i = _enemies.Count - 1; i >= 0; i--)
        {
            Enemy enemy = _enemies[i];
            float distance = enemy.Pos.DistanceTo(_playerPos);
            float range = 560.0f + _astraNovaBloom * 40.0f;
            if (distance < range)
            {
                float damage = Mathf.Lerp(142.0f + _astraNovaBloom * 22.0f, 42.0f + _astraTwinRefraction * 8.0f, distance / range) * _damageMultiplier;
                DamageEnemy(enemy, damage, _playerPos, true);
            }
        }

        PlaySfx(96.0f, 1.8f, 0.5f, 0.55f, 0.22f, 0);
    }

    private void CastVesperUltimate(Color color)
    {
        Vector2 dir = _aimDir.LengthSquared() < 0.01f ? Vector2.Right : _aimDir.Normalized();
        Vector2 start = _playerPos - dir * 80.0f;
        Vector2 end = _playerPos + dir * 1760.0f;
        float width = 58.0f + _vesperCharge * 4.0f + _vesperJudgmentCoil * 8.0f;

        Burst(_playerPos + dir * 84.0f, color, 70, 760.0f, 1.2f);
        ClearEnemyBulletsInBeam(start, end, width + 18.0f, true);

        for (int i = _enemies.Count - 1; i >= 0; i--)
        {
            Enemy enemy = _enemies[i];
            float distance = DistancePointToSegment(enemy.Pos, start, end);
            if (distance <= width + enemy.Radius)
            {
                float along = Mathf.Clamp((enemy.Pos - _playerPos).Dot(dir) / 1760.0f, 0.0f, 1.0f);
                float damage = Mathf.Lerp(360.0f + _vesperCharge * 34.0f + _vesperJudgmentCoil * 56.0f, 128.0f + _vesperJudgmentCoil * 20.0f, along) * _damageMultiplier;
                DamageEnemy(enemy, damage, _playerPos, true);
                Burst(enemy.Pos, color, 10, 360.0f, 0.48f);
            }
        }

        if (_vesperSeverLine > 0)
        {
            Vector2 right = new(-dir.Y, dir.X);
            for (int lane = 1; lane <= _vesperSeverLine; lane++)
            {
                float offset = 68.0f + lane * 42.0f;
                DamageVesperBeam(start + right * offset, end + right * offset, width * 0.38f, 112.0f + _vesperSeverLine * 26.0f, color);
                DamageVesperBeam(start - right * offset, end - right * offset, width * 0.38f, 112.0f + _vesperSeverLine * 26.0f, color);
            }
        }

        for (int i = 0; i < 18; i++)
        {
            float t = i / 17.0f;
            Vector2 point = start.Lerp(end, t);
            AddParticle(point, dir * 260.0f + RandomDirection() * 80.0f, color.Lerp(Paper, 0.32f), 10.0f, 0.34f + t * 0.14f);
        }

        PlaySfx(58.0f, 2.2f, 0.62f, 0.62f, 0.18f, 0);
    }

    private void DamageVesperBeam(Vector2 start, Vector2 end, float width, float damage, Color color)
    {
        ClearEnemyBulletsInBeam(start, end, width + 10.0f, true);
        for (int i = _enemies.Count - 1; i >= 0; i--)
        {
            Enemy enemy = _enemies[i];
            if (DistancePointToSegment(enemy.Pos, start, end) <= width + enemy.Radius)
            {
                DamageEnemy(enemy, damage * _damageMultiplier, _playerPos, true);
                Burst(enemy.Pos, color, 5, 240.0f, 0.35f);
            }
        }

        Vector2 dir = (end - start).Normalized();
        for (int i = 0; i < 8; i++)
        {
            float t = i / 7.0f;
            AddParticle(start.Lerp(end, t), dir * 160.0f + RandomDirection() * 52.0f, color.Lerp(Paper, 0.24f), 6.0f, 0.24f);
        }
    }

    private void CastKairoUltimate(Color color)
    {
        int droneCount = Math.Min(10, Math.Max(_orbiters, 4 + _kairoDroneBay + _kairoOverrideMatrix * 2));
        int volleys = Math.Min(5, 3 + Math.Min(2, _kairoSync / 2) + _kairoRelayProtocol);
        Burst(_playerPos, PickupBlue, 86, 720.0f, 1.35f);
        ClearBulletsNear(_playerPos, 360.0f + droneCount * 18.0f + _kairoOverrideMatrix * 36.0f, true);

        for (int volley = 0; volley < volleys; volley++)
        {
            for (int i = 0; i < droneCount; i++)
            {
                float angle = i * Mathf.Tau / droneCount + volley * 0.21f + _time * 0.18f;
                Vector2 origin = _playerPos + Vector2.Right.Rotated(angle) * (92.0f + volley * 18.0f);
                Enemy? target = FindNearestEnemy(origin, 1180.0f);
                if (target != null)
                {
                    FireOrbiterShot(origin, target);
                }
                else
                {
                    SpawnPlayerShot(origin, Vector2.Right.Rotated(angle), 1120.0f, 4.8f, (13.0f + _kairoSync * 2.2f + _kairoOverrideMatrix * 1.8f) * _damageMultiplier, 0.8f, _kairoRelayProtocol >= 2 ? 1 : 0, false);
                }
                AddParticle(origin, (origin - _playerPos).Normalized() * 170.0f, color, 7.0f, 0.22f);
            }
        }

        _orbiterFireTimer = 0.0f;
        _energy = Mathf.Clamp(_energy + _kairoRelayProtocol * 7.0f, 0.0f, _maxEnergy);
        PlaySfx(220.0f, 0.95f, 0.42f, 0.5f, 0.08f, 2);
    }

    private void CastSolUltimate(Color color)
    {
        Burst(_playerPos, Gold, 150, 900.0f, 1.85f);
        ClearBulletsNear(_playerPos, 760.0f + _solFlareCore * 55.0f, true);

        int count = Math.Min(58, 36 + _solBloom * 4 + _solFlareCore * 6);
        for (int i = 0; i < count; i++)
        {
            float angle = i * Mathf.Tau / count;
            Vector2 dir = Vector2.Right.Rotated(angle);
            SpawnPlayerShot(_playerPos + dir * 46.0f, dir, 940.0f, 6.8f, (18.0f + _solForge * 2.6f + _solFlareCore * 3.0f) * _damageMultiplier, 0.74f, 1, false);
        }

        for (int i = _enemies.Count - 1; i >= 0; i--)
        {
            Enemy enemy = _enemies[i];
            float distance = enemy.Pos.DistanceTo(_playerPos);
            float range = 720.0f + _solFlareCore * 50.0f;
            if (distance < range)
            {
                float damage = Mathf.Lerp(198.0f + _solForge * 18.0f + _solFlareCore * 30.0f, 54.0f + _solFlareCore * 8.0f, distance / range) * _damageMultiplier;
                DamageEnemy(enemy, damage, _playerPos, true);
            }
        }

        _playerHp = Mathf.Clamp(_playerHp + 14.0f + _solForge * 4.0f + _solRadiantMantle * 12.0f, 0.0f, _playerMaxHp);
        _invulnTimer = Mathf.Max(_invulnTimer, 0.55f + _solRadiantMantle * 0.35f);
        PlaySfx(132.0f, 1.35f, 0.58f, 0.58f, 0.18f, 1);
    }

    private void StartDash(Vector2 direction)
    {
        if (direction.LengthSquared() < 0.1f)
        {
            direction = Vector2.Right;
        }
        _dashTimer = 0.16f;
        _dashCooldown = 0.86f;
        _invulnTimer = 0.22f;
        _playerVel = direction.Normalized() * _dashPower;
        Burst(_playerPos, PolarityColor(_playerPolarity), 24, 520.0f, 0.7f);
        PlaySfx(150.0f, 1.2f, 0.16f, 0.34f, 0.12f, 2);
    }

    private void FirePlayerShot()
    {
        switch (_runPilot)
        {
            case PilotKind.Vesper:
                FireVesperShot();
                return;
            case PilotKind.Kairo:
                FireKairoShot();
                return;
            case PilotKind.Sol:
                FireSolShot();
                return;
        }

        int count = Math.Min(8, _multiShot + _astraRefraction);
        float spread = count == 1 ? 0.0f : 0.09f + count * 0.015f;
        Color color = PolarityColor(_playerPolarity);

        for (int i = 0; i < count; i++)
        {
            float offset = count == 1 ? 0.0f : (i - (count - 1) * 0.5f) * spread;
            Vector2 dir = _aimDir.Rotated(offset);
            Shot? shot = AddShot(true);
            if (shot == null)
            {
                continue;
            }

            shot.Pos = _playerPos + dir * 38.0f + dir.Orthogonal() * offset * 70.0f;
            shot.Prev = _playerPos;
            shot.Vel = dir * (_riftNeedle ? 1540.0f : 1280.0f);
            shot.Radius = _riftNeedle ? 5.0f : PlayerBulletRadius;
            shot.Damage = (_riftNeedle ? 24.0f : 18.0f) * (1.0f + _astraWake * 0.08f) * _damageMultiplier * PlayerShotDamageScale(_playerPolarity);
            shot.Life = _riftNeedle ? 0.76f : 1.25f;
            shot.MaxLife = shot.Life;
            shot.Polarity = _playerPolarity;
            shot.Pierce = _riftNeedle ? 2 : 0;
            shot.Rift = _riftNeedle;
            ResetShotTrail(shot, shot.Pos);

            if (_echoChance > 0.0f && _rng.Randf() < _echoChance)
            {
                Vector2 echoDir = dir.Rotated(_rng.RandfRange(-0.05f, 0.05f));
                Shot? echo = AddShot(true);
                if (echo != null)
                {
                    echo.Pos = shot.Pos - echoDir * 14.0f;
                    echo.Prev = shot.Prev;
                    echo.Vel = echoDir * 1460.0f;
                    echo.Radius = 4.5f;
                    echo.Damage = shot.Damage * 0.46f;
                    echo.Life = 0.62f;
                    echo.MaxLife = 0.62f;
                    echo.Polarity = _playerPolarity;
                    echo.Pierce = 1;
                    echo.Rift = true;
                    ResetShotTrail(echo, echo.Pos);
                }
            }
        }

        AddParticle(_playerPos + _aimDir * 32.0f, _aimDir * 180.0f, color, 10.0f, 0.18f);
        PlaySfx(_playerPolarity == 0 ? 690.0f : 520.0f, -140.0f, 0.06f, 0.14f, 0.01f, 1);
    }

    private void FireVesperShot()
    {
        Color color = PolarityColor(_playerPolarity);
        SpawnPlayerShot(_playerPos + _aimDir * 42.0f, _aimDir, 1740.0f, 5.5f, (55.0f + _vesperCharge * 7.5f + (_riftNeedle ? 8.0f : 0.0f)) * _damageMultiplier, 0.7f, 4 + _vesperCharge / 2 + (_riftNeedle ? 1 : 0), true);

        int sideRails = Math.Min(3, _vesperFork + Math.Max(0, _multiShot - 1));
        if (sideRails > 0)
        {
            for (int i = 1; i <= sideRails; i++)
            {
                float angle = 0.075f * i;
                SpawnPlayerShot(_playerPos + _aimDir * 36.0f, _aimDir.Rotated(angle), 1560.0f, 4.0f, (19.0f + _vesperCharge * 2.2f) * _damageMultiplier, 0.58f, 2, true);
                SpawnPlayerShot(_playerPos + _aimDir * 36.0f, _aimDir.Rotated(-angle), 1560.0f, 4.0f, (19.0f + _vesperCharge * 2.2f) * _damageMultiplier, 0.58f, 2, true);
            }
        }

        AddParticle(_playerPos + _aimDir * 34.0f, _aimDir * 220.0f, color, 9.0f, 0.16f);
        PlaySfx(_playerPolarity == 0 ? 430.0f : 330.0f, -180.0f, 0.08f, 0.18f, 0.01f, 1);
    }

    private void FireKairoShot()
    {
        Enemy? target = FindNearestEnemy(_playerPos, 760.0f);
        Vector2 dir = target == null ? _aimDir : (target.Pos - _playerPos).Normalized();
        int count = Math.Min(3, 1 + Math.Max(0, _multiShot - 1) / 2 + (_kairoSync >= 3 ? 1 : 0));
        for (int i = 0; i < count; i++)
        {
            float offset = count == 1 ? 0.0f : (i == 0 ? -0.08f : 0.08f);
            Vector2 shotDir = dir.Rotated(offset);
            SpawnPlayerShot(_playerPos + shotDir * 36.0f, shotDir, _riftNeedle ? 1320.0f : 1120.0f, _riftNeedle ? 4.2f : 5.0f, (12.8f + _kairoSync * 2.6f + (_riftNeedle ? 3.0f : 0.0f)) * _damageMultiplier, 1.05f, _riftNeedle ? 1 : 0, _riftNeedle);
        }

        AddParticle(_playerPos + dir * 28.0f, dir * 120.0f, PolarityColor(_playerPolarity), 7.0f, 0.14f);
        PlaySfx(_playerPolarity == 0 ? 760.0f : 610.0f, -120.0f, 0.05f, 0.1f, 0.01f, 1);
    }

    private void FireSolShot()
    {
        int count = 4 + _solBloom + Math.Max(0, _multiShot - 1);
        float spread = 0.13f + _solBloom * 0.012f;
        Color color = PolarityColor(_playerPolarity);
        for (int i = 0; i < count; i++)
        {
            float offset = (i - (count - 1) * 0.5f) * spread;
            Vector2 dir = _aimDir.Rotated(offset);
            SpawnPlayerShot(_playerPos + dir * 36.0f, dir, _riftNeedle ? 1180.0f : 1040.0f, _riftNeedle ? 5.0f : 6.2f, (13.5f + _solForge * 2.0f + (_riftNeedle ? 2.0f : 0.0f)) * _damageMultiplier, 0.62f + _solBloom * 0.025f, _riftNeedle ? 1 : 0, _riftNeedle);
        }

        AddParticle(_playerPos + _aimDir * 28.0f, _aimDir * 160.0f, color, 12.0f, 0.16f);
        PlaySfx(_playerPolarity == 0 ? 590.0f : 470.0f, -150.0f, 0.07f, 0.16f, 0.02f, 1);
    }

    private void SpawnPlayerShot(Vector2 pos, Vector2 dir, float speed, float radius, float damage, float life, int pierce, bool rift, int polarity = -1)
    {
        Shot? shot = AddShot(true);
        if (shot == null)
        {
            return;
        }

        dir = dir.LengthSquared() < 0.01f ? Vector2.Right : dir.Normalized();
        shot.Pos = pos;
        shot.Prev = _playerPos;
        shot.Vel = dir * speed;
        shot.Radius = radius;
        int shotPolarity = polarity >= 0 ? polarity : _playerPolarity;
        shot.Damage = damage * PlayerShotDamageScale(shotPolarity);
        shot.Life = life;
        shot.MaxLife = life;
        shot.Polarity = shotPolarity;
        shot.Pierce = pierce;
        shot.Rift = rift;
        ResetShotTrail(shot, shot.Pos);
    }

    private void FirePolarityStorm()
    {
        int count = 8 + _polarityStorm * 2;
        Color color = PolarityColor(_playerPolarity);
        for (int i = 0; i < count; i++)
        {
            Vector2 dir = Vector2.Right.Rotated(_time * 0.6f + i * Mathf.Tau / count);
            Shot? shot = AddShot(true);
            if (shot == null)
            {
                continue;
            }

            shot.Pos = _playerPos + dir * 42.0f;
            shot.Prev = _playerPos;
            shot.Vel = dir * (780.0f + _polarityStorm * 65.0f);
            shot.Radius = 5.0f;
            shot.Damage = (7.5f + _polarityStorm * 1.6f) * _damageMultiplier * PlayerShotDamageScale(_playerPolarity);
            shot.Life = 0.78f;
            shot.MaxLife = 0.78f;
            shot.Polarity = _playerPolarity;
            shot.Pierce = 0;
            shot.Rift = true;
            ResetShotTrail(shot, shot.Pos);
        }
        Burst(_playerPos, color, 12 + _polarityStorm * 4, 340.0f, 0.55f);
    }

    private void UpdateEnemies(float dt)
    {
        _spawnDirector += dt;
        float threat = ThreatLevel();
        int sector = CurrentSectorIndex();
        for (int i = _enemies.Count - 1; i >= 0; i--)
        {
            Enemy enemy = _enemies[i];
            enemy.SpawnPulse = Approach(enemy.SpawnPulse, 0.0f, dt * 2.8f);
            enemy.Phase += dt * (0.7f + threat * 0.05f);
            enemy.Cooldown -= dt * EnemyCooldownRate(enemy);
            enemy.Overheat = Mathf.Max(0.0f, enemy.Overheat - dt);
            enemy.ContactTimer -= dt;

            if (enemy.Kind == EnemyKind.Boss)
            {
                UpdateBoss(enemy, dt);
                continue;
            }

            Vector2 toPlayer = _playerPos - enemy.Pos;
            float distance = Mathf.Max(toPlayer.Length(), 1.0f);
            Vector2 dir = toPlayer / distance;
            Vector2 desired = Vector2.Zero;
            float speed = _enemySlow;

            switch (enemy.Kind)
            {
                case EnemyKind.Chaser:
                    desired = dir * (185.0f + threat * 9.0f) * speed;
                    if (enemy.Cooldown <= 0.0f)
                    {
                        FireEnemy(enemy, dir, 310.0f + threat * 14.0f, 1, 0.0f);
                        enemy.Cooldown = _rng.RandfRange(1.35f, 2.2f);
                    }
                    break;
                case EnemyKind.Weaver:
                    desired = (dir.Rotated(Mathf.Sin(enemy.Phase) * 1.2f) * 150.0f + dir.Orthogonal() * Mathf.Sin(enemy.Phase * 2.0f) * 120.0f) * speed;
                    if (enemy.Cooldown <= 0.0f)
                    {
                        FireEnemy(enemy, dir, 360.0f + threat * 18.0f, 3, 0.22f);
                        enemy.Cooldown = _rng.RandfRange(1.65f, 2.55f);
                    }
                    break;
                case EnemyKind.Turret:
                    desired = distance < 430.0f ? -dir * 95.0f : dir * 58.0f;
                    if (enemy.Cooldown <= 0.0f)
                    {
                        int petals = ScaledEnemyPatternCount(enemy.Kind, 7 + CurrentWaveInSector() / 2 + sector);
                        float baseAngle = enemy.Phase;
                        for (int a = 0; a < petals; a++)
                        {
                            FireEnemy(enemy, Vector2.Right.Rotated(baseAngle + Mathf.Tau * a / petals), 260.0f + threat * 12.0f, 1, 0.0f);
                        }
                        enemy.Cooldown = 2.35f;
                        enemy.Polarity = 1 - enemy.Polarity;
                    }
                    break;
                case EnemyKind.Splitter:
                    desired = (dir * 118.0f + new Vector2(Mathf.Sin(enemy.Phase * 1.7f), Mathf.Cos(enemy.Phase * 1.1f)) * 90.0f) * speed;
                    if (enemy.Cooldown <= 0.0f)
                    {
                        FireEnemy(enemy, dir, 330.0f + threat * 15.0f, 2, 0.16f);
                        enemy.Cooldown = _rng.RandfRange(1.2f, 1.9f);
                    }
                    break;
                case EnemyKind.Lance:
                    desired = distance > 640.0f ? dir * 210.0f : -dir * 100.0f + dir.Orthogonal() * Mathf.Sin(enemy.Phase * 2.3f) * 145.0f;
                    if (enemy.Cooldown <= 0.0f)
                    {
                        FireEnemy(enemy, dir, 620.0f + threat * 20.0f, 1, 0.0f, 18.0f);
                        enemy.Cooldown = _rng.RandfRange(1.0f, 1.55f);
                    }
                    break;
                case EnemyKind.Mine:
                    desired = (dir * 48.0f + RandomOrbit(enemy.Phase) * 34.0f) * speed;
                    if (distance < 145.0f && enemy.Cooldown > EnemyTelegraphLead)
                    {
                        enemy.Cooldown = EnemyTelegraphLead;
                    }
                    if (enemy.Cooldown <= 0.0f)
                    {
                        int spokes = ScaledEnemyPatternCount(enemy.Kind, enemy.Elite ? 12 : 8);
                        for (int a = 0; a < spokes; a++)
                        {
                            FireEnemy(enemy, Vector2.Right.Rotated(enemy.Phase + a * Mathf.Tau / spokes), 235.0f + threat * 7.0f, 1, 0.0f, enemy.Elite ? 12.0f : 9.0f);
                        }
                        enemy.Cooldown = enemy.Elite ? 1.4f : 2.1f;
                    }
                    break;
                case EnemyKind.Shard:
                    desired = (dir.Rotated(Mathf.Sin(enemy.Phase * 1.9f) * 0.85f) * (245.0f + threat * 4.0f) + dir.Orthogonal() * Mathf.Sin(enemy.Phase * 3.1f) * 180.0f) * speed;
                    if (enemy.Cooldown <= 0.0f)
                    {
                        FireEnemy(enemy, dir, 560.0f + threat * 17.0f, enemy.Elite ? 3 : 2, 0.09f, 7.0f);
                        enemy.Cooldown = enemy.Elite ? 0.78f : 1.12f;
                    }
                    break;
                case EnemyKind.Warden:
                    desired = distance < 520.0f ? -dir * 72.0f : dir * 78.0f;
                    if (enemy.Cooldown <= 0.0f)
                    {
                        FireEnemy(enemy, dir, 300.0f + threat * 10.0f, 5, 0.22f, 11.0f);
                        if (_enemies.Count < 26 + sector * 4)
                        {
                            SpawnEnemy(sector >= 3 ? EnemyKind.Shard : EnemyKind.Chaser, ClampToArena(enemy.Pos + RandomDirection() * 78.0f, 34.0f), 1 - enemy.Polarity, 1);
                        }
                        enemy.Cooldown = enemy.Elite ? 1.25f : 1.85f;
                    }
                    break;
                case EnemyKind.Drifter:
                    desired = (dir * 94.0f + dir.Orthogonal() * Mathf.Sin(enemy.Phase * 1.8f) * 245.0f) * speed;
                    if (enemy.Cooldown <= 0.0f)
                    {
                        FireEnemy(enemy, dir.Rotated(0.34f), 330.0f + threat * 12.0f, 2, 0.12f);
                        FireEnemy(enemy, dir.Rotated(-0.34f), 330.0f + threat * 12.0f, 2, 0.12f);
                        enemy.Cooldown = _rng.RandfRange(1.45f, 2.1f);
                    }
                    break;
                case EnemyKind.Bulwark:
                    desired = distance > 380.0f ? dir * 78.0f * speed : -dir * 28.0f;
                    if (enemy.Cooldown <= 0.0f)
                    {
                        FireEnemy(enemy, dir, 245.0f + threat * 8.0f, enemy.Elite ? 5 : 3, 0.3f, 12.0f);
                        enemy.Cooldown = enemy.Elite ? 1.8f : 2.45f;
                    }
                    break;
                case EnemyKind.Siren:
                    desired = (distance < 560.0f ? -dir * 88.0f : dir * 54.0f) + dir.Orthogonal() * Mathf.Sin(enemy.Phase * 1.35f) * 126.0f;
                    desired *= speed;
                    if (enemy.Cooldown <= 0.0f)
                    {
                        enemy.Polarity = 1 - enemy.Polarity;
                        int rings = ScaledEnemyPatternCount(enemy.Kind, enemy.Elite ? 10 : 7);
                        float spin = enemy.Phase;
                        for (int a = 0; a < rings; a++)
                        {
                            FireEnemy(enemy, Vector2.Right.Rotated(spin + a * Mathf.Tau / rings), 220.0f + threat * 7.0f, 1, 0.0f, 8.0f);
                        }
                        enemy.Cooldown = enemy.Elite ? 1.6f : 2.35f;
                    }
                    break;
                case EnemyKind.Harrier:
                    desired = dir.Rotated(Mathf.Sin(enemy.Phase * 2.6f) * 0.42f) * (292.0f + threat * 5.0f) * speed;
                    if (distance < 180.0f)
                    {
                        desired = -dir * 175.0f * speed;
                    }
                    if (enemy.Cooldown <= 0.0f)
                    {
                        FireEnemy(enemy, dir, 455.0f + threat * 14.0f, enemy.Elite ? 3 : 2, 0.1f, 7.0f);
                        enemy.Cooldown = enemy.Elite ? 0.82f : 1.18f;
                    }
                    break;
            }

            if (EnemyIsCharging(enemy))
            {
                float charge = EnemyTelegraph01(enemy);
                enemy.Vel = enemy.Vel.Lerp(Vector2.Zero, 1.0f - Mathf.Exp(-dt * (9.0f + charge * 10.0f)));
            }
            else
            {
                enemy.Vel = enemy.Vel.Lerp(desired, 1.0f - Mathf.Exp(-dt * 4.0f));
            }
            enemy.Pos += enemy.Vel * dt;
            enemy.Pos = ClampToArena(enemy.Pos, enemy.Radius);
        }
    }

    private void UpdateBoss(Enemy boss, float dt)
    {
        _bossPatternTimer += dt;
        int sector = CurrentSectorIndex();
        float threat = ThreatLevel();
        float hpRatio = Mathf.Clamp(boss.Hp / boss.MaxHp, 0.0f, 1.0f);
        float targetX = ScreenCenter.X + Mathf.Sin(_time * (0.78f + sector * 0.05f)) * (520.0f - sector * 24.0f) + Mathf.Sin(_time * 1.7f) * 110.0f;
        float targetY = Arena.Position.Y + 150.0f + sector * 18.0f + Mathf.Sin(_time * 1.1f) * 44.0f;
        Vector2 desired = new Vector2(targetX, targetY) - boss.Pos;
        if (EnemyIsCharging(boss))
        {
            float charge = EnemyTelegraph01(boss);
            boss.Vel = boss.Vel.Lerp(Vector2.Zero, 1.0f - Mathf.Exp(-dt * (7.0f + charge * 8.0f)));
        }
        else
        {
            boss.Vel = boss.Vel.Lerp(desired * 1.5f, 1.0f - Mathf.Exp(-dt * 1.9f));
        }
        boss.Pos += boss.Vel * dt;
        boss.Pos = ClampToArena(boss.Pos, boss.Radius);

        if (boss.Cooldown <= 0.0f)
        {
            Vector2 dir = (_playerPos - boss.Pos).Normalized();
            int pattern = ((int)(_bossPatternTimer / Mathf.Max(1.25f, 2.0f - sector * 0.12f))) % (4 + Math.Min(3, sector));
            if (pattern == 0)
            {
                int count = ScaledEnemyPatternCount(boss.Kind, 9 + sector * 2);
                for (int i = 0; i < count; i++)
                {
                    float offset = (i - (count - 1) * 0.5f) * 0.105f;
                    FireEnemy(boss, dir.Rotated(offset), 360.0f + threat * 6.0f + (1.0f - hpRatio) * 180.0f, 1, 0.0f, 12.0f);
                }
                boss.Cooldown = 0.9f - (1.0f - hpRatio) * 0.22f;
            }
            else if (pattern == 1)
            {
                int count = ScaledEnemyPatternCount(boss.Kind, 18 + sector * 3);
                float spin = _time * (hpRatio > 0.5f ? 0.6f : -1.0f);
                for (int i = 0; i < count; i++)
                {
                    FireEnemy(boss, Vector2.Right.Rotated(spin + i * Mathf.Tau / count), 250.0f + sector * 18.0f + (1.0f - hpRatio) * 100.0f, 1, 0.0f, 9.0f);
                }
                boss.Cooldown = 1.15f;
                boss.Polarity = 1 - boss.Polarity;
            }
            else if (pattern == 2)
            {
                int count = ScaledEnemyPatternCount(boss.Kind, 5);
                for (int i = 0; i < count; i++)
                {
                    float offset = (i - (count - 1) * 0.5f) * 0.36f;
                    FireEnemy(boss, dir.Rotated(offset), 520.0f, 1, 0.0f, 16.0f);
                }
                boss.Cooldown = 0.78f;
            }
            else if (pattern == 3)
            {
                if (_enemies.Count < 13)
                {
                    SpawnEnemy(sector >= 2 ? EnemyKind.Shard : EnemyKind.Weaver, boss.Pos + RandomDirection() * 110.0f, boss.Polarity);
                    SpawnEnemy(sector >= 3 ? EnemyKind.Mine : EnemyKind.Chaser, boss.Pos + RandomDirection() * 130.0f, 1 - boss.Polarity);
                }
                boss.Cooldown = 1.65f;
            }
            else if (pattern == 4)
            {
                SpawnHazardLine(sector, true);
                int count = ScaledEnemyPatternCount(boss.Kind, 5);
                for (int i = 0; i < count; i++)
                {
                    float offset = (i - (count - 1) * 0.5f) * 0.24f;
                    FireEnemy(boss, dir.Rotated(offset), 460.0f + threat * 10.0f, 1, 0.0f, 13.0f);
                }
                boss.Cooldown = 1.05f;
            }
            else if (pattern == 5)
            {
                int count = ScaledEnemyPatternCount(boss.Kind, 10 + sector * 2);
                for (int i = 0; i < count; i++)
                {
                    Vector2 spiral = Vector2.Right.Rotated(-_time * 1.4f + i * Mathf.Tau / count);
                    FireEnemy(boss, spiral, 330.0f + i * 8.0f, 1, 0.0f, 8.0f);
                }
                boss.Cooldown = 0.7f;
            }
            else
            {
                if (_enemies.Count < 18 + sector * 4)
                {
                    SpawnEnemy(EnemyKind.Warden, ClampToArena(boss.Pos + RandomDirection() * 170.0f, 70.0f), boss.Polarity);
                }
                boss.Cooldown = 1.3f;
            }
        }
    }

    private void FireEnemy(Enemy enemy, Vector2 direction, float speed, int count, float spread, float radius = EnemyBulletRadius)
    {
        if (direction.LengthSquared() < 0.01f)
        {
            direction = Vector2.Down;
        }
        direction = direction.Normalized();
        EnterOverheat(enemy);
        int shotCount = ScaledEnemyBulletCount(enemy.Kind, count);
        float shotSpeed = ScaledEnemyBulletSpeed(enemy.Kind, speed);
        float shotLife = EnemyBulletLife();
        for (int i = 0; i < shotCount; i++)
        {
            float offset = shotCount == 1 ? 0.0f : (i - (shotCount - 1) * 0.5f) * spread;
            Vector2 dir = direction.Rotated(offset);
            Shot? shot = AddShot(false);
            if (shot == null)
            {
                continue;
            }

            shot.Pos = enemy.Pos + dir * (enemy.Radius + 12.0f);
            shot.Prev = enemy.Pos;
            shot.Vel = dir * shotSpeed;
            shot.Radius = radius;
            shot.Damage = enemy.Kind == EnemyKind.Boss ? 18.0f + CurrentSectorIndex() * 2.0f : 9.0f + ThreatLevel() * 0.65f;
            shot.Life = shotLife;
            shot.MaxLife = shotLife;
            shot.Polarity = -1;
            shot.Pierce = 0;
            shot.Rift = false;
            ResetShotTrail(shot, shot.Pos);
        }
    }

    private void UpdateShots(float dt)
    {
        for (int i = _shots.Count - 1; i >= 0; i--)
        {
            Shot shot = _shots[i];
            shot.Prev = shot.Pos;
            PushShotTrail(shot, shot.Pos);
            shot.Pos += shot.Vel * dt;
            shot.Life -= dt;

            if (!shot.FromPlayer && _enemySlow < 0.99f)
            {
                shot.Vel *= 1.0f - dt * 0.06f;
            }

            if (shot.Life <= 0.0f || !ExpandedArena(180.0f).HasPoint(shot.Pos))
            {
                RemoveShotAt(i);
            }
        }
    }

    private void ResolveCombat(float dt)
    {
        for (int i = _shots.Count - 1; i >= 0; i--)
        {
            Shot shot = _shots[i];
            if (shot.FromPlayer)
            {
                bool removeShot = false;
                for (int j = _enemies.Count - 1; j >= 0; j--)
                {
                    Enemy enemy = _enemies[j];
                    float hitRadius = shot.Radius + enemy.Radius;
                    float dx = Mathf.Abs(shot.Pos.X - enemy.Pos.X);
                    float dy = Mathf.Abs(shot.Pos.Y - enemy.Pos.Y);
                    if (dx <= hitRadius && dy <= hitRadius && shot.Pos.DistanceSquaredTo(enemy.Pos) <= hitRadius * hitRadius)
                    {
                        bool assaultShot = shot.Polarity == 1;
                        bool overheated = EnemyOverheat01(enemy) > 0.0f;
                        float windowBonus = overheated ? (assaultShot ? _critMultiplier + (enemy.Elite ? 0.18f : 0.0f) : 1.08f) : (assaultShot ? 1.08f : 0.96f);
                        float damage = shot.Damage * windowBonus * _nextWaveDamageBoost;
                        bool lethal = enemy.Hp - damage <= 0.0f;
                        DamageEnemy(enemy, damage, shot.Pos, false);
                        if (!assaultShot)
                        {
                            AddCruiseCharge(overheated ? 1.2f : 1.9f, enemy.Pos);
                        }
                        else if (overheated && lethal && _assaultBurstTimer > 0.0f)
                        {
                            _assaultBurstTimer = Mathf.Min(AssaultBurstMax, _assaultBurstTimer + 0.26f);
                        }
                        float textChance = _visualPressure > 0.86f ? 0.04f : _visualPressure > 0.68f ? 0.1f : 0.22f;
                        if (_damageTexts.Count < MaxDamageTexts * 0.6f || _rng.Randf() < textChance)
                        {
                            string damageLabel = overheated && assaultShot ? $"{(int)damage}!" : ((int)damage).ToString();
                            AddText(damageLabel, enemy.Pos + RandomDirection() * 26.0f, PolarityColor(shot.Polarity), overheated && assaultShot ? 23.0f : 20.0f);
                        }
                        if (overheated && assaultShot && _counterTextCooldown <= 0.0f)
                        {
                            AddText(CounterText(), enemy.Pos + new Vector2(0.0f, -60.0f), Gold, 19.0f);
                            _counterTextCooldown = 0.42f;
                        }
                        Burst(shot.Pos, PolarityColor(shot.Polarity), shot.Rift ? 8 : 5, shot.Rift ? 360.0f : 210.0f, 0.42f);
                        _energy = Mathf.Clamp(_energy + (overheated && assaultShot ? 2.0f : 0.75f) * _absorbEfficiency, 0.0f, _maxEnergy);

                        if (shot.Pierce > 0)
                        {
                            shot.Pierce--;
                            shot.Damage *= 0.72f;
                        }
                        else
                        {
                            removeShot = true;
                        }
                        break;
                    }
                }

                if (removeShot)
                {
                    RemoveShotAt(i);
                }
            }
            else
            {
                float hitRadius = shot.Radius + PlayerRadius;
                float distanceSquared = shot.Pos.DistanceSquaredTo(_playerPos);
                if (!shot.Grazed && _playerPolarity == 0 && _dashTimer <= 0.0f)
                {
                    float grazeRadius = hitRadius + CruiseGrazeRadius;
                    if (distanceSquared <= grazeRadius * grazeRadius && distanceSquared > hitRadius * hitRadius)
                    {
                        shot.Grazed = true;
                        AddCruiseCharge(5.2f, shot.Pos);
                        _energy = Mathf.Clamp(_energy + 1.15f * _absorbEfficiency, 0.0f, _maxEnergy);
                        _score += 8 + Math.Min(_combo, 40);
                        _combo++;
                        RefreshRunBestCombo();
                        AddObjectiveProgress(RunObjectiveKind.AbsorbBullets, 1);
                        _comboTimer = 2.0f;
                        if (_absorbTextCooldown <= 0.0f)
                        {
                            AddText(ChargeText(), _playerPos + new Vector2(0.0f, -72.0f), PolarityBlue, 18.0f);
                            _absorbTextCooldown = 0.22f;
                        }
                    }
                }

                if (distanceSquared <= hitRadius * hitRadius)
                {
                    if (_dashTimer > 0.0f)
                    {
                        float absorbGain = 4.0f * _absorbEfficiency;
                        _energy = Mathf.Clamp(_energy + absorbGain, 0.0f, _maxEnergy);
                        _score += 15 + _combo;
                        _combo++;
                        RefreshRunBestCombo();
                        AddObjectiveProgress(RunObjectiveKind.AbsorbBullets, 1);
                        _comboTimer = 2.0f;
                        if (_absorbTextCooldown <= 0.0f)
                        {
                            AddText(AbsorbText(absorbGain), _playerPos + new Vector2(0.0f, -72.0f), EnemyBulletColor(), 20.0f);
                            _absorbTextCooldown = 0.18f;
                        }
                        Burst(shot.Pos, EnemyBulletColor(), 8, 280.0f, 0.55f);
                        PlaySfx(740.0f, -360.0f, 0.08f, 0.12f, 0.02f, 2);
                    }
                    else
                    {
                        float incoming = shot.Damage * (_playerPolarity == 0 ? 0.78f : 1.08f);
                        if (_playerPolarity == 0)
                        {
                            AddCruiseCharge(3.0f, shot.Pos);
                        }
                        DamagePlayer(incoming, shot.Pos);
                    }
                    RemoveShotAt(i);
                }
            }
        }

        for (int i = _enemies.Count - 1; i >= 0; i--)
        {
            Enemy enemy = _enemies[i];
            float hitRadius = enemy.Radius + PlayerRadius;
            if (enemy.Pos.DistanceSquaredTo(_playerPos) <= hitRadius * hitRadius)
            {
                if (_dashTimer > 0.0f)
                {
                    DamageEnemy(enemy, _dashDamage * _damageMultiplier, _playerPos, false);
                    Burst(enemy.Pos, PolarityColor(_playerPolarity), 12, 420.0f, 0.55f);
                }
                else if (enemy.ContactTimer <= 0.0f)
                {
                    DamagePlayer(enemy.Kind == EnemyKind.Boss ? 23.0f : 11.0f + CurrentSectorIndex() * 1.6f, enemy.Pos);
                    enemy.ContactTimer = 0.6f;
                }
            }
        }
    }

    private void DamageEnemy(Enemy enemy, float damage, Vector2 source, bool heavy)
    {
        enemy.Hp -= damage / Mathf.Max(0.1f, enemy.Armor);
        Vector2 knock = enemy.Pos - source;
        if (knock.LengthSquared() > 0.01f)
        {
            enemy.Vel += knock.Normalized() * (heavy ? 180.0f : 46.0f);
        }

        if (enemy.Hp <= 0.0f)
        {
            KillEnemy(enemy);
        }
    }

    private void KillEnemy(Enemy enemy)
    {
        if (!DetachEnemy(enemy))
        {
            return;
        }

        Color color = EnemyStateColor(enemy);
        Burst(enemy.Pos, color, enemy.Kind == EnemyKind.Boss ? 180 : 28, enemy.Kind == EnemyKind.Boss ? 980.0f : 440.0f, enemy.Kind == EnemyKind.Boss ? 2.2f : 0.9f);
        _shake = Mathf.Max(_shake, enemy.Kind == EnemyKind.Boss ? 1.0f : 0.24f);
        _flash = Mathf.Max(_flash, enemy.Kind == EnemyKind.Boss ? 0.75f : 0.12f);
        _combo++;
        RefreshRunBestCombo();
        _comboTimer = 2.7f;
        _score += enemy.Value * Math.Max(1, _combo / 4);
        _energy = Mathf.Clamp(_energy + 6.0f, 0.0f, _maxEnergy);
        _runKills++;
        AddObjectiveProgress(RunObjectiveKind.DefeatEnemies, 1);
        AddText($"+{enemy.Value}", enemy.Pos + new Vector2(0.0f, -38.0f), color, 24.0f);
        if (enemy.Kind == EnemyKind.Boss)
        {
            _runBossKills++;
            AddObjectiveProgress(RunObjectiveKind.DefeatBosses, 1);
            AddText(Tf("sector.cleared", T(CurrentSector().NameKey)), ScreenCenter + new Vector2(0.0f, -225.0f), CurrentSector().Accent, 36.0f);
        }
        PlaySfx(enemy.Kind == EnemyKind.Boss ? 64.0f : 180.0f, enemy.Kind == EnemyKind.Boss ? 0.2f : -50.0f, enemy.Kind == EnemyKind.Boss ? 1.4f : 0.16f, enemy.Kind == EnemyKind.Boss ? 0.62f : 0.28f, enemy.Kind == EnemyKind.Boss ? 0.38f : 0.08f, enemy.Kind == EnemyKind.Boss ? 0 : 2);

        int drops = enemy.Kind == EnemyKind.Boss ? 32 + CurrentSectorIndex() * 6 : 2 + (enemy.Kind == EnemyKind.Splitter ? 2 : 0) + (enemy.Elite ? 4 : 0);
        for (int i = 0; i < drops; i++)
        {
            SpawnPickup(enemy.Pos, i % 5 == 0 ? PickupKind.Energy : PickupKind.Dust);
        }
        if (_rng.Randf() < 0.08f + _leechChance + (CurrentSectorIndex() == 2 ? 0.06f : 0.0f) + (enemy.Elite ? 0.05f : 0.0f))
        {
            SpawnPickup(enemy.Pos, PickupKind.Repair);
        }

        if (enemy.Kind == EnemyKind.Splitter && enemy.SplitDepth < 1)
        {
            SpawnEnemy(EnemyKind.Chaser, ClampToArena(enemy.Pos + new Vector2(-42.0f, 36.0f), 30.0f), 1 - enemy.Polarity, enemy.SplitDepth + 1);
            SpawnEnemy(EnemyKind.Chaser, ClampToArena(enemy.Pos + new Vector2(42.0f, 36.0f), 30.0f), enemy.Polarity, enemy.SplitDepth + 1);
        }
        RecycleEnemy(enemy);
    }

    private void DamagePlayer(float amount, Vector2 source)
    {
        if (_invulnTimer > 0.0f)
        {
            return;
        }

        _waveTookDamage = true;
        float finalDamage = amount * _mirrorReduction;
        _playerHp -= finalDamage;
        _timeSinceHit = 0.0f;
        _invulnTimer = 0.86f;
        _shake = Mathf.Max(_shake, 0.72f);
        _flash = Mathf.Max(_flash, 0.45f);
        Burst(_playerPos, Rose, 34, 470.0f, 1.0f);
        AddText($"-{(int)finalDamage}", _playerPos + new Vector2(0.0f, -60.0f), Rose, 34.0f);
        PlaySfx(92.0f, -20.0f, 0.26f, 0.45f, 0.2f, 0);

        Vector2 knock = _playerPos - source;
        if (knock.LengthSquared() > 0.01f)
        {
            _playerVel += knock.Normalized() * 620.0f;
        }

        if (_playerHp <= 0.0f)
        {
            LoseRun();
        }
    }

    private void UpdatePickups(float dt)
    {
        for (int i = _pickups.Count - 1; i >= 0; i--)
        {
            Pickup pickup = _pickups[i];
            pickup.Life -= dt;
            Vector2 toPlayer = _playerPos - pickup.Pos;
            float distance = toPlayer.Length();
            float magnet = _pickupMagnet + (_energy / _maxEnergy) * 80.0f;
            if (distance < magnet && distance > 1.0f)
            {
                pickup.Vel = pickup.Vel.Lerp(toPlayer.Normalized() * Mathf.Lerp(240.0f, 840.0f, 1.0f - distance / magnet), 1.0f - Mathf.Exp(-dt * 9.0f));
            }
            pickup.Pos += pickup.Vel * dt;
            pickup.Vel *= 1.0f - dt * 2.1f;

            if (distance <= pickup.Radius + PlayerRadius)
            {
                CollectPickup(pickup);
                RemovePickupAt(i);
            }
            else if (pickup.Life <= 0.0f)
            {
                RemovePickupAt(i);
            }
        }
    }

    private void SpawnPickup(Vector2 pos, PickupKind kind)
    {
        float radius = kind == PickupKind.Repair ? 12.0f : 8.0f;
        Pickup? pickup = AddPickup();
        if (pickup == null)
        {
            return;
        }

        pickup.Kind = kind;
        pickup.Pos = pos;
        pickup.Vel = RandomDirection() * _rng.RandfRange(80.0f, 240.0f);
        pickup.Radius = radius;
        pickup.Life = kind == PickupKind.Repair ? 9.0f : 7.0f;
    }

    private void CollectPickup(Pickup pickup)
    {
        _runPickups++;
        AddObjectiveProgress(RunObjectiveKind.CollectPickups, 1);

        switch (pickup.Kind)
        {
            case PickupKind.Dust:
                _score += 30 + _combo;
                _energy = Mathf.Clamp(_energy + 2.5f, 0.0f, _maxEnergy);
                break;
            case PickupKind.Energy:
                _score += 50 + _combo;
                _energy = Mathf.Clamp(_energy + 12.0f, 0.0f, _maxEnergy);
                break;
            case PickupKind.Repair:
                _score += 80;
                _playerHp = Mathf.Clamp(_playerHp + 18.0f, 0.0f, _playerMaxHp);
                AddText(T("repair"), _playerPos + new Vector2(0.0f, -70.0f), Jade, 22.0f);
                break;
        }

        _comboTimer = 2.0f;
        Burst(pickup.Pos, PickupColor(pickup.Kind), 5, 180.0f, 0.38f);
    }

    private void UpdateParticles(float dt)
    {
        for (int i = _particles.Count - 1; i >= 0; i--)
        {
            Particle p = _particles[i];
            p.Life -= dt;
            p.Pos += p.Vel * dt;
            p.Vel *= 1.0f - dt * 1.7f;
            p.Spin += dt;
            if (p.Life <= 0.0f)
            {
                RemoveParticleAt(i);
            }
        }
    }

    private void UpdateDamageTexts(float dt)
    {
        for (int i = _damageTexts.Count - 1; i >= 0; i--)
        {
            DamageText text = _damageTexts[i];
            text.Life -= dt;
            text.Pos += new Vector2(0.0f, -42.0f) * dt;
            if (text.Life <= 0.0f)
            {
                RemoveDamageTextAt(i);
            }
        }
    }

    private void UpdateSectorHazards(float dt)
    {
        int sector = CurrentSectorIndex();
        if (sector > 0 && _mode == GameMode.Playing)
        {
            _sectorHazardTimer -= dt;
            if (_sectorHazardTimer <= 0.0f)
            {
                SpawnHazardLine(sector, false);
                _sectorHazardTimer = Mathf.Max(1.7f, 7.2f - sector * 1.05f - CurrentWaveInSector() * 0.08f);
            }
        }

        for (int i = _hazards.Count - 1; i >= 0; i--)
        {
            HazardLine hazard = _hazards[i];
            hazard.Life -= dt;
            bool active = hazard.Life < hazard.MaxLife - hazard.Warmup;
            if (active && _invulnTimer <= 0.0f)
            {
                float distance = DistanceToSegment(_playerPos, hazard.A, hazard.B);
                if (distance < hazard.Width + PlayerRadius)
                {
                    if (_dashTimer > 0.0f)
                    {
                        float absorbGain = 4.0f * _absorbEfficiency;
                        _energy = Mathf.Clamp(_energy + absorbGain, 0.0f, _maxEnergy);
                        AddObjectiveProgress(RunObjectiveKind.AbsorbBullets, 3);
                        if (_absorbTextCooldown <= 0.0f)
                        {
                            AddText(AbsorbText(absorbGain), _playerPos + new Vector2(0.0f, -80.0f), EnemyBulletColor(), 22.0f);
                            _absorbTextCooldown = 0.25f;
                        }
                        Burst(_playerPos, EnemyBulletColor(), 10, 260.0f, 0.45f);
                        _invulnTimer = 0.18f;
                    }
                    else
                    {
                        if (_playerPolarity == 0)
                        {
                            AddCruiseCharge(6.0f, (hazard.A + hazard.B) * 0.5f);
                        }
                        DamagePlayer(hazard.Damage * (_playerPolarity == 0 ? 0.82f : 1.08f), (hazard.A + hazard.B) * 0.5f);
                    }
                }
            }

            if (hazard.Life <= 0.0f)
            {
                _hazards.RemoveAt(i);
            }
        }
    }

    private void SpawnHazardLine(int sector, bool bossCast)
    {
        if (_hazards.Count > 10)
        {
            _hazards.RemoveAt(0);
        }

        bool vertical = _rng.Randf() < 0.5f;
        float x = _rng.RandfRange(Arena.Position.X + 90.0f, Arena.End.X - 90.0f);
        float y = _rng.RandfRange(Arena.Position.Y + 90.0f, Arena.End.Y - 90.0f);
        Vector2 a;
        Vector2 b;

        if (sector == 3)
        {
            float angle = _rng.RandfRange(-0.85f, 0.85f) + (bossCast ? Mathf.Pi * 0.5f : 0.0f);
            Vector2 dir = Vector2.Right.Rotated(angle);
            Vector2 center = new(x, y);
            a = center - dir * 1150.0f;
            b = center + dir * 1150.0f;
        }
        else if (vertical)
        {
            a = new Vector2(x, Arena.Position.Y - 90.0f);
            b = new Vector2(x + _rng.RandfRange(-120.0f, 120.0f), Arena.End.Y + 90.0f);
        }
        else
        {
            a = new Vector2(Arena.Position.X - 90.0f, y);
            b = new Vector2(Arena.End.X + 90.0f, y + _rng.RandfRange(-120.0f, 120.0f));
        }

        int polarity = (sector + _rng.RandiRange(0, 1)) % 2;
        Color color = sector >= 4 ? Rose : EnemyBulletColor().Lerp(CurrentSector().Accent, 0.2f);
        _hazards.Add(new HazardLine
        {
            A = a,
            B = b,
            Color = color,
            Life = bossCast ? 1.6f : 1.35f,
            MaxLife = bossCast ? 1.6f : 1.35f,
            Warmup = bossCast ? 0.72f : 0.82f,
            Width = bossCast ? 32.0f + sector * 5.0f : 24.0f + sector * 4.0f,
            Damage = 18.0f + sector * 5.0f,
            Polarity = polarity,
        });
    }

    private void UpdateOrbiters(float dt)
    {
        if (_orbiters <= 0)
        {
            return;
        }

        _orbiterFireTimer -= dt;
        if (_orbiterFireTimer > 0.0f)
        {
            return;
        }

        float interval = OrbiterFireInterval();
        _orbiterFireTimer = Mathf.Max(_orbiterFireTimer + interval, interval * 0.45f);

        int shotsToFire = _runPilot == PilotKind.Kairo ? _orbiters : Math.Min(_orbiters, 1 + _orbiters / 3);
        int fired = 0;
        for (int i = 0; i < _orbiters && fired < shotsToFire; i++)
        {
            Vector2 origin = OrbiterPosition(_playerPos, i, _orbiters);
            Enemy? target = FindNearestEnemy(origin, _runPilot == PilotKind.Kairo ? 820.0f : 680.0f);
            if (target == null)
            {
                continue;
            }

            FireOrbiterShot(origin, target);
            fired++;
        }
    }

    private float OrbiterFireInterval()
    {
        float interval = _runPilot == PilotKind.Kairo ? 0.82f : 0.98f;
        interval -= Mathf.Min(0.32f, _kairoSync * 0.05f);
        interval -= Mathf.Min(0.08f, _orbiters * 0.01f);
        return Mathf.Max(_runPilot == PilotKind.Kairo ? 0.4f : 0.56f, interval);
    }

    private void FireOrbiterShot(Vector2 origin, Enemy target)
    {
        Vector2 toTarget = target.Pos - origin;
        if (toTarget.LengthSquared() <= 1.0f)
        {
            return;
        }

        Vector2 dir = toTarget.Normalized();
        Shot? shot = AddShot(true);
        if (shot == null)
        {
            return;
        }

        shot.Pos = origin + dir * 11.0f;
        shot.Prev = origin;
        shot.Vel = dir * (_runPilot == PilotKind.Kairo ? 1160.0f + _kairoSync * 40.0f : 980.0f);
        shot.Radius = _runPilot == PilotKind.Kairo ? 5.2f : 4.8f;
        shot.Damage = (9.0f + _kairoSync * 2.2f + (_runPilot == PilotKind.Kairo ? 3.2f + _kairoOverrideMatrix * 1.9f : 0.0f)) * _damageMultiplier * PlayerShotDamageScale(_playerPolarity);
        shot.Life = 0.78f;
        shot.MaxLife = 0.78f;
        shot.Polarity = _playerPolarity;
        shot.Pierce = _runPilot == PilotKind.Kairo && (_kairoSync >= 4 || _kairoRelayProtocol >= 2) ? 1 : 0;
        shot.Rift = false;
        ResetShotTrail(shot, shot.Pos);

        AddParticle(origin, dir * 180.0f, PolarityColor(_playerPolarity), 7.0f, 0.16f);
        AddParticle(origin - dir * 6.0f, -dir * 90.0f, PickupBlue, 5.0f, 0.14f);
    }

    private Enemy? FindNearestEnemy(Vector2 from, float radius)
    {
        Enemy? best = null;
        float bestDistance = radius * radius;
        foreach (Enemy enemy in _enemies)
        {
            float distance = enemy.Pos.DistanceSquaredTo(from);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = enemy;
            }
        }
        return best;
    }

    private void ClearBulletsNear(Vector2 point, float radius, bool score)
    {
        float r2 = radius * radius;
        for (int i = _shots.Count - 1; i >= 0; i--)
        {
            Shot shot = _shots[i];
            if (!shot.FromPlayer && shot.Pos.DistanceSquaredTo(point) < r2)
            {
                if (score)
                {
                    _score += 5;
                    _energy = Mathf.Clamp(_energy + 1.2f, 0.0f, _maxEnergy);
                }
                Burst(shot.Pos, EnemyBulletColor(), 3, 160.0f, 0.3f);
                RemoveShotAt(i);
            }
        }
    }

    private void ClearEnemyBulletsInBeam(Vector2 start, Vector2 end, float width, bool score)
    {
        for (int i = _shots.Count - 1; i >= 0; i--)
        {
            Shot shot = _shots[i];
            if (shot.FromPlayer)
            {
                continue;
            }

            if (DistancePointToSegment(shot.Pos, start, end) <= width + shot.Radius)
            {
                if (score)
                {
                    _score += 5;
                    _energy = Mathf.Clamp(_energy + 1.2f, 0.0f, _maxEnergy);
                }
                Burst(shot.Pos, EnemyBulletColor(), 3, 160.0f, 0.3f);
                RemoveShotAt(i);
            }
        }
    }

    private void OpenUpgradeChoice()
    {
        _mode = GameMode.Upgrade;
        _rerollsRemaining = _baseRerolls;
        GenerateUpgradeChoices();
        ClearShots();
        Burst(ScreenCenter, Jade, 42, 360.0f, 1.2f);
        PlaySfx(330.0f, 110.0f, 0.38f, 0.28f, 0.04f, 1);
    }

    private void GenerateUpgradeChoices()
    {
        _upgradeChoices.Clear();
        List<UpgradeId> commonPool = CommonUpgradePool();
        List<UpgradeId> pilotPool = PilotUpgradePool(_runPilot);
        RemoveMaxed(commonPool);
        RemoveMaxed(pilotPool);

        int pilotSlots = Math.Min(2, pilotPool.Count);
        for (int i = 0; i < pilotSlots; i++)
        {
            AddRandomUpgradeChoice(pilotPool);
        }

        while (_upgradeChoices.Count < 4 && commonPool.Count > 0)
        {
            AddRandomUpgradeChoice(commonPool);
        }

        while (_upgradeChoices.Count < 4 && pilotPool.Count > 0)
        {
            AddRandomUpgradeChoice(pilotPool);
        }

        const float cardWidth = 342.0f;
        const float cardHeight = 386.0f;
        const float cardGap = 32.0f;
        float startX = (ScreenWidth - cardWidth * 4.0f - cardGap * 3.0f) * 0.5f;
        for (int i = 0; i < _upgradeChoices.Count; i++)
        {
            UpgradeCard card = _upgradeChoices[i];
            card.Rect = new Rect2(new Vector2(startX + i * (cardWidth + cardGap), 342.0f), new Vector2(cardWidth, cardHeight));
            _upgradeChoices[i] = card;
        }
    }

    private void AddRandomUpgradeChoice(List<UpgradeId> pool)
    {
        if (pool.Count <= 0)
        {
            return;
        }

        int index = _rng.RandiRange(0, pool.Count - 1);
        _upgradeChoices.Add(CreateCard(pool[index]));
        pool.RemoveAt(index);
    }

    private void RemoveMaxed(List<UpgradeId> pool)
    {
        for (int i = pool.Count - 1; i >= 0; i--)
        {
            if (GetRank(pool[i]) >= MaxRank(pool[i]))
            {
                pool.RemoveAt(i);
            }
        }
    }

    private static List<UpgradeId> CommonUpgradePool()
    {
        return new List<UpgradeId>
        {
            UpgradeId.PrismArray,
            UpgradeId.RailHeart,
            UpgradeId.CoolantLattice,
            UpgradeId.KineticBloom,
            UpgradeId.GravityWell,
            UpgradeId.VitalShell,
            UpgradeId.ResonanceLeech,
            UpgradeId.MoonWisp,
            UpgradeId.RiftNeedle,
            UpgradeId.MirrorSkin,
            UpgradeId.NovaCapacitor,
            UpgradeId.PolarityStorm,
            UpgradeId.CometTrail,
            UpgradeId.AegisBloom,
            UpgradeId.QuantumEcho,
            UpgradeId.SolarThesis,
            UpgradeId.EmergencyRepair,
            UpgradeId.OneWaveOverdrive,
            UpgradeId.GlassCannon,
            UpgradeId.BountyContract,
            UpgradeId.BulletTransmute,
            UpgradeId.HarmonicMap,
        };
    }

    private static List<UpgradeId> PilotUpgradePool(PilotKind pilot)
    {
        return pilot switch
        {
            PilotKind.Vesper => new List<UpgradeId> { UpgradeId.VesperCapacitor, UpgradeId.VesperSplitRail, UpgradeId.VesperJudgmentCoil, UpgradeId.VesperSeverLine },
            PilotKind.Kairo => new List<UpgradeId> { UpgradeId.KairoDroneBay, UpgradeId.KairoSwarmSync, UpgradeId.KairoOverrideMatrix, UpgradeId.KairoRelayProtocol },
            PilotKind.Sol => new List<UpgradeId> { UpgradeId.SolCoronaBloom, UpgradeId.SolSolarForge, UpgradeId.SolFlareCore, UpgradeId.SolRadiantMantle },
            _ => new List<UpgradeId> { UpgradeId.AstraRefraction, UpgradeId.AstraPrismWake, UpgradeId.AstraNovaBloom, UpgradeId.AstraTwinRefraction },
        };
    }

    private void UpdateUpgrade(float dt)
    {
        UpdateParticles(dt);
        UpdateDamageTexts(dt);
        bool one = KeyDown(Key.Key1);
        bool two = KeyDown(Key.Key2);
        bool three = KeyDown(Key.Key3);
        bool four = KeyDown(Key.Key4);
        bool reroll = KeyDown(Key.R);
        bool click = Input.IsMouseButtonPressed(MouseButton.Left);
        Vector2 mouse = GetGlobalMousePosition();

        if (one && !_lastOne)
        {
            ChooseUpgrade(0);
        }
        else if (two && !_lastTwo)
        {
            ChooseUpgrade(1);
        }
        else if (three && !_lastThree)
        {
            ChooseUpgrade(2);
        }
        else if (four && !_lastFour)
        {
            ChooseUpgrade(3);
        }
        else if (reroll && !_lastReroll && _rerollsRemaining > 0)
        {
            _rerollsRemaining--;
            GenerateUpgradeChoices();
            Burst(ScreenCenter, Violet, 28, 300.0f, 0.7f);
            PlaySfx(280.0f, 120.0f, 0.18f, 0.2f, 0.02f, 1);
        }
        else if (click && !_lastClick)
        {
            for (int i = 0; i < _upgradeChoices.Count; i++)
            {
                if (_upgradeChoices[i].Rect.HasPoint(mouse))
                {
                    ChooseUpgrade(i);
                    break;
                }
            }
        }
    }

    private void ChooseUpgrade(int index)
    {
        if (index < 0 || index >= _upgradeChoices.Count)
        {
            return;
        }

        UpgradeCard card = _upgradeChoices[index];
        if (!_upgradeOrder.Contains(card.Id))
        {
            _upgradeOrder.Add(card.Id);
        }
        _upgradeRanks[card.Id] = GetRank(card.Id) + 1;
        ApplyUpgrade(card.Id);
        AddText(card.Title.ToUpperInvariant(), ScreenCenter + new Vector2(0.0f, -170.0f), card.Accent, 32.0f);
        Burst(ScreenCenter, card.Accent, 64, 620.0f, 1.2f);
        PlaySfx(420.0f, 220.0f, 0.34f, 0.34f, 0.03f, 1);
        _mode = GameMode.Playing;
        BeginNextWave();
    }

    private void ApplyUpgrade(UpgradeId id)
    {
        switch (id)
        {
            case UpgradeId.PrismArray:
                _multiShot = Math.Min(5, _multiShot + 1);
                _fireInterval += 0.025f;
                break;
            case UpgradeId.RailHeart:
                _damageMultiplier += 0.26f;
                _fireInterval = Mathf.Max(0.1f, _fireInterval - 0.016f);
                break;
            case UpgradeId.CoolantLattice:
                _fireInterval = Mathf.Max(0.09f, _fireInterval - 0.045f);
                _maxEnergy += 10.0f;
                break;
            case UpgradeId.KineticBloom:
                _dashPower += 145.0f;
                _playerSpeed += 32.0f;
                _dashCooldown = Mathf.Min(_dashCooldown, 0.2f);
                break;
            case UpgradeId.GravityWell:
                _pickupMagnet += 110.0f;
                _enemySlow *= 0.92f;
                break;
            case UpgradeId.VitalShell:
                _playerMaxHp += 28.0f;
                _playerHp = Mathf.Clamp(_playerHp + 48.0f, 0.0f, _playerMaxHp);
                break;
            case UpgradeId.ResonanceLeech:
                _leechChance += 0.09f;
                _energy = Mathf.Clamp(_energy + 35.0f, 0.0f, _maxEnergy);
                break;
            case UpgradeId.MoonWisp:
                _orbiters = Math.Min(4, _orbiters + 1);
                _orbiterFireTimer = Mathf.Min(_orbiterFireTimer, 0.12f);
                break;
            case UpgradeId.RiftNeedle:
                _riftNeedle = true;
                _damageMultiplier += 0.12f;
                break;
            case UpgradeId.MirrorSkin:
                _mirrorReduction *= 0.86f;
                _playerHp = Mathf.Clamp(_playerHp + 22.0f, 0.0f, _playerMaxHp);
                break;
            case UpgradeId.NovaCapacitor:
                _novaCost = Mathf.Max(38.0f, _novaCost - 7.0f);
                _maxEnergy += 18.0f;
                _energy = Mathf.Clamp(_energy + 28.0f, 0.0f, _maxEnergy);
                break;
            case UpgradeId.PolarityStorm:
                _polarityStorm = Math.Min(6, _polarityStorm + 1);
                _polarityCooldownMax = PolarityCooldownFor(MetaRank(MetaUpgradeId.PolarityTuner), _polarityStorm);
                _polarityCooldown = Mathf.Min(_polarityCooldown, _polarityCooldownMax);
                break;
            case UpgradeId.CometTrail:
                _dashDamage += 32.0f;
                _dashPower += 85.0f;
                break;
            case UpgradeId.AegisBloom:
                _aegisRegen += 0.7f;
                _playerMaxHp += 12.0f;
                _playerHp = Mathf.Clamp(_playerHp + 20.0f, 0.0f, _playerMaxHp);
                break;
            case UpgradeId.QuantumEcho:
                _echoChance = Mathf.Min(0.55f, _echoChance + 0.11f);
                _damageMultiplier += 0.06f;
                break;
            case UpgradeId.SolarThesis:
                _critMultiplier += 0.22f;
                _damageMultiplier += 0.1f;
                break;
            case UpgradeId.EmergencyRepair:
                _playerMaxHp += 8.0f;
                _playerHp = Mathf.Clamp(_playerHp + 70.0f, 0.0f, _playerMaxHp);
                break;
            case UpgradeId.OneWaveOverdrive:
                _nextWaveDamageBoost = 1.55f;
                _energy = Mathf.Clamp(_energy + 45.0f, 0.0f, _maxEnergy);
                break;
            case UpgradeId.GlassCannon:
                _damageMultiplier += 0.42f;
                _playerMaxHp = Mathf.Max(70.0f, _playerMaxHp - 18.0f);
                _playerHp = Mathf.Min(_playerHp, _playerMaxHp);
                break;
            case UpgradeId.BountyContract:
                _nextWaveBonusEnemies += 5 + CurrentSectorIndex();
                _nextWaveRewardBoost = Mathf.Max(_nextWaveRewardBoost, 2.0f);
                break;
            case UpgradeId.BulletTransmute:
                ClearBulletsNear(ScreenCenter, 1600.0f, true);
                _energy = Mathf.Clamp(_energy + 35.0f, 0.0f, _maxEnergy);
                break;
            case UpgradeId.HarmonicMap:
                _baseRerolls = Math.Min(4, _baseRerolls + 1);
                _score += 500;
                break;
            case UpgradeId.AstraRefraction:
                _astraRefraction = Math.Min(4, _astraRefraction + 1);
                _damageMultiplier += 0.05f;
                break;
            case UpgradeId.AstraPrismWake:
                _astraWake = Math.Min(5, _astraWake + 1);
                _fireInterval = Mathf.Max(0.105f, _fireInterval - 0.02f);
                _damageMultiplier += 0.08f;
                break;
            case UpgradeId.VesperCapacitor:
                _vesperCharge = Math.Min(5, _vesperCharge + 1);
                _fireInterval = Mathf.Max(0.32f, _fireInterval - 0.046f);
                _damageMultiplier += 0.08f;
                break;
            case UpgradeId.VesperSplitRail:
                _vesperFork = Math.Min(3, _vesperFork + 1);
                _fireInterval = Mathf.Max(0.32f, _fireInterval - 0.026f);
                break;
            case UpgradeId.KairoDroneBay:
                _kairoDroneBay = Math.Min(4, _kairoDroneBay + 1);
                _orbiters = Math.Min(7, _orbiters + 1);
                _orbiterFireTimer = Mathf.Min(_orbiterFireTimer, 0.1f);
                break;
            case UpgradeId.KairoSwarmSync:
                _kairoSync = Math.Min(5, _kairoSync + 1);
                _fireInterval = Mathf.Max(0.17f, _fireInterval - 0.026f);
                _damageMultiplier += 0.06f;
                _orbiterFireTimer = Mathf.Min(_orbiterFireTimer, 0.08f);
                break;
            case UpgradeId.SolCoronaBloom:
                _solBloom = Math.Min(5, _solBloom + 1);
                _fireInterval = Mathf.Max(0.27f, _fireInterval - 0.028f);
                break;
            case UpgradeId.SolSolarForge:
                _solForge = Math.Min(5, _solForge + 1);
                _maxEnergy += 16.0f;
                _energy = Mathf.Clamp(_energy + 24.0f, 0.0f, _maxEnergy);
                _novaCost = Mathf.Max(34.0f, _novaCost - 6.0f);
                _damageMultiplier += 0.06f;
                break;
            case UpgradeId.AstraNovaBloom:
                _astraNovaBloom = Math.Min(4, _astraNovaBloom + 1);
                _novaCost = Mathf.Max(36.0f, _novaCost - 3.0f);
                _damageMultiplier += 0.04f;
                break;
            case UpgradeId.AstraTwinRefraction:
                _astraTwinRefraction = Math.Min(3, _astraTwinRefraction + 1);
                _damageMultiplier += 0.05f;
                break;
            case UpgradeId.VesperJudgmentCoil:
                _vesperJudgmentCoil = Math.Min(4, _vesperJudgmentCoil + 1);
                _novaCost = Mathf.Max(36.0f, _novaCost - 4.0f);
                _damageMultiplier += 0.05f;
                break;
            case UpgradeId.VesperSeverLine:
                _vesperSeverLine = Math.Min(3, _vesperSeverLine + 1);
                _damageMultiplier += 0.04f;
                break;
            case UpgradeId.KairoOverrideMatrix:
                _kairoOverrideMatrix = Math.Min(4, _kairoOverrideMatrix + 1);
                _orbiterFireTimer = Mathf.Min(_orbiterFireTimer, 0.08f);
                _novaCost = Mathf.Max(36.0f, _novaCost - 3.0f);
                break;
            case UpgradeId.KairoRelayProtocol:
                _kairoRelayProtocol = Math.Min(3, _kairoRelayProtocol + 1);
                _damageMultiplier += 0.04f;
                _orbiterFireTimer = Mathf.Min(_orbiterFireTimer, 0.06f);
                break;
            case UpgradeId.SolFlareCore:
                _solFlareCore = Math.Min(4, _solFlareCore + 1);
                _maxEnergy += 10.0f;
                _energy = Mathf.Clamp(_energy + 16.0f, 0.0f, _maxEnergy);
                _damageMultiplier += 0.04f;
                break;
            case UpgradeId.SolRadiantMantle:
                _solRadiantMantle = Math.Min(3, _solRadiantMantle + 1);
                _playerMaxHp += 8.0f;
                _playerHp = Mathf.Clamp(_playerHp + 18.0f, 0.0f, _playerMaxHp);
                _novaCost = Mathf.Max(36.0f, _novaCost - 4.0f);
                break;
        }
    }

    private UpgradeCard CreateCard(UpgradeId id)
    {
        int nextRank = GetRank(id) + 1;
        string rank = Tf("rank", nextRank);
        return id switch
        {
            UpgradeId.PrismArray => new UpgradeCard { Id = id, Title = T("upgrade.prism.title"), Tag = rank, Body = T("upgrade.prism.body"), Accent = Cyan },
            UpgradeId.RailHeart => new UpgradeCard { Id = id, Title = T("upgrade.rail.title"), Tag = rank, Body = T("upgrade.rail.body"), Accent = Rose },
            UpgradeId.CoolantLattice => new UpgradeCard { Id = id, Title = T("upgrade.coolant.title"), Tag = rank, Body = T("upgrade.coolant.body"), Accent = Jade },
            UpgradeId.KineticBloom => new UpgradeCard { Id = id, Title = T("upgrade.kinetic.title"), Tag = rank, Body = T("upgrade.kinetic.body"), Accent = Gold },
            UpgradeId.GravityWell => new UpgradeCard { Id = id, Title = T("upgrade.gravity.title"), Tag = rank, Body = T("upgrade.gravity.body"), Accent = Violet },
            UpgradeId.VitalShell => new UpgradeCard { Id = id, Title = T("upgrade.vital.title"), Tag = rank, Body = T("upgrade.vital.body"), Accent = Paper },
            UpgradeId.ResonanceLeech => new UpgradeCard { Id = id, Title = T("upgrade.leech.title"), Tag = rank, Body = T("upgrade.leech.body"), Accent = new Color(0.35f, 1.0f, 0.78f) },
            UpgradeId.MoonWisp => new UpgradeCard { Id = id, Title = T("upgrade.wisp.title"), Tag = rank, Body = T("upgrade.wisp.body"), Accent = new Color(0.72f, 0.82f, 1.0f) },
            UpgradeId.RiftNeedle => new UpgradeCard { Id = id, Title = T("upgrade.rift.title"), Tag = rank, Body = T("upgrade.rift.body"), Accent = new Color(1.0f, 0.36f, 0.82f) },
            UpgradeId.MirrorSkin => new UpgradeCard { Id = id, Title = T("upgrade.mirror.title"), Tag = rank, Body = T("upgrade.mirror.body"), Accent = new Color(0.98f, 0.88f, 0.48f) },
            UpgradeId.NovaCapacitor => new UpgradeCard { Id = id, Title = T("upgrade.nova.title"), Tag = rank, Body = T("upgrade.nova.body"), Accent = new Color(0.38f, 0.95f, 1.0f) },
            UpgradeId.PolarityStorm => new UpgradeCard { Id = id, Title = T("upgrade.storm.title"), Tag = rank, Body = T("upgrade.storm.body"), Accent = new Color(1.0f, 0.72f, 0.24f) },
            UpgradeId.CometTrail => new UpgradeCard { Id = id, Title = T("upgrade.comet.title"), Tag = rank, Body = T("upgrade.comet.body"), Accent = new Color(1.0f, 0.38f, 0.2f) },
            UpgradeId.AegisBloom => new UpgradeCard { Id = id, Title = T("upgrade.aegis.title"), Tag = rank, Body = T("upgrade.aegis.body"), Accent = new Color(0.48f, 1.0f, 0.64f) },
            UpgradeId.QuantumEcho => new UpgradeCard { Id = id, Title = T("upgrade.echo.title"), Tag = rank, Body = T("upgrade.echo.body"), Accent = new Color(0.68f, 0.56f, 1.0f) },
            UpgradeId.SolarThesis => new UpgradeCard { Id = id, Title = T("upgrade.solar.title"), Tag = rank, Body = T("upgrade.solar.body"), Accent = new Color(1.0f, 0.86f, 0.32f) },
            UpgradeId.EmergencyRepair => new UpgradeCard { Id = id, Title = T("upgrade.repair.title"), Tag = T("choice.instant"), Body = T("upgrade.repair.body"), Accent = Jade },
            UpgradeId.OneWaveOverdrive => new UpgradeCard { Id = id, Title = T("upgrade.overdrive.title"), Tag = T("choice.tactic"), Body = T("upgrade.overdrive.body"), Accent = Rose },
            UpgradeId.GlassCannon => new UpgradeCard { Id = id, Title = T("upgrade.glass.title"), Tag = T("choice.risk"), Body = T("upgrade.glass.body"), Accent = new Color(1.0f, 0.42f, 0.24f) },
            UpgradeId.BountyContract => new UpgradeCard { Id = id, Title = T("upgrade.bounty.title"), Tag = T("choice.contract"), Body = T("upgrade.bounty.body"), Accent = Gold },
            UpgradeId.BulletTransmute => new UpgradeCard { Id = id, Title = T("upgrade.transmute.title"), Tag = T("choice.instant"), Body = T("upgrade.transmute.body"), Accent = Cyan },
            UpgradeId.HarmonicMap => new UpgradeCard { Id = id, Title = T("upgrade.map.title"), Tag = T("choice.meta"), Body = T("upgrade.map.body"), Accent = Violet },
            UpgradeId.AstraRefraction => new UpgradeCard { Id = id, Title = T("upgrade.astra.refraction.title"), Tag = rank, Body = T("upgrade.astra.refraction.body"), Accent = PolarityBlue },
            UpgradeId.AstraPrismWake => new UpgradeCard { Id = id, Title = T("upgrade.astra.wake.title"), Tag = rank, Body = T("upgrade.astra.wake.body"), Accent = PolarityAmber },
            UpgradeId.VesperCapacitor => new UpgradeCard { Id = id, Title = T("upgrade.vesper.charge.title"), Tag = rank, Body = T("upgrade.vesper.charge.body"), Accent = AlertRed },
            UpgradeId.VesperSplitRail => new UpgradeCard { Id = id, Title = T("upgrade.vesper.fork.title"), Tag = rank, Body = T("upgrade.vesper.fork.body"), Accent = Rose },
            UpgradeId.KairoDroneBay => new UpgradeCard { Id = id, Title = T("upgrade.kairo.bay.title"), Tag = rank, Body = T("upgrade.kairo.bay.body"), Accent = PickupBlue },
            UpgradeId.KairoSwarmSync => new UpgradeCard { Id = id, Title = T("upgrade.kairo.sync.title"), Tag = rank, Body = T("upgrade.kairo.sync.body"), Accent = XpGreen },
            UpgradeId.SolCoronaBloom => new UpgradeCard { Id = id, Title = T("upgrade.sol.bloom.title"), Tag = rank, Body = T("upgrade.sol.bloom.body"), Accent = Gold },
            UpgradeId.SolSolarForge => new UpgradeCard { Id = id, Title = T("upgrade.sol.forge.title"), Tag = rank, Body = T("upgrade.sol.forge.body"), Accent = new Color(1.0f, 0.48f, 0.2f) },
            UpgradeId.AstraNovaBloom => new UpgradeCard { Id = id, Title = T("upgrade.astra.nova.title"), Tag = rank, Body = T("upgrade.astra.nova.body"), Accent = PolarityBlue },
            UpgradeId.AstraTwinRefraction => new UpgradeCard { Id = id, Title = T("upgrade.astra.twin.title"), Tag = rank, Body = T("upgrade.astra.twin.body"), Accent = PolarityAmber },
            UpgradeId.VesperJudgmentCoil => new UpgradeCard { Id = id, Title = T("upgrade.vesper.judgment.title"), Tag = rank, Body = T("upgrade.vesper.judgment.body"), Accent = AlertRed },
            UpgradeId.VesperSeverLine => new UpgradeCard { Id = id, Title = T("upgrade.vesper.sever.title"), Tag = rank, Body = T("upgrade.vesper.sever.body"), Accent = Rose },
            UpgradeId.KairoOverrideMatrix => new UpgradeCard { Id = id, Title = T("upgrade.kairo.override.title"), Tag = rank, Body = T("upgrade.kairo.override.body"), Accent = PickupBlue },
            UpgradeId.KairoRelayProtocol => new UpgradeCard { Id = id, Title = T("upgrade.kairo.relay.title"), Tag = rank, Body = T("upgrade.kairo.relay.body"), Accent = XpGreen },
            UpgradeId.SolFlareCore => new UpgradeCard { Id = id, Title = T("upgrade.sol.flare.title"), Tag = rank, Body = T("upgrade.sol.flare.body"), Accent = Gold },
            UpgradeId.SolRadiantMantle => new UpgradeCard { Id = id, Title = T("upgrade.sol.mantle.title"), Tag = rank, Body = T("upgrade.sol.mantle.body"), Accent = new Color(1.0f, 0.72f, 0.34f) },
            _ => new UpgradeCard { Id = id, Title = T("upgrade.unknown.title"), Tag = Tf("rank", 1), Body = T("upgrade.unknown.body"), Accent = Paper },
        };
    }

    private string UpgradeTitle(UpgradeId id)
    {
        return id switch
        {
            UpgradeId.PrismArray => T("upgrade.prism.title"),
            UpgradeId.RailHeart => T("upgrade.rail.title"),
            UpgradeId.CoolantLattice => T("upgrade.coolant.title"),
            UpgradeId.KineticBloom => T("upgrade.kinetic.title"),
            UpgradeId.GravityWell => T("upgrade.gravity.title"),
            UpgradeId.VitalShell => T("upgrade.vital.title"),
            UpgradeId.ResonanceLeech => T("upgrade.leech.title"),
            UpgradeId.MoonWisp => T("upgrade.wisp.title"),
            UpgradeId.RiftNeedle => T("upgrade.rift.title"),
            UpgradeId.MirrorSkin => T("upgrade.mirror.title"),
            UpgradeId.NovaCapacitor => T("upgrade.nova.title"),
            UpgradeId.PolarityStorm => T("upgrade.storm.title"),
            UpgradeId.CometTrail => T("upgrade.comet.title"),
            UpgradeId.AegisBloom => T("upgrade.aegis.title"),
            UpgradeId.QuantumEcho => T("upgrade.echo.title"),
            UpgradeId.SolarThesis => T("upgrade.solar.title"),
            UpgradeId.EmergencyRepair => T("upgrade.repair.title"),
            UpgradeId.OneWaveOverdrive => T("upgrade.overdrive.title"),
            UpgradeId.GlassCannon => T("upgrade.glass.title"),
            UpgradeId.BountyContract => T("upgrade.bounty.title"),
            UpgradeId.BulletTransmute => T("upgrade.transmute.title"),
            UpgradeId.HarmonicMap => T("upgrade.map.title"),
            UpgradeId.AstraRefraction => T("upgrade.astra.refraction.title"),
            UpgradeId.AstraPrismWake => T("upgrade.astra.wake.title"),
            UpgradeId.VesperCapacitor => T("upgrade.vesper.charge.title"),
            UpgradeId.VesperSplitRail => T("upgrade.vesper.fork.title"),
            UpgradeId.KairoDroneBay => T("upgrade.kairo.bay.title"),
            UpgradeId.KairoSwarmSync => T("upgrade.kairo.sync.title"),
            UpgradeId.SolCoronaBloom => T("upgrade.sol.bloom.title"),
            UpgradeId.SolSolarForge => T("upgrade.sol.forge.title"),
            UpgradeId.AstraNovaBloom => T("upgrade.astra.nova.title"),
            UpgradeId.AstraTwinRefraction => T("upgrade.astra.twin.title"),
            UpgradeId.VesperJudgmentCoil => T("upgrade.vesper.judgment.title"),
            UpgradeId.VesperSeverLine => T("upgrade.vesper.sever.title"),
            UpgradeId.KairoOverrideMatrix => T("upgrade.kairo.override.title"),
            UpgradeId.KairoRelayProtocol => T("upgrade.kairo.relay.title"),
            UpgradeId.SolFlareCore => T("upgrade.sol.flare.title"),
            UpgradeId.SolRadiantMantle => T("upgrade.sol.mantle.title"),
            _ => T("upgrade.unknown.title"),
        };
    }

    private static Color UpgradeAccent(UpgradeId id)
    {
        return id switch
        {
            UpgradeId.PrismArray => Cyan,
            UpgradeId.RailHeart => Rose,
            UpgradeId.CoolantLattice => Jade,
            UpgradeId.KineticBloom => Gold,
            UpgradeId.GravityWell => Violet,
            UpgradeId.VitalShell => Paper,
            UpgradeId.ResonanceLeech => new Color(0.35f, 1.0f, 0.78f),
            UpgradeId.MoonWisp => new Color(0.72f, 0.82f, 1.0f),
            UpgradeId.RiftNeedle => new Color(1.0f, 0.36f, 0.82f),
            UpgradeId.MirrorSkin => new Color(0.98f, 0.88f, 0.48f),
            UpgradeId.NovaCapacitor => new Color(0.38f, 0.95f, 1.0f),
            UpgradeId.PolarityStorm => new Color(1.0f, 0.72f, 0.24f),
            UpgradeId.CometTrail => new Color(1.0f, 0.38f, 0.2f),
            UpgradeId.AegisBloom => new Color(0.48f, 1.0f, 0.64f),
            UpgradeId.QuantumEcho => new Color(0.68f, 0.56f, 1.0f),
            UpgradeId.SolarThesis => new Color(1.0f, 0.86f, 0.32f),
            UpgradeId.EmergencyRepair => Jade,
            UpgradeId.OneWaveOverdrive => Rose,
            UpgradeId.GlassCannon => new Color(1.0f, 0.42f, 0.24f),
            UpgradeId.BountyContract => Gold,
            UpgradeId.BulletTransmute => Cyan,
            UpgradeId.HarmonicMap => Violet,
            UpgradeId.AstraRefraction => PolarityBlue,
            UpgradeId.AstraPrismWake => PolarityAmber,
            UpgradeId.VesperCapacitor => AlertRed,
            UpgradeId.VesperSplitRail => Rose,
            UpgradeId.KairoDroneBay => PickupBlue,
            UpgradeId.KairoSwarmSync => XpGreen,
            UpgradeId.SolCoronaBloom => Gold,
            UpgradeId.SolSolarForge => new Color(1.0f, 0.48f, 0.2f),
            UpgradeId.AstraNovaBloom => PolarityBlue,
            UpgradeId.AstraTwinRefraction => PolarityAmber,
            UpgradeId.VesperJudgmentCoil => AlertRed,
            UpgradeId.VesperSeverLine => Rose,
            UpgradeId.KairoOverrideMatrix => PickupBlue,
            UpgradeId.KairoRelayProtocol => XpGreen,
            UpgradeId.SolFlareCore => Gold,
            UpgradeId.SolRadiantMantle => new Color(1.0f, 0.72f, 0.34f),
            _ => Paper,
        };
    }

    private int GetRank(UpgradeId id)
    {
        return _upgradeRanks.TryGetValue(id, out int rank) ? rank : 0;
    }

    private int MetaRank(MetaUpgradeId id)
    {
        return _metaRanks.TryGetValue(id, out int rank) ? rank : 0;
    }

    private static int MetaUpgradeCost(MetaUpgradeDef def, int currentRank)
    {
        return def.BaseCost + currentRank * def.StepCost + currentRank * currentRank * 5 + Math.Max(0, currentRank - 2) * 8;
    }

    private int MaxRank(UpgradeId id)
    {
        return id switch
        {
            UpgradeId.RiftNeedle => 1,
            UpgradeId.MoonWisp => 4,
            UpgradeId.PrismArray => 4,
            UpgradeId.PolarityStorm => 6,
            UpgradeId.QuantumEcho => 5,
            UpgradeId.NovaCapacitor => 5,
            UpgradeId.EmergencyRepair => 99,
            UpgradeId.OneWaveOverdrive => 99,
            UpgradeId.GlassCannon => 5,
            UpgradeId.BountyContract => 99,
            UpgradeId.BulletTransmute => 99,
            UpgradeId.HarmonicMap => 3,
            UpgradeId.AstraRefraction => 4,
            UpgradeId.AstraPrismWake => 5,
            UpgradeId.VesperCapacitor => 5,
            UpgradeId.VesperSplitRail => 3,
            UpgradeId.KairoDroneBay => 4,
            UpgradeId.KairoSwarmSync => 5,
            UpgradeId.SolCoronaBloom => 5,
            UpgradeId.SolSolarForge => 5,
            UpgradeId.AstraNovaBloom => 4,
            UpgradeId.AstraTwinRefraction => 3,
            UpgradeId.VesperJudgmentCoil => 4,
            UpgradeId.VesperSeverLine => 3,
            UpgradeId.KairoOverrideMatrix => 4,
            UpgradeId.KairoRelayProtocol => 3,
            UpgradeId.SolFlareCore => 4,
            UpgradeId.SolRadiantMantle => 3,
            _ => 5,
        };
    }

    private void WinRun()
    {
        _mode = GameMode.Victory;
        _wonOnce = true;
        AwardMetaProgress(true);
        ClearShots();
        Burst(ScreenCenter, Gold, 220, 980.0f, 2.5f);
        PlaySfx(240.0f, 80.0f, 1.6f, 0.52f, 0.12f, 1);
    }

    private void LoseRun()
    {
        if (_mode == GameMode.GameOver)
        {
            return;
        }
        _mode = GameMode.GameOver;
        _playerHp = 0.0f;
        AwardMetaProgress(false);
        ClearShots();
        Burst(_playerPos, Rose, 140, 880.0f, 2.1f);
        _shake = 1.0f;
        _flash = 0.9f;
        PlaySfx(52.0f, -8.0f, 1.2f, 0.58f, 0.46f, 0);
    }

    private void UpdateEndScreen(float dt)
    {
        UpdateParticles(dt);
        UpdateDamageTexts(dt);
        if (KeyDown(Key.U) && !_lastMeta)
        {
            ResetTitle();
            _mode = GameMode.Meta;
            return;
        }

        bool restart = KeyDown(Key.Enter) || Input.IsMouseButtonPressed(MouseButton.Left);
        if (restart && !_lastRestart)
        {
            StartRun();
        }
        if (KeyDown(Key.Escape))
        {
            ResetTitle();
        }
    }

    private void GenerateBackdrop()
    {
        _stars.Clear();
        _nebulas.Clear();
        for (int i = 0; i < 260; i++)
        {
            float depth = _rng.RandfRange(0.15f, 1.0f);
            Color color = i % 11 == 0 ? GridLine : new Color(0.48f, 0.56f, 0.62f);
            _stars.Add(new Star
            {
                Pos = new Vector2(_rng.RandfRange(0.0f, ScreenWidth), _rng.RandfRange(0.0f, ScreenHeight)),
                Radius = _rng.RandfRange(0.45f, 1.35f) * depth,
                Twinkle = _rng.RandfRange(0.5f, 2.6f),
                Depth = depth,
                Color = color,
            });
        }

        _nebulas.Add(new Nebula { Pos = new Vector2(350.0f, 240.0f), Radius = 560.0f, Color = new Color(0.12f, 0.38f, 0.7f), Drift = 0.12f });
        _nebulas.Add(new Nebula { Pos = new Vector2(1500.0f, 300.0f), Radius = 680.0f, Color = new Color(0.16f, 0.22f, 0.28f), Drift = -0.08f });
        _nebulas.Add(new Nebula { Pos = new Vector2(1010.0f, 910.0f), Radius = 620.0f, Color = new Color(0.22f, 0.18f, 0.12f), Drift = 0.06f });
        _nebulas.Add(new Nebula { Pos = new Vector2(1860.0f, 940.0f), Radius = 420.0f, Color = new Color(0.1f, 0.24f, 0.2f), Drift = -0.13f });
    }

    private void DrawBackdrop()
    {
        Color sectorColor = CurrentSector().Accent;
        DrawRect(new Rect2(Vector2.Zero, new Vector2(ScreenWidth, ScreenHeight)), Void, true);
        DrawRect(new Rect2(Vector2.Zero, new Vector2(ScreenWidth, ScreenHeight)), new Color(0.018f, 0.023f, 0.03f), true);

        foreach (Nebula nebula in _nebulas)
        {
            Vector2 drift = new Vector2(Mathf.Sin(_time * nebula.Drift + _noiseSeed), Mathf.Cos(_time * nebula.Drift * 0.7f)) * 28.0f;
            for (int i = 5; i >= 1; i--)
            {
                float t = i / 5.0f;
                DrawCircle(nebula.Pos + drift, nebula.Radius * t, Alpha(nebula.Color, 0.007f * (1.0f - t + 0.22f)));
            }
        }

        for (int i = 0; i < _stars.Count; i++)
        {
            Star star = _stars[i];
            float twinkle = 0.55f + 0.45f * Mathf.Sin(_time * star.Twinkle + i * 0.37f);
            Vector2 drift = new Vector2(Mathf.Sin(_time * 0.08f * star.Depth + i), Mathf.Cos(_time * 0.07f * star.Depth + i * 0.3f)) * (12.0f * star.Depth);
            DrawCircle(star.Pos + drift, star.Radius * (0.7f + twinkle * 0.32f), Alpha(star.Color, 0.06f + twinkle * 0.16f));
        }

        float gridAlpha = 0.035f + 0.012f * Mathf.Sin(_time * 0.8f);
        for (int x = -120; x <= ScreenWidth + 120; x += 120)
        {
            DrawLine(new Vector2(x, 0.0f), new Vector2(x, ScreenHeight), Alpha(GridLine, gridAlpha), UiHairline, true);
        }
        for (int y = 0; y <= ScreenHeight; y += 108)
        {
            DrawLine(new Vector2(0.0f, y), new Vector2(ScreenWidth, y), Alpha(GridLine, gridAlpha * 0.85f), UiHairline, true);
        }

        for (int r = 160; r < 1020; r += 120)
        {
            DrawArc(ScreenCenter, r + Mathf.Sin(_time * 0.25f + r) * 3.0f, _time * 0.04f, Mathf.Tau + _time * 0.04f, 96, Alpha(sectorColor, 0.012f), UiHairline, true);
        }

        for (int i = 0; i < 18; i++)
        {
            float y = 110.0f + i * 52.0f + Mathf.Sin(_time * 0.35f + i) * 8.0f;
            float a = 0.018f + 0.012f * Mathf.Sin(_time * 0.9f + i * 0.7f);
            DrawLine(new Vector2(90.0f, y), new Vector2(ScreenWidth - 90.0f, y + Mathf.Sin(i) * 24.0f), Alpha(Paper, a), UiHairline, true);
        }

        for (int i = 0; i < 9; i++)
        {
            float angle = _time * 0.035f + i * Mathf.Tau / 9.0f;
            Vector2 spoke = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            DrawLine(ScreenCenter - spoke * 120.0f, ScreenCenter + spoke * 1040.0f, Alpha(sectorColor, 0.006f), UiHairline, true);
        }

        DrawVignette();
    }

    private void DrawTitleBackdrop()
    {
        DrawRect(new Rect2(Vector2.Zero, new Vector2(ScreenWidth, ScreenHeight)), Colors.Black, true);
        DrawRect(new Rect2(Vector2.Zero, new Vector2(ScreenWidth, ScreenHeight)), new Color(0.002f, 0.003f, 0.006f), true);

        Vector2 galaxyCenter = new(ScreenWidth * 0.5f, 360.0f);
        for (int i = 6; i >= 1; i--)
        {
            float t = i / 6.0f;
            DrawCircle(galaxyCenter + new Vector2(Mathf.Sin(_time * 0.05f + i) * 38.0f, Mathf.Cos(_time * 0.04f + i) * 18.0f), 520.0f * t, Alpha(new Color(0.05f, 0.08f, 0.16f), 0.025f * (1.0f - t + 0.25f)));
        }

        for (int i = 0; i < 7; i++)
        {
            float r = 220.0f + i * 82.0f;
            float a = _time * 0.018f + i * 0.33f;
            DrawArc(galaxyCenter + new Vector2(0.0f, 18.0f), r, a, a + Mathf.Pi * 1.18f, 96, Alpha(new Color(0.18f, 0.24f, 0.36f), 0.035f - i * 0.0035f), UiHairline, true);
        }

        for (int i = 0; i < _stars.Count; i++)
        {
            if (i % 3 != 0)
            {
                continue;
            }

            Star star = _stars[i];
            float twinkle = 0.48f + 0.52f * Mathf.Sin(_time * star.Twinkle * 0.72f + i * 0.41f);
            Vector2 drift = new Vector2(Mathf.Sin(_time * 0.025f * star.Depth + i), Mathf.Cos(_time * 0.02f * star.Depth + i * 0.3f)) * (5.0f * star.Depth);
            float alpha = 0.26f + twinkle * 0.34f;
            DrawCircle(star.Pos + drift, Mathf.Max(1.2f, star.Radius * 0.82f), Alpha(Paper, alpha));
        }

        Vector2 planet = new(ScreenWidth * 0.5f, ScreenHeight + 450.0f);
        DrawCircle(planet, 860.0f, Alpha(new Color(0.01f, 0.014f, 0.028f), 0.96f));
        DrawArc(planet, 860.0f, Mathf.Pi * 1.08f, Mathf.Pi * 1.92f, 160, Alpha(new Color(0.12f, 0.18f, 0.34f), 0.28f), UiStroke, true);
        DrawArc(planet, 805.0f, Mathf.Pi * 1.1f, Mathf.Pi * 1.9f, 160, Alpha(new Color(0.06f, 0.1f, 0.2f), 0.22f), UiHairline, true);
        DrawCircle(new Vector2(ScreenWidth * 0.22f, ScreenHeight * 0.28f), 260.0f, Alpha(new Color(0.08f, 0.04f, 0.12f), 0.045f));
        DrawCircle(new Vector2(ScreenWidth * 0.82f, ScreenHeight * 0.32f), 320.0f, Alpha(new Color(0.03f, 0.08f, 0.12f), 0.04f));
        DrawVignette();
    }

    private void DrawArenaFrame()
    {
        Rect2 rect = Arena;
        Color sectorColor = CurrentSector().Accent;
        DrawRect(rect, Alpha(Panel, 0.74f), true);
        DrawRect(rect.Grow(-2.0f), Alpha(Graphite, 0.28f), true);
        for (float x = rect.Position.X + 96.0f; x < rect.End.X; x += 96.0f)
        {
            DrawLine(new Vector2(x, rect.Position.Y), new Vector2(x, rect.End.Y), Alpha(GridLine, 0.11f), UiHairline, true);
        }
        for (float y = rect.Position.Y + 96.0f; y < rect.End.Y; y += 96.0f)
        {
            DrawLine(new Vector2(rect.Position.X, y), new Vector2(rect.End.X, y), Alpha(GridLine, 0.1f), UiHairline, true);
        }

        for (int i = 0; i < 5; i++)
        {
            DrawRect(rect.Grow(i * 5.0f), Alpha(i == 0 ? Paper : GridLine, 0.18f - i * 0.02f), false, UiHairline + i * 0.28f, true);
        }

        float pulse = 0.55f + 0.45f * Mathf.Sin(_time * 1.7f);
        DrawLine(rect.Position, rect.Position + new Vector2(160.0f, 0.0f), Alpha(sectorColor, 0.34f + pulse * 0.12f), UiAccentStroke, true);
        DrawLine(rect.Position, rect.Position + new Vector2(0.0f, 160.0f), Alpha(sectorColor, 0.34f + pulse * 0.12f), UiAccentStroke, true);
        DrawLine(rect.End, rect.End - new Vector2(160.0f, 0.0f), Alpha(Paper, 0.2f + pulse * 0.08f), UiAccentStroke, true);
        DrawLine(rect.End, rect.End - new Vector2(0.0f, 160.0f), Alpha(Paper, 0.2f + pulse * 0.08f), UiAccentStroke, true);

        for (int i = 0; i <= 16; i++)
        {
            float x = Mathf.Lerp(rect.Position.X, rect.End.X, i / 16.0f);
            float tick = i % 4 == 0 ? 22.0f : 10.0f;
            DrawLine(new Vector2(x, rect.Position.Y), new Vector2(x, rect.Position.Y + tick), Alpha(Paper, 0.16f), UiHairline, true);
            DrawLine(new Vector2(x, rect.End.Y), new Vector2(x, rect.End.Y - tick), Alpha(Paper, 0.12f), UiHairline, true);
        }

        for (int i = 0; i <= 8; i++)
        {
            float y = Mathf.Lerp(rect.Position.Y, rect.End.Y, i / 8.0f);
            float tick = i % 2 == 0 ? 22.0f : 10.0f;
            DrawLine(new Vector2(rect.Position.X, y), new Vector2(rect.Position.X + tick, y), Alpha(Paper, 0.14f), UiHairline, true);
            DrawLine(new Vector2(rect.End.X, y), new Vector2(rect.End.X - tick, y), Alpha(Paper, 0.12f), UiHairline, true);
        }
    }

    private void DrawPlayer()
    {
        Vector2 p = _playerPos + ShakeOffset();
        Color polarity = PolarityColor(_playerPolarity);
        float invuln = _invulnTimer > 0.0f ? 0.5f + 0.5f * Mathf.Sin(_time * 36.0f) : 1.0f;
        Vector2 forward = _aimDir;

        DrawPlayerTrail(p, polarity, invuln);
        DrawGlow(p, polarity, 52.0f, 0.035f * invuln, 3);
        DrawCircle(p, 42.0f, Alpha(polarity, 0.24f * invuln), false, 2.0f, true);
        if (_invulnTimer > 0.0f)
        {
            DrawCircle(p, 62.0f, Alpha(Paper, 0.2f + invuln * 0.16f), false, 2.0f, true);
        }

        DrawPilotHull(_runPilot, p, forward, polarity, invuln, 1.0f);
        DrawPolarityCooldownBadge(p, polarity);

        if (_dashTimer > 0.0f)
        {
            DrawLine(p - forward * 130.0f, p + forward * 18.0f, Alpha(polarity, 0.28f), 9.0f, true);
            DrawLine(p - forward * 152.0f, p + forward * 24.0f, Alpha(Paper, 0.24f), 2.0f, true);
        }

        for (int i = 0; i < _orbiters; i++)
        {
            Vector2 orb = OrbiterPosition(p, i, _orbiters);
            float charge = 1.0f - Mathf.Clamp(_orbiterFireTimer / OrbiterFireInterval(), 0.0f, 1.0f);
            DrawCircle(orb, 8.0f, Alpha(Graphite, 0.94f));
            DrawCircle(orb, 8.0f, Alpha(polarity, 0.76f), false, 2.0f, true);
            DrawCircle(orb, 12.0f + charge * 4.0f, Alpha(polarity, 0.12f + charge * 0.18f), false, 1.0f, true);
        }
    }

    private void DrawPlayerTrail(Vector2 current, Color polarity, float invuln)
    {
        if (_playerTrailCount < 2)
        {
            return;
        }

        Vector2 shake = ShakeOffset();
        int maxSegments = _visualPressure > 0.82f ? 6 : 11;
        int count = Math.Min(_playerTrailCount, maxSegments);
        Vector2 from = current;
        for (int i = 0; i < count; i++)
        {
            Vector2 to = _playerTrail[i] + shake;
            if (from.DistanceSquaredTo(to) < 4.0f)
            {
                continue;
            }

            float t = (i + 1.0f) / (count + 1.0f);
            float width = Mathf.Lerp(26.0f, 3.0f, t);
            float alpha = (1.0f - t) * (0.18f + (_dashTimer > 0.0f ? 0.16f : 0.0f)) * invuln;
            DrawLine(from, to, Alpha(polarity, alpha), width, true);
            DrawLine(from, to, Alpha(Paper, alpha * 0.36f), Mathf.Max(1.0f, width * 0.16f), true);
            from = to;
        }
    }

    private void DrawPolarityCooldownBadge(Vector2 playerPos, Color polarity)
    {
        if (_polarityCooldown <= 0.0f)
        {
            return;
        }

        Vector2 center = playerPos + new Vector2(32.0f, -32.0f);
        float ready = PolaritySwitchReady01();
        float radius = 9.5f;
        Color next = PolarityColor(1 - _playerPolarity);

        DrawCircle(center, radius + 2.0f, Alpha(Void, 0.62f));
        DrawCircle(center, radius, Alpha(Graphite, 0.84f));
        DrawCircle(center, radius, Alpha(Paper, 0.16f), false, UiHairline, true);
        DrawArc(center, radius, -Mathf.Pi * 0.5f, -Mathf.Pi * 0.5f + Mathf.Tau * ready, 24, Alpha(polarity, 0.72f), UiHairline, true);
        DrawCircle(center, 3.0f, Alpha(next, 0.42f));
    }

    private Vector2 OrbiterPosition(Vector2 center, int index, int total)
    {
        float a = _time * (1.8f + index * 0.13f) + index * Mathf.Tau / Math.Max(1, total);
        return center + new Vector2(82.0f, 0.0f).Rotated(a);
    }

    private void DrawPilotHull(PilotKind pilot, Vector2 center, Vector2 forward, Color polarity, float invuln, float scale)
    {
        Vector2 right = new(-forward.Y, forward.X);
        Color body = Alpha(Graphite, 0.78f * invuln);
        Color line = Alpha(Paper, 0.86f * invuln);
        Color softLine = Alpha(GridLine, 0.45f * invuln);
        switch (pilot)
        {
            case PilotKind.Vesper:
            {
                Vector2[] hull =
                {
                    center + forward * 44.0f * scale,
                    center + right * 11.0f * scale + forward * 12.0f * scale,
                    center + right * 18.0f * scale - forward * 28.0f * scale,
                    center - forward * 18.0f * scale,
                    center - right * 18.0f * scale - forward * 28.0f * scale,
                    center - right * 11.0f * scale + forward * 12.0f * scale,
                };
                DrawColoredPolygon(hull, body, Array.Empty<Vector2>(), null);
                DrawPolyline(ClosePolygon(hull), line, 2.0f * scale, true);
                DrawLine(center - forward * 22.0f * scale, center + forward * 42.0f * scale, Alpha(Paper, 0.62f * invuln), 2.4f * scale, true);
                DrawLine(center + right * 17.0f * scale - forward * 21.0f * scale, center + right * 35.0f * scale - forward * 9.0f * scale, softLine, 1.5f * scale, true);
                DrawLine(center - right * 17.0f * scale - forward * 21.0f * scale, center - right * 35.0f * scale - forward * 9.0f * scale, softLine, 1.5f * scale, true);
                DrawCircle(center + forward * 3.0f * scale, 9.0f * scale, Alpha(polarity, 0.82f * invuln), false, 2.2f * scale, true);
                break;
            }
            case PilotKind.Kairo:
            {
                Vector2[] core = RegularPolygon(center, 25.0f * scale, 8, forward.Angle() + Mathf.Pi * 0.125f);
                DrawColoredPolygon(core, body, Array.Empty<Vector2>(), null);
                DrawPolyline(ClosePolygon(core), line, 2.0f * scale, true);
                DrawCircle(center, 30.0f * scale, Alpha(polarity, 0.28f * invuln), false, 1.6f * scale, true);
                DrawCircle(center, 12.0f * scale, Alpha(Graphite, 0.94f * invuln));
                DrawCircle(center, 9.0f * scale, Alpha(polarity, 0.84f * invuln), false, 2.2f * scale, true);
                DrawCircle(center + right * 30.0f * scale - forward * 5.0f * scale, 7.0f * scale, Alpha(Graphite, 0.92f * invuln));
                DrawCircle(center + right * 30.0f * scale - forward * 5.0f * scale, 6.0f * scale, Alpha(polarity, 0.68f * invuln), false, 1.5f * scale, true);
                DrawCircle(center - right * 30.0f * scale - forward * 5.0f * scale, 7.0f * scale, Alpha(Graphite, 0.92f * invuln));
                DrawCircle(center - right * 30.0f * scale - forward * 5.0f * scale, 6.0f * scale, Alpha(polarity, 0.68f * invuln), false, 1.5f * scale, true);
                DrawLine(center - right * 23.0f * scale, center + right * 23.0f * scale, softLine, 1.4f * scale, true);
                break;
            }
            case PilotKind.Sol:
            {
                Vector2[] hull =
                {
                    center + forward * 34.0f * scale,
                    center + right * 34.0f * scale + forward * 4.0f * scale,
                    center + right * 16.0f * scale - forward * 26.0f * scale,
                    center - forward * 15.0f * scale,
                    center - right * 16.0f * scale - forward * 26.0f * scale,
                    center - right * 34.0f * scale + forward * 4.0f * scale,
                };
                DrawColoredPolygon(hull, body, Array.Empty<Vector2>(), null);
                DrawPolyline(ClosePolygon(hull), line, 2.0f * scale, true);
                DrawLine(center - right * 31.0f * scale + forward * 3.0f * scale, center + right * 31.0f * scale + forward * 3.0f * scale, Alpha(polarity, 0.55f * invuln), 1.8f * scale, true);
                DrawLine(center - forward * 14.0f * scale, center + forward * 28.0f * scale, Alpha(Paper, 0.48f * invuln), 1.6f * scale, true);
                DrawCircle(center, 13.0f * scale, Alpha(Graphite, 0.94f * invuln));
                DrawCircle(center, 9.5f * scale, Alpha(polarity, 0.84f * invuln), false, 2.2f * scale, true);
                DrawCircle(center, 18.0f * scale, Alpha(polarity, 0.16f * invuln), false, 1.2f * scale, true);
                break;
            }
            default:
            {
                Vector2[] hull =
                {
                    center + forward * 36.0f * scale,
                    center + right * 18.0f * scale - forward * 22.0f * scale,
                    center - forward * 12.0f * scale,
                    center - right * 18.0f * scale - forward * 22.0f * scale,
                };
                DrawColoredPolygon(hull, body, Array.Empty<Vector2>(), null);
                DrawPolyline(ClosePolygon(hull), line, 2.0f * scale, true);
                DrawLine(hull[1], hull[3], softLine, 1.5f * scale, true);
                DrawLine(center - forward * 8.0f * scale, center + forward * 34.0f * scale, Alpha(Paper, 0.55f * invuln), 1.6f * scale, true);
                DrawCircle(center, 13.0f * scale, Alpha(Graphite, 0.96f * invuln));
                DrawCircle(center, 10.0f * scale, Alpha(polarity, 0.84f * invuln), false, 2.4f * scale, true);
                break;
            }
        }
    }

    private void DrawEnemy(Enemy enemy)
    {
        Vector2 p = enemy.Pos + ShakeOffset();
        Color color = EnemyStateColor(enemy);
        float hp = Mathf.Clamp(enemy.Hp / enemy.MaxHp, 0.0f, 1.0f);
        float pulse = 1.0f + enemy.SpawnPulse * 0.7f + Mathf.Sin(_time * 5.0f + enemy.Phase) * 0.05f;
        bool heavyVisualLoad = _visualPressure > 0.72f && enemy.Kind != EnemyKind.Boss && !enemy.Elite;
        float charge = EnemyTelegraph01(enemy);
        float overheat = EnemyOverheat01(enemy);

        if (!heavyVisualLoad)
        {
            DrawGlow(p, charge > 0.0f ? EnemyBulletColor() : color, enemy.Radius * (1.55f + enemy.SpawnPulse * 0.55f + charge * 0.65f + overheat * 0.28f), enemy.Kind == EnemyKind.Boss ? 0.08f : 0.025f + charge * 0.04f, 3);
        }
        DrawCircle(p, enemy.Radius * pulse, Alpha(color, heavyVisualLoad ? 0.12f : 0.2f), false, heavyVisualLoad ? 1.2f : 1.6f + enemy.SpawnPulse * 2.0f, true);
        if (charge > 0.0f && !heavyVisualLoad)
        {
            Vector2 aim = (_playerPos - enemy.Pos).LengthSquared() > 0.01f ? (_playerPos - enemy.Pos).Normalized() : Vector2.Down;
            DrawLine(p + aim * enemy.Radius * 0.8f, p + aim * (enemy.Radius + 62.0f + charge * 36.0f), Alpha(EnemyBulletColor(), 0.18f + charge * 0.32f), UiHairline + charge * 0.55f, true);
            DrawCircle(p, enemy.Radius * (1.16f + charge * 0.22f), Alpha(EnemyBulletColor(), 0.24f + charge * 0.18f), false, UiHairline, true);
        }
        else if (overheat > 0.0f && !heavyVisualLoad)
        {
            DrawCircle(p, enemy.Radius * (1.18f + overheat * 0.12f), Alpha(Gold, 0.18f + overheat * 0.16f), false, UiHairline, true);
        }

        if (enemy.Kind == EnemyKind.Boss)
        {
            DrawBoss(enemy, p, color, hp);
            return;
        }

        int sides = enemy.Kind switch
        {
            EnemyKind.Chaser => 3,
            EnemyKind.Weaver => 5,
            EnemyKind.Turret => 8,
            EnemyKind.Splitter => 6,
            EnemyKind.Lance => 4,
            EnemyKind.Mine => 12,
            EnemyKind.Shard => 4,
            EnemyKind.Warden => 9,
            EnemyKind.Drifter => 7,
            EnemyKind.Bulwark => 4,
            EnemyKind.Siren => 10,
            EnemyKind.Harrier => 3,
            _ => 5,
        };
        float rotation = _time * (enemy.Kind == EnemyKind.Turret ? -0.8f : 1.2f) + enemy.Phase;
        Vector2[] body = RegularPolygon(p, enemy.Radius * (0.95f + enemy.SpawnPulse * 0.28f), sides, rotation);
        Color bodyColor = enemy.Elite ? Steel.Lerp(Paper, 0.12f) : Steel.Lerp(Graphite, 0.28f);
        DrawColoredPolygon(body, Alpha(bodyColor, 0.72f), Array.Empty<Vector2>(), null);
        DrawPolyline(ClosePolygon(body), Alpha(color, 0.78f), 2.0f, true);
        if (enemy.Elite)
        {
            DrawPolyline(ClosePolygon(RegularPolygon(p, enemy.Radius * 1.22f, sides, rotation)), Alpha(AlertRed, 0.64f), 2.0f, true);
        }
        DrawCircle(p, enemy.Radius * 0.28f, Alpha(Ink, 0.92f));
        DrawCircle(p, enemy.Radius * 0.24f, Alpha(color, 0.86f), false, Mathf.Max(1.5f, enemy.Radius * 0.07f), true);
        DrawEnemyMark(enemy, p, color, rotation);
        DrawRect(new Rect2(p + new Vector2(-enemy.Radius, enemy.Radius + 12.0f), new Vector2(enemy.Radius * 2.0f, 5.0f)), Alpha(Paper, 0.13f), true);
        DrawRect(new Rect2(p + new Vector2(-enemy.Radius, enemy.Radius + 12.0f), new Vector2(enemy.Radius * 2.0f * hp, 5.0f)), Alpha(color, 0.78f), true);
    }

    private void DrawEnemyMark(Enemy enemy, Vector2 p, Color color, float rotation)
    {
        float r = enemy.Radius;
        Color line = Alpha(Paper, 0.48f);
        Color accent = Alpha(color, 0.72f);
        Vector2 a = Vector2.Right.Rotated(rotation);
        Vector2 b = Vector2.Down.Rotated(rotation);

        switch (enemy.Kind)
        {
            case EnemyKind.Chaser:
                DrawLine(p - a * r * 0.14f, p + a * r * 0.54f, line, UiStroke, true);
                break;
            case EnemyKind.Weaver:
                DrawLine(p - a * r * 0.45f, p + a * r * 0.45f, line, UiHairline, true);
                DrawLine(p - b * r * 0.28f, p + b * r * 0.28f, accent, UiHairline, true);
                break;
            case EnemyKind.Turret:
                DrawLine(p - a * r * 0.58f, p + a * r * 0.58f, line, UiHairline, true);
                DrawLine(p - b * r * 0.58f, p + b * r * 0.58f, line, UiHairline, true);
                break;
            case EnemyKind.Splitter:
                DrawLine(p - b * r * 0.5f, p + b * r * 0.5f, line, UiStroke, true);
                DrawCircle(p, r * 0.42f, Alpha(color, 0.18f), false, UiHairline, true);
                break;
            case EnemyKind.Lance:
                DrawLine(p - a * r * 0.62f, p + a * r * 0.62f, accent, UiAccentStroke, true);
                DrawLine(p + a * r * 0.26f - b * r * 0.18f, p + a * r * 0.54f, line, UiHairline, true);
                DrawLine(p + a * r * 0.26f + b * r * 0.18f, p + a * r * 0.54f, line, UiHairline, true);
                break;
            case EnemyKind.Mine:
                DrawCircle(p, r * 0.48f, accent, false, UiHairline, true);
                DrawCircle(p, r * 0.16f, line);
                break;
            case EnemyKind.Shard:
                DrawLine(p - a * r * 0.5f - b * r * 0.2f, p + a * r * 0.5f + b * r * 0.2f, accent, UiStroke, true);
                break;
            case EnemyKind.Warden:
                DrawPolyline(ClosePolygon(RegularPolygon(p, r * 0.52f, 4, rotation + Mathf.Pi * 0.25f)), line, UiHairline, true);
                DrawCircle(p, r * 0.16f, accent);
                break;
            case EnemyKind.Drifter:
                DrawArc(p, r * 0.48f, rotation - 1.95f, rotation + 1.2f, 20, accent, UiStroke, true);
                DrawLine(p - b * r * 0.32f, p + b * r * 0.32f, line, UiHairline, true);
                break;
            case EnemyKind.Bulwark:
                DrawPolyline(ClosePolygon(RegularPolygon(p, r * 0.5f, 4, rotation)), line, UiStroke, true);
                DrawLine(p - a * r * 0.36f, p + a * r * 0.36f, accent, UiStroke, true);
                break;
            case EnemyKind.Siren:
                DrawCircle(p, r * 0.5f, Alpha(color, 0.2f), false, UiHairline, true);
                DrawArc(p, r * 0.42f, rotation, rotation + Mathf.Pi * 1.35f, 22, accent, UiStroke, true);
                break;
            case EnemyKind.Harrier:
                DrawLine(p - a * r * 0.48f - b * r * 0.28f, p + a * r * 0.52f, accent, UiStroke, true);
                DrawLine(p - a * r * 0.48f + b * r * 0.28f, p + a * r * 0.52f, accent, UiStroke, true);
                break;
        }
    }

    private void DrawBoss(Enemy enemy, Vector2 p, Color color, float hp)
    {
        float rotation = _time * 0.35f;
        Vector2[] core = RegularPolygon(p, enemy.Radius, 8, rotation);
        DrawColoredPolygon(core, Alpha(Graphite, 0.86f), Array.Empty<Vector2>(), null);
        DrawPolyline(ClosePolygon(core), Alpha(color, 0.86f), 3.0f, true);
        DrawPolyline(ClosePolygon(RegularPolygon(p, enemy.Radius * 1.38f, 8, rotation)), Alpha(GridLine, 0.28f), 2.0f, true);
        DrawCircle(p, enemy.Radius * 0.52f, Alpha(Ink, 0.88f));
        DrawCircle(p, enemy.Radius * 0.34f, Alpha(color, 0.84f), false, 4.0f, true);
        DrawLine(p - Vector2.Right.Rotated(rotation) * enemy.Radius * 0.62f, p + Vector2.Right.Rotated(rotation) * enemy.Radius * 0.62f, Alpha(Paper, 0.45f), 2.0f, true);
        DrawLine(p - Vector2.Down.Rotated(rotation) * enemy.Radius * 0.62f, p + Vector2.Down.Rotated(rotation) * enemy.Radius * 0.62f, Alpha(Paper, 0.28f), 2.0f, true);

        Rect2 bossBar = new(new Vector2(360.0f, 48.0f), new Vector2(1200.0f, 18.0f));
        DrawRect(bossBar, Alpha(Paper, 0.16f), true);
        DrawRect(new Rect2(bossBar.Position, new Vector2(bossBar.Size.X * hp, bossBar.Size.Y)), Alpha(Rose, 0.9f), true);
        DrawRect(bossBar, Alpha(Paper, 0.62f), false, UiStroke, true);
        DrawText(Tf("boss.sector", T(CurrentSector().NameKey)), bossBar.Position + new Vector2(0.0f, -8.0f), 22, Alpha(Paper, 0.72f), HorizontalAlignment.Left, 420.0f, true, 3);
    }

    private void DrawShot(Shot shot)
    {
        Color color = ShotVisualColor(shot);
        Vector2 pos = shot.Pos + ShakeOffset();
        if (shot.FromPlayer)
        {
            bool heavyVisualLoad = _visualPressure > 0.78f;
            DrawShotTrail(shot, pos, color, heavyVisualLoad);
            if (!heavyVisualLoad)
            {
                DrawCircle(pos, shot.Radius * 1.05f, Alpha(Paper, 0.78f));
                DrawCircle(pos, shot.Radius * 1.8f, Alpha(color, 0.32f), false, 1.0f, true);
            }
        }
        else
        {
            bool heavyVisualLoad = _visualPressure > 0.68f;
            if (!heavyVisualLoad)
            {
                DrawGlow(pos, color, shot.Radius * 4.0f, 0.08f, 3);
                DrawCircle(pos, shot.Radius * 1.82f, Alpha(Void, 0.74f));
            }
            DrawShotTrail(shot, pos, color, heavyVisualLoad);
            DrawCircle(pos, shot.Radius * (heavyVisualLoad ? 1.22f : 1.55f), Alpha(color, heavyVisualLoad ? 0.92f : 0.86f), false, heavyVisualLoad ? 1.8f : 2.5f, true);
            DrawCircle(pos, shot.Radius * 0.88f, Alpha(Graphite, 0.92f));
            DrawCircle(pos, shot.Radius * 0.34f, Alpha(Paper, 0.92f));
        }
    }

    private void DrawShotTrail(Shot shot, Vector2 current, Color color, bool heavyVisualLoad)
    {
        if (shot.TrailCount <= 0)
        {
            return;
        }

        int maxSegments = shot.FromPlayer ? (heavyVisualLoad ? 2 : (shot.Rift ? 4 : 3)) : (heavyVisualLoad ? 1 : 2);
        int count = Math.Min(shot.TrailCount, maxSegments);
        Vector2 shake = ShakeOffset();
        Vector2 from = current;
        for (int i = 0; i < count; i++)
        {
            Vector2 to = ShotTrailPoint(shot, i) + shake;
            if (from.DistanceSquaredTo(to) < 2.0f)
            {
                continue;
            }

            float t = (i + 1.0f) / (count + 1.0f);
            if (shot.FromPlayer)
            {
                float width = Mathf.Lerp(shot.Rift ? 8.0f : 9.5f, 1.8f, t);
                float alpha = (shot.Rift ? 0.34f : 0.24f) * (1.0f - t);
                if (!heavyVisualLoad)
                {
                    DrawLine(from, to, Alpha(Paper, alpha * 0.52f), width * 1.55f, true);
                }
                DrawLine(from, to, Alpha(color, alpha * 1.2f), width * 0.52f, true);
                DrawLine(from, to, Alpha(Paper, 0.46f * (1.0f - t)), Mathf.Max(1.0f, width * 0.18f), true);
            }
            else
            {
                float width = Mathf.Lerp(shot.Radius * 1.15f, 1.0f, t);
                float alpha = (heavyVisualLoad ? 0.18f : 0.26f) * (1.0f - t);
                DrawLine(from, to, Alpha(color, alpha), width, true);
                if (!heavyVisualLoad)
                {
                    DrawLine(from, to, Alpha(Paper, alpha * 0.28f), 1.0f, true);
                }
            }

            from = to;
        }
    }

    private static Vector2 ShotTrailPoint(Shot shot, int index)
    {
        return index switch
        {
            0 => shot.Trail0,
            1 => shot.Trail1,
            2 => shot.Trail2,
            _ => shot.Trail3,
        };
    }

    private void DrawPickup(Pickup pickup)
    {
        Color accent = PickupColor(pickup.Kind);
        Vector2 pos = pickup.Pos + ShakeOffset();
        float pulse = 1.0f + Mathf.Sin(_time * 7.0f + pickup.Pos.X) * 0.08f;
        float radius = pickup.Radius * (pickup.Kind == PickupKind.Repair ? 1.42f : 1.68f) * pulse;
        float rotation = Mathf.Pi * 0.25f;
        Vector2[] outer = RegularPolygon(pos, radius, 4, rotation);
        Vector2[] inner = RegularPolygon(pos, radius * 0.68f, 4, rotation);

        DrawColoredPolygon(outer, Alpha(Graphite, 0.88f), Array.Empty<Vector2>(), null);
        DrawPolyline(ClosePolygon(outer), Alpha(Paper, 0.42f), 1.35f, true);
        DrawPolyline(ClosePolygon(inner), Alpha(accent, 0.62f), UiHairline, true);
        DrawLine(pos + new Vector2(-radius * 0.52f, radius * 0.52f), pos + new Vector2(radius * 0.52f, radius * 0.52f), Alpha(Void, 0.52f), UiHairline, true);

        if (pickup.Kind == PickupKind.Repair)
        {
            DrawLine(pos - Vector2.Right * radius * 0.34f, pos + Vector2.Right * radius * 0.34f, Alpha(accent, 0.9f), 2.1f, true);
            DrawLine(pos - Vector2.Down * radius * 0.34f, pos + Vector2.Down * radius * 0.34f, Alpha(accent, 0.9f), 2.1f, true);
            DrawLine(pos - Vector2.Right * radius * 0.34f, pos + Vector2.Right * radius * 0.34f, Alpha(Paper, 0.42f), UiHairline, true);
        }
        else if (pickup.Kind == PickupKind.Energy)
        {
            Rect2 cell = new(pos - new Vector2(radius * 0.34f, radius * 0.23f), new Vector2(radius * 0.68f, radius * 0.46f));
            DrawRect(cell, Alpha(accent, 0.12f), true);
            DrawRect(cell, Alpha(accent, 0.78f), false, UiHairline, true);
            DrawLine(cell.Position + new Vector2(cell.Size.X * 0.22f, cell.Size.Y * 0.5f), cell.Position + new Vector2(cell.Size.X * 0.78f, cell.Size.Y * 0.5f), Alpha(Paper, 0.62f), 1.4f, true);
            DrawLine(cell.End + new Vector2(0.0f, -cell.Size.Y * 0.32f), cell.End + new Vector2(radius * 0.12f, -cell.Size.Y * 0.32f), Alpha(accent, 0.78f), UiHairline, true);
        }
        else
        {
            Vector2[] core = RegularPolygon(pos, radius * 0.32f, 4, rotation);
            DrawColoredPolygon(core, Alpha(accent, 0.34f), Array.Empty<Vector2>(), null);
            DrawPolyline(ClosePolygon(core), Alpha(accent, 0.82f), UiHairline, true);
            DrawCircle(pos, radius * 0.08f, Alpha(Paper, 0.64f));
        }

        DrawPolyline(ClosePolygon(RegularPolygon(pos, radius * 1.2f, 4, rotation)), Alpha(Paper, 0.12f), UiHairline, true);
    }

    private void DrawParticle(Particle particle)
    {
        float t = Mathf.Clamp(particle.Life / particle.MaxLife, 0.0f, 1.0f);
        Vector2 pos = particle.Pos + ShakeOffset();
        if (_visualPressure < 0.82f && particle.Vel.LengthSquared() > 2400.0f)
        {
            Vector2 dir = particle.Vel.Normalized();
            DrawLine(pos - dir * particle.Size * 2.0f, pos + dir * particle.Size * 0.6f, Alpha(particle.Color, t * 0.35f), Mathf.Max(1.0f, particle.Size * 0.34f), true);
        }
        DrawCircle(pos, particle.Size * (0.35f + t), Alpha(particle.Color, t * 0.62f));
    }

    private void DrawHazard(HazardLine hazard)
    {
        float age = hazard.MaxLife - hazard.Life;
        bool active = age > hazard.Warmup;
        float warm = Mathf.Clamp(age / Mathf.Max(0.01f, hazard.Warmup), 0.0f, 1.0f);
        float fade = Mathf.Clamp(hazard.Life / hazard.MaxLife, 0.0f, 1.0f);
        Color color = hazard.Color;
        float width = active ? hazard.Width : 3.0f + warm * 7.0f;
        DrawLine(hazard.A + ShakeOffset(), hazard.B + ShakeOffset(), Alpha(color, active ? 0.34f * fade : 0.58f * warm), width * 3.0f, true);
        DrawLine(hazard.A + ShakeOffset(), hazard.B + ShakeOffset(), Alpha(Paper, active ? 0.42f * fade : 0.62f * warm), active ? 4.0f : 2.0f, true);
        if (!active)
        {
            DrawLine(hazard.A + ShakeOffset(), hazard.B + ShakeOffset(), Alpha(color, 0.55f * Mathf.Sin(warm * Mathf.Pi)), 1.0f, true);
            Vector2 dir = (hazard.B - hazard.A).Normalized();
            for (int i = 1; i <= 7; i++)
            {
                Vector2 center = hazard.A.Lerp(hazard.B, i / 8.0f) + ShakeOffset();
                Vector2 side = dir.Orthogonal() * (12.0f + warm * 12.0f);
                DrawLine(center - side, center + side, Alpha(color, 0.18f + warm * 0.18f), 1.0f, true);
            }
        }
    }

    private void DrawHud()
    {
        Color polarity = PolarityColor(_playerPolarity);
        Rect2 top = new(new Vector2(74.0f, 16.0f), new Vector2(1772.0f, 62.0f));
        DrawGlow(top.Position + top.Size * 0.5f, CurrentSector().Accent, 230.0f, 0.026f, 4);
        DrawPanel(top, Alpha(Graphite, 0.82f), Alpha(GridLine, 0.34f));
        DrawLine(new Vector2(top.Position.X + 12.0f, top.End.Y - 1.0f), new Vector2(top.End.X - 12.0f, top.End.Y - 1.0f), Alpha(CurrentSector().Accent, 0.28f), UiHairline, true);

        Rect2 polarityCard = new(new Vector2(92.0f, 23.0f), new Vector2(220.0f, 48.0f));
        DrawPanel(polarityCard, Alpha(Ink, 0.46f), Alpha(polarity, 0.5f));
        DrawCircle(polarityCard.Position + new Vector2(25.0f, 20.0f), 11.0f, Alpha(Graphite, 0.95f));
        DrawCircle(polarityCard.Position + new Vector2(25.0f, 20.0f), 8.0f, Alpha(polarity, 0.9f), false, UiStroke, true);
        DrawText(_playerPolarity == 0 ? T("hud.cyan_resonance") : T("hud.gold_resonance"), polarityCard.Position + new Vector2(46.0f, 22.0f), 15, Paper, HorizontalAlignment.Left, 165.0f, true, 2);
        string switchText = _polarityCooldown > 0.0f
            ? Tf("hud.resonance_cooldown", _polarityCooldown)
            : _playerPolarity == 0
                ? $"{T("hud.cruise_charge")} {Mathf.RoundToInt(CruiseCharge01() * 100.0f)}%"
                : _assaultBurstTimer > 0.0f ? Tf("hud.assault_window", _assaultBurstTimer) : T("hud.resonance_ready");
        DrawText(switchText, polarityCard.Position + new Vector2(46.0f, 39.0f), 11, Alpha(_polarityCooldown <= 0.0f ? XpGreen : Paper, 0.68f), HorizontalAlignment.Left, 165.0f, false, 0);
        Rect2 polarityBar = new(polarityCard.Position + new Vector2(14.0f, 41.0f), new Vector2(polarityCard.Size.X - 28.0f, 3.0f));
        DrawRect(polarityBar, Alpha(Paper, 0.08f), true);
        float stanceMeter = _playerPolarity == 0 ? CruiseCharge01() : Mathf.Clamp(_assaultBurstTimer / AssaultBurstMax, 0.0f, 1.0f);
        float meter = _polarityCooldown > 0.0f ? PolaritySwitchReady01() : stanceMeter;
        DrawRect(new Rect2(polarityBar.Position, new Vector2(polarityBar.Size.X * meter, polarityBar.Size.Y)), Alpha(polarity, _polarityCooldown <= 0.0f ? 0.9f : 0.58f), true);

        DrawBar(new Rect2(new Vector2(342.0f, 46.0f), new Vector2(280.0f, 12.0f)), _hudHullValue, _hudHullTrail, AlertRed, T("hud.hull"), $"{Mathf.CeilToInt(_playerHp)}/{Mathf.CeilToInt(_playerMaxHp)}");
        DrawBar(new Rect2(new Vector2(648.0f, 46.0f), new Vector2(280.0f, 12.0f)), _hudEnergyValue, _hudEnergyTrail, polarity, T("hud.energy"), $"{Mathf.FloorToInt(_energy)}/{Mathf.FloorToInt(_maxEnergy)}");
        float dashReady = Mathf.Clamp(1.0f - Mathf.Max(_dashCooldown, 0.0f) / 0.86f, 0.0f, 1.0f);
        DrawBar(new Rect2(new Vector2(954.0f, 46.0f), new Vector2(200.0f, 12.0f)), _hudDashValue, _hudDashTrail, XpGreen, T("hud.dash"), $"{Mathf.RoundToInt(dashReady * 100.0f)}%");

        DrawHudMetric(new Rect2(new Vector2(1184.0f, 26.0f), new Vector2(130.0f, 42.0f)), T("hud.sector.label"), $"{CurrentSectorIndex() + 1}/5", CurrentSector().Accent);
        DrawHudMetric(new Rect2(new Vector2(1328.0f, 26.0f), new Vector2(150.0f, 42.0f)), T("hud.wave.label"), $"{_wave}/40", Paper);
        DrawHudMetric(new Rect2(new Vector2(1492.0f, 26.0f), new Vector2(230.0f, 42.0f)), T("hud.score.label"), $"{_score:000000}", Paper);
        DrawHudMetric(new Rect2(new Vector2(1728.0f, 26.0f), new Vector2(68.0f, 42.0f)), T("hud.combo.label"), $"{Math.Max(1, _combo)}", _combo > 8 ? XpGreen : Paper);
        DrawSettingsButton(HudSettingsButtonRect(), false);
        DrawUpgradeIcons();
        DrawRunObjectives();
        DrawTransientPolarityTip(polarity);
    }

    private void DrawTransientPolarityTip(Color current)
    {
        if (_polarityTipTimer <= 0.0f)
        {
            return;
        }

        float a = Mathf.Clamp(_polarityTipTimer / 2.4f, 0.0f, 1.0f);
        Rect2 tip = new(new Vector2(702.0f, 88.0f), new Vector2(516.0f, 36.0f));
        DrawPanel(tip, Alpha(Ink, 0.42f * a), Alpha(current, 0.3f + a * 0.34f));
        DrawText(PolarityTipText(), tip.Position + new Vector2(0.0f, 25.0f), 17, Alpha(current, 0.42f + a * 0.48f), HorizontalAlignment.Center, tip.Size.X, true, 2);
    }

    private void DrawUpgradeIcons()
    {
        if (_upgradeOrder.Count == 0)
        {
            return;
        }

        const int columns = 8;
        const float size = 34.0f;
        const float gap = 7.0f;
        Vector2 origin = new(92.0f, 94.0f);
        int visibleCount = 0;
        UpgradeId? hoverId = null;
        Rect2 hoverRect = default;
        Vector2 mouse = GetGlobalMousePosition();

        for (int i = 0; i < _upgradeOrder.Count; i++)
        {
            UpgradeId id = _upgradeOrder[i];
            int rank = GetRank(id);
            if (rank <= 0)
            {
                continue;
            }

            int column = visibleCount % columns;
            int row = visibleCount / columns;
            Rect2 rect = new(origin + new Vector2(column * (size + gap), row * (size + gap)), new Vector2(size, size));
            bool hover = rect.HasPoint(mouse);
            Color accent = UpgradeAccent(id);
            DrawUpgradeIcon(rect, id, rank, accent, hover);
            if (hover)
            {
                hoverId = id;
                hoverRect = rect;
            }
            visibleCount++;
        }

        if (visibleCount > 0)
        {
            DrawText(T("hud.build"), origin + new Vector2(0.0f, -8.0f), 13, Alpha(Paper, 0.5f), HorizontalAlignment.Left, 160.0f, false, 0);
        }

        if (hoverId.HasValue)
        {
            DrawUpgradeTooltip(hoverRect, hoverId.Value);
        }
    }

    private void DrawUpgradeIcon(Rect2 rect, UpgradeId id, int rank, Color accent, bool hover)
    {
        Rect2 drawRect = hover ? rect.Grow(3.0f) : rect;
        DrawPanel(drawRect, Alpha(Graphite, hover ? 0.76f : 0.58f), Alpha(accent, hover ? 0.72f : 0.42f));
        DrawUpgradeGlyph(id, drawRect.Position + drawRect.Size * 0.5f, drawRect.Size.X * 0.42f, accent);

        Vector2 badge = drawRect.End - new Vector2(7.0f, 7.0f);
        DrawCircle(badge, 8.0f, Alpha(Ink, 0.92f));
        DrawCircle(badge, 8.0f, Alpha(accent, 0.78f), false, UiStroke, true);
        string rankText = rank > 9 ? "+" : rank.ToString(System.Globalization.CultureInfo.InvariantCulture);
        DrawText(rankText, badge + new Vector2(-8.0f, 5.5f), 12, Paper, HorizontalAlignment.Center, 16.0f, false, 0);
    }

    private void DrawUpgradeGlyph(UpgradeId id, Vector2 center, float radius, Color accent)
    {
        Color line = Alpha(accent, 0.88f);
        Color soft = Alpha(accent, 0.18f);
        switch (id)
        {
            case UpgradeId.PrismArray:
            case UpgradeId.AstraRefraction:
            case UpgradeId.AstraPrismWake:
            case UpgradeId.AstraNovaBloom:
            case UpgradeId.AstraTwinRefraction:
                for (int i = -1; i <= 1; i++)
                {
                    DrawLine(center + new Vector2(-radius * 0.75f, i * radius * 0.34f), center + new Vector2(radius * 0.75f, i * radius * 0.34f), line, UiStroke, true);
                }
                break;
            case UpgradeId.KineticBloom:
            case UpgradeId.CometTrail:
            case UpgradeId.SolCoronaBloom:
            case UpgradeId.SolFlareCore:
                DrawLine(center + new Vector2(-radius * 0.6f, -radius * 0.5f), center + new Vector2(radius * 0.15f, 0.0f), line, UiStroke, true);
                DrawLine(center + new Vector2(-radius * 0.6f, radius * 0.5f), center + new Vector2(radius * 0.15f, 0.0f), line, UiStroke, true);
                DrawLine(center + new Vector2(0.0f, -radius * 0.5f), center + new Vector2(radius * 0.72f, 0.0f), Alpha(Paper, 0.6f), UiHairline, true);
                DrawLine(center + new Vector2(0.0f, radius * 0.5f), center + new Vector2(radius * 0.72f, 0.0f), Alpha(Paper, 0.6f), UiHairline, true);
                break;
            case UpgradeId.VitalShell:
            case UpgradeId.MirrorSkin:
            case UpgradeId.AegisBloom:
                DrawDiamond(center, radius * 0.72f, accent, 0.0f);
                DrawLine(center - Vector2.Right * radius * 0.45f, center + Vector2.Right * radius * 0.45f, Alpha(Paper, 0.65f), UiHairline, true);
                break;
            case UpgradeId.GravityWell:
            case UpgradeId.ResonanceLeech:
            case UpgradeId.KairoSwarmSync:
            case UpgradeId.KairoRelayProtocol:
                DrawCircle(center, radius * 0.72f, soft);
                DrawCircle(center, radius * 0.72f, line, false, UiStroke, true);
                DrawCircle(center, radius * 0.22f, Alpha(Paper, 0.74f));
                break;
            case UpgradeId.MoonWisp:
            case UpgradeId.QuantumEcho:
            case UpgradeId.PolarityStorm:
            case UpgradeId.KairoDroneBay:
            case UpgradeId.KairoOverrideMatrix:
                DrawCircle(center, radius * 0.6f, Alpha(accent, 0.12f), false, UiStroke, true);
                DrawCircle(center + new Vector2(radius * 0.46f, -radius * 0.28f), radius * 0.2f, line);
                DrawCircle(center - new Vector2(radius * 0.42f, -radius * 0.24f), radius * 0.16f, Alpha(Paper, 0.58f));
                break;
            case UpgradeId.RiftNeedle:
            case UpgradeId.RailHeart:
            case UpgradeId.SolarThesis:
            case UpgradeId.VesperCapacitor:
            case UpgradeId.VesperSplitRail:
            case UpgradeId.VesperJudgmentCoil:
            case UpgradeId.VesperSeverLine:
                DrawLine(center - Vector2.Right * radius * 0.72f, center + Vector2.Right * radius * 0.72f, line, UiAccentStroke, true);
                DrawCircle(center + Vector2.Right * radius * 0.42f, radius * 0.18f, Alpha(Paper, 0.72f));
                break;
            case UpgradeId.CoolantLattice:
            case UpgradeId.NovaCapacitor:
            case UpgradeId.BulletTransmute:
            case UpgradeId.SolSolarForge:
            case UpgradeId.SolRadiantMantle:
                DrawCircle(center, radius * 0.62f, soft);
                DrawLine(center - Vector2.Down * radius * 0.52f, center + Vector2.Down * radius * 0.52f, line, UiStroke, true);
                DrawLine(center - Vector2.Right * radius * 0.52f, center + Vector2.Right * radius * 0.52f, Alpha(Paper, 0.48f), UiHairline, true);
                break;
            case UpgradeId.GlassCannon:
            case UpgradeId.BountyContract:
            case UpgradeId.OneWaveOverdrive:
            case UpgradeId.HarmonicMap:
            case UpgradeId.EmergencyRepair:
                DrawDiamond(center, radius * 0.68f, accent, Mathf.Pi * 0.25f);
                DrawCircle(center, radius * 0.18f, Alpha(Paper, 0.72f));
                break;
        }
    }

    private void DrawUpgradeTooltip(Rect2 anchor, UpgradeId id)
    {
        int rank = GetRank(id);
        Rect2 tip = new(anchor.Position + new Vector2(0.0f, 42.0f), new Vector2(284.0f, 58.0f));
        if (tip.End.X > ScreenWidth - 24.0f)
        {
            tip.Position = new Vector2(ScreenWidth - tip.Size.X - 24.0f, tip.Position.Y);
        }

        Color accent = UpgradeAccent(id);
        DrawPanel(tip, Alpha(Ink, 0.86f), Alpha(accent, 0.62f));
        DrawText(UpgradeTitle(id).ToUpperInvariant(), tip.Position + new Vector2(14.0f, 23.0f), 17, Paper, HorizontalAlignment.Left, tip.Size.X - 28.0f, true, 2);
        DrawText(Tf("rank", rank), tip.Position + new Vector2(14.0f, 45.0f), 15, Alpha(accent, 0.92f), HorizontalAlignment.Left, tip.Size.X - 28.0f, false, 0);
    }

    private void DrawRunObjectives()
    {
        if (_runObjectives.Count == 0 || _mode == GameMode.Title || _mode == GameMode.Meta)
        {
            return;
        }

        Rect2 panel = new(new Vector2(1416.0f, 92.0f), new Vector2(408.0f, 46.0f + _runObjectives.Count * 36.0f));
        DrawPanel(panel, Alpha(Ink, 0.44f), Alpha(CurrentSector().Accent, 0.28f));
        DrawText(T("hud.objectives"), panel.Position + new Vector2(14.0f, 22.0f), 13, Alpha(Paper, 0.62f), HorizontalAlignment.Left, 210.0f, false, 0);
        string bonusText = _language == GameLanguage.Chinese ? $"奖励 +{_runObjectiveBonusDust}" : $"BONUS +{_runObjectiveBonusDust}";
        DrawText(bonusText, panel.Position + new Vector2(panel.Size.X - 132.0f, 22.0f), 13, Alpha(Gold, 0.72f), HorizontalAlignment.Right, 116.0f, false, 0);
        DrawLine(panel.Position + new Vector2(12.0f, 30.0f), panel.Position + new Vector2(panel.Size.X - 12.0f, 30.0f), Alpha(CurrentSector().Accent, 0.18f), UiHairline, true);

        for (int i = 0; i < _runObjectives.Count; i++)
        {
            RunObjective objective = _runObjectives[i];
            float progress = objective.Target <= 0 ? 1.0f : Mathf.Clamp(objective.Progress / (float)objective.Target, 0.0f, 1.0f);
            Color accent = objective.Completed ? Jade : CurrentSector().Accent;
            Rect2 row = new(panel.Position + new Vector2(10.0f, 36.0f + i * 36.0f), new Vector2(panel.Size.X - 20.0f, 30.0f));
            Rect2 bar = new(row.Position + new Vector2(25.0f, 23.0f), new Vector2(row.Size.X - 38.0f, 4.0f));
            DrawRect(row, Alpha(Graphite, objective.Completed ? 0.34f : 0.22f), true);
            DrawCircle(row.Position + new Vector2(11.0f, 15.0f), 4.5f, Alpha(accent, objective.Completed ? 0.86f : 0.52f));
            DrawText(ObjectiveText(objective), row.Position + new Vector2(24.0f, 16.0f), 13, objective.Completed ? Alpha(Jade, 0.9f) : Alpha(Paper, 0.74f), HorizontalAlignment.Left, row.Size.X - 112.0f, false, 0);
            DrawText(objective.Completed ? (_language == GameLanguage.Chinese ? "完成" : "DONE") : $"{objective.Progress}/{objective.Target}", row.Position + new Vector2(row.Size.X - 88.0f, 16.0f), 13, Alpha(accent, 0.84f), HorizontalAlignment.Right, 78.0f, false, 0);
            DrawRect(bar, Alpha(Paper, 0.08f), true);
            DrawRect(new Rect2(bar.Position, new Vector2(bar.Size.X * progress, bar.Size.Y)), Alpha(accent, objective.Completed ? 0.86f : 0.62f), true);
        }
    }

    private void DrawTitle()
    {
        float pulse = 0.55f + 0.45f * Mathf.Sin(_time * 3.2f);
        DrawText(TitleName(), new Vector2(0.0f, 192.0f), _language == GameLanguage.Chinese ? 72 : 66, Alpha(Paper, 0.58f), HorizontalAlignment.Center, ScreenWidth, false, 0);
        DrawText(TitleStartPrompt(), new Vector2(0.0f, 596.0f), 28, Alpha(Gold, 0.34f + pulse * 0.24f), HorizontalAlignment.Center, ScreenWidth, false, 0);

        DrawText(Tf("meta.wallet", _starDust), new Vector2(56.0f, 70.0f), 18, Alpha(Gold, 0.62f), HorizontalAlignment.Left, 340.0f, false, 0);
        DrawText(Tf("meta.best", _bestWave, _bestScore, _runsCompleted), new Vector2(56.0f, 100.0f), 15, Alpha(Paper, 0.36f), HorizontalAlignment.Left, 760.0f, false, 0);
        DrawPilotSelect();

        DrawTitleTextButton(MetaButtonRect(), T("menu.meta"), Gold);
        DrawTitleTextButton(TitleSettingsButtonRect(), T("menu.settings"), Alpha(Paper, 0.68f));

        if (_wonOnce)
        {
            DrawText(T("title.won_once"), new Vector2(0.0f, 990.0f), 16, Alpha(Gold, 0.54f), HorizontalAlignment.Center, ScreenWidth, false, 0);
        }
    }

    private void DrawTitleShuttle(Vector2 center, float scale)
    {
        Vector2 forward = Vector2.Up;
        Vector2 right = Vector2.Right;
        float trailPulse = 0.65f + 0.35f * Mathf.Sin(_time * 9.0f);

        for (int i = 0; i < 9; i++)
        {
            float t = i / 8.0f;
            float length = Mathf.Lerp(52.0f, 220.0f, t) * scale;
            float alpha = (1.0f - t) * (0.28f + trailPulse * 0.18f);
            Vector2 basePos = center - forward * 28.0f * scale + right * (i - 4) * 2.2f * scale;
            DrawLine(basePos, basePos - forward * length + right * (i - 4) * 0.4f * scale, Alpha(Paper, alpha), Mathf.Lerp(4.0f, 1.0f, t) * scale, true);
        }

        Vector2[] hull =
        {
            center + forward * 38.0f * scale,
            center + right * 13.0f * scale - forward * 1.0f * scale,
            center + right * 24.0f * scale - forward * 26.0f * scale,
            center + right * 6.0f * scale - forward * 17.0f * scale,
            center - forward * 34.0f * scale,
            center - right * 6.0f * scale - forward * 17.0f * scale,
            center - right * 24.0f * scale - forward * 26.0f * scale,
            center - right * 13.0f * scale - forward * 1.0f * scale,
        };
        DrawGlow(center, Paper, 92.0f * scale, 0.026f, 4);
        DrawColoredPolygon(hull, Alpha(Paper, 0.88f), Array.Empty<Vector2>(), null);
        DrawPolyline(ClosePolygon(hull), Alpha(new Color(0.74f, 0.78f, 0.84f), 0.82f), 1.6f * scale, true);
        DrawLine(center + forward * 28.0f * scale, center - forward * 26.0f * scale, Alpha(Graphite, 0.55f), 1.4f * scale, true);
        DrawCircle(center + forward * 3.0f * scale, 4.2f * scale, Alpha(Cyan, 0.72f));
    }

    private void DrawPilotSelect()
    {
        DrawText(T("menu.pilot"), new Vector2(0.0f, 678.0f), 16, Alpha(Paper, 0.38f), HorizontalAlignment.Center, ScreenWidth, false, 0);
        for (int i = 0; i < PilotCount(); i++)
        {
            DrawPilotCard(i, PilotFromIndex(i), PilotCardRect(i));
        }
    }

    private void DrawPilotCard(int index, PilotKind pilot, Rect2 rect)
    {
        bool unlocked = IsPilotUnlocked(pilot);
        bool selected = _selectedPilot == pilot;
        bool hover = rect.HasPoint(GetGlobalMousePosition());
        Color accent = unlocked ? PilotAccent(pilot) : GridLine;
        Rect2 drawRect = hover ? rect.Grow(4.0f) : rect;
        DrawPanel(drawRect, Alpha(Graphite, selected ? 0.46f : 0.24f), Alpha(accent, selected ? 0.72f : 0.22f));
        DrawPilotGlyph(pilot, drawRect.Position + new Vector2(34.0f, 43.0f), accent, unlocked);
        DrawText(PilotName(pilot).ToUpperInvariant(), drawRect.Position + new Vector2(72.0f, 32.0f), 20, unlocked ? Paper : Alpha(Paper, 0.38f), HorizontalAlignment.Left, drawRect.Size.X - 86.0f, true, 2);
        DrawText(PilotWeapon(pilot), drawRect.Position + new Vector2(72.0f, 55.0f), 15, Alpha(accent, unlocked ? 0.88f : 0.42f), HorizontalAlignment.Left, drawRect.Size.X - 86.0f, false, 0);
        DrawWrapped(unlocked ? PilotBody(pilot) : PilotUnlockText(pilot), drawRect.Position + new Vector2(18.0f, 76.0f), unlocked ? 14 : 13, Alpha(Paper, unlocked ? 0.62f : 0.48f), drawRect.Size.X - 36.0f, unlocked ? 18.0f : 16.0f);
        if (selected)
        {
            DrawLine(drawRect.Position + new Vector2(12.0f, drawRect.Size.Y - 10.0f), drawRect.End - new Vector2(12.0f, 10.0f), Alpha(accent, 0.9f), UiAccentStroke, true);
        }
        if (!unlocked)
        {
            DrawText("LOCK", drawRect.Position + new Vector2(drawRect.Size.X - 66.0f, 28.0f), 13, Alpha(Rose, 0.74f), HorizontalAlignment.Center, 50.0f, true, 2);
            Rect2 bar = new(drawRect.Position + new Vector2(18.0f, drawRect.Size.Y - 14.0f), new Vector2(drawRect.Size.X - 36.0f, 5.0f));
            float progress = PilotUnlockProgress(pilot);
            DrawRect(bar, Alpha(Paper, 0.08f), true);
            DrawRect(new Rect2(bar.Position, new Vector2(bar.Size.X * progress, bar.Size.Y)), Alpha(PilotAccent(pilot), 0.72f), true);
            DrawRect(bar, Alpha(Paper, 0.2f), false, UiHairline, true);
        }
    }

    private void DrawPilotGlyph(PilotKind pilot, Vector2 center, Color accent, bool unlocked)
    {
        Color line = Alpha(accent, unlocked ? 0.88f : 0.34f);
        DrawCircle(center, 22.0f, Alpha(Graphite, 0.9f));
        DrawCircle(center, 22.0f, line, false, UiStroke, true);
        DrawPilotHull(pilot, center, Vector2.Right, accent, unlocked ? 0.82f : 0.32f, 0.42f);
    }

    private void DrawSettings()
    {
        DrawRect(new Rect2(Vector2.Zero, new Vector2(ScreenWidth, ScreenHeight)), Alpha(Void, 0.56f), true);
        bool runSettings = IsRunViewMode(_settingsReturnMode);
        Rect2 panel = new(new Vector2(650.0f, runSettings ? 220.0f : 246.0f), new Vector2(620.0f, runSettings ? 640.0f : 560.0f));
        DrawGlow(panel.Position + panel.Size * 0.5f, CurrentSector().Accent, 380.0f, 0.04f, 7);
        DrawPanel(panel, Alpha(Graphite, 0.9f), Alpha(GridLine, 0.48f));
        DrawSettingsButton(new Rect2(panel.Position + new Vector2(30.0f, 30.0f), new Vector2(42.0f, 42.0f)), true);
        DrawText(T("settings.title"), panel.Position + new Vector2(0.0f, 92.0f), 44, Paper, HorizontalAlignment.Center, panel.Size.X, true, 4);
        DrawText(T("settings.subtitle"), panel.Position + new Vector2(58.0f, 138.0f), 19, Alpha(Paper, 0.64f), HorizontalAlignment.Center, panel.Size.X - 116.0f, true, 2);

        DrawMenuButton(SettingsGuideButtonRect(), T("settings.guide"), XpGreen, true);
        DrawMenuButton(SettingsLanguageButtonRect(), T("menu.language"), PolarityColor(_playerPolarity), false);
        if (runSettings)
        {
            DrawMenuButton(SettingsMainMenuButtonRect(), T("settings.main_menu"), Rose, false);
        }
        string deleteLabel = _deleteSaveConfirmTimer > 0.0f ? T("settings.delete_confirm") : T("settings.delete_save");
        DrawMenuButton(SettingsDeleteSaveButtonRect(), deleteLabel, _deleteSaveConfirmTimer > 0.0f ? Rose : AlertRed.Lerp(Paper, 0.2f), false);
        if (_deleteSaveConfirmTimer > 0.0f)
        {
            DrawText(T("settings.delete_warning"), panel.Position + new Vector2(70.0f, runSettings ? 462.0f : 382.0f), 15, Alpha(Rose, 0.76f), HorizontalAlignment.Center, panel.Size.X - 140.0f, true, 1);
        }
        else if (_deleteSaveNoticeTimer > 0.0f)
        {
            DrawText(T("settings.delete_notice"), panel.Position + new Vector2(70.0f, runSettings ? 462.0f : 382.0f), 16, Alpha(Jade, 0.78f), HorizontalAlignment.Center, panel.Size.X - 140.0f, true, 1);
        }
        string backLabel = runSettings ? T("settings.resume") : T("settings.back");
        DrawMenuButton(SettingsBackButtonRect(), backLabel, GridLine, false);
        DrawText(T("language.hint"), panel.Position + new Vector2(0.0f, runSettings ? 578.0f : 492.0f), 17, Alpha(Paper, 0.42f), HorizontalAlignment.Center, panel.Size.X, true, 2);
    }

    private void DrawGuide()
    {
        DrawRect(new Rect2(Vector2.Zero, new Vector2(ScreenWidth, ScreenHeight)), Alpha(Void, 0.64f), true);
        Rect2 panel = new(new Vector2(420.0f, 150.0f), new Vector2(1080.0f, 760.0f));
        DrawGlow(panel.Position + panel.Size * 0.5f, XpGreen, 460.0f, 0.035f, 8);
        DrawPanel(panel, Alpha(Graphite, 0.92f), Alpha(GridLine, 0.5f));
        DrawText(T("guide.title"), panel.Position + new Vector2(0.0f, 82.0f), 48, Paper, HorizontalAlignment.Center, panel.Size.X, true, 5);
        DrawText(T("guide.subtitle"), panel.Position + new Vector2(100.0f, 128.0f), 20, Alpha(Paper, 0.6f), HorizontalAlignment.Center, panel.Size.X - 200.0f, true, 2);

        string[] lines = GuideLines();
        for (int i = 0; i < lines.Length; i++)
        {
            float y = 196.0f + i * 74.0f;
            Rect2 row = new(panel.Position + new Vector2(84.0f, y), new Vector2(912.0f, 54.0f));
            Color accent = i switch
            {
                0 => Paper,
                1 => PolarityColor(_playerPolarity),
                2 => PolarityColor(1 - _playerPolarity),
                3 => XpGreen,
                4 => PickupBlue,
                _ => AlertRed,
            };
            DrawPanel(row, Alpha(Ink, 0.34f), Alpha(accent, 0.3f));
            DrawCircle(row.Position + new Vector2(28.0f, 27.0f), 9.0f, Alpha(accent, 0.88f));
            DrawWrapped(lines[i], row.Position + new Vector2(54.0f, 17.0f), 19, Alpha(Paper, 0.78f), row.Size.X - 78.0f, 25.0f);
        }

        DrawMenuButton(GuideBackButtonRect(), T("settings.back"), GridLine, false);
    }

    private void DrawMeta()
    {
        DrawRect(new Rect2(Vector2.Zero, new Vector2(ScreenWidth, ScreenHeight)), new Color(0.0f, 0.0f, 0.0f, 0.46f), true);
        DrawGlow(ScreenCenter + new Vector2(0.0f, -40.0f), Gold, 520.0f, 0.055f, 8);
        DrawGlow(ScreenCenter + new Vector2(-380.0f, 160.0f), Cyan, 340.0f, 0.035f, 7);
        DrawText(T("meta.title"), new Vector2(0.0f, 116.0f), 60, Gold, HorizontalAlignment.Center, ScreenWidth, true, 5);
        DrawText(T("meta.subtitle"), new Vector2(0.0f, 170.0f), 22, Alpha(Paper, 0.72f), HorizontalAlignment.Center, ScreenWidth, true, 2);

        DrawPanel(new Rect2(new Vector2(640.0f, 210.0f), new Vector2(640.0f, 50.0f)), Alpha(Ink, 0.72f), Alpha(Gold, 0.5f));
        DrawText(Tf("meta.wallet", _starDust), new Vector2(640.0f, 243.0f), 23, Gold, HorizontalAlignment.Center, 640.0f, true, 2);
        DrawText(Tf("meta.best", _bestWave, _bestScore, _runsCompleted), new Vector2(0.0f, 296.0f), 18, Alpha(Paper, 0.62f), HorizontalAlignment.Center, ScreenWidth, true, 2);

        Vector2 mouse = GetGlobalMousePosition();
        for (int i = 0; i < MetaUpgrades.Length; i++)
        {
            MetaUpgradeDef def = MetaUpgrades[i];
            int rank = MetaRank(def.Id);
            bool maxed = rank >= def.MaxRank;
            int cost = maxed ? 0 : MetaUpgradeCost(def, rank);
            Rect2 rect = MetaUpgradeRect(i);
            bool hover = rect.HasPoint(mouse);
            Rect2 drawRect = hover ? rect.Grow(5.0f) : rect;
            Color accent = def.Accent;
            DrawGlow(drawRect.Position + drawRect.Size * 0.5f, accent, hover ? 128.0f : 82.0f, hover ? 0.065f : 0.034f, 4);
            DrawPanel(drawRect, Alpha(Ink, hover ? 0.88f : 0.76f), Alpha(accent, hover ? 0.86f : 0.42f));
            DrawCircle(drawRect.Position + new Vector2(drawRect.Size.X - 35.0f, 34.0f), 18.0f, Alpha(accent, 0.72f));
            DrawText($"{i + 1}", drawRect.Position + new Vector2(drawRect.Size.X - 50.0f, 41.0f), 18, Ink, HorizontalAlignment.Center, 30.0f, false, 0);
            DrawText(T(def.TitleKey).ToUpperInvariant(), drawRect.Position + new Vector2(20.0f, 39.0f), 21, Paper, HorizontalAlignment.Left, drawRect.Size.X - 78.0f, true, 2);
            DrawText(Tf("meta.rank", rank, def.MaxRank), drawRect.Position + new Vector2(20.0f, 68.0f), 15, Alpha(accent, 0.95f), HorizontalAlignment.Left, 170.0f, true, 1);
            DrawWrapped(T(def.BodyKey), drawRect.Position + new Vector2(20.0f, 86.0f), 15, Alpha(Paper, 0.72f), drawRect.Size.X - 40.0f, 18.0f);

            Rect2 bar = new(drawRect.Position + new Vector2(20.0f, drawRect.Size.Y - 28.0f), new Vector2(drawRect.Size.X - 40.0f, 7.0f));
            float progress = def.MaxRank <= 0 ? 1.0f : (float)rank / def.MaxRank;
            DrawRect(bar.Grow(2.0f), Alpha(Paper, 0.08f), true);
            DrawRect(bar, Alpha(accent, 0.14f), true);
            DrawRect(new Rect2(bar.Position, new Vector2(bar.Size.X * progress, bar.Size.Y)), Alpha(accent, 0.78f), true);
            DrawRect(bar, Alpha(Paper, 0.28f), false, UiHairline, true);
            DrawText(maxed ? T("meta.max") : Tf("meta.cost", cost), drawRect.Position + new Vector2(20.0f, drawRect.Size.Y - 7.0f), 15, maxed ? Jade : (_starDust >= cost ? Gold : Rose), HorizontalAlignment.Left, drawRect.Size.X - 40.0f, true, 1);
        }

        DrawText(T("meta.buy_hint"), new Vector2(0.0f, 884.0f), 20, Alpha(Paper, 0.62f), HorizontalAlignment.Center, ScreenWidth, true, 2);
        DrawMenuButton(MetaBackButtonRect(), T("meta.back"), Violet, false);
    }

    private void DrawUpgrade()
    {
        DrawRect(new Rect2(Vector2.Zero, new Vector2(ScreenWidth, ScreenHeight)), new Color(0.0f, 0.0f, 0.0f, 0.56f), true);
        DrawText(T("upgrade.title"), new Vector2(0.0f, 218.0f), 42, Paper, HorizontalAlignment.Center, ScreenWidth, true, 5);
        DrawText($"{T("upgrade.hint")}    REROLL {_rerollsRemaining}    {T("language.hint")}", new Vector2(0.0f, 266.0f), 19, Alpha(Paper, 0.62f), HorizontalAlignment.Center, ScreenWidth, true, 2);
        DrawText(NextWavePreviewText(), new Vector2(0.0f, 304.0f), 18, Alpha(CurrentSector().Accent, 0.82f), HorizontalAlignment.Center, ScreenWidth, true, 2);

        Vector2 mouse = GetGlobalMousePosition();
        for (int i = 0; i < _upgradeChoices.Count; i++)
        {
            UpgradeCard card = _upgradeChoices[i];
            bool hover = card.Rect.HasPoint(mouse);
            Rect2 rect = hover ? card.Rect.Grow(7.0f) : card.Rect;
            Color border = Alpha(card.Accent, hover ? 0.86f : 0.46f);
            DrawGlow(rect.Position + rect.Size * 0.5f, card.Accent, hover ? 178.0f : 116.0f, hover ? 0.075f : 0.034f, 5);
            DrawPanel(rect, Alpha(Ink, hover ? 0.92f : 0.78f), border);

            Rect2 header = new(rect.Position, new Vector2(rect.Size.X, 104.0f));
            DrawRect(header, Alpha(card.Accent, hover ? 0.13f : 0.085f), true);
            DrawLine(header.Position + new Vector2(22.0f, header.Size.Y), header.Position + new Vector2(header.Size.X - 22.0f, header.Size.Y), Alpha(card.Accent, hover ? 0.42f : 0.24f), UiHairline, true);

            for (int line = 0; line < 5; line++)
            {
                float y = rect.Position.Y + 132.0f + line * 45.0f;
                DrawLine(new Vector2(rect.Position.X + 26.0f, y), new Vector2(rect.End.X - 26.0f, y + Mathf.Sin(_time + line) * 4.0f), Alpha(card.Accent, hover ? 0.065f : 0.035f), UiHairline, true);
            }

            Vector2 iconCenter = rect.Position + new Vector2(rect.Size.X - 62.0f, 62.0f);
            DrawCircle(iconCenter, 34.0f, Alpha(Graphite, 0.76f));
            DrawCircle(iconCenter, 34.0f, Alpha(card.Accent, hover ? 0.58f : 0.36f), false, UiStroke, true);
            DrawCircle(iconCenter, 24.0f, Alpha(card.Accent, 0.12f));
            DrawUpgradeGlyph(card.Id, iconCenter, 23.0f, card.Accent);

            DrawText(card.Tag.ToUpperInvariant(), rect.Position + new Vector2(26.0f, 38.0f), 15, Alpha(card.Accent, 0.96f), HorizontalAlignment.Left, rect.Size.X - 118.0f, true, 1);
            DrawText(card.Title.ToUpperInvariant(), rect.Position + new Vector2(26.0f, 82.0f), 27, Paper, HorizontalAlignment.Left, rect.Size.X - 122.0f, true, 3);
            DrawHighlightedWrapped(card.Body, rect.Position + new Vector2(26.0f, 152.0f), 20, Alpha(Paper, 0.68f), card.Accent, rect.Size.X - 52.0f, 31.0f);

            int rank = GetRank(card.Id);
            int maxRank = MaxRank(card.Id);
            int nextRank = Math.Min(rank + 1, maxRank);
            string rankText = _language == GameLanguage.Chinese ? $"等级 {rank}  >  {nextRank}" : $"RANK {rank}  >  {nextRank}";
            string slotText = _language == GameLanguage.Chinese ? $"按 {i + 1} 选择" : $"PRESS {i + 1}";
            Rect2 footer = new(rect.Position + new Vector2(24.0f, rect.Size.Y - 58.0f), new Vector2(rect.Size.X - 48.0f, 36.0f));
            DrawRect(footer, Alpha(Graphite, 0.34f), true);
            DrawRect(footer, Alpha(card.Accent, hover ? 0.36f : 0.2f), false, UiHairline, true);
            DrawText(rankText, footer.Position + new Vector2(12.0f, 23.0f), 14, Alpha(card.Accent, 0.94f), HorizontalAlignment.Left, footer.Size.X - 24.0f, false, 0);
            DrawText(slotText, footer.Position + new Vector2(0.0f, 23.0f), 14, Alpha(Paper, hover ? 0.78f : 0.52f), HorizontalAlignment.Right, footer.Size.X - 12.0f, false, 0);
        }
    }

    private void DrawEndScreen(bool victory)
    {
        DrawRect(new Rect2(Vector2.Zero, new Vector2(ScreenWidth, ScreenHeight)), new Color(0.0f, 0.0f, 0.0f, victory ? 0.34f : 0.58f), true);
        Color color = victory ? Gold : Rose;
        DrawGlow(ScreenCenter, color, victory ? 620.0f : 460.0f, 0.07f, 9);
        DrawText(victory ? T("end.victory.title") : T("end.defeat.title"), new Vector2(0.0f, 430.0f), 82, color, HorizontalAlignment.Center, ScreenWidth, true, 7);
        DrawText(Tf("end.score", _score), new Vector2(0.0f, 506.0f), 32, Paper, HorizontalAlignment.Center, ScreenWidth, true, 4);
        DrawText(victory ? T("end.victory.body") : T("end.defeat.body"), new Vector2(0.0f, 568.0f), 24, Alpha(Paper, 0.72f), HorizontalAlignment.Center, ScreenWidth, true, 3);
        DrawText(Tf("end.reward", _lastDustEarned, _lastRunWave), new Vector2(0.0f, 620.0f), 25, Gold, HorizontalAlignment.Center, ScreenWidth, true, 3);
        if (_lastObjectiveBonusDust > 0)
        {
            DrawText(Tf("end.objective_bonus", _lastObjectiveBonusDust), new Vector2(0.0f, 656.0f), 20, Alpha(Jade, 0.82f), HorizontalAlignment.Center, ScreenWidth, true, 2);
        }
        DrawPanel(new Rect2(new Vector2(760.0f, 700.0f), new Vector2(400.0f, 58.0f)), Alpha(Ink, 0.72f), Alpha(color, 0.48f));
        DrawText(T("end.restart"), new Vector2(760.0f, 738.0f), 23, Paper, HorizontalAlignment.Center, 400.0f, true, 3);
        DrawText(T("end.meta_hint"), new Vector2(0.0f, 800.0f), 20, Alpha(Paper, 0.64f), HorizontalAlignment.Center, ScreenWidth, true, 2);
        DrawText(T("language.hint"), new Vector2(0.0f, 830.0f), 20, Alpha(Paper, 0.58f), HorizontalAlignment.Center, ScreenWidth, true, 2);
    }

    private void DrawHudMetric(Rect2 rect, string label, string value, Color accent)
    {
        DrawPanel(rect, Alpha(Ink, 0.34f), Alpha(GridLine, 0.22f));
        DrawText(label.ToUpperInvariant(), rect.Position + new Vector2(10.0f, 16.0f), 10, Alpha(Paper, 0.44f), HorizontalAlignment.Left, rect.Size.X - 20.0f, false, 0);
        DrawText(value, rect.Position + new Vector2(10.0f, 35.0f), 18, accent, HorizontalAlignment.Left, rect.Size.X - 20.0f, true, 2);
    }

    private void DrawSettingsButton(Rect2 rect, bool withLabel)
    {
        bool hover = rect.HasPoint(GetGlobalMousePosition());
        Color accent = hover ? XpGreen : GridLine;
        DrawPanel(rect, Alpha(Ink, hover ? 0.7f : 0.42f), Alpha(accent, hover ? 0.72f : 0.34f));
        Vector2 center = rect.Position + rect.Size * 0.5f;
        DrawLine(center + new Vector2(-11.0f, -7.0f), center + new Vector2(11.0f, -7.0f), Alpha(Paper, hover ? 0.9f : 0.68f), UiStroke, true);
        DrawLine(center + new Vector2(-11.0f, 0.0f), center + new Vector2(11.0f, 0.0f), Alpha(Paper, hover ? 0.9f : 0.68f), UiStroke, true);
        DrawLine(center + new Vector2(-11.0f, 7.0f), center + new Vector2(11.0f, 7.0f), Alpha(Paper, hover ? 0.9f : 0.68f), UiStroke, true);

        if (withLabel)
        {
            DrawText(T("hud.settings"), rect.Position + new Vector2(rect.Size.X + 12.0f, 28.0f), 18, Alpha(Paper, 0.72f), HorizontalAlignment.Left, 120.0f, true, 2);
        }
    }

    private void DrawBar(Rect2 rect, float value, float trailValue, Color color, string label, string valueText)
    {
        value = Mathf.Clamp(value, 0.0f, 1.0f);
        trailValue = Mathf.Clamp(trailValue, 0.0f, 1.0f);
        DrawText(label.ToUpperInvariant(), rect.Position + new Vector2(0.0f, -8.0f), 13, Alpha(Paper, 0.5f), HorizontalAlignment.Left, 120.0f, false, 0);
        DrawText(valueText, rect.Position + new Vector2(0.0f, -8.0f), 13, Alpha(Paper, 0.74f), HorizontalAlignment.Right, rect.Size.X, false, 0);
        DrawRect(rect.Grow(3.0f), Alpha(Paper, 0.08f), true);
        DrawRect(rect, Alpha(color, 0.12f), true);
        DrawRect(new Rect2(rect.Position, new Vector2(rect.Size.X * trailValue, rect.Size.Y)), Alpha(Paper, 0.12f), true);
        DrawRect(new Rect2(rect.Position, new Vector2(rect.Size.X * trailValue, rect.Size.Y)), Alpha(color, 0.24f), true);
        DrawRect(new Rect2(rect.Position, new Vector2(rect.Size.X * value, rect.Size.Y)), Alpha(color, 0.92f), true);
        float lead = rect.Position.X + rect.Size.X * value;
        if (value > 0.015f && value < 0.985f)
        {
            DrawLine(new Vector2(lead, rect.Position.Y - 3.0f), new Vector2(lead, rect.End.Y + 3.0f), Alpha(Paper, 0.38f + 0.18f * Mathf.Sin(_time * 12.0f)), UiStroke, true);
        }
        DrawRect(rect, Alpha(Paper, 0.32f), false, UiHairline, true);
    }

    private void DrawPanel(Rect2 rect, Color fill, Color stroke)
    {
        DrawRect(rect, fill, true);
        DrawRect(rect, stroke, false, UiStroke, true);
        DrawLine(rect.Position, rect.Position + new Vector2(rect.Size.X, 0.0f), Alpha(Paper, 0.08f), UiHairline, true);
    }

    private void DrawMenuButton(Rect2 rect, string label, Color accent, bool primary)
    {
        bool hover = rect.HasPoint(GetGlobalMousePosition());
        Rect2 drawRect = hover ? rect.Grow(primary ? 8.0f : 5.0f) : rect;
        DrawGlow(drawRect.Position + drawRect.Size * 0.5f, accent, primary ? 150.0f : 90.0f, hover ? 0.08f : 0.045f, 4);
        DrawPanel(drawRect, Alpha(Ink, primary ? 0.78f : 0.62f), Alpha(accent, hover ? 0.82f : 0.42f));
        DrawText(label, drawRect.Position + new Vector2(0.0f, primary ? 43.0f : 34.0f), primary ? 28 : 21, Paper, HorizontalAlignment.Center, drawRect.Size.X, true, 3);
    }

    private void DrawTitleTextButton(Rect2 rect, string label, Color accent)
    {
        bool hover = rect.HasPoint(GetGlobalMousePosition());
        Color textColor = hover ? accent.Lerp(Paper, 0.28f) : Alpha(accent, 0.68f);
        DrawText(label.ToUpperInvariant(), rect.Position + new Vector2(0.0f, 28.0f), 18, textColor, HorizontalAlignment.Center, rect.Size.X, false, 0);
        DrawLine(rect.Position + new Vector2(rect.Size.X * 0.22f, rect.Size.Y - 5.0f), rect.Position + new Vector2(rect.Size.X * 0.78f, rect.Size.Y - 5.0f), Alpha(accent, hover ? 0.58f : 0.24f), UiHairline, true);
    }

    private void UpdateHudBarEasing(float dt)
    {
        float hullTarget = SafeRatio(_playerHp, _playerMaxHp);
        float energyTarget = SafeRatio(_energy, _maxEnergy);
        float dashTarget = Mathf.Clamp(1.0f - Mathf.Max(_dashCooldown, 0.0f) / 0.86f, 0.0f, 1.0f);

        _hudHullValue = EaseHudValue(_hudHullValue, hullTarget, hullTarget > _hudHullValue ? 10.0f : 8.0f, dt);
        _hudEnergyValue = EaseHudValue(_hudEnergyValue, energyTarget, energyTarget > _hudEnergyValue ? 7.5f : 10.5f, dt);
        _hudDashValue = EaseHudValue(_hudDashValue, dashTarget, dashTarget > _hudDashValue ? 12.0f : 18.0f, dt);

        _hudHullTrail = EaseHudValue(_hudHullTrail, hullTarget, hullTarget < _hudHullTrail ? 2.4f : 13.0f, dt);
        _hudEnergyTrail = EaseHudValue(_hudEnergyTrail, energyTarget, energyTarget < _hudEnergyTrail ? 3.0f : 9.0f, dt);
        _hudDashTrail = EaseHudValue(_hudDashTrail, dashTarget, dashTarget < _hudDashTrail ? 5.0f : 11.0f, dt);
    }

    private void SnapHudBars()
    {
        _hudHullValue = SafeRatio(_playerHp, _playerMaxHp);
        _hudHullTrail = _hudHullValue;
        _hudEnergyValue = SafeRatio(_energy, _maxEnergy);
        _hudEnergyTrail = _hudEnergyValue;
        _hudDashValue = Mathf.Clamp(1.0f - Mathf.Max(_dashCooldown, 0.0f) / 0.86f, 0.0f, 1.0f);
        _hudDashTrail = _hudDashValue;
    }

    private static float EaseHudValue(float current, float target, float speed, float dt)
    {
        return Mathf.Lerp(current, target, 1.0f - Mathf.Exp(-speed * dt));
    }

    private static float SafeRatio(float value, float maxValue)
    {
        if (maxValue <= 0.001f)
        {
            return 0.0f;
        }

        return Mathf.Clamp(value / maxValue, 0.0f, 1.0f);
    }

    private static float Progress01(int value, int target)
    {
        if (target <= 0)
        {
            return 1.0f;
        }

        return Mathf.Clamp(value / (float)target, 0.0f, 1.0f);
    }

    private float CruiseCharge01()
    {
        return Mathf.Clamp(_cruiseCharge / CruiseChargeMax, 0.0f, 1.0f);
    }

    private void AddCruiseCharge(float amount, Vector2 source)
    {
        if (amount <= 0.0f)
        {
            return;
        }

        _cruiseCharge = Mathf.Clamp(_cruiseCharge + amount * _absorbEfficiency, 0.0f, CruiseChargeMax);
        if (_visualPressure < 0.76f)
        {
            Vector2 pull = (_playerPos - source).LengthSquared() > 0.01f ? (_playerPos - source).Normalized() : Vector2.Up;
            AddParticle(source, pull * 120.0f, PolarityBlue, 5.0f, 0.18f);
        }
    }

    private float PlayerShotDamageScale(int stance)
    {
        if (stance == 0)
        {
            return 0.88f;
        }

        return _assaultBurstTimer > 0.0f ? _assaultPower : 1.12f;
    }

    private void EnterOverheat(Enemy enemy)
    {
        float duration = EnemyOverheatDuration(enemy);
        enemy.Overheat = Mathf.Max(enemy.Overheat, duration);
        enemy.OverheatMax = Mathf.Max(0.01f, duration);
    }

    private float EnemyOverheatDuration(Enemy enemy)
    {
        float duration = EnemyOverheatBase;
        duration += enemy.Kind switch
        {
            EnemyKind.Boss => 0.75f,
            EnemyKind.Lance or EnemyKind.Harrier or EnemyKind.Shard => 0.28f,
            EnemyKind.Bulwark or EnemyKind.Warden => 0.5f,
            _ => 0.0f,
        };
        if (enemy.Elite)
        {
            duration += 0.22f;
        }
        return duration;
    }

    private float EnemyOverheat01(Enemy enemy)
    {
        return Mathf.Clamp(enemy.Overheat / Mathf.Max(0.01f, enemy.OverheatMax), 0.0f, 1.0f);
    }

    private float EnemyTelegraph01(Enemy enemy)
    {
        if (enemy.Overheat > 0.0f)
        {
            return 0.0f;
        }

        float lead = EnemyTelegraphLead + (enemy.Kind == EnemyKind.Boss ? 0.28f : enemy.Elite ? 0.12f : 0.0f);
        return 1.0f - Mathf.Clamp(enemy.Cooldown / lead, 0.0f, 1.0f);
    }

    private bool EnemyIsCharging(Enemy enemy)
    {
        return enemy.Cooldown > 0.0f && enemy.Overheat <= 0.0f && EnemyTelegraph01(enemy) > 0.0f;
    }

    private float EnemyCooldownRate(Enemy enemy)
    {
        float rate = Mathf.Lerp(0.58f, 0.76f, RunProgress01());
        if (enemy.Kind == EnemyKind.Boss)
        {
            rate *= 0.9f;
        }
        else if (enemy.Kind == EnemyKind.Shard || enemy.Kind == EnemyKind.Harrier)
        {
            rate *= 1.08f;
        }
        if (enemy.Elite)
        {
            rate *= 1.08f;
        }
        return rate;
    }

    private static Color EnemyBulletColor()
    {
        return EnemyFireRed;
    }

    private Color ShotVisualColor(Shot shot)
    {
        return shot.FromPlayer ? PolarityColor(shot.Polarity) : EnemyBulletColor();
    }

    private Color EnemyStateColor(Enemy enemy)
    {
        float charge = EnemyTelegraph01(enemy);
        if (charge > 0.0f)
        {
            return PolarityColor(enemy.Polarity).Lerp(EnemyBulletColor(), Mathf.Clamp(0.44f + charge * 0.48f, 0.0f, 1.0f));
        }

        float overheat = EnemyOverheat01(enemy);
        if (overheat > 0.0f)
        {
            return PolarityColor(enemy.Polarity).Lerp(Paper, 0.42f + overheat * 0.32f).Lerp(Gold, 0.18f);
        }

        return PolarityColor(enemy.Polarity).Lerp(CurrentSector().Accent, 0.16f).Lerp(Steel, 0.18f);
    }

    private static float PolarityCooldownFor(int tunerRank, int stormRank)
    {
        float cooldown = PolaritySwitchCooldownBase - tunerRank * 0.1f - stormRank * 0.12f;
        return Mathf.Max(PolaritySwitchCooldownMin, cooldown);
    }

    private float PolaritySwitchReady01()
    {
        return 1.0f - Mathf.Clamp(_polarityCooldown / Mathf.Max(0.01f, _polarityCooldownMax), 0.0f, 1.0f);
    }

    private float CalculateVisualPressure()
    {
        float shotPressure = _shots.Count / 360.0f;
        float enemyPressure = _enemies.Count / 58.0f;
        float particlePressure = _particles.Count / (float)MaxParticles;
        float textPressure = _damageTexts.Count / (float)MaxDamageTexts;
        return Mathf.Clamp(Mathf.Max(shotPressure, Mathf.Max(enemyPressure, particlePressure * 0.9f)) + textPressure * 0.08f, 0.0f, 1.0f);
    }

    private static float DistancePointToSegment(Vector2 point, Vector2 start, Vector2 end)
    {
        Vector2 segment = end - start;
        float lengthSquared = segment.LengthSquared();
        if (lengthSquared <= 0.001f)
        {
            return point.DistanceTo(start);
        }

        float t = Mathf.Clamp((point - start).Dot(segment) / lengthSquared, 0.0f, 1.0f);
        return point.DistanceTo(start + segment * t);
    }

    private void DrawText(string text, Vector2 pos, int size, Color color, HorizontalAlignment alignment, float width, bool outline, int outlineSize)
    {
        Font font = _uiFont ?? ThemeDB.FallbackFont;
        if (outline && outlineSize > 0)
        {
            DrawStringOutline(font, pos, text, alignment, width, size, outlineSize, Alpha(Void, 0.78f));
        }
        DrawString(font, pos, text, alignment, width, size, color);
    }

    private static bool IsRunViewMode(GameMode mode)
    {
        return mode == GameMode.Playing || mode == GameMode.Upgrade || mode == GameMode.GameOver || mode == GameMode.Victory;
    }

    private void DrawWrapped(string text, Vector2 pos, int size, Color color, float width, float lineHeight)
    {
        float maxUnits = Mathf.Max(8.0f, width / Mathf.Max(1.0f, size * 0.52f));
        int lineIndex = 0;
        string line = string.Empty;
        float lineUnits = 0.0f;

        foreach (char c in text)
        {
            if (c == '\n')
            {
                DrawText(line, pos + new Vector2(0.0f, lineIndex * lineHeight), size, color, HorizontalAlignment.Left, width, true, 2);
                line = string.Empty;
                lineUnits = 0.0f;
                lineIndex++;
                continue;
            }

            float units = EstimateGlyphUnits(c);
            bool isSpace = c == ' ';
            if (line.Length > 0 && lineUnits + units > maxUnits)
            {
                DrawText(line.TrimEnd(), pos + new Vector2(0.0f, lineIndex * lineHeight), size, color, HorizontalAlignment.Left, width, true, 2);
                line = isSpace ? string.Empty : c.ToString();
                lineUnits = isSpace ? 0.0f : units;
                lineIndex++;
            }
            else
            {
                line += c;
                lineUnits += units;
            }
        }

        if (line.Length > 0)
        {
            DrawText(line.TrimEnd(), pos + new Vector2(0.0f, lineIndex * lineHeight), size, color, HorizontalAlignment.Left, width, true, 2);
        }
    }

    private void DrawHighlightedWrapped(string text, Vector2 pos, int size, Color color, Color accent, float width, float lineHeight)
    {
        float maxUnits = Mathf.Max(8.0f, width / Mathf.Max(1.0f, size * 0.52f));
        int lineIndex = 0;
        float lineUnits = 0.0f;
        List<RichTextSegment> line = new();

        foreach (RichTextSegment segment in SplitHighlightedSegments(text))
        {
            foreach (char c in segment.Text)
            {
                if (c == '\n')
                {
                    DrawRichLine(line, pos + new Vector2(0.0f, lineIndex * lineHeight), size, color, accent, width);
                    line.Clear();
                    lineUnits = 0.0f;
                    lineIndex++;
                    continue;
                }

                float units = EstimateGlyphUnits(c);
                bool isSpace = c == ' ';
                if (line.Count > 0 && lineUnits + units > maxUnits)
                {
                    DrawRichLine(line, pos + new Vector2(0.0f, lineIndex * lineHeight), size, color, accent, width);
                    line.Clear();
                    lineUnits = 0.0f;
                    lineIndex++;
                    if (isSpace)
                    {
                        continue;
                    }
                }

                if (!isSpace || line.Count > 0)
                {
                    AppendRichSegment(line, c.ToString(), segment.Highlight);
                    lineUnits += units;
                }
            }
        }

        if (line.Count > 0)
        {
            DrawRichLine(line, pos + new Vector2(0.0f, lineIndex * lineHeight), size, color, accent, width);
        }
    }

    private void DrawRichLine(List<RichTextSegment> line, Vector2 pos, int size, Color color, Color accent, float width)
    {
        List<RichTextSegment> cleanLine = TrimRichLineEnd(line);
        float cursor = pos.X;
        float right = pos.X + width;
        foreach (RichTextSegment segment in cleanLine)
        {
            float segmentWidth = EstimateTextPixelWidth(segment.Text, size);
            Color textColor = segment.Highlight ? accent.Lerp(Paper, 0.18f) : color;
            if (segment.Highlight)
            {
                DrawRect(new Rect2(new Vector2(cursor, pos.Y + 3.0f), new Vector2(segmentWidth, 2.0f)), Alpha(accent, 0.18f), true);
            }

            DrawText(segment.Text, new Vector2(cursor, pos.Y), size, textColor, HorizontalAlignment.Left, Mathf.Max(segmentWidth + 24.0f, right - cursor), true, 1);
            cursor += segmentWidth;
        }
    }

    private static List<RichTextSegment> TrimRichLineEnd(List<RichTextSegment> line)
    {
        List<RichTextSegment> cleanLine = new(line);
        while (cleanLine.Count > 0)
        {
            int lastIndex = cleanLine.Count - 1;
            RichTextSegment last = cleanLine[lastIndex];
            string trimmed = last.Text.TrimEnd();
            if (trimmed.Length == last.Text.Length)
            {
                break;
            }

            cleanLine.RemoveAt(lastIndex);
            if (trimmed.Length > 0)
            {
                cleanLine.Add(new RichTextSegment(trimmed, last.Highlight));
                break;
            }
        }

        return cleanLine;
    }

    private static List<RichTextSegment> SplitHighlightedSegments(string text)
    {
        List<RichTextSegment> segments = new();
        int index = 0;
        int normalStart = 0;
        while (index < text.Length)
        {
            string? match = MatchUpgradeHighlight(text, index);
            if (match is not null)
            {
                if (index > normalStart)
                {
                    AppendRichSegment(segments, text.Substring(normalStart, index - normalStart), false);
                }

                AppendRichSegment(segments, text.Substring(index, match.Length), true);
                index += match.Length;
                normalStart = index;
                continue;
            }

            index++;
        }

        if (normalStart < text.Length)
        {
            AppendRichSegment(segments, text.Substring(normalStart), false);
        }

        return segments;
    }

    private static string? MatchUpgradeHighlight(string text, int index)
    {
        if (char.IsDigit(text[index]) || text[index] == '+' || text[index] == '-')
        {
            int end = index + 1;
            while (end < text.Length && (char.IsDigit(text[end]) || text[end] == '.' || text[end] == '%' || text[end] == '/' || text[end] == '+'))
            {
                end++;
            }

            return text.Substring(index, end - index);
        }

        string? best = null;
        foreach (string term in UpgradeHighlightTerms)
        {
            if (term.Length == 0 || index + term.Length > text.Length)
            {
                continue;
            }

            if (string.Compare(text, index, term, 0, term.Length, StringComparison.OrdinalIgnoreCase) == 0 &&
                (best is null || term.Length > best.Length))
            {
                best = term;
            }
        }

        return best;
    }

    private static void AppendRichSegment(List<RichTextSegment> segments, string text, bool highlight)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        if (segments.Count > 0 && segments[segments.Count - 1].Highlight == highlight)
        {
            RichTextSegment last = segments[segments.Count - 1];
            segments[segments.Count - 1] = new RichTextSegment(last.Text + text, highlight);
            return;
        }

        segments.Add(new RichTextSegment(text, highlight));
    }

    private static float EstimateTextPixelWidth(string text, int size)
    {
        float units = 0.0f;
        foreach (char c in text)
        {
            units += EstimateGlyphUnits(c);
        }

        return units * size * 0.52f;
    }

    private static float EstimateGlyphUnits(char c)
    {
        if (c == ' ')
        {
            return 0.55f;
        }
        return c <= 127 ? 1.0f : 1.85f;
    }

    private void DrawVignette()
    {
        DrawRect(new Rect2(Vector2.Zero, new Vector2(ScreenWidth, 120.0f)), Alpha(Void, 0.18f), true);
        DrawRect(new Rect2(new Vector2(0.0f, ScreenHeight - 150.0f), new Vector2(ScreenWidth, 150.0f)), Alpha(Void, 0.22f), true);
        DrawRect(new Rect2(Vector2.Zero, new Vector2(120.0f, ScreenHeight)), Alpha(Void, 0.16f), true);
        DrawRect(new Rect2(new Vector2(ScreenWidth - 120.0f, 0.0f), new Vector2(120.0f, ScreenHeight)), Alpha(Void, 0.16f), true);
        DrawCircle(Vector2.Zero, 580.0f, Alpha(Void, 0.12f));
        DrawCircle(new Vector2(ScreenWidth, 0.0f), 580.0f, Alpha(Void, 0.12f));
        DrawCircle(new Vector2(0.0f, ScreenHeight), 620.0f, Alpha(Void, 0.14f));
        DrawCircle(new Vector2(ScreenWidth, ScreenHeight), 620.0f, Alpha(Void, 0.14f));
    }

    private void DrawDiamond(Vector2 center, float radius, Color color, float rotation)
    {
        Vector2[] points =
        {
            center + new Vector2(0.0f, -radius * 1.25f).Rotated(rotation),
            center + new Vector2(radius, 0.0f).Rotated(rotation),
            center + new Vector2(0.0f, radius * 1.25f).Rotated(rotation),
            center + new Vector2(-radius, 0.0f).Rotated(rotation),
        };
        DrawColoredPolygon(points, Alpha(color, 0.16f), Array.Empty<Vector2>(), null);
        DrawPolyline(ClosePolygon(points), Alpha(color, 0.9f), UiStroke, true);
        DrawCircle(center, radius * 0.2f, Alpha(Paper, 0.72f));
    }

    private void DrawPlus(Vector2 center, float radius, Color color)
    {
        DrawCircle(center, radius * 1.05f, Alpha(color, 0.14f));
        DrawCircle(center, radius * 1.05f, Alpha(color, 0.78f), false, UiStroke, true);
        DrawLine(center - Vector2.Right * radius * 0.72f, center + Vector2.Right * radius * 0.72f, Alpha(Paper, 0.78f), UiStroke, true);
        DrawLine(center - Vector2.Down * radius * 0.72f, center + Vector2.Down * radius * 0.72f, Alpha(Paper, 0.78f), UiStroke, true);
    }

    private void DrawGlow(Vector2 pos, Color color, float radius, float alpha, int layers)
    {
        for (int i = layers; i >= 1; i--)
        {
            float t = i / (float)layers;
            DrawCircle(pos, radius * t, Alpha(color, alpha * (1.0f - t * 0.72f)));
        }
    }

    private void AddParticle(Vector2 pos, Vector2 vel, Color color, float size, float life)
    {
        if (_visualPressure > 0.86f && _particles.Count > MaxParticles * 0.62f && _rng.Randf() < 0.5f)
        {
            return;
        }

        Particle particle = AddParticleObject();
        particle.Pos = pos;
        particle.Vel = vel;
        particle.Color = color;
        particle.Size = size;
        particle.Life = life;
        particle.MaxLife = life;
        particle.Spin = _rng.RandfRange(-1.0f, 1.0f);
    }

    private void Burst(Vector2 pos, Color color, int count, float speed, float life)
    {
        if (_visualPressure > 0.9f)
        {
            count = Mathf.Max(1, count / 4);
        }
        else if (_visualPressure > 0.72f)
        {
            count = Mathf.Max(2, count / 3);
        }
        else if (_visualPressure > 0.55f)
        {
            count = Mathf.Max(3, count / 2);
        }

        if (_particles.Count > MaxParticles * 0.78f)
        {
            count = Mathf.Max(1, count / 3);
        }
        else if (_particles.Count > MaxParticles * 0.58f)
        {
            count = Mathf.Max(2, count / 2);
        }

        for (int i = 0; i < count; i++)
        {
            Vector2 dir = RandomDirection();
            AddParticle(pos + dir * _rng.RandfRange(0.0f, 22.0f), dir * _rng.RandfRange(speed * 0.18f, speed), color.Lerp(Paper, _rng.RandfRange(0.0f, 0.36f)), _rng.RandfRange(3.0f, 12.0f), _rng.RandfRange(life * 0.35f, life));
        }
    }

    private void AddText(string text, Vector2 pos, Color color, float size)
    {
        DamageText damageText = AddDamageTextObject();
        damageText.Text = text;
        damageText.Pos = pos;
        damageText.Color = color;
        damageText.Life = 1.15f;
        damageText.MaxLife = 1.15f;
        damageText.Size = size;
    }

    private Vector2 RandomArenaEdge()
    {
        int side = _rng.RandiRange(0, 3);
        return side switch
        {
            0 => new Vector2(_rng.RandfRange(Arena.Position.X, Arena.End.X), Arena.Position.Y + 12.0f),
            1 => new Vector2(Arena.End.X - 12.0f, _rng.RandfRange(Arena.Position.Y, Arena.End.Y)),
            2 => new Vector2(_rng.RandfRange(Arena.Position.X, Arena.End.X), Arena.End.Y - 12.0f),
            _ => new Vector2(Arena.Position.X + 12.0f, _rng.RandfRange(Arena.Position.Y, Arena.End.Y)),
        };
    }

    private Vector2 RandomDirection()
    {
        return Vector2.Right.Rotated(_rng.RandfRange(0.0f, Mathf.Tau));
    }

    private static Rect2 StartButtonRect()
    {
        return new Rect2(new Vector2(760.0f, 560.0f), new Vector2(400.0f, 78.0f));
    }

    private static Rect2 MetaButtonRect()
    {
        return new Rect2(new Vector2(690.0f, 874.0f), new Vector2(260.0f, 42.0f));
    }

    private static Rect2 TitleSettingsButtonRect()
    {
        return new Rect2(new Vector2(970.0f, 874.0f), new Vector2(260.0f, 42.0f));
    }

    private static Rect2 PilotCardRect(int index)
    {
        return new Rect2(new Vector2(230.0f + index * 365.0f, 692.0f), new Vector2(330.0f, 132.0f));
    }

    private static int PilotCount()
    {
        return 4;
    }

    private static PilotKind PilotFromIndex(int index)
    {
        return index switch
        {
            1 => PilotKind.Vesper,
            2 => PilotKind.Kairo,
            3 => PilotKind.Sol,
            _ => PilotKind.Astra,
        };
    }

    private bool IsPilotUnlocked(PilotKind pilot)
    {
        return pilot == PilotKind.Astra || _wonOnce || PilotUnlockProgress(pilot) >= 1.0f;
    }

    private float PilotUnlockProgress(PilotKind pilot)
    {
        return pilot switch
        {
            PilotKind.Vesper => Mathf.Max(
                Progress01(_bestWave, VesperUnlockWave),
                Progress01(_careerKills, VesperUnlockKills)),
            PilotKind.Kairo => Mathf.Max(
                Progress01(_bestWave, KairoUnlockWave),
                Mathf.Max(Progress01(_careerPickups, KairoUnlockPickups), Progress01(_runsCompleted, KairoUnlockRuns))),
            PilotKind.Sol => Mathf.Max(
                Progress01(_bestWave, SolUnlockWave),
                Mathf.Max(Progress01(_careerBossKills, SolUnlockBosses), Progress01(_careerPerfectWaves, SolUnlockPerfectWaves))),
            _ => 1.0f,
        };
    }

    private string PilotName(PilotKind pilot)
    {
        return pilot switch
        {
            PilotKind.Vesper => T("pilot.vesper.name"),
            PilotKind.Kairo => T("pilot.kairo.name"),
            PilotKind.Sol => T("pilot.sol.name"),
            _ => T("pilot.astra.name"),
        };
    }

    private string TitleName()
    {
        return _language == GameLanguage.Chinese ? "穿越星际" : "Interstellar";
    }

    private string TitleStartPrompt()
    {
        return _language == GameLanguage.Chinese ? "点击开始" : "Tap To Start";
    }

    private string PilotBody(PilotKind pilot)
    {
        return pilot switch
        {
            PilotKind.Vesper => T("pilot.vesper.body"),
            PilotKind.Kairo => T("pilot.kairo.body"),
            PilotKind.Sol => T("pilot.sol.body"),
            _ => T("pilot.astra.body"),
        };
    }

    private string PilotWeapon(PilotKind pilot)
    {
        return pilot switch
        {
            PilotKind.Vesper => T("pilot.vesper.weapon"),
            PilotKind.Kairo => T("pilot.kairo.weapon"),
            PilotKind.Sol => T("pilot.sol.weapon"),
            _ => T("pilot.astra.weapon"),
        };
    }

    private string UltimateName(PilotKind pilot)
    {
        return pilot switch
        {
            PilotKind.Vesper => T("ultimate.vesper"),
            PilotKind.Kairo => T("ultimate.kairo"),
            PilotKind.Sol => T("ultimate.sol"),
            _ => T("ultimate.astra"),
        };
    }

    private string PilotUnlockText(PilotKind pilot)
    {
        if (_language == GameLanguage.Chinese)
        {
            return pilot switch
            {
                PilotKind.Vesper => $"解锁：敌人 {Math.Min(_careerKills, VesperUnlockKills)}/{VesperUnlockKills} 或波次 {Math.Min(_bestWave, VesperUnlockWave)}/{VesperUnlockWave}",
                PilotKind.Kairo => $"解锁：掉落 {Math.Min(_careerPickups, KairoUnlockPickups)}/{KairoUnlockPickups} / 远征 {Math.Min(_runsCompleted, KairoUnlockRuns)}/{KairoUnlockRuns} / 波次 {Math.Min(_bestWave, KairoUnlockWave)}/{KairoUnlockWave}",
                PilotKind.Sol => $"解锁：Boss {Math.Min(_careerBossKills, SolUnlockBosses)}/{SolUnlockBosses} / 无伤 {Math.Min(_careerPerfectWaves, SolUnlockPerfectWaves)}/{SolUnlockPerfectWaves} / 波次 {Math.Min(_bestWave, SolUnlockWave)}/{SolUnlockWave}",
                _ => T("pilot.unlock.free"),
            };
        }

        return pilot switch
        {
            PilotKind.Vesper => $"Kills {Math.Min(_careerKills, VesperUnlockKills)}/{VesperUnlockKills} or Wave {Math.Min(_bestWave, VesperUnlockWave)}/{VesperUnlockWave}",
            PilotKind.Kairo => $"Drops {Math.Min(_careerPickups, KairoUnlockPickups)}/{KairoUnlockPickups} / Runs {Math.Min(_runsCompleted, KairoUnlockRuns)}/{KairoUnlockRuns} / Wave {Math.Min(_bestWave, KairoUnlockWave)}/{KairoUnlockWave}",
            PilotKind.Sol => $"Boss {Math.Min(_careerBossKills, SolUnlockBosses)}/{SolUnlockBosses} / Clean {Math.Min(_careerPerfectWaves, SolUnlockPerfectWaves)}/{SolUnlockPerfectWaves} / Wave {Math.Min(_bestWave, SolUnlockWave)}/{SolUnlockWave}",
            _ => T("pilot.unlock.free"),
        };
    }

    private static Color PilotAccent(PilotKind pilot)
    {
        return pilot switch
        {
            PilotKind.Vesper => AlertRed,
            PilotKind.Kairo => PickupBlue,
            PilotKind.Sol => Gold,
            _ => PolarityBlue,
        };
    }

    private static Rect2 HudSettingsButtonRect()
    {
        return new Rect2(new Vector2(1804.0f, 26.0f), new Vector2(42.0f, 42.0f));
    }

    private static Rect2 SettingsGuideButtonRect()
    {
        return new Rect2(new Vector2(760.0f, 402.0f), new Vector2(400.0f, 58.0f));
    }

    private static Rect2 SettingsLanguageButtonRect()
    {
        return new Rect2(new Vector2(790.0f, 478.0f), new Vector2(340.0f, 48.0f));
    }

    private static Rect2 SettingsMainMenuButtonRect()
    {
        return new Rect2(new Vector2(790.0f, 550.0f), new Vector2(340.0f, 48.0f));
    }

    private Rect2 SettingsDeleteSaveButtonRect()
    {
        float y = IsRunViewMode(_settingsReturnMode) ? 622.0f : 550.0f;
        return new Rect2(new Vector2(790.0f, y), new Vector2(340.0f, 48.0f));
    }

    private Rect2 SettingsBackButtonRect()
    {
        float y = IsRunViewMode(_settingsReturnMode) ? 702.0f : 630.0f;
        return new Rect2(new Vector2(790.0f, y), new Vector2(340.0f, 50.0f));
    }

    private static Rect2 GuideBackButtonRect()
    {
        return new Rect2(new Vector2(790.0f, 828.0f), new Vector2(340.0f, 50.0f));
    }

    private static Rect2 MetaBackButtonRect()
    {
        return new Rect2(new Vector2(790.0f, 916.0f), new Vector2(340.0f, 50.0f));
    }

    private static Rect2 MetaUpgradeRect(int index)
    {
        const float width = 390.0f;
        const float height = 145.0f;
        const float gapX = 24.0f;
        const float gapY = 20.0f;
        int column = index % 4;
        int row = index / 4;
        return new Rect2(new Vector2(144.0f + column * (width + gapX), 336.0f + row * (height + gapY)), new Vector2(width, height));
    }

    private static Vector2 RandomOrbit(float phase)
    {
        return new Vector2(Mathf.Sin(phase * 1.3f), Mathf.Cos(phase * 0.9f));
    }

    private void AddWaveEnemyCallout(EnemyKind primary, EnemyKind support, Color accent)
    {
        bool newEnemy = _wave == FirstAppearanceWave(primary);
        string header = newEnemy ? Tf("wave.enemy.new", EnemyName(primary)) : Tf("wave.enemy.focus", EnemyName(primary), EnemyRole(primary));
        AddText(header, ScreenCenter + new Vector2(0.0f, -160.0f), newEnemy ? accent : Alpha(Paper, 0.82f), newEnemy ? 28.0f : 21.0f);
        if (newEnemy)
        {
            AddText(EnemyRole(primary), ScreenCenter + new Vector2(0.0f, -126.0f), Alpha(Paper, 0.72f), 18.0f);
        }
        else if (support != primary)
        {
            AddText(Tf("wave.enemy.support", EnemyName(support)), ScreenCenter + new Vector2(0.0f, -130.0f), Alpha(accent, 0.68f), 17.0f);
        }
    }

    private static EnemyKind SelectEnemyKind(int index, int sector, int waveInSector, EnemyKind primary, EnemyKind support)
    {
        if (support == primary)
        {
            return primary;
        }

        int supportEvery = sector >= 3 || waveInSector >= 6 ? 4 : 5;
        return index > 0 && index % supportEvery == supportEvery - 1 ? support : primary;
    }

    private static EnemyKind WavePrimaryEnemyKind(int sector, int waveInSector)
    {
        return sector switch
        {
            0 => waveInSector switch
            {
                1 => EnemyKind.Chaser,
                2 => EnemyKind.Weaver,
                3 => EnemyKind.Chaser,
                4 => EnemyKind.Drifter,
                5 => EnemyKind.Turret,
                6 => EnemyKind.Splitter,
                _ => EnemyKind.Lance,
            },
            1 => waveInSector switch
            {
                1 => EnemyKind.Lance,
                2 => EnemyKind.Mine,
                3 => EnemyKind.Bulwark,
                4 => EnemyKind.Weaver,
                5 => EnemyKind.Turret,
                6 => EnemyKind.Splitter,
                _ => EnemyKind.Drifter,
            },
            2 => waveInSector switch
            {
                1 => EnemyKind.Shard,
                2 => EnemyKind.Mine,
                3 => EnemyKind.Siren,
                4 => EnemyKind.Bulwark,
                5 => EnemyKind.Turret,
                6 => EnemyKind.Splitter,
                _ => EnemyKind.Shard,
            },
            3 => waveInSector switch
            {
                1 => EnemyKind.Harrier,
                2 => EnemyKind.Shard,
                3 => EnemyKind.Warden,
                4 => EnemyKind.Mine,
                5 => EnemyKind.Siren,
                6 => EnemyKind.Bulwark,
                _ => EnemyKind.Lance,
            },
            _ => waveInSector switch
            {
                1 => EnemyKind.Harrier,
                2 => EnemyKind.Bulwark,
                3 => EnemyKind.Warden,
                4 => EnemyKind.Mine,
                5 => EnemyKind.Splitter,
                6 => EnemyKind.Siren,
                _ => EnemyKind.Warden,
            },
        };
    }

    private static EnemyKind WaveSupportEnemyKind(int sector, int waveInSector)
    {
        return sector switch
        {
            0 => waveInSector switch
            {
                3 => EnemyKind.Weaver,
                5 => EnemyKind.Chaser,
                6 => EnemyKind.Weaver,
                7 => EnemyKind.Chaser,
                _ => WavePrimaryEnemyKind(sector, waveInSector),
            },
            1 => waveInSector switch
            {
                1 => EnemyKind.Chaser,
                4 => EnemyKind.Mine,
                5 => EnemyKind.Lance,
                6 => EnemyKind.Mine,
                7 => EnemyKind.Lance,
                _ => WavePrimaryEnemyKind(sector, waveInSector),
            },
            2 => waveInSector switch
            {
                2 => EnemyKind.Chaser,
                4 => EnemyKind.Weaver,
                5 => EnemyKind.Shard,
                6 => EnemyKind.Mine,
                7 => EnemyKind.Siren,
                _ => WavePrimaryEnemyKind(sector, waveInSector),
            },
            3 => waveInSector switch
            {
                2 => EnemyKind.Harrier,
                4 => EnemyKind.Harrier,
                5 => EnemyKind.Warden,
                6 => EnemyKind.Shard,
                7 => EnemyKind.Warden,
                _ => WavePrimaryEnemyKind(sector, waveInSector),
            },
            _ => waveInSector switch
            {
                1 => EnemyKind.Shard,
                2 => EnemyKind.Siren,
                3 => EnemyKind.Shard,
                4 => EnemyKind.Lance,
                5 => EnemyKind.Harrier,
                6 => EnemyKind.Turret,
                7 => EnemyKind.Bulwark,
                _ => WavePrimaryEnemyKind(sector, waveInSector),
            },
        };
    }

    private static int FirstAppearanceWave(EnemyKind kind)
    {
        return kind switch
        {
            EnemyKind.Chaser => 1,
            EnemyKind.Weaver => 2,
            EnemyKind.Drifter => 4,
            EnemyKind.Turret => 5,
            EnemyKind.Splitter => 6,
            EnemyKind.Lance => 7,
            EnemyKind.Mine => 10,
            EnemyKind.Bulwark => 11,
            EnemyKind.Shard => 17,
            EnemyKind.Siren => 19,
            EnemyKind.Harrier => 25,
            EnemyKind.Warden => 27,
            _ => 1,
        };
    }

    private string EnemyName(EnemyKind kind)
    {
        return T($"enemy.{EnemyKey(kind)}.name");
    }

    private string EnemyRole(EnemyKind kind)
    {
        return T($"enemy.{EnemyKey(kind)}.role");
    }

    private static string EnemyKey(EnemyKind kind)
    {
        return kind switch
        {
            EnemyKind.Chaser => "chaser",
            EnemyKind.Weaver => "weaver",
            EnemyKind.Turret => "turret",
            EnemyKind.Splitter => "splitter",
            EnemyKind.Lance => "lance",
            EnemyKind.Mine => "mine",
            EnemyKind.Shard => "shard",
            EnemyKind.Warden => "warden",
            EnemyKind.Drifter => "drifter",
            EnemyKind.Bulwark => "bulwark",
            EnemyKind.Siren => "siren",
            EnemyKind.Harrier => "harrier",
            _ => "chaser",
        };
    }

    private int CurrentSectorIndex()
    {
        return Mathf.Clamp((_wave <= 0 ? 0 : _wave - 1) / WavesPerSector, 0, SectorCount - 1);
    }

    private int CurrentWaveInSector()
    {
        return _wave <= 0 ? 0 : ((_wave - 1) % WavesPerSector) + 1;
    }

    private SectorInfo CurrentSector()
    {
        return Sectors[CurrentSectorIndex()];
    }

    private float ThreatLevel()
    {
        return CurrentWaveInSector() + CurrentSectorIndex() * 7.5f;
    }

    private float RunProgress01()
    {
        if (_wave <= 1)
        {
            return 0.0f;
        }

        return Mathf.Clamp((_wave - 1.0f) / (TotalWaves - 1.0f), 0.0f, 1.0f);
    }

    private int EnemyBulletCap()
    {
        int cap = Mathf.RoundToInt(Mathf.Lerp(EnemyBulletCapStart, EnemyBulletCapEnd, RunProgress01()));
        if (_enemies.Count > 48)
        {
            cap -= (_enemies.Count - 48) * 2;
        }
        return Math.Max(EnemyBulletCapStart, cap);
    }

    private int ActiveEnemyBulletCount()
    {
        return _activeEnemyBullets;
    }

    private int ScaledEnemyBulletCount(EnemyKind kind, int baseCount)
    {
        if (baseCount <= 1)
        {
            return 1;
        }

        float progress = RunProgress01();
        float density = kind == EnemyKind.Boss
            ? Mathf.Lerp(0.62f, 1.0f, progress)
            : Mathf.Lerp(0.46f, 1.0f, progress);
        int scaled = Mathf.RoundToInt(baseCount * density);
        return Mathf.Clamp(scaled, 1, baseCount);
    }

    private int ScaledEnemyPatternCount(EnemyKind kind, int baseCount)
    {
        if (baseCount <= 1)
        {
            return 1;
        }

        float progress = RunProgress01();
        float density = kind == EnemyKind.Boss
            ? Mathf.Lerp(0.62f, 1.0f, progress)
            : Mathf.Lerp(0.46f, 1.0f, progress);
        int scaled = Mathf.RoundToInt(baseCount * density);
        return Mathf.Clamp(scaled, 1, baseCount);
    }

    private float ScaledEnemyBulletSpeed(EnemyKind kind, float baseSpeed)
    {
        float progress = RunProgress01();
        float scale = Mathf.Lerp(EnemyBulletSpeedStartScale, EnemyBulletSpeedEndScale, progress);
        float cap = Mathf.Lerp(410.0f, 740.0f, progress);

        if (kind == EnemyKind.Lance || kind == EnemyKind.Shard || kind == EnemyKind.Harrier)
        {
            cap += 90.0f;
        }
        else if (kind == EnemyKind.Bulwark || kind == EnemyKind.Siren)
        {
            cap -= 70.0f;
        }
        else if (kind == EnemyKind.Boss)
        {
            cap += 120.0f;
        }

        return Mathf.Min(baseSpeed * scale, cap);
    }

    private float EnemyBulletLife()
    {
        return Mathf.Lerp(5.65f, 4.8f, RunProgress01());
    }

    private Vector2 ClampToArena(Vector2 point, float margin)
    {
        return new Vector2(
            Mathf.Clamp(point.X, Arena.Position.X + margin, Arena.End.X - margin),
            Mathf.Clamp(point.Y, Arena.Position.Y + margin, Arena.End.Y - margin));
    }

    private Rect2 ExpandedArena(float amount)
    {
        return Arena.Grow(amount);
    }

    private static float DistanceToSegment(Vector2 point, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float denominator = Mathf.Max(0.001f, ab.LengthSquared());
        float t = Mathf.Clamp((point - a).Dot(ab) / denominator, 0.0f, 1.0f);
        return point.DistanceTo(a + ab * t);
    }

    private Color PolarityColor(int polarity)
    {
        return polarity == 0 ? PolarityBlue : PolarityAmber;
    }

    private Color PickupColor(PickupKind kind)
    {
        return kind switch
        {
            PickupKind.Dust => XpGreen,
            PickupKind.Energy => PickupBlue,
            PickupKind.Repair => AlertRed.Lerp(Paper, 0.16f),
            _ => Paper,
        };
    }

    private static Color Alpha(Color color, float alpha)
    {
        return new Color(color.R, color.G, color.B, Mathf.Clamp(alpha, 0.0f, 1.0f));
    }

    private static float Approach(float value, float target, float amount)
    {
        if (value < target)
        {
            return Mathf.Min(value + amount, target);
        }
        return Mathf.Max(value - amount, target);
    }

    private Vector2 ShakeOffset()
    {
        if (_shake <= 0.001f)
        {
            return Vector2.Zero;
        }
        return new Vector2(
            Mathf.Sin(_time * 71.0f + _noiseSeed) * 18.0f * _shake,
            Mathf.Cos(_time * 83.0f - _noiseSeed) * 14.0f * _shake);
    }

    private Vector2[] RegularPolygon(Vector2 center, float radius, int sides, float rotation)
    {
        Vector2[] points = new Vector2[sides];
        for (int i = 0; i < sides; i++)
        {
            float a = rotation + i * Mathf.Tau / sides;
            points[i] = center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
        }
        return points;
    }

    private static Vector2[] ClosePolygon(Vector2[] polygon)
    {
        Vector2[] closed = new Vector2[polygon.Length + 1];
        for (int i = 0; i < polygon.Length; i++)
        {
            closed[i] = polygon[i];
        }
        closed[^1] = polygon[0];
        return closed;
    }

    private GameLanguage DetectLanguage()
    {
        string locale = OS.GetLocaleLanguage();
        return locale.StartsWith("zh", StringComparison.OrdinalIgnoreCase) ? GameLanguage.Chinese : GameLanguage.English;
    }

    private string T(string key)
    {
        if (!Texts.TryGetValue(key, out LocalizedText text))
        {
            return key;
        }
        return _language == GameLanguage.Chinese ? text.Chinese : text.English;
    }

    private string Tf(string key, params object[] args)
    {
        return string.Format(System.Globalization.CultureInfo.InvariantCulture, T(key), args);
    }

    private string SameRuleText()
    {
        return _language == GameLanguage.Chinese ? "红弹危险" : "RED FIRE IS DANGER";
    }

    private string OppositeRuleText()
    {
        return _language == GameLanguage.Chinese ? "过热突击" : "ASSAULT OVERHEAT";
    }

    private string PolarityTipText()
    {
        if (_language == GameLanguage.Chinese)
        {
            return _playerPolarity == 0 ? "巡航态：擦过红弹蓄势，等待破绽" : "突击态：抓住过热窗口爆发";
        }
        return _playerPolarity == 0 ? "CRUISE: graze red fire, build charge" : "ASSAULT: punish overheat windows";
    }

    private string PolarityCooldownText()
    {
        int seconds = Mathf.CeilToInt(Mathf.Max(0.0f, _polarityCooldown));
        return _language == GameLanguage.Chinese ? $"姿态锁定 {seconds} 秒" : $"STANCE LOCK {seconds}s";
    }

    private string ChargeText()
    {
        return _language == GameLanguage.Chinese ? "蓄势" : "CHARGE";
    }

    private string AssaultText(float power)
    {
        int percent = Mathf.RoundToInt((power - 1.0f) * 100.0f);
        return _language == GameLanguage.Chinese ? $"突击 +{percent}%" : $"ASSAULT +{percent}%";
    }

    private string CruiseText()
    {
        return _language == GameLanguage.Chinese ? "巡航重整" : "CRUISE RESET";
    }

    private string AbsorbText(float amount)
    {
        int value = Mathf.RoundToInt(amount);
        return _language == GameLanguage.Chinese ? $"清弹 +{value}" : $"CLEAR +{value}";
    }

    private string CounterText()
    {
        return _language == GameLanguage.Chinese ? "过热破绽" : "OVERHEAT BREAK";
    }

    private string ObjectiveText(RunObjective objective)
    {
        if (_language == GameLanguage.Chinese)
        {
            return objective.Kind switch
            {
                RunObjectiveKind.ReachWave => $"通过第 {objective.Target} 波",
                RunObjectiveKind.PerfectWaves => $"无伤清波 {objective.Target} 次",
                RunObjectiveKind.DefeatEnemies => $"击败敌人 {objective.Target} 个",
                RunObjectiveKind.AbsorbBullets => $"擦弹或清弹 {objective.Target} 次",
                RunObjectiveKind.CollectPickups => $"拾取掉落 {objective.Target} 个",
                RunObjectiveKind.BestCombo => $"最高连击 {objective.Target}",
                RunObjectiveKind.DefeatBosses => $"击败 Boss {objective.Target} 个",
                _ => "完成远征目标",
            };
        }

        return objective.Kind switch
        {
            RunObjectiveKind.ReachWave => $"Clear wave {objective.Target}",
            RunObjectiveKind.PerfectWaves => $"Clear {objective.Target} clean waves",
            RunObjectiveKind.DefeatEnemies => $"Defeat {objective.Target} enemies",
            RunObjectiveKind.AbsorbBullets => $"Graze or clear {objective.Target} bullets",
            RunObjectiveKind.CollectPickups => $"Collect {objective.Target} drops",
            RunObjectiveKind.BestCombo => $"Reach {objective.Target} combo",
            RunObjectiveKind.DefeatBosses => $"Defeat {objective.Target} bosses",
            _ => "Complete expedition goal",
        };
    }

    private string NextWavePreviewText()
    {
        int nextWave = Math.Min(_wave + 1, TotalWaves);
        int sector = Mathf.Clamp((nextWave - 1) / WavesPerSector, 0, SectorCount - 1);
        int waveInSector = ((nextWave - 1) % WavesPerSector) + 1;
        if (waveInSector == WavesPerSector)
        {
            string boss = Tf("boss.sector", T(Sectors[sector].NameKey));
            return _language == GameLanguage.Chinese ? $"下一波：{boss}" : $"NEXT: {boss}";
        }

        EnemyKind primary = WavePrimaryEnemyKind(sector, waveInSector);
        EnemyKind support = WaveSupportEnemyKind(sector, waveInSector);
        if (support == primary)
        {
            return _language == GameLanguage.Chinese
                ? $"下一波主敌：{EnemyName(primary)}"
                : $"NEXT PRIMARY: {EnemyName(primary)}";
        }

        return _language == GameLanguage.Chinese
            ? $"下一波主敌：{EnemyName(primary)}    辅助：{EnemyName(support)}"
            : $"NEXT PRIMARY: {EnemyName(primary)}    SUPPORT: {EnemyName(support)}";
    }

    private string[] GuideLines()
    {
        if (_language == GameLanguage.Chinese)
        {
            return new[]
            {
                "WASD 或方向键移动。鼠标控制射击方向，武器会自动开火。",
                "敌方子弹统一是红色，红弹永远危险。巡航态可以通过擦弹和稳定命中蓄势。",
                "敌人蓄压后会齐射红弹，齐射结束会短暂过热。过热时切入突击态打爆发。",
                "切换姿态后会锁定一小段时间。巡航用于整理战场，突击用于抓破绽输出。",
                "右键或 Shift 冲刺。冲刺时短暂无敌，并清除近身子弹。",
                "F 或 E 消耗能量释放角色大招。每个角色的大招效果不同。",
                "每波结束后选择一个升级。局外星尘可购买永久成长。",
            };
        }

        return new[]
        {
            "Move with WASD or arrow keys. Aim with the mouse; weapons fire automatically.",
            "Enemy bullets are always red, and red fire is always danger. Cruise mode builds charge by grazing and landing steady hits.",
            "Enemies telegraph, fire a volley, then briefly overheat. Shift into Assault during overheat windows for burst damage.",
            "Switching stance locks you in briefly. Cruise to stabilize the field; Assault to punish openings.",
            "Right mouse or Shift dashes. Dash briefly grants invulnerability and clears nearby bullets.",
            "F or E spends energy on your role ultimate. Every pilot has a different ultimate.",
            "Choose one upgrade after each wave. Spend Star Dust between runs for permanent growth.",
        };
    }

    private string TutorialText(int id)
    {
        if (_language == GameLanguage.Chinese)
        {
            return id switch
            {
                1 => "红弹永远危险。巡航态擦过红弹可以蓄势。",
                2 => "敌人齐射后会过热。切入突击态打破绽。",
                3 => "姿态切换有冷却：先判断防守窗口还是攻击窗口。",
                9 => "光束同样危险。用冲刺或大招清出通路。",
                _ => string.Empty,
            };
        }

        return id switch
        {
            1 => "Red fire is always danger. Graze in Cruise mode to build charge.",
            2 => "Enemies overheat after volleys. Shift into Assault to punish openings.",
            3 => "Stance shift has a cooldown. Read the field, then commit.",
            9 => "Beams are dangerous too. Dash or use ultimates to open lanes.",
            _ => string.Empty,
        };
    }

    private void UpdateLanguageToggle()
    {
        if (!KeyDown(Key.L) || _lastLanguage)
        {
            return;
        }

        ToggleLanguage();
    }

    private void ToggleLanguage()
    {
        _language = _language == GameLanguage.English ? GameLanguage.Chinese : GameLanguage.English;
        RefreshUpgradeChoiceText();
        AddText(T("language.changed"), ScreenCenter + new Vector2(0.0f, -265.0f), Jade, 26.0f);
        PlaySfx(520.0f, 160.0f, 0.16f, 0.22f, 0.02f, 1);
    }

    private void RefreshUpgradeChoiceText()
    {
        for (int i = 0; i < _upgradeChoices.Count; i++)
        {
            Rect2 rect = _upgradeChoices[i].Rect;
            UpgradeCard localized = CreateCard(_upgradeChoices[i].Id);
            localized.Rect = rect;
            _upgradeChoices[i] = localized;
        }
    }

    private bool KeyDown(Key key)
    {
        return Input.IsKeyPressed(key);
    }

    private void CaptureButtons()
    {
        _lastStart = KeyDown(Key.Enter);
        _lastToggle = KeyDown(Key.Space);
        _lastNova = KeyDown(Key.F) || KeyDown(Key.E);
        _lastDash = KeyDown(Key.Shift) || Input.IsMouseButtonPressed(MouseButton.Right);
        _lastClick = Input.IsMouseButtonPressed(MouseButton.Left);
        _lastRestart = KeyDown(Key.Enter) || Input.IsMouseButtonPressed(MouseButton.Left);
        _lastOne = KeyDown(Key.Key1);
        _lastTwo = KeyDown(Key.Key2);
        _lastThree = KeyDown(Key.Key3);
        _lastFour = KeyDown(Key.Key4);
        _lastFive = KeyDown(Key.Key5);
        _lastSix = KeyDown(Key.Key6);
        _lastSeven = KeyDown(Key.Key7);
        _lastEight = KeyDown(Key.Key8);
        _lastNine = KeyDown(Key.Key9);
        _lastReroll = KeyDown(Key.R);
        _lastLanguage = KeyDown(Key.L);
        _lastMeta = KeyDown(Key.U);
        _lastBack = KeyDown(Key.Escape);
    }

    private void SetupAudio()
    {
        AudioStreamGenerator generator = new()
        {
            MixRate = SampleRate,
            BufferLength = 0.18f,
        };
        _musicPlayer = new AudioStreamPlayer
        {
            Stream = generator,
            VolumeDb = -11.0f,
        };
        AddChild(_musicPlayer);
        _musicPlayer.Play();
        _musicPlayback = _musicPlayer.GetStreamPlayback() as AudioStreamGeneratorPlayback;
    }

    private void FillAudio()
    {
        if (_musicPlayback == null)
        {
            return;
        }

        int frames = Math.Min(_musicPlayback.GetFramesAvailable(), 2048);
        for (int i = 0; i < frames; i++)
        {
            float sample = MusicSample() + SfxSample();
            sample = Mathf.Clamp(sample, -0.85f, 0.85f);
            _musicPlayback.PushFrame(new Vector2(sample, sample));
            _musicClock += 1.0f / SampleRate;
        }
    }

    private float MusicSample()
    {
        int[] scale = { 0, 3, 5, 7, 10, 12, 15, 17 };
        float beat = _musicClock * (110.0f / 60.0f);
        int step = (int)MathF.Floor(beat * 2.0f) % scale.Length;
        float root = _mode == GameMode.GameOver ? 48.999f : _mode == GameMode.Victory ? 73.416f : 55.0f;
        float note = root * MathF.Pow(2.0f, scale[step] / 12.0f);
        float bass = MathF.Sin(Mathf.Tau * note * _musicClock) * 0.08f;
        float pad = MathF.Sin(Mathf.Tau * note * 0.5f * _musicClock + MathF.Sin(_musicClock * 0.7f) * 0.8f) * 0.05f;
        float shimmer = MathF.Sin(Mathf.Tau * note * 2.0f * _musicClock) * 0.018f * (0.5f + 0.5f * MathF.Sin(beat * Mathf.Tau));
        return (bass + pad + shimmer) * (_mode == GameMode.Title ? 0.6f : 1.0f);
    }

    private float SfxSample()
    {
        float output = 0.0f;
        for (int i = _voices.Count - 1; i >= 0; i--)
        {
            SfxVoice voice = _voices[i];
            voice.Age += 1.0f / SampleRate;
            if (voice.Age >= voice.Life)
            {
                _voices.RemoveAt(i);
                continue;
            }

            float t = voice.Age / voice.Life;
            float env = MathF.Sin(MathF.PI * t) * (1.0f - t * 0.45f);
            float freq = MathF.Max(20.0f, voice.Frequency + voice.Sweep * t);
            float wave = voice.Wave switch
            {
                0 => MathF.Sin(Mathf.Tau * freq * voice.Age),
                1 => MathF.Sin(Mathf.Tau * freq * voice.Age) > 0.0f ? 1.0f : -1.0f,
                _ => 2.0f * (voice.Age * freq - MathF.Floor(0.5f + voice.Age * freq)),
            };
            float noise = (_rng.Randf() * 2.0f - 1.0f) * voice.Noise;
            output += (wave * (1.0f - voice.Noise) + noise) * env * voice.Volume;
        }
        return output;
    }

    private void PlaySfx(float frequency, float sweep, float life, float volume, float noise, int wave)
    {
        if (_voices.Count > 24)
        {
            _voices.RemoveAt(0);
        }

        _voices.Add(new SfxVoice
        {
            Frequency = frequency,
            Sweep = sweep,
            Life = life,
            Volume = volume,
            Noise = noise,
            Wave = wave,
        });
    }
}
