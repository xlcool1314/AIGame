#nullable enable

using Godot;

public partial class MainGame
{
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
        Russian,
        PortugueseBrazil,
        German,
        Turkish,
        French,
        Japanese,
    }

    private enum DisplayResolutionPreset
    {
        R1280x720,
        R1600x900,
        R1920x1080,
        R2560x1440,
    }

    private enum VisualQuality
    {
        Low,
        Medium,
        High,
        Ultra,
    }

    private enum GameDifficulty
    {
        Cruise,
        Storm,
        Eclipse,
    }

    private enum PilotKind
    {
        Astra,
        Vesper,
        Kairo,
        Sol,
        Nyx,
        Rook,
        Lyra,
        Orion,
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
        StarterModule,
        PilotCodex,
        ComboEngine,
        EliteAnalyzer,
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

        public string ForBaseLanguage(GameLanguage language)
        {
            return language == GameLanguage.Chinese ? Chinese : English;
        }
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
        CastTactical,
        CastUltimate,
    }

    private enum WavePaceKind
    {
        Standard,
        Swarm,
        Elite,
        Recovery,
        Pressure,
        Boss,
    }

    private enum BossArchetype
    {
        Choir,
        Prism,
        Swarm,
        Forge,
        Rift,
        Mirror,
        Tempest,
        Bastion,
        Serpent,
        Oracle,
    }

    private enum BossPatternKind
    {
        AimedFan,
        SpiralRing,
        HeavyLance,
        SummonWing,
        HazardFan,
        ReverseSpiral,
        WardenCall,
        CrossBloom,
        MineDrift,
        MirrorFork,
        TempestWheel,
        BastionWall,
        SerpentCoil,
        OracleSnipe,
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
        ChainRelay,
        FractalSplit,
        SolarThesis,
        EmergencyRepair,
        OneWaveOverdrive,
        GlassCannon,
        BountyContract,
        BulletTransmute,
        HarmonicMap,
        PulseMagazine,
        ExecutionMark,
        StasisField,
        MagnetizedCore,
        RicochetMatrix,
        SeekerRack,
        ShieldRebound,
        ShadowClone,
        HeavySlug,
        PinballRounds,
        GyroStabilizer,
        VectorThrusters,
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
        NyxOrbit,
        NyxSingularity,
        NyxEventHorizon,
        NyxGravityCantor,
        RookBulwarkCore,
        RookSiegeBattery,
        RookAegisRelay,
        RookCitadelProtocol,
        LyraResonanceChord,
        LyraTempoBloom,
        LyraHarmonicCascade,
        LyraEncoreField,
        OrionCometSpear,
        OrionDeadeyeMark,
        OrionStarfallQuiver,
        OrionPerihelionVector,
        AstraPrismOrbit,
        VesperOverchargeRail,
        KairoHunterWing,
        SolIgnitionWave,
        NyxVoidTax,
        RookCounterBattery,
        LyraBeatTrigger,
        OrionMarkedPrey,
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
        public float DropMultiplier = 1.0f;
        public int SplitDepth;
        public int LastHitChainDepth;
        public int LastHitSplitDepth;
        public bool Elite;
        public float Armor;
        public float DashCooldown;
        public float DashWarmup;
        public float DashTime;
        public Vector2 DashDir;
        public BossArchetype BossArchetype;
        public int BossLastPattern;
        public BossPatternKind BossIntent;
        public float BossIntentPulse;
        public int BossPhase;
        public float BossGuard;
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
        public int Polarity;
        public bool FromPlayer;
        public int Pierce;
        public int ChainDepth;
        public int SplitDepth;
        public bool Rift;
        public bool Grazed;
        public int Homing;
        public int Bounces;
        public bool Heavy;
        public bool Shadow;
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

    private sealed class OrbiterVisual
    {
        public Vector2 Pos;
        public Vector2 Vel;
        public Vector2 Facing = Vector2.Up;
        public float Phase;
        public float CommandPulse;
        public bool Active;
    }

    private sealed class DroneCommandCue
    {
        public Vector2 Pos;
        public Vector2 Vel;
        public Vector2 Facing = Vector2.Up;
        public Color Color;
        public float Life;
        public float MaxLife;
        public float Scale;
    }

    private sealed class Shockwave
    {
        public Vector2 Center;
        public float Radius;
        public Color Color;
        public float Life;
        public float MaxLife;
    }

    private sealed class DamageText
    {
        public Vector2 Pos;
        public string Text = string.Empty;
        public Color Color;
        public float Life;
        public float MaxLife;
        public float Size;
        public bool ComboPop;
    }

    private sealed class TextCue
    {
        public string Text = string.Empty;
        public Vector2 Pos;
        public Color Color;
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

    private sealed class HazardField
    {
        public Vector2 Center;
        public Color Color;
        public float Radius;
        public float Life;
        public float MaxLife;
        public float Warmup;
        public float Damage;
        public float Pull;
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
        public string TitleKey = string.Empty;
        public string BodyKey = string.Empty;
        public Color Accent = Colors.White;
        public int Tier;
    }

    private sealed class PendingSpawn
    {
        public EnemyKind Kind;
        public int Polarity;
        public float RewardBoost;
        public bool Elite;
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
}
