using Godot;
using System;

public partial class 菜单选项 : VBoxContainer
{
	[Export]
	public Button PlayGame { get; set; }

	[Export]
	public Button GameSettings { get; set; }

	[Export]
	public Button ExitGame { get; set; }

	[Export]
	public Button MoveBgMusic { get; set; }

	[Export]
	public AudioStream BgAudioStream { get; set; }

	/// <summary>
	/// 背景音乐播放器
	/// </summary>
	AudioStreamPlayer _bgMusic;

	Tween _tween;

	float _fadeDuration = 2f;
	float _endThreshold = 3f;
	bool _isFadingOut;

	public override void _Ready()
	{
		PlayGame.Pressed += OnPlayGamePressed;
		ExitGame.Pressed += OnExitGamePressed;
		MoveBgMusic.Pressed += MoveBgMusicToEnd;

		_bgMusic = new AudioStreamPlayer
		{
			Stream = BgAudioStream
		};
		AddChild(_bgMusic);
		_bgMusic.Finished += PlayWithFadeIn;
		PlayWithFadeIn();
	}

	void MoveBgMusicToEnd()
	{
		if (_bgMusic?.Stream is null) return;

		_bgMusic.Seek((float)_bgMusic.Stream.GetLength() - 8f);
	}

	/// <summary>
	/// 缓缓播放
	/// </summary>
	void PlayWithFadeIn()
	{
		_isFadingOut = false;
		_bgMusic.VolumeDb = -40f;
		_bgMusic.Play();
		FadeVolume(5);
	}

	void FadeVolume(float targetDb)
	{
		_tween?.Kill();
		_tween = CreateTween();
		_tween.TweenProperty(_bgMusic, "volume_db", targetDb, _fadeDuration)
			.SetEase(Tween.EaseType.In)
			.SetTrans(Tween.TransitionType.Sine);
	}

	private void OnPlayGamePressed()
	{
		GetTree().ChangeSceneToFile("res://BlockWorld/BlockWorld.tscn");
	}

	private void OnExitGamePressed()
	{
		GetTree().Quit();
	}

	public override void _Process(double delta)
	{
		if (_bgMusic.Playing && !_isFadingOut)
		{
			var remaining = _bgMusic.Stream.GetLength() - _bgMusic.GetPlaybackPosition();

			if (remaining < _endThreshold)
			{
				_isFadingOut = true;
				FadeVolume(-40f);
			}
		}
	}
}
