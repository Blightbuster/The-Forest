using System;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public class CoopLobby
{
	private CoopLobby()
	{
	}

	public static CoopLobby Instance { get; private set; }

	public static bool IsInLobby
	{
		get
		{
			return CoopLobby.Instance != null && CoopLobby.Instance.Info.LobbyId.IsValid();
		}
	}

	public CoopLobbyInfo Info { get; private set; }

	public int MemberCount
	{
		get
		{
			return SteamMatchmaking.GetNumLobbyMembers(this.Info.LobbyId);
		}
	}

	public IEnumerable<CSteamID> AllMembers
	{
		get
		{
			for (int i = 0; i < this.MemberCount; i++)
			{
				yield return SteamMatchmaking.GetLobbyMemberByIndex(this.Info.LobbyId, i);
			}
			yield break;
		}
	}

	public IEnumerable<string> GetMembersData(string key)
	{
		for (int i = 0; i < this.MemberCount; i++)
		{
			CSteamID j = SteamMatchmaking.GetLobbyMemberByIndex(this.Info.LobbyId, i);
			yield return SteamMatchmaking.GetLobbyMemberData(this.Info.LobbyId, j, key);
		}
		yield break;
	}

	public void Destroy()
	{
		this.Info.Destroyed = true;
		SteamMatchmaking.SetLobbyJoinable(this.Info.LobbyId, false);
		if (CoopPeerStarter.Dedicated)
		{
			SteamGameServer.SetKeyValue("destroyed", "YES");
		}
		else
		{
			SteamMatchmaking.SetLobbyData(this.Info.LobbyId, "destroyed", "YES");
		}
	}

	public void SetName(string name)
	{
		this.Info.Name = name;
		if (CoopPeerStarter.Dedicated)
		{
			SteamGameServer.SetKeyValue("name", name);
		}
		else
		{
			SteamMatchmaking.SetLobbyData(this.Info.LobbyId, "name", name);
		}
	}

	public void SetGuid(string guid)
	{
		CoopLobby.HostGuid = guid;
		this.Info.Guid = guid;
		if (CoopPeerStarter.Dedicated)
		{
			SteamGameServer.SetKeyValue("guid", guid);
		}
		else
		{
			SteamMatchmaking.SetLobbyData(this.Info.LobbyId, "guid", guid);
		}
	}

	public void SetCurrentMembers(int value)
	{
		if (this.Info.CurrentMembers != value)
		{
			this.Info.CurrentMembers = value;
			SteamMatchmaking.SetLobbyData(this.Info.LobbyId, "currentmembers", this.Info.CurrentMembers.ToString());
		}
	}

	public void SetPrefabDbVersion(string value)
	{
		if (this.Info.PrefabDbVersion != value)
		{
			this.Info.PrefabDbVersion = value;
			SteamMatchmaking.SetLobbyData(this.Info.LobbyId, "prefabdbversion", this.Info.PrefabDbVersion);
		}
	}

	public void SetServer(CSteamID server)
	{
		this.Info.ServerId = server;
		SteamMatchmaking.SetLobbyGameServer(this.Info.LobbyId, 0u, 0, server);
	}

	public void SetMemberLimit(int limit)
	{
		this.Info.MemberLimit = limit;
		SteamMatchmaking.SetLobbyMemberLimit(this.Info.LobbyId, limit);
	}

	public void SetJoinable(bool joinable)
	{
		this.Info.Joinable = joinable;
		SteamMatchmaking.SetLobbyJoinable(this.Info.LobbyId, joinable);
	}

	public static void LeaveActive()
	{
		Debug.Log("CoopLobby.LeaveActive instance=" + CoopLobby.Instance);
		if (CoopLobby.Instance != null)
		{
			try
			{
				if (CoopLobby.Instance.Info.LobbyId.IsValid())
				{
					SteamMatchmaking.LeaveLobby(CoopLobby.Instance.Info.LobbyId);
				}
			}
			finally
			{
				CoopLobby.Instance = null;
			}
		}
	}

	public static void SetActive(CoopLobbyInfo info)
	{
		if (CoopLobby.IsInLobby && CoopLobby.Instance.Info.LobbyId == info.LobbyId)
		{
			return;
		}
		CoopLobby.LeaveActive();
		CoopLobby.Instance = new CoopLobby();
		CoopLobby.Instance.Info = info;
		if (info.IsOwner)
		{
			CoopLobby.Instance.SetName(CoopLobby.Instance.Info.Name);
			CoopLobby.Instance.SetMemberLimit(CoopLobby.Instance.Info.MemberLimit);
			CoopLobby.Instance.SetJoinable(true);
		}
	}

	public static string HostGuid;
}
