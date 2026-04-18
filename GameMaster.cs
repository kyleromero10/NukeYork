using Godot;
using System;
using System.Threading.Tasks;

public partial class GameMaster : Node
{
	[Export] public NetID myId;
	[Export] public NetID localID;
	[Export] public bool GameStarted;
	[Export] public float GameTimer = 10;
	[Export] public float EnemyWaveTimer = 10;
	[Export] public float disconnectTimer = 10;
	public Random rand = new Random();
	[Export] public Vector2 winnerPos = Vector2.Zero;
	[Export] public int randLevel;
	private int levelChoice;
	private bool _scoreboardShown = false;

	[Export] public bool GameFinished = false;
	[Export] public Control UI;
	[Export] public ScoreboardUI scoreboardUI;

	public override void _Ready()
	{
		base._Ready();
		GameCycle();
	}

	public override async void _Process(double delta)
	{
		if (!myId.IsNetworkReady || !GenericCore.Instance.IsGenericCoreConnected)
			return;

		if (GameStarted && !GameFinished)
		{
			if (GenericCore.Instance.IsServer)
			{
				GameTimer -= (float)delta;
				EnemyWaveTimer -= (float)delta;

				if (EnemyWaveTimer <= 0)
				{
					EnemyWaveTimer = 10;
					var spawns = GetTree().GetNodesInGroup("ESpawns");
					foreach (Marker2D spawn in spawns)
					{
						GD.Print("Spawning enemy at: " + spawn.GlobalPosition);
						int enemyType = rand.Next(1, 6);
						if (enemyType == 1 || enemyType == 2)
							enemyType = 1;
						else if (enemyType == 3 || enemyType == 4)
							enemyType = 2;
						else if (enemyType == 5)
							enemyType = 3;

						GenericCore.Instance.MainNetworkCore.NetCreateObject(
							enemyType + 2,
							new Vector3(spawn.GlobalPosition.X, spawn.GlobalPosition.Y, 0),
							Quaternion.Identity,
							localID.OwnerId);
					}
				}

				if (GameTimer <= 0)
				{
					Rpc(MethodName.SetGameFinishedRPC);
				}
			}
		}

		if (GameFinished)
		{
			if (!_scoreboardShown)
			{
				_scoreboardShown = true;
				await ToSignal(GetTree().CreateTimer(0.5f), SceneTreeTimer.SignalName.Timeout);
				scoreboardUI?.ShowScoreboard();
			}

			if (GenericCore.Instance.IsServer)
			{
				var players = GetTree().GetNodesInGroup("Player");
				if (players.Count == 0) return;

				disconnectTimer -= (float)delta;

				var enemies = GetTree().GetNodesInGroup("Enemy");
				foreach (Enemy enemy in enemies)
				{
					GD.Print("Destroying enemy: ");
					GenericCore.Instance.MainNetworkCore.NetDestroyObject(enemy.myId);
				}

				PlayerCharacter winner = (PlayerCharacter)players[0];

				foreach (PlayerCharacter player in players)
				{
					player.state = PlayerCharacter.PlayerState.Finish;

					if (player.totalKills > winner.totalKills)
					{
						winner = player;
						winnerPos = player.GlobalPosition;
					}
				}

				GD.Print("Winner: " + winner.playerName);
				winner.state = PlayerCharacter.PlayerState.Win;

				foreach (PlayerCharacter player in players)
				{
					if (player.myId.IsLocal)
					{
						player.myCamera.Position = winnerPos;
					}
				}

				if (disconnectTimer <= 0)
				{
					Rpc(MethodName.DisconnectClientsRPC);
					await ToSignal(GetTree().CreateTimer(.5f), SceneTreeTimer.SignalName.Timeout);
					GenericCore.Instance.DisconnectServer();
				}
			}
		}
	}

	public async void GameCycle()
	{
		while (!Node.IsInstanceValid(GenericCore.Instance) || !GenericCore.Instance.IsGenericCoreConnected)
		{
			await ToSignal(GetTree().CreateTimer(2.5f), SceneTreeTimer.SignalName.Timeout);
		}

		if (!GenericCore.Instance.IsServer)
			return;

		while (!GameStarted)
		{
			var x = GetTree().GetNodesInGroup("NPM");

			if (x.Count >= 2)
			{
				GameStarted = true;
				GD.Print("-A-");

				foreach (UserNpm node in x)
				{
					if (!node.IsReady)
						GameStarted = false;
				}
			}

			await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		}

		Rpc(MethodName.HideNPMsRPC);

		var myNPMs = GetTree().GetNodesInGroup("NPM");
		int count = 0;
		int count2 = 0;

		randLevel = rand.Next(0, myNPMs.Count);

		foreach (UserNpm node in myNPMs)
		{
			while (string.IsNullOrEmpty(node.PlayerName))
			{
				await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
			}
		}

		foreach (UserNpm node in myNPMs)
		{
			if (count2 == randLevel)
				levelChoice = node.LevelChoice;
			count2++;
		}

		GenericCore.Instance.MainNetworkCore.NetCreateObject(
			levelChoice + 6,
			Vector3.Zero,
			Quaternion.Identity);

		var spawns = GetTree().GetNodesInGroup("PlayerSpawns");

		foreach (UserNpm node in myNPMs)
		{
			Marker2D spawn = (Marker2D)spawns[count];

			PlayerCharacter temp =
				(PlayerCharacter)GenericCore.Instance.MainNetworkCore.NetCreateObject(
					node.Character,
					new Vector3(spawn.GlobalPosition.X, spawn.GlobalPosition.Y, 0),
					Quaternion.Identity,
					node.myID.OwnerId);

			temp.playerName = node.PlayerName;
			temp.currentHealth = temp.maxHealth;
			temp.radiation = 0;
			temp.totalKills = 0;

			count++;
			localID = temp.myId;
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true,
		TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void SetGameFinishedRPC()
	{
		GameFinished = true;
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false,
		TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void HideNPMsRPC()
	{
		var x = GetTree().GetNodesInGroup("NPM");
		foreach (UserNpm node in x)
			node.Hide();
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false,
		TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void DisconnectClientsRPC()
	{
		if (!GenericCore.Instance.IsServer)
			GenericCore.Instance.DisconnectFromGame();
	}
}
