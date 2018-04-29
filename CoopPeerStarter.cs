using System;
using System.Collections;
using Bolt;
using Steamworks;
using TheForest.Commons.Enums;
using TheForest.Save;
using TheForest.Utils;
using UnityEngine;


public class CoopPeerStarter : GlobalEventListener
{
	
	
	public static ResourceRequest PrefabDbResource
	{
		get
		{
			if (CoopPeerStarter._PrefabDbResource == null)
			{
				CoopPeerStarter._PrefabDbResource = Resources.LoadAsync<PrefabDatabase>("BoltPrefabDatabase");
			}
			return CoopPeerStarter._PrefabDbResource;
		}
	}

	
	private void Awake()
	{
		if (this.gui)
		{
			UnityEngine.Object.DontDestroyOnLoad(this.gui);
		}
		if (CoopPeerStarter.PrefabDbResource == null)
		{
			Debug.LogError("Issue occured when loading the Bolt Prefab DB");
		}
	}

	
	protected BoltConfig GetConfig()
	{
		BoltConfig configCopy = BoltRuntimeSettings.instance.GetConfigCopy();
		configCopy.connectionTimeout = 500000;
		configCopy.connectionRequestAttempts = ((!CoopPeerStarter.Dedicated) ? 60 : 15);
		configCopy.connectionRequestTimeout = 1000;
		return configCopy;
	}

	
	protected void BoltSetup()
	{
		BoltNetwork.SetCanReceiveEntities(false);
		BoltNetwork.RegisterTokenClass<CoopConnectToken>();
		BoltNetwork.RegisterTokenClass<CoopConstructionExToken>();
		BoltNetwork.RegisterTokenClass<CoopFloorToken>();
		BoltNetwork.RegisterTokenClass<CoopRoofToken>();
		BoltNetwork.RegisterTokenClass<CoopWallChunkToken>();
		BoltNetwork.RegisterTokenClass<CoopBridgeToken>();
		BoltNetwork.RegisterTokenClass<CoopZiplineToken>();
		BoltNetwork.RegisterTokenClass<CoopZiplineTreeToken>();
		BoltNetwork.RegisterTokenClass<CoopCraneToken>();
		BoltNetwork.RegisterTokenClass<CoopGardenToken>();
		BoltNetwork.RegisterTokenClass<CoopFoundationChunkTierToken>();
		BoltNetwork.RegisterTokenClass<CoopSingleAnchorToken>();
		BoltNetwork.RegisterTokenClass<CoopMutantDummyToken>();
		BoltNetwork.RegisterTokenClass<CoopDestroyTagToken>();
		BoltNetwork.RegisterTokenClass<CoopKickToken>();
		BoltNetwork.RegisterTokenClass<CoopCreateBreakToken>();
		BoltNetwork.RegisterTokenClass<CoopJoinDedicatedServerToken>();
		BoltNetwork.RegisterTokenClass<CoopJoinDedicatedServerFailed>();
		BoltNetwork.RegisterTokenClass<CoopWeaponUpgradesToken>();
		BoltNetwork.RegisterTokenClass<CoopSyncGirlPickupToken>();
	}

	
	protected virtual void OnLoadingDone()
	{
	}

	
	private IEnumerator LoadingDone()
	{
		this.OnLoadingDone();
		yield return null;
		if (GameSetup.Init == InitTypes.Continue)
		{
			while (LevelSerializer.IsDeserializing || PlayerSpawn.LoadSavedCharacter)
			{
				yield return null;
			}
		}
		yield return null;
		string name = "Unknown";
		if (!CoopPeerStarter.DedicatedHost)
		{
			name = SteamFriends.GetPersonaName();
			if (name == null || name.Trim().Length == 0)
			{
				yield return null;
				name = SteamFriends.GetPersonaName();
				if (name == null || name.Trim().Length == 0)
				{
					name = "Unknown";
				}
			}
			while (!LocalPlayer.GameObject)
			{
				yield return null;
			}
			Debug.Log("Attach local player");
			CoopUtils.AttachLocalPlayer(name);
		}
		try
		{
			BoltNetwork.UpdateSceneObjectsLookup();
		}
		catch (Exception)
		{
		}
		BoltNetwork.SetCanReceiveEntities(true);
		UnityEngine.Object.Destroy(base.gameObject);
		if (this.gui)
		{
			UnityEngine.Object.Destroy(this.gui);
		}
		yield break;
	}

	
	private LoadAsync GetAsync()
	{
		if (this._async)
		{
			return this._async;
		}
		if (this.gui && this.gui._async)
		{
			return this.gui._async;
		}
		return null;
	}

	
	protected void Update()
	{
		switch (this.mapState)
		{
		case CoopPeerStarter.MapState.Begin:
		{
			CoopPlayerCallbacks.ClearTrees();
			LoadAsync async = this.GetAsync();
			if (async)
			{
				async.gameObject.SetActive(true);
				this.mapState = CoopPeerStarter.MapState.Loading;
			}
			break;
		}
		case CoopPeerStarter.MapState.Loading:
			if (!this.GetAsync() || this.GetAsync().isDone)
			{
				this.mapState = CoopPeerStarter.MapState.Done;
			}
			break;
		case CoopPeerStarter.MapState.Done:
		{
			try
			{
				BoltNetwork.UpdateSceneObjectsLookup();
			}
			catch (Exception)
			{
			}
			Camera componentInChildren = base.GetComponentInChildren<Camera>();
			if (componentInChildren)
			{
				componentInChildren.enabled = false;
			}
			base.StartCoroutine(this.LoadingDone());
			this.mapState = CoopPeerStarter.MapState.Playing;
			break;
		}
		}
	}

	
	public static bool Dedicated;

	
	public static bool DedicatedHost;

	
	[HideInInspector]
	public CoopPeerStarter.MapState mapState;

	
	[HideInInspector]
	public CoopSteamNGUI gui;

	
	[HideInInspector]
	public LoadAsync _async;

	
	private static ResourceRequest _PrefabDbResource;

	
	public enum MapState
	{
		
		None,
		
		Begin,
		
		Loading,
		
		Done,
		
		Playing
	}
}
