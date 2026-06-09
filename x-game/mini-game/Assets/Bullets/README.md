# Enemy Bullet Art Folder Guide

这个目录用于放敌人子弹和 Boss 弹幕素材。

建议素材规格：
- 普通子弹：透明 PNG，建议 32x32 或 64x64。
- 重弹/环弹：透明 PNG，建议 64x64 或 96x96。
- Boss 子弹：透明 PNG，建议 64x64 或 128x128。
- 为了保持战斗可读性，敌方子弹最终仍建议统一偏红色。素材可以做成白色/灰度，后续由代码染红。
- 子弹轮廓要清楚，中心不要过亮到看不见边界。

普通敌人子弹：

| 目录 | 强度 | 用途 |
|---|---:|---|
| `Normal/LV01_AimedSmall` | LV01 | 追猎者等基础瞄准弹 |
| `Normal/LV02_FanSpread` | LV02 | 织弹者、小扇形弹幕 |
| `Normal/LV03_RingBurst` | LV03 | 炮台、星雷、鸣标的环形弹 |
| `Normal/LV04_HeavyLance` | LV04 | 长枪手、重弹、直线压迫弹 |
| `Normal/LV05_SpecialMechanic` | LV05 | 高级机制弹，例如分裂、召唤、空间压制 |

精英敌人子弹：

| 目录 | 强度 | 用途 |
|---|---:|---|
| `Elite/LV02_EliteAimed` | LV02 | 精英瞄准弹 |
| `Elite/LV03_EliteFan` | LV03 | 精英扇形弹 |
| `Elite/LV04_EliteRing` | LV04 | 精英环形弹 |
| `Elite/LV05_EliteSpecial` | LV05 | 精英特殊机制弹 |

Boss 子弹：

| 目录 | 对应 Boss Pattern |
|---|---|
| `Boss/BOSS_AimedFan` | AimedFan |
| `Boss/BOSS_SpiralRing` | SpiralRing |
| `Boss/BOSS_HeavyLance` | HeavyLance |
| `Boss/BOSS_SummonWing` | SummonWing |
| `Boss/BOSS_HazardFan` | HazardFan |
| `Boss/BOSS_ReverseSpiral` | ReverseSpiral |
| `Boss/BOSS_WardenCall` | WardenCall |
| `Boss/BOSS_CrossBloom` | CrossBloom |
| `Boss/BOSS_MineDrift` | MineDrift |
| `Boss/BOSS_MirrorFork` | MirrorFork |
| `Boss/BOSS_TempestWheel` | TempestWheel |
| `Boss/BOSS_BastionWall` | BastionWall |
| `Boss/BOSS_SerpentCoil` | SerpentCoil |
| `Boss/BOSS_OracleSnipe` | OracleSnipe |

`_AtlasOutput/` 是之后自动生成子弹图集时的输出目录。
