using Godot;
using System;
using System.Collections;

public partial class Crate : StaticBody2D
{
    public void OnAreaEntered(Area2D area)
    {
        /*if(GenericCore.Instance.IsServer)
        {
            var p =  GenericCore.Instance.MainNetworkCore.NetCreateObject
                (1,
                new Vector3(GlobalPosition.X, GlobalPosition.Y, 0),
                Quaternion.Identity,
                1
                );
        }*/
        QueueFree();
    }
}
