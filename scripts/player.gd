extends CharacterBody2D

@export var move_speed: float = 260.0
@export var jump_velocity: float = -600.0
@export var side_influence: float = 50.0

var gravity: float = ProjectSettings.get_setting("physics/2d/default_gravity")

func _physics_process(delta: float) -> void:
	# Gravitation
	if not is_on_floor():
		velocity.y += gravity * delta

	# Springen (Leertaste / ui_accept)
	if Input.is_action_just_pressed("ui_accept") and is_on_floor():
		velocity.y = jump_velocity

	# Links/Rechts (A/D oder Pfeile)
	var dir := Input.get_axis("ui_left", "ui_right")
	if dir != 0:
		velocity.x = move_toward(velocity.x, dir * move_speed, side_influence)
	else:
		velocity.x = move_toward(velocity.x, 0.0, side_influence)

	move_and_slide()
