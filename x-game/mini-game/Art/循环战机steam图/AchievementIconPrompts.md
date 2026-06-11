# 循环战机 Steam 成就图标生成需求

用途：给 AI 图像生成工具或美术外包使用，批量生成 25 个 Steam 成就图标。

## 统一美术规范

- 画面比例：正方形。
- 建议源文件：1024x1024 PNG，后续再缩放上传到 Steam。
- 风格：极简科幻、霓虹线框、太空战机、弹幕、循环轨道、能量核心。
- 配色：黑色深空底，主色青色，辅色金色、洋红、白色高光。
- 构图：中心主体清晰，占画面 65%-80%；外围可有细线圆环、轨道、星点、弹幕点阵。
- 可读性：必须在 64x64 小尺寸下仍能看清轮廓。
- 禁止：文字、数字、复杂背景、大面积渐变、写实人物、血腥、过多小细节、游戏截图感。
- 锁定版图标：同构图，整体灰阶、低亮度，只保留少量暗蓝轮廓光。
- 文件命名建议：`API_NAME.png` 和 `API_NAME_LOCKED.png`。

统一提示词前缀：

```text
square Steam achievement icon, minimalist sci-fi arcade roguelite style, dark deep space background, clean neon line art, cyan gold magenta accents, high contrast, readable at 64px, central symbol, subtle circular orbit frame, no text, no numbers, no watermark
```

统一反向提示词：

```text
text, letters, numbers, logo text, watermark, realistic human, gore, cluttered details, low contrast, blurry, screenshot, UI panel, excessive particles, tiny unreadable symbols
```

## 成就图标列表

### 01. FIRST_SORTIE

- 中文名：首次出击
- 图标概念：一架小战机从暗色圆环中起飞，尾部只有一束简洁青光。
- AI 提示词：

```text
square Steam achievement icon, minimalist sci-fi arcade roguelite style, dark deep space background, a sleek small starfighter launching upward from a thin circular orbit gate, cyan engine trail, tiny stars, clean silhouette, gold highlight on cockpit, high contrast, readable at 64px, no text
```

### 02. FIRST_RETURN

- 中文名：完成一局
- 图标概念：战机返回基地环，环形入口像一个完成闭环。
- AI 提示词：

```text
square Steam achievement icon, dark space, a starfighter entering a glowing circular docking ring, completed loop symbol, cyan rim light, soft gold landing beacon, minimal line art, strong central silhouette, no text
```

### 03. REACH_WAVE_10

- 中文名：初入循环
- 图标概念：一层波纹环围住战机，外圈有少量弹幕点。
- AI 提示词：

```text
square achievement icon, starfighter at center surrounded by one clean neon wave ring, sparse red bullet dots outside the ring, cyan and white highlights, dark space background, simple readable symbol, no text
```

### 04. REACH_WAVE_20

- 中文名：穿过中段
- 图标概念：两层交错的波纹环，战机正在穿过弹幕缝隙。
- AI 提示词：

```text
square achievement icon, starfighter slipping through two concentric bullet-wave rings, dark deep space, cyan orbit lines, gold speed accent, red bullet dots arranged with a clear gap, minimalist, no text
```

### 05. REACH_WAVE_30

- 中文名：深入星海
- 图标概念：三层星海轨道向外展开，中间战机更坚定。
- AI 提示词：

```text
square Steam achievement icon, central starfighter deep inside three luminous orbit rings, dense but clean starfield, cyan outer rings, magenta danger sparks, heroic high contrast silhouette, no text
```

### 06. CLEAR_CRUISE

- 中文名：巡航通关
- 图标概念：青色胜利核心被战机击穿，整体偏轻快。
- AI 提示词：

```text
square achievement icon, a cyan energy core cracked open by a small starfighter, clean victory burst, dark space, bright cyan and white, subtle gold orbit frame, simple arcade sci-fi style, no text
```

### 07. CLEAR_STORM

- 中文名：风暴通关
- 图标概念：金色风暴旋涡中，战机冲出中心。
- AI 提示词：

```text
square achievement icon, starfighter breaking through a golden storm spiral, dark space background, cyan ship outline, gold lightning-like orbit arcs, energetic but clean, high readability, no text
```

### 08. CLEAR_ECLIPSE

- 中文名：日蚀通关
- 图标概念：黑日蚀核心后方有洋红日冕，战机切过边缘。
- AI 提示词：

```text
square achievement icon, black eclipse core with magenta corona, starfighter cutting across the edge, cyan blade-like trail, dark cosmic background, dramatic final challenge symbol, no text
```

### 09. FIRST_BOSS_KILL

- 中文名：击破核心
- 图标概念：一个大型 Boss 核心被裂纹分成两半。
- AI 提示词：

```text
square achievement icon, large mechanical boss core split by a clean cyan strike, cracked circular armor, small starfighter silhouette below, dark space, magenta damage glow, no text
```

### 10. BOSS_HUNTER_10

- 中文名：Boss 猎手
- 图标概念：战机前方悬浮三枚被击破的核心碎片。
- AI 提示词：

```text
square achievement icon, starfighter facing a cluster of broken boss core trophies, three cracked circular cores, cyan and gold neon edges, dark space, clean trophy-like composition, no text
```

### 11. BOSS_HUNTER_50

- 中文名：核心粉碎者
- 图标概念：巨大的核心碎片环绕成王冠状。
- AI 提示词：

```text
square achievement icon, crown made of shattered boss cores around a central starfighter, intense magenta and gold highlights, cyan orbit frame, dark space, elite trophy feel, no text
```

### 12. COMBO_50

- 中文名：连击启动
- 图标概念：战机周围形成快速闭合的青色连锁环。
- AI 提示词：

```text
square achievement icon, starfighter encircled by a fast cyan combo chain ring, small linked bullet sparks, gold motion accents, dark space, clean loop energy symbol, no text
```

### 13. COMBO_100

- 中文名：高速循环
- 图标概念：双重高速轨道环形成发光无限循环感。
- AI 提示词：

```text
square achievement icon, high speed double orbit loop around a starfighter, cyan and gold trails forming an infinity-like circular motion, dark starfield, intense but simple, no text
```

### 14. PERFECT_WAVE

- 中文名：无伤一波
- 图标概念：透明护盾完整包住战机，红色子弹被挡在外圈。
- AI 提示词：

```text
square achievement icon, pristine transparent cyan shield around a starfighter, red bullet dots stopped outside the shield, clean dark background, white highlight, flawless defense symbol, no text
```

### 15. PERFECT_WAVES_20

- 中文名：精准驾驶
- 图标概念：多层护盾像莲花一样展开，战机无伤穿过弹幕。
- AI 提示词：

```text
square achievement icon, starfighter protected by layered cyan shield petals, red bullets curving around without touching, elegant precise dodging symbol, gold small highlights, no text
```

### 16. KILLS_1000

- 中文名：千机坠落
- 图标概念：战机前方一片敌机剪影碎片，中心清晰爆点。
- AI 提示词：

```text
square achievement icon, starfighter firing into a field of small broken enemy silhouettes, central cyan explosion, dark space, red enemy fragments, clean readable arcade action, no text
```

### 17. KILLS_10000

- 中文名：万机归零
- 图标概念：巨大的爆裂圆环吞没大量敌机碎片，战机居中。
- AI 提示词：

```text
square achievement icon, massive circular annihilation burst around a central starfighter, many tiny enemy fragments reduced to sparks, cyan white core, gold outer shockwave, no text
```

### 18. PICKUPS_1000

- 中文名：回收专家
- 图标概念：灰色经验方块被磁力线吸向战机。
- AI 提示词：

```text
square achievement icon, gray square experience shards being pulled by curved cyan magnetic lines toward a starfighter, dark space, clean collection symbol, subtle gold spark, no text
```

### 19. ABSORB_500

- 中文名：弹幕回收
- 图标概念：红色子弹撞入青色吸收环后变成能量粒子。
- AI 提示词：

```text
square achievement icon, red bullet dots flowing into a cyan absorption ring and transforming into blue energy particles, starfighter silhouette at center, dark background, clean visual contrast, no text
```

### 20. UNLOCK_VESPER

- 中文名：解锁暮轨
- 图标概念：长线轨道炮式战机剪影，中心有一条贯穿光轨。
- AI 提示词：

```text
square achievement icon, sleek railgun starfighter silhouette, one straight magenta beam line passing through the center, cyan edge glow, dark space, elegant precision weapon identity, no text
```

### 21. UNLOCK_KAIRO

- 中文名：解锁环序
- 图标概念：无人机环绕主机形成机械网络。
- AI 提示词：

```text
square achievement icon, central command fighter with three small drones in orbit, cyan network lines linking them, dark space, blue technical glow, clean drone swarm identity, no text
```

### 22. UNLOCK_ORION

- 中文名：解锁日冕
- 图标概念：橙金日冕包围高速战机，像最终机体登场。
- AI 提示词：

```text
square achievement icon, sharp starfighter emerging from an orange-gold solar corona, cyan cockpit light, dark cosmic background, final pilot unlock feeling, clean high contrast, no text
```

### 23. UNLOCK_ALL_PILOTS

- 中文名：全员出航
- 图标概念：八个简化战机轮廓围成一个圆，中间是循环核心。
- AI 提示词：

```text
square achievement icon, eight small distinct starfighter silhouettes arranged in a circular formation around a glowing loop core, cyan gold magenta accents, dark space, collection complete symbol, no text
```

### 24. META_FIRST_UPGRADE

- 中文名：第一次改装
- 图标概念：战机旁边出现一个发光模块插入核心。
- AI 提示词：

```text
square achievement icon, small glowing upgrade module docking into a starfighter core, cyan circuit lines, gold activation spark, dark sci-fi workbench feel without UI, clean central symbol, no text
```

### 25. META_ALL_MAX

- 中文名：完全改装
- 图标概念：完整升级树/模块环全部点亮，中央战机发光。
- AI 提示词：

```text
square achievement icon, fully upgraded starfighter at center, complete ring of luminous modules around it all activated, cyan gold magenta lights, dark space, ultimate completion trophy feel, no text
```

## 生成后的检查标准

- 缩小到 64x64 后仍能看清主体。
- 每张图不要出现文字和数字。
- 25 张图必须明显属于同一个游戏。
- 颜色可以区分成就类型：
  - 进度类：青色为主。
  - 难度通关：巡航青色、风暴金色、日蚀洋红。
  - Boss 类：红/洋红和裂纹核心。
  - 连击/无伤：青色护盾和金色速度线。
  - 收集/局外成长：灰色方块、模块、环形升级节点。
- 锁定版统一做灰阶暗色，不要重新设计构图。

## Steam 后台备注

Steam 官方要求成就名称和图标适合全年龄展示；所以图标不要做血腥、恐怖脸、挑衅手势或敏感符号。
