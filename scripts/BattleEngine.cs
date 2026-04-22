using Godot;
using Godot.Collections;

public partial class BattleEngine : RefCounted
{
    [Signal] public delegate void StateChangedEventHandler();
    [Signal] public delegate void CombatLogEventHandler(string message);
    [Signal] public delegate void CombatFinishedEventHandler(string result);

    private const int StartHandSize = 5;
    private const int MaxEnergy = 3;

    private GameData _gameData = null!;

    private int _playerMaxHp = 70;
    private int _playerHp = 70;
    private int _playerBlock = 0;
    private int _playerEnergy = MaxEnergy;

    private Dictionary _enemyData = new();
    private int _enemyHp = 0;
    private int _enemyBlock = 0;
    private int _enemyIntentIndex = 0;

    private Array<string> _drawPile = new();
    private Array<string> _discardPile = new();
    private Array<string> _hand = new();

    private bool _combatOver = false;

    public void Setup(GameData data, string deckId, string enemyId)
    {
        _gameData = data;
        _combatOver = false;

        _playerHp = _playerMaxHp;
        _playerBlock = 0;
        _enemyBlock = 0;
        _enemyIntentIndex = 0;

        _enemyData = _gameData.GetEnemy(enemyId);
        _enemyHp = _enemyData.GetValueOrDefault("max_hp", 0).AsInt32();

        var deckData = _gameData.GetDeck(deckId);
        _drawPile = deckData.GetValueOrDefault("cards", new Array()).AsGodotArray<string>().Duplicate();
        _discardPile.Clear();
        _hand.Clear();
        _drawPile.Shuffle();

        EmitSignal(SignalName.CombatLog, $"战斗开始！敌人：{_enemyData.GetValueOrDefault("name", "未知")}（{_enemyHp} HP）");
        StartPlayerTurn();
    }

    public Dictionary GetState()
    {
        return new Dictionary
        {
            {"player_hp", _playerHp},
            {"player_max_hp", _playerMaxHp},
            {"player_block", _playerBlock},
            {"player_energy", _playerEnergy},
            {"enemy_name", _enemyData.GetValueOrDefault("name", "未知")},
            {"enemy_hp", _enemyHp},
            {"enemy_max_hp", _enemyData.GetValueOrDefault("max_hp", 0).AsInt32()},
            {"enemy_block", _enemyBlock},
            {"enemy_intent", CurrentEnemyIntent().GetValueOrDefault("name", "...")},
            {"hand", _hand.Duplicate()},
            {"draw_count", _drawPile.Count},
            {"discard_count", _discardPile.Count},
            {"combat_over", _combatOver}
        };
    }

    public Dictionary GetCardView(string cardId) => _gameData.GetCard(cardId);

    public void PlayCard(int handIndex)
    {
        if (_combatOver || handIndex < 0 || handIndex >= _hand.Count)
        {
            return;
        }

        var cardId = _hand[handIndex];
        var card = _gameData.GetCard(cardId);
        if (card.Count == 0)
        {
            return;
        }

        var cost = card.GetValueOrDefault("cost", 0).AsInt32();
        if (cost > _playerEnergy)
        {
            EmitSignal(SignalName.CombatLog, $"能量不足，无法打出 {card.GetValueOrDefault("name", cardId)}");
            return;
        }

        _playerEnergy -= cost;
        _hand.RemoveAt(handIndex);
        _discardPile.Add(cardId);

        EmitSignal(SignalName.CombatLog, $"你打出 {card.GetValueOrDefault("name", cardId)}");
        ApplyActions(card.GetValueOrDefault("actions", new Array()).AsGodotArray(), true);
        CheckCombatEnd();
        EmitSignal(SignalName.StateChanged);
    }

    public void EndTurn()
    {
        if (_combatOver)
        {
            return;
        }

        EmitSignal(SignalName.CombatLog, "你的回合结束。");
        EnemyTurn();

        if (!_combatOver)
        {
            StartPlayerTurn();
        }

        EmitSignal(SignalName.StateChanged);
    }

    private void StartPlayerTurn()
    {
        _playerBlock = 0;
        _enemyBlock = 0;
        _playerEnergy = MaxEnergy;
        DrawToHand(StartHandSize);

        EmitSignal(SignalName.CombatLog, $"你的回合开始：能量恢复为 {_playerEnergy}");
        EmitSignal(SignalName.StateChanged);
    }

    private void EnemyTurn()
    {
        var intent = CurrentEnemyIntent();
        EmitSignal(SignalName.CombatLog, $"{_enemyData.GetValueOrDefault("name", "敌人")} 使用了 {intent.GetValueOrDefault("name", "攻击")}");

        ApplyActions(intent.GetValueOrDefault("actions", new Array()).AsGodotArray(), false);
        _enemyIntentIndex += 1;
        CheckCombatEnd();
    }

    private Dictionary CurrentEnemyIntent()
    {
        var intents = _enemyData.GetValueOrDefault("intents", new Array()).AsGodotArray();
        if (intents.Count == 0)
        {
            return new Dictionary();
        }

        return intents[_enemyIntentIndex % intents.Count].AsGodotDictionary();
    }

    private void ApplyActions(Array actions, bool fromPlayer)
    {
        foreach (var actionValue in actions)
        {
            var action = actionValue.AsGodotDictionary();
            var actionType = action.GetValueOrDefault("type", "").AsString();
            var value = action.GetValueOrDefault("value", 0).AsInt32();

            switch (actionType)
            {
                case "damage":
                    if (fromPlayer)
                    {
                        var dmgToEnemy = Mathf.Max(value - _enemyBlock, 0);
                        _enemyBlock = Mathf.Max(_enemyBlock - value, 0);
                        _enemyHp -= dmgToEnemy;
                        EmitSignal(SignalName.CombatLog, $"造成 {dmgToEnemy} 伤害（敌人格挡剩余 {_enemyBlock}）");
                    }
                    else
                    {
                        var dmgToPlayer = Mathf.Max(value - _playerBlock, 0);
                        _playerBlock = Mathf.Max(_playerBlock - value, 0);
                        _playerHp -= dmgToPlayer;
                        EmitSignal(SignalName.CombatLog, $"你受到 {dmgToPlayer} 伤害（你的格挡剩余 {_playerBlock}）");
                    }
                    break;

                case "block":
                    if (fromPlayer)
                    {
                        _playerBlock += value;
                        EmitSignal(SignalName.CombatLog, $"你获得 {value} 格挡");
                    }
                    else
                    {
                        _enemyBlock += value;
                        EmitSignal(SignalName.CombatLog, $"敌人获得 {value} 格挡");
                    }
                    break;

                case "draw":
                    if (fromPlayer)
                    {
                        DrawCards(value);
                        EmitSignal(SignalName.CombatLog, $"你抽了 {value} 张牌");
                    }
                    break;

                default:
                    EmitSignal(SignalName.CombatLog, $"未知动作类型: {actionType}");
                    break;
            }
        }
    }

    private void DrawToHand(int targetCount)
    {
        while (_hand.Count < targetCount)
        {
            if (!DrawOne())
            {
                break;
            }
        }
    }

    private void DrawCards(int count)
    {
        for (var i = 0; i < count; i++)
        {
            if (!DrawOne())
            {
                break;
            }
        }
    }

    private bool DrawOne()
    {
        if (_drawPile.Count == 0)
        {
            if (_discardPile.Count == 0)
            {
                return false;
            }

            _drawPile = _discardPile.Duplicate();
            _discardPile.Clear();
            _drawPile.Shuffle();
        }

        if (_drawPile.Count == 0)
        {
            return false;
        }

        _hand.Add(_drawPile[_drawPile.Count - 1]);
        _drawPile.RemoveAt(_drawPile.Count - 1);
        return true;
    }

    private void CheckCombatEnd()
    {
        if (_enemyHp <= 0)
        {
            _combatOver = true;
            _enemyHp = 0;
            EmitSignal(SignalName.CombatLog, "战斗胜利！");
            EmitSignal(SignalName.CombatFinished, "win");
        }
        else if (_playerHp <= 0)
        {
            _combatOver = true;
            _playerHp = 0;
            EmitSignal(SignalName.CombatLog, "你被击败了。");
            EmitSignal(SignalName.CombatFinished, "lose");
        }
    }
}
