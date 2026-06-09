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
    private const int DifficultyCount = 3;
    private const int ClearRecordCount = 5;
    private const float BossHpTrialScale = 2.0f;
    private const float PlayerRadius = 24.0f;
    private const float EnemyBulletRadius = 8.0f;
    private const float PlayerBulletRadius = 7.0f;
    private const float SampleRate = 44100.0f;
    private const int MaxSfxVoices = 16;
    private const float SfxGlobalGain = 0.48f;
    private const float SfxNoiseScale = 0.18f;
    private const int MaxEnemies = 96;
    private const int MaxShots = 480;
    private const int MaxPickups = 260;
    private const int MaxParticles = 720;
    private const int MaxDamageTexts = 72;
    private const int MaxOrbiters = 10;
    private const int MaxDroneCommandCues = 28;
    private const int MaxPoolSize = 900;
    private const float EnemyGridCellSize = 160.0f;
    private const int EnemyGridColumns = 12;
    private const int EnemyGridRows = 7;
    private const int EnemyGridCellCount = EnemyGridColumns * EnemyGridRows;
    private const int PolygonScratchMaxSides = 24;
    private const int PolygonScratchSlots = 16;
    private const int PlayerTrailCapacity = 14;
    private const int CruiseStance = 0;
    private const int AssaultStance = 1;
    private const float PolaritySwitchCooldownBase = 6.4f;
    private const float PolaritySwitchCooldownMin = 3.4f;
    private const float EnemyBulletSpeedStartScale = 0.54f;
    private const float EnemyBulletSpeedEndScale = 0.84f;
    private const int EnemyBulletCapStart = 72;
    private const int EnemyBulletCapEnd = 240;
    private const float CruiseChargeMax = 100.0f;
    private const float CruiseGrazeRadius = 58.0f;
    private const float AssaultBurstMin = 1.45f;
    private const float AssaultBurstMax = 4.4f;
    private const float UltimateCooldownBase = 8.5f;
    private const float UltimateCostBase = 108.0f;
    private const float UltimateCostFloor = 88.0f;
    private const int ScoreCacheBase = 12000;
    private const int ScoreCacheStep = 9000;
    private const int XpBase = 48;
    private const int XpStep = 14;
    private const float EnemyTelegraphLead = 0.58f;
    private const float EnemyOverheatBase = 1.55f;
    private const float UiHairline = 0.8f;
    private const float UiStroke = 1.05f;
    private const float UiAccentStroke = 1.3f;
    private const float PilotTextureTargetArea = 7100.0f;
    private const float PilotTextureMinMaxSide = 84.0f;
    private const float PilotTextureMaxMaxSide = 100.0f;
    private const float EnemyTextureRootScale = 2.36f;
    private const float EliteEnemyTextureRootScale = 2.56f;
    private const float EnemyTextureMinMaxSideScale = 2.1f;
    private const float EnemyTextureMaxMaxSideScale = 3.06f;
    private const float BossTextureRootScale = 2.72f;
    private const float BossTextureMinMaxSideScale = 2.35f;
    private const float BossTextureMaxMaxSideScale = 3.68f;
    private const float DroneTextureSupportTargetArea = 1250.0f;
    private const float DroneTextureKairoTargetArea = 1420.0f;
    private const float DroneTextureMinMaxSide = 30.0f;
    private const float DroneTextureMaxMaxSide = 46.0f;
    private const float DroneCommandTextureTargetArea = 3400.0f;
    private const float DroneCommandTextureMinMaxSide = 52.0f;
    private const float DroneCommandTextureMaxMaxSide = 76.0f;
    private const int MaxJoypadSlots = 8;
    private const float GamepadStickDeadZone = 0.24f;
    private const float GamepadAimDeadZone = 0.2f;
    private const float GamepadTriggerThreshold = 0.45f;
    private const float GamepadNavThreshold = 0.56f;
    private const float GamepadNavRepeat = 0.18f;
    private const int TitleDifficultyFocusStart = 0;
    private const int TitlePilotFocusStart = TitleDifficultyFocusStart + DifficultyCount;
    private const int TitleFooterFocusStart = TitlePilotFocusStart + 1;
    private const int TitleMetaFocus = TitleFooterFocusStart;
    private const int TitleStartFocus = TitleFooterFocusStart + 1;
    private const int TitleSettingsFocus = TitleFooterFocusStart + 2;
    private static readonly float[] BossPhaseThresholds = { 0.72f, 0.43f, 0.18f };
    private static readonly int[] BuildMilestoneThresholds = { 5, 8, 11 };

    private static List<float>[] CreateClearRecordLists()
    {
        List<float>[] lists = new List<float>[DifficultyCount];
        for (int i = 0; i < lists.Length; i++)
        {
            lists[i] = new List<float>();
        }

        return lists;
    }

    private static OrbiterVisual[] CreateOrbiterVisuals()
    {
        OrbiterVisual[] visuals = new OrbiterVisual[MaxOrbiters];
        for (int i = 0; i < visuals.Length; i++)
        {
            visuals[i] = new OrbiterVisual();
        }

        return visuals;
    }

    private static Vector2[][][] CreatePolygonScratch(int extraPoint)
    {
        Vector2[][][] buffers = new Vector2[PolygonScratchMaxSides + 1][][];
        for (int sides = 0; sides < buffers.Length; sides++)
        {
            buffers[sides] = new Vector2[PolygonScratchSlots][];
            int length = Math.Max(1, sides + extraPoint);
            for (int slot = 0; slot < PolygonScratchSlots; slot++)
            {
                buffers[sides][slot] = new Vector2[length];
            }
        }

        return buffers;
    }

    private static readonly Vector2 ScreenCenter = new(ScreenWidth * 0.5f, ScreenHeight * 0.5f);
    private static readonly Rect2 Arena = new(new Vector2(84.0f, 86.0f), new Vector2(1752.0f, 884.0f));
    private static readonly Rect2 ShotCullBounds = Arena.Grow(180.0f);
    private static readonly Color Void = new(0.002f, 0.004f, 0.008f);
    private static readonly Color Ink = new(0.008f, 0.015f, 0.022f);
    private static readonly Color Cyan = new(0.0f, 0.78f, 0.9f);
    private static readonly Color Gold = new(1.0f, 0.58f, 0.03f);
    private static readonly Color Rose = new(0.96f, 0.0f, 0.52f);
    private static readonly Color Jade = new(0.0f, 0.88f, 0.72f);
    private static readonly Color Violet = new(0.54f, 0.42f, 0.92f);
    private static readonly Color Paper = new(0.9f, 0.95f, 0.98f);
    private static readonly Color Steel = new(0.43f, 0.5f, 0.54f);
    private static readonly Color Graphite = new(0.018f, 0.028f, 0.036f);
    private static readonly Color Panel = new(0.035f, 0.06f, 0.067f);
    private static readonly Color GridLine = new(0.12f, 0.28f, 0.32f);
    private static readonly Color PolarityBlue = new(0.0f, 0.72f, 0.86f);
    private static readonly Color PolarityAmber = new(1.0f, 0.61f, 0.06f);
    private static readonly Color XpGreen = new(0.56f, 0.66f, 0.62f);
    private static readonly Color PickupBlue = new(0.12f, 0.52f, 0.86f);
    private static readonly Color AlertRed = new(0.98f, 0.04f, 0.36f);
    private static readonly Color EnemyFireRed = new(1.0f, 0.08f, 0.05f);
    private static readonly string[] UpgradeHighlightTerms =
    {
        "MAX RANK",
        "Max Rank",
        "Star Dust",
        "EXP",
        "Boss guard",
        "Boss",
        "red fire",
        "enemy bullets",
        "emergency clear",
        "overheated",
        "overheat",
        "low-hull",
        "guard-broken",
        "weapon",
        "pilot skill",
        "tactical",
        "skill",
        "ultimate",
        "energy",
        "hull",
        "shield",
        "invulnerable",
        "invulnerability",
        "dash",
        "focus",
        "pickup",
        "pickups",
        "reroll",
        "bullets",
        "clear",
        "chain",
        "chains",
        "split",
        "shards",
        "ricochet",
        "stasis",
        "magnet",
        "singularity",
        "execution",
        "mark",
        "marks",
        "rhythm",
        "echo",
        "orbit",
        "pulse",
        "pulses",
        "prism",
        "rail",
        "gravity",
        "lance",
        "lances",
        "pierce",
        "drone",
        "drones",
        "swarm",
        "corona",
        "volley",
        "volleys",
        "质变",
        "Boss",
        "Boss 护盾",
        "Boss 破盾",
        "红弹",
        "敌方子弹",
        "紧急清弹",
        "清弹",
        "过热",
        "破绽",
        "武器",
        "角色技能",
        "战术",
        "技能",
        "大招",
        "专注",
        "生命",
        "能量",
        "护盾",
        "无敌",
        "冲刺",
        "拾取物",
        "拾取",
        "重抽",
        "子弹",
        "弹幕",
        "弹道",
        "穿透",
        "连锁",
        "分裂",
        "碎片",
        "回弹",
        "停滞",
        "磁吸",
        "奇点",
        "处决",
        "标记",
        "节拍",
        "节奏",
        "回响",
        "环绕",
        "脉冲",
        "棱镜",
        "轨道",
        "重力",
        "星矛",
        "长枪",
        "无人机",
        "蜂群",
        "日冕",
        "齐射",
        "星尘",
        "弹匣",
        "壁垒",
        "ПРЕДЕЛ",
        "БОСС",
        "босс",
        "энергия",
        "энергию",
        "корпус",
        "щит",
        "защита",
        "перегрев",
        "пули",
        "очистка",
        "рой",
        "дроны",
        "цепи",
        "раскол",
        "рикошет",
        "стазис",
        "сингулярность",
        "ÁPICE",
        "chefe",
        "energia",
        "casco",
        "escudo",
        "guarda",
        "tiros",
        "limpeza",
        "superaquecimento",
        "drones",
        "enxame",
        "cadeia",
        "divisão",
        "ricochete",
        "estase",
        "singularidade",
        "KRÖNUNG",
        "Bosswache",
        "Boss",
        "Energie",
        "Hülle",
        "Schild",
        "Kugeln",
        "Noträumung",
        "Überhitzung",
        "Drohnen",
        "Schwarm",
        "Kette",
        "Splitter",
        "Abpraller",
        "Stasis",
        "Singularität",
        "DORUK",
        "boss",
        "enerji",
        "gövde",
        "kalkan",
        "koruma",
        "mermi",
        "temizleme",
        "aşırı ısı",
        "dron",
        "sürü",
        "zincir",
        "bölünme",
        "sekme",
        "durgunluk",
        "tekillik",
        "APOGÉE",
        "boss",
        "énergie",
        "coque",
        "bouclier",
        "garde",
        "tirs",
        "nettoyage",
        "surchauffe",
        "drones",
        "essaim",
        "chaîne",
        "chaînes",
        "fragmentation",
        "ricochet",
        "stase",
        "singularité",
        "最大",
        "Boss",
        "赤弾",
        "敵弾",
        "弾消し",
        "エネルギー",
        "耐久",
        "シールド",
        "無敵",
        "ダッシュ",
        "スキル",
        "奥義",
        "ドローン",
        "連鎖",
        "分裂",
        "跳弾",
        "過熱",
        "防御",
        "プリズム",
        "コロナ",
        "重力",
        "特異点",
        "回復",
    };

    private static readonly SectorInfo[] Sectors =
    {
        new("sector.0.name", "sector.0.trait", Cyan),
        new("sector.1.name", "sector.1.trait", new Color(0.16f, 0.86f, 0.92f)),
        new("sector.2.name", "sector.2.trait", Jade),
        new("sector.3.name", "sector.3.trait", Violet),
        new("sector.4.name", "sector.4.trait", Rose),
    };

    private const string MetaSavePath = "user://astra_fracture_meta.cfg";

    private static readonly MetaUpgradeDef[] MetaUpgrades =
    {
        new(MetaUpgradeId.HullPlating, "meta.hull.title", "meta.hull.body", Rose, 7, 95, 34),
        new(MetaUpgradeId.ReactorSeed, "meta.energy.title", "meta.energy.body", Cyan, 6, 105, 38),
        new(MetaUpgradeId.FocusLens, "meta.weapon.title", "meta.weapon.body", Gold, 6, 120, 42),
        new(MetaUpgradeId.DriftEngine, "meta.engine.title", "meta.engine.body", Violet, 6, 95, 36),
        new(MetaUpgradeId.SalvageRig, "meta.salvage.title", "meta.salvage.body", Jade, 5, 145, 54),
        new(MetaUpgradeId.StarterChart, "meta.chart.title", "meta.chart.body", new Color(0.52f, 0.76f, 0.94f), 3, 210, 86),
        new(MetaUpgradeId.RepairProtocol, "meta.repair.title", "meta.repair.body", new Color(0.08f, 0.86f, 0.66f), 5, 125, 46),
        new(MetaUpgradeId.AegisMatrix, "meta.aegis.title", "meta.aegis.body", new Color(0.38f, 0.82f, 0.94f), 4, 165, 66),
        new(MetaUpgradeId.NovaCatalyst, "meta.nova.title", "meta.nova.body", new Color(1.0f, 0.48f, 0.14f), 5, 145, 54),
        new(MetaUpgradeId.DroneDock, "meta.drone.title", "meta.drone.body", PickupBlue, 4, 175, 70),
        new(MetaUpgradeId.PolarityTuner, "meta.tuner.title", "meta.tuner.body", PolarityAmber, 4, 150, 62),
        new(MetaUpgradeId.DeepSurvey, "meta.survey.title", "meta.survey.body", new Color(0.74f, 0.44f, 0.92f), 4, 185, 78),
    };

    private static readonly Dictionary<string, LocalizedText> Texts = new()
    {
        ["wake"] = new("WAKE", "觉醒"),
        ["choir.core.event"] = new("THE CHOIR CORE", "合唱核心"),
        ["wave.intro"] = new("WAVE {0}", "第 {0} 波"),
        ["sector.enter"] = new("SECTOR {0}: {1}", "第 {0} 章：{1}"),
        ["sector.cleared"] = new("{0} CLEARED", "通过：{0}"),
        ["sector.0.name"] = new("Lumen Shoal", "光滩星区"),
        ["sector.0.trait"] = new("Calm opening field. Learn your pilot weapon, skill, and red-fire pressure.", "第一章没有环境危险。熟悉角色武器、角色技能和红弹压力。"),
        ["sector.1.name"] = new("Glass Reef", "玻璃星区"),
        ["sector.1.trait"] = new("Warning beams cut across the arena before firing.", "光束会先显示预警线，随后造成伤害。"),
        ["sector.2.name"] = new("Verdant Grave", "孢子星区"),
        ["sector.2.trait"] = new("Gravity wells begin to open. Step out before the core collapses.", "重力场开始出现。看到圆形预警后，尽快离开核心区域。"),
        ["sector.3.name"] = new("Clock Cathedral", "时钟星区"),
        ["sector.3.trait"] = new("Time shears the arena. Faster enemies use readable dash lanes.", "时间切割战场。高速敌人会用可预判的突进路线逼你走位。"),
        ["sector.4.name"] = new("Solar Wound", "太阳裂口"),
        ["sector.4.trait"] = new("Final sector. Elite dashes, beams, and gravity fields overlap.", "最终章。精英突进、光束和重力场会组合施压。"),
        ["repair"] = new("REPAIR", "修复"),
        ["language.changed"] = new("LANGUAGE: ENGLISH", "当前语言：中文"),
        ["language.hint"] = new("LANGUAGE: ENGLISH  [L]", "中文  [L切换]"),
        ["menu.start"] = new("START GAME", "开始游戏"),
        ["menu.meta"] = new("PERMANENT UPGRADES", "永久升级"),
        ["menu.language"] = new("SWITCH LANGUAGE", "切换语言"),
        ["menu.settings"] = new("SETTINGS", "设置"),
        ["gm.unlock.label"] = new("GM", "GM"),
        ["gm.unlock.toast"] = new("All pilots and difficulties unlocked.", "所有角色与难度已解锁。"),
        ["menu.pilot"] = new("PILOT", "角色"),
        ["pilot.selector.skill"] = new("Skill", "技能"),
        ["pilot.selector.ultimate"] = new("Ultimate", "大招"),
        ["pilot.selector.selected"] = new("Selected", "已选择"),
        ["pilot.selector.locked"] = new("Locked", "未解锁"),
        ["menu.tip"] = new("Click START, press Enter, or press A. Spend Star Dust in Permanent Upgrades between runs.", "点击“开始远征”、按 Enter 或按 A。每局结束后可用星尘购买永久升级。"),
        ["boss.choir_core"] = new("CHOIR CORE", "合唱核心"),
        ["boss.sector"] = new("{0} CORE", "{0} Boss"),
        ["wave.engage"] = new("ENGAGE", "接战"),
        ["wave.pace.short.standard"] = new("PATROL", "巡逻"),
        ["wave.pace.short.swarm"] = new("SWARM", "蜂群"),
        ["wave.pace.short.elite"] = new("ELITE", "精英"),
        ["wave.pace.short.recovery"] = new("SUPPLY", "补给"),
        ["wave.pace.short.pressure"] = new("RUSH", "压迫"),
        ["wave.pace.short.boss"] = new("BOSS", "Boss"),
        ["hud.route.label"] = new("ROUTE", "路线"),
        ["hud.spawn.label"] = new("SPAWN", "刷新"),
        ["hud.spawn.boss"] = new("--", "--"),
        ["combo.pop"] = new("COMBO x{0}", "连击X{0}"),
        ["combo.value"] = new("x{0}", "连击X{0}"),
        ["wave.enemy.focus"] = new("PRIMARY: {0} - {1}", "主敌：{0} - {1}"),
        ["wave.enemy.support"] = new("SUPPORT: {0}", "辅助：{0}"),
        ["wave.enemy.new"] = new("NEW ENEMY: {0}", "新敌人：{0}"),
        ["boss.title"] = new("{0}: {1}", "{0}：{1}"),
        ["boss.preview"] = new("{0} BOSS VARIANT", "{0} 随机 Boss"),
        ["boss.choir.name"] = new("Choir Core", "合唱核心"),
        ["boss.prism.name"] = new("Prism Regent", "棱镜王庭"),
        ["boss.swarm.name"] = new("Drone Matriarch", "蜂群母舰"),
        ["boss.forge.name"] = new("Solar Forge", "日冕熔炉"),
        ["boss.rift.name"] = new("Rift Warden", "裂隙典狱"),
        ["wave.pace.standard"] = new("PATROL", "巡逻波"),
        ["wave.pace.swarm"] = new("SWARM", "蜂群波"),
        ["wave.pace.elite"] = new("ELITE HUNT", "精英狩猎"),
        ["wave.pace.recovery"] = new("SUPPLY DRIFT", "补给漂流"),
        ["wave.pace.pressure"] = new("PRESSURE RUN", "压迫连战"),
        ["wave.pace.boss"] = new("BOSS", "Boss"),
        ["flow.momentum"] = new("MOMENTUM +FOCUS +ENERGY", "连战动量：专注与能量提升"),
        ["flow.supply"] = new("SUPPLY WAVE: fewer enemies, better recovery.", "补给波：敌人较少，回复更多。"),
        ["flow.draft"] = new("BUILD DRAFT READY", "构筑选择就绪"),
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
        ["hud.xp.label"] = new("EXP", "经验"),
        ["hud.level.label"] = new("LV", "等级"),
        ["hud.cache.label"] = new("CACHE", "缓存"),
        ["hud.combo.label"] = new("COMBO", "连击"),
        ["xp.gain"] = new("+{0} EXP", "+{0} 经验"),
        ["xp.level_up"] = new("LEVEL UP", "升级"),
        ["score.cache"] = new("SCORE CACHE +{0} DUST", "战绩缓存 +{0} 星尘"),
        ["score.cache.hint"] = new("score feeds end-run dust and instant resources", "分数会转化为结算星尘和即时资源"),
        ["score.combo_break"] = new("COMBO BREAK", "连击中断"),
        ["hud.build"] = new("BUILD", "构筑"),
        ["hud.objectives"] = new("EXPEDITION GOALS", "远征目标"),
        ["hud.cyan_resonance"] = new("TACTICAL", "战术技"),
        ["hud.gold_resonance"] = new("TACTICAL", "战术技"),
        ["hud.resonance_ready"] = new("SKILL READY", "技能就绪"),
        ["hud.resonance_cooldown"] = new("COOLDOWN {0:0.0}s", "冷却 {0:0.0}秒"),
        ["hud.cruise_charge"] = new("FOCUS", "专注"),
        ["hud.assault_window"] = new("SKILL {0:0.0}s", "战术 {0:0.0}秒"),
        ["hud.controls"] = new("WASD/LS MOVE  MOUSE/RS AIM  AUTO FIRE  A/LB DASH  X/RB SKILL  Y/RT ULT  START MENU", "WASD/左摇杆移动  鼠标/右摇杆瞄准  自动射击  A/LB冲刺  X/RB技能  Y/RT大招  Start菜单"),
        ["title.loop"] = new("LOOP", "循环"),
        ["title.fighter"] = new("FIGHTER", "战机"),
        ["title.subtitle"] = new("a pilot-build arcade roguelite built in Godot C#", "Godot C# 制作的角色构筑弹幕 Roguelite"),
        ["title.body"] = new("Red fire is always danger. Each pilot has a distinct weapon, pilot skill, ultimate, and upgrade path.", "红色弹幕永远危险。每个角色都有不同武器、角色技能、大招和升级流派。"),
        ["pilot.astra.name"] = new("Astra", "星棱"),
        ["pilot.astra.body"] = new("Balanced prism bolts. Skill focuses a refraction burst.", "均衡棱镜弹。角色技能释放折射聚焦。"),
        ["pilot.astra.weapon"] = new("Prism Bolts", "棱镜连射"),
        ["pilot.vesper.name"] = new("Vesper", "暮轨"),
        ["pilot.vesper.body"] = new("Slow heavy rail shots. Skill locks a cutting line.", "低频重炮。角色技能锁定一条切割射线。"),
        ["pilot.vesper.weapon"] = new("Rail Lance", "轨道长枪"),
        ["pilot.kairo.name"] = new("Kairo", "环序"),
        ["pilot.kairo.body"] = new("Starts with drones. Skill orders synchronized swarm fire.", "开局拥有无人机。角色技能指挥蜂群同步集火。"),
        ["pilot.kairo.weapon"] = new("Drone Net", "无人机网"),
        ["pilot.sol.name"] = new("Sol", "日冕"),
        ["pilot.sol.body"] = new("Wide scatter fire. Skill opens a protective corona field.", "宽角散射。角色技能展开护身日冕场。"),
        ["pilot.sol.weapon"] = new("Corona Scatter", "日冕散射"),
        ["pilot.unlock.free"] = new("Unlocked", "已解锁"),
        ["pilot.unlock.wave8"] = new("Reach wave 8", "到达第 8 波解锁"),
        ["pilot.unlock.wave16"] = new("Reach wave 16", "到达第 16 波解锁"),
        ["pilot.unlock.wave24"] = new("Reach wave 24", "到达第 24 波解锁"),
        ["title.start"] = new("ENTER / CLICK / A", "ENTER / 点击 / A 开始"),
        ["title.won_once"] = new("Choir Core fractured once. It remembers.", "合唱核心曾被击碎。它记得你。"),
        ["upgrade.title"] = new("CHOOSE A BUILD UPGRADE", "选择一个构筑升级"),
        ["upgrade.hint"] = new("1 / 2 / 3 or click. Gamepad: LS/D-Pad choose, A select, X reroll.", "按 1 / 2 / 3 或点击。手柄：左摇杆/方向键选择，A确认，X重抽。"),
        ["choice.instant"] = new("Instant", "立即生效"),
        ["choice.tactic"] = new("Tactic", "战术"),
        ["choice.risk"] = new("Risk", "风险"),
        ["choice.contract"] = new("Challenge", "挑战"),
        ["choice.meta"] = new("Map", "路线"),
        ["choice.capstone"] = new("Max Rank", "满级"),
        ["capstone.chain"] = new("MAX RANK: chain jumps farther and ends with a shard burst.", "满级：连锁跳得更远，最后爆出碎片。"),
        ["capstone.fractal"] = new("MAX RANK: split shots create a second split.", "满级：分裂弹会再分裂一次。"),
        ["capstone.astra.refraction"] = new("MAX RANK: Astra gains permanent side shots.", "满级：星棱获得常驻侧向子弹。"),
        ["capstone.astra.wake"] = new("MAX RANK: Astra skill fires stronger echo shots.", "满级：星棱技能追加更强的回声弹。"),
        ["capstone.astra.nova"] = new("MAX RANK: Astra skill reaches farther and ultimate costs less.", "满级：星棱技能范围更远，大招耗能更低。"),
        ["capstone.astra.twin"] = new("MAX RANK: Astra skill fires a cross beam.", "满级：星棱技能会打出十字光束。"),
        ["capstone.vesper.charge"] = new("MAX RANK: Vesper shots leave a delayed spear.", "满级：暮轨射击留下延迟星矛。"),
        ["capstone.vesper.fork"] = new("MAX RANK: side beams form a steady triangle.", "满级：侧向光束形成稳定三角火力。"),
        ["capstone.vesper.judgment"] = new("MAX RANK: Vesper skill pierces Boss guard and becomes wider.", "满级：暮轨技能穿透 Boss 护盾，并且范围更宽。"),
        ["capstone.vesper.sever"] = new("MAX RANK: side beams cross into a wider pattern.", "满级：侧向光束交叉，覆盖更宽。"),
        ["capstone.kairo.bay"] = new("MAX RANK: drones clear bullets around themselves.", "满级：无人机会清除自身周围红弹。"),
        ["capstone.kairo.sync"] = new("MAX RANK: drone shots gain an extra lock-on stream.", "满级：无人机追加一组锁定弹流。"),
        ["capstone.kairo.override"] = new("MAX RANK: skill refunds energy and drone damage increases.", "满级：技能返还能量，无人机伤害提高。"),
        ["capstone.kairo.relay"] = new("MAX RANK: skill refunds more energy and chain shots improve.", "满级：技能返还更多能量，连锁射击更强。"),
        ["capstone.sol.bloom"] = new("MAX RANK: spread shots cover almost all directions.", "满级：散射弹几乎覆盖全方向。"),
        ["capstone.sol.forge"] = new("MAX RANK: max energy increases and energy refills.", "满级：能量上限提高，并回复大量能量。"),
        ["capstone.sol.flare"] = new("MAX RANK: skill pulses jump through nearby enemies.", "满级：技能脉冲会在附近敌人之间连锁。"),
        ["capstone.sol.mantle"] = new("MAX RANK: skill gives a short shield.", "满级：使用技能会获得短暂护盾。"),
        ["end.victory.title"] = new("CORE FRACTURED", "核心已碎裂"),
        ["end.defeat.title"] = new("SIGNAL LOST", "信号丢失"),
        ["end.wave"] = new("REACHED WAVE {0}/{1}", "抵达波次 {0}/{1}"),
        ["end.victory.body"] = new("The starfield exhales. Your pattern survives.", "你击败了全部 Boss，完成了这次航行。"),
        ["end.defeat.body"] = new("The Choir rewinds the arena. Tune again.", "这次失败了。调整升级选择，再试一次。"),
        ["end.restart"] = new("ENTER / CLICK / A TO RESTART", "ENTER / 点击 / A 重新开始"),
        ["end.reward"] = new("STAR DUST +{0}   REACHED WAVE {1}/40", "获得星尘 +{0}   到达波次 {1}/40"),
        ["end.objective_bonus"] = new("GOAL BONUS +{0}", "目标奖励 +{0}"),
        ["end.score_bonus"] = new("CACHE DUST +{0}", "缓存星尘 +{0}"),
        ["end.meta_hint"] = new("B/Esc returns to title. Y opens Permanent Upgrades.", "B/Esc 返回主界面。Y 打开永久升级。"),
        ["objective.complete"] = new("GOAL DONE +{0} DUST", "目标完成 +{0} 星尘"),
        ["objective.clean_wave"] = new("NO DAMAGE WAVE +ENERGY", "无伤通过本波 +能量"),
        ["meta.title"] = new("STAR VAULT", "星尘工坊"),
        ["meta.subtitle"] = new("Permanent upgrades are a long route across many expeditions.", "永久升级需要多次远征逐步推进。"),
        ["meta.dust"] = new("Star Dust", "星尘"),
        ["meta.wallet"] = new("STAR DUST {0}", "星尘 {0}"),
        ["meta.best"] = new("BEST WAVE {0}/40   RUNS {2}", "最高波次 {0}/40   出航 {2}"),
        ["meta.open_hint"] = new("Press U or click Permanent Upgrades.", "按 U 或点击“永久升级”。"),
        ["meta.buy_hint"] = new("Click or use LS/D-Pad + A to buy. Keys 1-9 buy nodes. B/Esc returns.", "点击或用左摇杆/方向键 + A 购买。1-9 可快捷购买。B/Esc 返回。"),
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
        ["guide.subtitle"] = new("Controls, EXP, combo, upgrades, and progress rules are listed here.", "这里列出操作、经验、连击、升级和进程规则。"),
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
        ["meta.tuner.title"] = new("Tactical Console", "战术控制台"),
        ["meta.tuner.body"] = new("Pilot skills cool down faster and focus gains return more energy.", "角色战术技冷却更快，专注收益带来更多能量。"),
        ["meta.survey.title"] = new("Deep Survey", "深空测绘"),
        ["meta.survey.body"] = new("Earn a modest Star Dust bonus and read the opening waves more safely.", "略微提高星尘收益，并让开局节奏更稳。"),
        ["rank"] = new("Rank {0}", "等级 {0}"),
        ["upgrade.prism.title"] = new("Prism Array", "多重射击"),
        ["upgrade.prism.body"] = new("Each shot fires one extra bullet.", "每次射击多一发子弹。"),
        ["upgrade.rail.title"] = new("Rail Heart", "强力核心"),
        ["upgrade.rail.body"] = new("Weapon damage increases and fire rate improves slightly.", "提高武器伤害，并略微提高射速。"),
        ["upgrade.coolant.title"] = new("Coolant Lattice", "冷却装置"),
        ["upgrade.coolant.body"] = new("Fire faster and gain more max energy.", "射击更快，能量上限更高。"),
        ["upgrade.kinetic.title"] = new("Kinetic Bloom", "机动强化"),
        ["upgrade.kinetic.body"] = new("Move faster and dash farther.", "移动更快，冲刺更远。"),
        ["upgrade.gravity.title"] = new("Gravity Well", "拾取范围"),
        ["upgrade.gravity.body"] = new("Pickups fly to you from farther away. Enemies slow slightly.", "拾取范围更远，敌人略微变慢。"),
        ["upgrade.vital.title"] = new("Vital Shell", "生命护盾"),
        ["upgrade.vital.body"] = new("Increase max hull and heal now.", "提高生命上限，并立即回血。"),
        ["upgrade.leech.title"] = new("Repair Seed", "修复掉落"),
        ["upgrade.leech.body"] = new("Kills can drop repair pickups.", "击杀有概率掉落修复。"),
        ["upgrade.wisp.title"] = new("Moon Wisp", "自动浮游炮"),
        ["upgrade.wisp.body"] = new("Add an orbiting drone that auto-fires at nearby enemies.", "增加一个环绕无人机，自动攻击附近敌人。"),
        ["upgrade.rift.title"] = new("Rift Needle", "穿透弹"),
        ["upgrade.rift.body"] = new("Shots become faster piercing bullets.", "子弹变成更快的穿透弹。"),
        ["upgrade.mirror.title"] = new("Mirror Skin", "减伤装甲"),
        ["upgrade.mirror.body"] = new("Take less damage from hits.", "降低受到的伤害。"),
        ["upgrade.nova.title"] = new("Ultimate Capacitor", "大招强化"),
        ["upgrade.nova.body"] = new("Ultimate costs less energy and max energy increases.", "大招耗能降低，能量上限提高。"),
        ["upgrade.storm.title"] = new("Tactical Overdrive", "战术超载"),
        ["upgrade.storm.body"] = new("Pilot skill cooldown is shorter and fires extra shots.", "角色技能冷却缩短，并追加反击弹。"),
        ["upgrade.comet.title"] = new("Comet Trail", "冲刺强化"),
        ["upgrade.comet.body"] = new("Dash deals more damage and clears more bullets.", "冲刺伤害更高，并清除更多红弹。"),
        ["upgrade.aegis.title"] = new("Aegis Bloom", "自动回血"),
        ["upgrade.aegis.body"] = new("Slowly regenerates hull while you avoid damage.", "一段时间不受伤会缓慢回血。"),
        ["upgrade.echo.title"] = new("Quantum Echo", "额外射击"),
        ["upgrade.echo.body"] = new("Weapon shots can fire an extra piercing shot.", "武器射击有概率追加穿透弹。"),
        ["upgrade.chain.title"] = new("Chain Relay", "连锁中继"),
        ["upgrade.chain.body"] = new("Weapon hits can jump damage to nearby enemies.", "武器命中可把伤害跳到附近敌人。"),
        ["upgrade.fractal.title"] = new("Fractal Split", "分裂棱片"),
        ["upgrade.fractal.body"] = new("Kills create small split shots.", "击杀会生成小型分裂弹。"),
        ["upgrade.solar.title"] = new("Flow Core", "流派核心"),
        ["upgrade.solar.body"] = new("Pilot skills deal more damage.", "角色技能伤害更高。"),
        ["upgrade.repair.title"] = new("Emergency Repair", "紧急维修"),
        ["upgrade.repair.body"] = new("Repair hull immediately and gain a little max hull.", "立即回复生命，并少量提高最大生命。"),
        ["upgrade.overdrive.title"] = new("One-Wave Overdrive", "单波过载"),
        ["upgrade.overdrive.body"] = new("Next wave: much higher damage. Also gain energy now.", "下一波伤害大幅提高，并立即获得能量。"),
        ["upgrade.glass.title"] = new("Glass Cannon", "玻璃大炮"),
        ["upgrade.glass.body"] = new("Permanent damage up, but max hull goes down.", "永久提高伤害，但降低最大生命。"),
        ["upgrade.bounty.title"] = new("Risk Reward", "高风险奖励"),
        ["upgrade.bounty.body"] = new("Next wave has more enemies and better drops.", "下一波敌人更多，掉落更好。"),
        ["upgrade.transmute.title"] = new("Bullet Transmute", "弹幕转化"),
        ["upgrade.transmute.body"] = new("Clear enemy bullets now and gain energy.", "立刻清除敌方子弹，并获得能量。"),
        ["upgrade.map.title"] = new("Harmonic Map", "升级地图"),
        ["upgrade.map.body"] = new("Gain one extra reroll on future upgrade screens.", "之后的升级界面多一次重抽机会。"),
        ["upgrade.astra.refraction.title"] = new("Refraction Lattice", "折射阵列"),
        ["upgrade.astra.refraction.body"] = new("Astra gains extra prism lanes and a denser Focus Prism salvo.", "星棱增加额外弹道，空格棱镜聚焦也更密。"),
        ["upgrade.astra.wake.title"] = new("Prism Wake", "棱光余波"),
        ["upgrade.astra.wake.body"] = new("Prism shots hit harder, cycle faster, and extend Focus Prism.", "棱镜弹伤害提高、射击更快，并延长棱镜聚焦。"),
        ["upgrade.vesper.charge.title"] = new("Capacitor Spine", "蓄能脊柱"),
        ["upgrade.vesper.charge.body"] = new("Rail lance damage rises. Skill lock-on cuts deeper.", "轨道长枪伤害提高，角色技能锁定射线更锋利。"),
        ["upgrade.vesper.fork.title"] = new("Split Rail", "分裂轨道"),
        ["upgrade.vesper.fork.body"] = new("Rail shots and skill lock add narrow side lances.", "轨道炮和角色技能锁定追加两侧副枪线。"),
        ["upgrade.kairo.bay.title"] = new("Drone Bay", "无人机舱"),
        ["upgrade.kairo.bay.body"] = new("Kairo launches more orbiting drones. Skill commands a larger swarm.", "环序增加更多环绕无人机，角色技能指挥更大蜂群。"),
        ["upgrade.kairo.sync.title"] = new("Swarm Sync", "蜂群同步"),
        ["upgrade.kairo.sync.body"] = new("Drones fire faster and tactical volleys synchronize harder.", "无人机射击更快，空格同步齐射更强。"),
        ["upgrade.sol.bloom.title"] = new("Corona Bloom", "日冕绽放"),
        ["upgrade.sol.bloom.body"] = new("Sol scatter fire gains more rays. Skill corona covers more space.", "日冕散射弹数量增加，角色技能日冕场覆盖更大。"),
        ["upgrade.sol.forge.title"] = new("Solar Forge", "太阳熔炉"),
        ["upgrade.sol.forge.body"] = new("More energy, cheaper ultimates, and hotter corona skills.", "能量更多，大招更便宜，并强化日冕战术技。"),
        ["upgrade.astra.nova.title"] = new("Nova Bloom", "星爆棱镜"),
        ["upgrade.astra.nova.body"] = new("Prism Nova clears wider, and Focus Prism cuts more space.", "棱镜星爆范围更大，棱镜聚焦清场更强。"),
        ["upgrade.astra.twin.title"] = new("Twin Refraction", "双相折射"),
        ["upgrade.astra.twin.body"] = new("Prism Nova and Focus Prism echo with a second refraction ring.", "棱镜星爆和空格聚焦追加第二圈折射。"),
        ["upgrade.vesper.judgment.title"] = new("Judgment Coil", "裁决线圈"),
        ["upgrade.vesper.judgment.body"] = new("Rail Judgment and skill lock become wider, cheaper, and more lethal.", "轨道裁决和角色技能锁定更宽、更便宜，伤害更高。"),
        ["upgrade.vesper.sever.title"] = new("Sever Line", "裂轨余震"),
        ["upgrade.vesper.sever.body"] = new("Rail Judgment and skill lock create parallel aftershock beams.", "轨道裁决和角色技能锁定会生成平行余震光束。"),
        ["upgrade.kairo.override.title"] = new("Override Matrix", "覆写矩阵"),
        ["upgrade.kairo.override.body"] = new("Swarm Override adds command bursts to the pilot skill and stronger shots.", "蜂群覆写为角色技能增加指令齐射，弹幕更强。"),
        ["upgrade.kairo.relay.title"] = new("Relay Protocol", "接力协议"),
        ["upgrade.kairo.relay.body"] = new("Swarm relays refund energy and make skill commands chain shots.", "蜂群中继会回复能量，并让角色技能指令连锁更多射击。"),
        ["upgrade.sol.flare.title"] = new("Flare Core", "耀斑核心"),
        ["upgrade.sol.flare.body"] = new("Corona Flare and pilot skill burn brighter and clear a wider field.", "日冕耀斑和角色技能日冕场更亮，清场更强。"),
        ["upgrade.sol.mantle.title"] = new("Radiant Mantle", "光冕护层"),
        ["upgrade.sol.mantle.body"] = new("Radiant mantle adds hull, invulnerability, and skill recovery.", "提高生命、无敌时间，并强化角色技能回复。"),
        ["upgrade.unknown.title"] = new("Unknown", "未知升级"),
        ["upgrade.unknown.body"] = new("Effect not shown.", "效果未显示。"),
    };

    private readonly RandomNumberGenerator _rng = new();
    private readonly List<Enemy> _enemies = new();
    private readonly List<Shot> _shots = new();
    private readonly List<Pickup> _pickups = new();
    private readonly List<Particle> _particles = new();
    private readonly List<DroneCommandCue> _droneCommandCues = new();
    private readonly List<Shockwave> _shockwaves = new();
    private readonly List<DamageText> _damageTexts = new();
    private readonly List<HazardLine> _hazards = new();
    private readonly List<HazardField> _hazardFields = new();
    private readonly List<Star> _stars = new();
    private readonly List<Nebula> _nebulas = new();
    private readonly List<UpgradeCard> _upgradeChoices = new();
    private readonly List<UpgradeId> _upgradeOrder = new();
    private readonly List<RunObjective> _runObjectives = new();
    private readonly List<SfxVoice> _voices = new();
    private readonly Queue<TextCue> _centerTextQueue = new();
    private readonly Vector2[] _playerTrail = new Vector2[PlayerTrailCapacity];
    private readonly OrbiterVisual[] _orbiterVisuals = CreateOrbiterVisuals();
    private readonly Vector2[][][] _polygonScratch = CreatePolygonScratch(0);
    private readonly Vector2[][][] _closedPolygonScratch = CreatePolygonScratch(1);
    private readonly Queue<PendingSpawn> _pendingSpawns = new();
    private readonly Dictionary<UpgradeId, int> _upgradeRanks = new();
    private readonly Dictionary<MetaUpgradeId, int> _metaRanks = new();
    private readonly Dictionary<PilotKind, int> _pilotRuns = new();
    private readonly List<Enemy>[] _enemyGrid = CreateEnemyGrid();
    private readonly Stack<Enemy> _enemyPool = new();
    private readonly Stack<Shot> _shotPool = new();
    private readonly Stack<Pickup> _pickupPool = new();
    private readonly Stack<Particle> _particlePool = new();
    private readonly Stack<DamageText> _damageTextPool = new();
    private int _activeEnemyBullets;
    private float _visualPressure;
    private float _frameRatePressure;
    private Vector2 _drawShakeOffset = Vector2.Zero;
    private int _polygonScratchCursor;
    private int _closedPolygonScratchCursor;
    private float _centerTextQueueTimer;

    private GameMode _mode = GameMode.Title;
    private GameMode _settingsReturnMode = GameMode.Title;
    private PilotKind _selectedPilot = PilotKind.Astra;
    private PilotKind _runPilot = PilotKind.Astra;
    private Vector2 _playerPos = ScreenCenter;
    private Vector2 _playerVel = Vector2.Zero;
    private Vector2 _aimDir = Vector2.Right;
    private Vector2 _lastMousePos = ScreenCenter;
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
    private float _hudSpawnValue = 0.1f;
    private float _hudSpawnTrail = 0.1f;
    private float _fireTimer;
    private float _dashTimer;
    private float _dashCooldown;
    private float _invulnTimer;
    private float _polarityCooldown;
    private float _polarityCooldownMax = PolaritySwitchCooldownBase;
    private float _ultimateCooldown;
    private float _polarityDenyTextCooldown;
    private int _playerPolarity;
    private int _wave;
    private int _score;
    private int _combo;
    private float _comboTimer;
    private int _comboTier;
    private float _comboTierPulse;
    private int _scoreCacheLevel;
    private int _nextScoreCache;
    private int _runScoreBonusDust;
    private int _lastScoreBonusDust;
    private PilotKind? _lastUnlockedPilot;
    private float _scoreCachePulse;
    private int _runLevel = 1;
    private int _xp;
    private int _xpToNext = XpBase;
    private int _queuedLevelUps;
    private float _xpPulse;
    private float _waveClearTimer;
    private float _waveIntelPulse;
    private float _time;
    private float _shake;
    private float _flash;
    private float _slowMo = 1.0f;
    private float _playerTrailTimer;
    private float _spawnDirector;
    private float _waveSpawnTimer;
    private float _waveSpawnInterval;
    private int _waveNextSpawnCount;
    private int _waveBudget;
    private float _waveProgressBudget;
    private float _waveProgressSpent;
    private int _waveSpawnIndex;
    private int _waveEventMask;
    private float _waveRewardBoost = 1.0f;
    private float _bossPatternTimer;
    private float _sectorHazardTimer;
    private WavePaceKind _currentWavePace = WavePaceKind.Standard;
    private int _combatChain;
    private BossArchetype _lastBossArchetype = BossArchetype.Choir;
    private bool _hasLastBossArchetype;
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
    private DisplayResolutionPreset _resolutionPreset = DisplayResolutionPreset.R1920x1080;
    private VisualQuality _visualQuality = VisualQuality.High;
    private GameDifficulty _selectedDifficulty = GameDifficulty.Cruise;
    private GameDifficulty _runDifficulty = GameDifficulty.Cruise;
    private float _musicVolume = 0.52f;
    private float _sfxVolume = 0.56f;
    private Font? _uiFont;
    private bool _usingGamepad;
    private Rect2 _gamepadFocusRect;
    private bool _gamepadFocusVisible;
    private int _gamepadTitleIndex = TitlePilotFocusStart;
    private int _gamepadPilotIndex;
    private int _gamepadUpgradeIndex;
    private int _gamepadMetaIndex;
    private int _gamepadSettingsIndex;
    private int _guidePage;
    private float _gamepadNavCooldown;
    private int _gamepadLastNavX;
    private int _gamepadLastNavY;
    private float _runTimer;
    private readonly List<float>[] _clearTimeRecordsByDifficulty = CreateClearRecordLists();
    private readonly bool[] _difficultyTestUnlocks = new bool[DifficultyCount];

    private int _starDust;
    private int _lifetimeDust;
    private int _bestScore;
    private int _bestWave;
    private int _runsCompleted;
    private int _careerKills;
    private int _careerPickups;
    private int _careerAbsorbs;
    private int _careerBestCombo;
    private int _careerBossKills;
    private int _careerPerfectWaves;
    private int _lastDustEarned;
    private int _lastRunWave;
    private int _lastObjectiveBonusDust;
    private float _lastClearTime;
    private int _lastClearRecordRank;
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
    private int _chainRelay;
    private int _fractalSplit;
    private int _pulseMagazine;
    private int _executionMark;
    private int _stasisField;
    private int _magnetizedCore;
    private int _ricochetMatrix;
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
    private int _nyxOrbit;
    private int _nyxSingularity;
    private int _nyxEventHorizon;
    private int _nyxGravityCantor;
    private int _rookBulwarkCore;
    private int _rookSiegeBattery;
    private int _rookAegisRelay;
    private int _rookCitadelProtocol;
    private int _lyraResonanceChord;
    private int _lyraTempoBloom;
    private int _lyraHarmonicCascade;
    private int _lyraEncoreField;
    private int _orionCometSpear;
    private int _orionDeadeyeMark;
    private int _orionStarfallQuiver;
    private int _orionPerihelionVector;
    private int _lyraBeat;
    private int _draftBiasWeapon;
    private int _draftBiasDefense;
    private int _draftBiasSkill;
    private int _draftBiasFlow;
    private int _draftBiasEconomy;
    private int _buildMilestoneMask;
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
    private bool _lastConfirm;
    private bool _lastCancel;
    private bool _lastPause;
    private bool _lastSettingsShortcut;
    private bool _lastTitleLeft;
    private bool _lastTitleRight;
    private bool _lastSettingsLeft;
    private bool _lastSettingsRight;

    private AudioStreamPlayer? _musicPlayer;
    private AudioStreamGeneratorPlayback? _musicPlayback;
    private Texture2D? _titleLogoCn;
    private Texture2D? _titleLogoEn;
    private Texture2D? _supportDroneTexture;
    private Texture2D? _kairoDroneTexture;
    private Texture2D? _kairoCommandTexture;
    private Texture2D? _enemyBulletTexture;
    private Rect2 _supportDroneRegion;
    private Rect2 _kairoDroneRegion;
    private Rect2 _kairoCommandRegion;
    private Rect2 _enemyBulletRegion;
    private readonly Dictionary<PilotKind, Texture2D> _pilotTextures = new();
    private readonly Dictionary<PilotKind, Rect2> _pilotTextureRegions = new();
    private readonly Dictionary<PilotKind, Texture2D> _fighterBulletTextures = new();
    private readonly Dictionary<PilotKind, Rect2> _fighterBulletTextureRegions = new();
    private readonly Dictionary<EnemyKind, Texture2D> _enemyTextures = new();
    private readonly Dictionary<EnemyKind, Texture2D> _eliteEnemyTextures = new();
    private readonly Dictionary<BossArchetype, Texture2D> _bossTextures = new();
    private readonly Dictionary<EnemyKind, Rect2> _enemyTextureRegions = new();
    private readonly Dictionary<EnemyKind, Rect2> _eliteEnemyTextureRegions = new();
    private readonly Dictionary<BossArchetype, Rect2> _bossTextureRegions = new();

    private static List<Enemy>[] CreateEnemyGrid()
    {
        List<Enemy>[] grid = new List<Enemy>[EnemyGridCellCount];
        for (int i = 0; i < grid.Length; i++)
        {
            grid[i] = new List<Enemy>(8);
        }
        return grid;
    }

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
        LoadTitleLogos();
        LoadDroneTextures();
        LoadBulletTextures();
        LoadPilotTextures();
        LoadEnemyTextures();
        GenerateBackdrop();
        SetupAudio();
        LoadMetaProgress();
        ApplyWindowTitle();
        ResetTitle();
        SetProcess(true);
    }

    public override void _ExitTree()
    {
        _voices.Clear();
        _supportDroneTexture = null;
        _kairoDroneTexture = null;
        _kairoCommandTexture = null;
        _enemyBulletTexture = null;
        _pilotTextures.Clear();
        _pilotTextureRegions.Clear();
        _fighterBulletTextures.Clear();
        _fighterBulletTextureRegions.Clear();
        _enemyTextures.Clear();
        _eliteEnemyTextures.Clear();
        _bossTextures.Clear();
        _enemyTextureRegions.Clear();
        _eliteEnemyTextureRegions.Clear();
        _bossTextureRegions.Clear();
        _musicPlayback = null;
    }

    private void LoadTitleLogos()
    {
        _titleLogoCn = GD.Load<Texture2D>("res://Assets/Logo/Logo_Cn.png");
        _titleLogoEn = GD.Load<Texture2D>("res://Assets/Logo/Logo_En.png");
    }

    private void LoadDroneTextures()
    {
        _supportDroneTexture = LoadFirstTextureInDirectory("res://Assets/DroneArt/01_PlayerSupportDrone");
        _kairoDroneTexture = LoadFirstTextureInDirectory("res://Assets/DroneArt/02_KairoSwarmDrone") ?? _supportDroneTexture;
        _kairoCommandTexture = LoadFirstTextureInDirectory("res://Assets/DroneArt/03_KairoCommandBurst");

        _supportDroneRegion = TextureRegionOrEmpty(_supportDroneTexture);
        _kairoDroneRegion = TextureRegionOrEmpty(_kairoDroneTexture);
        _kairoCommandRegion = TextureRegionOrEmpty(_kairoCommandTexture);
    }

    private void LoadBulletTextures()
    {
        _enemyBulletTexture = LoadFirstTextureInDirectory("res://Assets/EnemyBullet");
        _enemyBulletRegion = TextureRegionOrEmpty(_enemyBulletTexture);
        _fighterBulletTextures.Clear();
        _fighterBulletTextureRegions.Clear();

        for (int i = 0; i < PilotCount(); i++)
        {
            PilotKind pilot = PilotFromIndex(i);
            Texture2D? texture = LoadFirstTextureInDirectory($"res://Assets/FighterBullets/{pilot}");
            if (texture == null)
            {
                continue;
            }

            _fighterBulletTextures[pilot] = texture;
            _fighterBulletTextureRegions[pilot] = VisibleTextureRegion(texture);
        }
    }

    private static Rect2 TextureRegionOrEmpty(Texture2D? texture)
    {
        return texture == null ? new Rect2(Vector2.Zero, Vector2.Zero) : VisibleTextureRegion(texture);
    }

    private void LoadPilotTextures()
    {
        _pilotTextures.Clear();
        _pilotTextureRegions.Clear();
        for (int i = 0; i < PilotCount(); i++)
        {
            PilotKind pilot = PilotFromIndex(i);
            Texture2D? texture = LoadPilotTexture(pilot);
            if (texture != null)
            {
                _pilotTextures[pilot] = texture;
                _pilotTextureRegions[pilot] = VisibleTextureRegion(texture);
            }
        }
    }

    private static Texture2D? LoadPilotTexture(PilotKind pilot)
    {
        return LoadFirstTextureInDirectory($"res://Assets/FighterArt/{pilot}");
    }

    private void LoadEnemyTextures()
    {
        _enemyTextures.Clear();
        _eliteEnemyTextures.Clear();
        _bossTextures.Clear();
        _enemyTextureRegions.Clear();
        _eliteEnemyTextureRegions.Clear();
        _bossTextureRegions.Clear();

        foreach (EnemyKind kind in Enum.GetValues(typeof(EnemyKind)))
        {
            if (kind == EnemyKind.Boss)
            {
                continue;
            }

            Texture2D? normal = LoadFirstTextureInDirectory(EnemyArtDirectory(kind, false));
            if (normal != null)
            {
                _enemyTextures[kind] = normal;
                _enemyTextureRegions[kind] = VisibleTextureRegion(normal);
            }

            Texture2D? elite = LoadFirstTextureInDirectory(EnemyArtDirectory(kind, true));
            if (elite != null)
            {
                _eliteEnemyTextures[kind] = elite;
                _eliteEnemyTextureRegions[kind] = VisibleTextureRegion(elite);
            }
        }

        foreach (BossArchetype archetype in Enum.GetValues(typeof(BossArchetype)))
        {
            Texture2D? texture = LoadFirstTextureInDirectory(BossArtDirectory(archetype));
            if (texture != null)
            {
                _bossTextures[archetype] = texture;
                _bossTextureRegions[archetype] = VisibleTextureRegion(texture);
            }
        }
    }

    private static Texture2D? LoadFirstTextureInDirectory(string directory)
    {
        DirAccess? dir = DirAccess.Open(directory);
        if (dir == null)
        {
            return null;
        }

        string[] files = dir.GetFiles();
        Array.Sort(files, StringComparer.OrdinalIgnoreCase);
        foreach (string file in files)
        {
            if (!IsTextureAsset(file))
            {
                continue;
            }

            Texture2D? texture = GD.Load<Texture2D>($"{directory}/{file}");
            if (texture != null)
            {
                return texture;
            }
        }

        return null;
    }

    private static string EnemyArtDirectory(EnemyKind kind, bool elite)
    {
        string root = elite ? "res://Assets/EnemyArt/Elite" : "res://Assets/EnemyArt/Normal";
        return kind switch
        {
            EnemyKind.Chaser => $"{root}/LV01_Basic/LV01_Chaser{(elite ? "_Elite" : string.Empty)}",
            EnemyKind.Weaver => $"{root}/LV01_Basic/LV01_Weaver{(elite ? "_Elite" : string.Empty)}",
            EnemyKind.Turret => $"{root}/LV02_EarlyMechanic/LV02_Turret{(elite ? "_Elite" : string.Empty)}",
            EnemyKind.Drifter => $"{root}/LV02_EarlyMechanic/LV02_Drifter{(elite ? "_Elite" : string.Empty)}",
            EnemyKind.Mine => $"{root}/LV02_EarlyMechanic/LV02_Mine{(elite ? "_Elite" : string.Empty)}",
            EnemyKind.Lance => $"{root}/LV03_MidThreat/LV03_Lance{(elite ? "_Elite" : string.Empty)}",
            EnemyKind.Splitter => $"{root}/LV03_MidThreat/LV03_Splitter{(elite ? "_Elite" : string.Empty)}",
            EnemyKind.Siren => $"{root}/LV03_MidThreat/LV03_Siren{(elite ? "_Elite" : string.Empty)}",
            EnemyKind.Shard => $"{root}/LV04_LateThreat/LV04_Shard{(elite ? "_Elite" : string.Empty)}",
            EnemyKind.Harrier => $"{root}/LV04_LateThreat/LV04_Harrier{(elite ? "_Elite" : string.Empty)}",
            EnemyKind.Bulwark => $"{root}/LV05_HeavyAnchor/LV05_Bulwark{(elite ? "_Elite" : string.Empty)}",
            EnemyKind.Warden => $"{root}/LV05_HeavyAnchor/LV05_Warden{(elite ? "_Elite" : string.Empty)}",
            _ => root,
        };
    }

    private static string BossArtDirectory(BossArchetype archetype)
    {
        return archetype switch
        {
            BossArchetype.Choir => "res://Assets/EnemyArt/Boss/BOSS_LV01_Choir",
            BossArchetype.Prism => "res://Assets/EnemyArt/Boss/BOSS_LV01_Prism",
            BossArchetype.Swarm => "res://Assets/EnemyArt/Boss/BOSS_LV02_Swarm",
            BossArchetype.Forge => "res://Assets/EnemyArt/Boss/BOSS_LV02_Forge",
            BossArchetype.Rift => "res://Assets/EnemyArt/Boss/BOSS_LV03_Rift",
            BossArchetype.Mirror => "res://Assets/EnemyArt/Boss/BOSS_LV03_Mirror",
            BossArchetype.Tempest => "res://Assets/EnemyArt/Boss/BOSS_LV04_Tempest",
            BossArchetype.Bastion => "res://Assets/EnemyArt/Boss/BOSS_LV04_Bastion",
            BossArchetype.Serpent => "res://Assets/EnemyArt/Boss/BOSS_LV05_Serpent",
            BossArchetype.Oracle => "res://Assets/EnemyArt/Boss/BOSS_LV05_Oracle",
            _ => "res://Assets/EnemyArt/Boss",
        };
    }

    private static bool IsTextureAsset(string file)
    {
        return file.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
            || file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
            || file.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
            || file.EndsWith(".webp", StringComparison.OrdinalIgnoreCase)
            || file.EndsWith(".svg", StringComparison.OrdinalIgnoreCase);
    }

    private static Rect2 VisibleTextureRegion(Texture2D texture)
    {
        Vector2 sourceSize = texture.GetSize();
        Rect2 fullRegion = new(Vector2.Zero, sourceSize);
        Image? image = texture.GetImage();
        if (image == null)
        {
            return fullRegion;
        }

        int width = image.GetWidth();
        int height = image.GetHeight();
        if (width <= 0 || height <= 0)
        {
            return fullRegion;
        }

        int minX = width;
        int minY = height;
        int maxX = -1;
        int maxY = -1;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (image.GetPixel(x, y).A <= 0.03f)
                {
                    continue;
                }

                minX = Math.Min(minX, x);
                minY = Math.Min(minY, y);
                maxX = Math.Max(maxX, x);
                maxY = Math.Max(maxY, y);
            }
        }

        if (maxX < minX || maxY < minY)
        {
            return fullRegion;
        }

        return new Rect2(new Vector2(minX, minY), new Vector2(maxX - minX + 1, maxY - minY + 1));
    }

    private Texture2D? CurrentTitleLogo()
    {
        if (_language == GameLanguage.Chinese)
        {
            return _titleLogoCn ?? _titleLogoEn;
        }

        return _titleLogoEn ?? _titleLogoCn;
    }

    public override void _Process(double delta)
    {
        float dt = Mathf.Min((float)delta, 0.033f);
        _time += dt;
        FillAudio();
        UpdatePointerInputMode();
        UpdateGamepadNavigation(dt);
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

        UpdateCenterTextQueue(dt);
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
        UpdatePerformancePressure(dt);
        QueueRedraw();
        CaptureButtons();
    }

    public override void _Draw()
    {
        _drawShakeOffset = CalculateShakeOffset();
        _polygonScratchCursor = 0;
        _closedPolygonScratchCursor = 0;
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

            foreach (HazardField field in _hazardFields)
            {
                DrawHazardField(field);
            }

            foreach (HazardLine hazard in _hazards)
            {
                DrawHazard(hazard);
            }

            foreach (Particle particle in _particles)
            {
                DrawParticle(particle);
            }

            foreach (DroneCommandCue cue in _droneCommandCues)
            {
                DrawDroneCommandCue(cue);
            }

            foreach (Shockwave shockwave in _shockwaves)
            {
                DrawShockwave(shockwave);
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
            float age = 1.0f - t;
            float pop = damageText.ComboPop ? 1.0f + Mathf.Sin(age * Mathf.Pi) * 0.34f : 1.0f;
            Vector2 pos = damageText.Pos + ShakeOffset();
            int drawSize = Mathf.RoundToInt(damageText.Size * pop);
            float textWidth = EstimateTextPixelWidth(damageText.Text, drawSize);
            float drawWidth = Mathf.Clamp(textWidth + (damageText.ComboPop ? 34.0f : 28.0f), damageText.ComboPop ? 96.0f : 70.0f, damageText.ComboPop ? 220.0f : 180.0f);
            DrawText(damageText.Text, pos - new Vector2(drawWidth * 0.5f, 0.0f), drawSize, Alpha(damageText.Color, t), HorizontalAlignment.Center, drawWidth, true, damageText.ComboPop ? 2 : 3);
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
        ClearOrbiterVisuals();
        ClearDroneCommandCues();
        _shockwaves.Clear();
        ClearDamageTexts();
        _centerTextQueue.Clear();
        _centerTextQueueTimer = 0.0f;
        _hazards.Clear();
        _hazardFields.Clear();
        _upgradeChoices.Clear();
        _voices.Clear();
        _gamepadPilotIndex = PilotIndex(_selectedPilot);
        _gamepadTitleIndex = TitlePilotFocusStart;
        _gamepadUpgradeIndex = 0;
        _gamepadSettingsIndex = 0;
    }

    private void LoadMetaProgress()
    {
        _metaRanks.Clear();
        ConfigFile config = new();
        if (config.Load(MetaSavePath) != Error.Ok)
        {
            ApplyRuntimeSettings(false);
            return;
        }

        string languageName = ReadConfigString(config, "settings", "language", _language.ToString());
        if (Enum.TryParse(languageName, out GameLanguage loadedLanguage))
        {
            _language = loadedLanguage;
        }
        string resolutionName = ReadConfigString(config, "settings", "resolution", _resolutionPreset.ToString());
        if (Enum.TryParse(resolutionName, out DisplayResolutionPreset loadedResolution))
        {
            _resolutionPreset = loadedResolution;
        }
        string qualityName = ReadConfigString(config, "settings", "quality", _visualQuality.ToString());
        if (Enum.TryParse(qualityName, out VisualQuality loadedQuality))
        {
            _visualQuality = loadedQuality;
        }
        GameDifficulty loadedDifficulty = _selectedDifficulty;
        string difficultyName = ReadConfigString(config, "settings", "difficulty", _selectedDifficulty.ToString());
        if (Enum.TryParse(difficultyName, out GameDifficulty parsedDifficulty))
        {
            loadedDifficulty = parsedDifficulty;
        }
        _musicVolume = Mathf.Clamp(ReadConfigInt(config, "settings", "music_volume", Mathf.RoundToInt(_musicVolume * 100.0f)) / 100.0f, 0.0f, 1.0f);
        _sfxVolume = Mathf.Clamp(ReadConfigInt(config, "settings", "sfx_volume", Mathf.RoundToInt(_sfxVolume * 100.0f)) / 100.0f, 0.0f, 1.0f);

        _starDust = Mathf.Max(0, ReadConfigInt(config, "meta", "star_dust", 0));
        _lifetimeDust = Mathf.Max(0, ReadConfigInt(config, "meta", "lifetime_dust", 0));
        _bestScore = Mathf.Max(0, ReadConfigInt(config, "stats", "best_score", 0));
        _bestWave = Mathf.Clamp(ReadConfigInt(config, "stats", "best_wave", 0), 0, TotalWaves);
        _runsCompleted = Mathf.Max(0, ReadConfigInt(config, "stats", "runs_completed", 0));
        _wonOnce = ReadConfigInt(config, "stats", "won_once", 0) > 0;
        _careerKills = Mathf.Max(0, ReadConfigInt(config, "career", "kills", 0));
        _careerPickups = Mathf.Max(0, ReadConfigInt(config, "career", "pickups", 0));
        _careerAbsorbs = Mathf.Max(0, ReadConfigInt(config, "career", "absorbs", 0));
        _careerBestCombo = Mathf.Max(0, ReadConfigInt(config, "career", "best_combo", 0));
        _careerBossKills = Mathf.Max(0, ReadConfigInt(config, "career", "boss_kills", 0));
        _careerPerfectWaves = Mathf.Max(0, ReadConfigInt(config, "career", "perfect_waves", 0));
        LoadClearTimeRecords(config);
        LoadDifficultyTestUnlocks(config);
        _selectedDifficulty = ClampDifficulty(loadedDifficulty);
        _runDifficulty = _selectedDifficulty;
        _pilotRuns.Clear();
        int pilotRunTotal = 0;
        for (int i = 0; i < PilotCount(); i++)
        {
            PilotKind pilot = PilotFromIndex(i);
            int runs = Mathf.Max(0, ReadConfigInt(config, "pilot_runs", pilot.ToString(), 0));
            if (runs > 0)
            {
                _pilotRuns[pilot] = runs;
                pilotRunTotal += runs;
            }
        }
        if (pilotRunTotal == 0 && _runsCompleted > 0)
        {
            _pilotRuns[PilotKind.Astra] = _runsCompleted;
        }
        string pilotName = ReadConfigString(config, "settings", "pilot", PilotKind.Astra.ToString());
        if (Enum.TryParse(pilotName, out PilotKind loadedPilot) && IsPilotUnlocked(loadedPilot))
        {
            _selectedPilot = loadedPilot;
        }
        else
        {
            _selectedPilot = PilotKind.Astra;
        }
        _gamepadPilotIndex = PilotIndex(_selectedPilot);
        _gamepadTitleIndex = TitlePilotFocusStart;
        ApplyRuntimeSettings(false);

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
        config.SetValue("career", "absorbs", _careerAbsorbs);
        config.SetValue("career", "best_combo", _careerBestCombo);
        config.SetValue("career", "boss_kills", _careerBossKills);
        config.SetValue("career", "perfect_waves", _careerPerfectWaves);
        foreach (GameDifficulty difficulty in DifficultyOrder())
        {
            List<float> records = ClearTimeRecords(difficulty);
            string key = DifficultyKey(difficulty);
            config.SetValue("difficulty_unlocks", key, _difficultyTestUnlocks[DifficultyIndex(difficulty)] ? 1 : 0);
            for (int i = 0; i < ClearRecordCount; i++)
            {
                int centiseconds = i < records.Count ? TimeToCentiseconds(records[i]) : 0;
                config.SetValue("records", $"clear_time_{key}_{i}_cs", centiseconds);
                if (difficulty == GameDifficulty.Cruise)
                {
                    config.SetValue("records", $"clear_time_{i}_cs", centiseconds);
                }
            }
        }
        List<float> cruiseRecords = ClearTimeRecords(GameDifficulty.Cruise);
        config.SetValue("records", "best_clear_time_cs", cruiseRecords.Count > 0 ? TimeToCentiseconds(cruiseRecords[0]) : 0);
        for (int i = 0; i < PilotCount(); i++)
        {
            PilotKind pilot = PilotFromIndex(i);
            config.SetValue("pilot_runs", pilot.ToString(), PilotRunCount(pilot));
        }
        config.SetValue("settings", "pilot", _selectedPilot.ToString());
        config.SetValue("settings", "difficulty", _selectedDifficulty.ToString());
        config.SetValue("settings", "language", _language.ToString());
        config.SetValue("settings", "music_volume", Mathf.RoundToInt(_musicVolume * 100.0f));
        config.SetValue("settings", "sfx_volume", Mathf.RoundToInt(_sfxVolume * 100.0f));
        config.SetValue("settings", "resolution", _resolutionPreset.ToString());
        config.SetValue("settings", "quality", _visualQuality.ToString());

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
        _careerAbsorbs = 0;
        _careerBestCombo = 0;
        _careerBossKills = 0;
        _careerPerfectWaves = 0;
        _pilotRuns.Clear();
        _wonOnce = false;
        _selectedPilot = PilotKind.Astra;
        _gamepadPilotIndex = PilotIndex(_selectedPilot);
        _gamepadTitleIndex = TitlePilotFocusStart;
        _lastDustEarned = 0;
        _lastRunWave = 0;
        _lastObjectiveBonusDust = 0;
        _lastScoreBonusDust = 0;
        _lastClearTime = 0.0f;
        _lastClearRecordRank = 0;
        _lastUnlockedPilot = null;
        foreach (List<float> records in _clearTimeRecordsByDifficulty)
        {
            records.Clear();
        }
        Array.Clear(_difficultyTestUnlocks, 0, _difficultyTestUnlocks.Length);
        _selectedDifficulty = GameDifficulty.Cruise;
        _runDifficulty = GameDifficulty.Cruise;
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

    private static GameDifficulty[] DifficultyOrder()
    {
        return new[] { GameDifficulty.Cruise, GameDifficulty.Storm, GameDifficulty.Eclipse };
    }

    private static int DifficultyIndex(GameDifficulty difficulty)
    {
        return difficulty switch
        {
            GameDifficulty.Storm => 1,
            GameDifficulty.Eclipse => 2,
            _ => 0,
        };
    }

    private static string DifficultyKey(GameDifficulty difficulty)
    {
        return difficulty switch
        {
            GameDifficulty.Storm => "storm",
            GameDifficulty.Eclipse => "eclipse",
            _ => "cruise",
        };
    }

    private List<float> ClearTimeRecords(GameDifficulty difficulty)
    {
        return _clearTimeRecordsByDifficulty[Mathf.Clamp(DifficultyIndex(difficulty), 0, DifficultyCount - 1)];
    }

    private bool IsDifficultyUnlocked(GameDifficulty difficulty)
    {
        if (_difficultyTestUnlocks[Mathf.Clamp(DifficultyIndex(difficulty), 0, DifficultyCount - 1)])
        {
            return true;
        }

        return difficulty switch
        {
            GameDifficulty.Storm => ClearTimeRecords(GameDifficulty.Cruise).Count > 0,
            GameDifficulty.Eclipse => ClearTimeRecords(GameDifficulty.Storm).Count > 0,
            _ => true,
        };
    }

    private void LoadDifficultyTestUnlocks(ConfigFile config)
    {
        foreach (GameDifficulty difficulty in DifficultyOrder())
        {
            _difficultyTestUnlocks[DifficultyIndex(difficulty)] = ReadConfigInt(config, "difficulty_unlocks", DifficultyKey(difficulty), 0) > 0;
        }
    }

    private GameDifficulty HighestUnlockedDifficulty()
    {
        for (int i = DifficultyCount - 1; i >= 0; i--)
        {
            GameDifficulty difficulty = DifficultyFromIndex(i);
            if (IsDifficultyUnlocked(difficulty))
            {
                return difficulty;
            }
        }

        return GameDifficulty.Cruise;
    }

    private GameDifficulty ClampDifficulty(GameDifficulty difficulty)
    {
        return IsDifficultyUnlocked(difficulty) ? difficulty : HighestUnlockedDifficulty();
    }

    private static GameDifficulty DifficultyFromIndex(int index)
    {
        return index switch
        {
            1 => GameDifficulty.Storm,
            2 => GameDifficulty.Eclipse,
            _ => GameDifficulty.Cruise,
        };
    }

    private Color DifficultyAccent(GameDifficulty difficulty)
    {
        return difficulty switch
        {
            GameDifficulty.Storm => Gold,
            GameDifficulty.Eclipse => Rose,
            _ => Cyan,
        };
    }

    private string DifficultyName(GameDifficulty difficulty)
    {
        return T($"difficulty.{DifficultyKey(difficulty)}");
    }

    private string DifficultyUnlockText(GameDifficulty difficulty)
    {
        return difficulty switch
        {
            GameDifficulty.Storm => Tf("difficulty.unlock", DifficultyName(GameDifficulty.Cruise)),
            GameDifficulty.Eclipse => Tf("difficulty.unlock", DifficultyName(GameDifficulty.Storm)),
            _ => string.Empty,
        };
    }

    private float DifficultyEnemyMoveScale()
    {
        return _runDifficulty switch
        {
            GameDifficulty.Storm => 1.12f,
            GameDifficulty.Eclipse => 1.28f,
            _ => 0.96f,
        };
    }

    private float DifficultyEnemyHpScale()
    {
        return _runDifficulty switch
        {
            GameDifficulty.Storm => 1.3f,
            GameDifficulty.Eclipse => 2.15f,
            _ => 0.94f,
        };
    }

    private float DifficultyBossHpScale()
    {
        return _runDifficulty switch
        {
            GameDifficulty.Storm => 1.38f,
            GameDifficulty.Eclipse => 2.2f,
            _ => 0.96f,
        };
    }

    private float DifficultyEnemyArmorScale()
    {
        return _runDifficulty switch
        {
            GameDifficulty.Storm => 1.06f,
            GameDifficulty.Eclipse => 1.16f,
            _ => 1.0f,
        };
    }

    private float DifficultyBossArmorScale()
    {
        return _runDifficulty switch
        {
            GameDifficulty.Storm => 1.08f,
            GameDifficulty.Eclipse => 1.24f,
            _ => 1.0f,
        };
    }

    private float DifficultyEnemyDamageTakenScale(Enemy enemy)
    {
        if (enemy.Kind == EnemyKind.Boss)
        {
            return 1.0f;
        }

        float scale = _runDifficulty switch
        {
            GameDifficulty.Storm => 0.9f,
            GameDifficulty.Eclipse => 0.68f,
            _ => 1.0f,
        };
        if (enemy.Elite)
        {
            scale *= _runDifficulty == GameDifficulty.Eclipse ? 0.88f : 0.94f;
        }

        return scale;
    }

    private float DifficultyEnemyBulletSpeedScale()
    {
        return _runDifficulty switch
        {
            GameDifficulty.Storm => 1.1f,
            GameDifficulty.Eclipse => 1.26f,
            _ => 0.94f,
        };
    }

    private float DifficultyEnemyDamageScale()
    {
        return _runDifficulty switch
        {
            GameDifficulty.Storm => 1.08f,
            GameDifficulty.Eclipse => 1.24f,
            _ => 0.94f,
        };
    }

    private float DifficultyHazardFrequencyScale()
    {
        return _runDifficulty switch
        {
            GameDifficulty.Storm => 1.12f,
            GameDifficulty.Eclipse => 1.28f,
            _ => 0.9f,
        };
    }

    private float DifficultyHazardTempoScale()
    {
        return _runDifficulty switch
        {
            GameDifficulty.Storm => 1.12f,
            GameDifficulty.Eclipse => 1.26f,
            _ => 0.92f,
        };
    }

    private float DifficultyHazardDamageScale()
    {
        return _runDifficulty switch
        {
            GameDifficulty.Storm => 1.12f,
            GameDifficulty.Eclipse => 1.35f,
            _ => 0.9f,
        };
    }

    private float DifficultyBossCooldownScale()
    {
        return _runDifficulty switch
        {
            GameDifficulty.Storm => 0.94f,
            GameDifficulty.Eclipse => 0.82f,
            _ => 1.06f,
        };
    }

    private void LoadClearTimeRecords(ConfigFile config)
    {
        foreach (List<float> records in _clearTimeRecordsByDifficulty)
        {
            records.Clear();
        }

        foreach (GameDifficulty difficulty in DifficultyOrder())
        {
            string key = DifficultyKey(difficulty);
            for (int i = 0; i < ClearRecordCount; i++)
            {
                AddClearTimeRecord(difficulty, CentisecondsToTime(ReadConfigInt(config, "records", $"clear_time_{key}_{i}_cs", 0)));
            }
        }

        List<float> cruiseRecords = ClearTimeRecords(GameDifficulty.Cruise);
        if (cruiseRecords.Count == 0)
        {
            for (int i = 0; i < ClearRecordCount; i++)
            {
                AddClearTimeRecord(GameDifficulty.Cruise, CentisecondsToTime(ReadConfigInt(config, "records", $"clear_time_{i}_cs", 0)));
            }
        }

        if (cruiseRecords.Count == 0)
        {
            AddClearTimeRecord(GameDifficulty.Cruise, CentisecondsToTime(ReadConfigInt(config, "records", "best_clear_time_cs", 0)));
        }
    }

    private int AddClearTimeRecord(GameDifficulty difficulty, float seconds)
    {
        if (seconds <= 0.0f)
        {
            return 0;
        }

        List<float> records = ClearTimeRecords(difficulty);
        records.Add(seconds);
        records.Sort();
        int rank = records.IndexOf(seconds) + 1;
        while (records.Count > ClearRecordCount)
        {
            records.RemoveAt(records.Count - 1);
        }

        return rank > 0 && rank <= ClearRecordCount ? rank : 0;
    }

    private static int TimeToCentiseconds(float seconds)
    {
        return Mathf.Max(0, Mathf.RoundToInt(seconds * 100.0f));
    }

    private static float CentisecondsToTime(int centiseconds)
    {
        return Mathf.Max(0, centiseconds) / 100.0f;
    }

    private void ApplyRuntimeSettings(bool save)
    {
        if (save)
        {
            ApplyWindowResolution();
        }
        GenerateBackdrop();
        if (save)
        {
            SaveMetaProgress();
        }
    }

    private void ApplyWindowResolution()
    {
        if (IsHeadlessLaunch())
        {
            return;
        }

        Vector2I size = ResolutionSize(_resolutionPreset);
        DisplayServer.WindowSetMinSize(new Vector2I(960, 540));
        DisplayServer.WindowSetSize(size);
        Rect2I usable = DisplayServer.ScreenGetUsableRect();
        if (size.X <= usable.Size.X && size.Y <= usable.Size.Y)
        {
            Vector2I offset = new((usable.Size.X - size.X) / 2, (usable.Size.Y - size.Y) / 2);
            DisplayServer.WindowSetPosition(usable.Position + offset);
        }
    }

    private static bool IsHeadlessLaunch()
    {
        foreach (string arg in System.Environment.GetCommandLineArgs())
        {
            if (string.Equals(arg, "--headless", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return OS.HasFeature("headless") || OS.HasFeature("server");
    }

    private static Vector2I ResolutionSize(DisplayResolutionPreset preset)
    {
        return preset switch
        {
            DisplayResolutionPreset.R1280x720 => new Vector2I(1280, 720),
            DisplayResolutionPreset.R1600x900 => new Vector2I(1600, 900),
            DisplayResolutionPreset.R2560x1440 => new Vector2I(2560, 1440),
            _ => new Vector2I(1920, 1080),
        };
    }

    private static string ResolutionDisplayName(DisplayResolutionPreset preset)
    {
        Vector2I size = ResolutionSize(preset);
        return $"{size.X} x {size.Y}";
    }

    private string QualityDisplayName(VisualQuality quality)
    {
        return quality switch
        {
            VisualQuality.Low => T("settings.quality.low"),
            VisualQuality.Medium => T("settings.quality.medium"),
            VisualQuality.Ultra => T("settings.quality.ultra"),
            _ => T("settings.quality.high"),
        };
    }

    private static int ResolutionPresetCount()
    {
        return 4;
    }

    private static int VisualQualityCount()
    {
        return 4;
    }

    private static int WrapIndex(int index, int count)
    {
        if (count <= 0)
        {
            return 0;
        }

        return (index % count + count) % count;
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
        int scoreDust = _score / 7000;
        int baseDust = 10 + Mathf.RoundToInt(reachedWave * 1.35f) + scoreDust + sectorBonus + (victory ? 45 : 0);
        float dustBonus = 1.0f + MetaRank(MetaUpgradeId.SalvageRig) * 0.045f + MetaRank(MetaUpgradeId.DeepSurvey) * 0.035f;
        int earned = Mathf.Max(10, Mathf.RoundToInt(baseDust * dustBonus)) + _runObjectiveBonusDust + _runScoreBonusDust;
        PilotKind? possibleUnlock = NextPilot(_runPilot);
        bool unlockWasLocked = possibleUnlock.HasValue && !IsPilotUnlocked(possibleUnlock.Value);

        _lastDustEarned = earned;
        _lastRunWave = reachedWave;
        _lastObjectiveBonusDust = _runObjectiveBonusDust;
        _lastClearTime = victory ? _runTimer : 0.0f;
        _lastClearRecordRank = 0;
        _lastScoreBonusDust = Mathf.RoundToInt(scoreDust * dustBonus) + _runScoreBonusDust;
        _lastUnlockedPilot = null;
        _starDust += earned;
        _lifetimeDust += earned;
        _runsCompleted++;
        _pilotRuns[_runPilot] = PilotRunCount(_runPilot) + 1;
        if (unlockWasLocked && possibleUnlock.HasValue && IsPilotUnlocked(possibleUnlock.Value))
        {
            _lastUnlockedPilot = possibleUnlock.Value;
        }
        _bestScore = Math.Max(_bestScore, _score);
        _bestWave = Math.Max(_bestWave, reachedWave);
        if (victory)
        {
            _lastClearRecordRank = AddClearTimeRecord(_runDifficulty, _runTimer);
        }
        _careerKills += _runKills;
        _careerPickups += _runPickups;
        _careerAbsorbs += _runAbsorbs;
        _careerBestCombo = Math.Max(_careerBestCombo, _runBestCombo);
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
        _selectedDifficulty = ClampDifficulty(_selectedDifficulty);
        _runDifficulty = _selectedDifficulty;

        _mode = GameMode.Playing;
        _settingsReturnMode = GameMode.Playing;
        _runPilot = _selectedPilot;
        _wave = 0;
        _score = 0;
        _runTimer = 0.0f;
        _combo = 0;
        _comboTimer = 0.0f;
        _comboTier = 0;
        _comboTierPulse = 0.0f;
        _scoreCacheLevel = 0;
        _nextScoreCache = ScoreCacheThreshold(0);
        _runScoreBonusDust = 0;
        _lastScoreBonusDust = 0;
        _lastUnlockedPilot = null;
        _scoreCachePulse = 0.0f;
        _runLevel = 1;
        _xp = 0;
        _xpToNext = XpThresholdForLevel(_runLevel);
        _queuedLevelUps = 0;
        _xpPulse = 0.0f;
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
        _ultimateCooldown = 0.0f;
        _playerPolarity = CruiseStance;
        _polarityCooldown = 0.0f;
        _polarityDenyTextCooldown = 0.0f;
        _waveClearTimer = 0.0f;
        _waveIntelPulse = 0.0f;
        _bossPatternTimer = 0.0f;
        _spawnDirector = 0.0f;
        _waveSpawnTimer = 0.0f;
        _waveSpawnInterval = 0.0f;
        _waveNextSpawnCount = 0;
        _waveBudget = 0;
        _waveProgressBudget = 0.0f;
        _waveProgressSpent = 0.0f;
        _waveSpawnIndex = 0;
        _waveEventMask = 0;
        _waveRewardBoost = 1.0f;
        _sectorHazardTimer = 4.0f;
        _currentWavePace = WavePaceKind.Standard;
        _combatChain = 0;
        _hasLastBossArchetype = false;
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
        _novaCost = UltimateCostBase - reactorRank * 1.2f - novaRank * 2.0f;
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
        _chainRelay = 0;
        _fractalSplit = 0;
        _pulseMagazine = 0;
        _executionMark = 0;
        _stasisField = 0;
        _magnetizedCore = 0;
        _ricochetMatrix = 0;
        _nyxOrbit = 0;
        _nyxSingularity = 0;
        _nyxEventHorizon = 0;
        _nyxGravityCantor = 0;
        _rookBulwarkCore = 0;
        _rookSiegeBattery = 0;
        _rookAegisRelay = 0;
        _rookCitadelProtocol = 0;
        _lyraResonanceChord = 0;
        _lyraTempoBloom = 0;
        _lyraHarmonicCascade = 0;
        _lyraEncoreField = 0;
        _orionCometSpear = 0;
        _orionDeadeyeMark = 0;
        _orionStarfallQuiver = 0;
        _orionPerihelionVector = 0;
        _lyraBeat = 0;
        _draftBiasWeapon = 0;
        _draftBiasDefense = 0;
        _draftBiasSkill = 0;
        _draftBiasFlow = 0;
        _draftBiasEconomy = 0;
        _buildMilestoneMask = 0;
        _riftNeedle = false;
        _runRewardGranted = false;
        _lastDustEarned = 0;
        _lastRunWave = 0;
        _lastObjectiveBonusDust = 0;
        _lastClearTime = 0.0f;
        _lastClearRecordRank = 0;
        _lastUnlockedPilot = null;
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
        ClampUltimateCost();
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
        ClearOrbiterVisuals();
        ClearDroneCommandCues();
        _shockwaves.Clear();
        ClearDamageTexts();
        _centerTextQueue.Clear();
        _centerTextQueueTimer = 0.0f;
        _hazards.Clear();
        _hazardFields.Clear();
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
                _playerMaxHp += 10.0f;
                _playerHp += 10.0f;
                break;
            case PilotKind.Nyx:
                _fireInterval = 0.38f;
                _damageMultiplier *= 0.92f;
                _playerSpeed += 22.0f;
                _dashPower += 70.0f;
                _pickupMagnet += 28.0f;
                break;
            case PilotKind.Rook:
                _fireInterval = 0.72f;
                _damageMultiplier += 0.12f;
                _playerSpeed -= 38.0f;
                _playerMaxHp += 52.0f;
                _playerHp += 52.0f;
                _mirrorReduction *= 0.88f;
                break;
            case PilotKind.Lyra:
                _fireInterval = 0.3f;
                _damageMultiplier *= 0.82f;
                _maxEnergy += 20.0f;
                _energy = Mathf.Clamp(_energy + 20.0f, 0.0f, _maxEnergy);
                _echoChance += 0.06f;
                break;
            case PilotKind.Orion:
                _fireInterval = 0.66f;
                _damageMultiplier += 0.24f;
                _critMultiplier += 0.18f;
                _playerSpeed -= 8.0f;
                break;
            default:
                break;
        }
    }

    private void ClampUltimateCost()
    {
        _novaCost = Mathf.Clamp(_novaCost, UltimateCostFloor, _maxEnergy);
    }

    private void BeginNextWave()
    {
        _wave++;
        _waveClearTimer = 0.0f;
        _waveTookDamage = false;
        _spawnDirector = 0.0f;
        _waveSpawnTimer = 0.0f;
        _waveSpawnInterval = 0.0f;
        _waveNextSpawnCount = 0;
        _waveBudget = 0;
        _waveProgressBudget = 0.0f;
        _waveProgressSpent = 0.0f;
        _waveSpawnIndex = 0;
        _waveEventMask = 0;
        _waveRewardBoost = 1.0f;
        _waveIntelPulse = 1.0f;
        _centerTextQueue.Clear();
        _centerTextQueueTimer = 0.0f;
        _pendingSpawns.Clear();
        _bossPatternTimer = 0.0f;
        _sectorHazardTimer = Mathf.Max(2.5f, 8.0f - CurrentSectorIndex());
        _currentWavePace = WavePaceFor(_wave);
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
            QueueCenterText(Tf("sector.enter", sector + 1, T(info.NameKey)), ScreenCenter + new Vector2(0.0f, -250.0f), info.Accent, 36.0f);
            QueueCenterText(T(info.TraitKey), ScreenCenter + new Vector2(0.0f, -202.0f), Paper, 22.0f);
        }

        if (_currentWavePace == WavePaceKind.Boss)
        {
            _waveBudget = 1;
            _waveProgressBudget = 1.0f;
            Enemy? boss = SpawnBoss();
            string title = boss != null ? BossTitle(boss.BossArchetype, sector) : Tf("boss.sector", T(info.NameKey));
            QueueCenterText(title, ScreenCenter + new Vector2(0.0f, -180.0f), boss != null ? BossAccent(boss.BossArchetype) : info.Accent, 44.0f);
            PlaySfx(72.0f, 0.5f, 1.6f, 0.46f, 0.3f, 0);
            return;
        }

        int baseBudget = WaveBaseBudget(sector, waveInSector);
        float budget = Mathf.Max(2.0f, (baseBudget + _nextWaveBonusEnemies) * WaveBudgetScale(_currentWavePace));
        _waveProgressBudget = WaveProgressBudget(budget, sector, waveInSector, _currentWavePace);
        _waveBudget = Mathf.CeilToInt(_waveProgressBudget);
        _waveRewardBoost = _nextWaveRewardBoost * WaveRewardScale(_currentWavePace);
        EnemyKind primaryKind = WavePrimaryEnemyKind(sector, waveInSector);
        EnemyKind supportKind = WaveSupportEnemyKind(sector, waveInSector);
        _nextWaveBonusEnemies = 0;
        _nextWaveRewardBoost = 1.0f;
        int openingBatch = WaveOpeningBatchCount(sector, waveInSector, _currentWavePace, _waveProgressBudget);
        for (int i = 0; i < openingBatch && !WaveProgressComplete(); i++)
        {
            SpawnPendingEnemy(CreateNextWaveSpawn(primaryKind, supportKind));
        }
        ScheduleNextSpawnBatch(false);

        QueueCenterText($"{T("wave.engage")}  /  {WavePaceText(_currentWavePace)}", ScreenCenter + new Vector2(0.0f, -210.0f), info.Accent, 42.0f);
        if (_currentWavePace == WavePaceKind.Recovery)
        {
            QueueCenterText(T("flow.supply"), ScreenCenter + new Vector2(0.0f, -168.0f), Jade, 22.0f);
            SpawnPickup(ScreenCenter + new Vector2(-72.0f, 34.0f), PickupKind.Energy);
            SpawnPickup(ScreenCenter + new Vector2(72.0f, 34.0f), PickupKind.Repair);
        }
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
            QueueCenterText(tutorial, ScreenCenter + new Vector2(0.0f, -162.0f), Paper, 23.0f);
        }
    }

    private void UpdateWaveSpawns(float dt)
    {
        if (_currentWavePace == WavePaceKind.Boss)
        {
            return;
        }

        int sector = CurrentSectorIndex();
        int waveInSector = CurrentWaveInSector();
        EnemyKind primaryKind = WavePrimaryEnemyKind(sector, waveInSector);
        EnemyKind supportKind = WaveSupportEnemyKind(sector, waveInSector);

        if (WaveProgressComplete() && _pendingSpawns.Count == 0)
        {
            _waveSpawnTimer = 0.0f;
            _waveSpawnInterval = 0.0f;
            _waveNextSpawnCount = 0;
            return;
        }

        RefreshSpawnSchedule();
        if (_enemies.Count <= 1 + sector / 2 && WaveProgress01() < 0.38f && _waveSpawnTimer > 1.15f)
        {
            _waveSpawnTimer = 1.15f;
        }
        _waveSpawnTimer -= dt;

        if (_waveSpawnTimer > 0.0f)
        {
            return;
        }

        float pressure = PerformancePressure();
        int activeSoftCap = ActiveWaveEnemySoftCap();
        if (_enemies.Count >= activeSoftCap && pressure > 0.62f)
        {
            _waveSpawnTimer = 0.38f;
            _waveSpawnInterval = Math.Max(_waveSpawnInterval, 0.38f);
            _waveNextSpawnCount = Math.Max(5, Math.Min(CurrentSpawnBatchCount(), 7));
            return;
        }

        int spawnCount = CurrentSpawnBatchCount();
        if (pressure > 0.9f)
        {
            spawnCount = Math.Min(spawnCount, 4);
        }
        else if (pressure > 0.8f || _enemies.Count > 56 + sector * 4)
        {
            spawnCount = Math.Min(spawnCount, 6);
        }

        int availableSlots = Math.Max(0, MaxEnemies - _enemies.Count);
        spawnCount = Math.Min(spawnCount, availableSlots);
        if (spawnCount <= 0)
        {
            _waveSpawnTimer = 0.38f;
            _waveSpawnInterval = Math.Max(_waveSpawnInterval, 0.38f);
            return;
        }

        int spawned = 0;
        while (spawned < spawnCount && _pendingSpawns.Count > 0)
        {
            SpawnPendingEnemy(_pendingSpawns.Dequeue());
            spawned++;
        }

        while (spawned < spawnCount && !WaveProgressComplete())
        {
            SpawnPendingEnemy(CreateNextWaveSpawn(primaryKind, supportKind));
            spawned++;
        }

        if (spawned > 0)
        {
            _waveIntelPulse = Math.Max(_waveIntelPulse, 0.45f);
        }

        ScheduleNextSpawnBatch(false);
    }

    private void ScheduleNextSpawnBatch(bool preserveProgress)
    {
        if (_currentWavePace == WavePaceKind.Boss || (WaveProgressComplete() && _pendingSpawns.Count == 0))
        {
            _waveSpawnTimer = 0.0f;
            _waveSpawnInterval = 0.0f;
            _waveNextSpawnCount = 0;
            return;
        }

        float progress = preserveProgress ? NextReserveSpawnProgress01() : 0.0f;
        _waveSpawnInterval = CurrentReserveSpawnInterval();
        _waveSpawnTimer = Mathf.Clamp(_waveSpawnInterval * (1.0f - progress), 0.0f, _waveSpawnInterval);
        _waveNextSpawnCount = CurrentSpawnBatchCount();
    }

    private void RefreshSpawnSchedule()
    {
        if (_waveSpawnInterval <= 0.001f)
        {
            ScheduleNextSpawnBatch(false);
            return;
        }

        float progress = NextReserveSpawnProgress01();
        float targetInterval = CurrentReserveSpawnInterval();
        if (Math.Abs(targetInterval - _waveSpawnInterval) > 0.035f)
        {
            _waveSpawnInterval = targetInterval;
            _waveSpawnTimer = Mathf.Clamp(_waveSpawnInterval * (1.0f - progress), 0.0f, _waveSpawnInterval);
        }

        _waveNextSpawnCount = CurrentSpawnBatchCount();
    }

    private float CurrentReserveSpawnInterval()
    {
        if (_currentWavePace == WavePaceKind.Boss)
        {
            return 0.0f;
        }

        float interval = BaseReserveSpawnInterval() / ComboSpawnRate();
        return Mathf.Clamp(interval, 0.9f, 7.0f);
    }

    private float BaseReserveSpawnInterval()
    {
        int sector = CurrentSectorIndex();
        int waveInSector = CurrentWaveInSector();
        float progress = Mathf.Pow(RunProgress01(), 0.78f);
        float interval = Mathf.Lerp(5.0f, 1.72f, progress);
        interval -= Math.Min(0.32f, Math.Max(0, waveInSector - 1) * 0.04f);
        interval *= WaveIntervalArcScale(waveInSector);
        interval *= WaveSpawnIntervalScale(_currentWavePace);
        interval *= Mathf.Lerp(1.08f, 0.68f, WavePressure01());
        interval *= 1.0f - Math.Min(0.16f, sector * 0.035f);
        return Mathf.Clamp(interval, 0.95f, 7.2f);
    }

    private int CurrentSpawnBatchCount()
    {
        if (_currentWavePace == WavePaceKind.Boss)
        {
            return 0;
        }

        int sector = CurrentSectorIndex();
        int waveInSector = CurrentWaveInSector();
        float wavePressure = WavePressure01();
        int count = 5 + Mathf.FloorToInt(wavePressure * 3.25f);
        if (sector >= 2 && wavePressure >= 0.45f)
        {
            count++;
        }
        if (sector >= 3 && wavePressure >= 0.72f)
        {
            count++;
        }

        count += waveInSector switch
        {
            3 => 1,
            4 => -1,
            6 => 1,
            7 => 2,
            _ => 0,
        };

        switch (_currentWavePace)
        {
            case WavePaceKind.Swarm:
                count += sector >= 2 ? 2 : 1;
                break;
            case WavePaceKind.Pressure:
                count++;
                break;
            case WavePaceKind.Elite:
                count = Math.Max(5, Math.Min(count + 1, 7 + sector / 2 + (wavePressure > 0.68f ? 1 : 0)));
                break;
            case WavePaceKind.Recovery:
                count = Math.Max(5, Math.Min(count - 1, 6 + sector / 2 + (wavePressure > 0.72f ? 1 : 0)));
                break;
        }

        if (_combo >= 96)
        {
            count++;
        }

        float pressure = PerformancePressure();
        if (pressure > 0.86f)
        {
            count = Math.Min(count, 4);
        }
        else if (pressure > 0.74f)
        {
            count = Math.Min(count, 6);
        }

        int minCount = pressure > 0.86f ? 4 : 5;
        return Mathf.Clamp(count, minCount, MaxSpawnBatchCount(sector, waveInSector));
    }

    private int ActiveWaveEnemySoftCap()
    {
        int sector = CurrentSectorIndex();
        int waveInSector = CurrentWaveInSector();
        float wavePressure = WavePressure01();
        int cap = 24 + sector * 7 + Mathf.FloorToInt(RunProgress01() * 18.0f);
        cap += Mathf.RoundToInt(Mathf.Lerp(-2.0f, 20.0f + sector * 2.0f, wavePressure));
        cap += waveInSector switch
        {
            1 => -3,
            2 => -1,
            3 => 7,
            4 => -4,
            5 => -2,
            6 => 8,
            7 => 12,
            _ => 0,
        };

        if (_currentWavePace == WavePaceKind.Swarm)
        {
            cap += 14;
        }
        else if (_currentWavePace == WavePaceKind.Pressure)
        {
            cap += 10;
        }
        else if (_currentWavePace == WavePaceKind.Elite || _currentWavePace == WavePaceKind.Recovery)
        {
            cap -= _currentWavePace == WavePaceKind.Recovery ? 3 : 1;
        }

        if (_combo >= 50)
        {
            cap += 5;
        }

        return Mathf.Clamp(cap + waveInSector / 2, 22, 86);
    }

    private PendingSpawn CreateNextWaveSpawn(EnemyKind primaryKind, EnemyKind supportKind)
    {
        EnemyKind kind = SelectEnemyKind(_waveSpawnIndex, CurrentSectorIndex(), CurrentWaveInSector(), primaryKind, supportKind, _currentWavePace);
        PendingSpawn spawn = new()
        {
            Kind = kind,
            Polarity = _waveSpawnIndex % 2,
            RewardBoost = _waveRewardBoost,
        };
        _waveSpawnIndex++;
        ConsumeWaveProgress(WaveSpawnProgressCost(kind, false));
        return spawn;
    }

    private void ConsumeWaveProgress(float amount)
    {
        _waveProgressSpent = Mathf.Clamp(_waveProgressSpent + amount, 0.0f, Math.Max(0.0f, _waveProgressBudget));
        TryTriggerWaveProgressEvent();
    }

    private bool WaveProgressComplete()
    {
        return _waveProgressBudget <= 0.0f || _waveProgressSpent >= _waveProgressBudget - 0.01f;
    }

    private float WaveProgress01()
    {
        if (_currentWavePace == WavePaceKind.Boss)
        {
            return 0.0f;
        }

        return _waveProgressBudget <= 0.0f ? 0.0f : Mathf.Clamp(_waveProgressSpent / _waveProgressBudget, 0.0f, 1.0f);
    }

    private float WavePressure01()
    {
        float progress = WaveProgress01();
        return progress * progress * (3.0f - 2.0f * progress);
    }

    private void TryTriggerWaveProgressEvent()
    {
        if (_currentWavePace == WavePaceKind.Boss || _mode != GameMode.Playing)
        {
            return;
        }

        float progress = WaveProgress01();
        for (int eventIndex = 0; eventIndex < 3; eventIndex++)
        {
            float threshold = eventIndex switch
            {
                0 => 0.32f,
                1 => 0.62f,
                _ => 0.86f,
            };
            int bit = 1 << eventIndex;
            if ((_waveEventMask & bit) != 0 || progress < threshold)
            {
                continue;
            }

            _waveEventMask |= bit;
            TriggerWaveProgressBeat(eventIndex);
        }
    }

    private void TriggerWaveProgressBeat(int eventIndex)
    {
        int sector = CurrentSectorIndex();
        int waveInSector = CurrentWaveInSector();
        float pressure = PerformancePressure();
        bool clean = !_waveTookDamage;
        bool highMomentum = _combo >= 18 + eventIndex * 20 && pressure < 0.78f;

        if (_currentWavePace == WavePaceKind.Recovery)
        {
            SpawnWaveEventPickup(eventIndex);
            QueueCenterText(T("flow.event.supply"), ScreenCenter + new Vector2(0.0f, -112.0f), Jade, 19.0f);
            _waveIntelPulse = 1.0f;
            return;
        }

        if (highMomentum)
        {
            QueueWaveEventEnemy(WaveEventEnemyKind(sector, _currentWavePace, eventIndex), eventIndex, true, 1.35f + eventIndex * 0.15f);
            if (_currentWavePace is WavePaceKind.Swarm or WavePaceKind.Pressure && pressure < 0.68f)
            {
                QueueWaveEventEnemy(WaveSupportEnemyKind(sector, waveInSector), eventIndex + 1, false, 1.12f);
            }
            if (sector > 0 || eventIndex > 0)
            {
                QueueWaveEventEnemy(WaveMechanicEnemyKind(sector, waveInSector, _currentWavePace, _waveSpawnIndex + eventIndex + 2), eventIndex + 2, false, 1.12f);
            }
            QueueCenterText(T("flow.event.elite"), ScreenCenter + new Vector2(0.0f, -112.0f), Gold, 20.0f);
            _waveIntelPulse = 1.0f;
            return;
        }

        if (_currentWavePace == WavePaceKind.Swarm && pressure < 0.72f)
        {
            int count = eventIndex == 0 ? 3 : 4;
            for (int i = 0; i < count; i++)
            {
                EnemyKind kind = i == count - 1
                    ? WaveMechanicEnemyKind(sector, waveInSector, _currentWavePace, _waveSpawnIndex + eventIndex + i)
                    : WaveEventEnemyKind(sector, _currentWavePace, eventIndex);
                QueueWaveEventEnemy(kind, eventIndex + i, false, 1.08f);
            }
            QueueCenterText(T("flow.event.surge"), ScreenCenter + new Vector2(0.0f, -112.0f), CurrentSector().Accent, 19.0f);
            _waveIntelPulse = 1.0f;
            return;
        }

        if (_currentWavePace == WavePaceKind.Pressure && pressure < 0.72f)
        {
            QueueWaveEventEnemy(WaveEventEnemyKind(sector, _currentWavePace, eventIndex), eventIndex, eventIndex > 0, eventIndex > 0 ? 1.28f : 1.12f);
            QueueWaveEventEnemy(WaveSupportEnemyKind(sector, waveInSector), eventIndex + 1, false, 1.08f);
            QueueWaveEventEnemy(WaveMechanicEnemyKind(sector, waveInSector, _currentWavePace, _waveSpawnIndex + eventIndex + 2), eventIndex + 2, false, 1.08f);
            QueueCenterText(T("flow.event.surge"), ScreenCenter + new Vector2(0.0f, -112.0f), Gold, 19.0f);
            _waveIntelPulse = 1.0f;
            return;
        }

        if (_currentWavePace == WavePaceKind.Elite && pressure < 0.68f)
        {
            QueueWaveEventEnemy(WaveEventEnemyKind(sector, _currentWavePace, eventIndex), eventIndex, true, 1.24f + eventIndex * 0.12f);
            if (eventIndex > 0)
            {
                QueueWaveEventEnemy(WaveMechanicEnemyKind(sector, waveInSector, _currentWavePace, _waveSpawnIndex + eventIndex + 1), eventIndex + 1, false, 1.08f);
            }
            QueueCenterText(T("flow.event.elite"), ScreenCenter + new Vector2(0.0f, -112.0f), Gold, 20.0f);
            _waveIntelPulse = 1.0f;
            return;
        }

        if (clean && _combo >= 8 + eventIndex * 8)
        {
            SpawnWaveEventPickup(eventIndex);
            QueueCenterText(T("flow.event.supply"), ScreenCenter + new Vector2(0.0f, -112.0f), Jade, 19.0f);
            _waveIntelPulse = 1.0f;
        }
    }

    private void QueueWaveEventEnemy(EnemyKind kind, int eventIndex, bool elite, float rewardBoost)
    {
        _pendingSpawns.Enqueue(new PendingSpawn
        {
            Kind = kind,
            Polarity = (_waveSpawnIndex + eventIndex) % 2,
            RewardBoost = _waveRewardBoost * rewardBoost,
            Elite = elite,
        });
    }

    private void SpawnWaveEventPickup(int eventIndex)
    {
        SpawnPickup(_playerPos + RandomDirection() * _rng.RandfRange(68.0f, 118.0f), eventIndex == 0 ? PickupKind.Energy : PickupKind.Repair);
    }

    private void SpawnPendingEnemy(PendingSpawn spawn)
    {
        Enemy? enemy = SpawnEnemy(spawn.Kind, RandomArenaEdge(), spawn.Polarity);
        if (enemy != null)
        {
            enemy.Value = (int)(enemy.Value * spawn.RewardBoost);
            if (spawn.Elite && enemy.Kind != EnemyKind.Boss)
            {
                enemy.Elite = true;
                enemy.Radius *= 1.1f;
                enemy.MaxHp *= 1.72f;
                enemy.Hp = enemy.MaxHp;
                enemy.Armor *= 1.12f;
                enemy.Value = Mathf.RoundToInt(enemy.Value * 1.65f);
                enemy.SpawnPulse = 1.0f;
            }
        }
    }

    private void SetupRunObjectives()
    {
        _runObjectives.Clear();

        int tier = ObjectiveTier();
        float rewardScale = ObjectiveRewardScale(tier);
        int nextMilestone = Mathf.Clamp(((_bestWave / 8) + 1) * 8, 8, TotalWaves);
        AddRunObjective(
            RunObjectiveKind.ReachWave,
            nextMilestone,
            ObjectiveReward(42 + nextMilestone * 3, rewardScale),
            "objective.route.title",
            "objective.route.body",
            DifficultyAccent(_selectedDifficulty).Lerp(CurrentSector().Accent, 0.16f),
            tier);

        if (_selectedDifficulty == GameDifficulty.Eclipse || _bestWave >= 20)
        {
            int comboTarget = TieredObjectiveTarget(tier, 24, 38, 56) + DifficultyObjectiveBonus(0, 5, 10);
            AddRunObjective(
                RunObjectiveKind.BestCombo,
                comboTarget,
                ObjectiveReward(72 + comboTarget, rewardScale),
                "objective.tempo.title",
                "objective.tempo.body",
                Gold,
                tier);
        }
        else
        {
            int perfectTarget = TieredObjectiveTarget(tier, 2, 3, 4);
            AddRunObjective(
                RunObjectiveKind.PerfectWaves,
                perfectTarget,
                ObjectiveReward(58 + perfectTarget * 24, rewardScale),
                "objective.clean.title",
                "objective.clean.body",
                Jade,
                tier);
        }

        AddPilotRunObjective(tier, rewardScale);
    }

    private int ObjectiveTier()
    {
        if (_bestWave >= 32)
        {
            return 2;
        }

        return _bestWave >= 16 ? 1 : 0;
    }

    private float ObjectiveRewardScale(int tier)
    {
        float difficultyScale = _selectedDifficulty switch
        {
            GameDifficulty.Storm => 1.22f,
            GameDifficulty.Eclipse => 1.5f,
            _ => 1.0f,
        };
        return difficultyScale * (1.0f + tier * 0.12f);
    }

    private int ObjectiveReward(int baseReward, float scale)
    {
        return Mathf.RoundToInt(baseReward * scale);
    }

    private static int TieredObjectiveTarget(int tier, int early, int mid, int late)
    {
        return tier switch
        {
            0 => early,
            1 => mid,
            _ => late,
        };
    }

    private int DifficultyObjectiveBonus(int cruise, int storm, int eclipse)
    {
        return _selectedDifficulty switch
        {
            GameDifficulty.Storm => storm,
            GameDifficulty.Eclipse => eclipse,
            _ => cruise,
        };
    }

    private void AddPilotRunObjective(int tier, float rewardScale)
    {
        switch (_selectedPilot)
        {
            case PilotKind.Vesper:
                AddRunObjective(RunObjectiveKind.CastTactical, TieredObjectiveTarget(tier, 5, 7, 9), ObjectiveReward(76 + tier * 26, rewardScale), "objective.pilot.vesper.title", "objective.pilot.vesper.body", PilotAccent(PilotKind.Vesper), tier);
                break;
            case PilotKind.Kairo:
                AddRunObjective(RunObjectiveKind.CastTactical, TieredObjectiveTarget(tier, 6, 9, 12), ObjectiveReward(72 + tier * 24 + MetaRank(MetaUpgradeId.SalvageRig) * 3, rewardScale), "objective.pilot.kairo.title", "objective.pilot.kairo.body", PilotAccent(PilotKind.Kairo), tier);
                break;
            case PilotKind.Sol:
                AddRunObjective(RunObjectiveKind.DefeatEnemies, TieredObjectiveTarget(tier, 110, 170, 240) + DifficultyObjectiveBonus(0, 20, 42), ObjectiveReward(72 + MetaRank(MetaUpgradeId.FocusLens) * 5, rewardScale), "objective.pilot.sol.title", "objective.pilot.sol.body", PilotAccent(PilotKind.Sol), tier);
                break;
            case PilotKind.Nyx:
                AddRunObjective(RunObjectiveKind.AbsorbBullets, TieredObjectiveTarget(tier, 58, 88, 126) + DifficultyObjectiveBonus(0, 10, 20), ObjectiveReward(78 + MetaRank(MetaUpgradeId.PolarityTuner) * 5, rewardScale), "objective.pilot.nyx.title", "objective.pilot.nyx.body", PilotAccent(PilotKind.Nyx), tier);
                break;
            case PilotKind.Rook:
                AddRunObjective(RunObjectiveKind.CastTactical, TieredObjectiveTarget(tier, 4, 6, 8), ObjectiveReward(78 + MetaRank(MetaUpgradeId.AegisMatrix) * 5, rewardScale), "objective.pilot.rook.title", "objective.pilot.rook.body", PilotAccent(PilotKind.Rook), tier);
                break;
            case PilotKind.Lyra:
                AddRunObjective(RunObjectiveKind.CollectPickups, TieredObjectiveTarget(tier, 130, 210, 300) + DifficultyObjectiveBonus(0, 24, 48), ObjectiveReward(74 + MetaRank(MetaUpgradeId.SalvageRig) * 5, rewardScale), "objective.pilot.lyra.title", "objective.pilot.lyra.body", PilotAccent(PilotKind.Lyra), tier);
                break;
            case PilotKind.Orion:
                AddRunObjective(RunObjectiveKind.DefeatBosses, TieredObjectiveTarget(tier, 1, 2, 3), ObjectiveReward(98 + MetaRank(MetaUpgradeId.FocusLens) * 5, rewardScale), "objective.pilot.orion.title", "objective.pilot.orion.body", PilotAccent(PilotKind.Orion), tier);
                break;
            default:
                AddRunObjective(RunObjectiveKind.AbsorbBullets, TieredObjectiveTarget(tier, 52, 82, 116) + DifficultyObjectiveBonus(0, 8, 18), ObjectiveReward(70 + MetaRank(MetaUpgradeId.PolarityTuner) * 5, rewardScale), "objective.pilot.astra.title", "objective.pilot.astra.body", PilotAccent(PilotKind.Astra), tier);
                break;
        }
    }

    private void AddRunObjective(RunObjectiveKind kind, int target, int rewardDust, string titleKey, string bodyKey, Color accent, int tier)
    {
        _runObjectives.Add(new RunObjective
        {
            Kind = kind,
            Target = Math.Max(1, target),
            RewardDust = Math.Max(1, rewardDust),
            TitleKey = titleKey,
            BodyKey = bodyKey,
            Accent = accent,
            Tier = tier,
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

        if (_wave < TotalWaves)
        {
            GrantWavePaceClearReward();
        }
    }

    private void GrantWavePaceClearReward()
    {
        Color accent = CurrentSector().Accent;
        int sector = CurrentSectorIndex();
        bool clean = !_waveTookDamage;

        switch (_currentWavePace)
        {
            case WavePaceKind.Swarm:
                _nextWaveRewardBoost = Mathf.Max(_nextWaveRewardBoost, 1.08f + _comboTier * 0.03f);
                for (int i = 0; i < 2 + Math.Min(3, _comboTier); i++)
                {
                    SpawnPickup(ScreenCenter + RandomDirection() * _rng.RandfRange(46.0f, 116.0f), PickupKind.Dust);
                }
                AddText(T("flow.reward.swarm"), ScreenCenter + new Vector2(0.0f, -82.0f), XpGray().Lerp(accent, 0.25f), 19.0f);
                break;
            case WavePaceKind.Elite:
                _nextWaveDamageBoost = Mathf.Max(_nextWaveDamageBoost, 1.12f + sector * 0.015f);
                _energy = Mathf.Clamp(_energy + 14.0f + sector * 2.0f, 0.0f, _maxEnergy);
                AddText(T("flow.reward.elite"), ScreenCenter + new Vector2(0.0f, -82.0f), Rose.Lerp(accent, 0.22f), 19.0f);
                break;
            case WavePaceKind.Recovery:
                _playerHp = Mathf.Clamp(_playerHp + 16.0f + sector * 3.0f, 0.0f, _playerMaxHp);
                _energy = Mathf.Clamp(_energy + 18.0f + sector * 3.0f, 0.0f, _maxEnergy);
                SpawnPickup(ScreenCenter + new Vector2(_rng.RandfRange(-88.0f, 88.0f), 42.0f), clean ? PickupKind.Repair : PickupKind.Energy);
                AddText(T("flow.reward.recovery"), ScreenCenter + new Vector2(0.0f, -82.0f), Jade, 19.0f);
                break;
            case WavePaceKind.Pressure:
                _nextWaveDamageBoost = Mathf.Max(_nextWaveDamageBoost, clean ? 1.18f : 1.1f);
                _energy = Mathf.Clamp(_energy + 8.0f + Math.Min(_combo, 60) * 0.22f, 0.0f, _maxEnergy);
                AddText(T("flow.reward.pressure"), ScreenCenter + new Vector2(0.0f, -82.0f), Gold, 19.0f);
                break;
            case WavePaceKind.Standard:
                if (clean && _combo >= 8)
                {
                    _energy = Mathf.Clamp(_energy + 8.0f, 0.0f, _maxEnergy);
                    _nextWaveRewardBoost = Mathf.Max(_nextWaveRewardBoost, 1.05f);
                    AddText(T("flow.reward.clean"), ScreenCenter + new Vector2(0.0f, -82.0f), Alpha(Jade, 0.88f), 18.0f);
                }
                break;
        }
    }

    private bool ShouldOpenUpgradeAfterWave()
    {
        if (_wave >= TotalWaves)
        {
            return false;
        }

        int waveInSector = CurrentWaveInSector();
        if (_wave <= 6 || waveInSector == 1 || waveInSector == WavesPerSector - 1 || waveInSector == WavesPerSector)
        {
            return true;
        }

        return waveInSector == 4 || _wave % 3 == 0;
    }

    private void GrantMomentumReward()
    {
        _combatChain = Math.Min(3, _combatChain + 1);
        float energyGain = 14.0f + _combatChain * 5.0f + CurrentSectorIndex() * 2.0f;
        _energy = Mathf.Clamp(_energy + energyGain, 0.0f, _maxEnergy);
        _cruiseCharge = Mathf.Clamp(_cruiseCharge + 16.0f + _combatChain * 8.0f, 0.0f, CruiseChargeMax);
        _playerHp = Mathf.Clamp(_playerHp + 3.0f + _combatChain * 2.0f, 0.0f, _playerMaxHp);
        _nextWaveDamageBoost = Mathf.Max(_nextWaveDamageBoost, 1.08f + _combatChain * 0.04f);
        AddText(T("flow.momentum"), ScreenCenter + new Vector2(0.0f, -118.0f), PilotAccent(_runPilot), 24.0f);
        Burst(ScreenCenter + new Vector2(0.0f, -84.0f), PilotAccent(_runPilot), 24 + _combatChain * 8, 320.0f, 0.82f);
        PlaySfx(390.0f + _combatChain * 45.0f, 110.0f, 0.18f, 0.22f, 0.02f, 1);
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

    private void IncreaseCombo(Vector2 source, int amount = 1)
    {
        if (amount <= 0)
        {
            return;
        }

        _combo += amount;
        _comboTimer = 2.4f;
        RefreshRunBestCombo();
        UpdateComboTier(source);
        AddComboPopText(source);
    }

    private void UpdateComboTier(Vector2 source)
    {
        int tier = ComboTierFor(_combo);
        if (tier <= _comboTier)
        {
            return;
        }

        _comboTier = tier;
        _comboTierPulse = 1.0f;
        float energyGain = 3.0f + tier * 2.0f;
        _energy = Mathf.Clamp(_energy + energyGain, 0.0f, _maxEnergy);
        _cruiseCharge = Mathf.Clamp(_cruiseCharge + 6.0f + tier * 4.0f, 0.0f, CruiseChargeMax);
        Color accent = tier >= 4 ? Gold : XpGreen.Lerp(Gold, tier * 0.18f);
        AddText(Tf("combo.surge", _combo), _playerPos + new Vector2(0.0f, -118.0f), accent, 21.0f + tier);
        if (_visualPressure < 0.82f)
        {
            Burst(source, accent, 10 + tier * 4, 300.0f + tier * 36.0f, 0.55f);
        }
        PlaySfx(360.0f + tier * 60.0f, 80.0f, 0.1f, 0.15f, 0.015f, 1);
    }

    private static int ComboTierFor(int combo)
    {
        return combo >= 100 ? 4 : combo >= 60 ? 3 : combo >= 30 ? 2 : combo >= 12 ? 1 : 0;
    }

    private void AddComboPopText(Vector2 source)
    {
        if (_combo <= 0 || _damageTexts.Count >= MaxDamageTexts)
        {
            return;
        }

        DamageText damageText = AddDamageTextObject();
        damageText.Text = Tf("combo.pop", _combo);
        Vector2 away = _playerPos - source;
        if (away.LengthSquared() < 0.01f)
        {
            away = RandomDirection();
        }
        Vector2 side = away.Normalized().Orthogonal() * _rng.RandfRange(-18.0f, 18.0f);
        damageText.Pos = _playerPos + new Vector2(0.0f, -76.0f) + away.Normalized() * 18.0f + side;
        damageText.Color = _combo >= 40 ? Gold : _combo >= 16 ? XpGreen : Alpha(Paper, 0.92f);
        damageText.Life = 0.68f;
        damageText.MaxLife = 0.68f;
        damageText.Size = Mathf.Clamp(20.0f + Mathf.Min(_combo, 80) * 0.14f, 20.0f, 32.0f);
        damageText.ComboPop = true;
    }

    private void AddRunScore(int amount, Vector2 source, Color accent, bool showPopup = false)
    {
        if (amount <= 0)
        {
            return;
        }

        _score += amount;
        if (showPopup && (_visualPressure < 0.74f || amount >= 600))
        {
            AddText($"+{amount}", source + new Vector2(0.0f, -38.0f), accent, amount >= 1000 ? 24.0f : 18.0f);
        }
    }

    private void CheckScoreCaches(Vector2 source, Color accent)
    {
        if (_nextScoreCache <= 0)
        {
            _nextScoreCache = ScoreCacheThreshold(_scoreCacheLevel);
        }

        int gainedDust = 0;
        int gainedCaches = 0;
        while (_score >= _nextScoreCache)
        {
            _scoreCacheLevel++;
            int dust = 3 + _scoreCacheLevel + CurrentSectorIndex();
            gainedDust += dust;
            gainedCaches++;
            _energy = Mathf.Clamp(_energy + 12.0f + _scoreCacheLevel * 2.0f, 0.0f, _maxEnergy);
            _cruiseCharge = Mathf.Clamp(_cruiseCharge + 16.0f + _scoreCacheLevel * 3.0f, 0.0f, CruiseChargeMax);
            _nextScoreCache = ScoreCacheThreshold(_scoreCacheLevel);
        }

        if (gainedDust <= 0)
        {
            return;
        }

        _runScoreBonusDust += gainedDust;
        _scoreCachePulse = 1.0f;
        Vector2 noticePos = source.DistanceSquaredTo(Vector2.Zero) > 0.01f ? source : ScreenCenter;
        AddText(Tf("score.cache", gainedDust), noticePos + new Vector2(0.0f, -72.0f), Gold, gainedCaches > 1 ? 28.0f : 24.0f);
        AddText(T("score.cache.hint"), ScreenCenter + new Vector2(0.0f, -112.0f), Alpha(Paper, 0.72f), 18.0f);
        Burst(noticePos, Gold, 24 + _scoreCacheLevel * 2, 360.0f, 0.82f);
        PlaySfx(560.0f + _scoreCacheLevel * 18.0f, 120.0f, 0.16f, 0.22f, 0.02f, 1);
    }

    private static int ScoreCacheThreshold(int level)
    {
        int rank = Math.Max(1, level + 1);
        return ScoreCacheBase * rank + ScoreCacheStep * rank * (rank - 1) / 2;
    }

    private int ScoreMultiplier()
    {
        return Math.Clamp(1 + Math.Min(_combo, 84) / 12, 1, 8);
    }

    private void AddExperience(int amount, Vector2 source)
    {
        if (amount <= 0)
        {
            return;
        }

        _xp += amount;
        _xpPulse = 1.0f;
        AddRunScore(amount * 14, source, XpGray());
        if (_visualPressure < 0.72f || amount >= 8)
        {
            AddText(Tf("xp.gain", amount), source + new Vector2(0.0f, -34.0f), Alpha(Paper, 0.78f), 16.0f);
        }

        while (_xp >= _xpToNext)
        {
            _xp -= _xpToNext;
            _runLevel++;
            _queuedLevelUps++;
            _xpToNext = XpThresholdForLevel(_runLevel);
            _energy = Mathf.Clamp(_energy + 12.0f + _runLevel * 1.5f, 0.0f, _maxEnergy);
            _cruiseCharge = Mathf.Clamp(_cruiseCharge + 18.0f, 0.0f, CruiseChargeMax);
        }

        if (_queuedLevelUps > 0 && _mode == GameMode.Playing)
        {
            OpenLevelUpChoice(source);
        }
    }

    private void OpenLevelUpChoice(Vector2 source)
    {
        _queuedLevelUps = Math.Max(0, _queuedLevelUps - 1);
        _combatChain = 0;
        OpenUpgradeChoice();
        AddText(T("xp.level_up"), ScreenCenter + new Vector2(0.0f, -168.0f), XpGray().Lerp(Paper, 0.22f), 36.0f);
        Burst(source, XpGray().Lerp(Paper, 0.2f), 44, 460.0f, 1.0f);
        PlaySfx(620.0f, 160.0f, 0.22f, 0.28f, 0.02f, 1);
    }

    private static int XpThresholdForLevel(int level)
    {
        int rank = Math.Max(1, level);
        return XpBase + (rank - 1) * XpStep + Math.Max(0, rank - 8) * 4;
    }

    private int XpPickupValue()
    {
        return 5 + CurrentSectorIndex() + (_currentWavePace == WavePaceKind.Elite ? 2 : 0) + (_currentWavePace == WavePaceKind.Boss ? 4 : 0);
    }

    private static Color XpGray()
    {
        return new Color(0.58f, 0.62f, 0.66f);
    }

    private Enemy? SpawnBoss()
    {
        int sector = CurrentSectorIndex();
        float threat = ThreatLevel();
        Enemy? boss = AddEnemy();
        if (boss == null)
        {
            return null;
        }

        BossArchetype archetype = ChooseBossArchetype(sector);
        boss.Kind = EnemyKind.Boss;
        boss.BossArchetype = archetype;
        boss.BossLastPattern = -1;
        boss.Pos = BossSpawnPosition(archetype, sector);
        boss.Vel = Vector2.Zero;
        boss.Radius = (82.0f + sector * 8.0f) * BossRadiusScale(archetype);
        boss.Hp = (3200.0f + sector * 1900.0f + threat * 120.0f) * BossHpScale(archetype) * DifficultyBossHpScale() * BossHpTrialScale;
        boss.MaxHp = boss.Hp;
        boss.Cooldown = BossOpeningCooldown(archetype) * DifficultyBossCooldownScale();
        boss.Overheat = 0.0f;
        boss.OverheatMax = 1.0f;
        boss.Phase = _rng.RandfRange(0.0f, Mathf.Tau);
        boss.SpawnPulse = 1.0f;
        boss.ContactTimer = 0.0f;
        boss.Polarity = _rng.RandiRange(0, 1);
        boss.Value = 8000 + sector * 5000;
        boss.SplitDepth = 0;
        boss.Elite = true;
        boss.Armor = (1.12f + sector * 0.1f) * BossArmorScale(archetype) * DifficultyBossArmorScale();
        boss.BossIntent = BossPatternKind.AimedFan;
        boss.BossIntentPulse = 0.0f;
        boss.BossPhase = 0;
        boss.BossGuard = 1.15f;
        Burst(boss.Pos, BossAccent(archetype), 100 + sector * 24, 760.0f + sector * 80.0f, 2.8f);
        _shake = 1.0f;
        return boss;
    }

    private BossArchetype ChooseBossArchetype(int sector)
    {
        BossArchetype[] pool = sector switch
        {
            0 => new[] { BossArchetype.Choir, BossArchetype.Prism },
            1 => new[] { BossArchetype.Choir, BossArchetype.Prism, BossArchetype.Mirror, BossArchetype.Tempest },
            2 => new[] { BossArchetype.Swarm, BossArchetype.Forge, BossArchetype.Mirror, BossArchetype.Bastion },
            3 => new[] { BossArchetype.Rift, BossArchetype.Tempest, BossArchetype.Serpent, BossArchetype.Bastion },
            _ => new[] { BossArchetype.Choir, BossArchetype.Forge, BossArchetype.Rift, BossArchetype.Serpent, BossArchetype.Oracle, BossArchetype.Tempest, BossArchetype.Bastion, BossArchetype.Mirror },
        };

        BossArchetype chosen = BossArchetype.Choir;
        for (int attempt = 0; attempt < 4; attempt++)
        {
            chosen = pool[_rng.RandiRange(0, pool.Length - 1)];
            if (!_hasLastBossArchetype || chosen != _lastBossArchetype)
            {
                break;
            }
        }

        _lastBossArchetype = chosen;
        _hasLastBossArchetype = true;
        return chosen;
    }

    private static Vector2 BossSpawnPosition(BossArchetype archetype, int sector)
    {
        float y = archetype switch
        {
            BossArchetype.Forge => Arena.Position.Y + 190.0f,
            BossArchetype.Swarm => Arena.Position.Y + 132.0f,
            BossArchetype.Rift => Arena.Position.Y + 165.0f,
            BossArchetype.Bastion => Arena.Position.Y + 205.0f,
            BossArchetype.Tempest => Arena.Position.Y + 118.0f,
            BossArchetype.Serpent => Arena.Position.Y + 152.0f,
            BossArchetype.Oracle => Arena.Position.Y + 136.0f,
            _ => Arena.Position.Y + 148.0f,
        };
        float x = ScreenWidth * 0.5f + (archetype == BossArchetype.Rift || archetype == BossArchetype.Serpent ? (sector % 2 == 0 ? -120.0f : 120.0f) : 0.0f);
        return new Vector2(x, y);
    }

    private static float BossHpScale(BossArchetype archetype)
    {
        return archetype switch
        {
            BossArchetype.Prism => 1.08f,
            BossArchetype.Swarm => 1.12f,
            BossArchetype.Forge => 1.28f,
            BossArchetype.Rift => 1.18f,
            BossArchetype.Mirror => 1.16f,
            BossArchetype.Tempest => 1.08f,
            BossArchetype.Bastion => 1.42f,
            BossArchetype.Serpent => 1.22f,
            BossArchetype.Oracle => 1.2f,
            _ => 1.14f,
        };
    }

    private static float BossRadiusScale(BossArchetype archetype)
    {
        return archetype switch
        {
            BossArchetype.Prism => 0.9f,
            BossArchetype.Swarm => 0.96f,
            BossArchetype.Forge => 1.1f,
            BossArchetype.Rift => 1.02f,
            BossArchetype.Mirror => 0.94f,
            BossArchetype.Tempest => 0.9f,
            BossArchetype.Bastion => 1.18f,
            BossArchetype.Serpent => 0.98f,
            BossArchetype.Oracle => 0.92f,
            _ => 1.0f,
        };
    }

    private static float BossArmorScale(BossArchetype archetype)
    {
        return archetype switch
        {
            BossArchetype.Prism => 1.02f,
            BossArchetype.Swarm => 1.04f,
            BossArchetype.Forge => 1.18f,
            BossArchetype.Rift => 1.1f,
            BossArchetype.Mirror => 1.04f,
            BossArchetype.Tempest => 0.98f,
            BossArchetype.Bastion => 1.28f,
            BossArchetype.Serpent => 1.08f,
            BossArchetype.Oracle => 1.06f,
            _ => 1.06f,
        };
    }

    private static float BossOpeningCooldown(BossArchetype archetype)
    {
        return archetype switch
        {
            BossArchetype.Prism => 0.72f,
            BossArchetype.Swarm => 0.82f,
            BossArchetype.Forge => 0.96f,
            BossArchetype.Rift => 0.78f,
            BossArchetype.Mirror => 0.7f,
            BossArchetype.Tempest => 0.66f,
            BossArchetype.Bastion => 1.08f,
            BossArchetype.Serpent => 0.8f,
            BossArchetype.Oracle => 0.9f,
            _ => 0.8f,
        };
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
        eliteChance += DifficultyIndex(_runDifficulty) * 0.04f;
        if (_currentWavePace == WavePaceKind.Elite)
        {
            eliteChance += 0.16f;
        }
        else if (_currentWavePace == WavePaceKind.Recovery)
        {
            eliteChance *= 0.35f;
        }

        bool elite = _rng.Randf() < eliteChance;
        if (elite)
        {
            radius *= 1.16f;
            hp *= 2.35f;
        }
        hp *= DifficultyEnemyHpScale();

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
        enemy.LastHitChainDepth = 0;
        enemy.LastHitSplitDepth = 0;
        enemy.Elite = elite;
        enemy.Armor = elite ? 1.18f + sector * 0.06f : 1.0f;
        enemy.DashCooldown = InitialEnemyDashCooldown(kind, sector, CurrentWaveInSector());
        enemy.DashWarmup = 0.0f;
        enemy.DashTime = 0.0f;
        enemy.DashDir = Vector2.Zero;
        enemy.BossPhase = 0;
        enemy.BossGuard = 0.0f;
        if (kind == EnemyKind.Bulwark)
        {
            enemy.Armor += 0.22f;
        }
        enemy.Armor *= DifficultyEnemyArmorScale();
        Burst(pos, PolarityColor(polarity), 12, 220.0f, 0.9f);
        return enemy;
    }

    private void UpdateTitle(float dt)
    {
        UpdateParticles(dt);
        UpdateDamageTexts(dt);
        int navX = ConsumeGamepadNavX();
        int navY = ConsumeGamepadNavY();
        int keyboardPilotNav = ConsumeTitleKeyboardPilotNavX();
        if (keyboardPilotNav != 0)
        {
            CycleTitlePilot(keyboardPilotNav);
        }

        if (navY != 0)
        {
            _gamepadTitleIndex = MoveTitleFocusVertical(_gamepadTitleIndex, navY);
            PlaySfx(260.0f + _gamepadTitleIndex * 16.0f, -16.0f, 0.07f, 0.12f, 0.01f, 1);
        }
        if (navX != 0)
        {
            if (IsTitlePilotFocus(_gamepadTitleIndex))
            {
                CycleTitlePilot(navX);
            }
            else
            {
                _gamepadTitleIndex = MoveTitleFocusHorizontal(_gamepadTitleIndex, navX);
                PlaySfx(260.0f + _gamepadTitleIndex * 16.0f, 20.0f, 0.07f, 0.12f, 0.01f, 1);
            }
        }
        _gamepadTitleIndex = ClampTitleFocus(_gamepadTitleIndex);
        if (IsTitlePilotFocus(_gamepadTitleIndex))
        {
            _gamepadPilotIndex = Mathf.Clamp(_gamepadPilotIndex, 0, PilotCount() - 1);
        }
        SetGamepadFocus(TitleFocusRect(_gamepadTitleIndex));

        Vector2 mouse = GetGlobalMousePosition();
        bool startKey = StartHeld();
        bool metaKey = MetaHeld();
        bool settingsKey = SettingsShortcutHeld();
        bool gamepadConfirm = JoyButtonHeld(JoyButton.A);
        bool click = Input.IsMouseButtonPressed(MouseButton.Left) && !_lastClick;
        if (click)
        {
            _usingGamepad = false;
            if (PilotPreviousButtonRect().HasPoint(mouse))
            {
                CycleTitlePilot(-1);
                return;
            }
            if (PilotNextButtonRect().HasPoint(mouse))
            {
                CycleTitlePilot(1);
                return;
            }
            if (PilotCardRect(0).HasPoint(mouse))
            {
                _gamepadTitleIndex = TitlePilotFocusStart;
                SelectDisplayedPilotOrShowLock(false);
                return;
            }

            if (TrySelectDifficulty(mouse))
            {
                return;
            }
        }

        if (gamepadConfirm && !_lastConfirm)
        {
            ActivateTitleFocus(_gamepadTitleIndex);
            return;
        }

        if ((startKey && !_lastStart) || (click && StartButtonRect().HasPoint(mouse)))
        {
            TryStartTitleRun();
        }
        else if (click && GmUnlockButtonRect().HasPoint(mouse))
        {
            UnlockAllForTesting();
        }
        else if ((metaKey && !_lastMeta) || (click && MetaButtonRect().HasPoint(mouse)))
        {
            _mode = GameMode.Meta;
            PlaySfx(360.0f, 80.0f, 0.18f, 0.2f, 0.02f, 1);
        }
        else if ((settingsKey && !_lastSettingsShortcut) || (click && TitleSettingsButtonRect().HasPoint(mouse)))
        {
            OpenSettings(GameMode.Title);
        }
    }

    private int ConsumeTitleKeyboardPilotNavX()
    {
        bool left = KeyDown(Key.Left) || KeyDown(Key.A);
        bool right = KeyDown(Key.Right) || KeyDown(Key.D);
        int nav = 0;
        if (left && !_lastTitleLeft)
        {
            nav = -1;
        }
        else if (right && !_lastTitleRight)
        {
            nav = 1;
        }

        if (nav != 0)
        {
            _usingGamepad = false;
        }

        return nav;
    }

    private PilotKind TitleDisplayedPilot()
    {
        _gamepadPilotIndex = Mathf.Clamp(_gamepadPilotIndex, 0, PilotCount() - 1);
        return PilotFromIndex(_gamepadPilotIndex);
    }

    private void CycleTitlePilot(int direction)
    {
        if (direction == 0)
        {
            return;
        }

        _gamepadPilotIndex = WrapIndex(_gamepadPilotIndex + direction, PilotCount());
        _gamepadTitleIndex = TitlePilotFocusStart;
        PilotKind pilot = TitleDisplayedPilot();
        if (IsPilotUnlocked(pilot))
        {
            _selectedPilot = pilot;
            SaveMetaProgress();
            PlaySfx(360.0f + _gamepadPilotIndex * 34.0f, 52.0f, 0.1f, 0.16f, 0.01f, 1);
        }
        else
        {
            PlaySfx(150.0f, -18.0f, 0.1f, 0.1f, 0.02f, 0);
        }
    }

    private bool SelectDisplayedPilotOrShowLock(bool startIfAlreadySelected)
    {
        PilotKind pilot = TitleDisplayedPilot();
        if (!IsPilotUnlocked(pilot))
        {
            AddText(PilotUnlockText(pilot), ScreenCenter + new Vector2(0.0f, 210.0f), Rose, 22.0f);
            PlaySfx(120.0f, -20.0f, 0.12f, 0.16f, 0.05f, 0);
            return false;
        }

        bool samePilot = _selectedPilot == pilot;
        _selectedPilot = pilot;
        SaveMetaProgress();
        PlaySfx(360.0f + _gamepadPilotIndex * 55.0f, 70.0f, 0.14f, 0.2f, 0.02f, 1);
        if (samePilot && startIfAlreadySelected)
        {
            StartRun();
        }

        return true;
    }

    private void TryStartTitleRun()
    {
        if (SelectDisplayedPilotOrShowLock(false))
        {
            StartRun();
        }
    }

    private bool TrySelectDifficulty(Vector2 mouse)
    {
        for (int i = 0; i < DifficultyCount; i++)
        {
            GameDifficulty difficulty = DifficultyFromIndex(i);
            if (!DifficultyButtonRect(i).HasPoint(mouse))
            {
                continue;
            }

            _gamepadTitleIndex = TitleDifficultyFocusStart + i;
            SelectDifficulty(difficulty);
            return true;
        }

        return false;
    }

    private void SelectDifficulty(GameDifficulty difficulty)
    {
        if (!IsDifficultyUnlocked(difficulty))
        {
            AddText(DifficultyUnlockText(difficulty), ScreenCenter + new Vector2(0.0f, 220.0f), Rose, 22.0f);
            PlaySfx(120.0f, -20.0f, 0.12f, 0.16f, 0.05f, 0);
            return;
        }

        _selectedDifficulty = difficulty;
        _runDifficulty = difficulty;
        SaveMetaProgress();
        PlaySfx(310.0f + DifficultyIndex(difficulty) * 92.0f, 60.0f, 0.12f, 0.16f, 0.02f, 1);
    }

    private void ActivateTitleFocus(int focus)
    {
        focus = ClampTitleFocus(focus);
        if (IsTitleDifficultyFocus(focus))
        {
            SelectDifficulty(DifficultyFromIndex(focus - TitleDifficultyFocusStart));
            return;
        }

        if (IsTitlePilotFocus(focus))
        {
            SelectDisplayedPilotOrShowLock(true);
            return;
        }

        if (focus == TitleMetaFocus)
        {
            _mode = GameMode.Meta;
            PlaySfx(360.0f, 80.0f, 0.18f, 0.2f, 0.02f, 1);
        }
        else if (focus == TitleStartFocus)
        {
            TryStartTitleRun();
        }
        else if (focus == TitleSettingsFocus)
        {
            OpenSettings(GameMode.Title);
        }
    }

    private static int MoveTitleFocusHorizontal(int focus, int direction)
    {
        if (IsTitleDifficultyFocus(focus))
        {
            int index = focus - TitleDifficultyFocusStart;
            return TitleDifficultyFocusStart + WrapIndex(index + direction, DifficultyCount);
        }

        if (IsTitlePilotFocus(focus))
        {
            return TitlePilotFocusStart;
        }

        if (IsTitleFooterFocus(focus))
        {
            int footer = focus - TitleFooterFocusStart;
            return TitleFooterFocusStart + WrapIndex(footer + direction, 3);
        }

        return TitlePilotFocusStart;
    }

    private int MoveTitleFocusVertical(int focus, int direction)
    {
        if (direction < 0)
        {
            if (IsTitleDifficultyFocus(focus))
            {
                return TitleStartFocus;
            }
            if (IsTitlePilotFocus(focus))
            {
                return TitleDifficultyFocusStart + DifficultyIndex(_selectedDifficulty);
            }
            if (IsTitleFooterFocus(focus))
            {
                return TitlePilotFocusStart;
            }
        }
        else if (direction > 0)
        {
            if (IsTitleDifficultyFocus(focus))
            {
                return TitlePilotFocusStart;
            }
            if (IsTitlePilotFocus(focus))
            {
                return TitleStartFocus;
            }
            if (IsTitleFooterFocus(focus))
            {
                int footer = focus - TitleFooterFocusStart;
                return TitleDifficultyFocusStart + Math.Min(footer, DifficultyCount - 1);
            }
        }

        return ClampTitleFocus(focus);
    }

    private static int ClampTitleFocus(int focus)
    {
        return Mathf.Clamp(focus, TitleDifficultyFocusStart, TitleSettingsFocus);
    }

    private static bool IsTitleDifficultyFocus(int focus)
    {
        return focus >= TitleDifficultyFocusStart && focus < TitlePilotFocusStart;
    }

    private static bool IsTitlePilotFocus(int focus)
    {
        return focus >= TitlePilotFocusStart && focus < TitleFooterFocusStart;
    }

    private static bool IsTitleFooterFocus(int focus)
    {
        return focus >= TitleFooterFocusStart && focus <= TitleSettingsFocus;
    }

    private static Rect2 TitleFocusRect(int focus)
    {
        focus = ClampTitleFocus(focus);
        if (IsTitleDifficultyFocus(focus))
        {
            return DifficultyButtonRect(focus - TitleDifficultyFocusStart);
        }
        if (IsTitlePilotFocus(focus))
        {
            return PilotCardRect(focus - TitlePilotFocusStart);
        }
        return focus switch
        {
            TitleMetaFocus => MetaButtonRect(),
            TitleSettingsFocus => TitleSettingsButtonRect(),
            _ => StartButtonRect(),
        };
    }

    private void UpdateSettings(float dt)
    {
        UpdateParticles(dt);
        UpdateDamageTexts(dt);
        _deleteSaveConfirmTimer = Mathf.Max(0.0f, _deleteSaveConfirmTimer - dt);
        _deleteSaveNoticeTimer = Mathf.Max(0.0f, _deleteSaveNoticeTimer - dt);
        int optionCount = SettingsOptionCount();
        _gamepadSettingsIndex = Mathf.Clamp(_gamepadSettingsIndex, 0, optionCount - 1);
        int nav = ConsumeGamepadNavY();
        if (nav != 0)
        {
            _gamepadSettingsIndex = (_gamepadSettingsIndex + nav + optionCount) % optionCount;
            PlaySfx(240.0f, 20.0f, 0.06f, 0.1f, 0.01f, 1);
        }
        SetGamepadFocus(SettingsOptionRect(_gamepadSettingsIndex));
        int horizontalNav = ConsumeSettingsHorizontalNav();
        if (horizontalNav != 0 && IsAdjustableSettingsOption(_gamepadSettingsIndex))
        {
            AdjustSettingsOption(_gamepadSettingsIndex, horizontalNav);
            return;
        }

        Vector2 mouse = GetGlobalMousePosition();
        bool click = Input.IsMouseButtonPressed(MouseButton.Left) && !_lastClick;
        if (click)
        {
            _usingGamepad = false;
        }

        if ((CancelHeld() && !_lastCancel) || (PauseHeld() && !_lastPause) || (click && SettingsBackButtonRect().HasPoint(mouse)))
        {
            _deleteSaveConfirmTimer = 0.0f;
            CloseSettings();
            return;
        }

        if (ConfirmHeld() && !_lastConfirm)
        {
            ActivateSettingsOption(_gamepadSettingsIndex);
            return;
        }

        for (int i = 0; i < optionCount; i++)
        {
            if (click && SettingsOptionRect(i).HasPoint(mouse))
            {
                _gamepadSettingsIndex = i;
                if (IsAdjustableSettingsOption(i))
                {
                    Rect2 rect = SettingsOptionRect(i);
                    int direction = mouse.X < rect.Position.X + rect.Size.X * 0.5f ? -1 : 1;
                    AdjustSettingsOption(i, direction);
                }
                else
                {
                    ActivateSettingsOption(i);
                }
                return;
            }
        }
    }

    private int SettingsOptionCount()
    {
        return IsRunViewMode(_settingsReturnMode) ? 9 : 8;
    }

    private Rect2 SettingsOptionRect(int index)
    {
        return new Rect2(new Vector2(680.0f, 286.0f + index * 62.0f), new Vector2(560.0f, 50.0f));
    }

    private bool IsAdjustableSettingsOption(int index)
    {
        return index >= 1 && index <= 5;
    }

    private int ConsumeSettingsHorizontalNav()
    {
        int nav = ConsumeGamepadNavX();
        if (nav != 0)
        {
            return nav;
        }

        bool left = KeyDown(Key.Left) || KeyDown(Key.A);
        bool right = KeyDown(Key.Right) || KeyDown(Key.D);
        if (left && !_lastSettingsLeft)
        {
            return -1;
        }
        if (right && !_lastSettingsRight)
        {
            return 1;
        }

        return 0;
    }

    private void AdjustSettingsOption(int index, int direction)
    {
        _deleteSaveConfirmTimer = 0.0f;
        switch (index)
        {
            case 1:
                _musicVolume = Mathf.Clamp(_musicVolume + direction * 0.1f, 0.0f, 1.0f);
                break;
            case 2:
                _sfxVolume = Mathf.Clamp(_sfxVolume + direction * 0.1f, 0.0f, 1.0f);
                break;
            case 3:
                _language = LanguageCycle[WrapIndex(LanguageCycleIndex(_language) + direction, LanguageCycle.Length)];
                RefreshUpgradeChoiceText();
                ApplyWindowTitle();
                break;
            case 4:
                _resolutionPreset = (DisplayResolutionPreset)WrapIndex((int)_resolutionPreset + direction, ResolutionPresetCount());
                break;
            case 5:
                _visualQuality = (VisualQuality)WrapIndex((int)_visualQuality + direction, VisualQualityCount());
                break;
            default:
                return;
        }

        if (index == 4)
        {
            ApplyWindowResolution();
        }
        else if (index == 5)
        {
            GenerateBackdrop();
        }
        SaveMetaProgress();
        PlaySfx(460.0f, direction >= 0 ? 70.0f : -70.0f, 0.1f, 0.16f, 0.02f, 1);
    }

    private void ActivateSettingsOption(int index)
    {
        bool runSettings = IsRunViewMode(_settingsReturnMode);
        int mainMenuIndex = runSettings ? 6 : -1;
        int deleteIndex = runSettings ? 7 : 6;
        int backIndex = runSettings ? 8 : 7;

        if (index == 0)
        {
            _deleteSaveConfirmTimer = 0.0f;
            _guidePage = 0;
            _mode = GameMode.Guide;
            PlaySfx(420.0f, 120.0f, 0.16f, 0.2f, 0.02f, 1);
            return;
        }

        if (IsAdjustableSettingsOption(index))
        {
            AdjustSettingsOption(index, 1);
            return;
        }

        if (index == mainMenuIndex)
        {
            _deleteSaveConfirmTimer = 0.0f;
            ResetTitle();
            PlaySfx(180.0f, -80.0f, 0.18f, 0.2f, 0.04f, 1);
            return;
        }

        if (index == deleteIndex)
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

        if (index == backIndex)
        {
            _deleteSaveConfirmTimer = 0.0f;
            CloseSettings();
        }
    }

    private void UpdateGuide(float dt)
    {
        UpdateParticles(dt);
        UpdateDamageTexts(dt);
        SetGamepadFocus(GuideBackButtonRect());
        int nav = ConsumeSettingsHorizontalNav();
        if (nav != 0)
        {
            _guidePage = WrapIndex(_guidePage + nav, GuidePageCount());
            PlaySfx(340.0f, nav > 0 ? 70.0f : -70.0f, 0.08f, 0.13f, 0.01f, 1);
            return;
        }

        Vector2 mouse = GetGlobalMousePosition();
        bool click = Input.IsMouseButtonPressed(MouseButton.Left) && !_lastClick;
        if (click)
        {
            _usingGamepad = false;
            for (int i = 0; i < GuidePageCount(); i++)
            {
                if (GuideTabRect(i).HasPoint(mouse))
                {
                    _guidePage = i;
                    PlaySfx(340.0f, 70.0f, 0.08f, 0.13f, 0.01f, 1);
                    return;
                }
            }
        }

        if ((CancelHeld() && !_lastCancel) || (PauseHeld() && !_lastPause) || (ConfirmHeld() && !_lastConfirm) || (click && GuideBackButtonRect().HasPoint(mouse)))
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
        _gamepadSettingsIndex = 0;
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
        _gamepadMetaIndex = Mathf.Clamp(_gamepadMetaIndex, 0, MetaUpgrades.Length - 1);
        int navX = ConsumeGamepadNavX();
        int navY = ConsumeGamepadNavY();
        if (navX != 0 || navY != 0)
        {
            int row = _gamepadMetaIndex / 4;
            int column = _gamepadMetaIndex % 4;
            column = (column + navX + 4) % 4;
            row = Mathf.Clamp(row + navY, 0, (MetaUpgrades.Length - 1) / 4);
            _gamepadMetaIndex = Mathf.Clamp(row * 4 + column, 0, MetaUpgrades.Length - 1);
            PlaySfx(230.0f + column * 18.0f, 18.0f, 0.055f, 0.1f, 0.01f, 1);
        }
        SetGamepadFocus(MetaUpgradeRect(_gamepadMetaIndex));

        Vector2 mouse = GetGlobalMousePosition();
        bool click = Input.IsMouseButtonPressed(MouseButton.Left) && !_lastClick;
        if (click)
        {
            _usingGamepad = false;
        }

        if ((CancelHeld() && !_lastCancel) || (click && MetaBackButtonRect().HasPoint(mouse)))
        {
            ResetTitle();
            return;
        }

        if (ConfirmHeld() && !_lastConfirm)
        {
            TryBuyMetaUpgrade(MetaUpgrades[_gamepadMetaIndex]);
            return;
        }

        for (int i = 0; i < MetaUpgrades.Length; i++)
        {
            if (MetaHotkeyPressed(i) || (click && MetaUpgradeRect(i).HasPoint(mouse)))
            {
                _gamepadMetaIndex = i;
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
        if (click)
        {
            _usingGamepad = false;
        }
        if ((click && HudSettingsButtonRect().HasPoint(mouse)) || (PauseHeld() && !_lastPause))
        {
            OpenSettings(GameMode.Playing);
            return;
        }

        float gameDt = dt * _slowMo;
        _slowMo = Approach(_slowMo, 1.0f, dt * 1.5f);
        _runTimer += dt;
        _energy = Mathf.Clamp(_energy + dt * 3.0f, 0.0f, _maxEnergy);
        _fireTimer -= gameDt;
        _dashTimer -= gameDt;
        _dashCooldown -= gameDt;
        _invulnTimer -= gameDt;
        _polarityCooldown = Mathf.Max(0.0f, _polarityCooldown - gameDt);
        _ultimateCooldown = Mathf.Max(0.0f, _ultimateCooldown - gameDt);
        _polarityDenyTextCooldown -= gameDt;
        _absorbTextCooldown -= gameDt;
        _counterTextCooldown -= gameDt;
        _polarityTipTimer -= gameDt;
        _scoreCachePulse = Mathf.Max(0.0f, _scoreCachePulse - gameDt * 2.6f);
        _xpPulse = Mathf.Max(0.0f, _xpPulse - gameDt * 3.2f);
        _comboTierPulse = Mathf.Max(0.0f, _comboTierPulse - gameDt * 1.9f);
        _waveIntelPulse = Mathf.Max(0.0f, _waveIntelPulse - gameDt * 0.72f);
        _assaultBurstTimer = Mathf.Max(0.0f, _assaultBurstTimer - gameDt);
        _cruiseCharge = Mathf.Max(0.0f, _cruiseCharge - gameDt * 0.45f);
        _timeSinceHit += gameDt;
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

        if (_mode != GameMode.Playing)
        {
            return;
        }

        if (_enemies.Count == 0 && _pendingSpawns.Count == 0 && WaveProgressComplete())
        {
            _waveClearTimer += gameDt;
            float clearDelay = Mathf.Lerp(1.0f, 0.28f, ComboPace01());
            if (_waveClearTimer > clearDelay)
            {
                _nextWaveDamageBoost = 1.0f;
                OnWaveCleared();
                if (_wave >= TotalWaves)
                {
                    WinRun();
                    return;
                }
                if (_mode != GameMode.Playing)
                {
                    return;
                }
                GrantMomentumReward();
                if (_mode != GameMode.Playing)
                {
                    return;
                }
                BeginNextWave();
            }
        }
    }

    private void UpdatePlayer(float dt)
    {
        Vector2 move = ReadMoveInput();

        Vector2 gamepadAim = ReadGamepadStick(JoyAxis.RightX, JoyAxis.RightY, GamepadAimDeadZone);
        if (gamepadAim.LengthSquared() > 0.01f)
        {
            _aimDir = gamepadAim.Normalized();
        }
        else if (_usingGamepad && move.LengthSquared() > 0.01f)
        {
            _aimDir = move.Normalized();
        }
        else if (!_usingGamepad)
        {
            Vector2 mouse = GetGlobalMousePosition();
            Vector2 aim = mouse - _playerPos;
            if (aim.LengthSquared() > 0.01f)
            {
                _aimDir = aim.Normalized();
            }
        }

        bool tactical = TacticalHeld();
        if (tactical && !_lastToggle)
        {
            if (_polarityCooldown <= 0.0f)
            {
                CastTacticalSkill();
            }
            else if (_polarityDenyTextCooldown <= 0.0f)
            {
                AddText(PolarityCooldownText(), _playerPos + new Vector2(0.0f, -88.0f), Alpha(Paper, 0.76f), 18.0f);
                _polarityDenyTextCooldown = 0.45f;
                PlaySfx(180.0f, -90.0f, 0.08f, 0.12f, 0.02f, 1);
            }
        }

        bool nova = UltimateHeld();
        if (nova && !_lastNova && _energy >= _novaCost && _ultimateCooldown <= 0.0f)
        {
            CastUltimate();
        }
        else if (nova && !_lastNova)
        {
            string message = _ultimateCooldown > 0.0f ? Tf("ultimate.cooldown", Mathf.CeilToInt(_ultimateCooldown)) : Tf("ultimate.need_energy", Mathf.CeilToInt(_novaCost));
            AddText(message, _playerPos + new Vector2(0.0f, -96.0f), Alpha(Paper, 0.72f), 18.0f);
            PlaySfx(160.0f, -60.0f, 0.08f, 0.12f, 0.02f, 1);
        }

        bool dash = DashHeld();
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
            FirePulseMagazine();
            _fireTimer = _fireInterval;
        }

        if (_playerHp <= 0.0f)
        {
            LoseRun();
        }
    }

    private void CastTacticalSkill()
    {
        float focus01 = CruiseCharge01();
        Color color = PilotAccent(_runPilot);
        _playerPolarity = CruiseStance;
        _polarityCooldown = _polarityCooldownMax;
        _polarityTipTimer = 2.35f;
        _energy = Mathf.Clamp(_energy + (4.0f + focus01 * 8.0f) * _absorbEfficiency, 0.0f, _maxEnergy);
        _assaultPower = 1.0f;
        _assaultBurstTimer = 0.0f;
        _cruiseCharge = 0.0f;

        switch (_runPilot)
        {
            case PilotKind.Vesper:
                CastVesperTactical(focus01, color);
                break;
            case PilotKind.Kairo:
                CastKairoTactical(focus01, color);
                break;
            case PilotKind.Sol:
                CastSolTactical(focus01, color);
                break;
            case PilotKind.Nyx:
                CastNyxTactical(focus01, color);
                break;
            case PilotKind.Rook:
                CastRookTactical(focus01, color);
                break;
            case PilotKind.Lyra:
                CastLyraTactical(focus01, color);
                break;
            case PilotKind.Orion:
                CastOrionTactical(focus01, color);
                break;
            default:
                CastAstraTactical(focus01, color);
                break;
        }

        if (_polarityStorm > 0)
        {
            FirePolarityStorm();
        }

        AddObjectiveProgress(RunObjectiveKind.CastTactical, 1);
        Burst(_playerPos, color, 28 + Mathf.RoundToInt(focus01 * 18.0f), 410.0f, 0.78f);
        AddText(TacticalTipText(), _playerPos + new Vector2(0.0f, -92.0f), color, 22.0f);
        PlaySfx(430.0f + focus01 * 120.0f, 0.55f, 0.16f, 0.26f, 0.03f, 2);
    }

    private void CastAstraTactical(float focus01, Color color)
    {
        Vector2 dir = _aimDir.LengthSquared() < 0.01f ? Vector2.Right : _aimDir.Normalized();
        Vector2 start = _playerPos - dir * 26.0f;
        Vector2 end = _playerPos + dir * (720.0f + focus01 * 240.0f + _astraNovaBloom * 42.0f);
        _fireTimer = 0.0f;
        _assaultPower = 1.18f + focus01 * 0.26f + _astraWake * 0.035f;
        _assaultBurstTimer = 1.28f + focus01 * 0.72f + _astraWake * 0.1f;
        ClearBulletsNear(_playerPos, 82.0f + focus01 * 72.0f, true);
        ClearEnemyBulletsInBeam(start, end, 42.0f + focus01 * 48.0f, true);
        if (IsUpgradeMaxed(UpgradeId.AstraTwinRefraction))
        {
            Vector2 right = new(-dir.Y, dir.X);
            DamageVesperBeam(_playerPos - right * 560.0f, _playerPos + right * 560.0f, 22.0f + focus01 * 20.0f, 26.0f + focus01 * 28.0f + _astraTwinRefraction * 8.0f, color);
        }

        int count = 3 + _astraRefraction + _astraTwinRefraction + Mathf.RoundToInt(focus01 * 2.0f);
        float spread = 0.18f + _astraTwinRefraction * 0.035f;
        for (int i = 0; i < count; i++)
        {
            float offset = count == 1 ? 0.0f : (i - (count - 1) * 0.5f) * spread / Math.Max(1, count - 1);
            Vector2 shotDir = dir.Rotated(offset);
            SpawnPlayerShot(_playerPos + shotDir * 42.0f, shotDir, 1460.0f, 5.0f, (22.0f + _astraWake * 2.8f + focus01 * 18.0f) * _damageMultiplier, 0.78f, 1 + _astraTwinRefraction / 2, true);
        }
    }

    private void CastVesperTactical(float focus01, Color color)
    {
        Vector2 dir = _aimDir.LengthSquared() < 0.01f ? Vector2.Right : _aimDir.Normalized();
        Vector2 start = _playerPos - dir * 44.0f;
        Vector2 end = _playerPos + dir * (1040.0f + focus01 * 260.0f);
        float width = 12.0f + _vesperCharge * 1.6f + _vesperJudgmentCoil * 2.2f + focus01 * 8.0f;
        float damage = 32.0f + _vesperCharge * 6.5f + _vesperJudgmentCoil * 12.0f + focus01 * 34.0f;
        _assaultPower = 1.04f + focus01 * 0.08f;
        _assaultBurstTimer = 0.72f + focus01 * 0.28f;
        DamageVesperBeam(start, end, width, damage, color);
        if (IsUpgradeMaxed(UpgradeId.VesperJudgmentCoil))
        {
            BreakBossGuardInBeam(start, end, width + 42.0f, 0.42f, color);
        }
        LockEnemiesInBeam(start, end, width + 20.0f, 0.54f + focus01 * 0.5f, color);

        if (_vesperFork + _vesperSeverLine > 0)
        {
            Vector2 right = new(-dir.Y, dir.X);
            int lanes = Math.Min(3, _vesperFork + _vesperSeverLine);
            for (int lane = 1; lane <= lanes; lane++)
            {
                float offset = 38.0f + lane * 24.0f;
                DamageVesperBeam(start + right * offset, end + right * offset, width * 0.38f, damage * 0.28f, color);
                DamageVesperBeam(start - right * offset, end - right * offset, width * 0.38f, damage * 0.28f, color);
            }
        }

        _fireTimer = Math.Min(_fireTimer, 0.05f);
    }

    private void CastKairoTactical(float focus01, Color color)
    {
        int droneCount = Math.Min(9, Math.Max(_orbiters, 3 + _kairoDroneBay + _kairoOverrideMatrix));
        _assaultPower = 1.08f + focus01 * 0.12f + _kairoSync * 0.018f;
        _assaultBurstTimer = 1.05f + focus01 * 0.5f + _kairoRelayProtocol * 0.1f;
        ClearBulletsNear(_playerPos, 112.0f + droneCount * 8.0f + focus01 * 64.0f, true);

        for (int i = 0; i < droneCount; i++)
        {
            float angle = i * Mathf.Tau / droneCount + _time * 0.24f;
            Vector2 origin = _playerPos + Vector2.Right.Rotated(angle) * (70.0f + focus01 * 28.0f);
            ClearBulletsNear(origin, 24.0f + focus01 * 14.0f, false);
            Enemy? target = FindNearestEnemy(origin, 1020.0f);
            if (target != null)
            {
                FireOrbiterShot(origin, target);
            }
            else
            {
                SpawnPlayerShot(origin, Vector2.Right.Rotated(angle), 980.0f, 4.2f, (9.0f + _kairoSync * 1.8f + focus01 * 5.0f) * _damageMultiplier, 0.72f, _kairoRelayProtocol >= 2 ? 1 : 0, false);
            }
            AddParticle(origin, (origin - _playerPos).Normalized() * 150.0f, color, 7.0f, 0.22f);
            AddDroneCommandCue(origin, origin - _playerPos, color, 0.78f + focus01 * 0.18f);
        }

        _orbiterFireTimer = 0.0f;
        _energy = Mathf.Clamp(_energy + _kairoRelayProtocol * (IsUpgradeMaxed(UpgradeId.KairoRelayProtocol) ? 7.0f : 4.0f), 0.0f, _maxEnergy);
    }

    private void CastSolTactical(float focus01, Color color)
    {
        float radius = 170.0f + _solBloom * 18.0f + _solFlareCore * 28.0f + focus01 * 120.0f;
        ClearBulletsNear(_playerPos, radius, true);
        _invulnTimer = Mathf.Max(_invulnTimer, 0.22f + _solRadiantMantle * 0.11f);
        _playerHp = Mathf.Clamp(_playerHp + 8.0f + _solForge * 2.4f + _solRadiantMantle * 5.5f + focus01 * 16.0f, 0.0f, _playerMaxHp);
        _assaultPower = 1.0f;
        _assaultBurstTimer = 0.36f + focus01 * 0.28f;
        if (IsUpgradeMaxed(UpgradeId.SolRadiantMantle))
        {
            _invulnTimer = Mathf.Max(_invulnTimer, 0.72f + focus01 * 0.35f);
            _playerHp = Mathf.Clamp(_playerHp + 10.0f, 0.0f, _playerMaxHp);
        }

        for (int i = _enemies.Count - 1; i >= 0; i--)
        {
            Enemy enemy = _enemies[i];
            float distance = enemy.Pos.DistanceTo(_playerPos);
            if (distance < radius + enemy.Radius)
            {
                float damage = Mathf.Lerp(54.0f + _solForge * 5.0f + _solFlareCore * 7.0f, 10.0f, distance / Mathf.Max(1.0f, radius)) * _damageMultiplier;
                enemy.Vel += (enemy.Pos - _playerPos).Normalized() * (120.0f + focus01 * 90.0f);
                DamageEnemy(enemy, damage, _playerPos, false);
            }
        }
        if (IsUpgradeMaxed(UpgradeId.SolFlareCore))
        {
            SolFlareCapstonePulse(_playerPos, radius, color);
        }
    }

    private void CastUltimate()
    {
        _energy -= _novaCost;
        _ultimateCooldown = UltimateCooldownBase;
        _slowMo = 0.68f;
        _shake = 0.62f;
        _flash = 0.46f;
        Color color = PilotAccent(_runPilot);
        AddText(UltimateName(_runPilot), _playerPos + new Vector2(0.0f, -104.0f), PilotAccent(_runPilot), 30.0f);
        CastUnifiedBulletClearUltimate(color);
        AddObjectiveProgress(RunObjectiveKind.CastUltimate, 1);
    }

    private void CastUnifiedBulletClearUltimate(Color color)
    {
        float radius = 420.0f + MetaRank(MetaUpgradeId.NovaCatalyst) * 24.0f + Math.Max(0.0f, _maxEnergy - 100.0f) * 0.7f;
        ClearBulletsNear(_playerPos, radius, false);
        _invulnTimer = Mathf.Max(_invulnTimer, 0.38f);
        AddShockwave(_playerPos, radius, color);
        Burst(_playerPos, color, 34, 520.0f, 0.85f);
        PlaySfx(120.0f, 0.9f, 0.28f, 0.42f, 0.12f, 0);
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

        if (IsUpgradeMaxed(UpgradeId.AstraNovaBloom))
        {
            int starCount = _visualPressure > 0.8f ? 12 : 18;
            for (int i = 0; i < starCount; i++)
            {
                float angle = -i * Mathf.Tau / starCount - _time * 0.18f;
                Vector2 dir = Vector2.Right.Rotated(angle);
                SpawnPlayerShot(_playerPos + dir * 78.0f, dir, 1040.0f, 5.8f, (22.0f + _astraNovaBloom * 5.0f) * _damageMultiplier, 0.96f, 2 + _astraTwinRefraction, true, i % 2);
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
                if (IsUpgradeMaxed(UpgradeId.VesperJudgmentCoil) && enemy.Kind == EnemyKind.Boss)
                {
                    enemy.BossGuard = 0.0f;
                    damage *= 1.16f;
                }
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
        if (IsUpgradeMaxed(UpgradeId.VesperSeverLine))
        {
            Vector2 cross = dir.Rotated(Mathf.Pi * 0.5f);
            DamageVesperBeam(_playerPos - cross * 760.0f, _playerPos + cross * 760.0f, width * 0.34f, 128.0f + _vesperSeverLine * 34.0f, color);
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

    private void LockEnemiesInBeam(Vector2 start, Vector2 end, float width, float lockTime, Color color)
    {
        for (int i = _enemies.Count - 1; i >= 0; i--)
        {
            Enemy enemy = _enemies[i];
            if (DistancePointToSegment(enemy.Pos, start, end) > width + enemy.Radius)
            {
                continue;
            }

            enemy.Vel *= 0.18f;
            enemy.Cooldown = Mathf.Max(enemy.Cooldown, lockTime);
            enemy.Overheat = Mathf.Max(enemy.Overheat, 0.45f + lockTime * 0.35f);
            enemy.OverheatMax = Mathf.Max(enemy.OverheatMax, enemy.Overheat);
            AddParticle(enemy.Pos, RandomDirection() * 80.0f, color.Lerp(Paper, 0.42f), 6.0f, 0.26f);
        }
    }

    private void BreakBossGuardInBeam(Vector2 start, Vector2 end, float width, float guardScale, Color color)
    {
        for (int i = _enemies.Count - 1; i >= 0; i--)
        {
            Enemy enemy = _enemies[i];
            if (enemy.Kind != EnemyKind.Boss || DistancePointToSegment(enemy.Pos, start, end) > width + enemy.Radius)
            {
                continue;
            }

            enemy.BossGuard *= guardScale;
            AddText("GUARD BREAK", enemy.Pos + new Vector2(0.0f, -enemy.Radius - 42.0f), color.Lerp(Paper, 0.2f), 18.0f);
            Burst(enemy.Pos, color, 14, 360.0f, 0.45f);
        }
    }

    private void CastKairoUltimate(Color color)
    {
        int droneCount = Math.Min(10, Math.Max(_orbiters, 4 + _kairoDroneBay + _kairoOverrideMatrix * 2));
        int volleys = Math.Min(5, 3 + Math.Min(2, _kairoSync / 2) + _kairoRelayProtocol);
        Burst(_playerPos, PickupBlue, 86, 720.0f, 1.35f);
        ClearBulletsNear(_playerPos, 360.0f + droneCount * 18.0f + _kairoOverrideMatrix * 36.0f, true);
        if (IsUpgradeMaxed(UpgradeId.KairoOverrideMatrix))
        {
            KairoOverrideTagAll(color);
        }

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
                AddDroneCommandCue(origin, origin - _playerPos, color, 0.92f);
            }
        }

        _orbiterFireTimer = 0.0f;
        _energy = Mathf.Clamp(_energy + _kairoRelayProtocol * 7.0f, 0.0f, _maxEnergy);
        PlaySfx(220.0f, 0.95f, 0.42f, 0.5f, 0.08f, 2);
    }

    private void KairoOverrideTagAll(Color color)
    {
        int tagged = 0;
        for (int i = _enemies.Count - 1; i >= 0 && tagged < 18; i--)
        {
            Enemy enemy = _enemies[i];
            Vector2 dir = (enemy.Pos - _playerPos).LengthSquared() > 0.01f ? (enemy.Pos - _playerPos).Normalized() : RandomDirection();
            SpawnPlayerShot(_playerPos + dir * 64.0f, dir, 1360.0f, 5.0f, (18.0f + _kairoOverrideMatrix * 5.0f) * _damageMultiplier, 0.72f, _kairoRelayProtocol >= 2 ? 1 : 0, true);
            AddParticle(enemy.Pos, RandomDirection() * 120.0f, color, 7.0f, 0.26f);
            tagged++;
        }
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
        if (IsUpgradeMaxed(UpgradeId.SolSolarForge))
        {
            _energy = Mathf.Clamp(_energy + 24.0f + _solForge * 5.0f, 0.0f, _maxEnergy);
            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                EnterOverheat(_enemies[i]);
            }
        }
        PlaySfx(132.0f, 1.35f, 0.58f, 0.58f, 0.18f, 1);
    }

    private void SolFlareCapstonePulse(Vector2 center, float radius, Color color)
    {
        int pulsed = 0;
        for (int i = _enemies.Count - 1; i >= 0 && pulsed < 10; i--)
        {
            Enemy enemy = _enemies[i];
            float distance = enemy.Pos.DistanceTo(center);
            if (distance > radius + 180.0f)
            {
                continue;
            }

            DamageEnemy(enemy, (24.0f + _solFlareCore * 8.0f) * _damageMultiplier, center, false);
            SpawnChainArc(center, enemy.Pos, color);
            pulsed++;
        }
    }

    private void CastNyxTactical(float focus01, Color color)
    {
        Vector2 center = _playerPos;
        Enemy? target = FindNearestEnemy(_playerPos, 860.0f + _nyxSingularity * 80.0f);
        if (target != null)
        {
            center = target.Pos;
        }

        float radius = 170.0f + _nyxSingularity * 34.0f + _nyxGravityCantor * 42.0f + focus01 * 110.0f;
        ClearBulletsNear(center, radius * 0.72f, true);
        int pulled = 0;
        for (int i = _enemies.Count - 1; i >= 0; i--)
        {
            Enemy enemy = _enemies[i];
            Vector2 delta = center - enemy.Pos;
            float distance = delta.Length();
            if (distance > radius + enemy.Radius)
            {
                continue;
            }

            Vector2 pull = distance > 1.0f ? delta / distance : RandomDirection();
            enemy.Vel += pull * (220.0f + _nyxEventHorizon * 42.0f + focus01 * 150.0f);
            enemy.Cooldown = Mathf.Max(enemy.Cooldown, 0.24f + _nyxEventHorizon * 0.08f);
            DamageEnemy(enemy, (22.0f + _nyxSingularity * 6.0f + _nyxGravityCantor * 7.0f + focus01 * 24.0f) * _damageMultiplier, center, false);
            if (IsUpgradeMaxed(UpgradeId.NyxEventHorizon))
            {
                EnterOverheat(enemy);
            }
            pulled++;
        }

        if (IsUpgradeMaxed(UpgradeId.NyxGravityCantor))
        {
            int blades = Math.Min(10, 4 + _nyxOrbit + _nyxGravityCantor);
            for (int i = 0; i < blades; i++)
            {
                Vector2 dir = Vector2.Right.Rotated(_time * 0.4f + i * Mathf.Tau / blades);
                SpawnPlayerShot(center + dir * 48.0f, dir, 860.0f, 5.4f, (18.0f + _nyxSingularity * 3.0f) * _damageMultiplier, 0.82f, 1, true);
            }
        }

        _assaultPower = 1.08f + focus01 * 0.16f;
        _assaultBurstTimer = 0.95f + focus01 * 0.42f;
        Burst(center, color, 32 + pulled, 460.0f, 0.78f);
    }

    private void CastRookTactical(float focus01, Color color)
    {
        Vector2 dir = _aimDir.LengthSquared() < 0.01f ? Vector2.Right : _aimDir.Normalized();
        Vector2 start = _playerPos - dir * 36.0f;
        Vector2 end = _playerPos + dir * (620.0f + _rookSiegeBattery * 80.0f + focus01 * 180.0f);
        float width = 62.0f + _rookBulwarkCore * 12.0f + _rookCitadelProtocol * 18.0f;
        ClearEnemyBulletsInBeam(start, end, width, true);
        _invulnTimer = Mathf.Max(_invulnTimer, 0.36f + _rookAegisRelay * 0.12f + focus01 * 0.22f);
        _playerHp = Mathf.Clamp(_playerHp + 5.0f + _rookAegisRelay * 4.0f, 0.0f, _playerMaxHp);

        int slammed = 0;
        for (int i = _enemies.Count - 1; i >= 0; i--)
        {
            Enemy enemy = _enemies[i];
            if (DistancePointToSegment(enemy.Pos, start, end) > width + enemy.Radius)
            {
                continue;
            }

            enemy.Vel += dir * (260.0f + _rookSiegeBattery * 42.0f);
            DamageEnemy(enemy, (42.0f + _rookBulwarkCore * 6.0f + _rookCitadelProtocol * 12.0f + focus01 * 38.0f) * _damageMultiplier, _playerPos, true);
            slammed++;
        }

        if (IsUpgradeMaxed(UpgradeId.RookCitadelProtocol))
        {
            _mirrorReduction *= 0.96f;
            _dashCooldown = Mathf.Min(_dashCooldown, 0.1f);
        }

        _assaultPower = 1.0f;
        _assaultBurstTimer = 0.7f + focus01 * 0.24f;
        Burst(_playerPos + dir * 90.0f, color, 22 + slammed * 2, 520.0f, 0.72f);
    }

    private void CastLyraTactical(float focus01, Color color)
    {
        int pulses = Math.Min(7, 3 + _lyraResonanceChord + _lyraEncoreField);
        float baseRadius = 130.0f + _lyraTempoBloom * 18.0f + focus01 * 80.0f;
        for (int pulse = 0; pulse < pulses; pulse++)
        {
            float radius = baseRadius + pulse * (34.0f + _lyraHarmonicCascade * 6.0f);
            ClearBulletsNear(_playerPos, radius * 0.34f, pulse == 0);
            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                Enemy enemy = _enemies[i];
                float distance = enemy.Pos.DistanceTo(_playerPos);
                if (Mathf.Abs(distance - radius) <= 42.0f + enemy.Radius)
                {
                    DamageEnemy(enemy, (16.0f + _lyraResonanceChord * 4.0f + _lyraHarmonicCascade * 5.0f + pulse * 2.0f) * _damageMultiplier, _playerPos, false);
                    if (_lyraHarmonicCascade > 0)
                    {
                        SpawnChainArc(_playerPos, enemy.Pos, color);
                    }
                }
            }
            DrawPulseParticles(_playerPos, radius, color);
        }

        _fireTimer = 0.0f;
        _assaultPower = 1.14f + _lyraTempoBloom * 0.035f + focus01 * 0.18f;
        _assaultBurstTimer = 1.05f + _lyraEncoreField * 0.12f + focus01 * 0.42f;
        _energy = Mathf.Clamp(_energy + 8.0f + _lyraTempoBloom * 2.5f, 0.0f, _maxEnergy);
        if (IsUpgradeMaxed(UpgradeId.LyraEncoreField))
        {
            _echoChance = Mathf.Min(0.65f, _echoChance + 0.02f);
        }
    }

    private void CastOrionTactical(float focus01, Color color)
    {
        int marks = Math.Min(7, 2 + _orionDeadeyeMark + _orionStarfallQuiver + Mathf.RoundToInt(focus01 * 2.0f));
        Vector2 origin = _playerPos;
        Enemy? previous = null;
        for (int i = 0; i < marks; i++)
        {
            Enemy? target = FindChainTarget(origin, previous, 860.0f + _orionPerihelionVector * 80.0f);
            if (target == null)
            {
                break;
            }

            Vector2 dir = (target.Pos - _playerPos).LengthSquared() > 0.01f ? (target.Pos - _playerPos).Normalized() : _aimDir;
            SpawnPlayerShot(_playerPos + dir * 44.0f, dir, 1880.0f + _orionCometSpear * 70.0f, 4.4f, (34.0f + _orionCometSpear * 7.0f + focus01 * 14.0f) * _damageMultiplier, 0.72f, 3 + _orionDeadeyeMark / 2, true);
            DamageEnemy(target, (28.0f + _orionDeadeyeMark * 8.0f) * _damageMultiplier, _playerPos, true);
            AddParticle(target.Pos, RandomDirection() * 100.0f, color, 7.0f, 0.24f);
            origin = target.Pos;
            previous = target;
        }

        if (IsUpgradeMaxed(UpgradeId.OrionPerihelionVector))
        {
            _dashCooldown = 0.0f;
            _playerVel += _aimDir * 260.0f;
        }
        _assaultPower = 1.1f + focus01 * 0.12f;
        _assaultBurstTimer = 0.8f + focus01 * 0.34f;
        ClearBulletsNear(_playerPos, 76.0f + _orionPerihelionVector * 18.0f, true);
    }

    private void DrawPulseParticles(Vector2 center, float radius, Color color)
    {
        int sparks = _visualPressure > 0.8f ? 4 : 8;
        for (int i = 0; i < sparks; i++)
        {
            float angle = i * Mathf.Tau / sparks + _time * 0.12f;
            Vector2 pos = center + Vector2.Right.Rotated(angle) * radius;
            AddParticle(pos, Vector2.Right.Rotated(angle) * 90.0f, color, 5.0f, 0.2f);
        }
    }

    private void CastNyxUltimate(Color color)
    {
        Enemy? target = FindNearestEnemy(_playerPos, 1280.0f);
        Vector2 center = target?.Pos ?? _playerPos;
        float radius = 520.0f + _nyxSingularity * 46.0f + _nyxGravityCantor * 64.0f;
        ClearBulletsNear(center, radius, true);
        Burst(center, color, 98, 760.0f, 1.45f);
        for (int i = _enemies.Count - 1; i >= 0; i--)
        {
            Enemy enemy = _enemies[i];
            float distance = enemy.Pos.DistanceTo(center);
            if (distance > radius + enemy.Radius)
            {
                continue;
            }

            Vector2 pull = (center - enemy.Pos).LengthSquared() > 1.0f ? (center - enemy.Pos).Normalized() : RandomDirection();
            enemy.Vel += pull * (420.0f + _nyxEventHorizon * 60.0f);
            float damage = Mathf.Lerp(270.0f + _nyxSingularity * 34.0f, 72.0f + _nyxGravityCantor * 15.0f, distance / radius) * _damageMultiplier;
            DamageEnemy(enemy, damage, center, true);
        }

        int blades = Math.Min(30, 14 + _nyxOrbit * 2 + _nyxGravityCantor * 3);
        for (int i = 0; i < blades; i++)
        {
            Vector2 dir = Vector2.Right.Rotated(i * Mathf.Tau / blades + _time * 0.4f);
            SpawnPlayerShot(center + dir * 72.0f, dir, 940.0f, 5.6f, (24.0f + _nyxOrbit * 3.0f) * _damageMultiplier, 0.9f, 2, true);
        }
        PlaySfx(88.0f, 1.5f, 0.48f, 0.54f, 0.2f, 0);
    }

    private void CastRookUltimate(Color color)
    {
        float radius = 600.0f + _rookBulwarkCore * 50.0f + _rookCitadelProtocol * 60.0f;
        ClearBulletsNear(_playerPos, radius * 0.9f, true);
        _invulnTimer = Mathf.Max(_invulnTimer, 1.05f + _rookAegisRelay * 0.35f);
        _playerHp = Mathf.Clamp(_playerHp + 20.0f + _rookAegisRelay * 12.0f, 0.0f, _playerMaxHp);
        Burst(_playerPos, color, 140, 820.0f, 1.65f);

        int waves = Math.Min(5, 2 + _rookSiegeBattery / 2 + _rookCitadelProtocol);
        for (int wave = 0; wave < waves; wave++)
        {
            int spokes = 10 + wave * 2;
            for (int i = 0; i < spokes; i++)
            {
                Vector2 dir = Vector2.Right.Rotated(i * Mathf.Tau / spokes + wave * 0.18f);
                SpawnPlayerShot(_playerPos + dir * (42.0f + wave * 24.0f), dir, 760.0f + wave * 80.0f, 7.4f, (32.0f + _rookSiegeBattery * 5.0f) * _damageMultiplier, 0.78f, 2 + _rookBulwarkCore / 2, false);
            }
        }

        for (int i = _enemies.Count - 1; i >= 0; i--)
        {
            Enemy enemy = _enemies[i];
            float distance = enemy.Pos.DistanceTo(_playerPos);
            if (distance < radius + enemy.Radius)
            {
                DamageEnemy(enemy, Mathf.Lerp(240.0f + _rookSiegeBattery * 24.0f, 58.0f, distance / radius) * _damageMultiplier, _playerPos, true);
            }
        }
        PlaySfx(64.0f, 1.6f, 0.5f, 0.55f, 0.22f, 0);
    }

    private void CastLyraUltimate(Color color)
    {
        ClearBulletsNear(_playerPos, 680.0f + _lyraHarmonicCascade * 52.0f, true);
        Burst(_playerPos, color, 112, 760.0f, 1.42f);
        int rings = Math.Min(5, 3 + _lyraEncoreField);
        for (int ring = 0; ring < rings; ring++)
        {
            int notes = Math.Min(42, 14 + _lyraResonanceChord * 2 + ring * 4);
            float speed = 820.0f + ring * 120.0f + _lyraTempoBloom * 45.0f;
            for (int i = 0; i < notes; i++)
            {
                Vector2 dir = Vector2.Right.Rotated(i * Mathf.Tau / notes + ring * 0.25f);
                SpawnPlayerShot(_playerPos + dir * (36.0f + ring * 26.0f), dir, speed, 4.4f, (17.0f + _lyraHarmonicCascade * 3.0f + ring * 1.8f) * _damageMultiplier, 0.86f, _lyraHarmonicCascade >= 3 ? 1 : 0, true);
            }
        }

        for (int i = _enemies.Count - 1; i >= 0; i--)
        {
            Enemy enemy = _enemies[i];
            float distance = enemy.Pos.DistanceTo(_playerPos);
            if (distance < 700.0f)
            {
                DamageEnemy(enemy, (96.0f + _lyraResonanceChord * 12.0f + _lyraHarmonicCascade * 18.0f) * _damageMultiplier, _playerPos, false);
            }
        }
        _energy = Mathf.Clamp(_energy + _lyraEncoreField * 8.0f, 0.0f, _maxEnergy);
        PlaySfx(280.0f, 1.1f, 0.46f, 0.48f, 0.08f, 2);
    }

    private void CastOrionUltimate(Color color)
    {
        int spears = Math.Min(14, 6 + _orionCometSpear + _orionStarfallQuiver * 2);
        Burst(_playerPos, color, 90, 760.0f, 1.35f);
        ClearBulletsNear(_playerPos, 300.0f + _orionPerihelionVector * 40.0f, true);
        for (int i = 0; i < spears; i++)
        {
            Enemy? target = FindNearestEnemy(_playerPos + RandomDirection() * i * 18.0f, 1400.0f);
            Vector2 dir = target != null && (target.Pos - _playerPos).LengthSquared() > 1.0f
                ? (target.Pos - _playerPos).Normalized()
                : _aimDir.Rotated((i - (spears - 1) * 0.5f) * 0.12f);
            Vector2 side = new(-dir.Y, dir.X);
            Vector2 origin = _playerPos + side * (i - (spears - 1) * 0.5f) * 18.0f;
            SpawnPlayerShot(origin + dir * 68.0f, dir, 2180.0f, 5.2f, (86.0f + _orionCometSpear * 12.0f + _orionDeadeyeMark * 8.0f) * _damageMultiplier, 0.82f, 6 + _orionDeadeyeMark, true);
            if (target != null)
            {
                DamageEnemy(target, (72.0f + _orionDeadeyeMark * 15.0f) * _damageMultiplier, origin, true);
            }
        }
        if (IsUpgradeMaxed(UpgradeId.OrionStarfallQuiver))
        {
            _nextWaveDamageBoost = Mathf.Max(_nextWaveDamageBoost, 1.18f);
        }
        PlaySfx(118.0f, 1.8f, 0.5f, 0.56f, 0.12f, 1);
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
            case PilotKind.Nyx:
                FireNyxShot();
                return;
            case PilotKind.Rook:
                FireRookShot();
                return;
            case PilotKind.Lyra:
                FireLyraShot();
                return;
            case PilotKind.Orion:
                FireOrionShot();
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
                }
            }
        }

        if (IsUpgradeMaxed(UpgradeId.AstraRefraction))
        {
            Vector2 right = new(-_aimDir.Y, _aimDir.X);
            for (int side = -1; side <= 1; side += 2)
            {
                Vector2 origin = _playerPos + _aimDir * 34.0f + right * side * 54.0f;
                SpawnPlayerShot(origin, _aimDir.Rotated(side * 0.035f), 1180.0f, 4.2f, (10.0f + _astraRefraction * 2.4f) * _damageMultiplier, 0.62f, 1, true);
            }
        }

        if (IsUpgradeMaxed(UpgradeId.AstraPrismWake) && _shots.Count < MaxShots * 0.88f)
        {
            Vector2 echoDir = _aimDir.Rotated(Mathf.Sin(_time * 5.0f) * 0.08f);
            SpawnPlayerShot(_playerPos + echoDir * 30.0f, echoDir, 1500.0f, 4.6f, (14.0f + _astraWake * 2.6f) * _damageMultiplier, 0.56f, 1, true);
        }

        AddParticle(_playerPos + _aimDir * 32.0f, _aimDir * 180.0f, color, 10.0f, 0.18f);
        PlaySfx(690.0f, -140.0f, 0.06f, 0.14f, 0.01f, 1);
    }

    private void FireVesperShot()
    {
        Color color = PolarityColor(_playerPolarity);
        SpawnPlayerShot(_playerPos + _aimDir * 42.0f, _aimDir, 1740.0f, 5.5f, (55.0f + _vesperCharge * 7.5f + (_riftNeedle ? 8.0f : 0.0f)) * _damageMultiplier, 0.7f, 4 + _vesperCharge / 2 + (_riftNeedle ? 1 : 0), true);
        if (IsUpgradeMaxed(UpgradeId.VesperCapacitor))
        {
            SpawnPlayerShot(_playerPos + _aimDir * 18.0f, _aimDir, 1180.0f, 7.2f, (31.0f + _vesperCharge * 4.2f) * _damageMultiplier, 0.48f, 3, true);
        }

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
        if (IsUpgradeMaxed(UpgradeId.VesperSplitRail))
        {
            SpawnPlayerShot(_playerPos + _aimDir * 32.0f, _aimDir.Rotated(0.18f), 1420.0f, 4.4f, (22.0f + _vesperFork * 4.0f) * _damageMultiplier, 0.55f, 2, true);
            SpawnPlayerShot(_playerPos + _aimDir * 32.0f, _aimDir.Rotated(-0.18f), 1420.0f, 4.4f, (22.0f + _vesperFork * 4.0f) * _damageMultiplier, 0.55f, 2, true);
        }

        AddParticle(_playerPos + _aimDir * 34.0f, _aimDir * 220.0f, color, 9.0f, 0.16f);
        PlaySfx(430.0f, -180.0f, 0.08f, 0.18f, 0.01f, 1);
    }

    private void FireKairoShot()
    {
        Vector2 dir = _aimDir.LengthSquared() < 0.01f ? Vector2.Right : _aimDir.Normalized();
        int count = Math.Min(IsUpgradeMaxed(UpgradeId.KairoSwarmSync) ? 4 : 3, 1 + Math.Max(0, _multiShot - 1) / 2 + (_kairoSync >= 3 ? 1 : 0) + (IsUpgradeMaxed(UpgradeId.KairoSwarmSync) ? 1 : 0));
        for (int i = 0; i < count; i++)
        {
            float offset = count == 1 ? 0.0f : (i == 0 ? -0.08f : 0.08f);
            Vector2 shotDir = dir.Rotated(offset);
            SpawnPlayerShot(_playerPos + shotDir * 36.0f, shotDir, _riftNeedle ? 1320.0f : 1120.0f, _riftNeedle ? 4.2f : 5.0f, (12.8f + _kairoSync * 2.6f + (_riftNeedle ? 3.0f : 0.0f)) * _damageMultiplier, 1.05f, _riftNeedle ? 1 : 0, _riftNeedle);
        }

        AddParticle(_playerPos + dir * 28.0f, dir * 120.0f, PolarityColor(_playerPolarity), 7.0f, 0.14f);
        PlaySfx(760.0f, -120.0f, 0.05f, 0.1f, 0.01f, 1);
    }

    private void FireSolShot()
    {
        int count = 4 + _solBloom + Math.Max(0, _multiShot - 1) + (IsUpgradeMaxed(UpgradeId.SolCoronaBloom) ? 3 : 0);
        float spread = (IsUpgradeMaxed(UpgradeId.SolCoronaBloom) ? 0.19f : 0.13f) + _solBloom * 0.012f;
        Color color = PolarityColor(_playerPolarity);
        for (int i = 0; i < count; i++)
        {
            float offset = (i - (count - 1) * 0.5f) * spread;
            Vector2 dir = _aimDir.Rotated(offset);
            SpawnPlayerShot(_playerPos + dir * 36.0f, dir, _riftNeedle ? 1180.0f : 1040.0f, _riftNeedle ? 5.0f : 6.2f, (13.5f + _solForge * 2.0f + (_riftNeedle ? 2.0f : 0.0f)) * _damageMultiplier, 0.62f + _solBloom * 0.025f, _riftNeedle ? 1 : 0, _riftNeedle);
        }

        AddParticle(_playerPos + _aimDir * 28.0f, _aimDir * 160.0f, color, 12.0f, 0.16f);
        PlaySfx(590.0f, -150.0f, 0.07f, 0.16f, 0.02f, 1);
    }

    private void FireNyxShot()
    {
        Vector2 baseDir = _aimDir.LengthSquared() < 0.01f ? Vector2.Right : _aimDir.Normalized();
        int blades = Math.Min(7, 2 + _nyxOrbit + Math.Max(0, _multiShot - 1) / 2);
        float arc = 0.16f + _nyxGravityCantor * 0.018f;
        for (int i = 0; i < blades; i++)
        {
            float offset = (i - (blades - 1) * 0.5f) * arc;
            Vector2 dir = baseDir.Rotated(offset);
            SpawnPlayerShot(_playerPos + dir * 38.0f + dir.Orthogonal() * offset * 52.0f, dir, 1040.0f + _nyxEventHorizon * 30.0f, 5.0f, (16.0f + _nyxSingularity * 3.2f + (_riftNeedle ? 3.0f : 0.0f)) * _damageMultiplier, 0.9f, _nyxGravityCantor >= 2 ? 1 : 0, true);
        }

        if (IsUpgradeMaxed(UpgradeId.NyxOrbit) && _shots.Count < MaxShots * 0.88f)
        {
            Vector2 side = baseDir.Rotated(Mathf.Pi * 0.5f);
            SpawnPlayerShot(_playerPos + side * 34.0f, baseDir.Rotated(0.28f), 940.0f, 4.6f, (14.0f + _nyxOrbit * 2.4f) * _damageMultiplier, 0.75f, 1, true);
            SpawnPlayerShot(_playerPos - side * 34.0f, baseDir.Rotated(-0.28f), 940.0f, 4.6f, (14.0f + _nyxOrbit * 2.4f) * _damageMultiplier, 0.75f, 1, true);
        }

        AddParticle(_playerPos + baseDir * 28.0f, baseDir * 130.0f, PilotAccent(_runPilot), 9.0f, 0.16f);
        PlaySfx(520.0f, -150.0f, 0.06f, 0.14f, 0.01f, 1);
    }

    private void FireRookShot()
    {
        Vector2 dir = _aimDir.LengthSquared() < 0.01f ? Vector2.Right : _aimDir.Normalized();
        int shells = Math.Min(4, 1 + _rookSiegeBattery / 2 + Math.Max(0, _multiShot - 1) / 3);
        for (int i = 0; i < shells; i++)
        {
            float offset = shells == 1 ? 0.0f : (i - (shells - 1) * 0.5f) * 0.12f;
            SpawnPlayerShot(_playerPos + dir.Rotated(offset) * 46.0f, dir.Rotated(offset), 1040.0f, 8.0f + _rookBulwarkCore * 0.35f, (56.0f + _rookSiegeBattery * 9.0f + _rookCitadelProtocol * 6.0f) * _damageMultiplier, 0.82f, 2 + _rookBulwarkCore / 2 + (_riftNeedle ? 1 : 0), _riftNeedle);
        }

        if (IsUpgradeMaxed(UpgradeId.RookSiegeBattery))
        {
            SpawnPlayerShot(_playerPos + dir * 28.0f, dir, 840.0f, 11.0f, (36.0f + _rookSiegeBattery * 5.0f) * _damageMultiplier, 0.62f, 1, false);
        }

        AddParticle(_playerPos + dir * 34.0f, dir * 230.0f, PilotAccent(_runPilot), 12.0f, 0.18f);
        PlaySfx(260.0f, -210.0f, 0.09f, 0.2f, 0.02f, 1);
    }

    private void FireLyraShot()
    {
        _lyraBeat++;
        int chord = 2 + _lyraResonanceChord / 2 + Math.Max(0, _multiShot - 1) / 2;
        if (_lyraBeat % 3 == 0)
        {
            chord += 1 + _lyraTempoBloom / 2;
        }
        chord = Math.Min(8, chord + (IsUpgradeMaxed(UpgradeId.LyraResonanceChord) ? 1 : 0));
        float spread = 0.1f + _lyraResonanceChord * 0.01f;
        for (int i = 0; i < chord; i++)
        {
            float offset = (i - (chord - 1) * 0.5f) * spread;
            Vector2 dir = _aimDir.Rotated(offset);
            SpawnPlayerShot(_playerPos + dir * 34.0f, dir, 1160.0f + _lyraTempoBloom * 36.0f, 4.4f, (12.0f + _lyraHarmonicCascade * 2.6f + (_lyraBeat % 3 == 0 ? 5.0f : 0.0f)) * _damageMultiplier, 0.72f, _lyraHarmonicCascade >= 2 ? 1 : 0, true);
        }

        if (IsUpgradeMaxed(UpgradeId.LyraTempoBloom) && _shots.Count < MaxShots * 0.88f)
        {
            Vector2 back = -_aimDir;
            SpawnPlayerShot(_playerPos + back * 20.0f, back, 760.0f, 4.2f, (9.0f + _lyraTempoBloom * 2.0f) * _damageMultiplier, 0.52f, 0, true);
        }

        AddParticle(_playerPos + _aimDir * 26.0f, _aimDir * 120.0f, PilotAccent(_runPilot), 8.0f, 0.14f);
        PlaySfx(820.0f + (_lyraBeat % 3) * 72.0f, -90.0f, 0.045f, 0.11f, 0.01f, 2);
    }

    private void FireOrionShot()
    {
        Enemy? target = FindNearestEnemy(_playerPos, 1220.0f + _orionDeadeyeMark * 70.0f);
        Vector2 dir = _aimDir.LengthSquared() < 0.01f ? Vector2.Right : _aimDir.Normalized();
        SpawnPlayerShot(_playerPos + dir * 52.0f, dir, 2060.0f + _orionCometSpear * 80.0f, 4.8f, (72.0f + _orionCometSpear * 10.0f + _orionDeadeyeMark * 6.0f + (_riftNeedle ? 8.0f : 0.0f)) * _damageMultiplier, 0.78f, 5 + _orionDeadeyeMark / 2 + (_riftNeedle ? 1 : 0), true);
        if (_orionStarfallQuiver > 0)
        {
            int side = Math.Min(4, _orionStarfallQuiver + Math.Max(0, _multiShot - 1) / 2);
            for (int i = 1; i <= side; i++)
            {
                float offset = 0.055f * i;
                SpawnPlayerShot(_playerPos + dir * 38.0f, dir.Rotated(offset), 1720.0f, 3.8f, (25.0f + _orionCometSpear * 4.0f) * _damageMultiplier, 0.56f, 2, true);
                SpawnPlayerShot(_playerPos + dir * 38.0f, dir.Rotated(-offset), 1720.0f, 3.8f, (25.0f + _orionCometSpear * 4.0f) * _damageMultiplier, 0.56f, 2, true);
            }
        }

        if (IsUpgradeMaxed(UpgradeId.OrionDeadeyeMark) && target != null)
        {
            SpawnChainArc(_playerPos, target.Pos, PilotAccent(_runPilot));
        }
        AddParticle(_playerPos + dir * 34.0f, dir * 260.0f, PilotAccent(_runPilot), 9.0f, 0.16f);
        PlaySfx(360.0f, -220.0f, 0.08f, 0.18f, 0.01f, 1);
    }

    private void FirePulseMagazine()
    {
        if (_pulseMagazine <= 0 || _shots.Count > MaxShots * 0.9f)
        {
            return;
        }

        int count = Math.Min(5, 1 + _pulseMagazine / 2 + (IsUpgradeMaxed(UpgradeId.PulseMagazine) ? 1 : 0));
        float spread = 0.18f + _pulseMagazine * 0.012f;
        for (int i = 0; i < count; i++)
        {
            float offset = count == 1 ? 0.0f : (i - (count - 1) * 0.5f) * spread;
            Vector2 dir = _aimDir.Rotated(offset);
            SpawnPlayerShot(_playerPos + dir * 30.0f, dir, 1180.0f, 4.0f, (7.0f + _pulseMagazine * 1.8f) * _damageMultiplier, 0.52f, _ricochetMatrix >= 3 ? 1 : 0, _riftNeedle);
        }
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
        shot.Pierce = pierce + (_ricochetMatrix >= 3 ? 1 : 0);
        shot.Rift = rift;
    }

    private void FirePolarityStorm()
    {
        int count = 8 + _polarityStorm * 2;
        Color color = PilotAccent(_runPilot);
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
        }
        Burst(_playerPos, color, 12 + _polarityStorm * 4, 340.0f, 0.55f);
    }

    private void UpdateEnemies(float dt)
    {
        _spawnDirector += dt;
        float threat = ThreatLevel();
        int sector = CurrentSectorIndex();
        float pressure = PerformancePressure();
        for (int i = _enemies.Count - 1; i >= 0; i--)
        {
            Enemy enemy = _enemies[i];
            enemy.SpawnPulse = Approach(enemy.SpawnPulse, 0.0f, dt * 2.8f);
            enemy.Phase += dt * (0.7f + threat * 0.05f);
            enemy.Cooldown -= dt * EnemyCooldownRate(enemy);
            enemy.Overheat = Mathf.Max(0.0f, enemy.Overheat - dt);
            enemy.ContactTimer -= dt;
            TickEnemyDash(enemy, dt);

            if (enemy.Kind == EnemyKind.Boss)
            {
                UpdateBoss(enemy, dt);
                continue;
            }

            Vector2 toPlayer = _playerPos - enemy.Pos;
            float distance = Mathf.Max(toPlayer.Length(), 1.0f);
            Vector2 dir = toPlayer / distance;
            Vector2 desired = Vector2.Zero;
            float speed = _enemySlow * DifficultyEnemyMoveScale();
            bool dashLocked = enemy.DashWarmup > 0.0f || enemy.DashTime > 0.0f;

            if (!dashLocked)
            {
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
                        desired = (distance < 430.0f ? -dir * 95.0f : dir * 58.0f) * speed;
                        if (enemy.Cooldown <= 0.0f)
                        {
                            int petals = ScaledEnemyPatternCount(enemy.Kind, 7 + CurrentWaveInSector() / 2 + sector);
                            float baseAngle = enemy.Phase;
                            for (int a = 0; a < petals; a++)
                            {
                                FireEnemy(enemy, Vector2.Right.Rotated(baseAngle + Mathf.Tau * a / petals), 260.0f + threat * 12.0f, 1, 0.0f);
                            }
                            enemy.Cooldown = 2.35f;
                            enemy.Polarity = OtherStance(enemy.Polarity);
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
                        desired = (distance > 640.0f ? dir * 210.0f : -dir * 100.0f + dir.Orthogonal() * Mathf.Sin(enemy.Phase * 2.3f) * 145.0f) * speed;
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
                        desired = (distance < 520.0f ? -dir * 72.0f : dir * 78.0f) * speed;
                        if (enemy.Cooldown <= 0.0f)
                        {
                            FireEnemy(enemy, dir, 300.0f + threat * 10.0f, 5, 0.22f, 11.0f);
                            if (_enemies.Count < 26 + sector * 4)
                            {
                                SpawnEnemy(sector >= 3 ? EnemyKind.Shard : EnemyKind.Chaser, ClampToArena(enemy.Pos + RandomDirection() * 78.0f, 34.0f), OtherStance(enemy.Polarity), 1);
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
                        desired = (distance > 380.0f ? dir * 78.0f : -dir * 28.0f) * speed;
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
                            enemy.Polarity = OtherStance(enemy.Polarity);
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
            }

            if (!dashLocked && ShouldStartEnemyDash(enemy, sector, CurrentWaveInSector(), distance, pressure))
            {
                StartEnemyDash(enemy, dir);
            }

            if (enemy.DashWarmup > 0.0f)
            {
                float warm01 = EnemyDashWarmup01(enemy);
                enemy.Vel = enemy.Vel.Lerp(enemy.DashDir * (52.0f + warm01 * 34.0f), 1.0f - Mathf.Exp(-dt * 8.0f));
            }
            else if (enemy.DashTime > 0.0f)
            {
                enemy.Vel = enemy.DashDir * EnemyDashSpeed(enemy) * speed;
            }
            else if (EnemyIsCharging(enemy))
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

    private void TickEnemyDash(Enemy enemy, float dt)
    {
        enemy.DashCooldown -= dt;
        if (enemy.DashWarmup > 0.0f)
        {
            enemy.DashWarmup = Mathf.Max(0.0f, enemy.DashWarmup - dt);
            if (enemy.DashWarmup <= 0.0f)
            {
                enemy.DashTime = EnemyDashDuration(enemy);
                enemy.SpawnPulse = Math.Max(enemy.SpawnPulse, 0.42f);
            }
        }
        else if (enemy.DashTime > 0.0f)
        {
            enemy.DashTime = Mathf.Max(0.0f, enemy.DashTime - dt);
        }
    }

    private bool ShouldStartEnemyDash(Enemy enemy, int sector, int waveInSector, float distance, float pressure)
    {
        if (enemy.DashCooldown > 0.0f || enemy.SplitDepth > 0 || EnemyIsCharging(enemy) || pressure > 0.82f)
        {
            return false;
        }

        if (sector == 0 && waveInSector < 3)
        {
            return false;
        }

        bool eligible = enemy.Kind switch
        {
            EnemyKind.Chaser => waveInSector >= 3 && distance is > 210.0f and < 820.0f,
            EnemyKind.Weaver => sector >= 1 && distance is > 260.0f and < 760.0f,
            EnemyKind.Lance => sector >= 1 && distance is > 320.0f and < 900.0f,
            EnemyKind.Splitter => sector >= 2 && waveInSector >= 3 && distance is > 260.0f and < 780.0f,
            EnemyKind.Shard => sector >= 2 && distance is > 180.0f and < 820.0f,
            EnemyKind.Drifter => sector >= 2 && distance is > 240.0f and < 820.0f,
            EnemyKind.Harrier => sector >= 3 && distance is > 170.0f and < 860.0f,
            EnemyKind.Siren => sector >= 4 && enemy.Elite && distance is > 300.0f and < 820.0f,
            _ => false,
        };

        if (!eligible)
        {
            return false;
        }

        float chance = 0.58f + sector * 0.08f + (enemy.Elite ? 0.16f : 0.0f);
        if (_currentWavePace is WavePaceKind.Pressure or WavePaceKind.Swarm)
        {
            chance += 0.12f;
        }
        if (_currentWavePace == WavePaceKind.Recovery)
        {
            chance *= 0.45f;
        }

        return _rng.Randf() < Mathf.Clamp(chance, 0.28f, 0.92f);
    }

    private void StartEnemyDash(Enemy enemy, Vector2 toPlayerDir)
    {
        Vector2 predicted = _playerPos + _playerVel * EnemyDashLead(enemy);
        Vector2 dashDir = predicted - enemy.Pos;
        if (dashDir.LengthSquared() <= 0.01f)
        {
            dashDir = toPlayerDir;
        }

        dashDir = dashDir.Normalized();
        switch (enemy.Kind)
        {
            case EnemyKind.Weaver:
            case EnemyKind.Drifter:
                dashDir = dashDir.Rotated((_rng.Randf() < 0.5f ? -1.0f : 1.0f) * _rng.RandfRange(0.8f, 1.35f));
                break;
            case EnemyKind.Lance:
            case EnemyKind.Shard:
                dashDir = dashDir.Rotated((_rng.Randf() < 0.5f ? -1.0f : 1.0f) * _rng.RandfRange(0.18f, 0.42f));
                break;
            case EnemyKind.Siren:
                dashDir = -dashDir.Rotated((_rng.Randf() < 0.5f ? -1.0f : 1.0f) * 0.45f);
                break;
        }

        enemy.DashDir = dashDir;
        enemy.DashWarmup = EnemyDashWarmup(enemy);
        enemy.DashTime = 0.0f;
        enemy.DashCooldown = NextEnemyDashCooldown(enemy, CurrentSectorIndex());
        enemy.Cooldown = Math.Max(enemy.Cooldown, enemy.DashWarmup + EnemyDashDuration(enemy) + 0.18f);
        enemy.SpawnPulse = Math.Max(enemy.SpawnPulse, 0.32f);
    }

    private static float InitialEnemyDashCooldown(EnemyKind kind, int sector, int waveInSector)
    {
        if (sector == 0 && waveInSector < 3)
        {
            return 99.0f;
        }

        float baseCooldown = kind switch
        {
            EnemyKind.Chaser => 4.8f,
            EnemyKind.Weaver => 6.2f,
            EnemyKind.Lance => 6.5f,
            EnemyKind.Splitter => 6.8f,
            EnemyKind.Shard => 5.4f,
            EnemyKind.Drifter => 6.4f,
            EnemyKind.Harrier => 4.6f,
            EnemyKind.Siren => 7.2f,
            _ => 99.0f,
        };
        return baseCooldown + Math.Max(0, 3 - sector) * 0.7f + waveInSector * 0.08f;
    }

    private float NextEnemyDashCooldown(Enemy enemy, int sector)
    {
        float cooldown = enemy.Kind switch
        {
            EnemyKind.Chaser => 6.1f,
            EnemyKind.Weaver => 7.4f,
            EnemyKind.Lance => 7.8f,
            EnemyKind.Splitter => 8.0f,
            EnemyKind.Shard => 6.7f,
            EnemyKind.Drifter => 7.6f,
            EnemyKind.Harrier => 5.8f,
            EnemyKind.Siren => 8.8f,
            _ => 99.0f,
        };
        cooldown -= sector * 0.38f;
        if (_currentWavePace is WavePaceKind.Pressure or WavePaceKind.Swarm)
        {
            cooldown -= 0.45f;
        }
        if (enemy.Elite)
        {
            cooldown *= 0.82f;
        }
        return Mathf.Clamp(cooldown + _rng.RandfRange(-0.42f, 0.72f), 3.2f, 9.5f);
    }

    private static float EnemyDashLead(Enemy enemy)
    {
        return enemy.Kind switch
        {
            EnemyKind.Chaser => 0.24f,
            EnemyKind.Lance => 0.34f,
            EnemyKind.Shard => 0.28f,
            EnemyKind.Harrier => 0.22f,
            _ => 0.18f,
        };
    }

    private static float EnemyDashWarmup(Enemy enemy)
    {
        float warmup = enemy.Kind switch
        {
            EnemyKind.Harrier => 0.3f,
            EnemyKind.Chaser => 0.38f,
            EnemyKind.Shard => 0.34f,
            EnemyKind.Lance => 0.42f,
            _ => 0.44f,
        };
        return enemy.Elite ? warmup * 0.84f : warmup;
    }

    private static float EnemyDashDuration(Enemy enemy)
    {
        return enemy.Kind switch
        {
            EnemyKind.Harrier => 0.18f,
            EnemyKind.Chaser => 0.2f,
            EnemyKind.Lance => 0.16f,
            EnemyKind.Siren => 0.18f,
            _ => 0.21f,
        };
    }

    private static float EnemyDashSpeed(Enemy enemy)
    {
        float speed = enemy.Kind switch
        {
            EnemyKind.Chaser => 660.0f,
            EnemyKind.Weaver => 560.0f,
            EnemyKind.Lance => 720.0f,
            EnemyKind.Splitter => 540.0f,
            EnemyKind.Shard => 760.0f,
            EnemyKind.Drifter => 620.0f,
            EnemyKind.Harrier => 820.0f,
            EnemyKind.Siren => 580.0f,
            _ => 520.0f,
        };
        return enemy.Elite ? speed * 1.12f : speed;
    }

    private static float EnemyDashWarmup01(Enemy enemy)
    {
        float warmup = EnemyDashWarmup(enemy);
        return warmup <= 0.0f ? 0.0f : 1.0f - Mathf.Clamp(enemy.DashWarmup / warmup, 0.0f, 1.0f);
    }

    private void UpdateBoss(Enemy boss, float dt)
    {
        _bossPatternTimer += dt;
        boss.BossGuard = Mathf.Max(0.0f, boss.BossGuard - dt);
        boss.BossIntentPulse = Mathf.Max(0.0f, boss.BossIntentPulse - dt);
        int sector = CurrentSectorIndex();
        float hpRatio = Mathf.Clamp(boss.Hp / boss.MaxHp, 0.0f, 1.0f);
        Vector2 desired = BossMovementTarget(boss, sector, hpRatio) - boss.Pos;
        if (EnemyIsCharging(boss))
        {
            float charge = EnemyTelegraph01(boss);
            boss.Vel = boss.Vel.Lerp(Vector2.Zero, 1.0f - Mathf.Exp(-dt * (7.0f + charge * 8.0f)));
        }
        else
        {
            boss.Vel = boss.Vel.Lerp(desired * BossMoveResponsiveness(boss.BossArchetype) * DifficultyEnemyMoveScale(), 1.0f - Mathf.Exp(-dt * BossMoveLerp(boss.BossArchetype)));
        }
        boss.Pos += boss.Vel * dt;
        boss.Pos = ClampToArena(boss.Pos, boss.Radius);

        if (boss.Cooldown <= 0.0f)
        {
            ExecuteBossPattern(boss, ChooseBossPattern(boss, sector, hpRatio), sector, hpRatio);
        }
    }

    private Vector2 BossMovementTarget(Enemy boss, int sector, float hpRatio)
    {
        float t = _time + boss.Phase;
        float enrage = 1.0f - hpRatio;
        return boss.BossArchetype switch
        {
            BossArchetype.Prism => new Vector2(
                ScreenCenter.X + Mathf.Sin(t * (1.12f + enrage * 0.22f)) * (600.0f - sector * 22.0f) + Mathf.Sin(t * 2.65f) * 86.0f,
                Arena.Position.Y + 132.0f + sector * 14.0f + Mathf.Sin(t * 1.8f) * 64.0f),
            BossArchetype.Swarm => new Vector2(
                Mathf.Lerp(ScreenCenter.X + Mathf.Sin(t * 0.82f) * 460.0f, _playerPos.X, 0.28f + enrage * 0.08f),
                Arena.Position.Y + 138.0f + sector * 16.0f + Mathf.Cos(t * 1.25f) * 58.0f),
            BossArchetype.Forge => new Vector2(
                ScreenCenter.X + Mathf.Sin(t * 0.55f) * (420.0f - sector * 14.0f),
                Arena.Position.Y + 188.0f + sector * 20.0f + Mathf.Sin(t * 0.9f) * 32.0f),
            BossArchetype.Rift => new Vector2(
                ScreenCenter.X + Mathf.Sin(t * (0.88f + enrage * 0.18f)) * (650.0f - sector * 18.0f) + Mathf.Sin(t * 2.15f) * 120.0f,
                Arena.Position.Y + 158.0f + sector * 18.0f + Mathf.Cos(t * 1.55f) * 72.0f),
            BossArchetype.Mirror => new Vector2(
                ScreenCenter.X + Mathf.Sin(t * (0.92f + enrage * 0.18f)) * (560.0f - sector * 20.0f),
                Arena.Position.Y + 145.0f + sector * 14.0f + Mathf.Sin(t * 2.1f) * 54.0f),
            BossArchetype.Tempest => new Vector2(
                ScreenCenter.X + Mathf.Sin(t * (1.28f + enrage * 0.28f)) * (640.0f - sector * 18.0f) + Mathf.Cos(t * 2.8f) * 84.0f,
                Arena.Position.Y + 125.0f + sector * 14.0f + Mathf.Cos(t * 1.9f) * 70.0f),
            BossArchetype.Bastion => new Vector2(
                Mathf.Lerp(ScreenCenter.X + Mathf.Sin(t * 0.46f) * 320.0f, _playerPos.X, 0.12f),
                Arena.Position.Y + 205.0f + sector * 14.0f + Mathf.Sin(t * 0.72f) * 24.0f),
            BossArchetype.Serpent => new Vector2(
                ScreenCenter.X + Mathf.Sin(t * (1.05f + enrage * 0.2f)) * (620.0f - sector * 18.0f),
                Arena.Position.Y + 154.0f + sector * 16.0f + Mathf.Sin(t * 2.35f) * 82.0f),
            BossArchetype.Oracle => new Vector2(
                ScreenCenter.X + Mathf.Sin(t * 0.72f) * (500.0f - sector * 18.0f) + Mathf.Sin(t * 2.4f) * 62.0f,
                Arena.Position.Y + 136.0f + sector * 12.0f + Mathf.Cos(t * 1.18f) * 44.0f),
            _ => new Vector2(
                ScreenCenter.X + Mathf.Sin(t * (0.78f + sector * 0.05f)) * (520.0f - sector * 24.0f) + Mathf.Sin(t * 1.7f) * 110.0f,
                Arena.Position.Y + 150.0f + sector * 18.0f + Mathf.Sin(t * 1.1f) * 44.0f),
        };
    }

    private static float BossMoveResponsiveness(BossArchetype archetype)
    {
        return archetype switch
        {
            BossArchetype.Prism => 1.75f,
            BossArchetype.Swarm => 1.55f,
            BossArchetype.Forge => 1.08f,
            BossArchetype.Rift => 1.62f,
            BossArchetype.Mirror => 1.72f,
            BossArchetype.Tempest => 1.9f,
            BossArchetype.Bastion => 0.94f,
            BossArchetype.Serpent => 1.7f,
            BossArchetype.Oracle => 1.42f,
            _ => 1.5f,
        };
    }

    private static float BossMoveLerp(BossArchetype archetype)
    {
        return archetype switch
        {
            BossArchetype.Prism => 2.35f,
            BossArchetype.Swarm => 2.05f,
            BossArchetype.Forge => 1.45f,
            BossArchetype.Rift => 2.15f,
            BossArchetype.Mirror => 2.25f,
            BossArchetype.Tempest => 2.55f,
            BossArchetype.Bastion => 1.25f,
            BossArchetype.Serpent => 2.35f,
            BossArchetype.Oracle => 1.82f,
            _ => 1.9f,
        };
    }

    private BossPatternKind ChooseBossPattern(Enemy boss, int sector, float hpRatio)
    {
        BossPatternKind chosen = BossPatternKind.AimedFan;
        for (int attempt = 0; attempt < 4; attempt++)
        {
            chosen = RollBossPattern(boss.BossArchetype, sector, hpRatio);
            if ((int)chosen != boss.BossLastPattern || attempt == 3)
            {
                break;
            }
        }

        boss.BossLastPattern = (int)chosen;
        return chosen;
    }

    private BossPatternKind RollBossPattern(BossArchetype archetype, int sector, float hpRatio)
    {
        float roll = _rng.Randf();
        float enrage = 1.0f - hpRatio;
        BossPatternKind pattern = archetype switch
        {
            BossArchetype.Prism => roll switch
            {
                < 0.28f => BossPatternKind.HeavyLance,
                < 0.52f => BossPatternKind.CrossBloom,
                < 0.72f => BossPatternKind.AimedFan,
                < 0.88f => BossPatternKind.SpiralRing,
                _ => enrage > 0.35f ? BossPatternKind.ReverseSpiral : BossPatternKind.HazardFan,
            },
            BossArchetype.Swarm => roll switch
            {
                < 0.34f => BossPatternKind.SummonWing,
                < 0.55f => BossPatternKind.MineDrift,
                < 0.72f => BossPatternKind.SpiralRing,
                < 0.88f => BossPatternKind.AimedFan,
                _ => sector >= 3 ? BossPatternKind.WardenCall : BossPatternKind.CrossBloom,
            },
            BossArchetype.Forge => roll switch
            {
                < 0.3f => BossPatternKind.HazardFan,
                < 0.52f => BossPatternKind.HeavyLance,
                < 0.72f => BossPatternKind.MineDrift,
                < 0.88f => BossPatternKind.CrossBloom,
                _ => BossPatternKind.AimedFan,
            },
            BossArchetype.Rift => roll switch
            {
                < 0.28f => BossPatternKind.ReverseSpiral,
                < 0.5f => BossPatternKind.HazardFan,
                < 0.68f => BossPatternKind.CrossBloom,
                < 0.84f => BossPatternKind.HeavyLance,
                _ => sector >= 2 ? BossPatternKind.WardenCall : BossPatternKind.SpiralRing,
            },
            BossArchetype.Mirror => roll switch
            {
                < 0.34f => BossPatternKind.MirrorFork,
                < 0.56f => BossPatternKind.HeavyLance,
                < 0.74f => BossPatternKind.CrossBloom,
                < 0.9f => BossPatternKind.AimedFan,
                _ => enrage > 0.4f ? BossPatternKind.ReverseSpiral : BossPatternKind.HazardFan,
            },
            BossArchetype.Tempest => roll switch
            {
                < 0.36f => BossPatternKind.TempestWheel,
                < 0.58f => BossPatternKind.SpiralRing,
                < 0.76f => BossPatternKind.ReverseSpiral,
                < 0.9f => BossPatternKind.HazardFan,
                _ => BossPatternKind.SummonWing,
            },
            BossArchetype.Bastion => roll switch
            {
                < 0.32f => BossPatternKind.BastionWall,
                < 0.54f => BossPatternKind.HeavyLance,
                < 0.72f => BossPatternKind.WardenCall,
                < 0.88f => BossPatternKind.HazardFan,
                _ => BossPatternKind.MineDrift,
            },
            BossArchetype.Serpent => roll switch
            {
                < 0.36f => BossPatternKind.SerpentCoil,
                < 0.56f => BossPatternKind.MineDrift,
                < 0.74f => BossPatternKind.ReverseSpiral,
                < 0.9f => BossPatternKind.AimedFan,
                _ => BossPatternKind.CrossBloom,
            },
            BossArchetype.Oracle => roll switch
            {
                < 0.34f => BossPatternKind.OracleSnipe,
                < 0.54f => BossPatternKind.HeavyLance,
                < 0.72f => BossPatternKind.CrossBloom,
                < 0.88f => BossPatternKind.WardenCall,
                _ => BossPatternKind.HazardFan,
            },
            _ => roll switch
            {
                < 0.24f => BossPatternKind.AimedFan,
                < 0.45f => BossPatternKind.SpiralRing,
                < 0.62f => BossPatternKind.HeavyLance,
                < 0.8f => BossPatternKind.SummonWing,
                < 0.92f => BossPatternKind.HazardFan,
                _ => sector >= 3 ? BossPatternKind.WardenCall : BossPatternKind.ReverseSpiral,
            },
        };

        return pattern;
    }

    private void ExecuteBossPattern(Enemy boss, BossPatternKind pattern, int sector, float hpRatio)
    {
        float threat = ThreatLevel();
        Vector2 dir = (_playerPos - boss.Pos).LengthSquared() > 0.01f ? (_playerPos - boss.Pos).Normalized() : Vector2.Down;
        float enrage = 1.0f - hpRatio;
        SetBossIntent(boss, pattern);
        switch (pattern)
        {
            case BossPatternKind.AimedFan:
            {
                int count = ScaledEnemyPatternCount(boss.Kind, 9 + sector * 2 + (boss.BossArchetype == BossArchetype.Prism ? 2 : 0));
                float spread = boss.BossArchetype == BossArchetype.Forge ? 0.13f : 0.105f;
                for (int i = 0; i < count; i++)
                {
                    float offset = (i - (count - 1) * 0.5f) * spread;
                    FireEnemy(boss, dir.Rotated(offset), 350.0f + threat * 6.0f + enrage * 170.0f, 1, 0.0f, 12.0f);
                }
                boss.Cooldown = BossPatternCooldown(boss.BossArchetype, 1.02f, hpRatio);
                break;
            }
            case BossPatternKind.SpiralRing:
            {
                int count = ScaledEnemyPatternCount(boss.Kind, 18 + sector * 3);
                float spin = _time * (hpRatio > 0.5f ? 0.62f : -1.05f) + boss.Phase;
                for (int i = 0; i < count; i++)
                {
                    FireEnemy(boss, Vector2.Right.Rotated(spin + i * Mathf.Tau / count), 245.0f + sector * 18.0f + enrage * 96.0f, 1, 0.0f, 9.0f);
                }
                boss.Cooldown = BossPatternCooldown(boss.BossArchetype, 1.2f, hpRatio);
                boss.Polarity = OtherStance(boss.Polarity);
                break;
            }
            case BossPatternKind.HeavyLance:
            {
                int count = ScaledEnemyPatternCount(boss.Kind, boss.BossArchetype == BossArchetype.Prism ? 7 : 5);
                for (int i = 0; i < count; i++)
                {
                    float offset = (i - (count - 1) * 0.5f) * (boss.BossArchetype == BossArchetype.Rift ? 0.3f : 0.36f);
                    FireEnemy(boss, dir.Rotated(offset), 510.0f + sector * 16.0f, 1, 0.0f, 15.0f);
                }
                boss.Cooldown = BossPatternCooldown(boss.BossArchetype, 0.92f, hpRatio);
                break;
            }
            case BossPatternKind.SummonWing:
                SpawnBossAdds(boss, sector, boss.BossArchetype == BossArchetype.Swarm ? 4 : 2);
                EnterOverheat(boss);
                Burst(boss.Pos, BossAccent(boss.BossArchetype), 18 + sector * 3, 340.0f, 0.75f);
                boss.Cooldown = BossPatternCooldown(boss.BossArchetype, 1.82f, hpRatio);
                break;
            case BossPatternKind.HazardFan:
            {
                SpawnHazardLine(sector, true);
                int count = ScaledEnemyPatternCount(boss.Kind, 5 + (boss.BossArchetype == BossArchetype.Forge ? 2 : 0));
                for (int i = 0; i < count; i++)
                {
                    float offset = (i - (count - 1) * 0.5f) * 0.24f;
                    FireEnemy(boss, dir.Rotated(offset), 450.0f + threat * 9.0f, 1, 0.0f, 13.0f);
                }
                boss.Cooldown = BossPatternCooldown(boss.BossArchetype, 1.28f, hpRatio);
                break;
            }
            case BossPatternKind.ReverseSpiral:
            {
                int count = ScaledEnemyPatternCount(boss.Kind, 10 + sector * 2);
                float baseAngle = -_time * (1.25f + enrage * 0.35f) + boss.Phase;
                for (int i = 0; i < count; i++)
                {
                    Vector2 spiral = Vector2.Right.Rotated(baseAngle + i * Mathf.Tau / count);
                    FireEnemy(boss, spiral, 318.0f + i * 8.0f + sector * 8.0f, 1, 0.0f, 8.0f);
                }
                boss.Cooldown = BossPatternCooldown(boss.BossArchetype, 0.86f, hpRatio);
                break;
            }
            case BossPatternKind.WardenCall:
                SpawnBossGuardian(boss, sector);
                FireEnemy(boss, dir, 430.0f + threat * 8.0f, 5, 0.24f, 12.0f);
                boss.Cooldown = BossPatternCooldown(boss.BossArchetype, 1.65f, hpRatio);
                break;
            case BossPatternKind.CrossBloom:
            {
                int count = ScaledEnemyPatternCount(boss.Kind, 8 + sector * 2);
                float baseAngle = boss.Phase + (boss.BossArchetype == BossArchetype.Prism ? Mathf.Pi * 0.25f : 0.0f);
                for (int i = 0; i < count; i++)
                {
                    Vector2 spoke = Vector2.Right.Rotated(baseAngle + i * Mathf.Tau / count);
                    FireEnemy(boss, spoke, 355.0f + sector * 14.0f + enrage * 70.0f, 1, 0.0f, 10.0f);
                }
                FireEnemy(boss, dir, 470.0f + sector * 12.0f, 3, 0.18f, 12.0f);
                boss.Cooldown = BossPatternCooldown(boss.BossArchetype, 1.18f, hpRatio);
                break;
            }
            case BossPatternKind.MineDrift:
                SpawnBossAdds(boss, sector, boss.BossArchetype == BossArchetype.Forge ? 3 : 2, true);
                FireEnemy(boss, Vector2.Down, 285.0f + sector * 12.0f, 7, 0.3f, 9.0f);
                boss.Cooldown = BossPatternCooldown(boss.BossArchetype, 1.55f, hpRatio);
                break;
            case BossPatternKind.MirrorFork:
            {
                int lanes = ScaledEnemyPatternCount(boss.Kind, 7 + sector);
                Vector2 mirrorDir = new(-dir.X, dir.Y);
                for (int i = 0; i < lanes; i++)
                {
                    float offset = (i - (lanes - 1) * 0.5f) * 0.16f;
                    FireEnemy(boss, dir.Rotated(offset), 390.0f + threat * 8.0f + enrage * 120.0f, 1, 0.0f, 10.0f);
                    FireEnemy(boss, mirrorDir.Rotated(-offset), 390.0f + threat * 8.0f + enrage * 120.0f, 1, 0.0f, 10.0f);
                }
                SpawnBossLineHazard(boss.Pos, Vector2.Right, sector, BossAccent(boss.BossArchetype), 28.0f);
                boss.Cooldown = BossPatternCooldown(boss.BossArchetype, 1.18f, hpRatio);
                break;
            }
            case BossPatternKind.TempestWheel:
            {
                int count = ScaledEnemyPatternCount(boss.Kind, 24 + sector * 3);
                float spin = _time * (1.7f + enrage * 0.6f) + boss.Phase;
                for (int i = 0; i < count; i++)
                {
                    Vector2 spoke = Vector2.Right.Rotated(spin + i * Mathf.Tau / count);
                    FireEnemy(boss, spoke, 260.0f + (i % 3) * 38.0f + sector * 12.0f, 1, 0.0f, 8.5f);
                }
                boss.Vel += RandomDirection() * (160.0f + enrage * 120.0f);
                boss.Cooldown = BossPatternCooldown(boss.BossArchetype, 1.06f, hpRatio);
                break;
            }
            case BossPatternKind.BastionWall:
            {
                boss.BossGuard = Mathf.Max(boss.BossGuard, 1.35f + sector * 0.08f);
                SpawnBossLineHazard(boss.Pos + new Vector2(0.0f, 120.0f), Vector2.Right, sector, BossAccent(boss.BossArchetype), 42.0f);
                SpawnBossAdds(boss, sector, 2 + Math.Min(2, sector), false);
                int count = ScaledEnemyPatternCount(boss.Kind, 5 + sector);
                for (int i = 0; i < count; i++)
                {
                    float offset = (i - (count - 1) * 0.5f) * 0.28f;
                    FireEnemy(boss, dir.Rotated(offset), 470.0f + sector * 20.0f, 1, 0.0f, 14.0f);
                }
                boss.Cooldown = BossPatternCooldown(boss.BossArchetype, 1.78f, hpRatio);
                break;
            }
            case BossPatternKind.SerpentCoil:
            {
                int count = ScaledEnemyPatternCount(boss.Kind, 16 + sector * 2);
                float spin = boss.Phase - _time * (1.0f + enrage * 0.35f);
                for (int i = 0; i < count; i++)
                {
                    Vector2 coil = Vector2.Right.Rotated(spin + i * Mathf.Tau / count);
                    FireEnemy(boss, coil, 285.0f + (i % 4) * 26.0f + sector * 10.0f, 1, 0.0f, 8.5f);
                }
                SpawnBossAdds(boss, sector, 2 + (enrage > 0.45f ? 1 : 0), true);
                boss.Cooldown = BossPatternCooldown(boss.BossArchetype, 1.26f, hpRatio);
                break;
            }
            case BossPatternKind.OracleSnipe:
            {
                Vector2 predicted = _playerPos + _playerVel * (0.38f + enrage * 0.2f);
                Vector2 aim = (predicted - boss.Pos).LengthSquared() > 0.01f ? (predicted - boss.Pos).Normalized() : dir;
                SpawnBossLineHazard(predicted, aim, sector, BossAccent(boss.BossArchetype), 22.0f);
                FireEnemy(boss, aim, 620.0f + sector * 28.0f + enrage * 120.0f, 3, 0.11f, 12.0f);
                if (enrage > 0.35f)
                {
                    FireEnemy(boss, aim.Rotated(0.36f), 500.0f + sector * 24.0f, 1, 0.0f, 10.0f);
                    FireEnemy(boss, aim.Rotated(-0.36f), 500.0f + sector * 24.0f, 1, 0.0f, 10.0f);
                }
                boss.Cooldown = BossPatternCooldown(boss.BossArchetype, 1.42f, hpRatio);
                break;
            }
        }

        if (_rng.Randf() < 0.32f + enrage * 0.18f)
        {
            boss.Polarity = OtherStance(boss.Polarity);
        }
    }

    private static void SetBossIntent(Enemy boss, BossPatternKind pattern, float pulse = 1.45f)
    {
        boss.BossIntent = pattern;
        boss.BossIntentPulse = Math.Max(boss.BossIntentPulse, pulse);
    }

    private float BossPatternCooldown(BossArchetype archetype, float baseCooldown, float hpRatio)
    {
        float styleScale = archetype switch
        {
            BossArchetype.Prism => 0.9f,
            BossArchetype.Swarm => 1.04f,
            BossArchetype.Forge => 1.12f,
            BossArchetype.Rift => 0.96f,
            BossArchetype.Mirror => 0.92f,
            BossArchetype.Tempest => 0.88f,
            BossArchetype.Bastion => 1.18f,
            BossArchetype.Serpent => 0.98f,
            BossArchetype.Oracle => 1.04f,
            _ => 1.0f,
        };
        return Mathf.Max(0.48f, baseCooldown * styleScale * (0.92f - (1.0f - hpRatio) * 0.22f) * DifficultyBossCooldownScale());
    }

    private void SpawnBossAdds(Enemy boss, int sector, int count, bool preferMines = false)
    {
        int cap = boss.BossArchetype == BossArchetype.Swarm ? 18 + sector * 3 : 13 + sector * 2;
        for (int i = 0; i < count && _enemies.Count < cap; i++)
        {
            EnemyKind kind = BossAddKind(boss.BossArchetype, sector, preferMines, i);
            Vector2 pos = ClampToArena(boss.Pos + RandomDirection() * _rng.RandfRange(100.0f, 185.0f), 52.0f);
            SpawnEnemy(kind, pos, i % 2 == 0 ? boss.Polarity : OtherStance(boss.Polarity));
        }
    }

    private static EnemyKind BossAddKind(BossArchetype archetype, int sector, bool preferMines, int index)
    {
        if (preferMines)
        {
            return sector >= 1 && index % 2 == 0 ? EnemyKind.Mine : EnemyKind.Drifter;
        }

        return archetype switch
        {
            BossArchetype.Prism => index % 2 == 0 ? EnemyKind.Weaver : EnemyKind.Lance,
            BossArchetype.Swarm => sector >= 3 && index == 0 ? EnemyKind.Harrier : index % 2 == 0 ? EnemyKind.Shard : EnemyKind.Chaser,
            BossArchetype.Forge => index % 2 == 0 ? EnemyKind.Mine : EnemyKind.Bulwark,
            BossArchetype.Rift => index % 2 == 0 ? EnemyKind.Drifter : EnemyKind.Shard,
            BossArchetype.Mirror => index % 2 == 0 ? EnemyKind.Lance : EnemyKind.Weaver,
            BossArchetype.Tempest => index % 2 == 0 ? EnemyKind.Harrier : EnemyKind.Shard,
            BossArchetype.Bastion => index % 2 == 0 ? EnemyKind.Bulwark : EnemyKind.Warden,
            BossArchetype.Serpent => index % 2 == 0 ? EnemyKind.Drifter : EnemyKind.Mine,
            BossArchetype.Oracle => index % 2 == 0 ? EnemyKind.Siren : EnemyKind.Lance,
            _ => sector >= 2 && index == 0 ? EnemyKind.Shard : index % 2 == 0 ? EnemyKind.Weaver : EnemyKind.Chaser,
        };
    }

    private void SpawnBossGuardian(Enemy boss, int sector)
    {
        if (_enemies.Count >= 18 + sector * 4)
        {
            return;
        }

        EnemyKind kind = sector >= 3 ? EnemyKind.Warden : sector >= 1 ? EnemyKind.Bulwark : EnemyKind.Lance;
        Vector2 pos = ClampToArena(boss.Pos + RandomDirection() * 170.0f, 70.0f);
        SpawnEnemy(kind, pos, boss.Polarity);
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
        float bulletLoad = ActiveEnemyBulletCount() / (float)Math.Max(1, EnemyBulletCap());
        float pressure = PerformancePressure();
        if (enemy.Kind == EnemyKind.Boss)
        {
            if (bulletLoad > 0.9f || pressure > 0.92f)
            {
                shotCount = Math.Max(1, Mathf.CeilToInt(shotCount * 0.68f));
            }
            else if (bulletLoad > 0.78f || pressure > 0.82f)
            {
                shotCount = Math.Max(1, Mathf.CeilToInt(shotCount * 0.84f));
            }
        }
        else
        {
            if (bulletLoad > 0.88f || pressure > 0.92f)
            {
                shotCount = Math.Max(1, Mathf.CeilToInt(shotCount * 0.55f));
            }
            else if (bulletLoad > 0.74f || pressure > 0.82f)
            {
                shotCount = Math.Max(1, Mathf.CeilToInt(shotCount * 0.75f));
            }
        }
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
            shot.Damage = (enemy.Kind == EnemyKind.Boss ? 20.0f + CurrentSectorIndex() * 3.0f : 9.0f + ThreatLevel() * 0.65f) * DifficultyEnemyDamageScale();
            shot.Life = shotLife;
            shot.MaxLife = shotLife;
            shot.Polarity = -1;
            shot.Pierce = 0;
            shot.Rift = false;
        }
    }

    private void UpdateShots(float dt)
    {
        for (int i = _shots.Count - 1; i >= 0; i--)
        {
            Shot shot = _shots[i];
            shot.Prev = shot.Pos;
            shot.Pos += shot.Vel * dt;
            shot.Life -= dt;

            if (!shot.FromPlayer && _enemySlow < 0.99f)
            {
                shot.Vel *= 1.0f - dt * 0.06f;
            }
            if (!shot.FromPlayer && _stasisField > 0)
            {
                shot.Vel *= 1.0f - dt * Mathf.Min(0.18f, 0.035f * _stasisField);
            }

            if (shot.Life <= 0.0f || !ShotCullBounds.HasPoint(shot.Pos))
            {
                RemoveShotAt(i);
            }
        }
    }

    private void ResolveCombat(float dt)
    {
        bool enemyGridBuilt = false;
        float pressure = PerformancePressure();
        Vector2 playerPos = _playerPos;
        for (int i = _shots.Count - 1; i >= 0; i--)
        {
            Shot shot = _shots[i];
            if (shot.FromPlayer)
            {
                if (!enemyGridBuilt)
                {
                    BuildEnemyGrid();
                    enemyGridBuilt = true;
                }

                bool removeShot = false;
                int cellX = EnemyGridCoordX(shot.Pos.X);
                int cellY = EnemyGridCoordY(shot.Pos.Y);
                for (int y = Math.Max(0, cellY - 1); y <= Math.Min(EnemyGridRows - 1, cellY + 1) && !removeShot; y++)
                {
                    for (int x = Math.Max(0, cellX - 1); x <= Math.Min(EnemyGridColumns - 1, cellX + 1) && !removeShot; x++)
                    {
                        List<Enemy> bucket = _enemyGrid[y * EnemyGridColumns + x];
                        for (int j = bucket.Count - 1; j >= 0; j--)
                        {
                            Enemy enemy = bucket[j];
                            if (enemy.Hp <= 0.0f)
                            {
                                continue;
                            }

                            float hitRadius = shot.Radius + enemy.Radius;
                            float deltaX = shot.Pos.X - enemy.Pos.X;
                            float deltaY = shot.Pos.Y - enemy.Pos.Y;
                            float dx = Mathf.Abs(deltaX);
                            float dy = Mathf.Abs(deltaY);
                            if (dx <= hitRadius && dy <= hitRadius && deltaX * deltaX + deltaY * deltaY <= hitRadius * hitRadius)
                            {
                                bool tacticalShot = _assaultBurstTimer > 0.0f;
                                bool overheated = EnemyOverheat01(enemy) > 0.0f;
                                float windowBonus = overheated
                                    ? (tacticalShot ? 1.18f + (_critMultiplier - 1.0f) * 0.72f + (enemy.Elite ? 0.14f : 0.0f) : 1.06f)
                                    : (tacticalShot ? 1.08f : 1.0f);
                                float damage = shot.Damage * windowBonus * _nextWaveDamageBoost;
                                if (_executionMark > 0 && (overheated || enemy.Hp / Mathf.Max(1.0f, enemy.MaxHp) < 0.34f || (enemy.Kind == EnemyKind.Boss && enemy.BossGuard <= 0.0f)))
                                {
                                    damage *= 1.0f + _executionMark * 0.075f + (IsUpgradeMaxed(UpgradeId.ExecutionMark) ? 0.12f : 0.0f);
                                }
                                bool lethal = enemy.Hp - damage <= 0.0f;
                                Vector2 hitPos = enemy.Pos;
                                DamageEnemy(enemy, damage, shot.Pos, false, shot.ChainDepth, shot.SplitDepth);
                                TryChainReaction(shot, enemy, hitPos, damage, overheated);
                                TryRicochetMatrix(shot, enemy, hitPos, damage);
                                if (!tacticalShot)
                                {
                                    AddCruiseCharge(overheated ? 1.2f : 1.9f, hitPos);
                                }
                                else if (overheated && lethal)
                                {
                                    _assaultBurstTimer = Mathf.Min(AssaultBurstMax, _assaultBurstTimer + 0.26f);
                                }
                                float textChance = pressure > 0.86f ? 0.04f : pressure > 0.68f ? 0.1f : 0.22f;
                                if (_damageTexts.Count < MaxDamageTexts * 0.6f || _rng.Randf() < textChance)
                                {
                                    string damageLabel = overheated && tacticalShot ? $"{(int)damage}!" : ((int)damage).ToString();
                                    AddText(damageLabel, hitPos + RandomDirection() * 26.0f, PolarityColor(shot.Polarity), overheated && tacticalShot ? 23.0f : 20.0f);
                                }
                                if (overheated && tacticalShot && _counterTextCooldown <= 0.0f)
                                {
                                    AddText(CounterText(), hitPos + new Vector2(0.0f, -60.0f), Gold, 19.0f);
                                    _counterTextCooldown = 0.42f;
                                }
                                Burst(shot.Pos, PolarityColor(shot.Polarity), pressure > 0.82f ? (shot.Rift ? 4 : 3) : (shot.Rift ? 8 : 5), shot.Rift ? 360.0f : 210.0f, 0.42f);
                                _energy = Mathf.Clamp(_energy + (overheated && tacticalShot ? 1.8f : 0.75f) * _absorbEfficiency, 0.0f, _maxEnergy);

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
                float deltaX = shot.Pos.X - playerPos.X;
                float deltaY = shot.Pos.Y - playerPos.Y;
                float distanceSquared = deltaX * deltaX + deltaY * deltaY;
                if (!shot.Grazed && _dashTimer <= 0.0f)
                {
                    float grazeRadius = hitRadius + CruiseGrazeRadius;
                    if (distanceSquared <= grazeRadius * grazeRadius && distanceSquared > hitRadius * hitRadius)
                    {
                        shot.Grazed = true;
                        AddCruiseCharge(5.2f, shot.Pos);
                        _energy = Mathf.Clamp(_energy + 1.15f * _absorbEfficiency, 0.0f, _maxEnergy);
                        AddRunScore(8 + Math.Min(_combo, 40), shot.Pos, PolarityBlue);
                        IncreaseCombo(shot.Pos);
                        AddObjectiveProgress(RunObjectiveKind.AbsorbBullets, 1);
                        if (_absorbTextCooldown <= 0.0f)
                        {
                            AddText(ChargeText(), playerPos + new Vector2(0.0f, -72.0f), PolarityBlue, 18.0f);
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
                        AddRunScore(15 + _combo, shot.Pos, EnemyBulletColor());
                        IncreaseCombo(shot.Pos);
                        AddObjectiveProgress(RunObjectiveKind.AbsorbBullets, 1);
                        if (_absorbTextCooldown <= 0.0f)
                        {
                            AddText(AbsorbText(absorbGain), playerPos + new Vector2(0.0f, -72.0f), EnemyBulletColor(), 20.0f);
                            _absorbTextCooldown = 0.18f;
                        }
                        Burst(shot.Pos, EnemyBulletColor(), 8, 280.0f, 0.55f);
                        PlaySfx(740.0f, -360.0f, 0.08f, 0.12f, 0.02f, 2);
                    }
                    else
                    {
                        float incoming = shot.Damage * 0.92f;
                        AddCruiseCharge(3.0f, shot.Pos);
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
            float deltaX = enemy.Pos.X - playerPos.X;
            float deltaY = enemy.Pos.Y - playerPos.Y;
            if (deltaX * deltaX + deltaY * deltaY <= hitRadius * hitRadius)
            {
                if (_dashTimer > 0.0f)
                {
                    DamageEnemy(enemy, _dashDamage * _damageMultiplier, playerPos, false);
                    Burst(enemy.Pos, PilotAccent(_runPilot), 12, 420.0f, 0.55f);
                }
                else if (enemy.ContactTimer <= 0.0f)
                {
                    DamagePlayer(enemy.Kind == EnemyKind.Boss ? 23.0f : 11.0f + CurrentSectorIndex() * 1.6f, enemy.Pos);
                    enemy.ContactTimer = 0.6f;
                }
            }
        }
    }

    private void DamageEnemy(Enemy enemy, float damage, Vector2 source, bool heavy, int chainDepth = 0, int splitDepth = 0)
    {
        enemy.LastHitChainDepth = Math.Max(0, chainDepth);
        enemy.LastHitSplitDepth = Math.Max(0, splitDepth);
        float previousHp = enemy.Hp;
        float finalDamage = damage / Mathf.Max(0.1f, enemy.Armor);
        if (enemy.Kind == EnemyKind.Boss)
        {
            finalDamage *= BossDamageTakenScale(enemy, heavy, chainDepth, splitDepth);
        }
        else
        {
            finalDamage *= DifficultyEnemyDamageTakenScale(enemy);
        }
        enemy.Hp -= finalDamage;
        Vector2 knock = enemy.Pos - source;
        if (knock.LengthSquared() > 0.01f)
        {
            enemy.Vel += knock.Normalized() * (heavy ? 180.0f : 46.0f);
        }

        if (enemy.Kind == EnemyKind.Boss && TryTriggerBossPhase(enemy, previousHp))
        {
            return;
        }

        if (enemy.Hp <= 0.0f)
        {
            KillEnemy(enemy);
        }
    }

    private static float BossDamageTakenScale(Enemy boss, bool heavy, int chainDepth, int splitDepth)
    {
        float scale = heavy ? 0.82f : 0.74f;
        if (chainDepth > 0)
        {
            scale *= Mathf.Lerp(0.76f, 0.54f, Mathf.Clamp(chainDepth / 4.0f, 0.0f, 1.0f));
        }
        if (splitDepth > 0)
        {
            scale *= splitDepth >= 2 ? 0.42f : 0.58f;
        }
        if (boss.BossGuard > 0.0f)
        {
            scale *= Mathf.Lerp(0.24f, 0.58f, 1.0f - Mathf.Clamp(boss.BossGuard / 1.2f, 0.0f, 1.0f));
        }
        return scale;
    }

    private bool TryTriggerBossPhase(Enemy boss, float previousHp)
    {
        if (boss.BossPhase >= BossPhaseThresholds.Length)
        {
            return false;
        }

        float previousRatio = Mathf.Clamp(previousHp / Mathf.Max(1.0f, boss.MaxHp), 0.0f, 1.0f);
        float currentRatio = Mathf.Clamp(boss.Hp / Mathf.Max(1.0f, boss.MaxHp), 0.0f, 1.0f);
        float threshold = BossPhaseThresholds[boss.BossPhase];
        if (previousRatio <= threshold || currentRatio > threshold)
        {
            return false;
        }

        boss.Hp = Mathf.Max(boss.Hp, boss.MaxHp * threshold);
        boss.BossPhase++;
        TriggerBossPhaseShift(boss, threshold);
        return true;
    }

    private void TriggerBossPhaseShift(Enemy boss, float threshold)
    {
        int sector = CurrentSectorIndex();
        Color color = BossAccent(boss.BossArchetype);
        boss.BossGuard = 0.86f + boss.BossPhase * 0.18f;
        boss.Cooldown = 0.18f;
        boss.Overheat = 0.0f;
        boss.Phase += 0.72f + boss.BossPhase * 0.18f;
        _shake = Mathf.Max(_shake, 0.72f);
        _flash = Mathf.Max(_flash, 0.42f);

        string text = Tf("boss.phase", boss.BossPhase + 1);
        AddText(text, boss.Pos + new Vector2(0.0f, -boss.Radius - 42.0f), color, 30.0f);
        ClearBulletsNear(_playerPos, 170.0f + boss.BossPhase * 28.0f, false);
        Burst(boss.Pos, color, PerformancePressure() > 0.8f ? 36 : 68, 660.0f + sector * 80.0f, 1.08f);
        TriggerBossSignatureShift(boss, sector, color);

        int addCount = boss.BossArchetype == BossArchetype.Swarm ? 3 + boss.BossPhase : 1 + boss.BossPhase;
        if (boss.BossPhase >= 2)
        {
            SpawnBossAdds(boss, sector, addCount, boss.BossArchetype == BossArchetype.Forge);
        }

        int ringCount = ScaledEnemyPatternCount(boss.Kind, 7 + sector + boss.BossPhase * 2);
        float spin = boss.Phase + boss.BossPhase * 0.33f;
        for (int i = 0; i < ringCount; i++)
        {
            Vector2 dir = Vector2.Right.Rotated(spin + i * Mathf.Tau / ringCount);
            FireEnemy(boss, dir, 285.0f + sector * 22.0f + boss.BossPhase * 34.0f, 1, 0.0f, 9.0f + boss.BossPhase);
        }
        boss.Overheat = 0.0f;
        boss.OverheatMax = 1.0f;
        boss.Cooldown = 0.52f;

        if (threshold <= 0.43f)
        {
            SpawnHazardLine(sector, true);
        }
    }

    private void TriggerBossSignatureShift(Enemy boss, int sector, Color color)
    {
        float pressure = PerformancePressure();
        Vector2 aim = (_playerPos - boss.Pos).LengthSquared() > 0.01f ? (_playerPos - boss.Pos).Normalized() : Vector2.Down;
        int phase = boss.BossPhase;
        switch (boss.BossArchetype)
        {
            case BossArchetype.Prism:
                SetBossIntent(boss, BossPatternKind.CrossBloom, 1.9f);
                SpawnBossLineHazard(boss.Pos, Vector2.Right.Rotated(Mathf.Pi * 0.25f), sector, color, 20.0f);
                if (pressure < 0.86f || phase >= 2)
                {
                    SpawnBossLineHazard(boss.Pos, Vector2.Right.Rotated(-Mathf.Pi * 0.25f), sector, color, 18.0f);
                }
                break;
            case BossArchetype.Swarm:
                SetBossIntent(boss, BossPatternKind.SummonWing, 1.9f);
                SpawnBossAdds(boss, sector, 2 + phase, false);
                if (pressure < 0.82f)
                {
                    FireEnemy(boss, Vector2.Down, 240.0f + sector * 18.0f, 5 + phase, 0.32f, 8.5f);
                }
                break;
            case BossArchetype.Forge:
                SetBossIntent(boss, BossPatternKind.BastionWall, 1.9f);
                boss.BossGuard = Mathf.Max(boss.BossGuard, 1.18f + phase * 0.16f);
                SpawnBossLineHazard(boss.Pos + new Vector2(0.0f, 92.0f), Vector2.Right, sector, color, 34.0f);
                SpawnBossAdds(boss, sector, phase >= 2 ? 3 : 2, true);
                break;
            case BossArchetype.Rift:
                SetBossIntent(boss, BossPatternKind.HazardFan, 1.9f);
                SpawnBossLineHazard(boss.Pos, Vector2.Down.Rotated(0.38f), sector, color, 24.0f);
                if (phase >= 2 && pressure < 0.88f)
                {
                    SpawnBossLineHazard(boss.Pos, Vector2.Down.Rotated(-0.38f), sector, color, 20.0f);
                }
                break;
            case BossArchetype.Mirror:
            {
                SetBossIntent(boss, BossPatternKind.MirrorFork, 1.9f);
                Vector2 mirror = new(-aim.X, aim.Y);
                FireEnemy(boss, aim.Rotated(0.18f), 360.0f + sector * 22.0f, 2 + phase, 0.18f, 9.0f);
                FireEnemy(boss, mirror.Rotated(-0.18f), 360.0f + sector * 22.0f, 2 + phase, 0.18f, 9.0f);
                break;
            }
            case BossArchetype.Tempest:
                SetBossIntent(boss, BossPatternKind.TempestWheel, 1.9f);
                boss.Vel += RandomDirection() * (220.0f + phase * 70.0f);
                if (pressure < 0.86f)
                {
                    int count = ScaledEnemyPatternCount(boss.Kind, 12 + sector + phase * 2);
                    for (int i = 0; i < count; i++)
                    {
                        Vector2 dir = Vector2.Right.Rotated(boss.Phase + i * Mathf.Tau / count);
                        FireEnemy(boss, dir, 250.0f + (i % 3) * 30.0f + phase * 18.0f, 1, 0.0f, 8.0f);
                    }
                }
                break;
            case BossArchetype.Bastion:
                SetBossIntent(boss, BossPatternKind.BastionWall, 1.9f);
                boss.BossGuard = Mathf.Max(boss.BossGuard, 1.35f + phase * 0.2f);
                SpawnBossLineHazard(boss.Pos + new Vector2(0.0f, 130.0f), Vector2.Right, sector, color, 46.0f);
                SpawnBossGuardian(boss, sector);
                break;
            case BossArchetype.Serpent:
                SetBossIntent(boss, BossPatternKind.SerpentCoil, 1.9f);
                SpawnBossAdds(boss, sector, 1 + phase, true);
                if (pressure < 0.88f)
                {
                    int count = ScaledEnemyPatternCount(boss.Kind, 9 + phase * 2);
                    for (int i = 0; i < count; i++)
                    {
                        Vector2 coil = Vector2.Right.Rotated(boss.Phase - _time * 0.6f + i * Mathf.Tau / count);
                        FireEnemy(boss, coil, 250.0f + (i % 4) * 24.0f + sector * 10.0f, 1, 0.0f, 8.0f);
                    }
                }
                break;
            case BossArchetype.Oracle:
            {
                SetBossIntent(boss, BossPatternKind.OracleSnipe, 1.9f);
                Vector2 predicted = _playerPos + _playerVel * (0.42f + phase * 0.08f);
                Vector2 snipe = (predicted - boss.Pos).LengthSquared() > 0.01f ? (predicted - boss.Pos).Normalized() : aim;
                SpawnBossLineHazard(predicted, snipe, sector, color, 20.0f + phase * 2.0f);
                FireEnemy(boss, snipe, 540.0f + sector * 24.0f + phase * 40.0f, 2 + phase, 0.1f, 10.0f);
                break;
            }
            default:
                SetBossIntent(boss, BossPatternKind.SpiralRing, 1.9f);
                break;
        }

        AddText(BossSignatureText(boss.BossArchetype), boss.Pos + new Vector2(0.0f, boss.Radius + 42.0f), Alpha(color.Lerp(Paper, 0.12f), 0.88f), 18.0f);
    }

    private void TryChainReaction(Shot shot, Enemy excluded, Vector2 start, float baseDamage, bool overheated)
    {
        int rank = _chainRelay;
        if (_runPilot == PilotKind.Kairo)
        {
            rank += _kairoRelayProtocol;
        }
        if (_runPilot == PilotKind.Sol)
        {
            rank += _solFlareCore / 2;
        }
        rank = Math.Min(rank, 8);
        if (rank <= 0 || baseDamage <= 0.0f)
        {
            return;
        }

        bool capstone = IsUpgradeMaxed(UpgradeId.ChainRelay);
        float chance = 0.18f + rank * 0.07f + (overheated ? 0.12f : 0.0f) + (shot.Rift ? 0.04f : 0.0f);
        if (capstone)
        {
            chance += 0.28f;
        }
        if (shot.SplitDepth > 0)
        {
            chance *= capstone ? 0.72f : 0.55f;
        }
        if (_rng.Randf() > Mathf.Clamp(chance, 0.0f, capstone ? 0.94f : 0.78f))
        {
            return;
        }

        int jumps = Math.Min(capstone ? 6 : 4, 1 + rank / 2 + (shot.Rift && rank >= 4 ? 1 : 0) + (capstone ? 1 : 0));
        if (_visualPressure > 0.9f)
        {
            jumps = Math.Min(jumps, 2);
        }
        else if (_visualPressure > 0.76f)
        {
            jumps = Math.Min(jumps, 3);
        }
        float radius = 245.0f + rank * 34.0f + (capstone ? 70.0f : 0.0f);
        float chainDamage = baseDamage * (0.24f + rank * 0.035f + (capstone ? 0.05f : 0.0f)) * (shot.SplitDepth > 0 ? 0.72f : 1.0f);
        Vector2 from = start;
        Enemy? previous = excluded;
        Color color = UpgradeAccent(UpgradeId.ChainRelay);
        int actualJumps = 0;

        for (int i = 0; i < jumps; i++)
        {
            Enemy? target = FindChainTarget(from, previous, radius);
            if (target == null)
            {
                break;
            }

            Vector2 targetPos = target.Pos;
            SpawnChainArc(from, targetPos, color);
            DamageEnemy(target, chainDamage, from, false, shot.ChainDepth + i + 1, shot.SplitDepth);
            AddRunScore(14 + rank * 4, targetPos, color);
            from = targetPos;
            previous = target;
            chainDamage *= 0.68f;
            actualJumps++;
        }

        if (capstone && actualJumps >= 2 && PerformancePressure() < 0.92f)
        {
            SpawnRelayCapstoneBurst(from, color, baseDamage);
        }
    }

    private void TryRicochetMatrix(Shot shot, Enemy excluded, Vector2 start, float baseDamage)
    {
        if (_ricochetMatrix <= 0 || shot.ChainDepth > 3 || _shots.Count > MaxShots * 0.9f)
        {
            return;
        }

        float chance = 0.16f + _ricochetMatrix * 0.055f + (shot.Rift ? 0.05f : 0.0f);
        if (IsUpgradeMaxed(UpgradeId.RicochetMatrix))
        {
            chance += 0.18f;
        }
        if (_rng.Randf() > Mathf.Clamp(chance, 0.0f, 0.82f))
        {
            return;
        }

        int bounces = Math.Min(IsUpgradeMaxed(UpgradeId.RicochetMatrix) ? 3 : 2, 1 + _ricochetMatrix / 3);
        if (PerformancePressure() > 0.84f)
        {
            bounces = Math.Min(bounces, 1);
        }
        for (int i = 0; i < bounces; i++)
        {
            Enemy? target = FindChainTarget(start, excluded, 360.0f + _ricochetMatrix * 38.0f);
            if (target == null)
            {
                break;
            }

            Vector2 dir = (target.Pos - start).LengthSquared() > 0.01f ? (target.Pos - start).Normalized() : RandomDirection();
            SpawnPlayerShot(start + dir * 18.0f, dir, 1180.0f, Mathf.Max(3.8f, shot.Radius * 0.82f), Mathf.Max(6.0f, baseDamage * (0.22f + _ricochetMatrix * 0.018f)), 0.42f, 0, true, shot.Polarity);
            SpawnChainArc(start, target.Pos, UpgradeAccent(UpgradeId.RicochetMatrix));
            start = target.Pos;
            excluded = target;
        }
    }

    private void SpawnRelayCapstoneBurst(Vector2 pos, Color color, float baseDamage)
    {
        if (_shots.Count > MaxShots * 0.86f)
        {
            return;
        }

        float pressure = PerformancePressure();
        int shards = pressure > 0.8f ? 3 : 5;
        float damage = Mathf.Max(7.0f, baseDamage * 0.16f);
        for (int i = 0; i < shards; i++)
        {
            Vector2 dir = Vector2.Right.Rotated(_time * 0.5f + i * Mathf.Tau / shards);
            SpawnPlayerShot(pos + dir * 18.0f, dir, 900.0f, 4.2f, damage, 0.46f, 0, true);
        }
        Burst(pos, color, pressure > 0.82f ? 5 : 10, 280.0f, 0.32f);
    }

    private Enemy? FindChainTarget(Vector2 from, Enemy? excluded, float radius)
    {
        Enemy? best = null;
        float bestDistance = radius * radius;
        for (int i = 0; i < _enemies.Count; i++)
        {
            Enemy enemy = _enemies[i];
            if (ReferenceEquals(enemy, excluded) || enemy.Hp <= 0.0f)
            {
                continue;
            }

            float deltaX = enemy.Pos.X - from.X;
            float deltaY = enemy.Pos.Y - from.Y;
            float distance = deltaX * deltaX + deltaY * deltaY;
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = enemy;
            }
        }
        return best;
    }

    private void SpawnChainArc(Vector2 from, Vector2 to, Color color)
    {
        Vector2 delta = to - from;
        Vector2 forward = delta.LengthSquared() > 0.01f ? delta.Normalized() : Vector2.Right;
        float pressure = PerformancePressure();
        if (pressure > 0.92f)
        {
            AddParticle(from.Lerp(to, 0.5f), forward * 120.0f, color, 4.0f, 0.18f);
            return;
        }

        int sparks = pressure > 0.78f ? 2 : 6;
        for (int i = 0; i < sparks; i++)
        {
            float t = (i + 1.0f) / (sparks + 1.0f);
            Vector2 point = from.Lerp(to, t) + RandomDirection() * _rng.RandfRange(4.0f, 18.0f);
            AddParticle(point, RandomDirection() * _rng.RandfRange(80.0f, 220.0f) + forward * 90.0f, color, 5.0f, 0.22f);
        }
        Burst(to, color, pressure > 0.82f ? 4 : 8, 240.0f, 0.36f);
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
        IncreaseCombo(enemy.Pos);
        int multiplier = ScoreMultiplier();
        int scoreGain = enemy.Value * multiplier;
        if (enemy.Kind == EnemyKind.Boss)
        {
            scoreGain *= 2;
        }
        AddRunScore(scoreGain, enemy.Pos, color);
        _energy = Mathf.Clamp(_energy + 6.0f, 0.0f, _maxEnergy);
        _runKills++;
        AddObjectiveProgress(RunObjectiveKind.DefeatEnemies, 1);
        if (enemy.Kind == EnemyKind.Boss)
        {
            _waveProgressSpent = Math.Max(_waveProgressSpent, _waveProgressBudget);
            _waveSpawnTimer = 0.0f;
            _waveSpawnInterval = 0.0f;
            _waveNextSpawnCount = 0;
            _runBossKills++;
            AddObjectiveProgress(RunObjectiveKind.DefeatBosses, 1);
            AddText(Tf("sector.cleared", T(CurrentSector().NameKey)), ScreenCenter + new Vector2(0.0f, -225.0f), CurrentSector().Accent, 36.0f);
        }
        PlaySfx(enemy.Kind == EnemyKind.Boss ? 64.0f : 180.0f, enemy.Kind == EnemyKind.Boss ? 0.2f : -50.0f, enemy.Kind == EnemyKind.Boss ? 1.4f : 0.16f, enemy.Kind == EnemyKind.Boss ? 0.62f : 0.28f, enemy.Kind == EnemyKind.Boss ? 0.38f : 0.08f, enemy.Kind == EnemyKind.Boss ? 0 : 2);

        int drops = enemy.Kind == EnemyKind.Boss ? 32 + CurrentSectorIndex() * 6 : 2 + (enemy.Kind == EnemyKind.Splitter ? 2 : 0) + (enemy.Elite ? 4 : 0);
        for (int i = 0; i < drops; i++)
        {
            SpawnPickup(enemy.Pos, PickupKind.Dust);
        }
        if (_rng.Randf() < 0.08f + _leechChance + (CurrentSectorIndex() == 2 ? 0.06f : 0.0f) + (enemy.Elite ? 0.05f : 0.0f))
        {
            SpawnPickup(enemy.Pos, PickupKind.Repair);
        }

        SpawnFractalSplit(enemy);

        if (enemy.Kind == EnemyKind.Splitter && enemy.SplitDepth < 1)
        {
            SpawnEnemy(EnemyKind.Chaser, ClampToArena(enemy.Pos + new Vector2(-42.0f, 36.0f), 30.0f), OtherStance(enemy.Polarity), enemy.SplitDepth + 1);
            SpawnEnemy(EnemyKind.Chaser, ClampToArena(enemy.Pos + new Vector2(42.0f, 36.0f), 30.0f), enemy.Polarity, enemy.SplitDepth + 1);
        }
        RecycleEnemy(enemy);
    }

    private void SpawnFractalSplit(Enemy enemy)
    {
        int rank = _fractalSplit;
        if (_runPilot == PilotKind.Astra)
        {
            rank += _astraTwinRefraction;
        }
        if (_runPilot == PilotKind.Vesper)
        {
            rank += _vesperFork / 2;
        }
        rank = Math.Min(rank, 8);
        if (rank <= 0 || _shots.Count > MaxShots * 0.82f)
        {
            return;
        }

        bool capstone = IsUpgradeMaxed(UpgradeId.FractalSplit);
        int sourceSplitDepth = Math.Max(0, enemy.LastHitSplitDepth);
        if (sourceSplitDepth >= (capstone ? 3 : 2))
        {
            return;
        }

        if (sourceSplitDepth == 1)
        {
            float cascadeChance = 0.08f + rank * 0.035f + (enemy.LastHitChainDepth > 0 ? 0.05f : 0.0f) + (capstone ? 0.36f : 0.0f);
            if (PerformancePressure() > 0.84f)
            {
                cascadeChance *= 0.45f;
            }
            if (rank < 4 || _rng.Randf() > cascadeChance)
            {
                return;
            }
        }

        bool boss = enemy.Kind == EnemyKind.Boss;
        int count = boss ? 8 + rank * 2 : 2 + Math.Min(rank, 5) + (enemy.Elite ? 1 : 0) + (capstone && sourceSplitDepth == 0 ? 2 : 0);
        if (sourceSplitDepth > 0)
        {
            count = Math.Min(count, capstone ? 4 : rank >= 6 ? 3 : 2);
        }
        else if (enemy.LastHitChainDepth > 0 && !boss)
        {
            count = Math.Max(2, count - 1);
        }
        float pressure = PerformancePressure();
        if (pressure > 0.82f)
        {
            count = Math.Min(count, boss ? 8 : 4);
        }
        if (pressure > 0.92f)
        {
            count = Math.Min(count, boss ? 5 : 2);
        }

        float baseAngle = _rng.RandfRange(0.0f, Mathf.Tau);
        float damage = (5.5f + rank * 1.45f + CurrentSectorIndex() * 0.7f) * _damageMultiplier * (enemy.Elite ? 1.18f : 1.0f);
        if (sourceSplitDepth > 0)
        {
            damage *= 0.58f;
        }
        float speed = 720.0f + rank * 45.0f;
        Color color = UpgradeAccent(UpgradeId.FractalSplit);
        int shardSplitDepth = sourceSplitDepth + 1;
        for (int i = 0; i < count; i++)
        {
            float angle = baseAngle + i * Mathf.Tau / count + _rng.RandfRange(-0.08f, 0.08f);
            SpawnFractalShard(enemy.Pos, Vector2.Right.Rotated(angle), speed, damage, rank, shardSplitDepth);
        }

        Burst(enemy.Pos, color, pressure > 0.82f ? 8 : 14, 280.0f, 0.44f);
    }

    private void SpawnFractalShard(Vector2 pos, Vector2 dir, float speed, float damage, int rank, int splitDepth)
    {
        Shot? shard = AddShot(true);
        if (shard == null)
        {
            return;
        }

        dir = dir.LengthSquared() < 0.01f ? Vector2.Right : dir.Normalized();
        shard.Pos = pos + dir * 18.0f;
        shard.Prev = pos;
        shard.Vel = dir * speed;
        shard.Radius = 3.6f + Math.Min(rank, 5) * 0.18f;
        shard.Damage = damage * PlayerShotDamageScale(_playerPolarity);
        shard.Life = 0.42f + Math.Min(rank, 6) * 0.035f;
        shard.MaxLife = shard.Life;
        shard.Polarity = _playerPolarity;
        shard.Pierce = IsUpgradeMaxed(UpgradeId.FractalSplit) && splitDepth <= 1 ? 1 : rank >= 5 ? 1 : 0;
        shard.ChainDepth = 0;
        shard.SplitDepth = splitDepth;
        shard.Rift = true;
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
        if (_combo > 0)
        {
            _combo = 0;
            _comboTimer = 0.0f;
            _comboTier = 0;
            _comboTierPulse = 0.0f;
            _waveSpawnTimer = Math.Max(_waveSpawnTimer, 0.75f);
            AddText(T("score.combo_break"), _playerPos + new Vector2(0.0f, -92.0f), Rose, 20.0f);
        }
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
            float distanceSquared = toPlayer.LengthSquared();
            float magnet = _pickupMagnet + (_energy / _maxEnergy) * 80.0f;
            if (distanceSquared < magnet * magnet && distanceSquared > 1.0f)
            {
                float distance = Mathf.Sqrt(distanceSquared);
                pickup.Vel = pickup.Vel.Lerp((toPlayer / distance) * Mathf.Lerp(240.0f, 840.0f, 1.0f - distance / magnet), 1.0f - Mathf.Exp(-dt * 9.0f));
            }
            pickup.Pos += pickup.Vel * dt;
            pickup.Vel *= 1.0f - dt * 2.1f;

            float collectRadius = pickup.Radius + PlayerRadius;
            if (distanceSquared <= collectRadius * collectRadius)
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
        float radius = kind switch
        {
            PickupKind.Dust => 6.2f,
            PickupKind.Repair => 12.0f,
            _ => 8.0f,
        };
        Pickup? pickup = AddPickup();
        if (pickup == null)
        {
            return;
        }

        pickup.Kind = kind;
        pickup.Pos = pos;
        pickup.Vel = RandomDirection() * _rng.RandfRange(80.0f, 240.0f);
        pickup.Radius = radius;
        pickup.Life = kind switch
        {
            PickupKind.Dust => 5.6f,
            PickupKind.Repair => 9.0f,
            _ => 7.0f,
        };
    }

    private void CollectPickup(Pickup pickup)
    {
        _runPickups++;
        AddObjectiveProgress(RunObjectiveKind.CollectPickups, 1);

        switch (pickup.Kind)
        {
            case PickupKind.Dust:
                AddExperience(XpPickupValue(), pickup.Pos);
                if (_magnetizedCore > 0)
                {
                    _energy = Mathf.Clamp(_energy + 0.8f + _magnetizedCore * 0.45f, 0.0f, _maxEnergy);
                    if (_rng.Randf() < 0.12f + _magnetizedCore * 0.035f)
                    {
                        MagnetizedCorePulse(pickup.Pos);
                    }
                }
                break;
            case PickupKind.Energy:
                _energy = Mathf.Clamp(_energy + 12.0f, 0.0f, _maxEnergy);
                if (_magnetizedCore > 0)
                {
                    ClearBulletsNear(pickup.Pos, 42.0f + _magnetizedCore * 12.0f, true);
                }
                break;
            case PickupKind.Repair:
                _playerHp = Mathf.Clamp(_playerHp + 18.0f, 0.0f, _playerMaxHp);
                AddText(T("repair"), _playerPos + new Vector2(0.0f, -70.0f), Jade, 22.0f);
                break;
        }

        _comboTimer = 2.0f;
        Burst(pickup.Pos, PickupColor(pickup.Kind), 5, 180.0f, 0.38f);
    }

    private void MagnetizedCorePulse(Vector2 point)
    {
        Color color = UpgradeAccent(UpgradeId.MagnetizedCore);
        float radius = 120.0f + _magnetizedCore * 24.0f;
        int hits = 0;
        for (int i = _enemies.Count - 1; i >= 0 && hits < 8; i--)
        {
            Enemy enemy = _enemies[i];
            if (enemy.Pos.DistanceSquaredTo(point) > (radius + enemy.Radius) * (radius + enemy.Radius))
            {
                continue;
            }

            DamageEnemy(enemy, (12.0f + _magnetizedCore * 3.5f) * _damageMultiplier, point, false);
            hits++;
        }
        ClearBulletsNear(point, radius * 0.42f, true);
        Burst(point, color, _visualPressure > 0.82f ? 4 : 9, 260.0f, 0.34f);
    }

    private void UpdateParticles(float dt)
    {
        for (int i = _shockwaves.Count - 1; i >= 0; i--)
        {
            Shockwave wave = _shockwaves[i];
            wave.Life -= dt;
            if (wave.Life <= 0.0f)
            {
                _shockwaves.RemoveAt(i);
            }
        }

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

        for (int i = _droneCommandCues.Count - 1; i >= 0; i--)
        {
            DroneCommandCue cue = _droneCommandCues[i];
            cue.Life -= dt;
            cue.Pos += cue.Vel * dt;
            cue.Vel *= 1.0f - dt * 2.4f;
            if (cue.Life <= 0.0f)
            {
                _droneCommandCues.RemoveAt(i);
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
                if (ShouldSpawnHazardField(sector))
                {
                    SpawnHazardField(sector, false);
                }
                else
                {
                    SpawnHazardLine(sector, false);
                }
                _sectorHazardTimer = NextSectorHazardInterval(sector);
            }
        }

        for (int i = _hazardFields.Count - 1; i >= 0; i--)
        {
            HazardField field = _hazardFields[i];
            field.Life -= dt;
            bool active = field.Life < field.MaxLife - field.Warmup;
            if (active)
            {
                Vector2 toCenter = field.Center - _playerPos;
                float distance = toCenter.Length();
                float inside = 1.0f - distance / Math.Max(1.0f, field.Radius);
                if (inside > 0.0f)
                {
                    if (_dashTimer > 0.0f)
                    {
                        float absorbGain = (2.8f + inside * 3.2f) * _absorbEfficiency;
                        _energy = Mathf.Clamp(_energy + absorbGain, 0.0f, _maxEnergy);
                        AddObjectiveProgress(RunObjectiveKind.AbsorbBullets, 2);
                        if (_absorbTextCooldown <= 0.0f)
                        {
                            AddText(AbsorbText(absorbGain), _playerPos + new Vector2(0.0f, -80.0f), field.Color, 21.0f);
                            _absorbTextCooldown = 0.25f;
                        }
                        _invulnTimer = Math.Max(_invulnTimer, 0.16f);
                    }
                    else
                    {
                        Vector2 pullDir = distance > 0.01f ? toCenter / distance : RandomDirection();
                        _playerVel += pullDir * field.Pull * Mathf.Clamp(inside, 0.0f, 1.0f) * dt;
                        if (_invulnTimer <= 0.0f && distance < field.Radius * 0.42f + PlayerRadius)
                        {
                            AddCruiseCharge(4.0f, field.Center);
                            DamagePlayer(field.Damage, field.Center);
                        }
                    }
                }
            }

            if (field.Life <= 0.0f)
            {
                _hazardFields.RemoveAt(i);
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
                        AddCruiseCharge(6.0f, (hazard.A + hazard.B) * 0.5f);
                        DamagePlayer(hazard.Damage * 0.94f, (hazard.A + hazard.B) * 0.5f);
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
        float tempo = DifficultyHazardTempoScale();
        float life = bossCast ? 1.6f : 1.35f;
        float warmup = (bossCast ? 0.72f : 0.82f) / tempo;
        _hazards.Add(new HazardLine
        {
            A = a,
            B = b,
            Color = color,
            Life = life,
            MaxLife = life,
            Warmup = warmup,
            Width = bossCast ? 32.0f + sector * 5.0f : 24.0f + sector * 4.0f,
            Damage = (18.0f + sector * 5.0f) * DifficultyHazardDamageScale(),
            Polarity = polarity,
        });
    }

    private bool ShouldSpawnHazardField(int sector)
    {
        if (sector < 2 || _hazardFields.Count >= 5)
        {
            return false;
        }

        float chance = sector switch
        {
            2 => 0.42f,
            3 => 0.38f,
            _ => 0.52f,
        };

        if (_currentWavePace == WavePaceKind.Recovery)
        {
            chance *= 0.45f;
        }
        else if (_currentWavePace is WavePaceKind.Pressure or WavePaceKind.Swarm)
        {
            chance += 0.08f;
        }

        if (CurrentWaveInSector() >= 6)
        {
            chance += 0.08f;
        }

        chance += WavePressure01() * 0.08f;
        chance += DifficultyIndex(_runDifficulty) * 0.04f;

        return _rng.Randf() < Mathf.Clamp(chance, 0.0f, 0.68f);
    }

    private float NextSectorHazardInterval(int sector)
    {
        int waveInSector = CurrentWaveInSector();
        float interval = 7.4f - sector * 0.92f - waveInSector * 0.07f;
        interval -= WavePressure01() * 0.55f;
        interval += _currentWavePace switch
        {
            WavePaceKind.Recovery => 1.15f,
            WavePaceKind.Elite => 0.42f,
            WavePaceKind.Pressure => -0.34f,
            WavePaceKind.Swarm => -0.22f,
            _ => 0.0f,
        };
        if (waveInSector == 4)
        {
            interval += 0.6f;
        }
        else if (waveInSector >= 7)
        {
            interval -= 0.28f;
        }
        interval /= DifficultyHazardFrequencyScale();
        return Mathf.Clamp(interval, 1.65f, 8.8f);
    }

    private void SpawnHazardField(int sector, bool bossCast)
    {
        if (_hazardFields.Count > 5)
        {
            _hazardFields.RemoveAt(0);
        }

        Vector2 center = ScreenCenter;
        for (int attempt = 0; attempt < 8; attempt++)
        {
            center = new Vector2(
                _rng.RandfRange(Arena.Position.X + 180.0f, Arena.End.X - 180.0f),
                _rng.RandfRange(Arena.Position.Y + 150.0f, Arena.End.Y - 150.0f));
            if (center.DistanceSquaredTo(_playerPos) > 260.0f * 260.0f || attempt >= 6)
            {
                break;
            }
        }

        float radius = sector switch
        {
            2 => 118.0f,
            3 => 132.0f,
            _ => 146.0f,
        } + CurrentWaveInSector() * 1.8f + (bossCast ? 22.0f : 0.0f);

        Color color = sector >= 4 ? Rose.Lerp(Gold, 0.18f) : CurrentSector().Accent.Lerp(EnemyBulletColor(), 0.28f);
        float tempo = DifficultyHazardTempoScale();
        float life = bossCast ? 2.25f : 2.05f;
        _hazardFields.Add(new HazardField
        {
            Center = center,
            Color = color,
            Radius = radius,
            Life = life,
            MaxLife = life,
            Warmup = (bossCast ? 0.78f : 0.86f) / tempo,
            Damage = (13.0f + sector * 3.4f) * DifficultyHazardDamageScale(),
            Pull = (235.0f + sector * 36.0f) * DifficultyHazardTempoScale(),
        });
    }

    private void SpawnBossLineHazard(Vector2 center, Vector2 direction, int sector, Color color, float width)
    {
        if (_hazards.Count > 10)
        {
            _hazards.RemoveAt(0);
        }

        Vector2 dir = direction.LengthSquared() > 0.01f ? direction.Normalized() : Vector2.Right;
        Vector2 a = center - dir * 1180.0f;
        Vector2 b = center + dir * 1180.0f;
        float tempo = DifficultyHazardTempoScale();
        _hazards.Add(new HazardLine
        {
            A = a,
            B = b,
            Color = color.Lerp(EnemyBulletColor(), 0.24f),
            Life = 1.55f,
            MaxLife = 1.55f,
            Warmup = 0.76f / tempo,
            Width = width + sector * 4.0f,
            Damage = (20.0f + sector * 5.0f) * DifficultyHazardDamageScale(),
            Polarity = -1,
        });
    }

    private void UpdateOrbiters(float dt)
    {
        if (_orbiters <= 0)
        {
            ClearOrbiterVisuals();
            return;
        }

        EnsureOrbiterVisuals();
        UpdateOrbiterAi(dt);

        _orbiterFireTimer -= dt;
        if (_orbiterFireTimer > 0.0f)
        {
            return;
        }

        float interval = OrbiterFireInterval();
        _orbiterFireTimer = Mathf.Max(_orbiterFireTimer + interval, interval * 0.45f);

        int shotsToFire = _runPilot == PilotKind.Kairo ? _orbiters : Math.Min(_orbiters, 1 + _orbiters / 3);
        int fired = 0;
        int count = Math.Min(_orbiters, MaxOrbiters);
        for (int i = 0; i < count && fired < shotsToFire; i++)
        {
            OrbiterVisual visual = _orbiterVisuals[i];
            Vector2 origin = visual.Active ? visual.Pos : _playerPos;
            if (_runPilot == PilotKind.Kairo && IsUpgradeMaxed(UpgradeId.KairoDroneBay))
            {
                ClearBulletsNear(origin, 34.0f, false);
            }
            Enemy? target = FindNearestEnemy(origin, _runPilot == PilotKind.Kairo ? 820.0f : 680.0f);
            if (target == null)
            {
                continue;
            }

            FireOrbiterShot(origin, target);
            visual.Facing = SafeDirection(target.Pos - origin, visual.Facing);
            visual.CommandPulse = 1.0f;
            fired++;
        }
    }

    private void EnsureOrbiterVisuals()
    {
        int count = Math.Min(_orbiters, MaxOrbiters);
        Vector2 aim = SafeDirection(_aimDir, Vector2.Up);
        Vector2 right = new(-aim.Y, aim.X);
        for (int i = 0; i < MaxOrbiters; i++)
        {
            OrbiterVisual visual = _orbiterVisuals[i];
            if (i >= count)
            {
                visual.Active = false;
                visual.CommandPulse = 0.0f;
                continue;
            }

            if (visual.Active)
            {
                continue;
            }

            visual.Active = true;
            visual.Phase = _rng.RandfRange(0.0f, Mathf.Tau);
            visual.Pos = ClampToArena(_playerPos - aim * _rng.RandfRange(48.0f, 92.0f) + right * _rng.RandfRange(-42.0f, 42.0f), 28.0f);
            visual.Vel = _playerVel * 0.24f;
            visual.Facing = aim;
            visual.CommandPulse = 0.0f;
        }
    }

    private void UpdateOrbiterAi(float dt)
    {
        int count = Math.Min(_orbiters, MaxOrbiters);
        Vector2 aim = SafeDirection(_aimDir, Vector2.Up);
        Vector2 right = new(-aim.Y, aim.X);
        bool kairo = _runPilot == PilotKind.Kairo;
        float stiffness = kairo ? 5.8f : 5.0f;
        float maxSpeed = kairo ? 620.0f : 540.0f;

        for (int i = 0; i < count; i++)
        {
            OrbiterVisual visual = _orbiterVisuals[i];
            if (!visual.Active)
            {
                continue;
            }

            Vector2 desired = OrbiterFormationTarget(i, count, aim, right, visual.Phase);
            Enemy? target = FindNearestEnemy(visual.Pos, kairo ? 760.0f : 620.0f);
            if (target != null)
            {
                Vector2 toTarget = target.Pos - visual.Pos;
                Vector2 targetDir = SafeDirection(toTarget, aim);
                Vector2 targetSide = new(-targetDir.Y, targetDir.X);
                float flank = Mathf.Sin(_time * 1.35f + visual.Phase) * (kairo ? 46.0f : 32.0f);
                Vector2 attackPoint = target.Pos - targetDir * (kairo ? 126.0f : 156.0f) + targetSide * flank;
                float attackPull = kairo ? 0.34f : 0.22f;
                desired = desired.Lerp(ClampToArena(attackPoint, 34.0f), attackPull);
                visual.Facing = SafeDirection(visual.Facing.Lerp(targetDir, 1.0f - Mathf.Exp(-dt * 7.5f)), targetDir);
            }
            else
            {
                visual.Facing = SafeDirection(visual.Facing.Lerp(aim, 1.0f - Mathf.Exp(-dt * 5.0f)), aim);
            }

            desired += OrbiterSeparation(i, count) + OrbiterBulletAvoidance(visual.Pos);
            Vector2 desiredVelocity = (desired - visual.Pos) * stiffness + _playerVel * (kairo ? 0.28f : 0.34f);
            desiredVelocity = LimitVector(desiredVelocity, maxSpeed);
            visual.Vel = visual.Vel.Lerp(desiredVelocity, 1.0f - Mathf.Exp(-dt * 8.0f));
            visual.Pos = ClampToArena(visual.Pos + visual.Vel * dt, 30.0f);
            visual.CommandPulse = Mathf.Max(0.0f, visual.CommandPulse - dt * 2.8f);
        }
    }

    private Vector2 OrbiterFormationTarget(int index, int count, Vector2 aim, Vector2 right, float phase)
    {
        int columns = Math.Min(3, Math.Max(1, count));
        int row = index / columns;
        int column = index % columns;
        float lateral = (column - (columns - 1) * 0.5f) * (_runPilot == PilotKind.Kairo ? 54.0f : 48.0f);
        if (row % 2 == 1)
        {
            lateral += (_runPilot == PilotKind.Kairo ? 18.0f : 14.0f);
        }

        float rear = (_runPilot == PilotKind.Kairo ? 54.0f : 66.0f) + row * 42.0f + Math.Abs(column - (columns - 1) * 0.5f) * 8.0f;
        Vector2 idle = right * Mathf.Sin(_time * 1.22f + phase) * 10.0f + aim * Mathf.Cos(_time * 1.05f + phase * 0.7f) * 8.0f;
        return ClampToArena(_playerPos - aim * rear + right * lateral + _playerVel * 0.08f + idle, 30.0f);
    }

    private Vector2 OrbiterSeparation(int index, int count)
    {
        Vector2 repel = Vector2.Zero;
        Vector2 pos = _orbiterVisuals[index].Pos;
        for (int i = 0; i < count; i++)
        {
            if (i == index || !_orbiterVisuals[i].Active)
            {
                continue;
            }

            Vector2 away = pos - _orbiterVisuals[i].Pos;
            float distanceSquared = away.LengthSquared();
            if (distanceSquared <= 0.01f || distanceSquared > 46.0f * 46.0f)
            {
                continue;
            }

            float distance = Mathf.Sqrt(distanceSquared);
            repel += away / distance * (46.0f - distance) * 0.95f;
        }

        Vector2 fromPlayer = pos - _playerPos;
        float playerDistanceSquared = fromPlayer.LengthSquared();
        if (playerDistanceSquared > 0.01f && playerDistanceSquared < 52.0f * 52.0f)
        {
            float distance = Mathf.Sqrt(playerDistanceSquared);
            repel += fromPlayer / distance * (52.0f - distance) * 0.7f;
        }

        return repel;
    }

    private Vector2 OrbiterBulletAvoidance(Vector2 pos)
    {
        if (_visualPressure > 0.88f)
        {
            return Vector2.Zero;
        }

        Vector2 repel = Vector2.Zero;
        int influences = 0;
        for (int i = 0; i < _shots.Count && influences < 4; i++)
        {
            Shot shot = _shots[i];
            if (shot.FromPlayer)
            {
                continue;
            }

            Vector2 away = pos - shot.Pos;
            float distanceSquared = away.LengthSquared();
            if (distanceSquared <= 0.01f || distanceSquared > 74.0f * 74.0f)
            {
                continue;
            }

            float distance = Mathf.Sqrt(distanceSquared);
            repel += away / distance * (74.0f - distance) * 0.48f;
            influences++;
        }

        return repel;
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
        if (_runPilot == PilotKind.Kairo && IsUpgradeMaxed(UpgradeId.KairoRelayProtocol))
        {
            shot.Rift = true;
        }
        else
        {
            shot.Rift = false;
        }
        AddParticle(origin, dir * 180.0f, PolarityColor(_playerPolarity), 7.0f, 0.16f);
        AddParticle(origin - dir * 6.0f, -dir * 90.0f, PickupBlue, 5.0f, 0.14f);
    }

    private Enemy? FindNearestEnemy(Vector2 from, float radius)
    {
        Enemy? best = null;
        float bestDistance = radius * radius;
        for (int i = 0; i < _enemies.Count; i++)
        {
            Enemy enemy = _enemies[i];
            if (enemy.Hp <= 0.0f)
            {
                continue;
            }

            float deltaX = enemy.Pos.X - from.X;
            float deltaY = enemy.Pos.Y - from.Y;
            float distance = deltaX * deltaX + deltaY * deltaY;
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = enemy;
            }
        }
        return best;
    }

    private void BuildEnemyGrid()
    {
        for (int i = 0; i < _enemyGrid.Length; i++)
        {
            _enemyGrid[i].Clear();
        }

        for (int i = 0; i < _enemies.Count; i++)
        {
            Enemy enemy = _enemies[i];
            if (enemy.Hp <= 0.0f)
            {
                continue;
            }

            _enemyGrid[EnemyGridIndex(enemy.Pos)].Add(enemy);
        }
    }

    private static int EnemyGridIndex(Vector2 pos)
    {
        return EnemyGridCoordY(pos.Y) * EnemyGridColumns + EnemyGridCoordX(pos.X);
    }

    private static int EnemyGridCoordX(float x)
    {
        return Mathf.Clamp((int)(x / EnemyGridCellSize), 0, EnemyGridColumns - 1);
    }

    private static int EnemyGridCoordY(float y)
    {
        return Mathf.Clamp((int)(y / EnemyGridCellSize), 0, EnemyGridRows - 1);
    }

    private void ClearBulletsNear(Vector2 point, float radius, bool score)
    {
        float r2 = radius * radius;
        int cleared = 0;
        int burstBudget = _visualPressure > 0.86f ? 8 : _visualPressure > 0.68f ? 16 : 34;
        for (int i = _shots.Count - 1; i >= 0; i--)
        {
            Shot shot = _shots[i];
            float deltaX = shot.Pos.X - point.X;
            float deltaY = shot.Pos.Y - point.Y;
            if (!shot.FromPlayer && deltaX * deltaX + deltaY * deltaY < r2)
            {
                if (score)
                {
                    AddRunScore(5, shot.Pos, EnemyBulletColor());
                    _energy = Mathf.Clamp(_energy + 1.2f, 0.0f, _maxEnergy);
                }
                if (cleared < burstBudget || _rng.Randf() < 0.08f)
                {
                    Burst(shot.Pos, EnemyBulletColor(), _visualPressure > 0.82f ? 1 : 3, 160.0f, 0.3f);
                }
                cleared++;
                RemoveShotAt(i);
            }
        }
    }

    private void ClearEnemyBulletsInBeam(Vector2 start, Vector2 end, float width, bool score)
    {
        int cleared = 0;
        int burstBudget = _visualPressure > 0.86f ? 8 : _visualPressure > 0.68f ? 16 : 34;
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
                    AddRunScore(5, shot.Pos, EnemyBulletColor());
                    _energy = Mathf.Clamp(_energy + 1.2f, 0.0f, _maxEnergy);
                }
                if (cleared < burstBudget || _rng.Randf() < 0.08f)
                {
                    Burst(shot.Pos, EnemyBulletColor(), _visualPressure > 0.82f ? 1 : 3, 160.0f, 0.3f);
                }
                cleared++;
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

        int pilotSlots = Math.Min(_wave <= 24 ? 2 : 1, pilotPool.Count);
        for (int i = 0; i < pilotSlots; i++)
        {
            AddRandomUpgradeChoice(pilotPool);
        }

        TryAddFlowUpgradeChoice(commonPool);

        while (_upgradeChoices.Count < 3 && commonPool.Count > 0)
        {
            AddRandomUpgradeChoice(commonPool);
        }

        while (_upgradeChoices.Count < 3 && pilotPool.Count > 0)
        {
            AddRandomUpgradeChoice(pilotPool);
        }

        const float cardWidth = 386.0f;
        const float cardHeight = 386.0f;
        const float cardGap = 42.0f;
        float startX = (ScreenWidth - cardWidth * 3.0f - cardGap * 2.0f) * 0.5f;
        for (int i = 0; i < _upgradeChoices.Count; i++)
        {
            UpgradeCard card = _upgradeChoices[i];
            card.Rect = new Rect2(new Vector2(startX + i * (cardWidth + cardGap), 342.0f), new Vector2(cardWidth, cardHeight));
            _upgradeChoices[i] = card;
        }

        _gamepadUpgradeIndex = Mathf.Clamp(_gamepadUpgradeIndex, 0, Math.Max(0, _upgradeChoices.Count - 1));
    }

    private void TryAddFlowUpgradeChoice(List<UpgradeId> commonPool)
    {
        if (_upgradeChoices.Count >= 3 || commonPool.Count <= 0)
        {
            return;
        }

        List<UpgradeId> candidates = FlowUpgradeCandidates(commonPool);
        if (candidates.Count <= 0)
        {
            return;
        }

        bool earlyBuild = _runLevel <= 5;
        bool hasFlowCore = _chainRelay > 0 || _fractalSplit > 0;
        bool waveWantsFlow = _currentWavePace == WavePaceKind.Swarm
            || _currentWavePace == WavePaceKind.Pressure
            || _currentWavePace == WavePaceKind.Elite
            || _currentWavePace == WavePaceKind.Boss;
        if (!earlyBuild && !hasFlowCore && !waveWantsFlow && _rng.Randf() > 0.36f)
        {
            return;
        }

        UpgradeId choice = candidates[_rng.RandiRange(0, candidates.Count - 1)];
        _upgradeChoices.Add(CreateCard(choice));
        commonPool.Remove(choice);
    }

    private List<UpgradeId> FlowUpgradeCandidates(List<UpgradeId> commonPool)
    {
        List<UpgradeId> candidates = new();
        AddFlowCandidate(candidates, commonPool, _runPilot == PilotKind.Kairo || _runPilot == PilotKind.Sol ? UpgradeId.ChainRelay : UpgradeId.FractalSplit);

        switch (_currentWavePace)
        {
            case WavePaceKind.Swarm:
            case WavePaceKind.Pressure:
                AddFlowCandidate(candidates, commonPool, UpgradeId.ChainRelay);
                break;
            case WavePaceKind.Elite:
            case WavePaceKind.Boss:
                AddFlowCandidate(candidates, commonPool, UpgradeId.FractalSplit);
                break;
        }

        if (_chainRelay > 0)
        {
            AddFlowCandidate(candidates, commonPool, UpgradeId.FractalSplit);
            AddFlowCandidate(candidates, commonPool, UpgradeId.QuantumEcho);
            AddFlowCandidate(candidates, commonPool, UpgradeId.RiftNeedle);
            AddFlowCandidate(candidates, commonPool, UpgradeId.RicochetMatrix);
        }

        if (_fractalSplit > 0)
        {
            AddFlowCandidate(candidates, commonPool, UpgradeId.ChainRelay);
            AddFlowCandidate(candidates, commonPool, UpgradeId.RiftNeedle);
            AddFlowCandidate(candidates, commonPool, UpgradeId.MoonWisp);
            AddFlowCandidate(candidates, commonPool, UpgradeId.PulseMagazine);
        }
        if (_executionMark > 0)
        {
            AddFlowCandidate(candidates, commonPool, UpgradeId.RicochetMatrix);
            AddFlowCandidate(candidates, commonPool, UpgradeId.StasisField);
        }
        if (_magnetizedCore > 0)
        {
            AddFlowCandidate(candidates, commonPool, UpgradeId.GravityWell);
            AddFlowCandidate(candidates, commonPool, UpgradeId.PulseMagazine);
        }

        AddFlowCandidate(candidates, commonPool, UpgradeId.ChainRelay);
        AddFlowCandidate(candidates, commonPool, UpgradeId.FractalSplit);
        AddFlowCandidate(candidates, commonPool, UpgradeId.PulseMagazine);
        AddFlowCandidate(candidates, commonPool, UpgradeId.ExecutionMark);
        AddFlowCandidate(candidates, commonPool, UpgradeId.StasisField);
        AddFlowCandidate(candidates, commonPool, UpgradeId.MagnetizedCore);
        AddFlowCandidate(candidates, commonPool, UpgradeId.RicochetMatrix);
        return candidates;
    }

    private static void AddFlowCandidate(List<UpgradeId> candidates, List<UpgradeId> commonPool, UpgradeId id)
    {
        if (commonPool.Contains(id) && !candidates.Contains(id))
        {
            candidates.Add(id);
        }
    }

    private void AddRandomUpgradeChoice(List<UpgradeId> pool)
    {
        if (pool.Count <= 0)
        {
            return;
        }

        int index = WeightedUpgradeIndex(pool);
        _upgradeChoices.Add(CreateCard(pool[index]));
        pool.RemoveAt(index);
    }

    private int WeightedUpgradeIndex(List<UpgradeId> pool)
    {
        float total = 0.0f;
        float[] weights = new float[pool.Count];
        for (int i = 0; i < pool.Count; i++)
        {
            weights[i] = UpgradeDraftWeight(pool[i]);
            total += weights[i];
        }

        float roll = _rng.RandfRange(0.0f, Math.Max(0.001f, total));
        float accum = 0.0f;
        for (int i = 0; i < pool.Count; i++)
        {
            accum += weights[i];
            if (roll <= accum)
            {
                return i;
            }
        }
        return _rng.RandiRange(0, pool.Count - 1);
    }

    private float UpgradeDraftWeight(UpgradeId id)
    {
        int bias = UpgradeBiasIndex(id) switch
        {
            0 => _draftBiasWeapon,
            1 => _draftBiasDefense,
            2 => _draftBiasSkill,
            3 => _draftBiasFlow,
            4 => _draftBiasEconomy,
            _ => 0,
        };

        float weight = 1.0f + Math.Min(8, bias) * 0.18f;
        int rank = GetRank(id);
        int maxRank = MaxRank(id);
        if (maxRank < 20 && rank > 0)
        {
            weight += 0.28f + rank * 0.08f;
            if (rank + 1 >= maxRank)
            {
                weight += 0.42f;
            }
        }

        if (IsPilotUpgrade(_runPilot, id))
        {
            weight += _wave <= 20 ? 0.72f : 0.38f;
        }
        if (IsSynergyUpgrade(id))
        {
            weight += 0.42f;
        }
        return Mathf.Max(0.1f, weight);
    }

    private bool IsSynergyUpgrade(UpgradeId id)
    {
        return id switch
        {
            UpgradeId.RicochetMatrix => _chainRelay > 0 || _executionMark > 0 || _orionDeadeyeMark > 0,
            UpgradeId.PulseMagazine => _multiShot > 1 || _fractalSplit > 0 || _lyraResonanceChord > 0,
            UpgradeId.StasisField => _executionMark > 0 || _nyxEventHorizon > 0 || _rookBulwarkCore > 0,
            UpgradeId.MagnetizedCore => _pickupMagnet > 280.0f || _magnetizedCore > 0,
            UpgradeId.ExecutionMark => _critMultiplier > 1.7f || _orionCometSpear > 0 || _vesperCharge > 0,
            UpgradeId.ChainRelay => _ricochetMatrix > 0 || _kairoRelayProtocol > 0 || _solFlareCore > 0,
            UpgradeId.FractalSplit => _pulseMagazine > 0 || _astraTwinRefraction > 0 || _lyraHarmonicCascade > 0,
            UpgradeId.NovaCapacitor => _novaCost > UltimateCostFloor + 0.5f,
            UpgradeId.GravityWell => _magnetizedCore > 0,
            _ => false,
        };
    }

    private string UpgradeMomentumText()
    {
        int bias = DominantDraftBiasIndex();
        int value = DraftBiasValue(bias);
        if (value < 3)
        {
            return T("choice.momentum.open");
        }

        return Tf("choice.momentum.focus", UpgradePathLabel(bias));
    }

    private string UpgradeBadgeText(UpgradeId id)
    {
        int rank = GetRank(id);
        int maxRank = MaxRank(id);
        if (maxRank < 20 && rank + 1 >= maxRank && CapstoneBody(id).Length > 0)
        {
            return T("choice.badge.capstone");
        }

        if (IsPilotUpgrade(_runPilot, id))
        {
            return T("choice.badge.pilot");
        }

        if (IsSynergyUpgrade(id))
        {
            return T("choice.badge.synergy");
        }

        int bias = UpgradeBiasIndex(id);
        if (DraftBiasValue(bias) >= 3 && DominantDraftBiasIndex() == bias)
        {
            return T("choice.badge.momentum");
        }

        return UpgradePathLabel(bias);
    }

    private int DominantDraftBiasIndex()
    {
        int bestIndex = 0;
        int bestValue = DraftBiasValue(0);
        for (int i = 1; i <= 4; i++)
        {
            int value = DraftBiasValue(i);
            if (value > bestValue)
            {
                bestIndex = i;
                bestValue = value;
            }
        }

        return bestIndex;
    }

    private int DraftBiasValue(int bias)
    {
        return bias switch
        {
            1 => _draftBiasDefense,
            2 => _draftBiasSkill,
            3 => _draftBiasFlow,
            4 => _draftBiasEconomy,
            _ => _draftBiasWeapon,
        };
    }

    private string UpgradePathLabel(int bias)
    {
        return bias switch
        {
            1 => T("choice.path.defense"),
            2 => T("choice.path.skill"),
            3 => T("choice.path.flow"),
            4 => T("choice.path.economy"),
            _ => T("choice.path.weapon"),
        };
    }

    private static Color BuildPathAccent(int bias)
    {
        return bias switch
        {
            1 => Jade,
            2 => Violet,
            3 => Cyan,
            4 => Gold,
            _ => Rose,
        };
    }

    private void UpdateDraftBias(UpgradeId id)
    {
        int chosen = UpgradeBiasIndex(id);
        _draftBiasWeapon = Math.Max(0, _draftBiasWeapon - 1);
        _draftBiasDefense = Math.Max(0, _draftBiasDefense - 1);
        _draftBiasSkill = Math.Max(0, _draftBiasSkill - 1);
        _draftBiasFlow = Math.Max(0, _draftBiasFlow - 1);
        _draftBiasEconomy = Math.Max(0, _draftBiasEconomy - 1);
        switch (chosen)
        {
            case 0:
                _draftBiasWeapon += 3;
                break;
            case 1:
                _draftBiasDefense += 3;
                break;
            case 2:
                _draftBiasSkill += 3;
                break;
            case 3:
                _draftBiasFlow += 3;
                break;
            case 4:
                _draftBiasEconomy += 3;
                break;
        }
    }

    private void TryGrantBuildMilestones(UpgradeId id)
    {
        int path = UpgradeBiasIndex(id);
        int value = DraftBiasValue(path);
        for (int tier = 0; tier < BuildMilestoneThresholds.Length; tier++)
        {
            if (value >= BuildMilestoneThresholds[tier] && !HasBuildMilestone(path, tier))
            {
                GrantBuildMilestone(path, tier);
            }
        }
    }

    private bool HasBuildMilestone(int path, int tier)
    {
        int bit = BuildMilestoneBit(path, tier);
        return (_buildMilestoneMask & bit) != 0;
    }

    private void MarkBuildMilestone(int path, int tier)
    {
        _buildMilestoneMask |= BuildMilestoneBit(path, tier);
    }

    private static int BuildMilestoneBit(int path, int tier)
    {
        return 1 << Mathf.Clamp(path * BuildMilestoneThresholds.Length + tier, 0, 30);
    }

    private static string BuildMilestoneTierLabel(int tier)
    {
        return tier switch
        {
            0 => "I",
            1 => "II",
            _ => "III",
        };
    }

    private string BuildMilestoneBody(int path)
    {
        return path switch
        {
            1 => T("build.breakthrough.defense"),
            2 => T("build.breakthrough.skill"),
            3 => T("build.breakthrough.flow"),
            4 => T("build.breakthrough.economy"),
            _ => T("build.breakthrough.weapon"),
        };
    }

    private void GrantBuildMilestone(int path, int tier)
    {
        MarkBuildMilestone(path, tier);
        Color accent = BuildPathAccent(path);
        float tierScale = tier + 1;

        switch (path)
        {
            case 1:
                _playerMaxHp += 12.0f + tierScale * 8.0f;
                _playerHp = Mathf.Clamp(_playerHp + 22.0f + tierScale * 12.0f, 0.0f, _playerMaxHp);
                _mirrorReduction *= 0.965f - tier * 0.015f;
                _invulnTimer = Mathf.Max(_invulnTimer, 0.35f + tier * 0.16f);
                break;
            case 2:
                _maxEnergy += 8.0f + tierScale * 6.0f;
                _energy = Mathf.Clamp(_energy + 24.0f + tierScale * 8.0f, 0.0f, _maxEnergy);
                _novaCost = Mathf.Max(UltimateCostFloor, _novaCost - 2.0f - tierScale * 1.5f);
                _polarityCooldownMax = Mathf.Max(PolaritySwitchCooldownMin, _polarityCooldownMax - 0.14f - tier * 0.08f);
                _polarityCooldown = Mathf.Min(_polarityCooldown, _polarityCooldownMax);
                if (tier >= 2)
                {
                    ClearBulletsNear(_playerPos, 260.0f, true);
                }
                break;
            case 3:
                _chainRelay = Math.Min(6, _chainRelay + 1);
                if (tier >= 1)
                {
                    _fractalSplit = Math.Min(6, _fractalSplit + 1);
                }
                _echoChance = Mathf.Min(0.68f, _echoChance + 0.035f + tier * 0.025f);
                _ricochetMatrix = Math.Max(_ricochetMatrix, tier >= 2 ? 1 : _ricochetMatrix);
                break;
            case 4:
                _pickupMagnet += 42.0f + tierScale * 32.0f;
                _nextWaveRewardBoost = Mathf.Max(_nextWaveRewardBoost, 1.16f + tier * 0.12f);
                if (tier >= 1)
                {
                    _baseRerolls = Math.Min(5, _baseRerolls + 1);
                }
                if (tier >= 2)
                {
                    SpawnPickup(_playerPos + new Vector2(-48.0f, -22.0f), PickupKind.Energy);
                    SpawnPickup(_playerPos + new Vector2(48.0f, -22.0f), PickupKind.Repair);
                }
                break;
            default:
                _damageMultiplier += 0.07f + tier * 0.045f;
                _fireInterval = Mathf.Max(0.09f, _fireInterval - 0.012f - tier * 0.004f);
                if (tier >= 1)
                {
                    _critMultiplier += 0.08f + tier * 0.04f;
                }
                if (tier >= 2)
                {
                    _nextWaveDamageBoost = Mathf.Max(_nextWaveDamageBoost, 1.24f);
                }
                break;
        }

        ClampUltimateCost();
        string title = Tf("build.breakthrough", UpgradePathLabel(path), BuildMilestoneTierLabel(tier));
        AddText(title.ToUpperInvariant(), ScreenCenter + new Vector2(0.0f, -92.0f), accent, 25.0f);
        AddText(BuildMilestoneBody(path), ScreenCenter + new Vector2(0.0f, -58.0f), Alpha(Paper, 0.76f), 18.0f);
        Burst(ScreenCenter + new Vector2(0.0f, -54.0f), accent, 28 + tier * 10, 360.0f + tier * 70.0f, 0.78f);
        PlaySfx(460.0f + tier * 70.0f, 170.0f, 0.18f, 0.24f, 0.02f, 1);
    }

    private static int UpgradeBiasIndex(UpgradeId id)
    {
        return id switch
        {
            UpgradeId.VitalShell or UpgradeId.MirrorSkin or UpgradeId.AegisBloom or UpgradeId.StasisField or UpgradeId.RookBulwarkCore or UpgradeId.RookAegisRelay or UpgradeId.RookCitadelProtocol or UpgradeId.SolRadiantMantle => 1,
            UpgradeId.NovaCapacitor or UpgradeId.PolarityStorm or UpgradeId.CometTrail or UpgradeId.OneWaveOverdrive or UpgradeId.BulletTransmute or UpgradeId.AstraNovaBloom or UpgradeId.VesperJudgmentCoil or UpgradeId.KairoOverrideMatrix or UpgradeId.SolFlareCore or UpgradeId.NyxSingularity or UpgradeId.NyxEventHorizon or UpgradeId.LyraEncoreField or UpgradeId.OrionPerihelionVector => 2,
            UpgradeId.MoonWisp or UpgradeId.QuantumEcho or UpgradeId.ChainRelay or UpgradeId.FractalSplit or UpgradeId.RicochetMatrix or UpgradeId.KairoDroneBay or UpgradeId.KairoRelayProtocol or UpgradeId.LyraHarmonicCascade or UpgradeId.NyxGravityCantor => 3,
            UpgradeId.GravityWell or UpgradeId.ResonanceLeech or UpgradeId.BountyContract or UpgradeId.HarmonicMap or UpgradeId.MagnetizedCore or UpgradeId.SolSolarForge => 4,
            _ => 0,
        };
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
            UpgradeId.ChainRelay,
            UpgradeId.FractalSplit,
            UpgradeId.SolarThesis,
            UpgradeId.EmergencyRepair,
            UpgradeId.OneWaveOverdrive,
            UpgradeId.GlassCannon,
            UpgradeId.BountyContract,
            UpgradeId.BulletTransmute,
            UpgradeId.HarmonicMap,
            UpgradeId.PulseMagazine,
            UpgradeId.ExecutionMark,
            UpgradeId.StasisField,
            UpgradeId.MagnetizedCore,
            UpgradeId.RicochetMatrix,
        };
    }

    private static List<UpgradeId> PilotUpgradePool(PilotKind pilot)
    {
        return pilot switch
        {
            PilotKind.Vesper => new List<UpgradeId> { UpgradeId.VesperCapacitor, UpgradeId.VesperSplitRail, UpgradeId.VesperJudgmentCoil, UpgradeId.VesperSeverLine },
            PilotKind.Kairo => new List<UpgradeId> { UpgradeId.KairoDroneBay, UpgradeId.KairoSwarmSync, UpgradeId.KairoOverrideMatrix, UpgradeId.KairoRelayProtocol },
            PilotKind.Sol => new List<UpgradeId> { UpgradeId.SolCoronaBloom, UpgradeId.SolSolarForge, UpgradeId.SolFlareCore, UpgradeId.SolRadiantMantle },
            PilotKind.Nyx => new List<UpgradeId> { UpgradeId.NyxOrbit, UpgradeId.NyxSingularity, UpgradeId.NyxEventHorizon, UpgradeId.NyxGravityCantor },
            PilotKind.Rook => new List<UpgradeId> { UpgradeId.RookBulwarkCore, UpgradeId.RookSiegeBattery, UpgradeId.RookAegisRelay, UpgradeId.RookCitadelProtocol },
            PilotKind.Lyra => new List<UpgradeId> { UpgradeId.LyraResonanceChord, UpgradeId.LyraTempoBloom, UpgradeId.LyraHarmonicCascade, UpgradeId.LyraEncoreField },
            PilotKind.Orion => new List<UpgradeId> { UpgradeId.OrionCometSpear, UpgradeId.OrionDeadeyeMark, UpgradeId.OrionStarfallQuiver, UpgradeId.OrionPerihelionVector },
            _ => new List<UpgradeId> { UpgradeId.AstraRefraction, UpgradeId.AstraPrismWake, UpgradeId.AstraNovaBloom, UpgradeId.AstraTwinRefraction },
        };
    }

    private static bool IsPilotUpgrade(PilotKind pilot, UpgradeId id)
    {
        return pilot switch
        {
            PilotKind.Vesper => id is UpgradeId.VesperCapacitor or UpgradeId.VesperSplitRail or UpgradeId.VesperJudgmentCoil or UpgradeId.VesperSeverLine,
            PilotKind.Kairo => id is UpgradeId.KairoDroneBay or UpgradeId.KairoSwarmSync or UpgradeId.KairoOverrideMatrix or UpgradeId.KairoRelayProtocol,
            PilotKind.Sol => id is UpgradeId.SolCoronaBloom or UpgradeId.SolSolarForge or UpgradeId.SolFlareCore or UpgradeId.SolRadiantMantle,
            PilotKind.Nyx => id is UpgradeId.NyxOrbit or UpgradeId.NyxSingularity or UpgradeId.NyxEventHorizon or UpgradeId.NyxGravityCantor,
            PilotKind.Rook => id is UpgradeId.RookBulwarkCore or UpgradeId.RookSiegeBattery or UpgradeId.RookAegisRelay or UpgradeId.RookCitadelProtocol,
            PilotKind.Lyra => id is UpgradeId.LyraResonanceChord or UpgradeId.LyraTempoBloom or UpgradeId.LyraHarmonicCascade or UpgradeId.LyraEncoreField,
            PilotKind.Orion => id is UpgradeId.OrionCometSpear or UpgradeId.OrionDeadeyeMark or UpgradeId.OrionStarfallQuiver or UpgradeId.OrionPerihelionVector,
            _ => id is UpgradeId.AstraRefraction or UpgradeId.AstraPrismWake or UpgradeId.AstraNovaBloom or UpgradeId.AstraTwinRefraction,
        };
    }

    private void UpdateUpgrade(float dt)
    {
        UpdateParticles(dt);
        UpdateDamageTexts(dt);
        if (_upgradeChoices.Count > 0)
        {
            _gamepadUpgradeIndex = Mathf.Clamp(_gamepadUpgradeIndex, 0, _upgradeChoices.Count - 1);
            int nav = ConsumeGamepadNavX();
            if (nav != 0)
            {
                _gamepadUpgradeIndex = (_gamepadUpgradeIndex + nav + _upgradeChoices.Count) % _upgradeChoices.Count;
                PlaySfx(250.0f + _gamepadUpgradeIndex * 30.0f, 18.0f, 0.055f, 0.1f, 0.01f, 1);
            }
            SetGamepadFocus(_upgradeChoices[_gamepadUpgradeIndex].Rect);
        }

        bool one = KeyDown(Key.Key1);
        bool two = KeyDown(Key.Key2);
        bool three = KeyDown(Key.Key3);
        bool four = KeyDown(Key.Key4);
        bool reroll = RerollHeld();
        bool click = Input.IsMouseButtonPressed(MouseButton.Left);
        Vector2 mouse = GetGlobalMousePosition();
        if (click && !_lastClick)
        {
            _usingGamepad = false;
        }

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
        else if (ConfirmHeld() && !_lastConfirm)
        {
            ChooseUpgrade(_gamepadUpgradeIndex);
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
                    _gamepadUpgradeIndex = i;
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
        UpdateDraftBias(card.Id);
        ApplyUpgrade(card.Id);
        TryGrantBuildMilestones(card.Id);
        AddText(card.Title.ToUpperInvariant(), ScreenCenter + new Vector2(0.0f, -170.0f), card.Accent, 32.0f);
        if (GetRank(card.Id) >= MaxRank(card.Id) && MaxRank(card.Id) < 20 && CapstoneBody(card.Id).Length > 0)
        {
            AddText(T("choice.capstone").ToUpperInvariant(), ScreenCenter + new Vector2(0.0f, -122.0f), card.Accent.Lerp(Paper, 0.18f), 25.0f);
        }
        Burst(ScreenCenter, card.Accent, 64, 620.0f, 1.2f);
        PlaySfx(420.0f, 220.0f, 0.34f, 0.34f, 0.03f, 1);
        if (_queuedLevelUps > 0)
        {
            OpenLevelUpChoice(ScreenCenter);
        }
        else
        {
            _mode = GameMode.Playing;
        }
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
            case UpgradeId.ChainRelay:
                _chainRelay = Math.Min(6, _chainRelay + 1);
                _damageMultiplier += 0.035f;
                break;
            case UpgradeId.FractalSplit:
                _fractalSplit = Math.Min(6, _fractalSplit + 1);
                _damageMultiplier += 0.035f;
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
                AddRunScore(500, ScreenCenter, UpgradeAccent(id), true);
                break;
            case UpgradeId.PulseMagazine:
                _pulseMagazine = Math.Min(5, _pulseMagazine + 1);
                _fireInterval = Mathf.Max(0.1f, _fireInterval - 0.012f);
                break;
            case UpgradeId.ExecutionMark:
                _executionMark = Math.Min(5, _executionMark + 1);
                _critMultiplier += 0.08f;
                _damageMultiplier += 0.035f;
                break;
            case UpgradeId.StasisField:
                _stasisField = Math.Min(5, _stasisField + 1);
                _enemySlow *= 0.965f;
                _pickupMagnet += 18.0f;
                break;
            case UpgradeId.MagnetizedCore:
                _magnetizedCore = Math.Min(4, _magnetizedCore + 1);
                _pickupMagnet += 72.0f;
                _maxEnergy += 6.0f;
                _energy = Mathf.Clamp(_energy + 10.0f, 0.0f, _maxEnergy);
                break;
            case UpgradeId.RicochetMatrix:
                _ricochetMatrix = Math.Min(5, _ricochetMatrix + 1);
                _chainRelay = Math.Max(_chainRelay, 1);
                _echoChance = Mathf.Min(0.62f, _echoChance + 0.04f);
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
                _fractalSplit = Math.Max(_fractalSplit, 1);
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
                _fractalSplit = Math.Max(_fractalSplit, 1);
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
                _chainRelay = Math.Max(_chainRelay, 1);
                _damageMultiplier += 0.04f;
                _orbiterFireTimer = Mathf.Min(_orbiterFireTimer, 0.06f);
                break;
            case UpgradeId.SolFlareCore:
                _solFlareCore = Math.Min(4, _solFlareCore + 1);
                _chainRelay = Math.Max(_chainRelay, 1);
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
            case UpgradeId.NyxOrbit:
                _nyxOrbit = Math.Min(5, _nyxOrbit + 1);
                _fireInterval = Mathf.Max(0.17f, _fireInterval - 0.025f);
                _fractalSplit = Math.Max(_fractalSplit, 1);
                break;
            case UpgradeId.NyxSingularity:
                _nyxSingularity = Math.Min(5, _nyxSingularity + 1);
                _damageMultiplier += 0.06f;
                _pickupMagnet += 24.0f;
                break;
            case UpgradeId.NyxEventHorizon:
                _nyxEventHorizon = Math.Min(4, _nyxEventHorizon + 1);
                _stasisField = Math.Max(_stasisField, 1);
                _novaCost = Mathf.Max(34.0f, _novaCost - 4.0f);
                break;
            case UpgradeId.NyxGravityCantor:
                _nyxGravityCantor = Math.Min(3, _nyxGravityCantor + 1);
                _chainRelay = Math.Max(_chainRelay, 1);
                _damageMultiplier += 0.04f;
                break;
            case UpgradeId.RookBulwarkCore:
                _rookBulwarkCore = Math.Min(5, _rookBulwarkCore + 1);
                _playerMaxHp += 18.0f;
                _playerHp = Mathf.Clamp(_playerHp + 24.0f, 0.0f, _playerMaxHp);
                _mirrorReduction *= 0.94f;
                break;
            case UpgradeId.RookSiegeBattery:
                _rookSiegeBattery = Math.Min(5, _rookSiegeBattery + 1);
                _damageMultiplier += 0.08f;
                _fireInterval = Mathf.Max(0.46f, _fireInterval - 0.035f);
                break;
            case UpgradeId.RookAegisRelay:
                _rookAegisRelay = Math.Min(4, _rookAegisRelay + 1);
                _aegisRegen += 0.42f;
                _maxEnergy += 8.0f;
                _energy = Mathf.Clamp(_energy + 12.0f, 0.0f, _maxEnergy);
                break;
            case UpgradeId.RookCitadelProtocol:
                _rookCitadelProtocol = Math.Min(3, _rookCitadelProtocol + 1);
                _dashDamage += 22.0f;
                _novaCost = Mathf.Max(36.0f, _novaCost - 5.0f);
                break;
            case UpgradeId.LyraResonanceChord:
                _lyraResonanceChord = Math.Min(5, _lyraResonanceChord + 1);
                _multiShot = Math.Min(5, _multiShot + (_lyraResonanceChord % 2 == 1 ? 1 : 0));
                _damageMultiplier += 0.04f;
                break;
            case UpgradeId.LyraTempoBloom:
                _lyraTempoBloom = Math.Min(5, _lyraTempoBloom + 1);
                _fireInterval = Mathf.Max(0.13f, _fireInterval - 0.028f);
                _echoChance = Mathf.Min(0.62f, _echoChance + 0.025f);
                break;
            case UpgradeId.LyraHarmonicCascade:
                _lyraHarmonicCascade = Math.Min(4, _lyraHarmonicCascade + 1);
                _chainRelay = Math.Max(_chainRelay, 1);
                _fractalSplit = Math.Max(_fractalSplit, 1);
                break;
            case UpgradeId.LyraEncoreField:
                _lyraEncoreField = Math.Min(3, _lyraEncoreField + 1);
                _maxEnergy += 12.0f;
                _energy = Mathf.Clamp(_energy + 18.0f, 0.0f, _maxEnergy);
                _novaCost = Mathf.Max(34.0f, _novaCost - 5.0f);
                break;
            case UpgradeId.OrionCometSpear:
                _orionCometSpear = Math.Min(5, _orionCometSpear + 1);
                _damageMultiplier += 0.09f;
                break;
            case UpgradeId.OrionDeadeyeMark:
                _orionDeadeyeMark = Math.Min(5, _orionDeadeyeMark + 1);
                _executionMark = Math.Max(_executionMark, 1);
                _critMultiplier += 0.1f;
                break;
            case UpgradeId.OrionStarfallQuiver:
                _orionStarfallQuiver = Math.Min(4, _orionStarfallQuiver + 1);
                _fireInterval = Mathf.Max(0.38f, _fireInterval - 0.04f);
                _fractalSplit = Math.Max(_fractalSplit, 1);
                break;
            case UpgradeId.OrionPerihelionVector:
                _orionPerihelionVector = Math.Min(3, _orionPerihelionVector + 1);
                _playerSpeed += 22.0f;
                _dashPower += 70.0f;
                _novaCost = Mathf.Max(34.0f, _novaCost - 5.0f);
                break;
        }

        if (IsUpgradeMaxed(id) && CapstoneBody(id).Length > 0)
        {
            ApplyCapstoneBonus(id);
        }
        ClampUltimateCost();
    }

    private void ApplyCapstoneBonus(UpgradeId id)
    {
        Color color = UpgradeAccent(id);
        switch (id)
        {
            case UpgradeId.ChainRelay:
                _energy = Mathf.Clamp(_energy + 22.0f, 0.0f, _maxEnergy);
                break;
            case UpgradeId.FractalSplit:
                _damageMultiplier += 0.08f;
                break;
            case UpgradeId.PulseMagazine:
                _fireInterval = Mathf.Max(0.09f, _fireInterval - 0.025f);
                break;
            case UpgradeId.ExecutionMark:
                _critMultiplier += 0.2f;
                break;
            case UpgradeId.StasisField:
                ClearBulletsNear(ScreenCenter, 1600.0f, true);
                break;
            case UpgradeId.MagnetizedCore:
                _pickupMagnet += 180.0f;
                break;
            case UpgradeId.RicochetMatrix:
                _chainRelay = Math.Max(_chainRelay, 3);
                break;
            case UpgradeId.AstraRefraction:
            case UpgradeId.AstraPrismWake:
                _fireInterval = Mathf.Max(0.09f, _fireInterval - 0.025f);
                break;
            case UpgradeId.AstraNovaBloom:
            case UpgradeId.AstraTwinRefraction:
                _novaCost = Mathf.Max(32.0f, _novaCost - 8.0f);
                break;
            case UpgradeId.VesperCapacitor:
            case UpgradeId.VesperJudgmentCoil:
                _damageMultiplier += 0.08f;
                break;
            case UpgradeId.VesperSplitRail:
            case UpgradeId.VesperSeverLine:
                _fireInterval = Mathf.Max(0.28f, _fireInterval - 0.035f);
                break;
            case UpgradeId.KairoDroneBay:
                _orbiters = Math.Min(9, _orbiters + 2);
                break;
            case UpgradeId.KairoSwarmSync:
            case UpgradeId.KairoRelayProtocol:
                _orbiterFireTimer = 0.0f;
                _energy = Mathf.Clamp(_energy + 18.0f, 0.0f, _maxEnergy);
                break;
            case UpgradeId.KairoOverrideMatrix:
                _novaCost = Mathf.Max(30.0f, _novaCost - 8.0f);
                break;
            case UpgradeId.SolCoronaBloom:
            case UpgradeId.SolFlareCore:
                _damageMultiplier += 0.06f;
                break;
            case UpgradeId.SolSolarForge:
                _maxEnergy += 20.0f;
                _energy = Mathf.Clamp(_energy + 40.0f, 0.0f, _maxEnergy);
                break;
            case UpgradeId.SolRadiantMantle:
                _playerMaxHp += 20.0f;
                _playerHp = Mathf.Clamp(_playerHp + 36.0f, 0.0f, _playerMaxHp);
                break;
            case UpgradeId.NyxOrbit:
            case UpgradeId.NyxSingularity:
                _fireInterval = Mathf.Max(0.14f, _fireInterval - 0.018f);
                break;
            case UpgradeId.NyxEventHorizon:
            case UpgradeId.NyxGravityCantor:
                _novaCost = Mathf.Max(30.0f, _novaCost - 8.0f);
                break;
            case UpgradeId.RookBulwarkCore:
            case UpgradeId.RookAegisRelay:
                _playerMaxHp += 18.0f;
                _playerHp = Mathf.Clamp(_playerHp + 36.0f, 0.0f, _playerMaxHp);
                break;
            case UpgradeId.RookSiegeBattery:
            case UpgradeId.RookCitadelProtocol:
                _damageMultiplier += 0.1f;
                break;
            case UpgradeId.LyraResonanceChord:
            case UpgradeId.LyraTempoBloom:
                _fireInterval = Mathf.Max(0.11f, _fireInterval - 0.02f);
                break;
            case UpgradeId.LyraHarmonicCascade:
            case UpgradeId.LyraEncoreField:
                _echoChance = Mathf.Min(0.72f, _echoChance + 0.1f);
                break;
            case UpgradeId.OrionCometSpear:
            case UpgradeId.OrionDeadeyeMark:
                _damageMultiplier += 0.1f;
                break;
            case UpgradeId.OrionStarfallQuiver:
            case UpgradeId.OrionPerihelionVector:
                _dashCooldown = 0.0f;
                _energy = Mathf.Clamp(_energy + 22.0f, 0.0f, _maxEnergy);
                break;
        }

        Burst(ScreenCenter, color, 42, 520.0f, 0.88f);
        PlaySfx(520.0f, 180.0f, 0.2f, 0.28f, 0.02f, 1);
    }

    private UpgradeCard CreateCard(UpgradeId id)
    {
        int nextRank = GetRank(id) + 1;
        string rank = Tf("rank", nextRank);
        UpgradeCard card = id switch
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
            UpgradeId.ChainRelay => new UpgradeCard { Id = id, Title = T("upgrade.chain.title"), Tag = rank, Body = T("upgrade.chain.body"), Accent = new Color(0.34f, 0.9f, 1.0f) },
            UpgradeId.FractalSplit => new UpgradeCard { Id = id, Title = T("upgrade.fractal.title"), Tag = rank, Body = T("upgrade.fractal.body"), Accent = new Color(0.92f, 0.74f, 1.0f) },
            UpgradeId.SolarThesis => new UpgradeCard { Id = id, Title = T("upgrade.solar.title"), Tag = rank, Body = T("upgrade.solar.body"), Accent = new Color(1.0f, 0.86f, 0.32f) },
            UpgradeId.EmergencyRepair => new UpgradeCard { Id = id, Title = T("upgrade.repair.title"), Tag = T("choice.instant"), Body = T("upgrade.repair.body"), Accent = Jade },
            UpgradeId.OneWaveOverdrive => new UpgradeCard { Id = id, Title = T("upgrade.overdrive.title"), Tag = T("choice.tactic"), Body = T("upgrade.overdrive.body"), Accent = Rose },
            UpgradeId.GlassCannon => new UpgradeCard { Id = id, Title = T("upgrade.glass.title"), Tag = T("choice.risk"), Body = T("upgrade.glass.body"), Accent = new Color(1.0f, 0.42f, 0.24f) },
            UpgradeId.BountyContract => new UpgradeCard { Id = id, Title = T("upgrade.bounty.title"), Tag = T("choice.contract"), Body = T("upgrade.bounty.body"), Accent = Gold },
            UpgradeId.BulletTransmute => new UpgradeCard { Id = id, Title = T("upgrade.transmute.title"), Tag = T("choice.instant"), Body = T("upgrade.transmute.body"), Accent = Cyan },
            UpgradeId.HarmonicMap => new UpgradeCard { Id = id, Title = T("upgrade.map.title"), Tag = T("choice.meta"), Body = T("upgrade.map.body"), Accent = Violet },
            UpgradeId.PulseMagazine => new UpgradeCard { Id = id, Title = T("upgrade.pulse.title"), Tag = rank, Body = T("upgrade.pulse.body"), Accent = new Color(0.74f, 0.9f, 1.0f) },
            UpgradeId.ExecutionMark => new UpgradeCard { Id = id, Title = T("upgrade.execution.title"), Tag = rank, Body = T("upgrade.execution.body"), Accent = new Color(1.0f, 0.46f, 0.34f) },
            UpgradeId.StasisField => new UpgradeCard { Id = id, Title = T("upgrade.stasis.title"), Tag = rank, Body = T("upgrade.stasis.body"), Accent = new Color(0.6f, 0.8f, 1.0f) },
            UpgradeId.MagnetizedCore => new UpgradeCard { Id = id, Title = T("upgrade.magnet.title"), Tag = rank, Body = T("upgrade.magnet.body"), Accent = new Color(0.64f, 1.0f, 0.72f) },
            UpgradeId.RicochetMatrix => new UpgradeCard { Id = id, Title = T("upgrade.ricochet.title"), Tag = rank, Body = T("upgrade.ricochet.body"), Accent = new Color(0.96f, 0.76f, 1.0f) },
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
            UpgradeId.NyxOrbit => new UpgradeCard { Id = id, Title = T("upgrade.nyx.orbit.title"), Tag = rank, Body = T("upgrade.nyx.orbit.body"), Accent = PilotAccent(PilotKind.Nyx) },
            UpgradeId.NyxSingularity => new UpgradeCard { Id = id, Title = T("upgrade.nyx.singularity.title"), Tag = rank, Body = T("upgrade.nyx.singularity.body"), Accent = PilotAccent(PilotKind.Nyx).Lerp(Paper, 0.18f) },
            UpgradeId.NyxEventHorizon => new UpgradeCard { Id = id, Title = T("upgrade.nyx.horizon.title"), Tag = rank, Body = T("upgrade.nyx.horizon.body"), Accent = Violet },
            UpgradeId.NyxGravityCantor => new UpgradeCard { Id = id, Title = T("upgrade.nyx.cantor.title"), Tag = rank, Body = T("upgrade.nyx.cantor.body"), Accent = new Color(0.54f, 0.7f, 1.0f) },
            UpgradeId.RookBulwarkCore => new UpgradeCard { Id = id, Title = T("upgrade.rook.bulwark.title"), Tag = rank, Body = T("upgrade.rook.bulwark.body"), Accent = PilotAccent(PilotKind.Rook) },
            UpgradeId.RookSiegeBattery => new UpgradeCard { Id = id, Title = T("upgrade.rook.siege.title"), Tag = rank, Body = T("upgrade.rook.siege.body"), Accent = Gold },
            UpgradeId.RookAegisRelay => new UpgradeCard { Id = id, Title = T("upgrade.rook.aegis.title"), Tag = rank, Body = T("upgrade.rook.aegis.body"), Accent = Jade },
            UpgradeId.RookCitadelProtocol => new UpgradeCard { Id = id, Title = T("upgrade.rook.citadel.title"), Tag = rank, Body = T("upgrade.rook.citadel.body"), Accent = new Color(1.0f, 0.84f, 0.42f) },
            UpgradeId.LyraResonanceChord => new UpgradeCard { Id = id, Title = T("upgrade.lyra.chord.title"), Tag = rank, Body = T("upgrade.lyra.chord.body"), Accent = PilotAccent(PilotKind.Lyra) },
            UpgradeId.LyraTempoBloom => new UpgradeCard { Id = id, Title = T("upgrade.lyra.tempo.title"), Tag = rank, Body = T("upgrade.lyra.tempo.body"), Accent = new Color(0.5f, 0.98f, 0.9f) },
            UpgradeId.LyraHarmonicCascade => new UpgradeCard { Id = id, Title = T("upgrade.lyra.cascade.title"), Tag = rank, Body = T("upgrade.lyra.cascade.body"), Accent = new Color(0.62f, 0.86f, 1.0f) },
            UpgradeId.LyraEncoreField => new UpgradeCard { Id = id, Title = T("upgrade.lyra.encore.title"), Tag = rank, Body = T("upgrade.lyra.encore.body"), Accent = Violet },
            UpgradeId.OrionCometSpear => new UpgradeCard { Id = id, Title = T("upgrade.orion.spear.title"), Tag = rank, Body = T("upgrade.orion.spear.body"), Accent = PilotAccent(PilotKind.Orion) },
            UpgradeId.OrionDeadeyeMark => new UpgradeCard { Id = id, Title = T("upgrade.orion.deadeye.title"), Tag = rank, Body = T("upgrade.orion.deadeye.body"), Accent = Rose },
            UpgradeId.OrionStarfallQuiver => new UpgradeCard { Id = id, Title = T("upgrade.orion.quiver.title"), Tag = rank, Body = T("upgrade.orion.quiver.body"), Accent = new Color(0.9f, 0.78f, 1.0f) },
            UpgradeId.OrionPerihelionVector => new UpgradeCard { Id = id, Title = T("upgrade.orion.perihelion.title"), Tag = rank, Body = T("upgrade.orion.perihelion.body"), Accent = Cyan },
            _ => new UpgradeCard { Id = id, Title = T("upgrade.unknown.title"), Tag = Tf("rank", 1), Body = T("upgrade.unknown.body"), Accent = Paper },
        };
        return ApplyCapstonePreview(card, nextRank);
    }

    private UpgradeCard ApplyCapstonePreview(UpgradeCard card, int nextRank)
    {
        int maxRank = MaxRank(card.Id);
        if (maxRank >= 20 || nextRank < maxRank)
        {
            return card;
        }

        string body = CapstoneBody(card.Id);
        if (body.Length <= 0)
        {
            return card;
        }

        card.Tag = T("choice.capstone");
        card.Body = $"{card.Body}\n{body}";
        return card;
    }

    private string CapstoneBody(UpgradeId id)
    {
        return id switch
        {
            UpgradeId.ChainRelay => T("capstone.chain"),
            UpgradeId.FractalSplit => T("capstone.fractal"),
            UpgradeId.PulseMagazine => T("capstone.pulse"),
            UpgradeId.ExecutionMark => T("capstone.execution"),
            UpgradeId.StasisField => T("capstone.stasis"),
            UpgradeId.MagnetizedCore => T("capstone.magnet"),
            UpgradeId.RicochetMatrix => T("capstone.ricochet"),
            UpgradeId.AstraRefraction => T("capstone.astra.refraction"),
            UpgradeId.AstraPrismWake => T("capstone.astra.wake"),
            UpgradeId.AstraNovaBloom => T("capstone.astra.nova"),
            UpgradeId.AstraTwinRefraction => T("capstone.astra.twin"),
            UpgradeId.VesperCapacitor => T("capstone.vesper.charge"),
            UpgradeId.VesperSplitRail => T("capstone.vesper.fork"),
            UpgradeId.VesperJudgmentCoil => T("capstone.vesper.judgment"),
            UpgradeId.VesperSeverLine => T("capstone.vesper.sever"),
            UpgradeId.KairoDroneBay => T("capstone.kairo.bay"),
            UpgradeId.KairoSwarmSync => T("capstone.kairo.sync"),
            UpgradeId.KairoOverrideMatrix => T("capstone.kairo.override"),
            UpgradeId.KairoRelayProtocol => T("capstone.kairo.relay"),
            UpgradeId.SolCoronaBloom => T("capstone.sol.bloom"),
            UpgradeId.SolSolarForge => T("capstone.sol.forge"),
            UpgradeId.SolFlareCore => T("capstone.sol.flare"),
            UpgradeId.SolRadiantMantle => T("capstone.sol.mantle"),
            UpgradeId.NyxOrbit => T("capstone.nyx.orbit"),
            UpgradeId.NyxSingularity => T("capstone.nyx.singularity"),
            UpgradeId.NyxEventHorizon => T("capstone.nyx.horizon"),
            UpgradeId.NyxGravityCantor => T("capstone.nyx.cantor"),
            UpgradeId.RookBulwarkCore => T("capstone.rook.bulwark"),
            UpgradeId.RookSiegeBattery => T("capstone.rook.siege"),
            UpgradeId.RookAegisRelay => T("capstone.rook.aegis"),
            UpgradeId.RookCitadelProtocol => T("capstone.rook.citadel"),
            UpgradeId.LyraResonanceChord => T("capstone.lyra.chord"),
            UpgradeId.LyraTempoBloom => T("capstone.lyra.tempo"),
            UpgradeId.LyraHarmonicCascade => T("capstone.lyra.cascade"),
            UpgradeId.LyraEncoreField => T("capstone.lyra.encore"),
            UpgradeId.OrionCometSpear => T("capstone.orion.spear"),
            UpgradeId.OrionDeadeyeMark => T("capstone.orion.deadeye"),
            UpgradeId.OrionStarfallQuiver => T("capstone.orion.quiver"),
            UpgradeId.OrionPerihelionVector => T("capstone.orion.perihelion"),
            _ => string.Empty,
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
            UpgradeId.ChainRelay => T("upgrade.chain.title"),
            UpgradeId.FractalSplit => T("upgrade.fractal.title"),
            UpgradeId.SolarThesis => T("upgrade.solar.title"),
            UpgradeId.EmergencyRepair => T("upgrade.repair.title"),
            UpgradeId.OneWaveOverdrive => T("upgrade.overdrive.title"),
            UpgradeId.GlassCannon => T("upgrade.glass.title"),
            UpgradeId.BountyContract => T("upgrade.bounty.title"),
            UpgradeId.BulletTransmute => T("upgrade.transmute.title"),
            UpgradeId.HarmonicMap => T("upgrade.map.title"),
            UpgradeId.PulseMagazine => T("upgrade.pulse.title"),
            UpgradeId.ExecutionMark => T("upgrade.execution.title"),
            UpgradeId.StasisField => T("upgrade.stasis.title"),
            UpgradeId.MagnetizedCore => T("upgrade.magnet.title"),
            UpgradeId.RicochetMatrix => T("upgrade.ricochet.title"),
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
            UpgradeId.NyxOrbit => T("upgrade.nyx.orbit.title"),
            UpgradeId.NyxSingularity => T("upgrade.nyx.singularity.title"),
            UpgradeId.NyxEventHorizon => T("upgrade.nyx.horizon.title"),
            UpgradeId.NyxGravityCantor => T("upgrade.nyx.cantor.title"),
            UpgradeId.RookBulwarkCore => T("upgrade.rook.bulwark.title"),
            UpgradeId.RookSiegeBattery => T("upgrade.rook.siege.title"),
            UpgradeId.RookAegisRelay => T("upgrade.rook.aegis.title"),
            UpgradeId.RookCitadelProtocol => T("upgrade.rook.citadel.title"),
            UpgradeId.LyraResonanceChord => T("upgrade.lyra.chord.title"),
            UpgradeId.LyraTempoBloom => T("upgrade.lyra.tempo.title"),
            UpgradeId.LyraHarmonicCascade => T("upgrade.lyra.cascade.title"),
            UpgradeId.LyraEncoreField => T("upgrade.lyra.encore.title"),
            UpgradeId.OrionCometSpear => T("upgrade.orion.spear.title"),
            UpgradeId.OrionDeadeyeMark => T("upgrade.orion.deadeye.title"),
            UpgradeId.OrionStarfallQuiver => T("upgrade.orion.quiver.title"),
            UpgradeId.OrionPerihelionVector => T("upgrade.orion.perihelion.title"),
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
            UpgradeId.ResonanceLeech => Jade.Lerp(Paper, 0.12f),
            UpgradeId.MoonWisp => PickupBlue.Lerp(Paper, 0.24f),
            UpgradeId.RiftNeedle => Rose.Lerp(Paper, 0.12f),
            UpgradeId.MirrorSkin => Gold.Lerp(Paper, 0.24f),
            UpgradeId.NovaCapacitor => Cyan.Lerp(Paper, 0.1f),
            UpgradeId.PolarityStorm => PolarityAmber,
            UpgradeId.CometTrail => AlertRed.Lerp(Gold, 0.18f),
            UpgradeId.AegisBloom => Jade,
            UpgradeId.QuantumEcho => Violet,
            UpgradeId.ChainRelay => Cyan,
            UpgradeId.FractalSplit => Rose.Lerp(Cyan, 0.16f),
            UpgradeId.SolarThesis => Gold.Lerp(Paper, 0.14f),
            UpgradeId.EmergencyRepair => Jade,
            UpgradeId.OneWaveOverdrive => Rose,
            UpgradeId.GlassCannon => AlertRed,
            UpgradeId.BountyContract => Gold,
            UpgradeId.BulletTransmute => Cyan,
            UpgradeId.HarmonicMap => Violet,
            UpgradeId.PulseMagazine => PickupBlue.Lerp(Paper, 0.2f),
            UpgradeId.ExecutionMark => AlertRed.Lerp(Gold, 0.08f),
            UpgradeId.StasisField => PickupBlue.Lerp(Paper, 0.18f),
            UpgradeId.MagnetizedCore => Jade.Lerp(Paper, 0.08f),
            UpgradeId.RicochetMatrix => Rose.Lerp(Paper, 0.16f),
            UpgradeId.AstraRefraction => PolarityBlue,
            UpgradeId.AstraPrismWake => PolarityAmber,
            UpgradeId.VesperCapacitor => AlertRed,
            UpgradeId.VesperSplitRail => Rose,
            UpgradeId.KairoDroneBay => PickupBlue,
            UpgradeId.KairoSwarmSync => XpGreen,
            UpgradeId.SolCoronaBloom => Gold,
            UpgradeId.SolSolarForge => Gold.Lerp(AlertRed, 0.28f),
            UpgradeId.AstraNovaBloom => PolarityBlue,
            UpgradeId.AstraTwinRefraction => PolarityAmber,
            UpgradeId.VesperJudgmentCoil => AlertRed,
            UpgradeId.VesperSeverLine => Rose,
            UpgradeId.KairoOverrideMatrix => PickupBlue,
            UpgradeId.KairoRelayProtocol => XpGreen,
            UpgradeId.SolFlareCore => Gold,
            UpgradeId.SolRadiantMantle => Gold.Lerp(Paper, 0.14f),
            UpgradeId.NyxOrbit => Violet.Lerp(Cyan, 0.1f),
            UpgradeId.NyxSingularity => Violet.Lerp(Paper, 0.18f),
            UpgradeId.NyxEventHorizon => Violet,
            UpgradeId.NyxGravityCantor => Violet.Lerp(Cyan, 0.22f),
            UpgradeId.RookBulwarkCore => Gold.Lerp(Paper, 0.3f),
            UpgradeId.RookSiegeBattery => Gold,
            UpgradeId.RookAegisRelay => Jade,
            UpgradeId.RookCitadelProtocol => Gold.Lerp(Paper, 0.18f),
            UpgradeId.LyraResonanceChord => Jade,
            UpgradeId.LyraTempoBloom => Jade.Lerp(Cyan, 0.18f),
            UpgradeId.LyraHarmonicCascade => Cyan.Lerp(Paper, 0.1f),
            UpgradeId.LyraEncoreField => Violet,
            UpgradeId.OrionCometSpear => Gold.Lerp(AlertRed, 0.12f),
            UpgradeId.OrionDeadeyeMark => Rose,
            UpgradeId.OrionStarfallQuiver => Rose.Lerp(Paper, 0.24f),
            UpgradeId.OrionPerihelionVector => Cyan,
            _ => Paper,
        };
    }

    private int GetRank(UpgradeId id)
    {
        return _upgradeRanks.TryGetValue(id, out int rank) ? rank : 0;
    }

    private bool IsUpgradeMaxed(UpgradeId id)
    {
        int maxRank = MaxRank(id);
        return maxRank < 20 && GetRank(id) >= maxRank;
    }

    private int MetaRank(MetaUpgradeId id)
    {
        return _metaRanks.TryGetValue(id, out int rank) ? rank : 0;
    }

    private static int MetaUpgradeCost(MetaUpgradeDef def, int currentRank)
    {
        return def.BaseCost + currentRank * def.StepCost + currentRank * currentRank * 18 + Math.Max(0, currentRank - 2) * 35;
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
            UpgradeId.ChainRelay => 6,
            UpgradeId.FractalSplit => 6,
            UpgradeId.NovaCapacitor => 5,
            UpgradeId.EmergencyRepair => 99,
            UpgradeId.OneWaveOverdrive => 99,
            UpgradeId.GlassCannon => 5,
            UpgradeId.BountyContract => 99,
            UpgradeId.BulletTransmute => 99,
            UpgradeId.HarmonicMap => 3,
            UpgradeId.PulseMagazine => 5,
            UpgradeId.ExecutionMark => 5,
            UpgradeId.StasisField => 5,
            UpgradeId.MagnetizedCore => 4,
            UpgradeId.RicochetMatrix => 5,
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
            UpgradeId.NyxOrbit => 5,
            UpgradeId.NyxSingularity => 5,
            UpgradeId.NyxEventHorizon => 4,
            UpgradeId.NyxGravityCantor => 3,
            UpgradeId.RookBulwarkCore => 5,
            UpgradeId.RookSiegeBattery => 5,
            UpgradeId.RookAegisRelay => 4,
            UpgradeId.RookCitadelProtocol => 3,
            UpgradeId.LyraResonanceChord => 5,
            UpgradeId.LyraTempoBloom => 5,
            UpgradeId.LyraHarmonicCascade => 4,
            UpgradeId.LyraEncoreField => 3,
            UpgradeId.OrionCometSpear => 5,
            UpgradeId.OrionDeadeyeMark => 5,
            UpgradeId.OrionStarfallQuiver => 4,
            UpgradeId.OrionPerihelionVector => 3,
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
        if (MetaHeld() && !_lastMeta)
        {
            ResetTitle();
            _mode = GameMode.Meta;
            return;
        }

        bool restart = ConfirmHeld() || StartHeld() || Input.IsMouseButtonPressed(MouseButton.Left);
        if (restart && !_lastRestart)
        {
            StartRun();
        }
        if (CancelHeld() && !_lastCancel)
        {
            ResetTitle();
        }
    }

    private void GenerateBackdrop()
    {
        _stars.Clear();
        _nebulas.Clear();
        int starCount = QualityStarCount();
        for (int i = 0; i < starCount; i++)
        {
            float depth = _rng.RandfRange(0.15f, 1.0f);
            Color color = i % 17 == 0 ? Gold.Lerp(Paper, 0.2f) : i % 11 == 0 ? Cyan.Lerp(Paper, 0.24f) : new Color(0.46f, 0.52f, 0.56f);
            _stars.Add(new Star
            {
                Pos = new Vector2(_rng.RandfRange(0.0f, ScreenWidth), _rng.RandfRange(0.0f, ScreenHeight)),
                Radius = _rng.RandfRange(0.45f, 1.35f) * depth,
                Twinkle = _rng.RandfRange(0.5f, 2.6f),
                Depth = depth,
                Color = color,
            });
        }

        _nebulas.Add(new Nebula { Pos = new Vector2(350.0f, 240.0f), Radius = 560.0f, Color = Cyan.Lerp(Void, 0.42f), Drift = 0.12f });
        _nebulas.Add(new Nebula { Pos = new Vector2(1500.0f, 300.0f), Radius = 680.0f, Color = Rose.Lerp(Void, 0.58f), Drift = -0.08f });
        if ((int)_visualQuality >= (int)VisualQuality.Medium)
        {
            _nebulas.Add(new Nebula { Pos = new Vector2(1010.0f, 910.0f), Radius = 620.0f, Color = Gold.Lerp(Void, 0.46f), Drift = 0.06f });
        }
        if ((int)_visualQuality >= (int)VisualQuality.High)
        {
            _nebulas.Add(new Nebula { Pos = new Vector2(1860.0f, 940.0f), Radius = 420.0f, Color = Jade.Lerp(Void, 0.52f), Drift = -0.13f });
        }
    }

    private void DrawBackdrop()
    {
        Color sectorColor = CurrentSector().Accent;
        DrawRect(new Rect2(Vector2.Zero, new Vector2(ScreenWidth, ScreenHeight)), Void, true);
        DrawRect(new Rect2(Vector2.Zero, new Vector2(ScreenWidth, ScreenHeight)), Alpha(Ink, 0.92f), true);

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
        DrawRect(new Rect2(Vector2.Zero, new Vector2(ScreenWidth, ScreenHeight)), Void, true);

        Vector2 galaxyCenter = new(ScreenWidth * 0.5f, 360.0f);
        for (int i = 6; i >= 1; i--)
        {
            float t = i / 6.0f;
            Color aura = i % 3 == 0 ? Rose : i % 2 == 0 ? Gold : Cyan;
            DrawCircle(galaxyCenter + new Vector2(Mathf.Sin(_time * 0.05f + i) * 38.0f, Mathf.Cos(_time * 0.04f + i) * 18.0f), 520.0f * t, Alpha(aura.Lerp(Void, 0.55f), 0.021f * (1.0f - t + 0.25f)));
        }

        for (int i = 0; i < 7; i++)
        {
            float r = 220.0f + i * 82.0f;
            float a = _time * 0.018f + i * 0.33f;
            Color arc = i % 3 == 0 ? Gold : i % 3 == 1 ? Cyan : Rose;
            DrawArc(galaxyCenter + new Vector2(0.0f, 18.0f), r, a, a + Mathf.Pi * 1.18f, 96, Alpha(arc, 0.031f - i * 0.003f), UiHairline, true);
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
        DrawCircle(planet, 860.0f, Alpha(Ink, 0.96f));
        DrawArc(planet, 860.0f, Mathf.Pi * 1.08f, Mathf.Pi * 1.92f, 160, Alpha(Cyan, 0.2f), UiStroke, true);
        DrawArc(planet, 805.0f, Mathf.Pi * 1.1f, Mathf.Pi * 1.9f, 160, Alpha(Gold, 0.12f), UiHairline, true);
        DrawCircle(new Vector2(ScreenWidth * 0.22f, ScreenHeight * 0.28f), 260.0f, Alpha(Rose, 0.026f));
        DrawCircle(new Vector2(ScreenWidth * 0.82f, ScreenHeight * 0.32f), 320.0f, Alpha(Cyan, 0.028f));
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
        Color polarity = PilotAccent(_runPilot);
        float invuln = _invulnTimer > 0.0f ? 0.5f + 0.5f * Mathf.Sin(_time * 36.0f) : 1.0f;
        Vector2 forward = _aimDir;

        DrawPlayerTrail(p, polarity, invuln);
        DrawPilotHull(_runPilot, p, forward, polarity, invuln, 1.0f);
        DrawPolarityCooldownBadge(p, polarity);

        if (_dashTimer > 0.0f)
        {
            DrawLine(p - forward * 130.0f, p + forward * 18.0f, Alpha(polarity, 0.28f), 9.0f, true);
            DrawLine(p - forward * 152.0f, p + forward * 24.0f, Alpha(Paper, 0.24f), 2.0f, true);
        }

        int orbiterCount = Math.Min(_orbiters, MaxOrbiters);
        for (int i = 0; i < orbiterCount; i++)
        {
            DrawOrbiter(_orbiterVisuals[i], polarity);
        }
    }

    private void DrawOrbiter(OrbiterVisual visual, Color polarity)
    {
        if (!visual.Active)
        {
            return;
        }

        Vector2 pos = visual.Pos + ShakeOffset();
        float charge = 1.0f - Mathf.Clamp(_orbiterFireTimer / OrbiterFireInterval(), 0.0f, 1.0f);
        bool kairo = _runPilot == PilotKind.Kairo;
        Texture2D? texture = kairo ? _kairoDroneTexture : _supportDroneTexture;
        Rect2 region = kairo ? _kairoDroneRegion : _supportDroneRegion;
        Color accent = kairo ? PickupBlue.Lerp(Rose, 0.28f) : polarity.Lerp(Paper, 0.14f);
        float pulse = Mathf.Max(charge * 0.16f, visual.CommandPulse * 0.28f);

        if (texture != null && region.Size.X > 0.0f && region.Size.Y > 0.0f)
        {
            Vector2 drawSize = DroneTextureDrawSize(region.Size, kairo);
            float shadowRadius = Mathf.Max(drawSize.X, drawSize.Y) * 0.34f;
            if (_visualPressure < 0.86f)
            {
                DrawCircle(pos + new Vector2(0.0f, 4.0f), shadowRadius, Alpha(Void, 0.34f));
                DrawCircle(pos, shadowRadius * (1.0f + charge * 0.22f), Alpha(accent, 0.08f + charge * 0.04f));
            }

            DrawFacingTexture(texture, region, pos, visual.Facing, drawSize, Alpha(Colors.White, 0.92f));
            if (visual.CommandPulse > 0.0f && _visualPressure < 0.84f)
            {
                float ringRadius = Mathf.Max(drawSize.X, drawSize.Y) * (0.52f + visual.CommandPulse * 0.22f);
                DrawArc(pos, ringRadius, -Mathf.Pi * 0.5f, Mathf.Pi * 1.5f, 24, Alpha(accent, visual.CommandPulse * 0.26f), UiHairline, true);
            }
            return;
        }

        DrawCircle(pos, 9.0f, Alpha(Graphite, 0.94f));
        DrawCircle(pos, 9.0f, Alpha(accent, 0.76f), false, 1.6f, true);
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
        Color next = PilotAccent(_runPilot).Lerp(Paper, 0.22f);

        DrawCircle(center, radius + 2.0f, Alpha(Void, 0.62f));
        DrawCircle(center, radius, Alpha(Graphite, 0.84f));
        DrawCircle(center, radius, Alpha(Paper, 0.16f), false, UiHairline, true);
        DrawArc(center, radius, -Mathf.Pi * 0.5f, -Mathf.Pi * 0.5f + Mathf.Tau * ready, 24, Alpha(polarity, 0.72f), UiHairline, true);
        DrawCircle(center, 3.0f, Alpha(next, 0.42f));
    }

    private void DrawPilotHull(PilotKind pilot, Vector2 center, Vector2 forward, Color polarity, float invuln, float scale)
    {
        if (TryDrawPilotTexture(pilot, center, forward, invuln, scale))
        {
            return;
        }

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
            case PilotKind.Nyx:
            {
                Vector2[] hull =
                {
                    center + forward * 28.0f * scale,
                    center + right * 24.0f * scale + forward * 4.0f * scale,
                    center + right * 12.0f * scale - forward * 28.0f * scale,
                    center - forward * 12.0f * scale,
                    center - right * 12.0f * scale - forward * 28.0f * scale,
                    center - right * 24.0f * scale + forward * 4.0f * scale,
                };
                DrawColoredPolygon(hull, body, Array.Empty<Vector2>(), null);
                DrawPolyline(ClosePolygon(hull), line, 2.0f * scale, true);
                DrawCircle(center, 22.0f * scale, Alpha(polarity, 0.18f * invuln), false, 1.4f * scale, true);
                DrawArc(center, 15.0f * scale, forward.Angle() - 1.2f, forward.Angle() + 1.2f, 22, Alpha(Paper, 0.42f * invuln), 1.3f * scale, true);
                DrawCircle(center, 7.0f * scale, Alpha(polarity, 0.78f * invuln));
                break;
            }
            case PilotKind.Rook:
            {
                Vector2[] hull =
                {
                    center + forward * 28.0f * scale,
                    center + right * 31.0f * scale + forward * 5.0f * scale,
                    center + right * 27.0f * scale - forward * 24.0f * scale,
                    center - forward * 32.0f * scale,
                    center - right * 27.0f * scale - forward * 24.0f * scale,
                    center - right * 31.0f * scale + forward * 5.0f * scale,
                };
                DrawColoredPolygon(hull, body, Array.Empty<Vector2>(), null);
                DrawPolyline(ClosePolygon(hull), line, 2.0f * scale, true);
                DrawLine(center - right * 24.0f * scale, center + right * 24.0f * scale, Alpha(polarity, 0.42f * invuln), 2.0f * scale, true);
                DrawDiamond(center, 10.0f * scale, Alpha(polarity, 0.78f * invuln), 0.0f);
                break;
            }
            case PilotKind.Lyra:
            {
                Vector2[] hull =
                {
                    center + forward * 34.0f * scale,
                    center + right * 14.0f * scale + forward * 6.0f * scale,
                    center + right * 26.0f * scale - forward * 16.0f * scale,
                    center - forward * 24.0f * scale,
                    center - right * 26.0f * scale - forward * 16.0f * scale,
                    center - right * 14.0f * scale + forward * 6.0f * scale,
                };
                DrawColoredPolygon(hull, body, Array.Empty<Vector2>(), null);
                DrawPolyline(ClosePolygon(hull), line, 2.0f * scale, true);
                DrawCircle(center, 17.0f * scale, Alpha(polarity, 0.16f * invuln), false, 1.2f * scale, true);
                DrawCircle(center, 9.0f * scale, Alpha(polarity, 0.78f * invuln), false, 2.0f * scale, true);
                DrawLine(center - right * 20.0f * scale, center + right * 20.0f * scale, softLine, 1.2f * scale, true);
                break;
            }
            case PilotKind.Orion:
            {
                Vector2[] hull =
                {
                    center + forward * 46.0f * scale,
                    center + right * 12.0f * scale + forward * 6.0f * scale,
                    center + right * 22.0f * scale - forward * 24.0f * scale,
                    center - forward * 16.0f * scale,
                    center - right * 22.0f * scale - forward * 24.0f * scale,
                    center - right * 12.0f * scale + forward * 6.0f * scale,
                };
                DrawColoredPolygon(hull, body, Array.Empty<Vector2>(), null);
                DrawPolyline(ClosePolygon(hull), line, 2.0f * scale, true);
                DrawLine(center - forward * 12.0f * scale, center + forward * 42.0f * scale, Alpha(polarity, 0.7f * invuln), 1.8f * scale, true);
                DrawCircle(center + forward * 9.0f * scale, 6.5f * scale, Alpha(Paper, 0.52f * invuln));
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

    private bool TryDrawPilotTexture(PilotKind pilot, Vector2 center, Vector2 forward, float alpha, float scale)
    {
        if (!_pilotTextures.TryGetValue(pilot, out Texture2D? texture) || texture == null)
        {
            return false;
        }

        Rect2 sourceRegion = PilotTextureRegion(pilot, texture);
        Vector2 sourceSize = sourceRegion.Size;
        if (sourceSize.X <= 0.0f || sourceSize.Y <= 0.0f)
        {
            return false;
        }

        Vector2 direction = forward.LengthSquared() > 0.01f ? forward.Normalized() : Vector2.Right;
        Vector2 drawSize = PilotTextureDrawSize(pilot, sourceSize, scale);
        Color tint = Alpha(Colors.White, Mathf.Clamp(alpha, 0.0f, 1.0f));
        DrawSetTransform(center, direction.Angle() + Mathf.Pi * 0.5f, Vector2.One);
        DrawTextureRectRegion(texture, new Rect2(drawSize * -0.5f, drawSize), sourceRegion, tint);
        DrawSetTransform(Vector2.Zero, 0.0f, Vector2.One);
        return true;
    }

    private Rect2 PilotTextureRegion(PilotKind pilot, Texture2D texture)
    {
        if (_pilotTextureRegions.TryGetValue(pilot, out Rect2 region) && region.Size.X > 0.0f && region.Size.Y > 0.0f)
        {
            return region;
        }

        return new Rect2(Vector2.Zero, texture.GetSize());
    }

    private static Vector2 PilotTextureDrawSize(PilotKind pilot, Vector2 sourceSize, float scale)
    {
        float area = Mathf.Max(1.0f, sourceSize.X * sourceSize.Y);
        float baseScale = Mathf.Sqrt(PilotTextureTargetArea / area) * PilotTextureScaleBias(pilot) * scale;
        Vector2 drawSize = sourceSize * baseScale;
        float maxSide = Mathf.Max(drawSize.X, drawSize.Y);
        if (maxSide <= 0.0f)
        {
            return drawSize;
        }

        float minSide = PilotTextureMinMaxSide * scale;
        float maxAllowedSide = PilotTextureMaxMaxSide * scale;
        if (maxSide < minSide)
        {
            drawSize *= minSide / maxSide;
        }
        else if (maxSide > maxAllowedSide)
        {
            drawSize *= maxAllowedSide / maxSide;
        }

        return drawSize;
    }

    private static float PilotTextureScaleBias(PilotKind pilot)
    {
        return pilot switch
        {
            PilotKind.Rook => 1.04f,
            PilotKind.Kairo or PilotKind.Nyx => 1.02f,
            PilotKind.Orion => 0.98f,
            _ => 1.0f,
        };
    }

    private void DrawEnemy(Enemy enemy)
    {
        Vector2 p = enemy.Pos + ShakeOffset();
        Color color = EnemyStateColor(enemy);
        float hp = Mathf.Clamp(enemy.Hp / enemy.MaxHp, 0.0f, 1.0f);
        float pulse = 1.0f + enemy.SpawnPulse * 0.7f + Mathf.Sin(_time * 5.0f + enemy.Phase) * 0.05f;
        float charge = EnemyTelegraph01(enemy);
        float overheat = EnemyOverheat01(enemy);
        float dashWarm = enemy.DashWarmup > 0.0f ? EnemyDashWarmup01(enemy) : 0.0f;
        bool importantState = charge > 0.0f || overheat > 0.0f || dashWarm > 0.0f || enemy.DashTime > 0.0f;
        bool heavyVisualLoad = _visualPressure > 0.72f && enemy.Kind != EnemyKind.Boss && !enemy.Elite && !importantState;
        bool artBacked = HasEnemyTexture(enemy);

        if (!artBacked && !heavyVisualLoad)
        {
            DrawGlow(p, charge > 0.0f ? EnemyBulletColor() : color, enemy.Radius * (1.55f + enemy.SpawnPulse * 0.55f + charge * 0.65f + overheat * 0.28f), enemy.Kind == EnemyKind.Boss ? 0.08f : 0.025f + charge * 0.04f, 3);
        }
        if (!artBacked)
        {
            DrawCircle(p, enemy.Radius * pulse, Alpha(color, heavyVisualLoad ? 0.12f : 0.2f), false, heavyVisualLoad ? 1.2f : 1.6f + enemy.SpawnPulse * 2.0f, true);
        }

        if (dashWarm > 0.0f && !heavyVisualLoad)
        {
            Vector2 dashDir = enemy.DashDir.LengthSquared() > 0.01f ? enemy.DashDir.Normalized() : Vector2.Down;
            float length = enemy.Radius + 84.0f + dashWarm * 74.0f;
            DrawLine(p + dashDir * enemy.Radius * 0.8f, p + dashDir * length, Alpha(Gold, 0.2f + dashWarm * 0.38f), UiHairline + dashWarm * 0.8f, true);
            DrawLine(p - dashDir * enemy.Radius * 0.55f, p + dashDir * enemy.Radius * 0.9f, Alpha(Paper, 0.28f + dashWarm * 0.18f), UiHairline, true);
            if (!artBacked)
            {
                DrawCircle(p, enemy.Radius * (1.12f + dashWarm * 0.28f), Alpha(Gold, 0.18f + dashWarm * 0.2f), false, UiHairline, true);
            }
        }
        else if (enemy.DashTime > 0.0f && !heavyVisualLoad)
        {
            Vector2 dashDir = enemy.DashDir.LengthSquared() > 0.01f ? enemy.DashDir.Normalized() : Vector2.Down;
            DrawLine(p - dashDir * enemy.Radius * 1.7f, p - dashDir * enemy.Radius * 0.2f, Alpha(Gold, 0.28f), 2.0f, true);
        }
        else if (charge > 0.0f && !heavyVisualLoad)
        {
            Vector2 aim = (_playerPos - enemy.Pos).LengthSquared() > 0.01f ? (_playerPos - enemy.Pos).Normalized() : Vector2.Down;
            DrawLine(p + aim * enemy.Radius * 0.8f, p + aim * (enemy.Radius + 62.0f + charge * 36.0f), Alpha(EnemyBulletColor(), 0.18f + charge * 0.32f), UiHairline + charge * 0.55f, true);
            if (!artBacked)
            {
                DrawCircle(p, enemy.Radius * (1.16f + charge * 0.22f), Alpha(EnemyBulletColor(), 0.24f + charge * 0.18f), false, UiHairline, true);
            }
        }
        else if (overheat > 0.0f && !heavyVisualLoad && !artBacked)
        {
            DrawCircle(p, enemy.Radius * (1.18f + overheat * 0.12f), Alpha(Gold, 0.18f + overheat * 0.16f), false, UiHairline, true);
        }

        if (enemy.Kind == EnemyKind.Boss)
        {
            DrawBoss(enemy, p, color, hp);
            return;
        }

        if (TryDrawEnemyTexture(enemy, p, color, hp))
        {
            return;
        }

        float rotation = _time * (enemy.Kind == EnemyKind.Turret ? -0.8f : 1.2f) + enemy.Phase;
        if (heavyVisualLoad)
        {
            DrawSimpleEnemy(enemy, p, color, hp, rotation);
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

    private bool HasEnemyTexture(Enemy enemy)
    {
        if (enemy.Kind == EnemyKind.Boss)
        {
            return _bossTextures.ContainsKey(enemy.BossArchetype);
        }

        return (enemy.Elite && _eliteEnemyTextures.ContainsKey(enemy.Kind)) || _enemyTextures.ContainsKey(enemy.Kind);
    }

    private bool TryDrawEnemyTexture(Enemy enemy, Vector2 p, Color color, float hp)
    {
        Texture2D? texture = null;
        if (enemy.Elite)
        {
            _eliteEnemyTextures.TryGetValue(enemy.Kind, out texture);
        }

        if (texture == null && !_enemyTextures.TryGetValue(enemy.Kind, out texture))
        {
            return false;
        }

        Vector2 facing = EnemyFacingDirection(enemy);
        Rect2 sourceRegion = EnemyTextureRegion(enemy, texture);
        Vector2 drawSize = EnemyTextureDrawSize(enemy, sourceRegion.Size);
        DrawFacingTexture(texture, sourceRegion, p, facing, drawSize, Alpha(Colors.White, 0.95f));

        if (hp < 0.995f || enemy.Elite)
        {
            Rect2 bar = new(p + new Vector2(-enemy.Radius, enemy.Radius + 12.0f), new Vector2(enemy.Radius * 2.0f, 5.0f));
            DrawRect(bar, Alpha(Paper, 0.13f), true);
            DrawRect(new Rect2(bar.Position, new Vector2(bar.Size.X * hp, bar.Size.Y)), Alpha(color, 0.78f), true);
        }

        return true;
    }

    private Rect2 EnemyTextureRegion(Enemy enemy, Texture2D texture)
    {
        Dictionary<EnemyKind, Rect2> regions = enemy.Elite ? _eliteEnemyTextureRegions : _enemyTextureRegions;
        if (regions.TryGetValue(enemy.Kind, out Rect2 region) && region.Size.X > 0.0f && region.Size.Y > 0.0f)
        {
            return region;
        }

        return new Rect2(Vector2.Zero, texture.GetSize());
    }

    private static Vector2 EnemyTextureDrawSize(Enemy enemy, Vector2 sourceSize)
    {
        if (sourceSize.X <= 0.0f || sourceSize.Y <= 0.0f)
        {
            return Vector2.Zero;
        }

        float rootScale = enemy.Elite ? EliteEnemyTextureRootScale : EnemyTextureRootScale;
        float visualRoot = enemy.Radius * rootScale * EnemyTextureScaleBias(enemy.Kind, enemy.Elite);
        return FitTextureSizeToArea(sourceSize, visualRoot * visualRoot, enemy.Radius * EnemyTextureMinMaxSideScale, enemy.Radius * EnemyTextureMaxMaxSideScale);
    }

    private static float EnemyTextureScaleBias(EnemyKind kind, bool elite)
    {
        float bias = kind switch
        {
            EnemyKind.Shard => 0.92f,
            EnemyKind.Harrier => 0.96f,
            EnemyKind.Chaser => 0.98f,
            EnemyKind.Mine => 0.92f,
            EnemyKind.Turret => 1.03f,
            EnemyKind.Splitter => 1.04f,
            EnemyKind.Siren => 1.04f,
            EnemyKind.Lance => 1.06f,
            EnemyKind.Bulwark => 1.08f,
            EnemyKind.Warden => 1.1f,
            _ => 1.0f,
        };

        return elite ? bias * 1.04f : bias;
    }

    private void DrawSimpleEnemy(Enemy enemy, Vector2 p, Color color, float hp, float rotation)
    {
        float r = enemy.Radius;
        DrawCircle(p, r * 0.82f, Alpha(Graphite, 0.76f));
        DrawCircle(p, r * 0.76f, Alpha(color, 0.58f), false, UiHairline, true);

        Vector2 a = Vector2.Right.Rotated(rotation);
        Vector2 b = Vector2.Down.Rotated(rotation);
        switch (enemy.Kind)
        {
            case EnemyKind.Chaser:
            case EnemyKind.Harrier:
                DrawLine(p - a * r * 0.3f, p + a * r * 0.48f, Alpha(Paper, 0.5f), UiHairline, true);
                break;
            case EnemyKind.Turret:
            case EnemyKind.Mine:
            case EnemyKind.Siren:
                DrawCircle(p, r * 0.32f, Alpha(Paper, 0.24f), false, UiHairline, true);
                break;
            case EnemyKind.Bulwark:
            case EnemyKind.Warden:
                DrawLine(p - a * r * 0.42f, p + a * r * 0.42f, Alpha(Paper, 0.42f), UiHairline, true);
                DrawLine(p - b * r * 0.42f, p + b * r * 0.42f, Alpha(Paper, 0.34f), UiHairline, true);
                break;
            default:
                DrawLine(p - a * r * 0.42f, p + a * r * 0.42f, Alpha(Paper, 0.42f), UiHairline, true);
                break;
        }

        if (hp < 0.995f)
        {
            Rect2 bar = new(p + new Vector2(-r * 0.72f, r + 9.0f), new Vector2(r * 1.44f, 3.0f));
            DrawRect(bar, Alpha(Paper, 0.1f), true);
            DrawRect(new Rect2(bar.Position, new Vector2(bar.Size.X * hp, bar.Size.Y)), Alpha(color, 0.62f), true);
        }
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
        BossArchetype archetype = enemy.BossArchetype;
        Color accent = BossAccent(archetype);
        color = color.Lerp(accent, 0.32f);
        float rotation = _time * (archetype == BossArchetype.Prism ? 0.58f : archetype == BossArchetype.Forge ? 0.18f : 0.35f);
        if (TryDrawBossTexture(enemy, p, accent))
        {
            DrawBossTextureCues(enemy, p, accent, rotation);
            return;
        }

        int sides = archetype switch
        {
            BossArchetype.Prism => 6,
            BossArchetype.Swarm => 10,
            BossArchetype.Forge => 4,
            BossArchetype.Rift => 7,
            BossArchetype.Mirror => 6,
            BossArchetype.Tempest => 9,
            BossArchetype.Bastion => 4,
            BossArchetype.Serpent => 11,
            BossArchetype.Oracle => 5,
            _ => 8,
        };
        Vector2[] core = RegularPolygon(p, enemy.Radius, sides, rotation);
        DrawColoredPolygon(core, Alpha(Graphite, 0.86f), Array.Empty<Vector2>(), null);
        DrawPolyline(ClosePolygon(core), Alpha(color, 0.86f), 2.4f, true);
        DrawPolyline(ClosePolygon(RegularPolygon(p, enemy.Radius * 1.38f, sides, -rotation * 0.65f)), Alpha(GridLine, 0.28f), 1.8f, true);
        DrawCircle(p, enemy.Radius * 0.52f, Alpha(Ink, 0.88f));
        DrawCircle(p, enemy.Radius * 0.34f, Alpha(color, 0.84f), false, 3.0f, true);
        if (enemy.BossGuard > 0.0f)
        {
            float guard = Mathf.Clamp(enemy.BossGuard / 1.2f, 0.0f, 1.0f);
            DrawCircle(p, enemy.Radius * (1.58f + guard * 0.18f), Alpha(Paper, 0.08f + guard * 0.12f), false, UiStroke, true);
            DrawArc(p, enemy.Radius * 1.7f, -Mathf.Pi * 0.5f, -Mathf.Pi * 0.5f + Mathf.Tau * guard, 48, Alpha(accent, 0.34f + guard * 0.28f), UiStroke, true);
        }
        if (enemy.BossIntentPulse > 0.0f)
        {
            float intent = Mathf.Clamp(enemy.BossIntentPulse / 1.45f, 0.0f, 1.0f);
            float ring = enemy.Radius * (1.62f + (1.0f - intent) * 0.28f);
            Color cue = Alpha(accent.Lerp(Paper, intent * 0.18f), 0.22f + intent * 0.34f);
            Vector2 aim = (_playerPos - enemy.Pos).LengthSquared() > 0.01f ? (_playerPos - enemy.Pos).Normalized() : Vector2.Down;
            DrawCircle(p, ring, cue, false, UiHairline, true);
            if (enemy.BossIntent is BossPatternKind.HeavyLance or BossPatternKind.OracleSnipe or BossPatternKind.HazardFan)
            {
                DrawLine(p, p + aim * ring * 1.18f, Alpha(EnemyBulletColor(), 0.22f + intent * 0.22f), UiStroke, true);
            }
            else if (enemy.BossIntent is BossPatternKind.SpiralRing or BossPatternKind.ReverseSpiral or BossPatternKind.TempestWheel or BossPatternKind.SerpentCoil)
            {
                DrawArc(p, ring * 0.92f, rotation, rotation + Mathf.Pi * 1.6f, 36, cue, UiStroke, true);
            }
            else if (enemy.BossIntent is BossPatternKind.SummonWing or BossPatternKind.WardenCall or BossPatternKind.MineDrift)
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector2 node = p + Vector2.Right.Rotated(rotation + i * Mathf.Tau / 4.0f) * ring * 0.62f;
                    DrawDiamond(node, enemy.Radius * 0.08f, cue, rotation);
                }
            }
        }

        if (archetype == BossArchetype.Swarm)
        {
            int nodes = 6;
            for (int i = 0; i < nodes; i++)
            {
                Vector2 node = p + Vector2.Right.Rotated(rotation + i * Mathf.Tau / nodes) * enemy.Radius * 0.68f;
                DrawCircle(node, enemy.Radius * 0.08f, Alpha(accent, 0.72f), false, UiStroke, true);
                DrawLine(p, node, Alpha(accent, 0.2f), UiHairline, true);
            }
        }
        else if (archetype == BossArchetype.Forge)
        {
            DrawLine(p - Vector2.Right.Rotated(rotation) * enemy.Radius * 0.72f, p + Vector2.Right.Rotated(rotation) * enemy.Radius * 0.72f, Alpha(Gold, 0.48f), 2.0f, true);
            DrawLine(p - Vector2.Down.Rotated(rotation) * enemy.Radius * 0.72f, p + Vector2.Down.Rotated(rotation) * enemy.Radius * 0.72f, Alpha(Rose, 0.34f), 2.0f, true);
            DrawArc(p, enemy.Radius * 0.68f, rotation, rotation + Mathf.Pi * 1.2f, 30, Alpha(accent, 0.5f), UiStroke, true);
        }
        else if (archetype == BossArchetype.Rift)
        {
            DrawArc(p, enemy.Radius * 0.7f, -rotation, -rotation + Mathf.Pi * 1.55f, 34, Alpha(Violet, 0.58f), UiStroke, true);
            DrawArc(p, enemy.Radius * 0.46f, rotation * 1.4f, rotation * 1.4f + Mathf.Pi, 24, Alpha(Cyan, 0.34f), UiHairline, true);
        }
        else if (archetype == BossArchetype.Mirror)
        {
            DrawLine(p - Vector2.Right.Rotated(rotation) * enemy.Radius * 0.72f, p + Vector2.Right.Rotated(rotation) * enemy.Radius * 0.72f, Alpha(Paper, 0.48f), 1.7f, true);
            DrawLine(p - Vector2.Right.Rotated(-rotation) * enemy.Radius * 0.72f, p + Vector2.Right.Rotated(-rotation) * enemy.Radius * 0.72f, Alpha(accent, 0.42f), 1.7f, true);
            DrawCircle(p, enemy.Radius * 0.72f, Alpha(Paper, 0.1f), false, UiHairline, true);
        }
        else if (archetype == BossArchetype.Tempest)
        {
            for (int i = 0; i < 3; i++)
            {
                DrawArc(p, enemy.Radius * (0.58f + i * 0.16f), rotation + i * 1.2f, rotation + i * 1.2f + Mathf.Pi * 1.35f, 28, Alpha(accent, 0.34f), UiStroke, true);
            }
        }
        else if (archetype == BossArchetype.Bastion)
        {
            DrawDiamond(p, enemy.Radius * 0.72f, Alpha(accent, 0.54f), rotation);
            DrawLine(p - Vector2.Right * enemy.Radius * 0.64f, p + Vector2.Right * enemy.Radius * 0.64f, Alpha(Paper, 0.35f), 1.8f, true);
            DrawLine(p - Vector2.Down * enemy.Radius * 0.64f, p + Vector2.Down * enemy.Radius * 0.64f, Alpha(Paper, 0.28f), 1.8f, true);
        }
        else if (archetype == BossArchetype.Serpent)
        {
            int nodes = 5;
            Vector2 last = p;
            for (int i = 0; i < nodes; i++)
            {
                Vector2 node = p + new Vector2(Mathf.Sin(rotation + i * 0.82f) * enemy.Radius * 0.52f, (i - 2) * enemy.Radius * 0.18f);
                DrawLine(last, node, Alpha(accent, 0.28f), UiStroke, true);
                DrawCircle(node, enemy.Radius * 0.07f, Alpha(accent, 0.66f));
                last = node;
            }
        }
        else if (archetype == BossArchetype.Oracle)
        {
            DrawDiamond(p, enemy.Radius * 0.58f, Alpha(accent, 0.48f), rotation);
            DrawCircle(p, enemy.Radius * 0.18f, Alpha(Paper, 0.58f), false, UiStroke, true);
            DrawLine(p, p + (_playerPos - enemy.Pos).Normalized() * enemy.Radius * 0.78f, Alpha(EnemyBulletColor(), 0.38f), UiHairline, true);
        }
        else
        {
            DrawLine(p - Vector2.Right.Rotated(rotation) * enemy.Radius * 0.62f, p + Vector2.Right.Rotated(rotation) * enemy.Radius * 0.62f, Alpha(Paper, 0.45f), 1.8f, true);
            DrawLine(p - Vector2.Down.Rotated(rotation) * enemy.Radius * 0.62f, p + Vector2.Down.Rotated(rotation) * enemy.Radius * 0.62f, Alpha(Paper, 0.28f), 1.8f, true);
        }

    }

    private bool TryDrawBossTexture(Enemy enemy, Vector2 p, Color accent)
    {
        if (!_bossTextures.TryGetValue(enemy.BossArchetype, out Texture2D? texture) || texture == null)
        {
            return false;
        }

        Vector2 facing = EnemyFacingDirection(enemy);
        Rect2 sourceRegion = BossTextureRegion(enemy.BossArchetype, texture);
        Vector2 drawSize = BossTextureDrawSize(enemy, sourceRegion.Size);
        DrawFacingTexture(texture, sourceRegion, p, facing, drawSize, Alpha(Colors.White, 0.98f));
        return true;
    }

    private Rect2 BossTextureRegion(BossArchetype archetype, Texture2D texture)
    {
        if (_bossTextureRegions.TryGetValue(archetype, out Rect2 region) && region.Size.X > 0.0f && region.Size.Y > 0.0f)
        {
            return region;
        }

        return new Rect2(Vector2.Zero, texture.GetSize());
    }

    private static Vector2 BossTextureDrawSize(Enemy enemy, Vector2 sourceSize)
    {
        if (sourceSize.X <= 0.0f || sourceSize.Y <= 0.0f)
        {
            return Vector2.Zero;
        }

        float visualRoot = enemy.Radius * BossTextureRootScale * BossTextureScaleBias(enemy.BossArchetype);
        return FitTextureSizeToArea(sourceSize, visualRoot * visualRoot, enemy.Radius * BossTextureMinMaxSideScale, enemy.Radius * BossTextureMaxMaxSideScale);
    }

    private static float BossTextureScaleBias(BossArchetype archetype)
    {
        return archetype switch
        {
            BossArchetype.Choir => 0.94f,
            BossArchetype.Prism => 0.98f,
            BossArchetype.Swarm => 1.1f,
            BossArchetype.Forge => 1.08f,
            BossArchetype.Rift => 1.04f,
            BossArchetype.Mirror => 1.04f,
            BossArchetype.Tempest => 1.0f,
            BossArchetype.Bastion => 1.14f,
            BossArchetype.Serpent => 1.08f,
            BossArchetype.Oracle => 1.02f,
            _ => 1.0f,
        };
    }

    private void DrawBossTextureCues(Enemy enemy, Vector2 p, Color accent, float rotation)
    {
        if (enemy.BossGuard > 0.0f)
        {
            float guard = Mathf.Clamp(enemy.BossGuard / 1.2f, 0.0f, 1.0f);
            DrawCircle(p, enemy.Radius * (1.58f + guard * 0.18f), Alpha(Paper, 0.08f + guard * 0.12f), false, UiStroke, true);
            DrawArc(p, enemy.Radius * 1.7f, -Mathf.Pi * 0.5f, -Mathf.Pi * 0.5f + Mathf.Tau * guard, 48, Alpha(accent, 0.34f + guard * 0.28f), UiStroke, true);
        }

        if (enemy.BossIntentPulse <= 0.0f)
        {
            return;
        }

        float intent = Mathf.Clamp(enemy.BossIntentPulse / 1.45f, 0.0f, 1.0f);
        float ring = enemy.Radius * (1.62f + (1.0f - intent) * 0.28f);
        Color cue = Alpha(accent.Lerp(Paper, intent * 0.18f), 0.22f + intent * 0.34f);
        Vector2 aim = (_playerPos - enemy.Pos).LengthSquared() > 0.01f ? (_playerPos - enemy.Pos).Normalized() : Vector2.Down;
        DrawCircle(p, ring, cue, false, UiHairline, true);
        if (enemy.BossIntent is BossPatternKind.HeavyLance or BossPatternKind.OracleSnipe or BossPatternKind.HazardFan)
        {
            DrawLine(p, p + aim * ring * 1.18f, Alpha(EnemyBulletColor(), 0.22f + intent * 0.22f), UiStroke, true);
        }
        else if (enemy.BossIntent is BossPatternKind.SpiralRing or BossPatternKind.ReverseSpiral or BossPatternKind.TempestWheel or BossPatternKind.SerpentCoil)
        {
            DrawArc(p, ring * 0.92f, rotation, rotation + Mathf.Pi * 1.6f, 36, cue, UiStroke, true);
        }
        else if (enemy.BossIntent is BossPatternKind.SummonWing or BossPatternKind.WardenCall or BossPatternKind.MineDrift)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector2 node = p + Vector2.Right.Rotated(rotation + i * Mathf.Tau / 4.0f) * ring * 0.62f;
                DrawDiamond(node, enemy.Radius * 0.08f, cue, rotation);
            }
        }
    }

    private Vector2 EnemyFacingDirection(Enemy enemy)
    {
        if ((enemy.DashWarmup > 0.0f || enemy.DashTime > 0.0f) && enemy.DashDir.LengthSquared() > 0.01f)
        {
            return enemy.DashDir.Normalized();
        }

        Vector2 toPlayer = _playerPos - enemy.Pos;
        if (toPlayer.LengthSquared() > 0.01f)
        {
            return toPlayer.Normalized();
        }

        if (enemy.Vel.LengthSquared() > 0.01f)
        {
            return enemy.Vel.Normalized();
        }

        return Vector2.Down;
    }

    private static Vector2 FitTextureSizeToArea(Vector2 sourceSize, float targetArea, float minMaxSide, float maxMaxSide)
    {
        float area = Mathf.Max(1.0f, sourceSize.X * sourceSize.Y);
        Vector2 drawSize = sourceSize * Mathf.Sqrt(Mathf.Max(1.0f, targetArea) / area);
        float maxSide = Mathf.Max(drawSize.X, drawSize.Y);
        if (maxSide <= 0.0f)
        {
            return drawSize;
        }

        if (maxSide < minMaxSide)
        {
            drawSize *= minMaxSide / maxSide;
        }
        else if (maxSide > maxMaxSide)
        {
            drawSize *= maxMaxSide / maxSide;
        }

        return drawSize;
    }

    private void DrawFacingTexture(Texture2D texture, Rect2 sourceRegion, Vector2 center, Vector2 direction, Vector2 drawSize, Color tint)
    {
        if (sourceRegion.Size.X <= 0.0f || sourceRegion.Size.Y <= 0.0f || drawSize.X <= 0.0f || drawSize.Y <= 0.0f)
        {
            return;
        }

        Vector2 facing = direction.LengthSquared() > 0.01f ? direction.Normalized() : Vector2.Down;
        DrawSetTransform(center, facing.Angle() + Mathf.Pi * 0.5f, Vector2.One);
        DrawTextureRectRegion(texture, new Rect2(drawSize * -0.5f, drawSize), sourceRegion, tint);
        DrawSetTransform(Vector2.Zero, 0.0f, Vector2.One);
    }

    private static Vector2 DroneTextureDrawSize(Vector2 sourceSize, bool kairo)
    {
        float targetArea = kairo ? DroneTextureKairoTargetArea : DroneTextureSupportTargetArea;
        float maxSide = kairo ? DroneTextureMaxMaxSide : DroneTextureMaxMaxSide - 4.0f;
        return FitTextureSizeToArea(sourceSize, targetArea, DroneTextureMinMaxSide, maxSide);
    }

    private static Vector2 DroneCommandTextureDrawSize(Vector2 sourceSize, float scale)
    {
        Vector2 size = FitTextureSizeToArea(sourceSize, DroneCommandTextureTargetArea, DroneCommandTextureMinMaxSide, DroneCommandTextureMaxMaxSide);
        return size * scale;
    }

    private void DrawDroneCommandCue(DroneCommandCue cue)
    {
        float t = Mathf.Clamp(cue.Life / cue.MaxLife, 0.0f, 1.0f);
        float age = 1.0f - t;
        Vector2 pos = cue.Pos + ShakeOffset();
        Vector2 facing = SafeDirection(cue.Facing, Vector2.Up);
        float alpha = Mathf.Sin(t * Mathf.Pi) * 0.78f;

        if (_kairoCommandTexture != null && _kairoCommandRegion.Size.X > 0.0f && _kairoCommandRegion.Size.Y > 0.0f)
        {
            Vector2 drawSize = DroneCommandTextureDrawSize(_kairoCommandRegion.Size, cue.Scale * (0.82f + age * 0.34f));
            DrawFacingTexture(_kairoCommandTexture, _kairoCommandRegion, pos, facing, drawSize, Alpha(Colors.White, alpha));
            return;
        }

        float radius = (22.0f + age * 18.0f) * cue.Scale;
        DrawCircle(pos, radius, Alpha(cue.Color, alpha * 0.18f), false, UiHairline, true);
        DrawLine(pos - facing * radius * 0.55f, pos + facing * radius * 0.75f, Alpha(cue.Color, alpha * 0.34f), UiHairline, true);
    }

    private void DrawShot(Shot shot)
    {
        Color color = ShotVisualColor(shot);
        Vector2 pos = shot.Pos + ShakeOffset();
        if (shot.FromPlayer)
        {
            bool heavyVisualLoad = _visualPressure > 0.78f;
            if (TryDrawShotTexture(shot, pos, color, heavyVisualLoad))
            {
                return;
            }

            if (!heavyVisualLoad)
            {
                DrawCircle(pos, shot.Radius * 1.05f, Alpha(Paper, 0.78f));
                DrawCircle(pos, shot.Radius * 1.8f, Alpha(color, 0.32f), false, 1.0f, true);
            }
            else
            {
                DrawCircle(pos, shot.Radius * 0.92f, Alpha(color, 0.72f));
            }
        }
        else
        {
            bool heavyVisualLoad = _visualPressure > 0.68f;
            if (TryDrawShotTexture(shot, pos, color, heavyVisualLoad))
            {
                return;
            }

            if (!heavyVisualLoad)
            {
                DrawGlow(pos, color, shot.Radius * 4.0f, 0.08f, 3);
                DrawCircle(pos, shot.Radius * 1.82f, Alpha(Void, 0.74f));
            }
            DrawCircle(pos, shot.Radius * (heavyVisualLoad ? 1.22f : 1.55f), Alpha(color, heavyVisualLoad ? 0.92f : 0.86f), false, heavyVisualLoad ? 1.8f : 2.5f, true);
            DrawCircle(pos, shot.Radius * 0.88f, Alpha(Graphite, 0.92f));
            DrawCircle(pos, shot.Radius * 0.34f, Alpha(Paper, 0.92f));
        }
    }

    private bool TryDrawShotTexture(Shot shot, Vector2 pos, Color color, bool heavyVisualLoad)
    {
        Texture2D? texture;
        Rect2 sourceRegion;
        if (shot.FromPlayer)
        {
            if (!_fighterBulletTextures.TryGetValue(_runPilot, out texture) || texture == null)
            {
                return false;
            }

            sourceRegion = FighterBulletTextureRegion(_runPilot, texture);
        }
        else
        {
            texture = _enemyBulletTexture;
            if (texture == null || _enemyBulletRegion.Size.X <= 0.0f || _enemyBulletRegion.Size.Y <= 0.0f)
            {
                return false;
            }

            sourceRegion = _enemyBulletRegion;
        }

        Vector2 drawSize = ShotTextureDrawSize(shot, sourceRegion.Size);
        if (drawSize.X <= 0.0f || drawSize.Y <= 0.0f)
        {
            return false;
        }

        Vector2 direction = shot.Vel.LengthSquared() > 0.01f ? shot.Vel.Normalized() : Vector2.Up;
        if (shot.FromPlayer)
        {
            if (!heavyVisualLoad)
            {
                DrawCircle(pos, Mathf.Max(drawSize.X, drawSize.Y) * 0.46f, Alpha(color, shot.Rift ? 0.18f : 0.1f), false, UiHairline, true);
            }

            DrawFacingTexture(texture, sourceRegion, pos, direction, drawSize, Alpha(Colors.White, shot.Rift ? 0.96f : 0.92f));
            return true;
        }

        if (!heavyVisualLoad)
        {
            DrawCircle(pos, Mathf.Max(drawSize.X, drawSize.Y) * 0.42f, Alpha(EnemyBulletColor(), 0.16f), false, UiHairline, true);
        }

        DrawFacingTexture(texture, sourceRegion, pos, direction, drawSize, Alpha(EnemyBulletColor().Lerp(Colors.White, 0.18f), 0.98f));
        return true;
    }

    private Rect2 FighterBulletTextureRegion(PilotKind pilot, Texture2D texture)
    {
        if (_fighterBulletTextureRegions.TryGetValue(pilot, out Rect2 region) && region.Size.X > 0.0f && region.Size.Y > 0.0f)
        {
            return region;
        }

        return new Rect2(Vector2.Zero, texture.GetSize());
    }

    private static Vector2 ShotTextureDrawSize(Shot shot, Vector2 sourceSize)
    {
        if (sourceSize.X <= 0.0f || sourceSize.Y <= 0.0f)
        {
            return Vector2.Zero;
        }

        float visualRoot = Mathf.Max(shot.FromPlayer ? 14.0f : 17.0f, shot.Radius * (shot.FromPlayer ? 2.45f : 2.6f));
        float minSide = shot.FromPlayer ? Mathf.Clamp(shot.Radius * 2.05f, 14.0f, 24.0f) : Mathf.Clamp(shot.Radius * 2.15f, 18.0f, 26.0f);
        float maxSide = shot.FromPlayer ? Mathf.Clamp(shot.Radius * 3.1f, 18.0f, 34.0f) : Mathf.Clamp(shot.Radius * 3.0f, 20.0f, 32.0f);
        return FitTextureSizeToArea(sourceSize, visualRoot * visualRoot, minSide, maxSide);
    }

    private void DrawPickup(Pickup pickup)
    {
        Color accent = PickupColor(pickup.Kind);
        Vector2 pos = pickup.Pos + ShakeOffset();
        float pulse = 1.0f + Mathf.Sin(_time * 7.0f + pickup.Pos.X) * 0.08f;
        if (pickup.Kind == PickupKind.Dust)
        {
            float size = pickup.Radius * 1.95f * pulse;
            Rect2 shard = new(pos - new Vector2(size, size) * 0.5f, new Vector2(size, size));
            DrawRect(shard, Alpha(accent, 0.7f), true);
            DrawRect(shard, Alpha(Paper, 0.18f), false, UiHairline, true);
            return;
        }

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

    private void DrawShockwave(Shockwave wave)
    {
        float life01 = Mathf.Clamp(wave.Life / wave.MaxLife, 0.0f, 1.0f);
        float age01 = 1.0f - life01;
        float eased = 1.0f - Mathf.Pow(1.0f - age01, 2.6f);
        float radius = Mathf.Lerp(18.0f, wave.Radius, eased);
        Vector2 center = wave.Center + ShakeOffset();
        float alpha = Mathf.Sin(age01 * Mathf.Pi) * 0.34f + life01 * 0.18f;
        float lineWidth = Mathf.Lerp(5.2f, 1.0f, age01);

        DrawCircle(center, radius, Alpha(wave.Color, alpha * 0.12f), false, lineWidth * 4.0f, true);
        DrawCircle(center, radius, Alpha(wave.Color.Lerp(Paper, 0.18f), alpha), false, lineWidth, true);
        if (_visualPressure < 0.78f)
        {
            DrawCircle(center, radius * 0.72f, Alpha(Paper, life01 * 0.1f), false, UiHairline, true);
        }
    }

    private void DrawHazardField(HazardField field)
    {
        float age = field.MaxLife - field.Life;
        bool active = age > field.Warmup;
        float warm = Mathf.Clamp(age / Mathf.Max(0.01f, field.Warmup), 0.0f, 1.0f);
        float fade = Mathf.Clamp(field.Life / field.MaxLife, 0.0f, 1.0f);
        Vector2 center = field.Center + ShakeOffset();
        Color color = field.Color;
        float radius = active ? field.Radius : Mathf.Lerp(24.0f, field.Radius, warm);
        float alpha = active ? 0.16f * fade : 0.12f + warm * 0.08f;

        DrawCircle(center, radius, Alpha(color, alpha), false, active ? 2.0f : 1.2f + warm * 1.6f, true);
        DrawCircle(center, radius * 0.42f, Alpha(active ? EnemyBulletColor() : Paper, active ? 0.2f * fade : 0.12f + warm * 0.16f), false, UiHairline, true);
        DrawCircle(center, radius * 0.16f, Alpha(color.Lerp(Paper, 0.16f), active ? 0.22f * fade : 0.18f + warm * 0.22f));
        if (!active && _visualPressure < 0.82f)
        {
            for (int i = 0; i < 8; i++)
            {
                Vector2 dir = Vector2.Right.Rotated(_time * 0.9f + i * Mathf.Tau / 8.0f);
                DrawLine(center + dir * radius * 0.74f, center + dir * radius, Alpha(color, 0.12f + warm * 0.18f), UiHairline, true);
            }
        }
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
        Color polarity = PilotAccent(_runPilot);
        Rect2 top = new(new Vector2(74.0f, 16.0f), new Vector2(1772.0f, 62.0f));
        DrawGlow(top.Position + top.Size * 0.5f, CurrentSector().Accent, 230.0f, 0.026f, 4);
        DrawPanel(top, Alpha(Graphite, 0.82f), Alpha(GridLine, 0.34f));
        DrawLine(new Vector2(top.Position.X + 12.0f, top.End.Y - 1.0f), new Vector2(top.End.X - 12.0f, top.End.Y - 1.0f), Alpha(CurrentSector().Accent, 0.28f), UiHairline, true);

        Rect2 polarityCard = new(new Vector2(92.0f, 23.0f), new Vector2(220.0f, 48.0f));
        DrawPanel(polarityCard, Alpha(Ink, 0.46f), Alpha(polarity, 0.5f));
        DrawCircle(polarityCard.Position + new Vector2(25.0f, 20.0f), 11.0f, Alpha(Graphite, 0.95f));
        DrawCircle(polarityCard.Position + new Vector2(25.0f, 20.0f), 8.0f, Alpha(polarity, 0.9f), false, UiStroke, true);
        DrawText(TacticalSkillName(_runPilot), polarityCard.Position + new Vector2(46.0f, 22.0f), 15, Paper, HorizontalAlignment.Left, 165.0f, true, 2);
        string switchText = _polarityCooldown > 0.0f
            ? Tf("hud.resonance_cooldown", _polarityCooldown)
            : _assaultBurstTimer > 0.0f
                ? Tf("hud.assault_window", _assaultBurstTimer)
                : $"{T("hud.cruise_charge")} {Mathf.RoundToInt(CruiseCharge01() * 100.0f)}%";
        DrawText(switchText, polarityCard.Position + new Vector2(46.0f, 39.0f), 11, Alpha(_polarityCooldown <= 0.0f ? XpGreen : Paper, 0.68f), HorizontalAlignment.Left, 165.0f, false, 0);
        Rect2 polarityBar = new(polarityCard.Position + new Vector2(14.0f, 41.0f), new Vector2(polarityCard.Size.X - 28.0f, 3.0f));
        DrawRect(polarityBar, Alpha(Paper, 0.08f), true);
        float skillMeter = _assaultBurstTimer > 0.0f ? Mathf.Clamp(_assaultBurstTimer / AssaultBurstMax, 0.0f, 1.0f) : CruiseCharge01();
        float meter = _polarityCooldown > 0.0f ? PolaritySwitchReady01() : skillMeter;
        DrawRect(new Rect2(polarityBar.Position, new Vector2(polarityBar.Size.X * meter, polarityBar.Size.Y)), Alpha(polarity, _polarityCooldown <= 0.0f ? 0.9f : 0.58f), true);

        DrawBar(new Rect2(new Vector2(342.0f, 46.0f), new Vector2(280.0f, 12.0f)), _hudHullValue, _hudHullTrail, AlertRed, T("hud.hull"), $"{Mathf.CeilToInt(_playerHp)}/{Mathf.CeilToInt(_playerMaxHp)}");
        DrawBar(new Rect2(new Vector2(648.0f, 46.0f), new Vector2(280.0f, 12.0f)), _hudEnergyValue, _hudEnergyTrail, polarity, T("hud.energy"), $"{Mathf.FloorToInt(_energy)}/{Mathf.FloorToInt(_maxEnergy)}");
        float dashReady = Mathf.Clamp(1.0f - Mathf.Max(_dashCooldown, 0.0f) / 0.86f, 0.0f, 1.0f);
        DrawBar(new Rect2(new Vector2(954.0f, 46.0f), new Vector2(200.0f, 12.0f)), _hudDashValue, _hudDashTrail, XpGreen, T("hud.dash"), $"{Mathf.RoundToInt(dashReady * 100.0f)}%");

        DrawHudMetric(new Rect2(new Vector2(1184.0f, 26.0f), new Vector2(130.0f, 42.0f)), T("hud.sector.label"), $"{CurrentSectorIndex() + 1}/5", CurrentSector().Accent);
        DrawHudMetric(new Rect2(new Vector2(1338.0f, 26.0f), new Vector2(158.0f, 42.0f)), T("hud.route.label"), WavePaceShortText(_currentWavePace), CurrentSector().Accent);
        Color comboAccent = _comboTierPulse > 0.0f ? Gold.Lerp(Paper, 0.18f) : _combo > 8 ? XpGreen : Paper;
        DrawHudMetric(new Rect2(new Vector2(1522.0f, 26.0f), new Vector2(126.0f, 42.0f)), T("hud.combo.label"), Tf("combo.value", _combo), comboAccent);
        DrawSpawnMetric(new Rect2(new Vector2(1668.0f, 26.0f), new Vector2(116.0f, 42.0f)));
        DrawSettingsButton(HudSettingsButtonRect(), false);
        DrawBossHealthHud();
        DrawWaveIntelHud();
        DrawUpgradeIcons();
        DrawRunObjectives();
        DrawBottomExperienceBar();
        DrawTransientPolarityTip(polarity);
    }

    private void DrawWaveIntelHud()
    {
        if (_wave <= 0 || _currentWavePace == WavePaceKind.Boss)
        {
            return;
        }

        int sector = CurrentSectorIndex();
        int waveInSector = CurrentWaveInSector();
        EnemyKind primary = WavePrimaryEnemyKind(sector, waveInSector);
        Color accent = _waveIntelPulse > 0.0f ? CurrentSector().Accent.Lerp(Paper, _waveIntelPulse * 0.18f) : CurrentSector().Accent;
        Rect2 panel = new(new Vector2(594.0f, 82.0f), new Vector2(732.0f, 44.0f));
        int active = Math.Max(0, _enemies.Count);
        int reserve = WaveReserveEstimate();
        float progress = WaveProgress01();
        float cooldown = NextReserveSpawnProgress01();
        string batchText = reserve > 0 || _pendingSpawns.Count > 0
            ? Tf("wave.intel.batch", Math.Max(0.0f, _waveSpawnTimer), Math.Max(1, _waveNextSpawnCount))
            : T("wave.intel.complete");
        bool fast = SpawnRateMultiplier() > 1.65f;

        DrawPanel(panel, Alpha(Ink, 0.34f), Alpha(accent, 0.22f + _waveIntelPulse * 0.12f));
        DrawLine(panel.Position + new Vector2(116.0f, 9.0f), panel.Position + new Vector2(116.0f, panel.Size.Y - 9.0f), Alpha(Paper, 0.1f), UiHairline, true);
        DrawLine(panel.Position + new Vector2(364.0f, 9.0f), panel.Position + new Vector2(364.0f, panel.Size.Y - 9.0f), Alpha(Paper, 0.1f), UiHairline, true);
        DrawLine(panel.Position + new Vector2(586.0f, 9.0f), panel.Position + new Vector2(586.0f, panel.Size.Y - 9.0f), Alpha(Paper, 0.1f), UiHairline, true);

        DrawText(Tf("wave.intel.wave", _wave, TotalWaves), panel.Position + new Vector2(12.0f, 16.0f), 11, Alpha(Paper, 0.54f), HorizontalAlignment.Left, 92.0f, false, 0);
        DrawText(WavePaceShortText(_currentWavePace), panel.Position + new Vector2(12.0f, 34.0f), 13, Alpha(accent, 0.84f), HorizontalAlignment.Left, 92.0f, true, 1);

        DrawText(Tf("wave.intel.primary", EnemyName(primary)), panel.Position + new Vector2(132.0f, 28.0f), 14, Alpha(Paper, 0.78f), HorizontalAlignment.Left, 218.0f, true, 1);
        DrawText(batchText, panel.Position + new Vector2(380.0f, 18.0f), 12, Alpha(fast ? Gold : accent, 0.82f), HorizontalAlignment.Left, 188.0f, true, 1);
        DrawText(SpawnSpeedText(), panel.Position + new Vector2(498.0f, 35.0f), 11, Alpha(fast ? Gold : Paper, 0.62f), HorizontalAlignment.Right, 70.0f, false, 0);

        Rect2 cooldownBar = new(panel.Position + new Vector2(380.0f, 28.0f), new Vector2(188.0f, 4.0f));
        DrawRect(cooldownBar, Alpha(Paper, 0.08f), true);
        DrawRect(new Rect2(cooldownBar.Position, new Vector2(cooldownBar.Size.X * cooldown, cooldownBar.Size.Y)), Alpha(fast ? Gold : accent, 0.72f + _comboTierPulse * 0.16f), true);

        DrawText(Tf("wave.intel.progress_short", Mathf.RoundToInt(progress * 100.0f), active), panel.Position + new Vector2(panel.Size.X - 132.0f, 18.0f), 11, Alpha(Paper, 0.58f), HorizontalAlignment.Right, 116.0f, false, 0);
        DrawText(Tf("wave.intel.reserve", reserve), panel.Position + new Vector2(panel.Size.X - 132.0f, 35.0f), 11, Alpha(Paper, 0.43f), HorizontalAlignment.Right, 116.0f, false, 0);

        Rect2 bar = new(panel.Position + new Vector2(12.0f, panel.Size.Y - 4.0f), new Vector2(panel.Size.X - 24.0f, 2.0f));
        DrawRect(bar, Alpha(Paper, 0.08f), true);
        DrawRect(new Rect2(bar.Position, new Vector2(bar.Size.X * progress, bar.Size.Y)), Alpha(accent, 0.68f), true);
    }

    private void DrawBossHealthHud()
    {
        Enemy? boss = ActiveBoss();
        if (boss == null)
        {
            return;
        }

        Color accent = BossAccent(boss.BossArchetype);
        float hp = SafeRatio(boss.Hp, boss.MaxHp);
        Rect2 panel = new(new Vector2(520.0f, 88.0f), new Vector2(880.0f, 60.0f));
        Rect2 bar = new(panel.Position + new Vector2(24.0f, 31.0f), new Vector2(panel.Size.X - 48.0f, 9.0f));
        DrawGlow(panel.Position + panel.Size * 0.5f, accent, 260.0f, 0.035f, 5);
        DrawPanel(panel, Alpha(Ink, 0.58f), Alpha(accent, 0.5f));
        DrawText(BossTitle(boss.BossArchetype, CurrentSectorIndex()).ToUpperInvariant(), panel.Position + new Vector2(24.0f, 22.0f), 17, Alpha(Paper, 0.82f), HorizontalAlignment.Left, 430.0f, true, 2);
        DrawText(Tf("boss.phase_hud", boss.BossPhase + 1, BossPhaseThresholds.Length + 1), panel.Position + new Vector2(panel.Size.X - 360.0f, 22.0f), 13, Alpha(accent, 0.78f), HorizontalAlignment.Right, 132.0f, true, 1);
        DrawText($"{Mathf.CeilToInt(Mathf.Max(0.0f, boss.Hp))}/{Mathf.CeilToInt(boss.MaxHp)}", panel.Position + new Vector2(panel.Size.X - 212.0f, 22.0f), 15, Alpha(accent, 0.86f), HorizontalAlignment.Right, 188.0f, true, 1);
        DrawRect(bar.Grow(2.0f), Alpha(Paper, 0.08f), true);
        DrawRect(bar, Alpha(Paper, 0.14f), true);
        DrawRect(new Rect2(bar.Position, new Vector2(bar.Size.X * hp, bar.Size.Y)), Alpha(accent, 0.9f), true);
        if (boss.BossGuard > 0.0f)
        {
            float guard = Mathf.Clamp(boss.BossGuard / 1.4f, 0.0f, 1.0f);
            DrawRect(new Rect2(bar.Position, new Vector2(bar.Size.X * guard, 2.0f)), Alpha(Paper, 0.62f), true);
            DrawText(Tf("boss.guard_hud", Mathf.RoundToInt(guard * 100.0f)), panel.Position + new Vector2(24.0f, 49.0f), 10, Alpha(Paper, 0.5f), HorizontalAlignment.Left, 180.0f, false, 0);
        }
        else
        {
            DrawText(BossNextPhaseText(boss), panel.Position + new Vector2(24.0f, 49.0f), 10, Alpha(Paper, 0.38f), HorizontalAlignment.Left, 260.0f, false, 0);
        }
        if (boss.BossIntentPulse > 0.0f || boss.BossLastPattern >= 0)
        {
            float intentPulse = Mathf.Clamp(boss.BossIntentPulse / 1.45f, 0.0f, 1.0f);
            Color intentColor = intentPulse > 0.0f ? accent.Lerp(Paper, intentPulse * 0.2f) : Paper;
            DrawText(Tf("boss.intent_hud", BossPatternText(boss.BossIntent)).ToUpperInvariant(), panel.Position + new Vector2(panel.Size.X - 330.0f, 51.0f), 11, Alpha(intentColor, 0.44f + intentPulse * 0.34f), HorizontalAlignment.Right, 306.0f, false, 0);
        }
        for (int i = 0; i < BossPhaseThresholds.Length; i++)
        {
            float marker = BossPhaseThresholds[i];
            float x = bar.Position.X + bar.Size.X * marker;
            DrawLine(new Vector2(x, bar.Position.Y - 4.0f), new Vector2(x, bar.End.Y + 4.0f), Alpha(Paper, i < boss.BossPhase ? 0.22f : 0.58f), UiHairline, true);
        }
        DrawRect(bar, Alpha(Paper, 0.48f), false, UiHairline, true);
    }

    private void DrawTransientPolarityTip(Color current)
    {
        if (_polarityTipTimer <= 0.0f)
        {
            return;
        }

        float a = Mathf.Clamp(_polarityTipTimer / 2.4f, 0.0f, 1.0f);
        Rect2 tip = new(new Vector2(702.0f, ActiveBoss() == null ? 88.0f : 148.0f), new Vector2(516.0f, 36.0f));
        DrawPanel(tip, Alpha(Ink, 0.42f * a), Alpha(current, 0.3f + a * 0.34f));
        DrawText(TacticalTipText(), tip.Position + new Vector2(0.0f, 25.0f), 17, Alpha(current, 0.42f + a * 0.48f), HorizontalAlignment.Center, tip.Size.X, true, 2);
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
        bool capstone = IsUpgradeMaxed(id) && CapstoneBody(id).Length > 0;
        if (capstone)
        {
            DrawGlow(drawRect.Position + drawRect.Size * 0.5f, accent, hover ? 58.0f : 42.0f, hover ? 0.05f : 0.032f, 3);
        }
        DrawPanel(drawRect, Alpha(Graphite, hover ? 0.76f : 0.58f), Alpha(accent, hover ? 0.72f : 0.42f));
        if (capstone)
        {
            Vector2 center = drawRect.Position + drawRect.Size * 0.5f;
            DrawPolyline(ClosePolygon(RegularPolygon(center, drawRect.Size.X * 0.64f, 4, Mathf.Pi * 0.25f)), Alpha(accent, hover ? 0.9f : 0.64f), UiHairline, true);
            DrawLine(center - Vector2.Right * drawRect.Size.X * 0.22f, center + Vector2.Right * drawRect.Size.X * 0.22f, Alpha(Paper, 0.42f), UiHairline, true);
            DrawLine(center - Vector2.Down * drawRect.Size.X * 0.22f, center + Vector2.Down * drawRect.Size.X * 0.22f, Alpha(Paper, 0.42f), UiHairline, true);
        }
        DrawUpgradeGlyph(id, drawRect.Position + drawRect.Size * 0.5f, drawRect.Size.X * 0.42f, accent);

        Vector2 badge = drawRect.End - new Vector2(7.0f, 7.0f);
        DrawCircle(badge, 8.0f, Alpha(Ink, 0.92f));
        DrawCircle(badge, 8.0f, Alpha(accent, capstone ? 0.96f : 0.78f), false, UiStroke, true);
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
            case UpgradeId.PulseMagazine:
            case UpgradeId.LyraResonanceChord:
            case UpgradeId.LyraTempoBloom:
                for (int i = -1; i <= 1; i++)
                {
                    DrawLine(center + new Vector2(-radius * 0.75f, i * radius * 0.34f), center + new Vector2(radius * 0.75f, i * radius * 0.34f), line, UiStroke, true);
                }
                break;
            case UpgradeId.KineticBloom:
            case UpgradeId.CometTrail:
            case UpgradeId.SolCoronaBloom:
            case UpgradeId.SolFlareCore:
            case UpgradeId.OrionPerihelionVector:
                DrawLine(center + new Vector2(-radius * 0.6f, -radius * 0.5f), center + new Vector2(radius * 0.15f, 0.0f), line, UiStroke, true);
                DrawLine(center + new Vector2(-radius * 0.6f, radius * 0.5f), center + new Vector2(radius * 0.15f, 0.0f), line, UiStroke, true);
                DrawLine(center + new Vector2(0.0f, -radius * 0.5f), center + new Vector2(radius * 0.72f, 0.0f), Alpha(Paper, 0.6f), UiHairline, true);
                DrawLine(center + new Vector2(0.0f, radius * 0.5f), center + new Vector2(radius * 0.72f, 0.0f), Alpha(Paper, 0.6f), UiHairline, true);
                break;
            case UpgradeId.VitalShell:
            case UpgradeId.MirrorSkin:
            case UpgradeId.AegisBloom:
            case UpgradeId.RookBulwarkCore:
            case UpgradeId.RookAegisRelay:
            case UpgradeId.RookCitadelProtocol:
                DrawDiamond(center, radius * 0.72f, accent, 0.0f);
                DrawLine(center - Vector2.Right * radius * 0.45f, center + Vector2.Right * radius * 0.45f, Alpha(Paper, 0.65f), UiHairline, true);
                break;
            case UpgradeId.GravityWell:
            case UpgradeId.ResonanceLeech:
            case UpgradeId.ChainRelay:
            case UpgradeId.KairoSwarmSync:
            case UpgradeId.KairoRelayProtocol:
            case UpgradeId.StasisField:
            case UpgradeId.MagnetizedCore:
            case UpgradeId.NyxSingularity:
            case UpgradeId.NyxEventHorizon:
            case UpgradeId.NyxGravityCantor:
            case UpgradeId.LyraHarmonicCascade:
            case UpgradeId.LyraEncoreField:
                DrawCircle(center, radius * 0.72f, soft);
                DrawCircle(center, radius * 0.72f, line, false, UiStroke, true);
                DrawCircle(center, radius * 0.22f, Alpha(Paper, 0.74f));
                if (id == UpgradeId.ChainRelay)
                {
                    DrawLine(center - new Vector2(radius * 0.62f, radius * 0.36f), center, Alpha(Paper, 0.62f), UiHairline, true);
                    DrawLine(center, center + new Vector2(radius * 0.62f, -radius * 0.36f), Alpha(Paper, 0.62f), UiHairline, true);
                }
                break;
            case UpgradeId.MoonWisp:
            case UpgradeId.QuantumEcho:
            case UpgradeId.PolarityStorm:
            case UpgradeId.KairoDroneBay:
            case UpgradeId.KairoOverrideMatrix:
            case UpgradeId.NyxOrbit:
            case UpgradeId.RicochetMatrix:
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
            case UpgradeId.ExecutionMark:
            case UpgradeId.RookSiegeBattery:
            case UpgradeId.OrionCometSpear:
            case UpgradeId.OrionDeadeyeMark:
            case UpgradeId.OrionStarfallQuiver:
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
            case UpgradeId.FractalSplit:
                DrawDiamond(center, radius * 0.68f, accent, Mathf.Pi * 0.25f);
                DrawCircle(center, radius * 0.18f, Alpha(Paper, 0.72f));
                if (id == UpgradeId.FractalSplit)
                {
                    DrawDiamond(center + new Vector2(radius * 0.48f, -radius * 0.34f), radius * 0.22f, Alpha(Paper, 0.72f), 0.0f);
                    DrawDiamond(center - new Vector2(radius * 0.48f, -radius * 0.3f), radius * 0.18f, Alpha(Paper, 0.56f), 0.0f);
                }
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

        float rowHeight = 57.0f;
        Rect2 panel = new(new Vector2(1360.0f, 92.0f), new Vector2(464.0f, 56.0f + _runObjectives.Count * rowHeight));
        Color panelAccent = DifficultyAccent(_selectedDifficulty).Lerp(CurrentSector().Accent, 0.18f);
        DrawPanel(panel, Alpha(Ink, 0.42f), Alpha(panelAccent, 0.24f));
        DrawText(T("hud.objectives").ToUpperInvariant(), panel.Position + new Vector2(16.0f, 23.0f), 12, Alpha(Paper, 0.62f), HorizontalAlignment.Left, 214.0f, false, 0);
        int visibleBonusDust = _runObjectiveBonusDust + _runScoreBonusDust;
        string bonusText = Tf("objective.bonus", visibleBonusDust);
        DrawText(bonusText, panel.Position + new Vector2(panel.Size.X - 142.0f, 23.0f), 12, Alpha(Gold, 0.72f), HorizontalAlignment.Right, 124.0f, false, 0);
        DrawLine(panel.Position + new Vector2(14.0f, 33.0f), panel.Position + new Vector2(panel.Size.X - 14.0f, 33.0f), Alpha(panelAccent, 0.18f), UiHairline, true);

        for (int i = 0; i < _runObjectives.Count; i++)
        {
            RunObjective objective = _runObjectives[i];
            float progress = objective.Target <= 0 ? 1.0f : Mathf.Clamp(objective.Progress / (float)objective.Target, 0.0f, 1.0f);
            Color accent = ObjectiveAccent(objective);
            Rect2 row = new(panel.Position + new Vector2(12.0f, 42.0f + i * rowHeight), new Vector2(panel.Size.X - 24.0f, 48.0f));
            Rect2 marker = new(row.Position + new Vector2(15.0f, 17.0f), new Vector2(10.0f, 10.0f));
            Rect2 bar = new(row.Position + new Vector2(34.0f, row.Size.Y - 7.0f), new Vector2(row.Size.X - 50.0f, 3.0f));
            DrawRect(row, Alpha(Graphite, objective.Completed ? 0.34f : 0.2f), true);
            DrawLine(row.Position + new Vector2(0.0f, 7.0f), row.Position + new Vector2(0.0f, row.Size.Y - 7.0f), Alpha(accent, objective.Completed ? 0.82f : 0.5f), UiHairline, true);
            DrawRect(marker, Alpha(accent, objective.Completed ? 0.28f : 0.12f), true);
            DrawRect(marker, Alpha(accent, objective.Completed ? 0.72f : 0.42f), false, UiHairline, true);
            DrawText(ObjectiveTitle(objective), row.Position + new Vector2(34.0f, 18.0f), 12, Alpha(accent, objective.Completed ? 0.88f : 0.78f), HorizontalAlignment.Left, row.Size.X - 144.0f, true, 1);
            DrawText(ObjectiveBody(objective), row.Position + new Vector2(34.0f, 36.0f), 11, objective.Completed ? Alpha(Jade, 0.72f) : Alpha(Paper, 0.55f), HorizontalAlignment.Left, row.Size.X - 142.0f, false, 0);
            DrawText(ObjectiveProgressText(objective), row.Position + new Vector2(row.Size.X - 102.0f, 20.0f), 13, Alpha(accent, objective.Completed ? 0.9f : 0.8f), HorizontalAlignment.Right, 90.0f, false, 0);
            DrawRect(bar, Alpha(Paper, 0.08f), true);
            DrawRect(new Rect2(bar.Position, new Vector2(bar.Size.X * progress, bar.Size.Y)), Alpha(accent, objective.Completed ? 0.86f : 0.62f), true);
        }
    }

    private void DrawTitle()
    {
        float pulse = 0.55f + 0.45f * Mathf.Sin(_time * 3.2f);
        DrawTitleLogo(pulse);
        DrawTitleStartButton(StartButtonRect(), pulse);

        DrawText(Tf("meta.wallet", _starDust), new Vector2(56.0f, 70.0f), 18, Alpha(Gold, 0.62f), HorizontalAlignment.Left, 340.0f, false, 0);
        DrawText(Tf("meta.best", _bestWave, _bestScore, _runsCompleted), new Vector2(56.0f, 100.0f), 15, Alpha(Paper, 0.36f), HorizontalAlignment.Left, 760.0f, false, 0);
        DrawDifficultySelector();
        DrawNextGoalPanel(TitleNextGoalRect(), true);
        DrawTitleLeaderboard();
        DrawPilotSelect();

        DrawTitleTextButton(MetaButtonRect(), T("menu.meta"), Gold);
        DrawTitleTextButton(TitleSettingsButtonRect(), T("menu.settings"), Alpha(Paper, 0.68f));
        DrawGmUnlockButton();

        if (_wonOnce)
        {
            DrawText(T("title.won_once"), new Vector2(0.0f, 1006.0f), 16, Alpha(Gold, 0.54f), HorizontalAlignment.Center, ScreenWidth, false, 0);
        }
    }

    private void DrawTitleLogo(float pulse)
    {
        Vector2 center = new(ScreenWidth * 0.5f, 188.0f);
        Color cyan = new Color(0.28f, 0.88f, 1.0f);
        Color amber = new Color(1.0f, 0.62f, 0.18f);
        Color logoWhite = Alpha(Paper, 0.72f + pulse * 0.08f);

        DrawGlow(center + new Vector2(0.0f, 18.0f), cyan, 340.0f, 0.022f + pulse * 0.014f, 6);
        DrawGlow(center + new Vector2(0.0f, 92.0f), amber, 180.0f, 0.014f + pulse * 0.01f, 4);

        for (int i = 0; i < 4; i++)
        {
            float radius = 114.0f + i * 34.0f;
            float spin = _time * (0.055f + i * 0.012f) + i * 0.72f;
            Color arcColor = i % 2 == 0 ? cyan : amber;
            DrawArc(center, radius, spin, spin + Mathf.Pi * (1.22f - i * 0.08f), 96, Alpha(arcColor, 0.13f - i * 0.018f), UiHairline, true);
            DrawArc(center, radius + 8.0f, spin + Mathf.Pi * 1.18f, spin + Mathf.Pi * 1.68f, 64, Alpha(Paper, 0.05f - i * 0.006f), UiHairline, true);
        }

        Texture2D? logo = CurrentTitleLogo();
        if (logo != null)
        {
            DrawTitleLogoTexture(logo, center, pulse);
            return;
        }

        DrawTitleFighterMark(center + new Vector2(0.0f, 86.0f), pulse, cyan, amber);

        string title = TitleName().ToUpperInvariant();
        int titleSize = TitleFontSize();
        Vector2 titlePos = new(0.0f, 200.0f);
        DrawText(title, titlePos + new Vector2(3.0f, 5.0f), titleSize, Alpha(cyan, 0.12f), HorizontalAlignment.Center, ScreenWidth, false, 0);
        DrawText(title, titlePos + new Vector2(-3.0f, -2.0f), titleSize, Alpha(amber, 0.1f), HorizontalAlignment.Center, ScreenWidth, false, 0);
        DrawText(title, titlePos, titleSize, logoWhite, HorizontalAlignment.Center, ScreenWidth, true, 5);
    }

    private void DrawTitleLogoTexture(Texture2D logo, Vector2 center, float pulse)
    {
        Vector2 sourceSize = logo.GetSize();
        if (sourceSize.X <= 1.0f || sourceSize.Y <= 1.0f)
        {
            return;
        }

        const float maxWidth = 660.0f;
        const float maxHeight = 330.0f;
        float scale = Math.Min(maxWidth / sourceSize.X, maxHeight / sourceSize.Y);
        Vector2 drawSize = sourceSize * scale;
        Rect2 rect = new(center - drawSize * 0.5f + new Vector2(0.0f, 18.0f), drawSize);
        Vector2 logoCenter = rect.Position + rect.Size * 0.5f;

        DrawTextureRect(logo, new Rect2(rect.Position + new Vector2(0.0f, 8.0f), rect.Size), false, Alpha(Void, 0.34f));
        DrawGlow(logoCenter + new Vector2(-110.0f, 18.0f), Cyan, 245.0f, 0.014f + pulse * 0.006f, 4);
        DrawGlow(logoCenter + new Vector2(84.0f, 42.0f), Gold, 210.0f, 0.012f + pulse * 0.005f, 4);
        DrawGlow(logoCenter + new Vector2(190.0f, 34.0f), Rose, 150.0f, 0.008f + pulse * 0.004f, 3);
        DrawLine(rect.Position + new Vector2(drawSize.X * 0.04f, drawSize.Y * 0.72f), rect.Position + new Vector2(drawSize.X * 0.22f, drawSize.Y * 0.72f), Alpha(Cyan, 0.18f + pulse * 0.1f), UiHairline, true);
        DrawLine(rect.Position + new Vector2(drawSize.X * 0.78f, drawSize.Y * 0.72f), rect.Position + new Vector2(drawSize.X * 0.96f, drawSize.Y * 0.72f), Alpha(Rose, 0.16f + pulse * 0.08f), UiHairline, true);
        DrawLine(rect.Position + new Vector2(drawSize.X * 0.36f, drawSize.Y * 0.9f), rect.Position + new Vector2(drawSize.X * 0.64f, drawSize.Y * 0.9f), Alpha(Gold, 0.2f + pulse * 0.1f), UiHairline, true);
        DrawTextureRect(logo, rect, false, Alpha(Colors.White, 0.9f + pulse * 0.08f));
    }

    private void DrawTitleStartButton(Rect2 rect, float pulse)
    {
        bool hover = rect.HasPoint(GetGlobalMousePosition()) || IsGamepadFocused(rect);
        Color accent = hover ? Gold.Lerp(Paper, 0.22f) : Gold;
        float glow = hover ? 0.048f : 0.026f + pulse * 0.008f;
        Rect2 drawRect = hover ? rect.Grow(3.0f) : rect;

        DrawGlow(drawRect.Position + drawRect.Size * 0.5f, accent, hover ? 128.0f : 86.0f, glow, 4);
        DrawPanel(drawRect, Alpha(Ink, hover ? 0.7f : 0.48f), Alpha(accent, hover ? 0.82f : 0.42f));
        DrawRect(drawRect.Grow(-5.0f), Alpha(accent, 0.05f + pulse * 0.025f), false, UiHairline, true);
        DrawLine(drawRect.Position + new Vector2(18.0f, drawRect.Size.Y - 7.0f), drawRect.End - new Vector2(18.0f, 7.0f), Alpha(accent, 0.4f + pulse * 0.18f), UiHairline, true);
        DrawText(T("menu.start"), drawRect.Position + new Vector2(0.0f, drawRect.Size.Y * 0.68f), 20, Alpha(Paper, hover ? 0.94f : 0.76f), HorizontalAlignment.Center, drawRect.Size.X, true, 2);
    }

    private void DrawDifficultySelector()
    {
        Vector2 mouse = GetGlobalMousePosition();
        DrawText(T("difficulty.title"), new Vector2(0.0f, 384.0f), 13, Alpha(Paper, 0.38f), HorizontalAlignment.Center, ScreenWidth, true, 0);
        for (int i = 0; i < DifficultyCount; i++)
        {
            GameDifficulty difficulty = DifficultyFromIndex(i);
            Rect2 rect = DifficultyButtonRect(i);
            bool unlocked = IsDifficultyUnlocked(difficulty);
            bool selected = _selectedDifficulty == difficulty;
            bool hover = rect.HasPoint(mouse) || IsGamepadFocused(rect);
            Color accent = DifficultyAccent(difficulty);
            Color line = Alpha(accent, unlocked ? selected ? 0.78f : hover ? 0.48f : 0.24f : 0.1f);
            Color fill = Alpha(Ink, selected ? 0.58f : hover && unlocked ? 0.36f : 0.22f);

            DrawPanel(rect, fill, line);
            if (selected)
            {
                DrawLine(rect.Position + new Vector2(14.0f, rect.Size.Y - 5.0f), rect.End - new Vector2(14.0f, 5.0f), Alpha(accent, 0.58f), UiHairline, true);
            }

            string label = DifficultyName(difficulty);
            Color textColor = unlocked ? Alpha(selected ? Paper : accent.Lerp(Paper, 0.34f), selected ? 0.86f : 0.66f) : Alpha(Steel, 0.48f);
            DrawText(label, rect.Position + new Vector2(0.0f, 23.0f), 14, textColor, HorizontalAlignment.Center, rect.Size.X, true, 0);
            if (!unlocked)
            {
                DrawText(T("ui.lock"), rect.Position + new Vector2(rect.Size.X - 52.0f, 21.0f), 10, Alpha(Rose, 0.54f), HorizontalAlignment.Center, 44.0f, false, 0);
            }
        }
    }

    private void DrawTitleFighterMark(Vector2 center, float pulse, Color cyan, Color amber)
    {
        float scale = 1.0f + pulse * 0.035f;
        Vector2 nose = center + new Vector2(0.0f, -34.0f) * scale;
        Vector2 leftWing = center + new Vector2(-34.0f, 18.0f) * scale;
        Vector2 rightWing = center + new Vector2(34.0f, 18.0f) * scale;
        Vector2 tail = center + new Vector2(0.0f, 34.0f) * scale;
        Vector2 leftInner = center + new Vector2(-11.0f, 12.0f) * scale;
        Vector2 rightInner = center + new Vector2(11.0f, 12.0f) * scale;

        DrawCircle(center, 48.0f * scale, Alpha(cyan, 0.035f + pulse * 0.02f));
        DrawArc(center, 50.0f * scale, -Mathf.Pi * 0.18f + _time * 0.22f, Mathf.Pi * 1.42f + _time * 0.22f, 72, Alpha(cyan, 0.32f), UiHairline, true);
        DrawArc(center, 63.0f * scale, Mathf.Pi * 0.72f - _time * 0.16f, Mathf.Pi * 1.9f - _time * 0.16f, 72, Alpha(amber, 0.22f), UiHairline, true);

        DrawLine(nose, leftWing, Alpha(Paper, 0.76f), UiStroke, true);
        DrawLine(nose, rightWing, Alpha(Paper, 0.76f), UiStroke, true);
        DrawLine(leftWing, tail, Alpha(cyan, 0.48f), UiHairline, true);
        DrawLine(rightWing, tail, Alpha(cyan, 0.48f), UiHairline, true);
        DrawLine(tail, nose, Alpha(Paper, 0.46f), UiHairline, true);
        DrawLine(leftInner, tail, Alpha(amber, 0.42f), UiHairline, true);
        DrawLine(rightInner, tail, Alpha(amber, 0.42f), UiHairline, true);
        DrawCircle(center, 5.5f + pulse * 1.4f, Alpha(amber, 0.72f));
    }

    private void DrawTitleLeaderboard()
    {
        Rect2 panel = LeaderboardPanelRect();
        Color accent = DifficultyAccent(_selectedDifficulty);
        DrawGlow(panel.Position + panel.Size * 0.5f, accent, 180.0f, 0.014f, 3);
        DrawPanel(panel, Alpha(Ink, 0.34f), Alpha(accent, 0.26f));
        DrawLine(panel.Position + new Vector2(18.0f, 44.0f), panel.Position + new Vector2(panel.Size.X - 18.0f, 44.0f), Alpha(accent, 0.22f), UiHairline, true);
        DrawText(T("leader.title"), panel.Position + new Vector2(22.0f, 27.0f), 14, Alpha(Paper, 0.7f), HorizontalAlignment.Left, panel.Size.X - 44.0f, true, 1);
        DrawText(DifficultyName(_selectedDifficulty), panel.Position + new Vector2(panel.Size.X - 146.0f, 27.0f), 13, Alpha(accent, 0.72f), HorizontalAlignment.Right, 124.0f, false, 0);

        var rows = LeaderboardRows();
        for (int i = 0; i < rows.Length; i++)
        {
            var rowData = rows[i];
            Rect2 row = new(panel.Position + new Vector2(26.0f, 60.0f + i * 29.0f), new Vector2(panel.Size.X - 52.0f, 25.0f));
            DrawLine(row.Position + new Vector2(0.0f, row.Size.Y - 1.0f), row.Position + new Vector2(row.Size.X, row.Size.Y - 1.0f), Alpha(Paper, 0.06f), UiHairline, true);
            DrawText(rowData.Label, row.Position + new Vector2(0.0f, 18.0f), 12, Alpha(rowData.Accent, 0.64f), HorizontalAlignment.Left, 150.0f, true, 1);
            DrawText(rowData.Value, row.Position + new Vector2(row.Size.X - 190.0f, 18.0f), 13, Alpha(rowData.Accent, 0.76f), HorizontalAlignment.Right, 190.0f, true, 1);
        }
    }

    private (string Label, string Value, Color Accent)[] LeaderboardRows()
    {
        (string Label, string Value, Color Accent)[] rows = new (string Label, string Value, Color Accent)[ClearRecordCount];
        List<float> records = ClearTimeRecords(_selectedDifficulty);
        for (int i = 0; i < rows.Length; i++)
        {
            bool hasRecord = i < records.Count;
            rows[i] = (
                Tf("leader.rank", i + 1),
                hasRecord ? FormatRecordTime(records[i]) : T("leader.no_record"),
                LeaderboardRankAccent(i, hasRecord)
            );
        }

        return rows;
    }

    private void DrawNextGoalPanel(Rect2 panel, bool compact)
    {
        Color accent = NextGoalAccent();
        DrawGlow(panel.Position + panel.Size * 0.5f, accent, compact ? 150.0f : 190.0f, compact ? 0.018f : 0.026f, compact ? 3 : 4);
        DrawPanel(panel, Alpha(Ink, compact ? 0.28f : 0.46f), Alpha(accent, compact ? 0.24f : 0.36f));
        DrawLine(panel.Position + new Vector2(16.0f, panel.Size.Y - 8.0f), panel.Position + new Vector2(panel.Size.X - 16.0f, panel.Size.Y - 8.0f), Alpha(accent, 0.24f), UiHairline, true);
        DrawText(T("goal.title").ToUpperInvariant(), panel.Position + new Vector2(18.0f, compact ? 25.0f : 29.0f), compact ? 12 : 13, Alpha(accent, 0.72f), HorizontalAlignment.Left, 132.0f, false, 0);
        DrawText(NextGoalText(), panel.Position + new Vector2(154.0f, compact ? 26.0f : 31.0f), compact ? 16 : 18, Alpha(Paper, compact ? 0.66f : 0.74f), HorizontalAlignment.Left, panel.Size.X - 176.0f, true, compact ? 1 : 2);
    }

    private string NextGoalText()
    {
        PilotKind? lockedPilot = NextLockedPilot();
        if (lockedPilot.HasValue)
        {
            PilotKind previous = PreviousPilot(lockedPilot.Value);
            return Tf("goal.unlock_pilot", PilotName(previous), PilotName(lockedPilot.Value));
        }

        if (!_wonOnce)
        {
            return T("goal.clear_40");
        }

        GameDifficulty? lockedDifficulty = NextLockedDifficulty();
        if (lockedDifficulty.HasValue)
        {
            return DifficultyUnlockText(lockedDifficulty.Value);
        }

        List<float> records = ClearTimeRecords(_selectedDifficulty);
        if (records.Count > 0)
        {
            return Tf("goal.beat_record", FormatRecordTime(records[0]));
        }

        return T("goal.set_record");
    }

    private Color NextGoalAccent()
    {
        PilotKind? lockedPilot = NextLockedPilot();
        if (lockedPilot.HasValue)
        {
            return PilotAccent(lockedPilot.Value);
        }

        GameDifficulty? lockedDifficulty = NextLockedDifficulty();
        if (lockedDifficulty.HasValue)
        {
            return DifficultyAccent(lockedDifficulty.Value);
        }

        return _wonOnce ? Gold : Jade;
    }

    private PilotKind? NextLockedPilot()
    {
        for (int i = 0; i < PilotCount(); i++)
        {
            PilotKind pilot = PilotFromIndex(i);
            if (!IsPilotUnlocked(pilot))
            {
                return pilot;
            }
        }

        return null;
    }

    private GameDifficulty? NextLockedDifficulty()
    {
        for (int i = 0; i < DifficultyCount; i++)
        {
            GameDifficulty difficulty = DifficultyFromIndex(i);
            if (!IsDifficultyUnlocked(difficulty))
            {
                return difficulty;
            }
        }

        return null;
    }

    private static PilotKind? NextPilot(PilotKind pilot)
    {
        int index = PilotIndex(pilot);
        if (index < 0 || index >= PilotCount() - 1)
        {
            return null;
        }

        return PilotFromIndex(index + 1);
    }

    private string FormatRecordTime(float seconds)
    {
        if (seconds <= 0.0f)
        {
            return T("leader.no_record");
        }

        int centiseconds = TimeToCentiseconds(seconds);
        int minutes = centiseconds / 6000;
        int secondsPart = centiseconds / 100 % 60;
        int fraction = centiseconds % 100;
        return $"{minutes:00}:{secondsPart:00}.{fraction:00}";
    }

    private static Color LeaderboardRankAccent(int index, bool hasRecord)
    {
        if (!hasRecord)
        {
            return Paper.Lerp(Graphite, 0.35f);
        }

        return index switch
        {
            0 => Gold,
            1 => PickupBlue,
            2 => Jade,
            _ => Paper.Lerp(PickupBlue, 0.28f),
        };
    }

    private void DrawPilotSelect()
    {
        Rect2 panel = PilotCardRect(0);
        PilotKind pilot = TitleDisplayedPilot();
        bool unlocked = IsPilotUnlocked(pilot);
        bool selected = unlocked && _selectedPilot == pilot;
        Color accent = unlocked ? PilotAccent(pilot) : GridLine;
        Rect2 drawRect = panel;

        DrawText(T("menu.pilot"), new Vector2(0.0f, panel.Position.Y - 18.0f), 13, Alpha(Paper, 0.34f), HorizontalAlignment.Center, ScreenWidth, false, 0);
        DrawPilotSwitchArrow(PilotPreviousButtonRect(), -1, accent);
        DrawPilotSwitchArrow(PilotNextButtonRect(), 1, accent);

        DrawLine(drawRect.Position + new Vector2(260.0f, 24.0f), drawRect.Position + new Vector2(drawRect.Size.X - 42.0f, 24.0f), Alpha(accent, unlocked ? 0.16f : 0.08f), UiHairline, true);
        DrawLine(drawRect.Position + new Vector2(260.0f, drawRect.Size.Y - 26.0f), drawRect.Position + new Vector2(drawRect.Size.X - 42.0f, drawRect.Size.Y - 26.0f), Alpha(Paper, unlocked ? 0.08f : 0.04f), UiHairline, true);
        DrawGlow(drawRect.Position + new Vector2(190.0f, drawRect.Size.Y * 0.58f), accent, 210.0f, unlocked ? 0.02f : 0.01f, 4);

        Vector2 artCenter = drawRect.Position + new Vector2(190.0f, 132.0f);
        DrawLine(artCenter + new Vector2(-86.0f, 82.0f), artCenter + new Vector2(86.0f, 82.0f), Alpha(accent, unlocked ? 0.24f : 0.12f), UiHairline, true);
        DrawLine(artCenter + new Vector2(-48.0f, 92.0f), artCenter + new Vector2(48.0f, 92.0f), Alpha(Paper, unlocked ? 0.16f : 0.07f), UiHairline, true);
        DrawPilotHull(pilot, artCenter, Vector2.Up, accent, unlocked ? 0.95f : 0.3f, 1.5f);

        Vector2 textPos = drawRect.Position + new Vector2(370.0f, 52.0f);
        float textWidth = drawRect.Size.X - 418.0f;
        DrawText(PilotName(pilot).ToUpperInvariant(), textPos, 31, unlocked ? Paper : Alpha(Paper, 0.36f), HorizontalAlignment.Left, 320.0f, true, 4);
        DrawText($"{_gamepadPilotIndex + 1:00}/{PilotCount():00}", drawRect.Position + new Vector2(drawRect.Size.X - 92.0f, 41.0f), 13, Alpha(accent, unlocked ? 0.68f : 0.34f), HorizontalAlignment.Right, 72.0f, false, 0);
        DrawLine(textPos + new Vector2(0.0f, 44.0f), drawRect.Position + new Vector2(drawRect.Size.X - 30.0f, 86.0f), Alpha(accent, unlocked ? 0.26f : 0.12f), UiHairline, true);

        DrawText(PilotWeapon(pilot), textPos + new Vector2(0.0f, 70.0f), 17, Alpha(accent, unlocked ? 0.9f : 0.42f), HorizontalAlignment.Left, 270.0f, true, 1);
        DrawText($"{T("pilot.selector.skill")}: {TacticalSkillName(pilot)}", textPos + new Vector2(330.0f, 70.0f), 15, Alpha(Paper, unlocked ? 0.62f : 0.32f), HorizontalAlignment.Right, Mathf.Max(180.0f, textWidth - 330.0f), true, 1);
        DrawText($"{T("pilot.selector.ultimate")}: {UltimateName(pilot)}", textPos + new Vector2(0.0f, 98.0f), 14, Alpha(Paper, unlocked ? 0.52f : 0.28f), HorizontalAlignment.Left, textWidth, false, 0);
        DrawWrapped(unlocked ? PilotBody(pilot) : PilotUnlockText(pilot), textPos + new Vector2(0.0f, 126.0f), unlocked ? 13 : 12, Alpha(Paper, unlocked ? 0.58f : 0.48f), textWidth, unlocked ? 16.0f : 15.0f);

        string stateText = unlocked ? T("pilot.selector.selected") : T("pilot.selector.locked");
        Color stateColor = unlocked ? accent : Rose;
        DrawText(stateText, drawRect.Position + new Vector2(drawRect.Size.X - 166.0f, drawRect.Size.Y - 24.0f), 11, Alpha(stateColor, selected ? 0.82f : 0.58f), HorizontalAlignment.Right, 134.0f, false, 0);

        if (!unlocked)
        {
            Rect2 bar = new(drawRect.Position + new Vector2(370.0f, drawRect.Size.Y - 31.0f), new Vector2(430.0f, 4.0f));
            float progress = PilotUnlockProgress(pilot);
            DrawRect(bar, Alpha(Paper, 0.08f), true);
            DrawRect(new Rect2(bar.Position, new Vector2(bar.Size.X * progress, bar.Size.Y)), Alpha(PilotAccent(pilot), 0.58f), true);
        }
    }

    private void DrawPilotSwitchArrow(Rect2 rect, int direction, Color accent)
    {
        bool hover = rect.HasPoint(GetGlobalMousePosition());
        Color line = Alpha(accent, hover ? 0.82f : 0.46f);
        Rect2 drawRect = rect;

        Vector2 center = drawRect.Position + drawRect.Size * 0.5f;
        float sign = direction < 0 ? -1.0f : 1.0f;
        Vector2 tip = center + new Vector2(sign * 10.0f, 0.0f);
        Vector2 top = center + new Vector2(-sign * 9.0f, -17.0f);
        Vector2 bottom = center + new Vector2(-sign * 9.0f, 17.0f);
        DrawLine(top, tip, line, UiAccentStroke, true);
        DrawLine(bottom, tip, line, UiAccentStroke, true);
    }

    private void DrawSettings()
    {
        DrawRect(new Rect2(Vector2.Zero, new Vector2(ScreenWidth, ScreenHeight)), Alpha(Void, 0.56f), true);
        bool runSettings = IsRunViewMode(_settingsReturnMode);
        Rect2 panel = new(new Vector2(580.0f, 130.0f), new Vector2(760.0f, 820.0f));
        DrawGlow(panel.Position + panel.Size * 0.5f, CurrentSector().Accent, 380.0f, 0.04f, 7);
        DrawPanel(panel, Alpha(Graphite, 0.9f), Alpha(GridLine, 0.48f));
        DrawSettingsButton(new Rect2(panel.Position + new Vector2(30.0f, 30.0f), new Vector2(42.0f, 42.0f)), true);
        DrawText(T("settings.title"), panel.Position + new Vector2(0.0f, 92.0f), 44, Paper, HorizontalAlignment.Center, panel.Size.X, true, 4);
        DrawText(T("settings.subtitle"), panel.Position + new Vector2(58.0f, 138.0f), 19, Alpha(Paper, 0.64f), HorizontalAlignment.Center, panel.Size.X - 116.0f, true, 2);

        DrawMenuButton(SettingsOptionRect(0), T("settings.guide"), XpGreen, true);
        DrawSettingsValueRow(SettingsOptionRect(1), T("settings.music"), $"{Mathf.RoundToInt(_musicVolume * 100.0f)}%", Cyan, _musicVolume);
        DrawSettingsValueRow(SettingsOptionRect(2), T("settings.sfx"), $"{Mathf.RoundToInt(_sfxVolume * 100.0f)}%", Gold, _sfxVolume);
        DrawSettingsValueRow(SettingsOptionRect(3), T("settings.language"), LanguageDisplayName(_language), PolarityColor(_playerPolarity), -1.0f);
        DrawSettingsValueRow(SettingsOptionRect(4), T("settings.resolution"), ResolutionDisplayName(_resolutionPreset), PickupBlue, -1.0f);
        DrawSettingsValueRow(SettingsOptionRect(5), T("settings.quality"), QualityDisplayName(_visualQuality), Jade, (int)_visualQuality / (float)(VisualQualityCount() - 1));
        if (runSettings)
        {
            DrawMenuButton(SettingsOptionRect(6), T("settings.main_menu"), Rose, false);
        }
        string deleteLabel = _deleteSaveConfirmTimer > 0.0f ? T("settings.delete_confirm") : T("settings.delete_save");
        DrawMenuButton(SettingsOptionRect(runSettings ? 7 : 6), deleteLabel, _deleteSaveConfirmTimer > 0.0f ? Rose : AlertRed.Lerp(Paper, 0.2f), false);
        if (_deleteSaveConfirmTimer > 0.0f)
        {
            DrawText(T("settings.delete_warning"), panel.Position + new Vector2(70.0f, 728.0f), 15, Alpha(Rose, 0.76f), HorizontalAlignment.Center, panel.Size.X - 140.0f, true, 1);
        }
        else if (_deleteSaveNoticeTimer > 0.0f)
        {
            DrawText(T("settings.delete_notice"), panel.Position + new Vector2(70.0f, 728.0f), 16, Alpha(Jade, 0.78f), HorizontalAlignment.Center, panel.Size.X - 140.0f, true, 1);
        }
        string backLabel = runSettings ? T("settings.resume") : T("settings.back");
        DrawMenuButton(SettingsOptionRect(runSettings ? 8 : 7), backLabel, GridLine, false);
        DrawText(T("settings.adjust_hint"), panel.Position + new Vector2(0.0f, 778.0f), 17, Alpha(Paper, 0.42f), HorizontalAlignment.Center, panel.Size.X, true, 2);
    }

    private void DrawGuide()
    {
        DrawRect(new Rect2(Vector2.Zero, new Vector2(ScreenWidth, ScreenHeight)), Alpha(Void, 0.64f), true);
        Rect2 panel = new(new Vector2(420.0f, 150.0f), new Vector2(1080.0f, 760.0f));
        DrawGlow(panel.Position + panel.Size * 0.5f, XpGreen, 460.0f, 0.035f, 8);
        DrawPanel(panel, Alpha(Graphite, 0.92f), Alpha(GridLine, 0.5f));
        DrawText(T("guide.title"), panel.Position + new Vector2(0.0f, 82.0f), 48, Paper, HorizontalAlignment.Center, panel.Size.X, true, 5);
        DrawText(T("guide.subtitle"), panel.Position + new Vector2(100.0f, 128.0f), 20, Alpha(Paper, 0.6f), HorizontalAlignment.Center, panel.Size.X - 200.0f, true, 2);

        for (int i = 0; i < GuidePageCount(); i++)
        {
            Rect2 tab = GuideTabRect(i);
            bool selected = i == _guidePage;
            bool hover = tab.HasPoint(GetGlobalMousePosition());
            Color accent = selected ? XpGreen : GridLine;
            DrawPanel(tab, Alpha(Ink, selected ? 0.68f : 0.34f), Alpha(accent, selected || hover ? 0.64f : 0.28f));
            DrawText(T($"guide.tab.{i}"), tab.Position + new Vector2(0.0f, 27.0f), 15, selected ? Paper : Alpha(Paper, 0.58f), HorizontalAlignment.Center, tab.Size.X, true, 1);
        }

        string[] lines = GuideLines();
        for (int i = 0; i < lines.Length; i++)
        {
            float y = 242.0f + i * 82.0f;
            Rect2 row = new(panel.Position + new Vector2(84.0f, y), new Vector2(912.0f, 62.0f));
            Color accent = i switch
            {
                0 => Paper,
                1 => EnemyBulletColor(),
                2 => PilotAccent(_runPilot),
                3 => XpGreen,
                4 => PickupBlue,
                _ => AlertRed,
            };
            DrawPanel(row, Alpha(Ink, 0.34f), Alpha(accent, 0.3f));
            DrawCircle(row.Position + new Vector2(28.0f, 31.0f), 9.0f, Alpha(accent, 0.88f));
            DrawWrapped(lines[i], row.Position + new Vector2(54.0f, 17.0f), 18, Alpha(Paper, 0.78f), row.Size.X - 78.0f, 23.0f);
        }

        DrawText(T("guide.page_hint"), panel.Position + new Vector2(0.0f, 638.0f), 17, Alpha(Paper, 0.45f), HorizontalAlignment.Center, panel.Size.X, true, 1);
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
            bool hover = rect.HasPoint(mouse) || IsGamepadFocused(rect);
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
        DrawText(NextWavePreviewText(), new Vector2(0.0f, 276.0f), 18, Alpha(CurrentSector().Accent, 0.82f), HorizontalAlignment.Center, ScreenWidth, true, 2);
        DrawText(UpgradeMomentumText(), new Vector2(0.0f, 302.0f), 14, Alpha(Paper, 0.42f), HorizontalAlignment.Center, ScreenWidth, true, 1);
        DrawBuildDirectionPanel();

        Vector2 mouse = GetGlobalMousePosition();
        for (int i = 0; i < _upgradeChoices.Count; i++)
        {
            UpgradeCard card = _upgradeChoices[i];
            bool hover = card.Rect.HasPoint(mouse) || IsGamepadFocused(card.Rect);
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
            string badgeText = UpgradeBadgeText(card.Id);
            float badgeWidth = Mathf.Clamp(EstimateTextPixelWidth(badgeText.ToUpperInvariant(), 11) + 20.0f, 76.0f, 196.0f);
            Rect2 badge = new(rect.Position + new Vector2(26.0f, 52.0f), new Vector2(badgeWidth, 22.0f));
            DrawRect(badge, Alpha(card.Accent, hover ? 0.18f : 0.1f), true);
            DrawRect(badge, Alpha(card.Accent, hover ? 0.44f : 0.26f), false, UiHairline, true);
            DrawText(badgeText.ToUpperInvariant(), badge.Position + new Vector2(9.0f, 15.5f), 11, Alpha(card.Accent.Lerp(Paper, 0.18f), 0.9f), HorizontalAlignment.Left, badge.Size.X - 18.0f, false, 0);
            DrawText(card.Title.ToUpperInvariant(), rect.Position + new Vector2(26.0f, 82.0f), 27, Paper, HorizontalAlignment.Left, rect.Size.X - 122.0f, true, 3);
            DrawHighlightedWrapped(card.Body, rect.Position + new Vector2(26.0f, 152.0f), 20, Alpha(Paper, 0.68f), card.Accent, rect.Size.X - 52.0f, 31.0f);

            int rank = GetRank(card.Id);
            int maxRank = MaxRank(card.Id);
            int nextRank = Math.Min(rank + 1, maxRank);
            string rankText = Tf("upgrade.rank_change", rank, nextRank);
            string slotText = IsGamepadFocused(card.Rect)
                ? T("upgrade.select_gamepad")
                : Tf("upgrade.select_key", i + 1);
            Rect2 footer = new(rect.Position + new Vector2(24.0f, rect.Size.Y - 58.0f), new Vector2(rect.Size.X - 48.0f, 36.0f));
            DrawRect(footer, Alpha(Graphite, 0.34f), true);
            DrawRect(footer, Alpha(card.Accent, hover ? 0.36f : 0.2f), false, UiHairline, true);
            DrawText(rankText, footer.Position + new Vector2(12.0f, 23.0f), 14, Alpha(card.Accent, 0.94f), HorizontalAlignment.Left, footer.Size.X - 24.0f, false, 0);
            DrawText(slotText, footer.Position + new Vector2(0.0f, 23.0f), 14, Alpha(Paper, hover ? 0.78f : 0.52f), HorizontalAlignment.Right, footer.Size.X - 12.0f, false, 0);
        }
    }

    private void DrawBuildDirectionPanel()
    {
        Rect2 panel = new(new Vector2(470.0f, 756.0f), new Vector2(980.0f, 86.0f));
        int dominant = DominantDraftBiasIndex();
        int dominantValue = DraftBiasValue(dominant);
        Color accent = dominantValue >= 3 ? BuildPathAccent(dominant) : Alpha(Paper, 0.62f);
        DrawGlow(panel.Position + panel.Size * 0.5f, accent, 210.0f, 0.018f, 4);
        DrawPanel(panel, Alpha(Ink, 0.48f), Alpha(accent, dominantValue >= 3 ? 0.38f : 0.22f));
        DrawText(T("build.panel.title").ToUpperInvariant(), panel.Position + new Vector2(22.0f, 28.0f), 13, Alpha(Paper, 0.54f), HorizontalAlignment.Left, 180.0f, false, 0);
        string focus = dominantValue >= 3
            ? Tf("build.panel.focus", UpgradePathLabel(dominant))
            : T("build.panel.open");
        DrawText(focus.ToUpperInvariant(), panel.Position + new Vector2(22.0f, 52.0f), 18, Alpha(accent, 0.86f), HorizontalAlignment.Left, 260.0f, true, 1);

        for (int i = 0; i < 5; i++)
        {
            float x = panel.Position.X + 318.0f + i * 126.0f;
            int value = DraftBiasValue(i);
            float maxMilestone = BuildMilestoneThresholds[BuildMilestoneThresholds.Length - 1];
            float progress = Mathf.Clamp(value / maxMilestone, 0.0f, 1.0f);
            Color pathAccent = BuildPathAccent(i);
            Rect2 bar = new(new Vector2(x, panel.Position.Y + 48.0f), new Vector2(88.0f, 5.0f));
            DrawText(UpgradePathLabel(i).ToUpperInvariant(), new Vector2(x, panel.Position.Y + 28.0f), 12, Alpha(pathAccent, i == dominant && dominantValue >= 3 ? 0.92f : 0.58f), HorizontalAlignment.Left, 104.0f, false, 0);
            DrawRect(bar, Alpha(Paper, 0.08f), true);
            DrawRect(new Rect2(bar.Position, new Vector2(bar.Size.X * progress, bar.Size.Y)), Alpha(pathAccent, 0.66f), true);
            DrawRect(bar, Alpha(pathAccent, 0.28f), false, UiHairline, true);
            for (int tier = 0; tier < BuildMilestoneThresholds.Length; tier++)
            {
                float tickX = bar.Position.X + bar.Size.X * (BuildMilestoneThresholds[tier] / maxMilestone);
                bool claimed = HasBuildMilestone(i, tier);
                Color tickColor = claimed ? pathAccent.Lerp(Paper, 0.16f) : Alpha(Paper, 0.22f);
                DrawLine(new Vector2(tickX, bar.Position.Y - 5.0f), new Vector2(tickX, bar.End.Y + 5.0f), Alpha(tickColor, claimed ? 0.86f : 0.58f), claimed ? UiStroke : UiHairline, true);
            }
        }
    }

    private void DrawEndScreen(bool victory)
    {
        DrawRect(new Rect2(Vector2.Zero, new Vector2(ScreenWidth, ScreenHeight)), new Color(0.0f, 0.0f, 0.0f, victory ? 0.34f : 0.58f), true);
        Color color = victory ? Gold : Rose;
        DrawGlow(ScreenCenter, color, victory ? 620.0f : 460.0f, 0.07f, 9);
        DrawText(victory ? T("end.victory.title") : T("end.defeat.title"), new Vector2(0.0f, 430.0f), 82, color, HorizontalAlignment.Center, ScreenWidth, true, 7);
        DrawText(Tf("end.wave", _lastRunWave, TotalWaves), new Vector2(0.0f, 506.0f), 32, Paper, HorizontalAlignment.Center, ScreenWidth, true, 4);
        DrawText(victory ? T("end.victory.body") : T("end.defeat.body"), new Vector2(0.0f, 568.0f), 24, Alpha(Paper, 0.72f), HorizontalAlignment.Center, ScreenWidth, true, 3);
        DrawText(Tf("end.reward", _lastDustEarned, _lastRunWave), new Vector2(0.0f, 620.0f), 25, Gold, HorizontalAlignment.Center, ScreenWidth, true, 3);
        float bonusY = 656.0f;
        if (_lastScoreBonusDust > 0)
        {
            DrawText(Tf("end.score_bonus", _lastScoreBonusDust), new Vector2(0.0f, bonusY), 20, Alpha(Gold, 0.86f), HorizontalAlignment.Center, ScreenWidth, true, 2);
            bonusY += 30.0f;
        }
        if (_lastObjectiveBonusDust > 0)
        {
            DrawText(Tf("end.objective_bonus", _lastObjectiveBonusDust), new Vector2(0.0f, bonusY), 20, Alpha(Jade, 0.82f), HorizontalAlignment.Center, ScreenWidth, true, 2);
        }
        DrawPanel(new Rect2(new Vector2(760.0f, 700.0f), new Vector2(400.0f, 58.0f)), Alpha(Ink, 0.72f), Alpha(color, 0.48f));
        DrawText(T("end.restart"), new Vector2(760.0f, 738.0f), 23, Paper, HorizontalAlignment.Center, 400.0f, true, 3);
        float hintY = 786.0f;
        if (_lastUnlockedPilot.HasValue)
        {
            DrawText(Tf("end.unlock_pilot", PilotName(_lastUnlockedPilot.Value)), new Vector2(0.0f, hintY), 22, Alpha(PilotAccent(_lastUnlockedPilot.Value), 0.9f), HorizontalAlignment.Center, ScreenWidth, true, 2);
            hintY += 34.0f;
        }
        if (victory && _lastClearTime > 0.0f)
        {
            string recordText = _lastClearRecordRank > 0
                ? Tf("end.clear_record", _lastClearRecordRank, FormatRecordTime(_lastClearTime))
                : Tf("end.clear_time", FormatRecordTime(_lastClearTime));
            DrawText(recordText, new Vector2(0.0f, hintY), 20, Alpha(_lastClearRecordRank > 0 ? Gold : Paper, 0.82f), HorizontalAlignment.Center, ScreenWidth, true, 2);
            hintY += 34.0f;
        }
        DrawNextGoalPanel(new Rect2(new Vector2(600.0f, hintY), new Vector2(720.0f, 52.0f)), false);
        hintY += 76.0f;
        DrawText(T("end.meta_hint"), new Vector2(0.0f, hintY), 20, Alpha(Paper, 0.64f), HorizontalAlignment.Center, ScreenWidth, true, 2);
        DrawText(T("language.hint"), new Vector2(0.0f, hintY + 30.0f), 20, Alpha(Paper, 0.58f), HorizontalAlignment.Center, ScreenWidth, true, 2);
    }

    private void DrawHudMetric(Rect2 rect, string label, string value, Color accent)
    {
        DrawPanel(rect, Alpha(Ink, 0.34f), Alpha(GridLine, 0.22f));
        DrawText(label.ToUpperInvariant(), rect.Position + new Vector2(10.0f, 16.0f), 10, Alpha(Paper, 0.44f), HorizontalAlignment.Left, rect.Size.X - 20.0f, false, 0);
        DrawText(value, rect.Position + new Vector2(10.0f, 35.0f), 18, accent, HorizontalAlignment.Left, rect.Size.X - 20.0f, true, 2);
    }

    private void DrawSpawnMetric(Rect2 rect)
    {
        float rate = SpawnRateMultiplier();
        Color accent = rate >= 1.65f ? Gold : Alpha(Paper, 0.72f);
        int comboBonus = ComboSpawnBonusPercent();
        string label = comboBonus > 0 ? Tf("hud.spawn.combo", comboBonus) : T("hud.spawn.label");
        DrawPanel(rect, Alpha(Ink, 0.34f), Alpha(GridLine, 0.22f));
        DrawText(label.ToUpperInvariant(), rect.Position + new Vector2(10.0f, 16.0f), 10, Alpha(comboBonus > 0 ? Gold : Paper, comboBonus > 0 ? 0.62f : 0.44f), HorizontalAlignment.Left, rect.Size.X - 20.0f, false, 0);
        DrawText(SpawnMetricText(), rect.Position + new Vector2(10.0f, 34.0f), 17, accent, HorizontalAlignment.Left, rect.Size.X - 20.0f, true, 2);

        Rect2 bar = new(rect.Position + new Vector2(10.0f, rect.Size.Y - 6.0f), new Vector2(rect.Size.X - 20.0f, 3.0f));
        DrawRect(bar, Alpha(Paper, 0.08f), true);
        DrawRect(new Rect2(bar.Position, new Vector2(bar.Size.X * _hudSpawnTrail, bar.Size.Y)), Alpha(Paper, 0.12f), true);
        DrawRect(new Rect2(bar.Position, new Vector2(bar.Size.X * _hudSpawnValue, bar.Size.Y)), Alpha(accent, 0.72f + _comboTierPulse * 0.2f), true);
    }

    private void DrawScoreCacheMeter(Rect2 rect)
    {
        int previous = _scoreCacheLevel <= 0 ? 0 : ScoreCacheThreshold(_scoreCacheLevel - 1);
        int span = Math.Max(1, _nextScoreCache - previous);
        float progress = Mathf.Clamp((_score - previous) / (float)span, 0.0f, 1.0f);
        Color accent = _scoreCachePulse > 0.0f ? Gold : XpGreen;
        Rect2 bar = new(rect.Position + new Vector2(10.0f, rect.Size.Y - 6.0f), new Vector2(rect.Size.X - 20.0f, 3.0f));
        DrawRect(bar, Alpha(Paper, 0.08f), true);
        DrawRect(new Rect2(bar.Position, new Vector2(bar.Size.X * progress, bar.Size.Y)), Alpha(accent, 0.74f + _scoreCachePulse * 0.22f), true);
        DrawText($"{T("hud.cache.label")} {Mathf.RoundToInt(progress * 100.0f)}%", rect.Position + new Vector2(rect.Size.X - 92.0f, 16.0f), 10, Alpha(accent, 0.62f + _scoreCachePulse * 0.26f), HorizontalAlignment.Right, 82.0f, false, 0);
    }

    private void DrawExperienceMeter(Rect2 rect)
    {
        float progress = Mathf.Clamp(_xp / (float)Math.Max(1, _xpToNext), 0.0f, 1.0f);
        Color accent = XpGray().Lerp(Paper, 0.18f + _xpPulse * 0.28f);
        Rect2 bar = new(rect.Position + new Vector2(10.0f, rect.Size.Y - 6.0f), new Vector2(rect.Size.X - 20.0f, 3.0f));
        DrawRect(bar, Alpha(Paper, 0.08f), true);
        DrawRect(new Rect2(bar.Position, new Vector2(bar.Size.X * progress, bar.Size.Y)), Alpha(accent, 0.74f + _xpPulse * 0.2f), true);
        DrawText($"{Mathf.RoundToInt(progress * 100.0f)}%", rect.Position + new Vector2(rect.Size.X - 60.0f, 16.0f), 10, Alpha(accent, 0.62f + _xpPulse * 0.26f), HorizontalAlignment.Right, 50.0f, false, 0);
    }

    private void DrawBottomExperienceBar()
    {
        Rect2 panel = new(new Vector2(330.0f, 1002.0f), new Vector2(1260.0f, 44.0f));
        float progress = Mathf.Clamp(_xp / (float)Math.Max(1, _xpToNext), 0.0f, 1.0f);
        Color accent = XpGray().Lerp(Paper, 0.2f + _xpPulse * 0.32f);
        DrawPanel(panel, Alpha(Ink, 0.52f), Alpha(accent, 0.3f + _xpPulse * 0.28f));
        DrawLine(panel.Position + new Vector2(14.0f, panel.Size.Y - 7.0f), panel.Position + new Vector2(panel.Size.X - 14.0f, panel.Size.Y - 7.0f), Alpha(Paper, 0.08f), UiHairline, true);

        Rect2 levelBox = new(panel.Position + new Vector2(14.0f, 8.0f), new Vector2(78.0f, 28.0f));
        DrawRect(levelBox, Alpha(Graphite, 0.58f), true);
        DrawRect(levelBox, Alpha(accent, 0.38f + _xpPulse * 0.22f), false, UiHairline, true);
        DrawText($"{T("hud.level.label")} {_runLevel}", levelBox.Position + new Vector2(0.0f, 20.0f), 14, Alpha(Paper, 0.82f + _xpPulse * 0.14f), HorizontalAlignment.Center, levelBox.Size.X, true, 1);

        Rect2 bar = new(panel.Position + new Vector2(112.0f, 15.0f), new Vector2(panel.Size.X - 250.0f, 14.0f));
        DrawRect(bar.Grow(2.0f), Alpha(Paper, 0.07f), true);
        DrawRect(bar, Alpha(Graphite, 0.8f), true);
        DrawRect(new Rect2(bar.Position, new Vector2(bar.Size.X * progress, bar.Size.Y)), Alpha(accent, 0.68f + _xpPulse * 0.26f), true);
        DrawLine(bar.Position + new Vector2(0.0f, 2.0f), bar.Position + new Vector2(bar.Size.X * progress, 2.0f), Alpha(Paper, 0.22f + _xpPulse * 0.24f), UiHairline, true);

        string value = $"{_xp}/{_xpToNext}";
        DrawText(T("hud.xp.label"), bar.Position + new Vector2(0.0f, -5.0f), 12, Alpha(Paper, 0.5f), HorizontalAlignment.Left, 100.0f, false, 0);
        DrawText(value, bar.Position + new Vector2(bar.Size.X + 12.0f, 10.0f), 14, Alpha(Paper, 0.72f), HorizontalAlignment.Left, 100.0f, false, 0);
        DrawText($"{Mathf.RoundToInt(progress * 100.0f)}%", bar.Position + new Vector2(0.0f, 10.0f), 13, Alpha(Paper, 0.62f + _xpPulse * 0.22f), HorizontalAlignment.Center, bar.Size.X, false, 0);
    }

    private void DrawSettingsButton(Rect2 rect, bool withLabel)
    {
        bool hover = rect.HasPoint(GetGlobalMousePosition()) || IsGamepadFocused(rect);
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

    private void DrawSettingsValueRow(Rect2 rect, string label, string value, Color accent, float fill01)
    {
        bool hover = rect.HasPoint(GetGlobalMousePosition()) || IsGamepadFocused(rect);
        Rect2 drawRect = hover ? rect.Grow(5.0f) : rect;
        DrawGlow(drawRect.Position + drawRect.Size * 0.5f, accent, hover ? 120.0f : 72.0f, hover ? 0.056f : 0.028f, 4);
        DrawPanel(drawRect, Alpha(Ink, hover ? 0.72f : 0.58f), Alpha(accent, hover ? 0.78f : 0.38f));
        DrawText(label.ToUpperInvariant(), drawRect.Position + new Vector2(24.0f, 32.0f), 17, Alpha(Paper, hover ? 0.9f : 0.68f), HorizontalAlignment.Left, drawRect.Size.X - 280.0f, true, 2);
        DrawText("<", drawRect.Position + new Vector2(drawRect.Size.X - 204.0f, 32.0f), 18, Alpha(accent, hover ? 0.9f : 0.5f), HorizontalAlignment.Center, 34.0f, true, 1);
        DrawText(value, drawRect.Position + new Vector2(drawRect.Size.X - 170.0f, 32.0f), 17, Alpha(Paper, hover ? 0.92f : 0.68f), HorizontalAlignment.Center, 126.0f, true, 2);
        DrawText(">", drawRect.Position + new Vector2(drawRect.Size.X - 44.0f, 32.0f), 18, Alpha(accent, hover ? 0.9f : 0.5f), HorizontalAlignment.Center, 34.0f, true, 1);

        Rect2 line = new(drawRect.Position + new Vector2(22.0f, drawRect.Size.Y - 8.0f), new Vector2(drawRect.Size.X - 44.0f, 2.0f));
        DrawRect(line, Alpha(Paper, 0.07f), true);
        float progress = fill01 >= 0.0f ? Mathf.Clamp(fill01, 0.0f, 1.0f) : 1.0f;
        DrawRect(new Rect2(line.Position, new Vector2(line.Size.X * progress, line.Size.Y)), Alpha(accent, fill01 >= 0.0f ? 0.66f : 0.28f), true);
    }

    private void DrawMenuButton(Rect2 rect, string label, Color accent, bool primary)
    {
        bool hover = rect.HasPoint(GetGlobalMousePosition()) || IsGamepadFocused(rect);
        Rect2 drawRect = hover ? rect.Grow(primary ? 8.0f : 5.0f) : rect;
        DrawGlow(drawRect.Position + drawRect.Size * 0.5f, accent, primary ? 150.0f : 90.0f, hover ? 0.08f : 0.045f, 4);
        DrawPanel(drawRect, Alpha(Ink, primary ? 0.78f : 0.62f), Alpha(accent, hover ? 0.82f : 0.42f));
        DrawText(label, drawRect.Position + new Vector2(0.0f, primary ? 43.0f : 34.0f), primary ? 28 : 21, Paper, HorizontalAlignment.Center, drawRect.Size.X, true, 3);
    }

    private void DrawTitleTextButton(Rect2 rect, string label, Color accent)
    {
        bool hover = rect.HasPoint(GetGlobalMousePosition()) || IsGamepadFocused(rect);
        Color textColor = hover ? accent.Lerp(Paper, 0.28f) : Alpha(accent, 0.68f);
        DrawText(label.ToUpperInvariant(), rect.Position + new Vector2(0.0f, 28.0f), 18, textColor, HorizontalAlignment.Center, rect.Size.X, false, 0);
        DrawLine(rect.Position + new Vector2(rect.Size.X * 0.22f, rect.Size.Y - 5.0f), rect.Position + new Vector2(rect.Size.X * 0.78f, rect.Size.Y - 5.0f), Alpha(accent, hover ? 0.58f : 0.24f), UiHairline, true);
    }

    private void DrawGmUnlockButton()
    {
        Rect2 rect = GmUnlockButtonRect();
        bool hover = rect.HasPoint(GetGlobalMousePosition());
        Color accent = hover ? Rose.Lerp(Paper, 0.18f) : Alpha(Rose, 0.58f);
        DrawPanel(rect, Alpha(Ink, hover ? 0.62f : 0.34f), Alpha(accent, hover ? 0.72f : 0.34f));
        DrawText(T("gm.unlock.label"), rect.Position + new Vector2(0.0f, 23.0f), 14, Alpha(Paper, hover ? 0.86f : 0.58f), HorizontalAlignment.Center, rect.Size.X, true, 1);
    }

    private void UpdateHudBarEasing(float dt)
    {
        float hullTarget = SafeRatio(_playerHp, _playerMaxHp);
        float energyTarget = SafeRatio(_energy, _maxEnergy);
        float dashTarget = Mathf.Clamp(1.0f - Mathf.Max(_dashCooldown, 0.0f) / 0.86f, 0.0f, 1.0f);
        float spawnTarget = NextReserveSpawnProgress01();

        _hudHullValue = EaseHudValue(_hudHullValue, hullTarget, hullTarget > _hudHullValue ? 10.0f : 8.0f, dt);
        _hudEnergyValue = EaseHudValue(_hudEnergyValue, energyTarget, energyTarget > _hudEnergyValue ? 7.5f : 10.5f, dt);
        _hudDashValue = EaseHudValue(_hudDashValue, dashTarget, dashTarget > _hudDashValue ? 12.0f : 18.0f, dt);
        _hudSpawnValue = EaseHudValue(_hudSpawnValue, spawnTarget, spawnTarget > _hudSpawnValue ? 9.0f : 5.0f, dt);

        _hudHullTrail = EaseHudValue(_hudHullTrail, hullTarget, hullTarget < _hudHullTrail ? 2.4f : 13.0f, dt);
        _hudEnergyTrail = EaseHudValue(_hudEnergyTrail, energyTarget, energyTarget < _hudEnergyTrail ? 3.0f : 9.0f, dt);
        _hudDashTrail = EaseHudValue(_hudDashTrail, dashTarget, dashTarget < _hudDashTrail ? 5.0f : 11.0f, dt);
        _hudSpawnTrail = EaseHudValue(_hudSpawnTrail, spawnTarget, spawnTarget < _hudSpawnTrail ? 2.0f : 10.0f, dt);
    }

    private void SnapHudBars()
    {
        _hudHullValue = SafeRatio(_playerHp, _playerMaxHp);
        _hudHullTrail = _hudHullValue;
        _hudEnergyValue = SafeRatio(_energy, _maxEnergy);
        _hudEnergyTrail = _hudEnergyValue;
        _hudDashValue = Mathf.Clamp(1.0f - Mathf.Max(_dashCooldown, 0.0f) / 0.86f, 0.0f, 1.0f);
        _hudDashTrail = _hudDashValue;
        _hudSpawnValue = NextReserveSpawnProgress01();
        _hudSpawnTrail = _hudSpawnValue;
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

    private float ComboPace01()
    {
        return Mathf.Clamp(_combo / 80.0f, 0.0f, 1.0f);
    }

    private float ProgressSpawnRate()
    {
        if (_currentWavePace == WavePaceKind.Boss)
        {
            return 1.0f;
        }

        return Mathf.Clamp(5.0f / BaseReserveSpawnInterval(), 1.0f, 5.6f);
    }

    private float ComboSpawnRate()
    {
        if (_combo <= 0)
        {
            return 1.0f;
        }

        float combo01 = 1.0f - Mathf.Exp(-Mathf.Min(_combo, 160) / 76.0f);
        return 1.0f + combo01 * 0.38f;
    }

    private int ComboSpawnBonusPercent()
    {
        return Mathf.RoundToInt((1.0f - 1.0f / ComboSpawnRate()) * 100.0f);
    }

    private float SpawnRateMultiplier()
    {
        if (_currentWavePace == WavePaceKind.Boss)
        {
            return 1.0f;
        }

        return Mathf.Clamp(5.0f / CurrentReserveSpawnInterval(), 1.0f, 5.6f);
    }

    private static float SpawnRate01(float rate)
    {
        return Mathf.Clamp((rate - 1.0f) / (5.6f - 1.0f), 0.0f, 1.0f);
    }

    private float NextReserveSpawnProgress01()
    {
        if (_currentWavePace == WavePaceKind.Boss)
        {
            return 0.0f;
        }

        if (WaveProgressComplete() && _pendingSpawns.Count == 0)
        {
            return 1.0f;
        }

        if (_waveSpawnInterval <= 0.001f)
        {
            return 0.0f;
        }

        return Mathf.Clamp(1.0f - Mathf.Max(0.0f, _waveSpawnTimer) / _waveSpawnInterval, 0.0f, 1.0f);
    }

    private string SpawnSpeedText()
    {
        if (_currentWavePace == WavePaceKind.Boss)
        {
            return T("hud.spawn.boss");
        }

        return $"x{SpawnRateMultiplier():0.00}";
    }

    private string SpawnMetricText()
    {
        if (_currentWavePace == WavePaceKind.Boss)
        {
            return T("hud.spawn.boss");
        }

        if (WaveProgressComplete() && _pendingSpawns.Count == 0)
        {
            return T("hud.spawn.done");
        }

        return Tf("hud.spawn.next_short", Math.Max(0.0f, _waveSpawnTimer), Math.Max(1, _waveNextSpawnCount));
    }

    private int WaveReserveEstimate()
    {
        if (_currentWavePace == WavePaceKind.Boss)
        {
            return 0;
        }

        float remaining = Math.Max(0.0f, _waveProgressBudget - _waveProgressSpent);
        return _pendingSpawns.Count + Mathf.CeilToInt(remaining / 1.08f);
    }

    private Enemy? ActiveBoss()
    {
        Enemy? best = null;
        float bestHp = -1.0f;
        foreach (Enemy enemy in _enemies)
        {
            if (enemy.Kind != EnemyKind.Boss)
            {
                continue;
            }

            if (enemy.Hp > bestHp)
            {
                bestHp = enemy.Hp;
                best = enemy;
            }
        }

        return best;
    }

    private string BossNextPhaseText(Enemy boss)
    {
        if (boss.BossPhase >= BossPhaseThresholds.Length)
        {
            return T("boss.final_hud");
        }

        int thresholdPercent = Mathf.RoundToInt(BossPhaseThresholds[boss.BossPhase] * 100.0f);
        return Tf("boss.next_hud", thresholdPercent);
    }

    private string BossPatternText(BossPatternKind pattern)
    {
        return pattern switch
        {
            BossPatternKind.SpiralRing => T("boss.pattern.spiral"),
            BossPatternKind.HeavyLance => T("boss.pattern.lance"),
            BossPatternKind.SummonWing => T("boss.pattern.summon"),
            BossPatternKind.HazardFan => T("boss.pattern.hazard"),
            BossPatternKind.ReverseSpiral => T("boss.pattern.reverse"),
            BossPatternKind.WardenCall => T("boss.pattern.warden"),
            BossPatternKind.CrossBloom => T("boss.pattern.cross"),
            BossPatternKind.MineDrift => T("boss.pattern.mine"),
            BossPatternKind.MirrorFork => T("boss.pattern.mirror"),
            BossPatternKind.TempestWheel => T("boss.pattern.tempest"),
            BossPatternKind.BastionWall => T("boss.pattern.bastion"),
            BossPatternKind.SerpentCoil => T("boss.pattern.serpent"),
            BossPatternKind.OracleSnipe => T("boss.pattern.oracle"),
            _ => T("boss.pattern.fan"),
        };
    }

    private int QualityParticleCap()
    {
        return QualityParticleCap(PerformancePressure());
    }

    private int QualityParticleCap(float pressure)
    {
        int cap = _visualQuality switch
        {
            VisualQuality.Low => 240,
            VisualQuality.Medium => 360,
            VisualQuality.Ultra => 680,
            _ => 520,
        };

        if (pressure > 0.9f)
        {
            cap = Mathf.RoundToInt(cap * 0.62f);
        }
        else if (pressure > 0.76f)
        {
            cap = Mathf.RoundToInt(cap * 0.78f);
        }

        return Math.Max(140, cap);
    }

    private int QualityDamageTextCap()
    {
        return QualityDamageTextCap(PerformancePressure());
    }

    private int QualityDamageTextCap(float pressure)
    {
        int cap = _visualQuality switch
        {
            VisualQuality.Low => 36,
            VisualQuality.Medium => 48,
            VisualQuality.Ultra => MaxDamageTexts,
            _ => 60,
        };

        if (pressure > 0.86f)
        {
            cap = Math.Min(cap, 28);
        }
        else if (pressure > 0.72f)
        {
            cap = Math.Min(cap, 40);
        }

        return cap;
    }

    private int QualityStarCount()
    {
        return _visualQuality switch
        {
            VisualQuality.Low => 90,
            VisualQuality.Medium => 170,
            VisualQuality.Ultra => 360,
            _ => 260,
        };
    }

    private float QualityBurstScale()
    {
        return _visualQuality switch
        {
            VisualQuality.Low => 0.45f,
            VisualQuality.Medium => 0.72f,
            VisualQuality.Ultra => 1.18f,
            _ => 1.0f,
        };
    }

    private int QualityGlowLayers(int layers)
    {
        int qualityLayers = _visualQuality switch
        {
            VisualQuality.Low => Math.Min(layers, 2),
            VisualQuality.Medium => Math.Min(layers, 4),
            VisualQuality.Ultra => layers + 1,
            _ => layers,
        };

        float pressure = PerformancePressure();
        if (pressure > 0.9f)
        {
            return Math.Min(qualityLayers, 2);
        }
        if (pressure > 0.76f)
        {
            return Math.Min(qualityLayers, 3);
        }
        return qualityLayers;
    }

    private float CalculateVisualPressure()
    {
        float pressure = PerformancePressure();
        float shotPressure = _shots.Count / 320.0f;
        float enemyPressure = _enemies.Count / 52.0f;
        float hazardPressure = (_hazards.Count + _hazardFields.Count * 1.4f) / 14.0f;
        float particlePressure = _particles.Count / (float)QualityParticleCap(pressure);
        float textPressure = _damageTexts.Count / (float)QualityDamageTextCap(pressure);
        return Mathf.Clamp(Mathf.Max(Mathf.Max(shotPressure, hazardPressure), Mathf.Max(enemyPressure, particlePressure * 0.9f)) + textPressure * 0.08f, 0.0f, 1.0f);
    }

    private float PerformancePressure()
    {
        return Mathf.Max(_visualPressure, _frameRatePressure);
    }

    private void UpdatePerformancePressure(float dt)
    {
        float fps = (float)Engine.GetFramesPerSecond();
        float target = 0.0f;
        if (fps > 1.0f && fps < 45.0f)
        {
            target = 1.0f;
        }
        else if (fps > 1.0f && fps < 54.0f)
        {
            target = 0.55f;
        }

        _frameRatePressure = EaseHudValue(_frameRatePressure, target, target > _frameRatePressure ? 2.2f : 0.75f, dt);
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
        size = LocalizedFontSize(text, size, width);
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
                IsStandaloneHighlightMatch(text, index, term) &&
                (best is null || term.Length > best.Length))
            {
                best = term;
            }
        }

        return best;
    }

    private static bool IsStandaloneHighlightMatch(string text, int index, string term)
    {
        if (!RequiresHighlightBoundary(term))
        {
            return true;
        }

        int before = index - 1;
        int after = index + term.Length;
        if (before >= 0 && IsHighlightWordChar(text[before]))
        {
            return false;
        }
        if (after < text.Length && IsHighlightWordChar(text[after]))
        {
            return false;
        }

        return true;
    }

    private static bool RequiresHighlightBoundary(string term)
    {
        foreach (char c in term)
        {
            if (IsCjkOrKana(c))
            {
                return false;
            }
        }

        foreach (char c in term)
        {
            if (char.IsLetterOrDigit(c))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsHighlightWordChar(char c)
    {
        return char.IsLetterOrDigit(c) || c == '_' || c == '-';
    }

    private static bool IsCjkOrKana(char c)
    {
        return (c >= '\u3400' && c <= '\u9fff') ||
            (c >= '\uf900' && c <= '\ufaff') ||
            (c >= '\u3040' && c <= '\u30ff') ||
            (c >= '\u31f0' && c <= '\u31ff') ||
            (c >= '\uac00' && c <= '\ud7af');
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
        layers = QualityGlowLayers(layers);
        if (layers <= 0)
        {
            return;
        }

        for (int i = layers; i >= 1; i--)
        {
            float t = i / (float)layers;
            DrawCircle(pos, radius * t, Alpha(color, alpha * (1.0f - t * 0.72f)));
        }
    }

    private void AddParticle(Vector2 pos, Vector2 vel, Color color, float size, float life)
    {
        float pressure = PerformancePressure();
        int particleCap = QualityParticleCap(pressure);
        AddParticle(pos, vel, color, size, life, particleCap, pressure);
    }

    private void AddParticle(Vector2 pos, Vector2 vel, Color color, float size, float life, int particleCap, float pressure)
    {
        if (_particles.Count >= particleCap)
        {
            return;
        }

        if (pressure > 0.86f && _particles.Count > particleCap * 0.62f && _rng.Randf() < 0.5f)
        {
            return;
        }

        Particle particle = AddParticleObject(particleCap);
        particle.Pos = pos;
        particle.Vel = vel;
        particle.Color = color;
        particle.Size = size;
        particle.Life = life;
        particle.MaxLife = life;
        particle.Spin = _rng.RandfRange(-1.0f, 1.0f);
    }

    private void AddDroneCommandCue(Vector2 pos, Vector2 facing, Color color, float scale)
    {
        if (_visualPressure > 0.9f)
        {
            return;
        }

        if (_droneCommandCues.Count >= MaxDroneCommandCues)
        {
            _droneCommandCues.RemoveAt(0);
        }

        Vector2 direction = SafeDirection(facing, _aimDir);
        _droneCommandCues.Add(new DroneCommandCue
        {
            Pos = pos,
            Vel = direction * 36.0f,
            Facing = direction,
            Color = color,
            Life = 0.36f,
            MaxLife = 0.36f,
            Scale = scale,
        });
    }

    private void AddShockwave(Vector2 center, float radius, Color color)
    {
        if (_shockwaves.Count >= 6)
        {
            _shockwaves.RemoveAt(0);
        }

        _shockwaves.Add(new Shockwave
        {
            Center = center,
            Radius = radius,
            Color = color,
            Life = 0.58f,
            MaxLife = 0.58f,
        });
    }

    private void Burst(Vector2 pos, Color color, int count, float speed, float life)
    {
        float pressure = PerformancePressure();
        count = Mathf.Max(1, Mathf.RoundToInt(count * QualityBurstScale()));
        if (pressure > 0.9f)
        {
            count = Mathf.Max(1, count / 4);
        }
        else if (pressure > 0.72f)
        {
            count = Mathf.Max(2, count / 3);
        }
        else if (pressure > 0.55f)
        {
            count = Mathf.Max(3, count / 2);
        }

        int particleCap = QualityParticleCap(pressure);
        if (_particles.Count > particleCap * 0.78f)
        {
            count = Mathf.Max(1, count / 3);
        }
        else if (_particles.Count > particleCap * 0.58f)
        {
            count = Mathf.Max(2, count / 2);
        }

        for (int i = 0; i < count; i++)
        {
            Vector2 dir = RandomDirection();
            AddParticle(pos + dir * _rng.RandfRange(0.0f, 22.0f), dir * _rng.RandfRange(speed * 0.18f, speed), color.Lerp(Paper, _rng.RandfRange(0.0f, 0.36f)), _rng.RandfRange(3.0f, 12.0f), _rng.RandfRange(life * 0.35f, life), particleCap, pressure);
        }
    }

    private void UpdateCenterTextQueue(float dt)
    {
        if (_centerTextQueue.Count == 0)
        {
            _centerTextQueueTimer = Math.Max(0.0f, _centerTextQueueTimer - dt);
            return;
        }

        _centerTextQueueTimer -= dt;
        if (_centerTextQueueTimer > 0.0f)
        {
            return;
        }

        TextCue cue = _centerTextQueue.Dequeue();
        AddTextNow(cue.Text, cue.Pos, cue.Color, cue.Size, false);
        _centerTextQueueTimer = _centerTextQueue.Count > 0 ? 0.34f : 0.0f;
    }

    private void QueueCenterText(string text, Vector2 pos, Color color, float size)
    {
        if (_centerTextQueue.Count > 9)
        {
            _centerTextQueue.Dequeue();
        }

        _centerTextQueue.Enqueue(new TextCue
        {
            Text = text,
            Pos = pos,
            Color = color,
            Size = size,
        });
    }

    private void AddText(string text, Vector2 pos, Color color, float size)
    {
        AddTextNow(text, pos, color, size, false);
    }

    private void AddTextNow(string text, Vector2 pos, Color color, float size, bool elastic)
    {
        DamageText damageText = AddDamageTextObject();
        damageText.Text = text;
        damageText.Pos = pos;
        damageText.Color = color;
        damageText.Life = elastic ? 1.32f : 1.15f;
        damageText.MaxLife = damageText.Life;
        damageText.Size = size;
        damageText.ComboPop = elastic;
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
        return new Rect2(new Vector2(810.0f, 928.0f), new Vector2(300.0f, 44.0f));
    }

    private static Rect2 MetaButtonRect()
    {
        return new Rect2(new Vector2(372.0f, 940.0f), new Vector2(280.0f, 36.0f));
    }

    private static Rect2 TitleSettingsButtonRect()
    {
        return new Rect2(new Vector2(1268.0f, 940.0f), new Vector2(280.0f, 36.0f));
    }

    private static Rect2 GmUnlockButtonRect()
    {
        return new Rect2(new Vector2(1810.0f, 46.0f), new Vector2(58.0f, 28.0f));
    }

    private static Rect2 LeaderboardPanelRect()
    {
        return new Rect2(new Vector2(1392.0f, 116.0f), new Vector2(430.0f, 218.0f));
    }

    private static Rect2 DifficultyButtonRect(int index)
    {
        return new Rect2(new Vector2(650.0f + index * 210.0f, 404.0f), new Vector2(200.0f, 32.0f));
    }

    private static Rect2 TitleNextGoalRect()
    {
        return new Rect2(new Vector2(560.0f, 454.0f), new Vector2(800.0f, 48.0f));
    }

    private static Rect2 PilotCardRect(int index)
    {
        return new Rect2(new Vector2(410.0f, 600.0f), new Vector2(1100.0f, 280.0f));
    }

    private static Rect2 PilotPreviousButtonRect()
    {
        Rect2 panel = PilotCardRect(0);
        return new Rect2(panel.Position + new Vector2(-74.0f, 104.0f), new Vector2(48.0f, 82.0f));
    }

    private static Rect2 PilotNextButtonRect()
    {
        Rect2 panel = PilotCardRect(0);
        return new Rect2(panel.Position + new Vector2(panel.Size.X + 26.0f, 104.0f), new Vector2(48.0f, 82.0f));
    }

    private static int PilotCount()
    {
        return 8;
    }

    private int UnlockedPilotCount()
    {
        int count = 0;
        for (int i = 0; i < PilotCount(); i++)
        {
            if (IsPilotUnlocked(PilotFromIndex(i)))
            {
                count++;
            }
        }

        return count;
    }

    private static PilotKind PilotFromIndex(int index)
    {
        return index switch
        {
            1 => PilotKind.Vesper,
            2 => PilotKind.Rook,
            3 => PilotKind.Kairo,
            4 => PilotKind.Nyx,
            5 => PilotKind.Lyra,
            6 => PilotKind.Sol,
            7 => PilotKind.Orion,
            _ => PilotKind.Astra,
        };
    }

    private static int PilotIndex(PilotKind pilot)
    {
        return pilot switch
        {
            PilotKind.Vesper => 1,
            PilotKind.Rook => 2,
            PilotKind.Kairo => 3,
            PilotKind.Nyx => 4,
            PilotKind.Lyra => 5,
            PilotKind.Sol => 6,
            PilotKind.Orion => 7,
            _ => 0,
        };
    }

    private bool IsPilotUnlocked(PilotKind pilot)
    {
        return pilot == PilotKind.Astra || PilotUnlockProgress(pilot) >= 1.0f;
    }

    private void UnlockAllForTesting()
    {
        for (int i = 0; i < PilotCount(); i++)
        {
            PilotKind pilot = PilotFromIndex(i);
            _pilotRuns[pilot] = Math.Max(PilotRunCount(pilot), 1);
        }
        for (int i = 0; i < DifficultyCount; i++)
        {
            _difficultyTestUnlocks[i] = true;
        }

        SaveMetaProgress();
        AddText(T("gm.unlock.toast"), ScreenCenter + new Vector2(0.0f, 230.0f), Jade, 22.0f);
        PlaySfx(560.0f, 90.0f, 0.18f, 0.18f, 0.02f, 1);
    }

    private float PilotUnlockProgress(PilotKind pilot)
    {
        PilotKind previous = PreviousPilot(pilot);
        if (previous == pilot)
        {
            return 1.0f;
        }

        return Progress01(PilotRunCount(previous), 1);
    }

    private PilotKind PreviousPilot(PilotKind pilot)
    {
        int index = PilotIndex(pilot);
        return index <= 0 ? PilotKind.Astra : PilotFromIndex(index - 1);
    }

    private int PilotRunCount(PilotKind pilot)
    {
        return _pilotRuns.TryGetValue(pilot, out int runs) ? Math.Max(0, runs) : 0;
    }

    private string PilotName(PilotKind pilot)
    {
        return pilot switch
        {
            PilotKind.Vesper => T("pilot.vesper.name"),
            PilotKind.Kairo => T("pilot.kairo.name"),
            PilotKind.Sol => T("pilot.sol.name"),
            PilotKind.Nyx => T("pilot.nyx.name"),
            PilotKind.Rook => T("pilot.rook.name"),
            PilotKind.Lyra => T("pilot.lyra.name"),
            PilotKind.Orion => T("pilot.orion.name"),
            _ => T("pilot.astra.name"),
        };
    }

    private string TitleName()
    {
        return T("title.name");
    }

    private void ApplyWindowTitle()
    {
        DisplayServer.WindowSetTitle(TitleName());
    }

    private string TitleStartPrompt()
    {
        return T("title.start");
    }

    private string PilotBody(PilotKind pilot)
    {
        return pilot switch
        {
            PilotKind.Vesper => T("pilot.vesper.body"),
            PilotKind.Kairo => T("pilot.kairo.body"),
            PilotKind.Sol => T("pilot.sol.body"),
            PilotKind.Nyx => T("pilot.nyx.body"),
            PilotKind.Rook => T("pilot.rook.body"),
            PilotKind.Lyra => T("pilot.lyra.body"),
            PilotKind.Orion => T("pilot.orion.body"),
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
            PilotKind.Nyx => T("pilot.nyx.weapon"),
            PilotKind.Rook => T("pilot.rook.weapon"),
            PilotKind.Lyra => T("pilot.lyra.weapon"),
            PilotKind.Orion => T("pilot.orion.weapon"),
            _ => T("pilot.astra.weapon"),
        };
    }

    private string UltimateName(PilotKind pilot)
    {
        return T("ultimate.common");
    }

    private string PilotUnlockText(PilotKind pilot)
    {
        if (pilot == PilotKind.Astra)
        {
            return T("pilot.unlock.free");
        }

        PilotKind previous = PreviousPilot(pilot);
        return Tf("pilot.unlock.chain", PilotName(previous), Math.Min(PilotRunCount(previous), 1), 1);
    }

    private static Color PilotAccent(PilotKind pilot)
    {
        return pilot switch
        {
            PilotKind.Vesper => Rose,
            PilotKind.Kairo => PickupBlue,
            PilotKind.Sol => Gold,
            PilotKind.Nyx => Violet.Lerp(Cyan, 0.18f),
            PilotKind.Rook => new Color(0.92f, 0.72f, 0.38f),
            PilotKind.Lyra => Jade,
            PilotKind.Orion => new Color(1.0f, 0.48f, 0.14f),
            _ => PolarityBlue,
        };
    }

    private static Rect2 HudSettingsButtonRect()
    {
        return new Rect2(new Vector2(1804.0f, 26.0f), new Vector2(42.0f, 42.0f));
    }

    private Rect2 SettingsDeleteSaveButtonRect()
    {
        return SettingsOptionRect(IsRunViewMode(_settingsReturnMode) ? 7 : 6);
    }

    private Rect2 SettingsBackButtonRect()
    {
        return SettingsOptionRect(IsRunViewMode(_settingsReturnMode) ? 8 : 7);
    }

    private static Rect2 GuideBackButtonRect()
    {
        return new Rect2(new Vector2(790.0f, 828.0f), new Vector2(340.0f, 50.0f));
    }

    private static int GuidePageCount()
    {
        return 4;
    }

    private static Rect2 GuideTabRect(int index)
    {
        const float width = 200.0f;
        const float height = 38.0f;
        const float gap = 20.0f;
        return new Rect2(new Vector2(520.0f + index * (width + gap), 318.0f), new Vector2(width, height));
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
        QueueCenterText(header, ScreenCenter + new Vector2(0.0f, -160.0f), newEnemy ? accent : Alpha(Paper, 0.82f), newEnemy ? 28.0f : 21.0f);
        if (newEnemy)
        {
            QueueCenterText(EnemyRole(primary), ScreenCenter + new Vector2(0.0f, -126.0f), Alpha(Paper, 0.72f), 18.0f);
        }
        else if (support != primary)
        {
            QueueCenterText(Tf("wave.enemy.support", EnemyName(support)), ScreenCenter + new Vector2(0.0f, -130.0f), Alpha(accent, 0.68f), 17.0f);
        }
    }

    private static WavePaceKind WavePaceFor(int wave)
    {
        if (wave <= 0)
        {
            return WavePaceKind.Standard;
        }

        int sector = (wave - 1) / WavesPerSector;
        int waveInSector = ((wave - 1) % WavesPerSector) + 1;
        if (waveInSector == WavesPerSector)
        {
            return WavePaceKind.Boss;
        }

        if (sector == 0)
        {
            return waveInSector switch
            {
                3 => WavePaceKind.Swarm,
                4 => WavePaceKind.Recovery,
                5 => WavePaceKind.Elite,
                6 => WavePaceKind.Pressure,
                7 => WavePaceKind.Swarm,
                _ => WavePaceKind.Standard,
            };
        }

        if (sector == 1)
        {
            return waveInSector switch
            {
                2 => WavePaceKind.Swarm,
                4 => WavePaceKind.Recovery,
                5 => WavePaceKind.Elite,
                6 => WavePaceKind.Pressure,
                7 => WavePaceKind.Elite,
                _ => WavePaceKind.Standard,
            };
        }

        return waveInSector switch
        {
            2 => WavePaceKind.Swarm,
            3 => WavePaceKind.Pressure,
            4 => WavePaceKind.Recovery,
            5 => WavePaceKind.Elite,
            6 => WavePaceKind.Pressure,
            7 => sector >= 3 ? WavePaceKind.Elite : WavePaceKind.Swarm,
            _ => WavePaceKind.Standard,
        };
    }

    private static int WaveBaseBudget(int sector, int waveInSector)
    {
        return sector switch
        {
            0 => 18 + waveInSector * 5,
            1 => 28 + waveInSector * 6,
            2 => 38 + waveInSector * 7,
            3 => 48 + waveInSector * 8,
            _ => 58 + waveInSector * 9,
        };
    }

    private static int WaveOpeningBatchCount(int sector, int waveInSector, WavePaceKind pace, float budget)
    {
        int count = 5 + sector / 2 + Mathf.FloorToInt(waveInSector * 0.42f);
        count += waveInSector switch
        {
            3 => 1,
            4 => -1,
            6 => 1,
            7 => 2,
            _ => 0,
        };

        count = pace switch
        {
            WavePaceKind.Swarm => count + 2,
            WavePaceKind.Pressure => count + 1,
            WavePaceKind.Elite => Math.Max(5, count - 1),
            WavePaceKind.Recovery => Math.Max(5, count - 2),
            _ => count,
        };
        int budgetCap = Math.Max(5, Mathf.FloorToInt(budget * 0.2f));
        return Mathf.Clamp(count, 5, Math.Min(budgetCap, 10));
    }

    private static int MaxSpawnBatchCount(int sector, int waveInSector)
    {
        int cap = 8 + sector / 2;
        if (waveInSector is 3 or 6)
        {
            cap++;
        }
        if (waveInSector >= 7)
        {
            cap++;
        }

        return Mathf.Clamp(cap, 5, 10);
    }

    private static float WaveProgressBudget(float baseBudget, int sector, int waveInSector, WavePaceKind pace)
    {
        float sectorScale = 1.0f + sector * 0.07f;
        float lateWaveScale = 1.0f + Math.Max(0, waveInSector - 3) * 0.05f;
        float paceScale = pace switch
        {
            WavePaceKind.Swarm => 0.98f,
            WavePaceKind.Elite => 1.02f,
            WavePaceKind.Recovery => 0.88f,
            WavePaceKind.Pressure => 1.05f,
            _ => 1.0f,
        };
        return Math.Max(2.0f, baseBudget * sectorScale * lateWaveScale * paceScale * WaveBudgetArcScale(waveInSector) * 1.1f);
    }

    private static float WaveSpawnProgressCost(EnemyKind kind, bool elite)
    {
        float cost = kind switch
        {
            EnemyKind.Chaser => 0.92f,
            EnemyKind.Weaver => 1.05f,
            EnemyKind.Turret => 1.22f,
            EnemyKind.Splitter => 1.46f,
            EnemyKind.Lance => 1.34f,
            EnemyKind.Mine => 0.88f,
            EnemyKind.Shard => 0.78f,
            EnemyKind.Warden => 1.72f,
            EnemyKind.Drifter => 1.06f,
            EnemyKind.Bulwark => 1.95f,
            EnemyKind.Siren => 1.52f,
            EnemyKind.Harrier => 1.28f,
            _ => 1.0f,
        };
        return elite ? cost * 1.55f : cost;
    }

    private static EnemyKind WaveEventEnemyKind(int sector, WavePaceKind pace, int eventIndex)
    {
        if (pace == WavePaceKind.Swarm)
        {
            return sector >= 3 ? EnemyKind.Harrier : EnemyKind.Splitter;
        }
        if (pace == WavePaceKind.Elite)
        {
            return sector >= 3 ? EnemyKind.Warden : EnemyKind.Bulwark;
        }
        if (pace == WavePaceKind.Pressure)
        {
            return eventIndex == 0 ? (sector >= 2 ? EnemyKind.Siren : EnemyKind.Lance) : (sector >= 3 ? EnemyKind.Warden : EnemyKind.Drifter);
        }

        return sector >= 2 ? EnemyKind.Siren : EnemyKind.Turret;
    }

    private static float WaveBudgetScale(WavePaceKind pace)
    {
        return pace switch
        {
            WavePaceKind.Swarm => 1.24f,
            WavePaceKind.Elite => 0.82f,
            WavePaceKind.Recovery => 0.68f,
            WavePaceKind.Pressure => 1.12f,
            _ => 1.0f,
        };
    }

    private static float WaveRewardScale(WavePaceKind pace)
    {
        return pace switch
        {
            WavePaceKind.Elite => 1.35f,
            WavePaceKind.Recovery => 1.2f,
            WavePaceKind.Pressure => 1.15f,
            _ => 1.0f,
        };
    }

    private static float WaveSpawnIntervalScale(WavePaceKind pace)
    {
        return pace switch
        {
            WavePaceKind.Swarm => 0.76f,
            WavePaceKind.Elite => 1.16f,
            WavePaceKind.Recovery => 1.28f,
            WavePaceKind.Pressure => 0.88f,
            _ => 1.0f,
        };
    }

    private static float WaveBudgetArcScale(int waveInSector)
    {
        return waveInSector switch
        {
            1 => 0.86f,
            2 => 0.96f,
            3 => 1.08f,
            4 => 0.86f,
            5 => 0.96f,
            6 => 1.1f,
            7 => 1.18f,
            _ => 1.0f,
        };
    }

    private static float WaveIntervalArcScale(int waveInSector)
    {
        return waveInSector switch
        {
            1 => 1.15f,
            2 => 1.03f,
            3 => 0.88f,
            4 => 1.24f,
            5 => 1.1f,
            6 => 0.92f,
            7 => 0.82f,
            _ => 1.0f,
        };
    }

    private string WavePaceText(WavePaceKind pace)
    {
        return pace switch
        {
            WavePaceKind.Swarm => T("wave.pace.swarm"),
            WavePaceKind.Elite => T("wave.pace.elite"),
            WavePaceKind.Recovery => T("wave.pace.recovery"),
            WavePaceKind.Pressure => T("wave.pace.pressure"),
            WavePaceKind.Boss => T("wave.pace.boss"),
            _ => T("wave.pace.standard"),
        };
    }

    private string WavePaceShortText(WavePaceKind pace)
    {
        return pace switch
        {
            WavePaceKind.Swarm => T("wave.pace.short.swarm"),
            WavePaceKind.Elite => T("wave.pace.short.elite"),
            WavePaceKind.Recovery => T("wave.pace.short.recovery"),
            WavePaceKind.Pressure => T("wave.pace.short.pressure"),
            WavePaceKind.Boss => T("wave.pace.short.boss"),
            _ => T("wave.pace.short.standard"),
        };
    }

    private static EnemyKind SelectEnemyKind(int index, int sector, int waveInSector, EnemyKind primary, EnemyKind support, WavePaceKind pace)
    {
        if (ShouldInjectMechanicEnemy(index, sector, waveInSector, pace))
        {
            return WaveMechanicEnemyKind(sector, waveInSector, pace, index);
        }

        int supportEvery = pace switch
        {
            WavePaceKind.Swarm => 6,
            WavePaceKind.Elite => 3,
            WavePaceKind.Pressure => 2,
            WavePaceKind.Recovery => 5,
            _ => sector >= 3 || waveInSector >= 6 ? 4 : 5,
        };
        return support != primary && index > 0 && index % supportEvery == supportEvery - 1 ? support : primary;
    }

    private static bool ShouldInjectMechanicEnemy(int index, int sector, int waveInSector, WavePaceKind pace)
    {
        if (index <= 0 || (sector == 0 && waveInSector < 5))
        {
            return false;
        }

        int every = pace switch
        {
            WavePaceKind.Swarm => sector >= 2 ? 3 : 4,
            WavePaceKind.Pressure => 3,
            WavePaceKind.Elite => 3,
            WavePaceKind.Recovery => 6,
            _ => sector >= 3 || waveInSector >= 6 ? 4 : 5,
        };

        if (SpawnIndexPressurePhase(index) >= 2)
        {
            every = Math.Max(2, every - 1);
        }

        return index % every == every - 1;
    }

    private static int SpawnIndexPressurePhase(int spawnIndex)
    {
        return spawnIndex < 10 ? 0 : spawnIndex < 24 ? 1 : 2;
    }

    private static EnemyKind WaveMechanicEnemyKind(int sector, int waveInSector, WavePaceKind pace, int index)
    {
        int roll = Math.Abs(index + waveInSector * 3 + (int)pace * 5);
        if (sector <= 0)
        {
            return waveInSector >= 6 && roll % 3 == 0 ? EnemyKind.Turret : EnemyKind.Drifter;
        }

        if (sector == 1)
        {
            if (waveInSector <= 1)
            {
                return EnemyKind.Drifter;
            }
            if (waveInSector == 2)
            {
                return EnemyKind.Mine;
            }

            return (roll % 4) switch
            {
                0 => EnemyKind.Mine,
                1 => EnemyKind.Lance,
                2 => EnemyKind.Drifter,
                _ => EnemyKind.Turret,
            };
        }

        if (sector == 2)
        {
            if (waveInSector < 3)
            {
                return (roll % 4) switch
                {
                    0 => EnemyKind.Splitter,
                    1 => EnemyKind.Mine,
                    2 => EnemyKind.Lance,
                    _ => EnemyKind.Turret,
                };
            }
            if (waveInSector == 3)
            {
                return (roll % 4) switch
                {
                    0 => EnemyKind.Siren,
                    1 => EnemyKind.Splitter,
                    2 => EnemyKind.Mine,
                    _ => EnemyKind.Lance,
                };
            }

            return (roll % 5) switch
            {
                0 => EnemyKind.Splitter,
                1 => EnemyKind.Siren,
                2 => EnemyKind.Bulwark,
                3 => EnemyKind.Mine,
                _ => EnemyKind.Lance,
            };
        }

        if (sector == 3)
        {
            if (waveInSector < 3)
            {
                return (roll % 3) switch
                {
                    0 => EnemyKind.Shard,
                    1 => EnemyKind.Siren,
                    _ => EnemyKind.Bulwark,
                };
            }
            if (waveInSector == 3)
            {
                return (roll % 4) switch
                {
                    0 => EnemyKind.Warden,
                    1 => EnemyKind.Shard,
                    2 => EnemyKind.Siren,
                    _ => EnemyKind.Bulwark,
                };
            }

            return (roll % 5) switch
            {
                0 => EnemyKind.Warden,
                1 => EnemyKind.Harrier,
                2 => EnemyKind.Siren,
                3 => EnemyKind.Shard,
                _ => EnemyKind.Bulwark,
            };
        }

        return (roll % 6) switch
        {
            0 => EnemyKind.Warden,
            1 => EnemyKind.Siren,
            2 => EnemyKind.Harrier,
            3 => EnemyKind.Bulwark,
            4 => EnemyKind.Shard,
            _ => EnemyKind.Splitter,
        };
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
                4 => EnemyKind.Weaver,
                5 => EnemyKind.Turret,
                6 => EnemyKind.Turret,
                _ => EnemyKind.Drifter,
            },
            1 => waveInSector switch
            {
                1 => EnemyKind.Drifter,
                2 => EnemyKind.Mine,
                3 => EnemyKind.Lance,
                4 => EnemyKind.Weaver,
                5 => EnemyKind.Mine,
                6 => EnemyKind.Lance,
                _ => EnemyKind.Splitter,
            },
            2 => waveInSector switch
            {
                1 => EnemyKind.Splitter,
                2 => EnemyKind.Mine,
                3 => EnemyKind.Siren,
                4 => EnemyKind.Bulwark,
                5 => EnemyKind.Turret,
                6 => EnemyKind.Siren,
                _ => EnemyKind.Bulwark,
            },
            3 => waveInSector switch
            {
                1 => EnemyKind.Shard,
                2 => EnemyKind.Shard,
                3 => EnemyKind.Warden,
                4 => EnemyKind.Harrier,
                5 => EnemyKind.Siren,
                6 => EnemyKind.Bulwark,
                _ => EnemyKind.Warden,
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
                7 => EnemyKind.Turret,
                _ => WavePrimaryEnemyKind(sector, waveInSector),
            },
            1 => waveInSector switch
            {
                1 => EnemyKind.Chaser,
                3 => EnemyKind.Chaser,
                4 => EnemyKind.Drifter,
                5 => EnemyKind.Lance,
                6 => EnemyKind.Mine,
                7 => EnemyKind.Drifter,
                _ => WavePrimaryEnemyKind(sector, waveInSector),
            },
            2 => waveInSector switch
            {
                1 => EnemyKind.Lance,
                2 => EnemyKind.Chaser,
                4 => EnemyKind.Mine,
                5 => EnemyKind.Splitter,
                6 => EnemyKind.Mine,
                7 => EnemyKind.Siren,
                _ => WavePrimaryEnemyKind(sector, waveInSector),
            },
            3 => waveInSector switch
            {
                2 => EnemyKind.Drifter,
                4 => EnemyKind.Harrier,
                5 => EnemyKind.Warden,
                6 => EnemyKind.Shard,
                7 => EnemyKind.Harrier,
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
            EnemyKind.Drifter => 7,
            EnemyKind.Turret => 5,
            EnemyKind.Lance => 11,
            EnemyKind.Mine => 10,
            EnemyKind.Splitter => 15,
            EnemyKind.Bulwark => 20,
            EnemyKind.Shard => 25,
            EnemyKind.Siren => 19,
            EnemyKind.Harrier => 28,
            EnemyKind.Warden => 27,
            _ => 1,
        };
    }

    private string EnemyName(EnemyKind kind)
    {
        return T($"enemy.{EnemyKey(kind)}.name");
    }

    private string BossTitle(BossArchetype archetype, int sector)
    {
        return Tf("boss.title", T(Sectors[Mathf.Clamp(sector, 0, SectorCount - 1)].NameKey), BossName(archetype));
    }

    private string BossName(BossArchetype archetype)
    {
        return T($"boss.{BossKey(archetype)}.name");
    }

    private string BossSignatureText(BossArchetype archetype)
    {
        return T($"boss.{BossKey(archetype)}.signature");
    }

    private static string BossKey(BossArchetype archetype)
    {
        return archetype switch
        {
            BossArchetype.Prism => "prism",
            BossArchetype.Swarm => "swarm",
            BossArchetype.Forge => "forge",
            BossArchetype.Rift => "rift",
            BossArchetype.Mirror => "mirror",
            BossArchetype.Tempest => "tempest",
            BossArchetype.Bastion => "bastion",
            BossArchetype.Serpent => "serpent",
            BossArchetype.Oracle => "oracle",
            _ => "choir",
        };
    }

    private static Color BossAccent(BossArchetype archetype)
    {
        return archetype switch
        {
            BossArchetype.Prism => Cyan,
            BossArchetype.Swarm => Jade,
            BossArchetype.Forge => Gold,
            BossArchetype.Rift => Violet,
            BossArchetype.Mirror => Cyan.Lerp(Paper, 0.22f),
            BossArchetype.Tempest => Cyan,
            BossArchetype.Bastion => Gold.Lerp(Paper, 0.2f),
            BossArchetype.Serpent => Jade.Lerp(Paper, 0.1f),
            BossArchetype.Oracle => Rose.Lerp(Paper, 0.2f),
            _ => Rose,
        };
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

        float pressure = PerformancePressure();
        if (pressure > 0.92f)
        {
            cap = Mathf.RoundToInt(cap * 0.68f);
        }
        else if (pressure > 0.82f)
        {
            cap = Mathf.RoundToInt(cap * 0.82f);
        }
        else if (pressure > 0.72f)
        {
            cap = Mathf.RoundToInt(cap * 0.92f);
        }

        int floor = pressure > 0.92f ? 48 : pressure > 0.82f ? 56 : EnemyBulletCapStart;
        return Math.Max(floor, cap);
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
            ? Mathf.Lerp(0.8f, 1.12f, progress)
            : Mathf.Lerp(0.46f, 1.0f, progress);
        int scaled = Mathf.RoundToInt(baseCount * density);
        int max = kind == EnemyKind.Boss ? baseCount + Mathf.CeilToInt(baseCount * 0.18f) : baseCount;
        return Mathf.Clamp(scaled, 1, max);
    }

    private int ScaledEnemyPatternCount(EnemyKind kind, int baseCount)
    {
        if (baseCount <= 1)
        {
            return 1;
        }

        float progress = RunProgress01();
        float density = kind == EnemyKind.Boss
            ? Mathf.Lerp(0.8f, 1.12f, progress)
            : Mathf.Lerp(0.46f, 1.0f, progress);
        if (kind == EnemyKind.Boss)
        {
            float pressure = PerformancePressure();
            float bulletLoad = ActiveEnemyBulletCount() / (float)Math.Max(1, EnemyBulletCap());
            if (pressure > 0.92f || bulletLoad > 0.92f)
            {
                density *= 0.68f;
            }
            else if (pressure > 0.82f || bulletLoad > 0.78f)
            {
                density *= 0.84f;
            }
        }
        int scaled = Mathf.RoundToInt(baseCount * density);
        int max = kind == EnemyKind.Boss ? baseCount + Mathf.CeilToInt(baseCount * 0.18f) : baseCount;
        return Mathf.Clamp(scaled, 1, max);
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

        float difficultyScale = DifficultyEnemyBulletSpeedScale();
        return Mathf.Min(baseSpeed * scale * difficultyScale, cap * difficultyScale);
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

    private static Vector2 SafeDirection(Vector2 value, Vector2 fallback)
    {
        if (value.LengthSquared() > 0.0001f)
        {
            return value.Normalized();
        }

        return fallback.LengthSquared() > 0.0001f ? fallback.Normalized() : Vector2.Up;
    }

    private static Vector2 LimitVector(Vector2 value, float maxLength)
    {
        float maxSquared = maxLength * maxLength;
        float lengthSquared = value.LengthSquared();
        if (lengthSquared <= maxSquared || lengthSquared <= 0.0001f)
        {
            return value;
        }

        return value / Mathf.Sqrt(lengthSquared) * maxLength;
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

    private Color PickupColor(PickupKind kind)
    {
        return kind switch
        {
            PickupKind.Dust => XpGray(),
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
        return _drawShakeOffset;
    }

    private Vector2 CalculateShakeOffset()
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
        int count = Mathf.Clamp(sides, 3, PolygonScratchMaxSides);
        int slot = _polygonScratchCursor++ & (PolygonScratchSlots - 1);
        Vector2[] points = _polygonScratch[count][slot];
        for (int i = 0; i < count; i++)
        {
            float a = rotation + i * Mathf.Tau / count;
            points[i] = center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
        }
        return points;
    }

    private Vector2[] ClosePolygon(Vector2[] polygon)
    {
        int count = Mathf.Clamp(polygon.Length, 3, PolygonScratchMaxSides);
        int slot = _closedPolygonScratchCursor++ & (PolygonScratchSlots - 1);
        Vector2[] closed = _closedPolygonScratch[count][slot];
        for (int i = 0; i < count; i++)
        {
            closed[i] = polygon[i];
        }
        closed[count] = polygon[0];
        return closed;
    }

    private GameLanguage DetectLanguage()
    {
        string locale = OS.GetLocaleLanguage();
        if (locale.StartsWith("zh", StringComparison.OrdinalIgnoreCase))
        {
            return GameLanguage.Chinese;
        }
        if (locale.StartsWith("ru", StringComparison.OrdinalIgnoreCase))
        {
            return GameLanguage.Russian;
        }
        if (locale.StartsWith("pt", StringComparison.OrdinalIgnoreCase))
        {
            return GameLanguage.PortugueseBrazil;
        }
        if (locale.StartsWith("de", StringComparison.OrdinalIgnoreCase))
        {
            return GameLanguage.German;
        }
        if (locale.StartsWith("tr", StringComparison.OrdinalIgnoreCase))
        {
            return GameLanguage.Turkish;
        }
        if (locale.StartsWith("fr", StringComparison.OrdinalIgnoreCase))
        {
            return GameLanguage.French;
        }
        if (locale.StartsWith("ja", StringComparison.OrdinalIgnoreCase))
        {
            return GameLanguage.Japanese;
        }

        return GameLanguage.English;
    }

    private string T(string key)
    {
        if (!Texts.TryGetValue(key, out LocalizedText text))
        {
            if (LocalizedOverrides.TryGetValue(key, out MultiLocalizedText localized))
            {
                return localized.For(_language);
            }
            return LocalizeMissingKey(key);
        }
        return LocalizeKey(key, text);
    }

    private string Tf(string key, params object[] args)
    {
        return string.Format(System.Globalization.CultureInfo.InvariantCulture, T(key), args);
    }

    private string TacticalSkillName(PilotKind pilot)
    {
        return pilot switch
        {
            PilotKind.Vesper => T("tactical.vesper.name"),
            PilotKind.Kairo => T("tactical.kairo.name"),
            PilotKind.Sol => T("tactical.sol.name"),
            PilotKind.Nyx => T("tactical.nyx.name"),
            PilotKind.Rook => T("tactical.rook.name"),
            PilotKind.Lyra => T("tactical.lyra.name"),
            PilotKind.Orion => T("tactical.orion.name"),
            _ => T("tactical.astra.name"),
        };
    }

    private string TacticalTipText()
    {
        return _runPilot switch
        {
            PilotKind.Vesper => T("tactical.vesper.tip"),
            PilotKind.Kairo => T("tactical.kairo.tip"),
            PilotKind.Sol => T("tactical.sol.tip"),
            PilotKind.Nyx => T("tactical.nyx.tip"),
            PilotKind.Rook => T("tactical.rook.tip"),
            PilotKind.Lyra => T("tactical.lyra.tip"),
            PilotKind.Orion => T("tactical.orion.tip"),
            _ => T("tactical.astra.tip"),
        };
    }

    private string PolarityCooldownText()
    {
        int seconds = Mathf.CeilToInt(Mathf.Max(0.0f, _polarityCooldown));
        return Tf("tactical.cooldown", seconds);
    }

    private string ChargeText()
    {
        return T("tactical.focus");
    }

    private string AbsorbText(float amount)
    {
        int value = Mathf.RoundToInt(amount);
        return Tf("tactical.clear", value);
    }

    private string CounterText()
    {
        return T("tactical.overheat");
    }

    private string ObjectiveText(RunObjective objective)
    {
        return objective.Kind switch
        {
            RunObjectiveKind.ReachWave => Tf("objective.reach_wave", objective.Target),
            RunObjectiveKind.PerfectWaves => Tf("objective.perfect_waves", objective.Target),
            RunObjectiveKind.DefeatEnemies => Tf("objective.defeat_enemies", objective.Target),
            RunObjectiveKind.AbsorbBullets => Tf("objective.absorb_bullets", objective.Target),
            RunObjectiveKind.CollectPickups => Tf("objective.collect_pickups", objective.Target),
            RunObjectiveKind.BestCombo => Tf("objective.best_combo", objective.Target),
            RunObjectiveKind.DefeatBosses => Tf("objective.defeat_bosses", objective.Target),
            RunObjectiveKind.CastTactical => Tf("objective.cast_tactical", objective.Target),
            RunObjectiveKind.CastUltimate => Tf("objective.cast_ultimate", objective.Target),
            _ => T("objective.default"),
        };
    }

    private string ObjectiveTitle(RunObjective objective)
    {
        return string.IsNullOrWhiteSpace(objective.TitleKey) ? ObjectiveText(objective) : T(objective.TitleKey);
    }

    private string ObjectiveBody(RunObjective objective)
    {
        return string.IsNullOrWhiteSpace(objective.BodyKey) ? ObjectiveText(objective) : Tf(objective.BodyKey, objective.Target);
    }

    private string ObjectiveProgressText(RunObjective objective)
    {
        return objective.Completed ? T("ui.done") : $"{objective.Progress}/{objective.Target}";
    }

    private Color ObjectiveAccent(RunObjective objective)
    {
        return objective.Completed ? Jade : objective.Accent.Lerp(CurrentSector().Accent, 0.08f + objective.Tier * 0.03f);
    }

    private string NextWavePreviewText()
    {
        int nextWave = Math.Min(_wave + 1, TotalWaves);
        int sector = Mathf.Clamp((nextWave - 1) / WavesPerSector, 0, SectorCount - 1);
        int waveInSector = ((nextWave - 1) % WavesPerSector) + 1;
        string pace = WavePaceText(WavePaceFor(nextWave));
        if (waveInSector == WavesPerSector)
        {
            string boss = Tf("boss.preview", T(Sectors[sector].NameKey));
            return Tf("next.boss", boss);
        }

        EnemyKind primary = WavePrimaryEnemyKind(sector, waveInSector);
        EnemyKind support = WaveSupportEnemyKind(sector, waveInSector);
        if (support == primary)
        {
            return Tf("next.primary", pace, EnemyName(primary));
        }

        return Tf("next.primary_support", pace, EnemyName(primary), EnemyName(support));
    }

    private string[] GuideLines()
    {
        return _guidePage switch
        {
            1 => new[]
            {
                T("guide.combo.0"),
                T("guide.combo.1"),
                T("guide.combo.2"),
                T("guide.combo.3"),
            },
            2 => new[]
            {
                T("guide.build.0"),
                T("guide.build.1"),
                T("guide.build.2"),
                T("guide.build.3"),
            },
            3 => new[]
            {
                T("guide.progress.0"),
                T("guide.progress.1"),
                T("guide.progress.2"),
                T("guide.progress.3"),
            },
            _ => new[]
            {
                T("guide.basic.0"),
                T("guide.basic.1"),
                T("guide.basic.2"),
                T("guide.basic.3"),
            },
        };
    }

    private string TutorialText(int id)
    {
        return id switch
        {
            1 => T("tutorial.1"),
            2 => T("tutorial.2"),
            3 => T("tutorial.3"),
            9 => T("tutorial.9"),
            _ => string.Empty,
        };
    }

    private void UpdateLanguageToggle()
    {
        if (!LanguageHeld() || _lastLanguage)
        {
            return;
        }

        ToggleLanguage();
    }

    private void ToggleLanguage()
    {
        int index = LanguageCycleIndex(_language);
        _language = LanguageCycle[(index + 1) % LanguageCycle.Length];
        RefreshUpgradeChoiceText();
        ApplyWindowTitle();
        SaveMetaProgress();
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

    private void UpdatePointerInputMode()
    {
        Vector2 mouse = GetGlobalMousePosition();
        if (mouse.DistanceSquaredTo(_lastMousePos) > 4.0f ||
            Input.IsMouseButtonPressed(MouseButton.Left) ||
            Input.IsMouseButtonPressed(MouseButton.Right))
        {
            _usingGamepad = false;
        }
        _lastMousePos = mouse;
    }

    private bool JoyButtonHeld(JoyButton button)
    {
        for (int device = 0; device < MaxJoypadSlots; device++)
        {
            if (Input.IsJoyButtonPressed(device, button))
            {
                _usingGamepad = true;
                return true;
            }
        }

        return false;
    }

    private float JoyAxisValue(JoyAxis axis)
    {
        float best = 0.0f;
        for (int device = 0; device < MaxJoypadSlots; device++)
        {
            float value = Input.GetJoyAxis(device, axis);
            if (Mathf.Abs(value) > Mathf.Abs(best))
            {
                best = value;
            }
        }

        return best;
    }

    private bool JoyTriggerHeld(JoyAxis axis)
    {
        float value = JoyAxisValue(axis);
        if (value <= GamepadTriggerThreshold)
        {
            return false;
        }

        _usingGamepad = true;
        return true;
    }

    private Vector2 ReadGamepadStick(JoyAxis xAxis, JoyAxis yAxis, float deadZone)
    {
        Vector2 raw = new(JoyAxisValue(xAxis), JoyAxisValue(yAxis));
        float length = raw.Length();
        if (length < deadZone)
        {
            return Vector2.Zero;
        }

        _usingGamepad = true;
        float scaled = Mathf.Clamp((length - deadZone) / (1.0f - deadZone), 0.0f, 1.0f);
        return raw.Normalized() * scaled;
    }

    private Vector2 ReadMoveInput()
    {
        Vector2 move = Vector2.Zero;
        bool keyboardMove = false;
        if (KeyDown(Key.A) || KeyDown(Key.Left))
        {
            move.X -= 1.0f;
            keyboardMove = true;
        }
        if (KeyDown(Key.D) || KeyDown(Key.Right))
        {
            move.X += 1.0f;
            keyboardMove = true;
        }
        if (KeyDown(Key.W) || KeyDown(Key.Up))
        {
            move.Y -= 1.0f;
            keyboardMove = true;
        }
        if (KeyDown(Key.S) || KeyDown(Key.Down))
        {
            move.Y += 1.0f;
            keyboardMove = true;
        }
        if (keyboardMove)
        {
            _usingGamepad = false;
        }

        move += ReadGamepadStick(JoyAxis.LeftX, JoyAxis.LeftY, GamepadStickDeadZone);
        if (JoyButtonHeld(JoyButton.DpadLeft))
        {
            move.X -= 1.0f;
        }
        if (JoyButtonHeld(JoyButton.DpadRight))
        {
            move.X += 1.0f;
        }
        if (JoyButtonHeld(JoyButton.DpadUp))
        {
            move.Y -= 1.0f;
        }
        if (JoyButtonHeld(JoyButton.DpadDown))
        {
            move.Y += 1.0f;
        }

        return move.LengthSquared() > 1.0f ? move.Normalized() : move;
    }

    private bool ConfirmHeld()
    {
        return KeyDown(Key.Enter) || JoyButtonHeld(JoyButton.A);
    }

    private bool StartHeld()
    {
        return KeyDown(Key.Enter) || JoyButtonHeld(JoyButton.Start);
    }

    private bool CancelHeld()
    {
        return KeyDown(Key.Escape) || JoyButtonHeld(JoyButton.B) || JoyButtonHeld(JoyButton.Back);
    }

    private bool PauseHeld()
    {
        return KeyDown(Key.Escape) || JoyButtonHeld(JoyButton.Start) || JoyButtonHeld(JoyButton.Back);
    }

    private bool TacticalHeld()
    {
        return KeyDown(Key.Space) || JoyButtonHeld(JoyButton.X) || JoyButtonHeld(JoyButton.RightShoulder);
    }

    private bool UltimateHeld()
    {
        return KeyDown(Key.F) || KeyDown(Key.E) || JoyButtonHeld(JoyButton.Y) || JoyTriggerHeld(JoyAxis.TriggerRight);
    }

    private bool DashHeld()
    {
        return KeyDown(Key.Shift) || Input.IsMouseButtonPressed(MouseButton.Right) || JoyButtonHeld(JoyButton.A) || JoyButtonHeld(JoyButton.LeftShoulder) || JoyTriggerHeld(JoyAxis.TriggerLeft);
    }

    private bool RerollHeld()
    {
        return KeyDown(Key.R) || JoyButtonHeld(JoyButton.X);
    }

    private bool MetaHeld()
    {
        return KeyDown(Key.U) || JoyButtonHeld(JoyButton.Y);
    }

    private bool LanguageHeld()
    {
        return KeyDown(Key.L) || JoyButtonHeld(JoyButton.RightStick);
    }

    private bool SettingsShortcutHeld()
    {
        return JoyButtonHeld(JoyButton.X);
    }

    private void UpdateGamepadNavigation(float dt)
    {
        _gamepadNavCooldown = Mathf.Max(0.0f, _gamepadNavCooldown - dt);
        if (RawGamepadNavX() == 0)
        {
            _gamepadLastNavX = 0;
        }
        if (RawGamepadNavY() == 0)
        {
            _gamepadLastNavY = 0;
        }

        _gamepadFocusVisible = false;
    }

    private int ConsumeGamepadNavX()
    {
        int raw = RawGamepadNavX();
        if (raw == 0)
        {
            return 0;
        }

        if (_gamepadNavCooldown > 0.0f && raw == _gamepadLastNavX)
        {
            return 0;
        }

        _gamepadLastNavX = raw;
        _gamepadLastNavY = 0;
        _gamepadNavCooldown = GamepadNavRepeat;
        _usingGamepad = true;
        return raw;
    }

    private int ConsumeGamepadNavY()
    {
        int raw = RawGamepadNavY();
        if (raw == 0)
        {
            return 0;
        }

        if (_gamepadNavCooldown > 0.0f && raw == _gamepadLastNavY)
        {
            return 0;
        }

        _gamepadLastNavY = raw;
        _gamepadLastNavX = 0;
        _gamepadNavCooldown = GamepadNavRepeat;
        _usingGamepad = true;
        return raw;
    }

    private int RawGamepadNavX()
    {
        if (JoyButtonHeld(JoyButton.DpadLeft))
        {
            return -1;
        }
        if (JoyButtonHeld(JoyButton.DpadRight))
        {
            return 1;
        }

        float x = JoyAxisValue(JoyAxis.LeftX);
        if (x < -GamepadNavThreshold)
        {
            _usingGamepad = true;
            return -1;
        }
        if (x > GamepadNavThreshold)
        {
            _usingGamepad = true;
            return 1;
        }

        return 0;
    }

    private int RawGamepadNavY()
    {
        if (JoyButtonHeld(JoyButton.DpadUp))
        {
            return -1;
        }
        if (JoyButtonHeld(JoyButton.DpadDown))
        {
            return 1;
        }

        float y = JoyAxisValue(JoyAxis.LeftY);
        if (y < -GamepadNavThreshold)
        {
            _usingGamepad = true;
            return -1;
        }
        if (y > GamepadNavThreshold)
        {
            _usingGamepad = true;
            return 1;
        }

        return 0;
    }

    private void SetGamepadFocus(Rect2 rect)
    {
        _gamepadFocusRect = rect;
        _gamepadFocusVisible = _usingGamepad;
    }

    private bool IsGamepadFocused(Rect2 rect)
    {
        return _gamepadFocusVisible &&
            _gamepadFocusRect.Position.DistanceSquaredTo(rect.Position) < 1.0f &&
            _gamepadFocusRect.Size.DistanceSquaredTo(rect.Size) < 1.0f;
    }

    private void CaptureButtons()
    {
        _lastStart = StartHeld();
        _lastConfirm = ConfirmHeld();
        _lastCancel = CancelHeld();
        _lastPause = PauseHeld();
        _lastToggle = TacticalHeld();
        _lastNova = UltimateHeld();
        _lastDash = DashHeld();
        _lastClick = Input.IsMouseButtonPressed(MouseButton.Left);
        _lastRestart = ConfirmHeld() || StartHeld() || Input.IsMouseButtonPressed(MouseButton.Left);
        _lastOne = KeyDown(Key.Key1);
        _lastTwo = KeyDown(Key.Key2);
        _lastThree = KeyDown(Key.Key3);
        _lastFour = KeyDown(Key.Key4);
        _lastFive = KeyDown(Key.Key5);
        _lastSix = KeyDown(Key.Key6);
        _lastSeven = KeyDown(Key.Key7);
        _lastEight = KeyDown(Key.Key8);
        _lastNine = KeyDown(Key.Key9);
        _lastReroll = RerollHeld();
        _lastLanguage = LanguageHeld();
        _lastMeta = MetaHeld();
        _lastBack = CancelHeld();
        _lastSettingsShortcut = SettingsShortcutHeld();
        _lastTitleLeft = KeyDown(Key.Left) || KeyDown(Key.A);
        _lastTitleRight = KeyDown(Key.Right) || KeyDown(Key.D);
        _lastSettingsLeft = KeyDown(Key.Left) || KeyDown(Key.A);
        _lastSettingsRight = KeyDown(Key.Right) || KeyDown(Key.D);
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
            VolumeDb = -15.0f,
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
            float sample = MusicSample() * _musicVolume + SfxSample() * _sfxVolume;
            sample = SoftClip(sample, 0.52f);
            _musicPlayback.PushFrame(new Vector2(sample, sample));
            _musicClock += 1.0f / SampleRate;
        }
    }

    private float MusicSample()
    {
        int[] scale = { 0, 3, 5, 7, 10, 12, 15, 17 };
        float tempo = _mode == GameMode.Title ? 82.0f : 96.0f;
        float beat = _musicClock * (tempo / 60.0f);
        int step = (int)MathF.Floor(beat * 2.0f) & 7;
        int arpeggio = (int)MathF.Floor(beat * 4.0f) & 3;
        float root = _mode == GameMode.GameOver ? 48.999f : _mode == GameMode.Victory ? 73.416f : 55.0f;
        float melodyNote = root * MathF.Pow(2.0f, (scale[(step + arpeggio) & 7] + 12) / 12.0f);
        float harmonyNote = root * MathF.Pow(2.0f, (scale[(step + 2) & 7] + 7) / 12.0f);
        float bassNote = root * MathF.Pow(2.0f, (scale[step] - 12) / 12.0f);
        float beatFraction = beat - MathF.Floor(beat);
        float phrase = 0.55f + 0.45f * MathF.Sin(beat * Mathf.Tau * 0.125f);
        float gate = beatFraction < 0.68f ? 1.0f : Mathf.Clamp(1.0f - (beatFraction - 0.68f) / 0.32f, 0.0f, 1.0f);
        float bass = ChipTriangle(bassNote * _musicClock) * 0.038f;
        float melody = SoftChipPulse(melodyNote * _musicClock, 0.36f) * 0.021f * gate * phrase;
        float harmony = ChipTriangle(harmonyNote * _musicClock) * 0.009f * gate * (1.0f - phrase * 0.25f);
        float modeGain = _mode == GameMode.Title ? 0.46f : _mode == GameMode.GameOver ? 0.4f : 0.66f;
        return (bass + melody + harmony) * modeGain;
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
            float attack = Mathf.Clamp(t / 0.12f, 0.0f, 1.0f);
            float decay = 1.0f - t;
            decay *= decay;
            float env = attack * decay;
            float freq = MathF.Max(20.0f, voice.Frequency + voice.Sweep * t);
            float wave = voice.Wave switch
            {
                0 => MathF.Sin(Mathf.Tau * freq * voice.Age) * 0.58f,
                1 => SoftChipPulse(freq * voice.Age, 0.46f) * 0.46f + ChipTriangle(freq * 0.5f * voice.Age) * 0.12f,
                _ => ChipTriangle(freq * voice.Age) * 0.42f,
            };
            float noise = (_rng.Randf() * 2.0f - 1.0f) * voice.Noise;
            output += (wave * (1.0f - voice.Noise) + noise) * env * voice.Volume * 0.72f;
        }
        return SoftClip(output, 0.34f);
    }

    private void PlaySfx(float frequency, float sweep, float life, float volume, float noise, int wave)
    {
        if (_voices.Count >= MaxSfxVoices)
        {
            _voices.RemoveAt(0);
        }

        _voices.Add(new SfxVoice
        {
            Frequency = Mathf.Clamp(frequency * 0.9f, 42.0f, 860.0f),
            Sweep = Mathf.Clamp(sweep * 0.36f, -170.0f, 170.0f),
            Life = Mathf.Clamp(life * 0.68f, 0.05f, 0.68f),
            Volume = Mathf.Clamp(volume * SfxGlobalGain, 0.0f, 0.2f),
            Noise = Mathf.Clamp(noise * SfxNoiseScale, 0.0f, 0.05f),
            Wave = wave,
        });
    }

    private static float SoftChipPulse(float phase, float duty)
    {
        phase -= MathF.Floor(phase);
        float edge = phase < duty ? 1.0f : -1.0f;
        return MathF.Tanh(edge * 1.35f + MathF.Sin(phase * Mathf.Tau) * 0.35f) * 0.72f;
    }

    private static float ChipTriangle(float phase)
    {
        phase -= MathF.Floor(phase);
        return 1.0f - 4.0f * MathF.Abs(phase - 0.5f);
    }

    private static float SoftClip(float value, float limit)
    {
        return MathF.Tanh(value / limit) * limit;
    }
}
