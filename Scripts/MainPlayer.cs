using Godot;

namespace DesktopAssistant.Scripts;

public partial class MainPlayer : Entity
{

	/// <summary>加速速率</summary>
	[Export]
	public float Acceleration { get; set; } = 5000.0f;

	/// <summary>减速率（松手后减速快慢）</summary>
	[Export]
	public float Friction { get; set; } = 5000.0f;

	/// <summary>冲刺速度</summary>
	[Export]
	public float DashSpeed { get; set; } = 1200.0f;

	/// <summary>跳跃初速度</summary>
	[Export]
	public float JumpVelocity { get; set; } = 800.0f;

	/// <summary>冲刺持续时间</summary>
	[Export]
	public float DashDuration { get; set; } = 0.3f;

	/// <summary>冲刺冷却时间</summary>
	[Export]
	public float DashCooldown { get; set; } = 0.25f;

	/// <summary>双击判定窗口（秒）</summary>
	[Export]
	public float DoublePressWindow { get; set; } = 0.25f;

	/// <summary>正在冲刺中</summary>
	private bool _isDashing = false;
	/// <summary>冲刺方向</summary>
	private Vector2 _dashDirection = Vector2.Zero;
	/// <summary>冲刺剩余时间</summary>
	private double _dashTimer = 0.0;
	/// <summary>冲刺冷却剩余时间</summary>
	private double _cooldownTimer = 0.0;

	/// <summary>上一次按下的方向按键名</summary>
	private string _lastPressAction = "";
	/// <summary>距上一次按键的时间</summary>
	private double _timeSinceLastPress = 999.0;

	[Export]
	public int MaxJumpCount { get; set; } = 3;

	private int _jumpCount = 999;

	bool CanJump => _jumpCount < MaxJumpCount;

	protected override void HandleMove(double delta)
	{
		var dt = (float)delta;

		if (IsOnFloor()) _jumpCount = 0;
		if (_cooldownTimer > 0) _cooldownTimer -= dt;

		// 冲刺状态跳过正常移动输入
		if (_isDashing)
		{
			_dashTimer -= dt;
			if (_dashTimer > 0)
			{
				Velocity = new Vector2(_dashDirection.X * DashSpeed, Velocity.Y);
				MoveAndSlide();
				return;
			}

			_isDashing = false;
			_cooldownTimer = DashCooldown;
		}

		// 跳跃（仅限地面）
		if (Input.IsActionJustPressed("jump") && CanJump)
		{
			_jumpCount++;
			Velocity = Velocity with { Y = -JumpVelocity };
		}

		CheckDoubleTap("move_left", Vector2.Left);
		CheckDoubleTap("move_right", Vector2.Right);

		var inputDir = Input.GetAxis("move_left", "move_right");
		var targetVelocity = new Vector2(inputDir * MaxSpeed, Velocity.Y);
		var deltaSpeed = (inputDir != 0 ? Acceleration : Friction) * dt;
		Velocity = Velocity.MoveToward(targetVelocity, deltaSpeed);
	}

	public override void _Process(double delta)
	{
		_timeSinceLastPress += delta;
	}

	/// <summary>
	/// 检查双击
	/// </summary>
	private void CheckDoubleTap(string action, Vector2 direction)
	{
		if (!Input.IsActionJustPressed(action)) return;

		if (_cooldownTimer > 0)
		{
			_lastPressAction = "";
			return;
		}

		if (_lastPressAction == action && _timeSinceLastPress <= DoublePressWindow)
		{
			_isDashing = true;
			_dashDirection = direction;
			_dashTimer = DashDuration;
			_lastPressAction = "";
			return;
		}

		_lastPressAction = action;
		_timeSinceLastPress = 0.0;
	}
}
