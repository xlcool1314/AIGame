# AIGame（Godot 4 + C# 数据驱动卡牌战斗原型）

这是一个基于 **Godot 4 + C#** 的最小可运行战斗原型，提供类似《杀戮尖塔》的单场战斗循环：抽牌、出牌、结束回合、敌人行动。

## 当前实现

- 全部核心逻辑使用 C#：`GameData.cs` / `BattleEngine.cs` / `BattleScene.cs`
- 数据驱动：卡牌 / 敌人 / 初始牌组来源于 JSON 配置
- 战斗机制：生命、格挡、能量、抽牌堆、弃牌堆、手牌
- 行动类型：`damage` / `block` / `draw`
- 基础 UI：状态区、敌人意图、手牌按钮、战斗日志

## 目录结构

- `x-game/project.godot`：Godot 项目入口（主场景 + 分辨率）
- `x-game/scenes/BattleScene.tscn`：战斗场景 UI
- `x-game/scripts/GameData.cs`：JSON 读取与数据索引
- `x-game/scripts/BattleEngine.cs`：战斗状态机与规则
- `x-game/scripts/BattleScene.cs`：UI 渲染与输入处理
- `x-game/data/cards.json`：卡牌定义
- `x-game/data/enemies.json`：敌人定义（含意图循环）
- `x-game/data/decks.json`：初始牌组

## 运行方式

1. 使用 Godot 4.x（Mono / .NET）打开 `x-game/project.godot`。
2. 确保本地安装 .NET SDK。
3. 运行项目，进入战斗场景。

## 后续建议

1. 增加更多动作类型（例如 `weak`、`vulnerable`、`poison`）。
2. 扩展战斗奖励与局外成长。
3. 使用资源表或编辑器工具提升配置效率。

## 显示稳定性说明

- 已针对高分辨率屏幕（如 2560x1440）调整布局：日志区不再使用 `fit_content` 自适应高度，改为固定拉伸比例，避免垂直布局在刷新日志时出现抖动/闪烁。
- 项目窗口拉伸模式改为 `viewport`，并显式开启 VSync，减少高分辨率下的画面撕裂和闪烁概率。
