using Godot;
using System.Collections.Generic;

public partial class AudioManager : Node
{
	[Export] public AudioStreamPlayer Music;
	[Export] public Node SfxPool;

	[Export] public int SfxPoolSize = 12;

	private readonly Dictionary<string, AudioStream> _bgm = new();
	private readonly Dictionary<string, AudioStream> _sfx = new();
	
	private string _currentLevelBgmKey = "";
	private double _scanCooldown = 0.0;

	private readonly List<AudioStreamPlayer> _sfxPlayers = new();

	public override void _Ready()
	{
		if (Music == null)
{
	GD.PushError("AudioManager: Music reference not set (exported field).");
	return;
}
if (SfxPool == null)
{
	GD.PushError("AudioManager: SfxPool reference not set (exported field).");
	return;
}
		
		// Load BGM
		_bgm["Level1"] = GD.Load<AudioStream>("res://Assets/Audio/BGM/CityLevel.wav");
		_bgm["Level2"] = GD.Load<AudioStream>("res://Assets/Audio/BGM/BunkerLevel.wav");
		_bgm["Level3"] = GD.Load<AudioStream>("res://Assets/Audio/BGM/LabLevel.wav");

		// Load SFX
		_sfx["ButtonClick"] = GD.Load<AudioStream>("res://Assets/Audio/SFX/ButtonClick.wav");
		_sfx["LightMelee"]  = GD.Load<AudioStream>("res://Assets/Audio/SFX/LightMelee.wav");
		_sfx["HeavyMelee"]  = GD.Load<AudioStream>("res://Assets/Audio/SFX/HeavyMelee.wav");
		_sfx["ShootAttack"] = GD.Load<AudioStream>("res://Assets/Audio/SFX/ShootAttack.wav");
		_sfx["Hurt"]        = GD.Load<AudioStream>("res://Assets/Audio/SFX/Hurt.wav");
		_sfx["Revive"]      = GD.Load<AudioStream>("res://Assets/Audio/SFX/Revive.wav");
		_sfx["YouWin"]      = GD.Load<AudioStream>("res://Assets/Audio/SFX/YouWin.wav");

		_sfx["LightEnemyAttack"]      = GD.Load<AudioStream>("res://Assets/Audio/SFX/LightEnemyAttack.wav");
		_sfx["HeavyEnemyAttack"]      = GD.Load<AudioStream>("res://Assets/Audio/SFX/HeavyEnemyAttack.wav");
		_sfx["LabSpecialEnemyAttack"] = GD.Load<AudioStream>("res://Assets/Audio/SFX/LabSpecialEnemyAttack.wav");

		// Build SFX pool
		for (int i = 0; i < SfxPoolSize; i++)
		{
			var p = new AudioStreamPlayer();
			p.Bus = "SFX";
			SfxPool.AddChild(p);
			_sfxPlayers.Add(p);
		}

		// Music defaults
		Music.Bus = "Music";
		Music.Autoplay = false;
	}
	
	[Callable]
	public void PlayBgmForLevel(int levelIndex)
	{
		string key = levelIndex switch
		{
			1 => "Level1",
			2 => "Level2",
			3 => "Level3",
			_ => "Level1"
		};

		if (!_bgm.TryGetValue(key, out var stream))
			return;

		if (Music.Stream == stream && Music.Playing)
			return;

		Music.Stream = stream;
		Music.Play();
	}

	[Callable]
	public void StopBgm()
	{
		Music.Stop();
		Music.Stream = null;
	}

	[Callable]
	public void PlaySfx(string key, float volumeDb = 0f, float pitch = 1f)
	{
		if (!_sfx.TryGetValue(key, out var stream))
			return;

		var player = GetFreeSfxPlayer();
		player.Stream = stream;
		player.VolumeDb = volumeDb;
		player.PitchScale = pitch;
		player.Play();
	}

	private AudioStreamPlayer GetFreeSfxPlayer()
	{
		foreach (var p in _sfxPlayers)
		{
			if (!p.Playing)
				return p;
		}

		return _sfxPlayers[0];
	}
	
	public override void _Process(double delta)
{
	_scanCooldown -= delta;
	if (_scanCooldown > 0.0) return;
	_scanCooldown = 0.25;

	string levelKey = DetectLevelKey(); // "", "Level1", "Level2", "Level3"

	if (string.IsNullOrEmpty(levelKey))
	{
		if (!string.IsNullOrEmpty(_currentLevelBgmKey))
		{
			_currentLevelBgmKey = "";
			StopBgm();
		}
		return;
	}

	if (levelKey != _currentLevelBgmKey)
	{
		_currentLevelBgmKey = levelKey;

		// Map to your existing BGM dictionary keys
		if (levelKey == "Level1") PlayBgmForLevel(1); // CityLevel.wav
		else if (levelKey == "Level2") PlayBgmForLevel(2); // BunkerLevel.wav
		else if (levelKey == "Level3") PlayBgmForLevel(3); // LabLevel.wav
	}
}
private string DetectLevelKey()
{
	// Search scene tree for an instanced level scene
	// Works no matter what your entry scene is.
	foreach (var child in GetTree().Root.GetChildren())
	{
		var key = FindLevelKeyRecursive(child);
		if (!string.IsNullOrEmpty(key))
			return key;
	}
	return "";
}

private string FindLevelKeyRecursive(Node node)
{
	// SceneFilePath is set for instanced scenes (what we want)
	string path = node.SceneFilePath ?? "";

	if (path.EndsWith("Levels/Level1/Level1Streets.tscn"))
		return "Level1";
	if (path.EndsWith("Levels/Level2/Level2Alleys.tscn"))
		return "Level2";
	if (path.EndsWith("Levels/Level3/Level3NoodleShop.tscn"))
		return "Level3";

	foreach (var c in node.GetChildren())
	{
		var found = FindLevelKeyRecursive(c);
		if (!string.IsNullOrEmpty(found))
			return found;
	}

	return "";
}
}
