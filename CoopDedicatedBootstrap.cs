using System;
using System.Runtime.CompilerServices;
using Bolt;
using Steamworks;
using TheForest.Commons.Enums;
using TheForest.Utils;
using TheForest.Utils.Settings;
using UdpKit;
using UnityEngine;
using UnityEngine.UI;


public class CoopDedicatedBootstrap : MonoBehaviour
{
	
	private void Awake()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		GameSettings.Init();
		if (this.HasOption(commandLineArgs, "+connect"))
		{
			this.ClientRoutine(commandLineArgs);
		}
		else if (this.HasOption(commandLineArgs, "-dedicated"))
		{
			if (this.HasOption(commandLineArgs, "-showlogs", SteamDSConfig.ShowLogs))
			{
				ConsoleWriter.Open();
				if (CoopDedicatedBootstrap.<>f__mg$cache0 == null)
				{
					CoopDedicatedBootstrap.<>f__mg$cache0 = new Application.LogCallback(CoopAdminCommand.Application_logMessageReceived);
				}
				Application.logMessageReceived += CoopDedicatedBootstrap.<>f__mg$cache0;
				SteamDSConfig.ShowLogs = true;
			}
			this.ServerRoutine(commandLineArgs);
		}
		else
		{
			SteamDSConfig.isDedicatedServer = false;
			CoopPeerStarter.DedicatedHost = false;
			SteamClientDSConfig.isDedicatedClient = false;
			Application.LoadLevel("TitleScene");
		}
	}

	
	private void ClientRoutine(string[] args)
	{
		Debug.Log("Starting dedicated client");
		SteamDSConfig.isDedicatedServer = false;
		CoopPeerStarter.DedicatedHost = false;
		SteamClientDSConfig.isDedicatedClient = true;
		SteamClientDSConfig.serverAddress = this.GetStringArg(args, "+connect", "127.0.0.1:27015");
		SteamClientDSConfig.password = this.GetPasswordArg(args, "+password", string.Empty);
		SteamClientDSConfig.adminPassword = this.GetPasswordArg(args, "+password_admin", string.Empty);
		SteamClientDSConfig.EndPoint = UdpEndPoint.Parse(SteamClientDSConfig.serverAddress);
		GameSetup.SetInitType(this.GetEnumArg<InitTypes>(args, "-inittype", SteamClientDSConfig.gameType));
		Application.LoadLevel("SteamStartSceneDedicatedServer_Client");
	}

	
	private void ServerRoutine(string[] args)
	{
		Debug.Log("Starting dedicated server");
		SteamDSConfig.isDedicatedServer = true;
		CoopPeerStarter.DedicatedHost = true;
		SteamClientDSConfig.isDedicatedClient = false;
		SteamDSConfig.ConfigFilePath = this.GetStringArg(args, "-configfilepath", SteamDSConfig.ConfigFilePath);
		SteamDSConfig.noSteamClient = this.HasOption(args, "-nosteamclient");
		SteamDSConfig.useLaunchDisplay = this.HasOption(args, "-usedisplay");
		SteamDSConfig.ServerAddress = this.GetStringArg(args, "-serverip", SteamDSConfig.ServerAddress);
		SteamDSConfig.ServerSteamPort = this.GetShortArg(args, "-serversteamport", SteamDSConfig.ServerSteamPort);
		SteamDSConfig.ServerGamePort = this.GetShortArg(args, "-servergameport", SteamDSConfig.ServerGamePort);
		SteamDSConfig.ServerQueryPort = this.GetShortArg(args, "-serverqueryport", SteamDSConfig.ServerQueryPort);
		SteamDSConfig.ServerPlayers = this.GetIntArg(args, "-serverplayers", SteamDSConfig.ServerPlayers);
		SteamDSConfig.ServerName = this.GetStringArg(args, "-servername", SteamDSConfig.ServerName);
		SteamDSConfig.ServerPassword = this.GetPasswordArg(args, "-serverpassword", SteamDSConfig.ServerPassword);
		SteamDSConfig.ServerAdminPassword = this.GetPasswordArg(args, "-serverpassword_admin", SteamDSConfig.ServerAdminPassword);
		SteamDSConfig.ServerSteamAccount = this.GetStringArg(args, "-serversteamaccount", SteamDSConfig.ServerSteamAccount);
		SteamDSConfig.GameAutoSaveIntervalMinutes = this.GetIntArg(args, "-serverautosaveinterval", SteamDSConfig.GameAutoSaveIntervalMinutes);
		SteamDSConfig.ServerVACEnabled = this.HasOption(args, "-enableVAC", SteamDSConfig.ServerVACEnabled);
		SteamDSConfig.GameType = this.GetStringArg(args, "-inittype", SteamDSConfig.GameType);
		SteamDSConfig.GameDifficulty = this.GetStringArg(args, "-difficulty", SteamDSConfig.GameDifficulty);
		SteamDSConfig.GameSaveSlot = this.GetIntArg(args, "-slot", SteamDSConfig.GameSaveSlot);
		SteamDSConfig.VeganMode = this.HasOption(args, "-veganmode", SteamDSConfig.VeganMode);
		SteamDSConfig.VegetarianMode = this.HasOption(args, "-vegetarianmode", SteamDSConfig.VegetarianMode);
		SteamDSConfig.ResetHolesMode = this.HasOption(args, "-resetholesmode", SteamDSConfig.ResetHolesMode);
		SteamDSConfig.TreeRegrowMode = this.HasOption(args, "-treeregrowmode", SteamDSConfig.TreeRegrowMode);
		SteamDSConfig.AllowBuildingDestruction = this.HasOption(args, "-allowbuildingdestruction", SteamDSConfig.AllowBuildingDestruction);
		SteamDSConfig.AllowEnemiesCreative = this.HasOption(args, "-allowenemiescreative", SteamDSConfig.AllowEnemiesCreative);
		this.ApplyCheatsAndOptions();
		SteamDSConfig.UseServerConfigFile = false;
		if (SteamDSConfig.useLaunchDisplay)
		{
			this.showServerData();
		}
		else
		{
			this.LaunchDSServer();
		}
	}

	
	private void ApplyCheatsAndOptions()
	{
		Cheats.NoEnemies = SteamDSConfig.VeganMode;
		Cheats.NoEnemiesDuringDay = SteamDSConfig.VegetarianMode;
		Cheats.ResetHoles = SteamDSConfig.ResetHolesMode;
		PlayerPreferences.SetLocalTreeRegrowth(SteamDSConfig.TreeRegrowMode);
		PlayerPreferences.SetLocalNoDestructionMode(!SteamDSConfig.AllowBuildingDestruction);
		PlayerPreferences.SetLocalAllowEnemiesCreativeMode(SteamDSConfig.AllowEnemiesCreative);
	}

	
	public void LaunchDSServer()
	{
		if (SteamDSConfig.useLaunchDisplay)
		{
			this.setServerData();
		}
		UdpIPv4Address udpIPv4Address = UdpIPv4Address.Any;
		try
		{
			udpIPv4Address = UdpIPv4Address.Parse(SteamDSConfig.ServerAddress);
		}
		catch
		{
			udpIPv4Address = UdpIPv4Address.Any;
		}
		SteamDSConfig.EndPoint = new UdpEndPoint(udpIPv4Address, SteamDSConfig.ServerGamePort);
		GameSetup.SetPlayerMode(PlayerModes.Multiplayer);
		GameSetup.SetMpType(MpTypes.Server);
		GameSetup.SetGameType(GameTypes.Standard);
		GameSetup.SetDifficulty(this.StringToEnum<DifficultyModes>(SteamDSConfig.GameDifficulty));
		GameSetup.SetInitType(this.StringToEnum<InitTypes>(SteamDSConfig.GameType));
		GameSetup.SetSlot((Slots)SteamDSConfig.GameSaveSlot);
		SteamDSConfig.ServerAuthMode = ((!SteamDSConfig.ServerVACEnabled) ? EServerMode.eServerModeAuthentication : EServerMode.eServerModeAuthenticationAndSecure);
		Debug.LogFormat("Dedicated server info:\n IP:{0}, steamPort:{1}, gamePort:{2}, queryPort:{3}\n players:{4}, admin password:'{5}', password:'{6}', autosave interval:{7}", new object[]
		{
			udpIPv4Address,
			SteamDSConfig.ServerSteamPort,
			SteamDSConfig.ServerGamePort,
			SteamDSConfig.ServerQueryPort,
			SteamDSConfig.ServerPlayers,
			(!string.IsNullOrEmpty(SteamDSConfig.ServerAdminPassword)) ? "yes" : "no",
			(!string.IsNullOrEmpty(SteamDSConfig.ServerPassword)) ? "yes" : "no",
			SteamDSConfig.GameAutoSaveIntervalMinutes
		});
		Debug.LogFormat("Game setup: {0} {1} {2}, {3} {4} game, slot {5}", new object[]
		{
			GameSetup.Game,
			GameSetup.Mode,
			GameSetup.MpType,
			GameSetup.Init,
			GameSetup.Difficulty,
			GameSetup.Slot
		});
		PlayerPreferences.ApplyMaxFrameRate();
		Application.LoadLevel("SteamStartSceneDedicatedServer");
	}

	
	private void showServerData()
	{
		this.canvas.gameObject.SetActive(true);
		this.canvas.enabled = true;
		InputField[] componentsInChildren = base.GetComponentsInChildren<InputField>();
		componentsInChildren[0].text = SteamDSConfig.ServerAddress.ToString();
		componentsInChildren[0].contentType = InputField.ContentType.Standard;
		componentsInChildren[1].text = SteamDSConfig.ServerSteamPort.ToString();
		componentsInChildren[1].contentType = InputField.ContentType.IntegerNumber;
		componentsInChildren[2].text = SteamDSConfig.ServerGamePort.ToString();
		componentsInChildren[2].contentType = InputField.ContentType.IntegerNumber;
		componentsInChildren[3].text = SteamDSConfig.ServerQueryPort.ToString();
		componentsInChildren[3].contentType = InputField.ContentType.IntegerNumber;
		componentsInChildren[4].text = SteamDSConfig.GameDifficulty.ToString();
		componentsInChildren[4].contentType = InputField.ContentType.Standard;
		componentsInChildren[5].text = SteamDSConfig.GameType.ToString();
		componentsInChildren[5].contentType = InputField.ContentType.Standard;
		componentsInChildren[6].text = SteamDSConfig.ServerPlayers.ToString();
		componentsInChildren[6].contentType = InputField.ContentType.IntegerNumber;
		componentsInChildren[7].text = SteamDSConfig.ServerName.ToString();
		componentsInChildren[7].contentType = InputField.ContentType.Standard;
		componentsInChildren[8].text = SteamDSConfig.ServerPassword.ToString();
		componentsInChildren[8].contentType = InputField.ContentType.Standard;
		componentsInChildren[9].text = SteamDSConfig.ServerAdminPassword.ToString();
		componentsInChildren[9].contentType = InputField.ContentType.Standard;
		componentsInChildren[10].text = SteamDSConfig.ServerSteamAccount.ToString();
		componentsInChildren[10].contentType = InputField.ContentType.Standard;
		componentsInChildren[11].text = SteamDSConfig.GameAutoSaveIntervalMinutes.ToString();
		componentsInChildren[11].contentType = InputField.ContentType.IntegerNumber;
		componentsInChildren[12].text = SteamDSConfig.GameSaveSlot.ToString();
		componentsInChildren[12].contentType = InputField.ContentType.IntegerNumber;
		Toggle[] componentsInChildren2 = base.GetComponentsInChildren<Toggle>();
		componentsInChildren2[0].isOn = SteamDSConfig.ShowLogs;
		componentsInChildren2[1].isOn = SteamDSConfig.ServerVACEnabled;
	}

	
	private void setServerData()
	{
		this.canvas.gameObject.SetActive(true);
		this.canvas.enabled = true;
		InputField[] componentsInChildren = base.GetComponentsInChildren<InputField>();
		SteamDSConfig.ServerAddress = componentsInChildren[0].text;
		SteamDSConfig.ServerSteamPort = ushort.Parse(componentsInChildren[1].text);
		SteamDSConfig.ServerGamePort = ushort.Parse(componentsInChildren[2].text);
		SteamDSConfig.ServerQueryPort = ushort.Parse(componentsInChildren[3].text);
		SteamDSConfig.GameDifficulty = componentsInChildren[4].text;
		SteamDSConfig.GameType = componentsInChildren[5].text;
		SteamDSConfig.ServerPlayers = int.Parse(componentsInChildren[6].text);
		SteamDSConfig.ServerName = componentsInChildren[7].text;
		SteamDSConfig.ServerPassword = componentsInChildren[8].text;
		SteamDSConfig.ServerAdminPassword = componentsInChildren[9].text;
		SteamDSConfig.ServerSteamAccount = componentsInChildren[10].text;
		SteamDSConfig.GameAutoSaveIntervalMinutes = int.Parse(componentsInChildren[11].text);
		SteamDSConfig.GameSaveSlot = int.Parse(componentsInChildren[12].text);
		Toggle[] componentsInChildren2 = base.GetComponentsInChildren<Toggle>();
		SteamDSConfig.ShowLogs = componentsInChildren2[0].isOn;
		SteamDSConfig.ServerVACEnabled = componentsInChildren2[1].isOn;
	}

	
	private bool HasOption(string[] args, string name, bool defaultState)
	{
		return this.HasOption(args, name) || defaultState;
	}

	
	private bool HasOption(string[] args, string name)
	{
		return Array.IndexOf<string>(args, name) >= 0;
	}

	
	private string GetPasswordArg(string[] args, string name, string defaultValue)
	{
		string text = this.GetStringArg(args, name, defaultValue);
		if (!string.IsNullOrEmpty(text))
		{
			text = SteamDSConfig.PasswordToHash(text);
		}
		return text;
	}

	
	private string GetStringArg(string[] args, string name, string defaultValue)
	{
		int num = Array.IndexOf<string>(args, name);
		if (num >= 0 && num + 1 < args.Length && !string.IsNullOrEmpty(args[num + 1]))
		{
			return args[num + 1].Trim();
		}
		return defaultValue;
	}

	
	private int GetIntArg(string[] args, string name, int defaultValue)
	{
		int num = Array.IndexOf<string>(args, name);
		int result;
		if (num >= 0 && num + 1 < args.Length && !string.IsNullOrEmpty(args[num + 1]) && int.TryParse(args[num + 1].Trim(), out result))
		{
			return result;
		}
		return defaultValue;
	}

	
	private ushort GetShortArg(string[] args, string name, ushort defaultValue)
	{
		int num = Array.IndexOf<string>(args, name);
		ushort result;
		if (num >= 0 && num + 1 < args.Length && !string.IsNullOrEmpty(args[num + 1]) && ushort.TryParse(args[num + 1].Trim(), out result))
		{
			return result;
		}
		return defaultValue;
	}

	
	private T StringToEnum<T>(string value)
	{
		return (T)((object)Enum.Parse(typeof(T), value.Trim()));
	}

	
	private T GetEnumArg<T>(string[] args, string name, T defaultValue)
	{
		int num = Array.IndexOf<string>(args, name);
		if (num >= 0 && num + 1 < args.Length && !string.IsNullOrEmpty(args[num + 1]))
		{
			return (T)((object)Enum.Parse(typeof(T), args[num + 1].Trim()));
		}
		return defaultValue;
	}

	
	public bool UseServerArgs;

	
	public string[] ServerArgs = new string[]
	{
		"-dedicated",
		"-serverip",
		string.Empty,
		"-serverport",
		"27015",
		"-serverplayers",
		"16",
		"-serverpassword",
		string.Empty,
		"-serverpassword_admin",
		string.Empty,
		"-serverautosaveinterval",
		"15"
	};

	
	public bool UseClientArgs;

	
	public string[] ClientArgs = new string[]
	{
		"+connect",
		"127.0.0.1"
	};

	
	public Canvas canvas;

	
	[CompilerGenerated]
	private static Application.LogCallback <>f__mg$cache0;
}
