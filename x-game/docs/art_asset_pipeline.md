# Art Asset Pipeline

这份规则用于后续逐步替换程序占位美术。当前优先完成了卡牌组件的预制体化，后续角色、敌人、地图节点也建议沿用同样思路：数据只声明资源路径，界面通过 `.tscn` 预制体承载版式。

## Card Prefab

卡牌预制体位于：

`res://scenes/ui/CardView.tscn`

脚本位于：

`res://scripts/CardCell.cs`

不要在各个界面里单独拼卡牌外观。需要显示卡牌时优先使用：

```csharp
CardCell.Create(card, CardCellMode.Hand, playable, footerText, index)
```

这样战斗手牌、卡牌库、奖励预览都可以共享同一套卡牌版式。

## Card Asset Naming

推荐按约定放置资源，不需要逐张卡改代码：

```text
res://art/cards/illustrations/{card_id}.png
res://art/cards/full/{card_id}.png
res://art/cards/frames/{template_id}.png
res://art/cards/frames/{type}_{rarity}.png
res://art/cards/frames/{type}.png
res://art/cards/frames/default.png
res://art/cards/icons/{card_id}.png
res://art/cards/icons/{type}.png
res://art/cards/icons/{rarity}.png
```

查找优先级是：卡牌数据里的显式路径优先，其次使用上面的命名约定。

## Card Data Fields

`data/cards.json` 中每张卡现在可以额外配置：

```json
{
  "id": "strike",
  "templateId": "attack_common",
  "artPath": "res://art/cards/illustrations/strike.png",
  "framePath": "res://art/cards/frames/attack_common.png",
  "iconPath": "res://art/cards/icons/attack.png",
  "fullArtPath": "res://art/cards/full/strike.png"
}
```

这些字段都可以省略。省略时会自动按命名约定查找，如果找不到资源就显示基础占位样式。

## Recommended Sizes

卡牌完整图：`512x704`，透明或不透明均可。

卡牌插画：`512x288` 或等比例横图，建议透明 PNG 或 WebP。

卡牌框：`512x704` 透明 PNG，边框和装饰放在透明图层里。

卡牌图标：`128x128` 透明 PNG。

卡牌文字仍由游戏实时渲染，这样中英文、多语言和数值改动不会要求重新导出整张卡图。
