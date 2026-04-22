extends RefCounted
class_name GameData

var cards: Dictionary = {}
var enemies: Dictionary = {}
var decks: Dictionary = {}

func load_all() -> void:
	cards = _load_indexed_json("res://data/cards.json", "cards")
	enemies = _load_indexed_json("res://data/enemies.json", "enemies")
	decks = _load_indexed_json("res://data/decks.json", "decks")

func get_card(card_id: String) -> Dictionary:
	return cards.get(card_id, {})

func get_enemy(enemy_id: String) -> Dictionary:
	return enemies.get(enemy_id, {})

func get_deck(deck_id: String) -> Dictionary:
	return decks.get(deck_id, {})

func _load_indexed_json(path: String, key: String) -> Dictionary:
	var raw_text := FileAccess.get_file_as_string(path)
	if raw_text.is_empty():
		push_error("无法读取数据文件: %s" % path)
		return {}
	var parsed = JSON.parse_string(raw_text)
	if typeof(parsed) != TYPE_DICTIONARY:
		push_error("JSON 格式不正确: %s" % path)
		return {}
	var list_data: Array = parsed.get(key, [])
	var result: Dictionary = {}
	for item in list_data:
		if typeof(item) == TYPE_DICTIONARY and item.has("id"):
			result[item["id"]] = item
	return result
