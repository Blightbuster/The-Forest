using System;
using Steamworks;
using TheForest.Commons.Enums;
using UdpKit;


public static class SteamClientDSConfig
{
	
	
	
	public static bool IsClientAtWorld
	{
		get
		{
			return SteamClientDSConfig.isClientAtWorld;
		}
		set
		{
			SteamClientDSConfig.isClientAtWorld = value;
			FMOD_StudioSystem.FmodDSClientOff = !SteamClientDSConfig.isClientAtWorld;
		}
	}

	
	public static void Clear()
	{
		SteamClientDSConfig.isDedicatedClient = false;
		SteamClientDSConfig.isDSFirstClient = false;
		SteamClientDSConfig.password = string.Empty;
		SteamClientDSConfig.adminPassword = string.Empty;
		SteamClientDSConfig.serverAddress = "127.0.0.1:27015";
		SteamClientDSConfig.Guid = string.Empty;
		SteamClientDSConfig.Server = null;
		SteamClientDSConfig.playerCount = 0;
		SteamClientDSConfig.isClientAtWorld = false;
	}

	
	public const int steamBlobSize = 2048;

	
	public static bool isDedicatedClient = false;

	
	public static bool isDSFirstClient = false;

	
	public static string password = string.Empty;

	
	public static string adminPassword = string.Empty;

	
	public static string serverAddress = "127.0.0.1:27015";

	
	public static InitTypes gameType = InitTypes.New;

	
	public static UdpEndPoint EndPoint;

	
	public static string Guid;

	
	public static gameserveritem_t Server;

	
	public static int playerCount = 0;

	
	public static byte[] steamClientBlob = new byte[2048];

	
	private static bool isClientAtWorld = false;
}
