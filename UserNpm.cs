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
	[Export] public float R = 1f;
	[Export] public float G = 1f;
	[Export] public float B = 1f;
	[Export] public HSlider RSlider;
	[Export] public HSlider GSlider;
	[Export] public HSlider BSlider;
	[Export] public Label RLabel;
	[Export] public Label GLabel;
	[Export] public Label BLabel;

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

	public void OnRChanged(float value)
	{
		if (!myID.IsLocal) return;

		R = value;

		if (RLabel != null)
			RLabel.Text = ((int)(R)).ToString();
		R = value / 255f;
		if (SpriteIdle != null)
			SpriteIdle.Modulate = new Color(R, G, B);
	}

	public void OnGChanged(float value)
	{
		if (!myID.IsLocal) return;
	
		G = value;

		if (GLabel != null)
			GLabel.Text = ((int)(G)).ToString();
		G = value / 255f;	
		if (SpriteIdle != null)
			SpriteIdle.Modulate = new Color(R, G, B);
	}

	public void OnBChanged(float value)
	{
		if (!myID.IsLocal) return;

		B = value;

		if (BLabel != null)
			BLabel.Text = ((int)(B)).ToString();
		B = value / 255f;
		if (SpriteIdle != null)
			SpriteIdle.Modulate = new Color(R, G, B);
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
