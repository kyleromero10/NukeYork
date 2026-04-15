using Godot;
using System;

public partial class GameMaster : Node
{
    [Export]
    public NetID myId;

    [Export] public NetID localID;

    [Export]
    public bool GameStarted;

    [Export] public float GameTimer = 360;

    [Export]
    public bool GameFinished = false;

    [Export] public Control UI;

    public override void _Ready()
    {
        base._Ready();
        GameCycle();
    }

    public override void _Process(double delta)
    {
        /*if(!myId.IsSynced)
        {
            return;
        }*/
        if(!myId.IsNetworkReady)
        {
            return;
        }

        if(GameStarted && !GameFinished)
        {
            if(GenericCore.Instance.IsServer)
            {
                GameTimer -= (float)delta;
                //GD.Print("Game Timer: " + GameTimer);

                if(GameTimer <= 0)
                {
                    GameFinished = true;
                }
            }
            //Check for end game conditions.
            //If met, set GameFinished to true and do end game stuff.
        }

        if(GameFinished)
        {
            if(GenericCore.Instance.IsServer)
            {
                var t = GetTree().GetNodesInGroup("Player");
                PlayerCharacter winner = null;
                foreach(PlayerCharacter player in t)
                {
                    if(winner == null || player.totalKills > winner.totalKills)
                    {
                        winner = player;
                    }
                }
                GD.Print("Winner: " + winner.playerName);
            }
            //Show end game screen.
        }
    }

    public async void GameCycle()
    {
        //Make sure generic core is server.
        while(!Node.IsInstanceValid(GenericCore.Instance) || !GenericCore.Instance.IsGenericCoreConnected)
        {
            await ToSignal(GetTree().CreateTimer(2.5f), SceneTreeTimer.SignalName.Timeout);
        }
        if(!GenericCore.Instance.IsServer)
        {
            return;
        }
        

        //Find all NPMS
        //Wait until they are all "ready"
        while(!GameStarted)
        {
            //Find all NPMS
            var x = GetTree().GetNodesInGroup("NPM");
            if(x.Count>=2)
            {
                GameStarted = true;
                GD.Print("-A-");
                foreach (UserNpm node in x)
                {
                    if(!node.IsReady)
                    {
                        GameStarted = false;
                        //break;
                    }
                }
            }

            //await ToSignal(GetTree().CreateTimer(2.5f), SceneTreeTimer.SignalName.Timeout);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }

         Rpc(MethodName.HideNPMsRPC);

        var myNPMs = GetTree().GetNodesInGroup("NPM");
        var spawns = GetTree().GetNodesInGroup("PlayerSpawns");
        int count = 0;
        //Create the characters
        foreach(UserNpm node in myNPMs)
        {
            Marker2D spawn = (Marker2D)spawns[count];
            PlayerCharacter temp = (PlayerCharacter)GenericCore.Instance.MainNetworkCore.NetCreateObject(node.Character, new Vector3(spawn.GlobalPosition.X, spawn.GlobalPosition.Y, 0),
                Quaternion.Identity, node.myID.OwnerId); //Spawn the player character.
            temp.playerName = "P" + (count + 1);
            count++;
            localID = temp.myId;
        }

        GenericCore.Instance.MainNetworkCore.NetCreateObject(2, new Vector3(300, 200, 0),
                Quaternion.Identity, localID.OwnerId); //Spawn an enemy for testing.

        //Create the users' characters.
        //Use Node2D or 3D spawner to spawn level.
        //Game should be able to start.
    }


    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false,
        TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void HideNPMsRPC()
    {
        var x = GetTree().GetNodesInGroup("NPM");
        foreach (UserNpm node in x)
        {
            node.Hide();
        }
    }

    
}
