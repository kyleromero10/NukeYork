using Godot;
using System;

public partial class PlayerCharacter : CharacterBody2D
{
    [Export] public NetID myId;
    //[Export] public Label nameBox;
    [Export] public AnimationPlayer animPlayer;
    [Export] public Sprite2D sprite;
    [Export] public Area2D hitbox;

    [Export] public string playerName;
	[Export] public int health;
	[Export] public int damageMultiplier;
	[Export] public float speed = 300;
    [Export] public Vector2 SyncedVelocity
	{
		get => Velocity;
		set => Velocity = value;
	}
    [Export] public PlayerState state = PlayerState.Idle;
    

    public enum PlayerState
    {
        Idle,
        Moving,
        LightAttack,
        HeavyAttack,
        Hurt,
        Dead
    }

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
            HandleMovement();
            MoveAndSlide();
        }
        if (myId.IsLocal)
        {
            Vector2 myInputAxis = new Vector2(
                Input.GetAxis("Left", "Right"),
                Input.GetAxis("Up", "Down")
                );
            RpcId(1,"MoveMeRPC", myInputAxis);

            if(Input.IsActionJustPressed("LightAttack") && CanAttack())
            {
                int attack = 1;
                RpcId(1,"AttackRPC", attack);
            }
            if(Input.IsActionJustPressed("HeavyAttack") && CanAttack())
            {
                int attack = 2;
                RpcId(1,"AttackRPC", attack);
            }
        }
        if(!GenericCore.Instance.IsServer)
        {
            flipSprite();

            if(state == PlayerState.Idle)
            {
                animPlayer.Play("Idle");
            }
            else if(state == PlayerState.Moving)
            {
                animPlayer.Play("Walk");
            }
            else if(state == PlayerState.LightAttack)
            {
                animPlayer.Play("LightAttack");
            }
            else if(state == PlayerState.HeavyAttack)
            {
                animPlayer.Play("HeavyAttack");
            }
            
        }
    }


    public void HandleMovement()
    {
        if(CanMove())
        {
            if(Velocity.Length() > 0)
            {
                state = PlayerState.Moving;
            }
            else
            {
                state = PlayerState.Idle;
            }
        }
    }

    public void flipSprite()
    {
        if(Velocity.X < 0)
        {
            sprite.FlipH = true;
        }
        else if(Velocity.X > 0)
        {
            sprite.FlipH = false;
        }
    }

    public bool CanMove()
    {
        return state == PlayerState.Idle || state == PlayerState.Moving;
    }
    public bool CanAttack()
    {
        return state == PlayerState.Idle || state == PlayerState.Moving;
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false,
    TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void MoveMeRPC(Vector2 dir)
    {
        if (GenericCore.Instance.IsServer)
        {          
            SyncedVelocity = dir.Normalized() * speed;
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false,
    TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void AttackRPC(int attackType)
    {
        if (GenericCore.Instance.IsServer)
        {          
            if(attackType == 1)
            {
                state = PlayerState.LightAttack;
            }
            else if(attackType == 2)
            {
                state = PlayerState.HeavyAttack;
            }
        }
    }

    public void AttackComplete()
    {
        RpcId(1,"AttackCompleteRPC");
    }

    public void SetMonitor()
    {
        RpcId(1,"SetMonitorRPC");
    }

    public void onAreaEntered(Area2D area)
    {
        if(GenericCore.Instance.IsServer)
        {
            GD.Print("Hitbox entered: " + area.Name);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false,
    TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SetMonitorRPC()
    {
        if (GenericCore.Instance.IsServer)
        {         
            if(hitbox.Monitoring == true)
            {
                hitbox.Monitoring = false;
            }
            else 
                hitbox.Monitoring = true;
        }
    }
    
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false,
    TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void AttackCompleteRPC()
    {
        if (GenericCore.Instance.IsServer)
        {          
            state = PlayerState.Idle;
        }
    }
}
