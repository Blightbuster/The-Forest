using System;
using System.Collections;
using Steamworks;
using UnityEngine;


public class AutoJoinDedicatedServer : MonoBehaviour
{
	
	private IEnumerator Start()
	{
		if (!SteamManager.Initialized || CoopPeerStarter.DedicatedHost)
		{
			yield return null;
		}
		if (!SteamManager.Initialized || CoopPeerStarter.DedicatedHost)
		{
			yield break;
		}
		while (string.IsNullOrEmpty(SteamUser.GetSteamID().ToString()))
		{
			if (CoopPeerStarter.DedicatedHost)
			{
				yield break;
			}
			yield return null;
		}
		if (CoopPeerStarter.DedicatedHost)
		{
			yield break;
		}
		if (AutoJoinDedicatedServer.AutoStartAfterDelay)
		{
			yield return new WaitForSeconds(1f);
			Application.LoadLevel("SteamStartSceneDedicatedServer_Client");
		}
		yield break;
	}

	
	private void OnGameServerChangeRequested(GameServerChangeRequested_t param)
	{
		SteamClientDSConfig.serverAddress = param.m_rgchServer;
		SteamClientDSConfig.password = param.m_rgchPassword;
		Application.LoadLevel("SteamStartSceneDedicatedServer_Client");
	}

	
	public static bool AutoStartAfterDelay;

	
	private Callback<GameServerChangeRequested_t> GameServerChangeRequested;
}
