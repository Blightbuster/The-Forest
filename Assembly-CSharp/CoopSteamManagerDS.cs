using System;
using System.Collections.Generic;
using Steamworks;
using TheForest.Commons.Enums;
using TheForest.Utils;
using UnityEngine;

public class CoopSteamManagerDS : MonoBehaviour
{
	private void Awake()
	{
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		SteamDSConfig.udpPlatform = new DotNetPlatform();
		SteamDSConfig.manager = this;
		SteamDSConfig.connectedToSteam = false;
	}

	public void SetStart(LoadAsync asyncObj)
	{
		this.m_CallbackSteamServersConnected = Callback<SteamServersConnected_t>.CreateGameServer(new Callback<SteamServersConnected_t>.DispatchDelegate(this.OnSteamServersConnected));
		this.m_CallbackSteamServersConnectFailure = Callback<SteamServerConnectFailure_t>.CreateGameServer(new Callback<SteamServerConnectFailure_t>.DispatchDelegate(this.OnSteamServersConnectFailure));
		this.m_CallbackSteamServersDisconnected = Callback<SteamServersDisconnected_t>.CreateGameServer(new Callback<SteamServersDisconnected_t>.DispatchDelegate(this.OnSteamServersDisconnected));
		this.m_CallbackPolicyResponse = Callback<GSPolicyResponse_t>.CreateGameServer(new Callback<GSPolicyResponse_t>.DispatchDelegate(this.OnPolicyResponse));
		this.m_CallbackGSAuthTicketResponse = Callback<ValidateAuthTicketResponse_t>.CreateGameServer(new Callback<ValidateAuthTicketResponse_t>.DispatchDelegate(this.OnValidateAuthTicketResponse));
		this.m_CallbackP2PSessionRequest = Callback<P2PSessionRequest_t>.CreateGameServer(new Callback<P2PSessionRequest_t>.DispatchDelegate(this.OnP2PSessionRequest));
		this.m_CallbackP2PSessionConnectFail = Callback<P2PSessionConnectFail_t>.CreateGameServer(new Callback<P2PSessionConnectFail_t>.DispatchDelegate(this.OnP2PSessionConnectFail));
		this.m_CallbackGSClientApprove = Callback<GSClientApprove_t>.CreateGameServer(new Callback<GSClientApprove_t>.DispatchDelegate(this.OnSteamClientApproved));
		this.m_CallbackGSClientDeny = Callback<GSClientDeny_t>.CreateGameServer(new Callback<GSClientDeny_t>.DispatchDelegate(this.OnSteamClientDeny));
		this.m_CallbackGSClientKick = Callback<GSClientKick_t>.CreateGameServer(new Callback<GSClientKick_t>.DispatchDelegate(this.OnSteamClientKick));
		this.loadAsync = asyncObj;
		Debug.Log("Steam Manager Started.");
	}

	public void Shutdown()
	{
		SteamGameServer.EnableHeartbeats(false);
		SteamGameServer.LogOff();
		GameServer.Shutdown();
		Debug.Log("Shutdown.");
	}

	public void SetUpdate()
	{
		if (!SteamDSConfig.initialized)
		{
			return;
		}
		GameServer.RunCallbacks();
		if (SteamDSConfig.connectedToSteam)
		{
			this.SendUpdatedServerDetailsToSteam();
		}
		SteamGameServer.EnableHeartbeats(true);
	}

	private void Launch()
	{
		CoopSteamManager.Initialize();
		CoopPeerStarter.Dedicated = true;
		CoopPeerStarter.DedicatedHost = true;
		CoopLobby.SetActive(new CoopLobbyInfo(SteamGameServer.GetSteamID())
		{
			Name = SteamDSConfig.ServerName,
			Joinable = true,
			MemberLimit = SteamDSConfig.ServerPlayers
		});
		if (GameSetup.IsSavedGame)
		{
			SaveSlotUtils.LoadHostGameGUID();
		}
		if (string.IsNullOrEmpty(CoopLobby.Instance.Info.Guid) || GameSetup.IsNewGame)
		{
			CoopLobby.Instance.SetGuid(Guid.NewGuid().ToString());
		}
		SteamDSConfig.ServerGUID = CoopLobby.Instance.Info.Guid;
		GameSetup.SetMpType(MpTypes.Server);
		GameSetup.SetPlayerMode(PlayerModes.Multiplayer);
		FMOD_StudioSystem.ForceFmodOff = true;
		GameObject gameObject = new GameObject("CoopSteamServerStarter");
		CoopSteamServerStarter coopSteamServerStarter = gameObject.AddComponent<CoopSteamServerStarter>();
		coopSteamServerStarter.mapState = CoopPeerStarter.MapState.None;
		coopSteamServerStarter._async = this.loadAsync;
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
	}

	private void OnSteamClientApproved(GSClientApprove_t token)
	{
		Debug.Log("ClientApproved " + token.m_SteamID);
	}

	private void OnSteamClientDeny(GSClientDeny_t token)
	{
		Debug.Log("ClientDeny " + token.m_SteamID);
		if (SteamDSConfig.clientConnectionInfo.ContainsValue(token.m_SteamID))
		{
			foreach (KeyValuePair<uint, CSteamID> keyValuePair in SteamDSConfig.clientConnectionInfo)
			{
				if (token.m_SteamID.m_SteamID == keyValuePair.Value.m_SteamID)
				{
					foreach (BoltConnection boltConnection in BoltNetwork.connections)
					{
						if (boltConnection.ConnectionId == keyValuePair.Key)
						{
							boltConnection.Disconnect(new CoopJoinDedicatedServerFailed
							{
								Error = "Steam Client Deny"
							});
						}
					}
				}
			}
		}
	}

	private void OnSteamClientKick(GSClientKick_t token)
	{
		Debug.Log("OnSteamClientKick " + token.m_SteamID);
		if (SteamDSConfig.clientConnectionInfo.ContainsValue(token.m_SteamID))
		{
			foreach (KeyValuePair<uint, CSteamID> keyValuePair in SteamDSConfig.clientConnectionInfo)
			{
				if (token.m_SteamID.m_SteamID == keyValuePair.Value.m_SteamID)
				{
					foreach (BoltConnection boltConnection in BoltNetwork.connections)
					{
						if (boltConnection.ConnectionId == keyValuePair.Key)
						{
							boltConnection.Disconnect(new CoopJoinDedicatedServerFailed
							{
								Error = "Steam Client Kick"
							});
						}
					}
				}
			}
		}
	}

	private void OnSteamServersConnected(SteamServersConnected_t pLogonSuccess)
	{
		Debug.Log("Connected to Steam successfully");
		SteamDSConfig.connectedToSteam = true;
		this.SendUpdatedServerDetailsToSteam();
	}

	private void OnSteamServersConnectFailure(SteamServerConnectFailure_t pConnectFailure)
	{
		SteamDSConfig.connectedToSteam = false;
		Debug.Log("Failed to connect to Steam");
	}

	private void OnSteamServersDisconnected(SteamServersDisconnected_t pLoggedOff)
	{
		SteamDSConfig.connectedToSteam = false;
		Debug.Log("Got logged out of Steam");
	}

	private void OnPolicyResponse(GSPolicyResponse_t pPolicyResponse)
	{
		if (SteamGameServer.BSecure())
		{
			Debug.Log("The Forest Server is VAC Secure!");
		}
		else
		{
			Debug.Log("The Forest Server is not VAC Secure!");
		}
		Debug.Log("Game server SteamID:" + SteamGameServer.GetSteamID().ToString());
		this.Launch();
	}

	private void OnValidateAuthTicketResponse(ValidateAuthTicketResponse_t pResponse)
	{
		Debug.Log("OnValidateAuthTicketResponse Called steamID: " + pResponse.m_SteamID);
		if (pResponse.m_eAuthSessionResponse == EAuthSessionResponse.k_EAuthSessionResponseOK)
		{
		}
	}

	private void OnP2PSessionRequest(P2PSessionRequest_t pCallback)
	{
		Debug.Log("OnP2PSesssionRequest Called steamIDRemote: " + pCallback.m_steamIDRemote);
		SteamGameServerNetworking.AcceptP2PSessionWithUser(pCallback.m_steamIDRemote);
	}

	private void OnP2PSessionConnectFail(P2PSessionConnectFail_t pCallback)
	{
		Debug.Log("OnP2PSessionConnectFail Called steamIDRemote: " + pCallback.m_steamIDRemote);
	}

	private void SendUpdatedServerDetailsToSteam()
	{
		SteamGameServer.SetMaxPlayerCount(SteamDSConfig.ServerPlayers);
		SteamGameServer.SetPasswordProtected(!string.IsNullOrEmpty(SteamDSConfig.ServerPassword));
		SteamGameServer.SetServerName(SteamDSConfig.ServerName);
		SteamGameServer.SetMapName(SteamDSConfig.MapName);
		SteamGameServer.SetGameTags(SteamDSConfig.ServerGUID + ";__E3C26D06F07B6AB14EC25F4823E9A30D6B4ED0450527C1E768739D96C9F061AE");
		SteamGameServer.ForceHeartbeat();
	}

	protected Callback<SteamServersConnected_t> m_CallbackSteamServersConnected;

	protected Callback<SteamServerConnectFailure_t> m_CallbackSteamServersConnectFailure;

	protected Callback<SteamServersDisconnected_t> m_CallbackSteamServersDisconnected;

	protected Callback<GSPolicyResponse_t> m_CallbackPolicyResponse;

	protected Callback<ValidateAuthTicketResponse_t> m_CallbackGSAuthTicketResponse;

	protected Callback<P2PSessionRequest_t> m_CallbackP2PSessionRequest;

	protected Callback<P2PSessionConnectFail_t> m_CallbackP2PSessionConnectFail;

	protected Callback<GSClientApprove_t> m_CallbackGSClientApprove;

	protected Callback<GSClientDeny_t> m_CallbackGSClientDeny;

	protected Callback<GSClientKick_t> m_CallbackGSClientKick;

	private bool m_bInitialized;

	private bool m_bConnectedToSteam;

	private LoadAsync loadAsync;

	private float time = 1f;

	private float timer = 1f;
}
