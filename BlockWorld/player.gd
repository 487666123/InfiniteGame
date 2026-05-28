extends CharacterBody2D

const SPEED := 150.0
const JUMP_VELOCITY := -320.0
const GRAVITY := 980.0

var _w_prev := false

func _physics_process(delta: float) -> void:
	velocity.y += GRAVITY * delta

	# 水平输入：方向键 + WASD
	var dir := Input.get_axis("ui_left", "ui_right")
	if dir == 0:
		dir = float(Input.is_physical_key_pressed(KEY_D)) - float(Input.is_physical_key_pressed(KEY_A))
	velocity.x = dir * SPEED

	# 跳跃：Space + W（仅按下的那一帧生效）
	var w_now := Input.is_physical_key_pressed(KEY_W)
	var w_just := w_now and not _w_prev
	_w_prev = w_now
	if (Input.is_action_just_pressed("ui_accept") or w_just) and is_on_floor():
		velocity.y = JUMP_VELOCITY

	move_and_slide()
