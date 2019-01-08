using System;
using Steamworks;

public class CoopLobbyInfo
{
	public CoopLobbyInfo(ulong steamid) : this(new CSteamID(steamid))
	{
	}

	public CoopLobbyInfo(CSteamID lobbyId)
	{
		this.LobbyId = lobbyId;
	}

	public CSteamID LobbyId { get; private set; }

	public CSteamID ServerId
	{
		get
		{
			if (this.serverId.m_SteamID != 0UL)
			{
				return this.serverId;
			}
			uint num;
			ushort num2;
			CSteamID result;
			if (SteamMatchmaking.GetLobbyGameServer(this.LobbyId, out num, out num2, out result))
			{
				this.serverId = result;
				return result;
			}
			return default(CSteamID);
		}
		set
		{
			this.serverId = value;
		}
	}

	public CSteamID OwnerSteamId
	{
		get
		{
			if (this.ownerId.m_SteamID == 0UL)
			{
				this.ownerId = SteamMatchmaking.GetLobbyOwner(this.LobbyId);
				return this.ownerId;
			}
			return this.ownerId;
		}
	}

	public void RequestData()
	{
		SteamMatchmaking.RequestLobbyData(this.LobbyId);
	}

	public void UpdateData()
	{
		this.Name = SteamMatchmaking.GetLobbyData(this.LobbyId, "name");
		this.Guid = SteamMatchmaking.GetLobbyData(this.LobbyId, "guid");
		if (!int.TryParse(SteamMatchmaking.GetLobbyData(this.LobbyId, "currentmembers"), out this.CurrentMembers))
		{
			this.CurrentMembers = 0;
		}
		this.Destroyed = (SteamMatchmaking.GetLobbyData(this.LobbyId, "destroyed") == "YES");
		this.MemberLimit = SteamMatchmaking.GetLobbyMemberLimit(this.LobbyId);
		this.PrefabDbVersion = SteamMatchmaking.GetLobbyData(this.LobbyId, "prefabdbversion");
	}

	public string Name;

	public string Guid;

	public string PrefabDbVersion;

	public int CurrentMembers;

	public bool Joinable;

	public bool Destroyed;

	public int MemberLimit;

	public bool IsOwner;

	private CSteamID serverId = default(CSteamID);

	private CSteamID ownerId = default(CSteamID);
}
