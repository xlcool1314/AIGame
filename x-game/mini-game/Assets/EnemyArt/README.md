# Enemy Art Folder Guide

这个目录用于放敌人主体美术图。目录名已经标注了敌人等级，以及是否是 Boss。

建议素材规格：
- 普通敌人：透明 PNG，建议 192x192 或 256x256。
- 精英敌人：可以放独立图，也可以只放强化版颜色/轮廓图。
- Boss：透明 PNG，建议 512x512 或 768x768。
- 图像主体尽量居中，周围留少量透明边距。
- 如果希望代码染色，素材可以偏白/灰；如果希望固定美术色彩，就直接放成品色。

普通敌人：

| 目录 | 中文名 | 强度 | 首次出现 | 玩法定位 |
|---|---|---:|---:|---|
| `Normal/LV01_Basic/LV01_Chaser` | 追猎者 | LV01 | 第 1 波 | 直线追击，发射简单瞄准弹 |
| `Normal/LV01_Basic/LV01_Weaver` | 织弹者 | LV01 | 第 2 波 | 横向游走，发射小扇形弹 |
| `Normal/LV02_EarlyMechanic/LV02_Turret` | 炮台 | LV02 | 第 5 波 | 保持距离，制造环形弹幕 |
| `Normal/LV02_EarlyMechanic/LV02_Drifter` | 弧行者 | LV02 | 第 7 波 | 绕弧移动，从侧面发射弹幕 |
| `Normal/LV02_EarlyMechanic/LV02_Mine` | 星雷 | LV02 | 第 10 波 | 缓慢漂移，爆出放射弹幕 |
| `Normal/LV03_MidThreat/LV03_Lance` | 长枪手 | LV03 | 第 11 波 | 远距离发射高速重弹 |
| `Normal/LV03_MidThreat/LV03_Splitter` | 分裂体 | LV03 | 第 15 波 | 被击破后会分裂出小敌人 |
| `Normal/LV03_MidThreat/LV03_Siren` | 鸣标 | LV03 | 第 19 波 | 蓄能后释放慢速红环 |
| `Normal/LV04_LateThreat/LV04_Shard` | 碎片 | LV04 | 第 25 波 | 高速骚扰，连续发射小弹 |
| `Normal/LV04_LateThreat/LV04_Harrier` | 掠袭者 | LV04 | 第 28 波 | 高速突进，停顿后短促连射 |
| `Normal/LV05_HeavyAnchor/LV05_Bulwark` | 壁垒 | LV05 | 第 20 波 | 慢速高血量，是本波核心目标 |
| `Normal/LV05_HeavyAnchor/LV05_Warden` | 守望者 | LV05 | 第 27 波 | 召唤援兵并压制空间 |

精英敌人：

`Elite/` 目录与普通敌人对应，用于放精英版外观。精英版不一定要完全重画，可以只强化轮廓、核心、装甲、发光区域。

Boss：

| 目录 | Boss 等级 | 设计建议 |
|---|---:|---|
| `Boss/BOSS_LV01_Choir` | BOSS LV01 | 入门 Boss，轮廓清晰，压迫感适中 |
| `Boss/BOSS_LV01_Prism` | BOSS LV01 | 棱镜/分裂/折射感 |
| `Boss/BOSS_LV02_Swarm` | BOSS LV02 | 群体/召唤/蜂巢感 |
| `Boss/BOSS_LV02_Forge` | BOSS LV02 | 熔炉/重火力/装甲感 |
| `Boss/BOSS_LV03_Rift` | BOSS LV03 | 裂隙/黑洞/空间撕裂感 |
| `Boss/BOSS_LV03_Mirror` | BOSS LV03 | 镜像/对称/反射感 |
| `Boss/BOSS_LV04_Tempest` | BOSS LV04 | 风暴/旋转/高速感 |
| `Boss/BOSS_LV04_Bastion` | BOSS LV04 | 堡垒/屏障/厚重感 |
| `Boss/BOSS_LV05_Serpent` | BOSS LV05 | 蛇形/缠绕/连续压迫感 |
| `Boss/BOSS_LV05_Oracle` | BOSS LV05 | 预判/狙击/终盘压迫感 |

`_AtlasOutput/` 是之后自动生成敌人图集时的输出目录。
