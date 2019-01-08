using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TheForest.UI;
using TheForest.Utils;
using UdpKit;
using UniLinq;
using UnityEngine;

public class CoopKick
{
	public CoopKick()
	{
		this.LoadList();
	}

	public static CoopKick Instance
	{
		get
		{
			if (CoopKick.instance == null)
			{
				CoopKick.instance = new CoopKick();
			}
			return CoopKick.instance;
		}
	}

	private void LoadList()
	{
		byte[] bytes = PlayerPrefsFile.GetBytes("BanList", null, false);
		if (bytes == null || bytes.Length == 0)
		{
			this.kickedSteamIds = new List<CoopKick.KickedPlayer>();
			return;
		}
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			using (MemoryStream memoryStream = new MemoryStream(bytes))
			{
				this.kickedSteamIds = (List<CoopKick.KickedPlayer>)binaryFormatter.Deserialize(memoryStream);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			this.kickedSteamIds = new List<CoopKick.KickedPlayer>();
		}
	}

	private static void SaveList()
	{
		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			using (MemoryStream memoryStream = new MemoryStream())
			{
				binaryFormatter.Serialize(memoryStream, CoopKick.Instance.kickedSteamIds);
				PlayerPrefsFile.SetBytes("BanList", memoryStream.ToArray(), false);
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public List<CoopKick.KickedPlayer> KickedPlayers
	{
		get
		{
			return this.kickedSteamIds;
		}
	}

	public static void KickPlayer(ulong steamid, int duration = -1, string message = "HOST_KICKED_YOU")
	{
		BoltConnection connection;
		BoltEntity playerEntity;
		MpPlayerList.GetConnectionAndEntity(steamid, out connection, out playerEntity);
		CoopKick.KickPlayer(connection, playerEntity, duration, message);
	}

	public static void KickPlayerByConnectionId(int connectionId, int duration = -1, string message = "HOST_KICKED_YOU")
	{
		BoltConnection connectionFromConnectionId = MpPlayerList.GetConnectionFromConnectionId(connectionId);
		CoopKick.KickPlayer(connectionFromConnectionId, null, duration, message);
	}

	public static void KickPlayer(BoltEntity playerEntity, int duration, string message = "HOST_KICKED_YOU")
	{
		CoopKick.KickPlayer(null, playerEntity, duration, message);
	}

	public static void KickPlayer(BoltConnection connection, BoltEntity playerEntity, int duration, string message = "HOST_KICKED_YOU")
	{
		if (!BoltNetwork.isServer)
		{
			return;
		}
		if (connection == null && playerEntity == null)
		{
			return;
		}
		if (connection == null)
		{
			connection = playerEntity.source;
		}
		try
		{
			ulong steamId = CoopKick.GetSteamId(connection);
			if (steamId == 0UL)
			{
				steamId = CoopKick.GetSteamId(playerEntity);
			}
			string name = (!(playerEntity == null)) ? playerEntity.GetState<IPlayerState>().name : steamId.ToString();
			if (duration >= 0 && steamId > 0UL && !CoopKick.IsBanned(steamId))
			{
				CoopKick.Instance.kickedSteamIds.Add(new CoopKick.KickedPlayer
				{
					Name = name,
					SteamId = steamId,
					BanEndTime = ((duration <= 0) ? 0L : (DateTime.UtcNow.ToUnixTimestamp() + (long)duration))
				});
				CoopKick.SaveList();
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		connection.Disconnect(new CoopKickToken
		{
			KickMessage = message,
			Banned = (duration == 0)
		});
	}

	private static ulong GetSteamId(BoltConnection connection)
	{
		if (connection == null)
		{
			return 0UL;
		}
		if (!SteamDSConfig.isDedicatedServer)
		{
			return connection.RemoteEndPoint.SteamId.Id;
		}
		if (SteamDSConfig.clientConnectionInfo.ContainsKey(connection.ConnectionId))
		{
			return SteamDSConfig.clientConnectionInfo[connection.ConnectionId].m_SteamID;
		}
		return 0UL;
	}

	private static ulong GetSteamId(BoltEntity playerEntity)
	{
		if (playerEntity == null)
		{
			return 0UL;
		}
		return CoopKick.GetSteamId(playerEntity.source);
	}

	public static void BanPlayer(BoltEntity playerEntity)
	{
		CoopKick.KickPlayer(playerEntity, 0, "HOST_BANNED_YOU_PERMANANTLY");
	}

	public static void BanPlayer(BoltConnection connection, BoltEntity playerEntity)
	{
		CoopKick.KickPlayer(connection, playerEntity, 0, "HOST_BANNED_YOU_PERMANANTLY");
	}

	public static void UnBanPlayer(ulong steamId)
	{
		CoopKick.KickedPlayer kickedPlayer = CoopKick.Instance.kickedSteamIds.FirstOrDefault((CoopKick.KickedPlayer k) => k.SteamId == steamId);
		if (kickedPlayer != null)
		{
			CoopKick.Instance.kickedSteamIds.Remove(kickedPlayer);
			CoopKick.SaveList();
		}
	}

	public static bool UnBanPlayer(string name)
	{
		CoopKick.KickedPlayer kickedPlayer = CoopKick.Instance.kickedSteamIds.FirstOrDefault((CoopKick.KickedPlayer k) => k.Name == name);
		if (kickedPlayer != null)
		{
			CoopKick.Instance.kickedSteamIds.Remove(kickedPlayer);
			CoopKick.SaveList();
			return true;
		}
		return false;
	}

	public static void ClearKickList()
	{
		CoopKick.Instance.kickedSteamIds.Clear();
	}

	public static bool IsBanned(UdpSteamID steamid)
	{
		CoopKick.CheckBanEndTimes();
		return CoopKick.Instance.kickedSteamIds.Any((CoopKick.KickedPlayer k) => k.SteamId == steamid.Id);
	}

	public static bool IsBanned(ulong steamid)
	{
		CoopKick.CheckBanEndTimes();
		return CoopKick.Instance.kickedSteamIds.Any((CoopKick.KickedPlayer k) => k.SteamId == steamid);
	}

	public static void CheckBanEndTimes()
	{
		bool flag = false;
		long num = DateTime.UtcNow.ToUnixTimestamp();
		for (int i = CoopKick.Instance.kickedSteamIds.Count - 1; i >= 0; i--)
		{
			long banEndTime = CoopKick.Instance.kickedSteamIds[i].BanEndTime;
			if (banEndTime > 0L && banEndTime < num)
			{
				CoopKick.Instance.kickedSteamIds.RemoveAt(i);
				flag = true;
			}
		}
		if (flag)
		{
			CoopKick.SaveList();
		}
	}

	private const string BanlistKey = "BanList";

	private List<CoopKick.KickedPlayer> kickedSteamIds = new List<CoopKick.KickedPlayer>();

	private static CoopKick instance;

	[Serializable]
	public class KickedPlayer
	{
		public string Name;

		public ulong SteamId;

		public long BanEndTime;
	}
}
