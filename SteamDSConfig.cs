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
		if (SteamDSConfig.serverConfig.Length > 0)
		{
			foreach (string text in SteamDSConfig.serverConfig)
			{
				if (!text.Contains("
				{
					string text2 = text.Replace(key, string.Empty);
					return text2.Trim();
				}
			}
		}
		Debug.LogError("No ServerConfig key (" + key + ") found. Check your configuration file.");
		return string.Empty;
	}

	
	public static void LoadServerCfg()
	{
		if (!SteamDSConfig.ServerConfigLoaded)
		{
			SteamDSConfig.serverConfig = SteamDSConfig.GetServerCfg();
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
		return Application.persistentDataPath + "/" + SteamDSConfig.serverConfigurationDir + "/";
	}

	
	public static string[] CreateServerCfg()
	{
		try
		{
			if (!Directory.Exists(SteamDSConfig.GetServerCfgDir()))
			{
				Directory.CreateDirectory(SteamDSConfig.GetServerCfgDir());
			}
			if (File.Exists(SteamDSConfig.GetServerCfgPath()))
			{
				File.Delete(SteamDSConfig.GetServerCfgPath());
			}
			File.WriteAllLines(SteamDSConfig.GetServerCfgPath(), SteamDSConfig.ServerDefaultData);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
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

	
	public static int minAutoSaveTime = 15;

	
	public static int minPlayersPerServer = 2;

	
	public static int maxPlayersPerServer = 100;

	
	private static bool _veganMode = false;

	
	private static bool _vegetarianMode = false;

	
	private static bool _resetHolesMode = false;

	
	private static bool _treeRegrowMode = false;

	
	private static bool _allowbuildingdestruction = true;

	
	private static bool _allowEnemiesCreativeMode = false;

	
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

	
	public static string[] serverConfig;

	
	public static string[] ServerDefaultData = new string[]
	{
		"
		"
		"serverIP 192.168.1.47",
		"
		"serverSteamPort 8766",
		"
		"serverGamePort 27015",
		"
		"serverQueryPort 27016",
		"
		"serverName The Forest Game",
		"
		"serverPlayers 4",
		"
		"enableVAC off",
		"
		"serverPassword ",
		"
		"serverPasswordAdmin ",
		"
		"serverSteamAccount ",
		"
		"serverAutoSaveInterval 30",
		"
		"difficulty Normal",
		"
		"initType Continue",
		"
		"slot 1",
		"
		"showLogs off",
		"
		"serverContact email@gmail.com",
		"
		"veganMode off",
		"
		"vegetarianMode off",
		"
		"resetHolesMode off",
		"
		"treeRegrowMode off",
		"
		"allowBuildingDestruction on",
		"
		"allowEnemiesCreativeMode off"
	};
}
