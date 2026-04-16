using Godot;
using System;

public partial class PlayerCharacter : CharacterBody2D
{
    [Export] public NetID myId;
    //[Export] public Label nameBox;
    [Export] public AnimationPlayer animPlayer;
    [Export] public Sprite2D sprite;
    [Export] public Area2D hitbox;
    [Export] public Area2D heavyHitbox;

    [Export] public string playerName;
	[Export] public int maxHealth;
    [Export] public int currentHealth;
    [Export] public int radiation;
    [Export] public bool isInRadiation;
    [Export] public float deathTimer = 5;
	[Export] public int damageMultiplier;
	[Export] public float speed = 300;
    [Export] public int totalKills;

    [Export] public Camera2D myCamera;
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
        Dead,
        Win,
        Finish
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
        if(!myId.IsNetworkReady || !GenericCore.Instance.IsGenericCoreConnected)
        {
            return;
        }
        if (GenericCore.Instance.IsServer)
        {
            if(state == PlayerState.Dead && deathTimer > 0)
            {
                deathTimer -= (float)delta;
                GD.Print("Death timer: " + deathTimer);
                if(deathTimer <= 0)
                {

                    deathTimer = 0;
                    currentHealth = maxHealth;
                    state = PlayerState.Idle;
                    deathTimer = 3f;
                }
            }
            if(radiation >= 100)
            {
                isInRadiation = true;
            }
            else if(radiation <= 0)
            {
                isInRadiation = false;
                radiation = 0;
            }

            flipSprite();
            HandleMovement();
            MoveAndSlide();
        }
        if (myId.IsLocal)
        {
            Vector2 myInputAxis = new Vector2(
                Input.GetAxis("Left", "Right"),
                Input.GetAxis("Up", "Down")
                );
            if(CanMove())
            {
                RpcId(1,"MoveMeRPC", myInputAxis);
            }
            else
            {
                RpcId(1,"MoveMeRPC", Vector2.Zero);
            }

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

            if (myCamera == null)
            {
                // Grab the existing camera from the scene
                myCamera = GetViewport().GetCamera2D();
                //GD.Print("myCamera: " + myCamera);

                if (myCamera != null)
                {
                    // Reparent the camera to this character so it follows automatically
                    Node previousParent = myCamera.GetParent();
                    previousParent?.RemoveChild(myCamera);
                    AddChild(myCamera);
                }
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
            else if(state == PlayerState.Hurt)
            {
                animPlayer.Play("Hurt");
            }
            else if(state == PlayerState.Dead)
            {
                animPlayer.Play("Death");
            }
            else if(state == PlayerState.Win)
            {
                animPlayer.Play("Win");
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
            hitbox.Scale= new Vector2(-1,1);
            heavyHitbox.Scale= new Vector2(-1,1);
            sprite.FlipH = true;
        }
        else if(Velocity.X > 0)
        {
            hitbox.Scale= new Vector2(1,1);
            heavyHitbox.Scale= new Vector2(1,1);
            sprite.FlipH = false;
        }
    }

    public void activateRadiation()
    {
        isInRadiation = true;
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

    public void VictoryComplete()
    {
        RpcId(1,"VictoryCompleteRPC");
    }
    public void ActionComplete()
    {
        RpcId(1,"ActionCompleteRPC");
    }

    public void SetMonitorTrue()
    {
        RpcId(1,"SetMonitorRPC", 1);
    }

    public void SetMonitorFalse()
    {
        RpcId(1,"SetMonitorRPC", 2);
    }

    public void SetHeavyMonitorTrue()
    {
        RpcId(1,"SetHeavyMonitorRPC", 1);
    }

    public void SetHeavyMonitorFalse()
    {
        RpcId(1,"SetHeavyMonitorRPC", 2);
    }

    public void onHitboxEntered(DamageReceiver damageReceiver)
    {
        if(GenericCore.Instance.IsServer)
        {
            if(damageReceiver is DamageReceiver)
            {
                damageReceiver.EmitSignal("OnHitEnemy", damageMultiplier, this);
                radiation += 5;
                GD.Print("Hitbox entered: " + damageReceiver.Name);
            }
        }
    }

    public void onHeavyHitboxEntered(DamageReceiver damageReceiver)
    {
        if(GenericCore.Instance.IsServer)
        {
            if(damageReceiver is DamageReceiver)
            {
                damageReceiver.EmitSignal("OnHitEnemy", damageMultiplier*2, this);
                radiation += 5;
                GD.Print("Hitbox entered: " + damageReceiver.Name);
            }
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
                radiation += 2;
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
                hitbox.Monitorable = true;
            }
            else if(set == 2)
            {
                hitbox.Monitoring = false;
                hitbox.Monitorable = false;
            }
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false,
    TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SetHeavyMonitorRPC(int set)
    {
        if (GenericCore.Instance.IsServer)
        {         
            if(set == 1)
            {
                heavyHitbox.Monitoring = true;
                heavyHitbox.Monitorable = true;
            }
            else if(set == 2)
            {
                heavyHitbox.Monitoring = false;
                heavyHitbox.Monitorable = false;
            }
        }
    }
    
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false,
    TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void ActionCompleteRPC()
    {
        if (GenericCore.Instance.IsServer)
        {          
            state = PlayerState.Idle;
        }
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false,
    TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void VictoryCompleteRPC()
    {
        if (GenericCore.Instance.IsServer)
        {          
            state = PlayerState.Finish;
        }
    }
}
