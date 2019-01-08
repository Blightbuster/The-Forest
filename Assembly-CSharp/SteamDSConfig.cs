using System;
using System.Collections.Generic;
using System.IO;
using Bolt;
using Steamworks;
using TheForest.Commons.Enums;
using TheForest.Tools;
using TheForest.Utils;
using UdpKit;
using UnityEngine;

public static class SteamDSConfig
{
	public static bool IsServerSaving
	{
		get
		{
			return SteamDSConfig.currentServerStatus == 1;
		}
	}

	public static bool isDedicated
	{
		get
		{
			return SteamDSConfig.isDedicatedServer || SteamClientDSConfig.isDedicatedClient;
		}
	}

	public static void StartAutoSaveMode()
	{
		SteamDSConfig.SetServerStatus(0);
		GameObject gameObject = new GameObject("GameAutoSave");
		AutoSaveMode autoSaveMode = gameObject.AddComponent<AutoSaveMode>();
	}

	public static void SaveGame()
	{
		GameSetup.SetSlot((Slots)SteamDSConfig.GameSaveSlot);
		LevelSerializer.Checkpoint();
		SaveSlotUtils.SaveHostGameGUID();
	}

	public static void SetServerStatus(int status)
	{
		SteamDSConfig.currentServerStatus = status;
		ServerStatusInfo serverStatusInfo = ServerStatusInfo.Create(GlobalTargets.AllClients);
		serverStatusInfo.Status = SteamDSConfig.currentServerStatus;
		serverStatusInfo.Send();
	}

	public static void ReceiveServerStatus(int status)
	{
		SteamDSConfig.currentServerStatus = status;
	}

	private static ushort defaultShort(string key, ushort defaultValue)
	{
		if (SteamDSConfig.UseServerConfigFile)
		{
			string serverData = SteamDSConfig.GetServerData(key);
			ushort result = defaultValue;
			if (serverData != string.Empty && ushort.TryParse(serverData, out result))
			{
				return result;
			}
		}
		return defaultValue;
	}

	private static int defaultInt(string key, int defaultValue, int minValue, int maxValue)
	{
		return Mathf.Clamp(SteamDSConfig.defaultInt(key, defaultValue), minValue, maxValue);
	}

	private static int defaultInt(string key, int defaultValue, int minValue)
	{
		return Mathf.Max(SteamDSConfig.defaultInt(key, defaultValue), minValue);
	}

	private static int defaultInt(string key, int defaultValue)
	{
		if (SteamDSConfig.UseServerConfigFile)
		{
			string serverData = SteamDSConfig.GetServerData(key);
			int result = defaultValue;
			if (serverData != string.Empty && int.TryParse(serverData, out result))
			{
				return result;
			}
		}
		return defaultValue;
	}

	private static bool defaultBool(string key, bool defaultValue)
	{
		if (SteamDSConfig.UseServerConfigFile)
		{
			string serverData = SteamDSConfig.GetServerData(key);
			if (serverData != string.Empty)
			{
				return serverData.ToLower() == "on" || serverData.ToLower() == "true";
			}
		}
		return defaultValue;
	}

	private static string defaultString(string key, string defaultValue = "")
	{
		if (SteamDSConfig.UseServerConfigFile)
		{
			string serverData = SteamDSConfig.GetServerData(key);
			return (!(serverData != string.Empty)) ? defaultValue : serverData;
		}
		return defaultValue;
	}

	public static string PasswordToHash(string password)
	{
		return Hash.ToMD5(Hash.ToSHA512(password));
	}

	public static string ServerAddress
	{
		get
		{
			return SteamDSConfig.defaultString("serverIP", SteamDSConfig._serverAddress);
		}
		set
		{
			SteamDSConfig._serverAddress = value;
		}
	}

	public static ushort ServerSteamPort
	{
		get
		{
			return SteamDSConfig.defaultShort("serverSteamPort", SteamDSConfig._serverSteamPort);
		}
		set
		{
			SteamDSConfig._serverSteamPort = value;
		}
	}

	public static ushort ServerGamePort
	{
		get
		{
			return SteamDSConfig.defaultShort("serverGamePort", SteamDSConfig._serverGamePort);
		}
		set
		{
			SteamDSConfig._serverGamePort = value;
		}
	}

	public static ushort ServerQueryPort
	{
		get
		{
			return SteamDSConfig.defaultShort("serverQueryPort", SteamDSConfig._serverQueryPort);
		}
		set
		{
			SteamDSConfig._serverQueryPort = value;
		}
	}

	public static string ServerName
	{
		get
		{
			return SteamDSConfig.defaultString("serverName", SteamDSConfig._serverName);
		}
		set
		{
			SteamDSConfig._serverName = value;
		}
	}

	public static bool ServerVACEnabled
	{
		get
		{
			return SteamDSConfig.defaultBool("enableVAC", SteamDSConfig._enableVAC);
		}
		set
		{
			SteamDSConfig._enableVAC = value;
		}
	}

	public static string ServerPassword
	{
		get
		{
			return SteamDSConfig.defaultString("serverPassword", SteamDSConfig._serverPassword);
		}
		set
		{
			SteamDSConfig._serverPassword = value;
		}
	}

	public static string ServerAdminPassword
	{
		get
		{
			return SteamDSConfig.defaultString("serverPasswordAdmin", SteamDSConfig._serverAdminPassword);
		}
		set
		{
			SteamDSConfig._serverAdminPassword = value;
		}
	}

	public static string ServerSteamAccount
	{
		get
		{
			return SteamDSConfig.defaultString("serverSteamAccount", SteamDSConfig._serverSteamAccount);
		}
		set
		{
			SteamDSConfig._serverSteamAccount = value;
		}
	}

	public static int ServerPlayers
	{
		get
		{
			return SteamDSConfig.defaultInt("serverPlayers", SteamDSConfig._serverPlayers, SteamDSConfig.minPlayersPerServer, SteamDSConfig.maxPlayersPerServer);
		}
		set
		{
			SteamDSConfig._serverPlayers = Mathf.Clamp(value, SteamDSConfig.minPlayersPerServer, SteamDSConfig.maxPlayersPerServer);
		}
	}

	public static int GameAutoSaveIntervalMinutes
	{
		get
		{
			return SteamDSConfig.defaultInt("serverAutoSaveInterval", SteamDSConfig._gameAutoSaveIntervalMinutes, SteamDSConfig.minAutoSaveTime);
		}
		set
		{
			SteamDSConfig._gameAutoSaveIntervalMinutes = Math.Max(value, SteamDSConfig.minAutoSaveTime);
		}
	}

	public static string GameDifficulty
	{
		get
		{
			return SteamDSConfig.defaultString("difficulty", SteamDSConfig._gameDifficulty);
		}
		set
		{
			SteamDSConfig._gameDifficulty = value;
		}
	}

	public static string GameType
	{
		get
		{
			return SteamDSConfig.defaultString("initType", SteamDSConfig._gameType);
		}
		set
		{
			SteamDSConfig._gameType = value;
		}
	}

	public static int GameSaveSlot
	{
		get
		{
			return SteamDSConfig.defaultInt("slot", SteamDSConfig._gameSaveSlot);
		}
		set
		{
			SteamDSConfig._gameSaveSlot = value;
		}
	}

	public static bool ShowLogs
	{
		get
		{
			return SteamDSConfig.defaultBool("showLogs", SteamDSConfig._showLogs);
		}
		set
		{
			SteamDSConfig._showLogs = value;
		}
	}

	public static string SaveFolderPath
	{
		get
		{
			return SteamDSConfig.defaultString("saveFolderPath", SteamDSConfig._saveFolderPath);
		}
		set
		{
			SteamDSConfig._saveFolderPath = value;
		}
	}

	public static int TargetFpsIdle
	{
		get
		{
			return SteamDSConfig.defaultInt("targetFpsIdle", SteamDSConfig._targetFpsIdle);
		}
		set
		{
			SteamDSConfig._targetFpsIdle = value;
		}
	}

	public static int TargetFpsActive
	{
		get
		{
			return SteamDSConfig.defaultInt("targetFpsActive", SteamDSConfig._targetFpsActive);
		}
		set
		{
			SteamDSConfig._targetFpsActive = value;
		}
	}

	public static bool VeganMode
	{
		get
		{
			return SteamDSConfig.defaultBool("veganMode", SteamDSConfig._veganMode);
		}
		set
		{
			SteamDSConfig._veganMode = value;
		}
	}

	public static bool VegetarianMode
	{
		get
		{
			return SteamDSConfig.defaultBool("vegetarianMode", SteamDSConfig._vegetarianMode);
		}
		set
		{
			SteamDSConfig._vegetarianMode = value;
		}
	}

	public static bool ResetHolesMode
	{
		get
		{
			return SteamDSConfig.defaultBool("resetHolesMode", SteamDSConfig._resetHolesMode);
		}
		set
		{
			SteamDSConfig._resetHolesMode = value;
		}
	}

	public static bool TreeRegrowMode
	{
		get
		{
			return SteamDSConfig.defaultBool("treeRegrowMode", SteamDSConfig._treeRegrowMode);
		}
		set
		{
			SteamDSConfig._treeRegrowMode = value;
		}
	}

	public static bool AllowBuildingDestruction
	{
		get
		{
			return SteamDSConfig.defaultBool("allowBuildingDestruction", SteamDSConfig._allowbuildingdestruction);
		}
		set
		{
			SteamDSConfig._allowbuildingdestruction = value;
		}
	}

	public static bool AllowEnemiesCreative
	{
		get
		{
			return SteamDSConfig.defaultBool("allowEnemiesCreativeMode", SteamDSConfig._allowEnemiesCreativeMode);
		}
		set
		{
			SteamDSConfig._allowEnemiesCreativeMode = value;
		}
	}

	public static bool AllowCheats
	{
		get
		{
			return SteamDSConfig.defaultBool("allowCheats", SteamDSConfig._allowcheats);
		}
		set
		{
			SteamDSConfig._allowcheats = value;
		}
	}

	public static AppId_t AppIdDS
	{
		get
		{
			return new AppId_t(556450u);
		}
	}

	public static string GetServerData(string key)
	{
		SteamDSConfig.LoadServerCfg();
		if (SteamDSConfig.serverConfig != null && SteamDSConfig.serverConfig.Count > 0)
		{
			string key2 = key.ToLower();
			if (SteamDSConfig.serverConfig.ContainsKey(key2))
			{
				return SteamDSConfig.serverConfig[key2];
			}
		}
		Debug.LogError("No ServerConfig key (" + key + ") found. Check your configuration file.");
		return string.Empty;
	}

	public static void LoadServerCfg()
	{
		if (!SteamDSConfig.ServerConfigLoaded)
		{
			string[] serverCfg = SteamDSConfig.GetServerCfg();
			SteamDSConfig.serverConfig = new Dictionary<string, string>();
			for (int i = 0; i < serverCfg.Length; i++)
			{
				string text = serverCfg[i].Trim();
				if (!text.StartsWith("//"))
				{
					int num = text.IndexOf(' ');
					if (num == -1)
					{
						SteamDSConfig.serverConfig.Add(text.ToLower(), string.Empty);
						if (text.EndsWith("s"))
						{
							SteamDSConfig.serverConfig.Add(text, string.Empty);
						}
					}
					else
					{
						string text2 = text.Substring(0, num).ToLower();
						string text3 = text.Substring(num + 1, text.Length - (num + 1));
						SteamDSConfig.serverConfig.Add(text2, text3.Trim());
						if (text2.EndsWith("s"))
						{
							SteamDSConfig.serverConfig.Add(text2.TrimEnd(new char[]
							{
								's'
							}), text3.Trim());
						}
					}
				}
			}
			SteamDSConfig.ServerConfigLoaded = true;
		}
	}

	public static string[] GetServerCfg()
	{
		try
		{
			if (File.Exists(SteamDSConfig.GetServerCfgPath()))
			{
				return File.ReadAllLines(SteamDSConfig.GetServerCfgPath());
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		return SteamDSConfig.CreateServerCfg();
	}

	public static string GetServerCfgPath()
	{
		return SteamDSConfig.ConfigFilePath;
	}

	public static string GetServerCfgDir()
	{
		return Path.GetDirectoryName(SteamDSConfig.ConfigFilePath);
	}

	public static string[] CreateServerCfg()
	{
		try
		{
			if (!Directory.Exists(SteamDSConfig.GetServerCfgDir()))
			{
				Directory.CreateDirectory(SteamDSConfig.GetServerCfgDir());
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		try
		{
			if (File.Exists(SteamDSConfig.GetServerCfgPath()))
			{
				File.Delete(SteamDSConfig.GetServerCfgPath());
			}
		}
		catch (Exception exception2)
		{
			Debug.LogException(exception2);
		}
		try
		{
			File.WriteAllLines(SteamDSConfig.GetServerCfgPath(), SteamDSConfig.ServerDefaultData);
		}
		catch (Exception exception3)
		{
			Debug.LogException(exception3);
		}
		return SteamDSConfig.ServerDefaultData;
	}

	public static bool isDedicatedServer = false;

	public static CoopSteamManagerDS manager;

	public static string ProductName = "The Forest";

	public static string ProductDescription = "The Forest";

	public static string MapName = "The Forest";

	public static string ServerVersion = "0.11.3.0.0";

	public static bool initialized = false;

	public static bool connectedToSteam = false;

	public static UdpPlatform udpPlatform;

	public static UdpEndPoint EndPoint;

	public static string ServerGUID = string.Empty;

	public static bool isUsingDummyPlayer = false;

	public static Dictionary<uint, CSteamID> clientConnectionInfo = new Dictionary<uint, CSteamID>();

	public static int currentServerStatus = -1;

	public const int ServerStarting = -1;

	public const int ServerRunning = 0;

	public const int ServerSaving = 1;

	public static EServerMode ServerAuthMode = EServerMode.eServerModeAuthentication;

	public static bool useLaunchDisplay = false;

	public static bool noSteamClient = false;

	private static string _serverAddress = "192.168.1.47";

	private static ushort _serverSteamPort = 27016;

	private static ushort _serverGamePort = 27015;

	private static ushort _serverQueryPort = 27016;

	private static string _serverName = "The Forest Server";

	private static bool _enableVAC = false;

	private static string _serverPassword = string.Empty;

	private static string _serverAdminPassword = string.Empty;

	private static string _serverSteamAccount = string.Empty;

	private static int _serverPlayers = 8;

	private static int _gameAutoSaveIntervalMinutes = 30;

	private static string _gameDifficulty = "Normal";

	private static string _gameType = "Continue";

	private static int _gameSaveSlot = 1;

	private static bool _showLogs = false;

	private static string _saveFolderPath = Application.persistentDataPath + "/ds/";

	private static int _targetFpsIdle = 5;

	private static int _targetFpsActive = 60;

	public static int minAutoSaveTime = 15;

	public static int minPlayersPerServer = 2;

	public static int maxPlayersPerServer = 100;

	private static bool _veganMode = false;

	private static bool _vegetarianMode = false;

	private static bool _resetHolesMode = false;

	private static bool _treeRegrowMode = false;

	private static bool _allowbuildingdestruction = true;

	private static bool _allowEnemiesCreativeMode = false;

	private static bool _allowcheats = false;

	public static string serverConfigurationDir = "ds";

	public static string serverConfigurationFile = "Server.cfg";

	public static string ConfigFilePath = string.Concat(new string[]
	{
		Application.persistentDataPath,
		"/",
		SteamDSConfig.serverConfigurationDir,
		"/",
		SteamDSConfig.serverConfigurationFile
	});

	public static bool ServerConfigLoaded = false;

	public static bool UseServerConfigFile = true;

	public static Dictionary<string, string> serverConfig;

	public static string[] ServerDefaultData = new string[]
	{
		"// Dedicated Server Settings.",
		"// Server IP address - Note: If you have a router, this address is the internal address, and you need to configure ports forwarding",
		"serverIP 192.168.1.47",
		"// Steam Communication Port - Note: If you have a router you will need to open this port.",
		"serverSteamPort 8766",
		"// Game Communication Port - Note: If you have a router you will need to open this port.",
		"serverGamePort 27015",
		"// Query Communication Port - Note: If you have a router you will need to open this port.",
		"serverQueryPort 27016",
		"// Server display name",
		"serverName The Forest Game",
		"// Maximum number of players",
		"serverPlayers 4",
		"// Enable VAC (Valve Anti-cheat System at the server. Must be set off or on",
		"enableVAC off",
		"// Server password. blank means no password",
		"serverPassword ",
		"// Server administration password. blank means no password",
		"serverPasswordAdmin ",
		"// Your Steam account name. blank means anonymous",
		"serverSteamAccount ",
		"// Time between server auto saves in minutes - The minumum time is 15 minutes, the default time is 30",
		"serverAutoSaveInterval 30",
		"// Game difficulty mode. Must be set to Peaceful Normal or Hard",
		"difficulty Normal",
		"// New or continue a game. Must be set to New or Continue",
		"initType Continue",
		"// Slot to save the game. Must be set 1 2 3 4 or 5",
		"slot 1",
		"// Show event log. Must be set off or on",
		"showLogs off",
		"// Contact email for server admin",
		"serverContact email@gmail.com",
		"// No enemies",
		"veganMode off",
		"// No enemies during day time",
		"vegetarianMode off",
		"// Reset all structure holes when loading a save",
		"resetHolesMode off",
		"// Regrow 10% of cut down trees when sleeping",
		"treeRegrowMode off",
		"// Allow building destruction",
		"allowBuildingDestruction on",
		"// Allow enemies in creative games",
		"allowEnemiesCreativeMode off",
		"// Allow clients to use the built in debug console",
		"allowCheats off",
		"// Allows defining a custom folder for save slots, leave empty to use the default location",
		"saveFolderPath",
		"// Target FPS when no client is connected",
		"targetFpsIdle 5",
		"// Target FPS when there is at least one client connected",
		"targetFpsActive 60"
	};
}
