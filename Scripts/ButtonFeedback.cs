using Godot;

namespace InfiniteGame.Scripts;

/// <summary>
/// 支持 hover 动画和 hover 音效
/// </summary>
[GlobalClass]
public partial class ButtonFeedback : Node
{
	[Export]
	public Tween.TransitionType TransitionType { get; set; } = Tween.TransitionType.Back;

	[Export]
	public Tween.EaseType EaseType { get; set; } = Tween.EaseType.Out;

	[Export]
	public Vector2 HoverScale { get; set; } = new Vector2(1.1f, 1.1f);

	private Vector2 _defaultScale;

	[Export]
	public float Time = 0.2f;

	[Export]
	public AudioStream HoverSound { get; set; }

	AudioStreamPlayer _audioPlayer;

	/// <summary>
	/// 父节点
	/// </summary>
	private Control _target;

	public override void _Ready()
	{
		// 父节点
		_target = GetParent() as Control;
		Connnect_Signals();
		CallDeferred("Setup");

		_audioPlayer = new AudioStreamPlayer();
		AddChild(_audioPlayer);
	}

	void Connnect_Signals()
	{
		_target.MouseEntered += On_Hover;
		_target.MouseExited += Off_Hover;
	}

	void Setup()
	{
		_target.PivotOffset = _target.Size / 2;
		_defaultScale = _target.Scale;
	}

	void On_Hover()
	{
		PlayHoverSound();
		AddTween("scale", HoverScale, Time);
	}

	void Off_Hover() => AddTween("scale", _defaultScale, Time);

	void PlayHoverSound()
	{
		if (HoverSound is null) return;

		_audioPlayer.Stream = HoverSound;
		_audioPlayer.Play();
	}

	Tween _tween;

	void AddTween(string property, Vector2 value, float seconds)
	{
		_tween?.Kill();

		_tween = CreateTween();
		_tween.TweenProperty(_target, property, value, seconds)
			.SetEase(EaseType)
			.SetTrans(TransitionType);
	}
}
