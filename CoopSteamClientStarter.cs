using System;
using System.Collections;
using Bolt;
using Steamworks;
using TheForest.Commons.Enums;
using TheForest.Save;
using TheForest.Utils;
using UdpKit;
using UnityEngine;


internal class CoopSteamClientStarter : CoopPeerStarter
{
	
	private IEnumerator Start()
	{
		CoopSteamClientStarter.IsAdmin = false;
		PlayerPrefs.Save();
		Debug.Log("CoopSteamServerStarter");
		yield return CoopPeerStarter.PrefabDbResource;
		this._connectionAttempts = 1;
		if (BoltNetwork.isRunning)
		{
			BoltLauncher.Shutdown();
			yield return null;
		}
		this.InitBolt();
		yield break;
	}

	
	protected new void Update()
	{
		if (this._planePosArrived && this._planeRotArrived && CoopAckChecker.ACKED && this.mapState == CoopPeerStarter.MapState.None)
		{
			base.CancelInvoke("OnDisconnected");
			CoopClientCallbacks.OnDisconnected = null;
			this.mapState = CoopPeerStarter.MapState.Begin;
		}
		base.Update();
	}

	
	public override void ConnectAttempt(UdpEndPoint endpoint, IProtocolToken token)
	{
		Debug.Log(string.Concat(new object[]
		{
			"ConnectAttempt: ",
			endpoint.Address,
			":",
			endpoint.Port
		}));
		base.CancelInvoke("OnDisconnected");
		base.Invoke("OnDisconnected", (float)(6 * this._connectionAttempts));
	}

	
	public void OnDisconnected()
	{
		base.CancelInvoke("OnDisconnected");
		CoopClientCallbacks.OnDisconnected = null;
		if (this._connectionAttempts <= 3)
		{
			Debug.Log("Client connection attempt #" + this._connectionAttempts);
			base.StartCoroutine(this.RetryConnectingRoutine());
		}
		else
		{
			CoopPlayerCallbacks.WasDisconnectedFromServer = true;
			BoltLauncher.Shutdown();
			UnityEngine.Object.Destroy(base.gameObject);
			Application.LoadLevel("TitleScene");
		}
	}

	
	private IEnumerator RetryConnectingRoutine()
	{
		if (!CoopSteamClientStarter.Retrying)
		{
			CoopSteamClientStarter.Retrying = true;
			this._connectionAttempts++;
			BoltLauncher.Shutdown();
			yield return null;
			yield return null;
			this.InitBolt();
			CoopSteamClientStarter.Retrying = false;
		}
		yield break;
	}

	
	public override void BoltStartDone()
	{
		BoltNetwork.AddGlobalEventListener(CoopAckChecker.Instance);
		if (!CoopPeerStarter.Dedicated)
		{
			CoopSteamClient.Start();
		}
		if (!PlayerSpawn.HasMPCharacterSave())
		{
			GameSetup.SetInitType(InitTypes.New);
		}
		base.BoltSetup();
		this.Connect();
		PlayerSpawn.LoadSavedCharacter = GameSetup.IsSavedGame;
	}

	
	private void InitBolt()
	{
		if (this._connectionAttempts < 3)
		{
			CoopClientCallbacks.OnDisconnected = new Action(this.OnDisconnected);
		}
		BoltConfig config = base.GetConfig();
		if (CoopPeerStarter.Dedicated)
		{
			BoltLauncher.SetUdpPlatform(new DotNetPlatform());
			BoltLauncher.StartClient(config);
		}
		else
		{
			BoltLauncher.StartClient(SteamUser.GetSteamID().ToEndPoint(), config);
		}
		CoopAckChecker.ACKED = false;
	}

	
	private void Connect()
	{
		if (CoopPeerStarter.Dedicated)
		{
			Debug.LogFormat("connecting to: {0}", new object[]
			{
				SteamClientDSConfig.serverAddress
			});
			int num = SteamUser.InitiateGameConnection(SteamClientDSConfig.steamClientBlob, 2048, SteamClientDSConfig.Server.m_steamID, SteamClientDSConfig.EndPoint.Address.Packed, SteamClientDSConfig.EndPoint.Port, SteamClientDSConfig.Server.m_bSecure);
			CoopJoinDedicatedServerToken coopJoinDedicatedServerToken = new CoopJoinDedicatedServerToken();
			coopJoinDedicatedServerToken.AdminPassword = SteamClientDSConfig.adminPassword;
			coopJoinDedicatedServerToken.ServerPassword = SteamClientDSConfig.password;
			if (num > 0)
			{
				coopJoinDedicatedServerToken.steamBlobToken = new byte[num];
				for (int i = 0; i < num; i++)
				{
					coopJoinDedicatedServerToken.steamBlobToken[i] = SteamClientDSConfig.steamClientBlob[i];
				}
			}
			BoltNetwork.Connect(SteamClientDSConfig.EndPoint, coopJoinDedicatedServerToken);
		}
		else
		{
			BoltNetwork.Connect(CoopLobby.Instance.Info.ServerId.ToEndPoint());
		}
	}

	
	public override void ConnectFailed(UdpEndPoint endpoint, IProtocolToken token)
	{
		if (this.gui)
		{
			this.gui.SetErrorText("Could not connect to server (attempt " + this._connectionAttempts + "/3)");
		}
		else
		{
			Debug.LogError("could not connect to dedicated server: " + SteamClientDSConfig.serverAddress);
		}
		this._connectionAttempts = 3;
		CoopClientCallbacks.OnDisconnected = null;
		CoopPlayerCallbacks.WasDisconnectedFromServer = true;
		BoltLauncher.Shutdown();
		SteamClientConfig.KickMessage = ((!SteamClientDSConfig.isDedicatedClient) ? "Server timeout. Could not connect to this server." : "Server does not seem to be set up correctly");
		UnityEngine.Object.Destroy(base.gameObject);
		Application.LoadLevel("TitleScene");
	}

	
	public override void EntityAttached(BoltEntity entity)
	{
		if (entity.StateIs<ICoopServerInfo>())
		{
			entity.GetState<ICoopServerInfo>().AddCallback("PlanePosition", new PropertyCallbackSimple(this.PlanePositionArrived));
			entity.GetState<ICoopServerInfo>().AddCallback("PlaneRotation", new PropertyCallbackSimple(this.PlaneRotationArrived));
		}
	}

	
	private void PlanePositionArrived()
	{
		base.CancelInvoke("OnDisconnected");
		Debug.Log("PLANE Pos");
		this._planePosArrived = true;
	}

	
	private void PlaneRotationArrived()
	{
		base.CancelInvoke("OnDisconnected");
		Debug.Log("PLANE Rot");
		this._planeRotArrived = true;
	}

	
	private bool _planePosArrived;

	
	private bool _planeRotArrived;

	
	public int _connectionAttempts;

	
	public static bool IsAdmin;

	
	public static bool Retrying;
}
