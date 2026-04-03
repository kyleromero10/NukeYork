using Godot;
using System;

public partial class PlayerCharacter : CharacterBody2D
{
    [Export]
    public NetID myId;
    [Export]
    public Label nameBox;

    [Export] public string playerName;
	[Export] public int health;
	[Export] public int damageMultiplier;
	[Export] public float speed = 300;

    public void Synchronized()
    {
       
        //nameBox.Text = playerName;
    }


    //This was an error on the template.
    //Also need to synchronize position on players and levels.
    //Also need to setup synchronize Signal on all characters.
    public override void _Process(double delta)
    {
        if(!myId.IsSynced)
        {
            return;
        }
        if (GenericCore.Instance.IsServer)
        {
            MoveAndSlide();
        }
        if (myId.IsLocal)
        {
            Vector2 myInputAxis = new Vector2(
                Input.GetAxis("left", "right"),
                Input.GetAxis("up", "down")
                );
            RpcId(1,"MoveMe", myInputAxis);

            if(Input.IsActionJustPressed("LightAttack"))
            {
                
            }
        }
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false,
    TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void MoveMe(Vector2 dir)
    {
        if (GenericCore.Instance.IsServer)
        {          
            Velocity = dir.Normalized() * speed;
        }
    }
}
