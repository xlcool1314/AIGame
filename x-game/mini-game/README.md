# Astra Fracture / 星穹裂隙

1920x1080 Godot 4.6 C# procedural arcade roguelite.

一款 1920x1080 的 Godot 4.6 C# 程序化美术街机 Roguelite。

## Language / 语言

- The game auto-selects Chinese when the OS language is `zh`; otherwise it starts in English.
- Press `L` anywhere to switch between English and Chinese.
- 游戏会在系统语言为 `zh` 时默认使用中文，否则默认英文。
- 任意界面按 `L` 可在中文和英文之间切换。

## Campaign / 战役

The main run is now a long-form five-sector campaign:

- 5 sectors
- 8 waves per sector
- 40 waves total
- A Choir Core boss at the end of every sector
- New environmental rules after the first sector
- Elite enemies appear more often in later sectors

主模式现在是一条更长的战役：

- 5 章
- 每章 8 波
- 总计 40 波
- 每章第 8 波是 Boss
- 第一章之后会出现环境光束
- 后期会出现更多精英敌人

## Sectors / 星域

1. Lumen Shoal / 光滩星区：教学章，没有环境危险。
2. Glass Reef / 玻璃星区：预警光束会出现，随后造成伤害。
3. Verdant Grave / 孢子星区：敌人血量更高，修复掉落更多。
4. Clock Cathedral / 时钟星区：环境光束角度更复杂，弹幕更密。
5. Solar Wound / 太阳裂口：最终章，精英敌人更多，Boss 更强。

## New Content / 新增内容

- New enemies: Mine, Shard, Warden.
- Elite enemies: larger, tougher, richer rewards, stronger visual rings.
- Sector hazards: warning beams that become damaging lances.
- Boss variants: every sector boss has higher stats and extra patterns.
- New upgrades: Nova Capacitor, Polarity Storm, Comet Trail, Aegis Bloom, Quantum Echo, Solar Thesis.

- 新敌人：地雷体、碎晶体、守卫体。
- 精英敌人：更大、更硬，掉落更多奖励。
- 环境危险：光束会先预警，再造成伤害；同色光束可以吸收。
- Boss 变体：每章 Boss 都会更强，并增加弹幕模式。
- 新升级：Nova 强化、换色反击、冲刺强化、自动回血、额外射击、反色专精。

## Controls / 操作

- WASD / Arrow keys: Move
- Mouse: Aim
- Auto fire: the ship shoots while enemies are alive
- Right mouse / Shift: Dash, briefly invulnerable and clears nearby bullets
- Q / Tab: Shift resonance color
- F / E: Spend energy to cast Nova
- L: Switch language

- WASD / 方向键：移动
- 鼠标：瞄准
- 自动射击：场上有敌人时飞船会持续开火
- 鼠标右键 / Shift：冲刺，短暂无敌并清除近身弹幕
- Q / Tab：切换共振颜色
- F / E：消耗能量释放 Nova
- L：切换语言

## Core Rules / 核心规则

- Bullets matching the player's color are absorbed into energy.
- Player shots deal higher damage to opposite-color enemies.
- Choose one upgrade after every cleared wave.
- Matching-color hazard beams can also be absorbed.
- Boss waves are wave 8, 16, 24, 32, and 40.

- 同色子弹会被吸收并转化为能量。
- 攻击反色敌人会造成更高伤害。
- 每一波结束后选择一个升级。
- 同色环境光束也可以吸收。
- Boss 波次是第 8、16、24、32、40 波。

## Upgrade Choices / 升级选择

- Upgrade screens now show 4 cards instead of 3.
- Press `R` to reroll once; some choices can increase future rerolls.
- Cards include permanent upgrades, instant repairs, one-wave tactics, and risk/reward contracts.

- 升级界面现在是 4 选 1，不再是 3 选 1。
- 按 `R` 可以重抽一次；部分选择可以增加之后的重抽次数。
- 卡牌包含永久升级、立即维修、单波战术和风险契约。

## Main Menu / 主界面

- The game now starts on a real main menu.
- Mouse clicks outside the Start button will not begin combat.
- Permanent Upgrades opens the Star Vault between runs.

- 游戏现在会停在主界面。
- 点击“开始远征”以外的位置不会直接开战。
- “永久升级”会打开局外成长界面。

## Meta Progression / 局外成长

- Every run awards Star Dust based on reached wave, score, sector progress, and victory.
- Star Dust is saved locally and can be spent on permanent upgrades.
- Permanent upgrades improve starting hull, energy, weapon damage, mobility, pickup range, Star Dust gain, and rerolls.
- Save data is stored through Godot at `user://astra_fracture_meta.cfg`.

- 每局结束都会根据到达波次、分数、章节进度和是否通关获得星尘。
- 星尘会自动保存，可在主界面的“永久升级”中使用。
- 永久升级会提高开局生命、能量、伤害、机动、拾取范围、星尘收益和重抽次数。
- 存档由 Godot 写入 `user://astra_fracture_meta.cfg`。

## Color Switching / 切色玩法

- Same-color bullets and beams are absorbed into energy.
- Opposite-color enemies take much higher damage.
- Switching color shows a short combat tip, and successful absorbs/opposite hits now display feedback text.

- 同色子弹和光束会被吸收并转成能量。
- 反色敌人会受到更高伤害。
- 切色时会显示短提示，成功吸收和反色命中也会弹出反馈文字。

## Performance / 性能优化

- Combat objects now use pools for enemies, shots, pickups, particles, and floating damage text.
- Bullet, pickup, particle, and damage text counts have runtime caps to avoid late-wave spikes.
- Large bursts automatically reduce particle count when the screen is already busy.

- 敌人、子弹、拾取物、粒子和伤害数字现在使用对象池复用。
- 子弹、拾取物、粒子和伤害数字都有数量上限，避免后期突然掉帧。
- 屏幕已经很忙时，大爆炸会自动减少粒子数量。

## Build / 构建

```powershell
dotnet build MiniGame.csproj --configfile NuGet.Config
```

Main scene / 主场景：`res://Scenes/Main.tscn`
