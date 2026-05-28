using Godot;

namespace DesktopAssistant.Scripts;

public partial class GameMenu : Control
{
	[Export]
	public Button ReturnMainMenuButton { get; set; }

	[Export]
	public Button ContinueGameButton { get; set; }

	public override void _Ready()
	{
		Visible = false;
		ProcessMode = ProcessModeEnum.Always;

		ContinueGameButton.Pressed += ContinueGame;
		ReturnMainMenuButton.Pressed += ReturnMainMenu;
	}

	public override void _Input(InputEvent @event)
	{
		if (Input.IsActionJustPressed("open_game_menu"))
		{
			if (Visible) ContinueGame();
			else OpenGameMenu();
		}
	}

	void OpenGameMenu()
	{
		GetTree().Paused = true;
		Visible = true;
	}

	void ContinueGame()
	{
		GetTree().Paused = false;
		Visible = false;
	}

	void ReturnMainMenu()
	{
		GetTree().Paused = false;
		GetTree().ChangeSceneToFile("res://Scenes/Main.tscn");
	}
}
