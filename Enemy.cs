using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
    [Export] public NetID myId;
    //[Export] public Label nameBox;
    [Export] public AnimationPlayer animPlayer;
    [Export] public Sprite2D sprite;
    [Export] public Area2D hitbox;

    [Export] public int maxHealth;
    [Export] public int currentHealth;
	[Export] public int damageMultiplier;
	[Export] public float speed = 100;
    [Export] public Vector2 SyncedVelocity
	{
		get => Velocity;
		set => Velocity = value;
	}
    [Export] public PlayerState state = PlayerState.Moving;
    Vector2 directionToPlayer = Vector2.Zero;
    

    public enum PlayerState
    {
        Moving,
        Attack,
        Hurt,
        Dead
    }

    public override void _Process(double delta)
    {
        if(!myId.IsSynced)
        {
            GD.Print("Not synced yet");
            return;
        }
        if (GenericCore.Instance.IsServer)
        {
            HandleMovement();
            MoveAndSlide();
        }
        if (myId.IsLocal)
        {
            /*var players = GetTree().GetNodesInGroup("Player");
            foreach(PlayerCharacter player in players)
            {
                var distance = GlobalPosition.DistanceTo(player.GlobalPosition);
                Vector2 directionToPlayerCheck = player.GlobalPosition - GlobalPosition;
                //GD.Print("Direction to player: " + directionToPlayerCheck);
                if(directionToPlayerCheck.Length() > directionToPlayer.Length() || directionToPlayer == Vector2.Zero)
                {
                    directionToPlayer = directionToPlayerCheck;
                }
            }*/
            GetClosestPlayer();
            if(CanMove())
            {
                RpcId(1,"MoveMeRPC", directionToPlayer);
            }
            else
            {
                RpcId(1,"MoveMeRPC", Vector2.Zero);
            }
        }
        if(!GenericCore.Instance.IsServer)
        {
            flipSprite();


            if(state == PlayerState.Moving)
            {
                animPlayer.Play("Walk");
            }
            else if(state == PlayerState.Attack)
            {
                animPlayer.Play("Attack");
            }
            else if(state == PlayerState.Hurt)
            {
                animPlayer.Play("Hurt");
            }
            
        }
    }


    public void GetClosestPlayer()
    {
        var players = GetTree().GetNodesInGroup("Player");
        var closestPlayer = players[0] as PlayerCharacter;
        var closestDistance = GlobalPosition.DistanceTo(closestPlayer.GlobalPosition);
        foreach(PlayerCharacter player in players)
        {
            var distance = GlobalPosition.DistanceTo(player.GlobalPosition);
            //Vector2 directionToPlayerCheck = player.GlobalPosition - GlobalPosition;
            //GD.Print("Direction to player: " + directionToPlayerCheck);
            if(distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = player;
            }
        }
        directionToPlayer = closestPlayer.GlobalPosition - GlobalPosition;
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
                state = PlayerState.Moving;
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
        return state == PlayerState.Moving;
    }
    public bool CanAttack()
    {
        return state == PlayerState.Moving;
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false,
    TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void MoveMeRPC(Vector2 dir)
    {
        if (GenericCore.Instance.IsServer)
        {          
            SyncedVelocity = dir.Normalized() * speed;
            //GD.Print("Synced velocity: " + SyncedVelocity);
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
                state = PlayerState.Attack;
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

    public void onHitboxEntered(DamageReceiver damageReceiver)
    {
        if(GenericCore.Instance.IsServer)
        {
            damageReceiver.EmitSignal("OnHit", damageMultiplier);
            GD.Print("Hitbox entered: " + damageReceiver.Name);
        }
    }

    public void onHit(int Damage)
    {
        if(GenericCore.Instance.IsServer)
        {
            state = PlayerState.Hurt;
            currentHealth -= Damage;
            GD.Print("Hurtbox entered: ");
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
            state = PlayerState.Moving;
        }
    }

}
