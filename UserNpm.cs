using Godot;
using System;
using System.ComponentModel;

public partial class UserNpm : Control
{
	[Export] public string PlayerName;
	[Export] public int Character;
	[Export] public int LevelChoice;
	[Export] public bool IsReady;
	[Export] public TextEdit MyName;
	[Export] public NetID myID;
	[Export] public ItemList MyCharacter;
	[Export] public ItemList Level;
	[Export] public CheckBox MyCheckBox;
	[Export] public Sprite2D SpriteIdle;

	public override void _Ready()
	{
		base._Ready();
		slowStart();
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		if(!myID.IsLocal)
		{
			MyCheckBox.ButtonPressed = IsReady;
		}
	}


	public async void slowStart()
	{
		await ToSignal(GetTree().CreateTimer(0.15f), SceneTreeTimer.SignalName.Timeout);
		//Go ahead and set values
		if(!myID.IsLocal)
		{
			Hide();
		}
	}

	public void OnNameChanged()
	{
		if(myID.IsLocal)
		{
			PlayerName = MyName.Text;
			//Figure out how to get message to server
			Rpc(MethodName.NameChangeRPC, MyName.Text);
		}
	}

	/*public void OnTextSet()
	{
		
	}*/
	public void OnCharacterChange(int n)
	{
		if(myID.IsLocal)
		{
			SpriteIdle.Frame = n+1;
			//Team = n;
			RpcId(1,"CharacterChangeRPC", n);
			//Rpc(MethodName.CharacterChangeRPC, n);
		}
	}
	
	public void OnLevelChange(int n)
	{
		if(myID.IsLocal)
		{
			//Team = n;
			RpcId(1,"LevelChangeRPC", n);
			//Rpc(MethodName.CharacterChangeRPC, n);
		}
	}

	public void OnToggleChange(bool r)
	{
		if(myID.IsLocal)
		{
			Rpc(MethodName.IsReadyChange, r);
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false,
		TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void IsReadyChange(bool n)
	{
		if(GenericCore.Instance.IsServer)
		{
			IsReady = n;
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false,
		TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public async void NameChangeRPC(string n)
	{
		if(GenericCore.Instance.IsServer)
		{
			PlayerName = n;
			MyName.Text = PlayerName;
			await ToSignal(GetTree().CreateTimer(0.15f), SceneTreeTimer.SignalName.Timeout);
			MyName.SetCaretColumn(MyName.Text.Length);
			GD.Print("Caret Column: ", MyName.GetCaretColumn());
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false,
		TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public async void CharacterChangeRPC(int n)
	{
		if(GenericCore.Instance.IsServer)
		{
			Character = n;
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false,
		TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public async void LevelChangeRPC(int n)
	{
		if(GenericCore.Instance.IsServer)
		{
			LevelChoice = n;
		}
	}

	
}
