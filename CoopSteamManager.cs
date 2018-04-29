using System;
using System.Runtime.CompilerServices;
using Steamworks;
using TheForest.Networking;
using UnityEngine;


public static class CoopSteamManager
{
	
	public static void Initialize()
	{
		if (CoopSteamManager.runInit)
		{
			Debug.Log("CoopSteamManager Initialize");
			CoopSteamManager.runInit = false;
			if (CoopSteamManager.<>f__mg$cache0 == null)
			{
				CoopSteamManager.<>f__mg$cache0 = new Callback<GameServerChangeRequested_t>.DispatchDelegate(CoopSteamManager.JoinServerRequest);
			}
			CoopSteamManager.GameServerChangeRequested_Callback = Callback<GameServerChangeRequested_t>.Create(CoopSteamManager.<>f__mg$cache0);
			if (CoopSteamManager.<>f__mg$cache1 == null)
			{
				CoopSteamManager.<>f__mg$cache1 = new Callback<LobbyInvite_t>.DispatchDelegate(CoopSteamManager.LobbyInvite);
			}
			CoopSteamManager.LobbyInvite_Callback = Callback<LobbyInvite_t>.Create(CoopSteamManager.<>f__mg$cache1);
			if (CoopSteamManager.<>f__mg$cache2 == null)
			{
				CoopSteamManager.<>f__mg$cache2 = new Callback<GameLobbyJoinRequested_t>.DispatchDelegate(CoopSteamManager.LobbyJoin);
			}
			CoopSteamManager.LobbyJoin_Callback = Callback<GameLobbyJoinRequested_t>.Create(CoopSteamManager.<>f__mg$cache2);
			if (CoopSteamManager.<>f__mg$cache3 == null)
			{
				CoopSteamManager.<>f__mg$cache3 = new Callback<P2PSessionRequest_t>.DispatchDelegate(CoopSteamManager.P2PSessionRequest);
			}
			CoopSteamManager.P2PSessionRequest_Callback = Callback<P2PSessionRequest_t>.Create(CoopSteamManager.<>f__mg$cache3);
			if (CoopSteamManager.<>f__mg$cache4 == null)
			{
				CoopSteamManager.<>f__mg$cache4 = new Callback<P2PSessionConnectFail_t>.DispatchDelegate(CoopSteamManager.P2PSessionConnectFail);
			}
			CoopSteamManager.P2PSessionConnectFail_Callback = Callback<P2PSessionConnectFail_t>.Create(CoopSteamManager.<>f__mg$cache4);
		}
	}

	
	private static void LobbyJoin(GameLobbyJoinRequested_t param)
	{
		if (BoltNetwork.isRunning)
		{
			return;
		}
		AutoJoinAfterMPInvite autoJoinAfterMPInvite = UnityEngine.Object.FindObjectOfType<AutoJoinAfterMPInvite>();
		CoopSteamNGUI coopSteamNGUI = UnityEngine.Object.FindObjectOfType<CoopSteamNGUI>();
		if (coopSteamNGUI)
		{
			if (CoopLobby.IsInLobby)
			{
				return;
			}
			coopSteamNGUI.SetJoinText(param.m_steamIDLobby.m_SteamID);
		}
		else if (autoJoinAfterMPInvite)
		{
			autoJoinAfterMPInvite.SetInvitedToGameId(param.m_steamIDLobby.m_SteamID);
		}
	}

	
	private static void JoinServerRequest(GameServerChangeRequested_t param)
	{
		if (BoltNetwork.isRunning || SteamClientDSConfig.Server != null)
		{
			return;
		}
		AutoJoinAfterMPInvite autoJoinAfterMPInvite = UnityEngine.Object.FindObjectOfType<AutoJoinAfterMPInvite>();
		CoopSteamNGUI exists = UnityEngine.Object.FindObjectOfType<CoopSteamNGUI>();
		SteamClientDSConfig.serverAddress = param.m_rgchServer;
		SteamClientDSConfig.password = SteamDSConfig.PasswordToHash(param.m_rgchPassword);
		if (exists)
		{
			if (CoopLobby.IsInLobby)
			{
				return;
			}
			Debug.Log("todo ?");
		}
		else if (autoJoinAfterMPInvite)
		{
			autoJoinAfterMPInvite.invitedToPassword = param.m_rgchPassword;
			autoJoinAfterMPInvite.SetInvitedToServer(param.m_rgchServer);
		}
	}

	
	private static void LobbyInvite(LobbyInvite_t param)
	{
		if (BoltNetwork.isRunning)
		{
			return;
		}
		AutoJoinAfterMPInvite autoJoinAfterMPInvite = UnityEngine.Object.FindObjectOfType<AutoJoinAfterMPInvite>();
		CoopSteamNGUI coopSteamNGUI = UnityEngine.Object.FindObjectOfType<CoopSteamNGUI>();
		if (coopSteamNGUI)
		{
			if (CoopLobby.IsInLobby)
			{
				return;
			}
			coopSteamNGUI.SetJoinText(param.m_ulSteamIDLobby);
		}
		else if (autoJoinAfterMPInvite)
		{
			autoJoinAfterMPInvite.SetInvitedToGameId(param.m_ulSteamIDLobby);
		}
	}

	
	public static void Shutdown()
	{
		CoopSteamManager.P2PSessionRequest_Callback = null;
		CoopSteamManager.P2PSessionConnectFail_Callback = null;
		CoopSteamManager.runInit = true;
	}

	
	public static void Dump(string tag, P2PSessionState_t s)
	{
		Debug.Log("##### " + tag + " #####");
		Debug.Log("m_bConnecting: " + s.m_bConnecting);
		Debug.Log("m_bConnectionActive: " + s.m_bConnectionActive);
		Debug.Log("m_bUsingRelay: " + s.m_bUsingRelay);
		Debug.Log("m_eP2PSessionError: " + s.m_eP2PSessionError);
		Debug.Log("m_nBytesQueuedForSend: " + s.m_nBytesQueuedForSend);
		Debug.Log("m_nPacketsQueuedForSend: " + s.m_nPacketsQueuedForSend);
		Debug.Log("m_nRemoteIP: " + s.m_nRemoteIP);
		Debug.Log("m_nRemotePort: " + s.m_nRemotePort);
	}

	
	private static void P2PSessionConnectFail(P2PSessionConnectFail_t param)
	{
		Debug.LogError(string.Concat(new object[]
		{
			"P2PSessionConnectFail: error=",
			param.m_eP2PSessionError,
			", remoteId=",
			param.m_steamIDRemote
		}));
		if (CoopLobby.Instance != null && CoopLobby.Instance.Info != null)
		{
			Debug.LogError("P2PSessionConnectFail: ServerId=" + CoopLobby.Instance.Info.ServerId);
			P2PSessionState_t s;
			if (SteamNetworking.GetP2PSessionState(CoopLobby.Instance.Info.ServerId, out s))
			{
				CoopSteamManager.Dump("Server", s);
			}
			Debug.LogError("P2PSessionConnectFail: OwnerSteamId=" + CoopLobby.Instance.Info.OwnerSteamId);
			if (SteamNetworking.GetP2PSessionState(CoopLobby.Instance.Info.OwnerSteamId, out s))
			{
				CoopSteamManager.Dump("Lobby Owner", s);
			}
		}
		else
		{
			Debug.LogError("P2PSessionConnectFail dump error: " + ((CoopLobby.Instance != null) ? "'CoopLobby.Instance.Info' is null" : "'CoopLobby.Instance' is null"));
		}
	}

	
	private static void P2PSessionRequest(P2PSessionRequest_t param)
	{
		Debug.Log("CoopSteamManager.P2PSessionRequest (client): remoteId=" + param.m_steamIDRemote);
		SteamNetworking.AcceptP2PSessionWithUser(param.m_steamIDRemote);
	}

	
	private static bool runInit = true;

	
	private static Callback<GameServerChangeRequested_t> GameServerChangeRequested_Callback;

	
	private static Callback<LobbyInvite_t> LobbyInvite_Callback;

	
	private static Callback<GameLobbyJoinRequested_t> LobbyJoin_Callback;

	
	private static Callback<P2PSessionRequest_t> P2PSessionRequest_Callback;

	
	private static Callback<P2PSessionConnectFail_t> P2PSessionConnectFail_Callback;

	
	[CompilerGenerated]
	private static Callback<GameServerChangeRequested_t>.DispatchDelegate <>f__mg$cache0;

	
	[CompilerGenerated]
	private static Callback<LobbyInvite_t>.DispatchDelegate <>f__mg$cache1;

	
	[CompilerGenerated]
	private static Callback<GameLobbyJoinRequested_t>.DispatchDelegate <>f__mg$cache2;

	
	[CompilerGenerated]
	private static Callback<P2PSessionRequest_t>.DispatchDelegate <>f__mg$cache3;

	
	[CompilerGenerated]
	private static Callback<P2PSessionConnectFail_t>.DispatchDelegate <>f__mg$cache4;
}
