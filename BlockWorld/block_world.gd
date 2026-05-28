# ============================================================================
# extends — 继承自 Node2D，所有 2D 节点的基类
# 场景树中的每个节点都对应一个脚本，extends 指明从哪个类型继承
# ============================================================================
extends Node2D

# ============================================================================
# const — 常量，定义后不可修改，编译期确定
# := 是类型推断，GDScript 自动推断右侧值的类型
# 和显式写 : int 等价，只是省略了类型注解
# ============================================================================
const WORLD_W := 60 # 世界宽度（格数）
const WORLD_H := 40 # 世界高度（格数）
const GROUND_Y := 28 # 地表所在行号

# ============================================================================
# enum — 枚举，定义一组具名整型常量
# 不指定值时从 0 开始递增，这里 AIR 手动设为 -1
# 用法: Tile.AIR → -1, Tile.DIRT → 0, Tile.STONE → 1, Tile.GRASS → 2
# ============================================================================
enum Tile {AIR = -1, DIRT, STONE, GRASS}

# ============================================================================
# @onready — 特殊注解，表示在 _ready() 之前初始化
# $TileMap 是 get_node("TileMap") 的语法糖
# : TileMap 是显式类型声明，GDScript 会做类型检查
# 不加 := 推断是因为 $ 返回的是 Node，需要显式转为 TileMap
# ============================================================================
@onready var _tilemap: TileMapLayer = $TileMapLayer

# var — 变量声明
# Array 不带泛型时元素全是 Variant（任意类型）
# 这里 _grid[x][y] 每个元素存 Tile 枚举值（本质是 int）
# 没有加 @onready，因为 _init_grid() 是在 _ready() 里手动调用的
# ============================================================================
var _grid: Array

# ============================================================================
# _ready() — Godot 生命周期函数，节点首次进入场景树时自动调用
# Camera2D 和 TileSet 的初始化已移至 BlockWorld.tscn
# ============================================================================
func _ready() -> void:
	_setup_physics() # 为固体 tile 添加碰撞
	_init_grid() # 初始化二维数组
	_generate() # 生成地形
	_render() # 绘制到 TileMap
	_spawn_player() # 把玩家放到地表

func _setup_physics() -> void:
	var ts := _tilemap.tile_set
	ts.set_physics_layer_collision_layer(0, 1)
	var pts := PackedVector2Array([Vector2(0, 0), Vector2(16, 0), Vector2(16, 16), Vector2(0, 16)])
	var src: TileSetAtlasSource = ts.get_source(0)
	for tid in [Tile.DIRT, Tile.STONE, Tile.GRASS]:
		var td := src.get_tile_data(Vector2i(tid, 0), 0)
		td.add_collision_polygon(0)
		td.set_collision_polygon_points(0, 0, pts)

func _spawn_player() -> void:
	var cx := WORLD_W / 2
	var surf_y := GROUND_Y + int(sin(cx * 0.25) * 4 + cos(cx * 0.13) * 2)
	$Player.global_position = Vector2(cx * 16, (surf_y - 2) * 16)

# ============================================================================
# _init_grid — 初始化二维数组
# GDScript 的 Array 是动态数组，必须手动构建二维结构
# 先建外层数组，resize 确定长度，再给每个元素初始化内层数组
# ============================================================================
func _init_grid() -> void:
	_grid = []
	_grid.resize(WORLD_W) # 设定外层长度 = 60
	for x in WORLD_W:
		_grid[x] = [] # 每列新建一个数组
		_grid[x].resize(WORLD_H) # 设定内层长度 = 40
		for y in WORLD_H:
			_grid[x][y] = Tile.AIR # 初始全是空气

# ============================================================================
# _generate — 用简单三角函数生成起伏地形
# sin + cos 叠加产生自然感的地表起伏
# 分层逻辑: 表面→草地，下面4层→泥土，更深→石头
# 这是一维的列生成（只依赖 x），二维地形的生成需要噪声算法
# ============================================================================
func _generate() -> void:
	for x in WORLD_W:
		# sin(x*0.25)*4 产生主起伏（幅度4格，周期约25格）
		# cos(x*0.13)*2 叠加次级起伏，使地形更自然
		var surf := GROUND_Y + int(sin(x * 0.25) * 4 + cos(x * 0.13) * 2)

		# 地表格子设为草地
		_grid[x][surf] = Tile.GRASS

		# 地表以下的填充逻辑
		# range(surf+1, WORLD_H) — 从地表+1 到世界底部
		# y - surf 是"在地表下第几层"
		# 三目运算符: if 条件 else — GDScript 没有 ? : 语法
		for y in range(surf + 1, WORLD_H):
			_grid[x][y] = Tile.DIRT if y - surf < 5 else Tile.STONE

# ============================================================================
# _render — 把 _grid 数组同步到 TileMapLayer
# clear() 清空所有格子，set_cell 逐格设置
# set_cell(格子坐标, 图集源ID, 图集中坐标)
# TileMapLayer 代表单一图层，不需要传 layer 参数
# 全量重绘在格子少时完全够用，大规模世界需要增量更新
# ============================================================================
func _render() -> void:
	_tilemap.clear()
	for x in WORLD_W:
		for y in WORLD_H:
			var t: int = _grid[x][y] # 显式声明 int 避免类型推断歧义
			if t != Tile.AIR: # 跳过空气（不绘制）
				_tilemap.set_cell(Vector2i(x, y), 0, Vector2i(t, 0))

# ============================================================================
# _unhandled_input — 处理未被其他节点消费的输入事件
# event is InputEventMouseButton — is 关键字做类型检查
# GDScript 中所有输入事件都继承自 InputEvent
# 鼠标按下、释放各产生一个事件，is_pressed() 判断是否按下
# ============================================================================
func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventMouseButton and event.is_pressed():
		_on_click(event)

# ============================================================================
# _on_click — 鼠标点击逻辑
# 把屏幕坐标 → TileMap 本地坐标 → 格子坐标
# local_to_map 和 to_local 是 TileMapLayer 的坐标转换方法
# match 是 GDScript 的模式匹配语句，类似 switch 但更强大
# ============================================================================
func _on_click(event: InputEventMouseButton) -> void:
	# get_global_mouse_position() — 获取鼠标在全局场景的坐标
	# to_local() 转成 TileMap 的相对坐标
	# local_to_map() 把像素坐标转成格子索引
	var pos := _tilemap.local_to_map(_tilemap.to_local(get_global_mouse_position()))

	# 越界检查 — 防止数组访问崩溃
	if pos.x < 0 or pos.x >= WORLD_W or pos.y < 0 or pos.y >= WORLD_H:
		return

	# match 等价于其他语言的 switch
	# MOUSE_BUTTON_LEFT / MOUSE_BUTTON_RIGHT 是枚举常量
	match event.button_index:
		MOUSE_BUTTON_LEFT:
			# 左键：移除物块（设为空气）
			_grid[pos.x][pos.y] = Tile.AIR
			_render()
		MOUSE_BUTTON_RIGHT:
			# 右键：在空位放置泥土
			if _grid[pos.x][pos.y] == Tile.AIR:
				_grid[pos.x][pos.y] = Tile.DIRT
				_render()
