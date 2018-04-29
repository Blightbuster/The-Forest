using System;
using System.Collections;
using UnityEngine;


public class CoopDedicatedClientStarter : MonoBehaviour
{
	
	private void Awake()
	{
		UnityEngine.Object.DontDestroyOnLoad(this.loadAsync.gameObject);
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	
	private IEnumerator Start()
	{
		CoopPeerStarter.Dedicated = true;
		CoopPeerStarter.DedicatedHost = false;
		SteamClientDSConfig.IsClientAtWorld = false;
		yield return null;
		this.Connect();
		yield break;
	}

	
	private void Connect()
	{
		this.clientStarter = base.gameObject.AddComponent<CoopSteamClientStarter>();
		this.clientStarter._async = this.loadAsync;
	}

	
	private CoopSteamClientStarter clientStarter;

	
	public LoadAsync loadAsync;
}
