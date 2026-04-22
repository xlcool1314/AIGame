# AIGame（Godot 4 + C# 数据驱动卡牌战斗框架）

这是一个用 **Godot 4 + C#** 编写的最小可运行框架，目标是快速搭建类似《杀戮尖塔》的单场战斗原型。

## 当前版本特性

- 全部逻辑脚本已切换为 C#：`GameData.cs` / `BattleEngine.cs` / `BattleScene.cs`
- 数据驱动：卡牌 / 敌人 / 初始牌组都来自 `data/*.json`
- 回合制战斗：玩家回合 + 敌人意图循环
- 战斗要素：手牌、抽牌堆、弃牌堆、能量、格挡、日志
- 支持动作：`damage` / `block` / `draw`
- UI 目标分辨率：**1920 x 1080**，并使用深色幻想风样式

## 目录结构

- `project.godot`：项目入口与分辨率配置（1920x1080）
- `scenes/BattleScene.tscn`：战斗主场景（含 UI 样式）
- `scripts/GameData.cs`：JSON 数据加载与索引
- `scripts/BattleEngine.cs`：核心战斗逻辑（与 UI 解耦）
- `scripts/BattleScene.cs`：UI 与输入层
- `data/cards.json`：卡牌配置
- `data/enemies.json`：敌人配置（含意图序列）
- `data/decks.json`：初始牌组配置

## 如何运行

1. 用 Godot 4.x（Mono/.NET 版本）打开项目。
2. 确保本机已安装 .NET SDK（Godot C# 必需）。
3. 点击运行，主场景为 `scenes/BattleScene.tscn`。

## 如何扩展

### 1) 新增卡牌
在 `data/cards.json` 中添加 `cards` 条目并配置 `actions`。

### 2) 新增敌人
在 `data/enemies.json` 中增加敌人与 `intents`（敌人按顺序循环意图）。

### 3) 增加动作类型
在 `scripts/BattleEngine.cs` 的 `ApplyActions()` 中新增 `switch` 分支。
