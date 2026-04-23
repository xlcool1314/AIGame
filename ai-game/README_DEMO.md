# AiGame Godot C# Demo

这是一个为后续扩展准备的最小 DEMO，目标是先把三件事搭起来：

1. C# 脚本结构
2. 数据驱动配置
3. 可替换的美术资源入口

## 目录说明

- `Scenes/Main.tscn`
  主入口场景
- `Scripts/Data`
  JSON 数据模型和加载逻辑
- `Scripts/Systems`
  游戏状态与经营逻辑
- `Scripts/UI`
  轻量 UI 工具和贴图回退逻辑
- `Data`
  商品、客人、装饰、场景皮肤配置
- `Assets/Placeholders`
  占位图，你可以直接替换或在 JSON 里改路径

## 现在能做什么

1. 自动刷新客人
2. 售卖 4 种商品
3. 获得金币、茶香、声望
4. 购买 3 个装饰
5. 获得基础被动收益
6. 切换普通模式和桌面模式
7. 把窗口吸附到屏幕左侧或右侧
8. 记住窗口模式、位置和大小

## 如何替换美术

### 最简单的方法

直接替换这些文件：

- `Assets/Placeholders/background_placeholder.svg`
- `Assets/Placeholders/clerk_placeholder.svg`
- `Assets/Placeholders/customer_placeholder.svg`
- `Assets/Placeholders/tea_placeholder.svg`
- `Assets/Placeholders/decor_placeholder.svg`

### 更灵活的方法

新增自己的图片后，修改这些 JSON 里的路径：

- `Data/scene_skin.json`
- `Data/products.json`
- `Data/customers.json`
- `Data/decors.json`

## 建议下一步

1. 加存档
2. 把桌面模式继续扩成真正的小窗 HUD
3. 加更完整的装饰布局系统
4. 把当前 JSON 升级成 Godot `Resource` 或 Scriptable 风格的数据资产
