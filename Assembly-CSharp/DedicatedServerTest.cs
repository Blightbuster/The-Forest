using System;
using Steamworks;
using UnityEngine;

public class DedicatedServerTest : MonoBehaviour
{
	private void Start()
	{
		if (!Packsize.Test())
		{
			Debug.LogError("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.", this);
		}
		if (!DllCheck.Test())
		{
			Debug.LogError("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.", this);
		}
		try
		{
			if (SteamAPI.RestartAppIfNecessary(SteamDSConfig.AppIdDS))
			{
				Application.Quit();
				return;
			}
		}
		catch (DllNotFoundException arg)
		{
			Debug.LogError("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + arg, this);
			Application.Quit();
			return;
		}
		this.m_bInitialized = SteamAPI.Init();
		if (!this.m_bInitialized)
		{
			Debug.LogError("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.", this);
			return;
		}
		SteamConfig.BuildId = SteamApps.GetAppBuildId();
		string betaName;
		if (SteamApps.GetCurrentBetaName(out betaName, 50))
		{
			SteamConfig.BetaName = betaName;
		}
		Debug.Log("Steam Started");
		this.m_CallbackSteamServersConnected = Callback<SteamServersConnected_t>.CreateGameServer(new Callback<SteamServersConnected_t>.DispatchDelegate(this.OnSteamServersConnected));
		this.m_CallbackSteamServersConnectFailure = Callback<SteamServerConnectFailure_t>.CreateGameServer(new Callback<SteamServerConnectFailure_t>.DispatchDelegate(this.OnSteamServersConnectFailure));
		this.m_CallbackSteamServersDisconnected = Callback<SteamServersDisconnected_t>.CreateGameServer(new Callback<SteamServersDisconnected_t>.DispatchDelegate(this.OnSteamServersDisconnected));
		this.m_CallbackPolicyResponse = Callback<GSPolicyResponse_t>.CreateGameServer(new Callback<GSPolicyResponse_t>.DispatchDelegate(this.OnPolicyResponse));
		this.m_CallbackGSAuthTicketResponse = Callback<ValidateAuthTicketResponse_t>.CreateGameServer(new Callback<ValidateAuthTicketResponse_t>.DispatchDelegate(this.OnValidateAuthTicketResponse));
		this.m_CallbackP2PSessionRequest = Callback<P2PSessionRequest_t>.CreateGameServer(new Callback<P2PSessionRequest_t>.DispatchDelegate(this.OnP2PSessionRequest));
		this.m_CallbackP2PSessionConnectFail = Callback<P2PSessionConnectFail_t>.CreateGameServer(new Callback<P2PSessionConnectFail_t>.DispatchDelegate(this.OnP2PSessionConnectFail));
		this.m_bInitialized = false;
		this.m_bConnectedToSteam = false;
		EServerMode eServerMode = EServerMode.eServerModeAuthenticationAndSecure;
		this.m_bInitialized = GameServer.Init(0u, SteamDSConfig.ServerSteamPort, SteamDSConfig.ServerGamePort, SteamDSConfig.ServerQueryPort, eServerMode, SteamDSConfig.ServerVersion);
		if (!this.m_bInitialized)
		{
			Debug.Log("SteamGameServer_Init call failed");
			return;
		}
		SteamGameServer.SetModDir("theforestDS");
		SteamGameServer.SetProduct("The Forest");
		SteamGameServer.SetGameDescription("The Forest Game Description");
		SteamGameServer.LogOnAnonymous();
		SteamGameServer.EnableHeartbeats(true);
		Debug.Log("Started.");
	}

	private void OnDisable()
	{
		SteamGameServer.EnableHeartbeats(false);
		SteamGameServer.LogOff();
		GameServer.Shutdown();
		Debug.Log("Shutdown.");
	}

	private void Update()
	{
		if (!this.m_bInitialized)
		{
			return;
		}
		GameServer.RunCallbacks();
		if (this.m_bConnectedToSteam)
		{
			this.SendUpdatedServerDetailsToSteam();
		}
	}

	private void OnSteamServersConnected(SteamServersConnected_t pLogonSuccess)
	{
		Debug.Log("SpaceWarServer connected to Steam successfully");
		this.m_bConnectedToSteam = true;
		this.SendUpdatedServerDetailsToSteam();
	}

	private void OnSteamServersConnectFailure(SteamServerConnectFailure_t pConnectFailure)
	{
		this.m_bConnectedToSteam = false;
		Debug.Log("SpaceWarServer failed to connect to Steam");
	}

	private void OnSteamServersDisconnected(SteamServersDisconnected_t pLoggedOff)
	{
		this.m_bConnectedToSteam = false;
		Debug.Log("SpaceWarServer got logged out of Steam");
	}

	private void OnPolicyResponse(GSPolicyResponse_t pPolicyResponse)
	{
		if (SteamGameServer.BSecure())
		{
			Debug.Log("SpaceWarServer is VAC Secure!");
		}
		else
		{
			Debug.Log("SpaceWarServer is not VAC Secure!");
		}
		Debug.Log("Game server SteamID:" + SteamGameServer.GetSteamID().ToString());
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
		SteamGameServer.SetPasswordProtected(false);
		SteamGameServer.SetServerName(SteamDSConfig.ServerName);
		SteamGameServer.SetBotPlayerCount(0);
		SteamGameServer.SetMapName(SteamDSConfig.MapName);
	}

	protected Callback<SteamServersConnected_t> m_CallbackSteamServersConnected;

	protected Callback<SteamServerConnectFailure_t> m_CallbackSteamServersConnectFailure;

	protected Callback<SteamServersDisconnected_t> m_CallbackSteamServersDisconnected;

	protected Callback<GSPolicyResponse_t> m_CallbackPolicyResponse;

	protected Callback<ValidateAuthTicketResponse_t> m_CallbackGSAuthTicketResponse;

	protected Callback<P2PSessionRequest_t> m_CallbackP2PSessionRequest;

	protected Callback<P2PSessionConnectFail_t> m_CallbackP2PSessionConnectFail;

	private bool m_bInitialized;

	private bool m_bConnectedToSteam;
}
