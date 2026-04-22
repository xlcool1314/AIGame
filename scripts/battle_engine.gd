extends RefCounted
class_name BattleEngine

signal state_changed
signal combat_log(message: String)
signal combat_finished(result: String)

const START_HAND_SIZE := 5
const MAX_ENERGY := 3

var game_data: GameData

var player_max_hp := 70
var player_hp := 70
var player_block := 0
var player_energy := MAX_ENERGY

var enemy_data: Dictionary = {}
var enemy_hp := 0
var enemy_block := 0
var enemy_intent_index := 0

var draw_pile: Array[String] = []
var discard_pile: Array[String] = []
var hand: Array[String] = []

var combat_over := false

func setup(data: GameData, deck_id: String, enemy_id: String) -> void:
	game_data = data
	combat_over = false
	player_hp = player_max_hp
	player_block = 0
	enemy_block = 0
	enemy_intent_index = 0
	
	enemy_data = game_data.get_enemy(enemy_id)
	enemy_hp = int(enemy_data.get("max_hp", 0))
	
	var deck_data := game_data.get_deck(deck_id)
	draw_pile = deck_data.get("cards", []).duplicate()
	discard_pile.clear()
	hand.clear()
	_shuffle_array(draw_pile)
	
	emit_signal("combat_log", "战斗开始！敌人：%s（%d HP）" % [enemy_data.get("name", "未知"), enemy_hp])
	_start_player_turn()

func get_state() -> Dictionary:
	return {
		"player_hp": player_hp,
		"player_max_hp": player_max_hp,
		"player_block": player_block,
		"player_energy": player_energy,
		"enemy_name": enemy_data.get("name", "未知"),
		"enemy_hp": enemy_hp,
		"enemy_max_hp": int(enemy_data.get("max_hp", 0)),
		"enemy_block": enemy_block,
		"enemy_intent": _current_enemy_intent().get("name", "..."),
		"hand": hand.duplicate(),
		"draw_count": draw_pile.size(),
		"discard_count": discard_pile.size(),
		"combat_over": combat_over
	}

func play_card(hand_index: int) -> void:
	if combat_over:
		return
	if hand_index < 0 or hand_index >= hand.size():
		return
	var card_id := hand[hand_index]
	var card := game_data.get_card(card_id)
	if card.is_empty():
		return
	var cost := int(card.get("cost", 0))
	if cost > player_energy:
		emit_signal("combat_log", "能量不足，无法打出 %s" % card.get("name", card_id))
		return
	
	player_energy -= cost
	hand.remove_at(hand_index)
	discard_pile.append(card_id)
	
	emit_signal("combat_log", "你打出 %s" % card.get("name", card_id))
	_apply_actions(card.get("actions", []), true)
	_check_combat_end()
	emit_signal("state_changed")

func end_turn() -> void:
	if combat_over:
		return
	emit_signal("combat_log", "你的回合结束。")
	_enemy_turn()
	if not combat_over:
		_start_player_turn()
	emit_signal("state_changed")

func get_card_view(card_id: String) -> Dictionary:
	return game_data.get_card(card_id)

func _start_player_turn() -> void:
	player_block = 0
	enemy_block = 0
	player_energy = MAX_ENERGY
	_draw_to_hand(START_HAND_SIZE)
	emit_signal("combat_log", "你的回合开始：能量恢复为 %d" % player_energy)
	emit_signal("state_changed")

func _enemy_turn() -> void:
	var intent := _current_enemy_intent()
	emit_signal("combat_log", "%s 使用了 %s" % [enemy_data.get("name", "敌人"), intent.get("name", "攻击")])
	_apply_actions(intent.get("actions", []), false)
	enemy_intent_index += 1
	_check_combat_end()

func _current_enemy_intent() -> Dictionary:
	var intents: Array = enemy_data.get("intents", [])
	if intents.is_empty():
		return {}
	return intents[enemy_intent_index % intents.size()]

func _apply_actions(actions: Array, from_player: bool) -> void:
	for action in actions:
		if typeof(action) != TYPE_DICTIONARY:
			continue
		var action_type: String = action.get("type", "")
		var value := int(action.get("value", 0))
		match action_type:
			"damage":
				if from_player:
					var dmg_to_enemy: int = maxi(value - enemy_block, 0)
					enemy_block = maxi(enemy_block - value, 0)
					enemy_hp -= dmg_to_enemy
					emit_signal("combat_log", "造成 %d 伤害（敌人格挡剩余 %d）" % [dmg_to_enemy, enemy_block])
				else:
					var dmg_to_player: int = maxi(value - player_block, 0)
					player_block = maxi(player_block - value, 0)
					player_hp -= dmg_to_player
					emit_signal("combat_log", "你受到 %d 伤害（你的格挡剩余 %d）" % [dmg_to_player, player_block])
			"block":
				if from_player:
					player_block += value
					emit_signal("combat_log", "你获得 %d 格挡" % value)
				else:
					enemy_block += value
					emit_signal("combat_log", "敌人获得 %d 格挡" % value)
			"draw":
				if from_player:
					_draw_cards(value)
					emit_signal("combat_log", "你抽了 %d 张牌" % value)
			_:
				emit_signal("combat_log", "未知动作类型: %s" % action_type)

func _draw_to_hand(target_count: int) -> void:
	while hand.size() < target_count:
		if not _draw_one():
			break

func _draw_cards(count: int) -> void:
	for _i in range(count):
		if not _draw_one():
			break

func _draw_one() -> bool:
	if draw_pile.is_empty():
		if discard_pile.is_empty():
			return false
		draw_pile = discard_pile.duplicate()
		discard_pile.clear()
		_shuffle_array(draw_pile)
	if draw_pile.is_empty():
		return false
	hand.append(draw_pile.pop_back())
	return true

func _check_combat_end() -> void:
	if enemy_hp <= 0:
		combat_over = true
		enemy_hp = 0
		emit_signal("combat_log", "战斗胜利！")
		emit_signal("combat_finished", "win")
	elif player_hp <= 0:
		combat_over = true
		player_hp = 0
		emit_signal("combat_log", "你被击败了。")
		emit_signal("combat_finished", "lose")

func _shuffle_array(arr: Array) -> void:
	arr.shuffle()
