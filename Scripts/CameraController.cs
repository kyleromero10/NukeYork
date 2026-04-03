using Godot;
using System.Collections.Generic;

//WIP FOR CAMERA CALCULATION

public partial class CameraController : Node
{
	[Export] public Camera2D Camera;
	[Export] public float Smooth = 8f;

	[Export] public float MinX = -999999f;
	[Export] public float MaxX =  999999f;
	[Export] public float MinY = -999999f;
	[Export] public float MaxY =  999999f;

	public override void _Process(double delta)
	{
		if (Camera == null) return;

		var players = GetTree().GetNodesInGroup("players");
		if (players.Count == 0) return;

		Vector2 sum = Vector2.Zero;
		int count = 0;

		foreach (var n in players)
		{
			if (n is Node2D p)
			{
				sum += p.GlobalPosition;
				count++;
			}
		}

		if (count == 0) return;

		Vector2 avg = sum / count;
		avg.X = Mathf.Clamp(avg.X, MinX, MaxX);
		avg.Y = Mathf.Clamp(avg.Y, MinY, MaxY);

		// Exponential smoothing that behaves well with variable delta
		float t = 1f - Mathf.Exp(-Smooth * (float)delta);
		Camera.GlobalPosition = Camera.GlobalPosition.Lerp(avg, t);
	}
}
