using Godot;
using System;

public partial class GameMaster : Node
{
    [Export]
    public NetID myId;

    [Export]
    public bool GameStarted;

    [Export]
    public bool GameFinished;

    [Export] public Control UI;

    public override void _Ready()
    {
        base._Ready();
        GameCycle();
    }

    public override void _Process(double delta)
    {
        if(!myId.IsSynced)
        {
            return;
        }
        if(GameStarted && !GameFinished)
        {
            //Check for end game conditions.
            //If met, set GameFinished to true and do end game stuff.
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

        var myNPMs = GetTree().GetNodesInGroup("NPM");
        int count = 0;
        //Create the characters
        foreach(UserNpm node in myNPMs)
        {

            PlayerCharacter temp = (PlayerCharacter)GenericCore.Instance.MainNetworkCore.NetCreateObject(node.Character, new Vector3(128 * count, 200, 0),
                Quaternion.Identity, node.myID.OwnerId);
            //temp.myColor = node.MyColor;
            count++;
        }

        //Create the users' characters.
        //Use Node2D or 3D spawner to spawn level.
        //Game should be able to start.
    }

    public void HideNPMs()
    {
        var x = GetTree().GetNodesInGroup("NPM");
        foreach (UserNpm node in x)
        {
            node.Hide();
        }
    }
}
