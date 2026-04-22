extends Control

const GameData = preload("res://scripts/game_data.gd")
const BattleEngine = preload("res://scripts/battle_engine.gd")

var game_data: GameData
var battle: BattleEngine

@onready var player_status_label: Label = $MarginContainer/VBox/PlayerStatus
@onready var enemy_status_label: Label = $MarginContainer/VBox/EnemyStatus
@onready var deck_status_label: Label = $MarginContainer/VBox/DeckStatus
@onready var hand_container: HBoxContainer = $MarginContainer/VBox/HandPanel/HandCards
@onready var log_text: RichTextLabel = $MarginContainer/VBox/BattleLog
@onready var end_turn_button: Button = $MarginContainer/VBox/EndTurnButton

func _ready() -> void:
	game_data = GameData.new()
	game_data.load_all()
	
	battle = BattleEngine.new()
	battle.combat_log.connect(_append_log)
	battle.state_changed.connect(_refresh_ui)
	battle.combat_finished.connect(_on_combat_finished)
	
	end_turn_button.pressed.connect(_on_end_turn_pressed)
	battle.setup(game_data, "starter", "green_slime")
	_refresh_ui()

func _refresh_ui() -> void:
	var state := battle.get_state()
	player_status_label.text = "玩家 HP: %d/%d  格挡: %d  能量: %d" % [
		state["player_hp"], state["player_max_hp"], state["player_block"], state["player_energy"]
	]
	enemy_status_label.text = "%s HP: %d/%d  格挡: %d  意图: %s" % [
		state["enemy_name"], state["enemy_hp"], state["enemy_max_hp"], state["enemy_block"], state["enemy_intent"]
	]
	deck_status_label.text = "抽牌堆: %d  弃牌堆: %d" % [state["draw_count"], state["discard_count"]]
	
	for child in hand_container.get_children():
		child.queue_free()
	
	var hand_cards: Array = state["hand"]
	for i in hand_cards.size():
		var card_id: String = hand_cards[i]
		var card := battle.get_card_view(card_id)
		var button := Button.new()
		button.custom_minimum_size = Vector2(180, 100)
		button.text = "%s (%d)\n%s" % [
			card.get("name", card_id),
			int(card.get("cost", 0)),
			card.get("description", "")
		]
		button.disabled = state["combat_over"]
		button.pressed.connect(_on_card_pressed.bind(i))
		hand_container.add_child(button)
	
	end_turn_button.disabled = state["combat_over"]

func _on_card_pressed(hand_index: int) -> void:
	battle.play_card(hand_index)

func _on_end_turn_pressed() -> void:
	battle.end_turn()

func _append_log(message: String) -> void:
	log_text.append_text("%s\n" % message)
	log_text.scroll_to_line(log_text.get_line_count())

func _on_combat_finished(result: String) -> void:
	if result == "win":
		_append_log("你赢了！可扩展到奖励结算 / 下一个房间。")
	else:
		_append_log("你输了！可扩展到重开 / 结算页面。")
