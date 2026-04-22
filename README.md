# AIGame（Godot 数据驱动卡牌战斗框架）

这是一个用 **Godot 4** 编写的最小可运行框架，目标是快速搭建类似《杀戮尖塔》的单场战斗原型。

## 特性

- 数据驱动：卡牌 / 敌人 / 初始牌组都来自 `data/*.json`
- 回合制战斗：玩家回合 + 敌人意图循环
- 卡牌区、抽牌堆、弃牌堆、能量、格挡、日志完整联动
- 可扩展动作系统：目前支持 `damage` / `block` / `draw`

## 目录结构

- `project.godot`：项目入口配置
- `scenes/BattleScene.tscn`：战斗主场景
- `scripts/game_data.gd`：JSON 数据加载与索引
- `scripts/battle_engine.gd`：核心战斗逻辑（与 UI 解耦）
- `scripts/battle_scene.gd`：UI 与输入层
- `data/cards.json`：卡牌配置
- `data/enemies.json`：敌人配置（含意图序列）
- `data/decks.json`：初始牌组配置

## 如何运行

1. 用 Godot 4.x 打开本项目根目录。
2. 点击运行（主场景已配置为 `scenes/BattleScene.tscn`）。
3. 在界面中点击手牌打出卡牌，点击“结束回合”让敌人行动。

## 如何扩展

### 1) 新增卡牌

在 `data/cards.json` 的 `cards` 数组里新增条目：

```json
{
  "id": "new_card",
  "name": "新卡",
  "cost": 1,
  "description": "描述文本",
  "actions": [
    { "type": "damage", "value": 8 }
  ]
}
```

### 2) 新增敌人

在 `data/enemies.json` 里新增敌人，并配置 `intents`，敌人会按序循环使用意图。

### 3) 增加动作类型

在 `scripts/battle_engine.gd` 的 `_apply_actions()` 中新增 `match` 分支即可。

