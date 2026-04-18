using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public partial class ScoreboardUI : Control
{
	[Export] public VBoxContainer scoreList;
	[Export] public Label titleLabel;
	[Export] public Label continueLabel;

	private bool hasBuilt = false;

	public override void _Ready()
	{
		Visible = false;
	}

	public void ShowScoreboard()
	{
		GD.Print("ShowScoreboard called");
		Visible = true;

		ClearList();

		List<PlayerCharacter> players = new List<PlayerCharacter>();

		foreach (var p in GetTree().GetNodesInGroup("Player"))
		{
			if (p is PlayerCharacter pc)
			{
				players.Add(pc);
			}
		}

		// Sort by kills (highest first)
		players = players
			.OrderByDescending(p => p.totalKills)
			.ToList();

		int rank = 1;

		foreach (var player in players)
		{
			AddRow(player.playerName, player.totalKills, rank);
			rank++;
		}

		if (continueLabel != null)
		{
			continueLabel.Text = "Server closing session...";
		}
	}

	private void AddRow(string playerName, int kills, int rank)
	{
		HBoxContainer row = new HBoxContainer();
		row.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

		Label nameLabel = new Label();
		nameLabel.Text = $"{rank}. {playerName}";
		nameLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

		Label killLabel = new Label();
		killLabel.Text = kills.ToString();
		killLabel.HorizontalAlignment = HorizontalAlignment.Right;

		row.AddChild(nameLabel);
		row.AddChild(killLabel);

		scoreList.AddChild(row);
	}

	private void ClearList()
	{
		foreach (var child in scoreList.GetChildren())
		{
			child.QueueFree();
		}
	}
}
