using Godot;
using System;

public partial class PlayerUi : Control
{
	[Export] public NetID myId;
	[Export] public Label nameLabel;
	[Export] public ProgressBar healthBar;
	[Export] public ProgressBar radiationBar;
	[Export] public Label timerLabel;
	[Export] public GameMaster gameMaster;
	[Export] public PlayerCharacter player;

	public override void _Process(double delta)
	{
		if (GenericCore.Instance.IsServer)
		{
			if (gameMaster.GameStarted && !gameMaster.GameFinished)
			{
				timerLabel.Text =
					"Time Remaining: " +
					((int)gameMaster.GameTimer / 10) +
					((int)gameMaster.GameTimer % 10);
			}
		}

		if (player == null)
		{
			foreach (var p in GetTree().GetNodesInGroup("Player"))
			{
				PlayerCharacter pc = p as PlayerCharacter;
				if (pc != null && pc.myId.IsLocal)
				{
					player = pc;
					break;
				}
			}
			return;
		}

		healthBar.MaxValue = player.maxHealth;
		healthBar.Value = player.currentHealth;

		radiationBar.MaxValue = 100;
		radiationBar.Value = player.radiation;
	}
}
