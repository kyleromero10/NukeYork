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
    [Export] public float  prepAttackTime = 2f;
    public bool isPreppingAttack = false;
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
            flipSprite();
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
            if(isPreppingAttack && CanAttack())
            {
                prepAttackTime -= (float)delta;
                if(prepAttackTime <= 0)
                {
                    prepAttackTime = 2f;
                    isPreppingAttack = false;
                    RpcId(1,"AttackRPC", 1);
                }
            }
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
            else if(state == PlayerState.Dead)
            {
                animPlayer.Play("Death");
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
        if(closestDistance < 40)
        {
            directionToPlayer = Vector2.Zero;
            isPreppingAttack = true;
        }
        else
        {
            isPreppingAttack = false;
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
                state = PlayerState.Moving;
            }
        }
    }

    public void flipSprite()
    {
        if(Velocity.X < 0)
        {
            hitbox.Scale= new Vector2(-1,1);
            sprite.FlipH = true;
        }
        else if(Velocity.X > 0)
        {
            hitbox.Scale= new Vector2(1,1);
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

    public void SetMonitorTrue()
    {
        RpcId(1,"SetMonitorRPC", 1);
    }

    public void SetMonitorFalse()
    {
        RpcId(1,"SetMonitorRPC", 2);
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
            currentHealth -= Damage;
            if(currentHealth <= 0)
            {
                state = PlayerState.Dead;
                currentHealth = 0;
                GD.Print("Enemy died");
            }
            else
            {
                state = PlayerState.Hurt;
                GD.Print("Enemy hurt");
            }
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false,
    TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SetMonitorRPC(int set)
    {
        if (GenericCore.Instance.IsServer)
        {         
            if(set == 1)
            {
                hitbox.Monitoring = true;
            }
            else if(set == 2)
            {
                hitbox.Monitoring = false;
            }
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
