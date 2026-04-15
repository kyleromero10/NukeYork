using Godot;
using System;

public partial class PlayerUi : Control
{
    [Export] public NetID myId;
    [Export] public Label nameLabel;
    [Export] public Label healthLabel;
    [Export] public Label radiationLabel;
    [Export] public Label timerLabel;
    [Export] public GameMaster gameMaster;
    [Export] public PlayerCharacter player;

    public override void _Process(double delta)
    {
        if(GenericCore.Instance.IsServer)
        {
            if(gameMaster.GameStarted && !gameMaster.GameFinished)
            {
                timerLabel.Text = "Time Remaining: " + (int)gameMaster.GameTimer/60 + ":" + (int)gameMaster.GameTimer%60;
            }
            
        }
        
    }

}
