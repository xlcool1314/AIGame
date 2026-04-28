# AIGame（Godot 4 + C# 数据驱动卡牌战斗原型）

这是一个基于 **Godot 4 + C#** 的扫雷 + 卡牌战斗探索原型。核心跑局围绕矿区扫雷探勘展开：玩家在分层矿井中选择路线，进入矿区后翻格、标记暗雷、依据数字判断风险；战斗、事件、商店和休整作为路线中的风险与成长节点。

## 当前实现

- 全部核心逻辑使用 C#：`GameData.cs` / `BattleEngine.cs` / `BattleScene.cs`
- 数据驱动：卡牌 / 敌人 / 初始牌组 / 事件 / 奖励来源于 JSON 配置
- 矿井探索：分层路线选择包含扫雷矿区、事件、战斗、商店、休整和最终精英战
- 扫雷核心：矿区房间支持翻格、标记、数字提示、踩雷扣血和安全格清理奖励
- 主界面流程：从主菜单开始游戏，支持新游戏、读档、设置和返回主菜单
- 多语言：界面壳支持中文 / English 切换，并保存到用户设置
- 存档：在路线选择等稳定节点自动保存跑局，主菜单可读档继续
- 事件房间：支持多选项，并能造成伤害、治疗、增加生命上限、获得矿晶或加入卡牌
- 战斗机制：生命、格挡、能量、抽牌堆、弃牌堆、手牌
- 战斗奖励：胜利后获得矿晶、治疗，并可从奖励卡牌中选择一张加入牌组
- 商店与休整：可花费矿晶购买卡牌/治疗，休整点可治疗或换取矿晶与遗物记录
- 行动类型：`damage` / `block` / `draw`
- 基础 UI：跑局状态、房间描述、事件选项、敌人意图、手牌按钮、奖励选择、日志

## 目录结构

- `x-game/project.godot`：Godot 项目入口（主场景 + 分辨率）
- `x-game/scenes/MainMenu.tscn`：主菜单、读档和设置入口
- `x-game/scenes/BattleScene.tscn`：探索 + 战斗一体化场景 UI
- `x-game/scripts/MainMenu.cs`：主菜单交互
- `x-game/scripts/Localization.cs`：中英文界面文本与语言设置
- `x-game/scripts/SaveManager.cs`：跑局存档读写
- `x-game/scripts/GameData.cs`：JSON 读取与数据索引
- `x-game/scripts/RunEngine.cs`：跑局状态、扫雷矿区、分层矿井路线、商店、休整、事件与奖励结算
- `x-game/scripts/BattleEngine.cs`：战斗状态机与规则
- `x-game/scripts/BattleScene.cs`：UI 渲染与输入处理
- `x-game/data/cards.json`：卡牌定义
- `x-game/data/enemies.json`：敌人定义（含意图循环）
- `x-game/data/decks.json`：初始牌组
- `x-game/data/events.json`：矿井事件与选项
- `x-game/data/rewards.json`：战斗奖励池

## 运行方式

1. 使用 Godot 4.x（Mono / .NET）打开 `x-game/project.godot`。
2. 确保本地安装 .NET SDK。
3. 运行项目，进入主菜单。

## 后续建议

1. 增加扫雷首翻保护、右键标记、连锁翻开和不同矿层规则。
2. 将扫雷结果与卡牌战斗联动，例如探明矿脉获得能量、误触暗雷触发伏击。
3. 将当前固定分层路线扩展为随机地图生成与事件池抽样。
4. 增加遗物的实际扫雷/战斗效果、局外成长和存档。
