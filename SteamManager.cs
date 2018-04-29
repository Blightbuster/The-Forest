using System;
using System.Text;
using Steamworks;
using UnityEngine;


public class SteamManager : MonoBehaviour
{
	
	
	private static SteamManager Instance
	{
		get
		{
			return SteamManager.s_instance ?? new GameObject("SteamManager").AddComponent<SteamManager>();
		}
	}

	
	
	public static bool Initialized
	{
		get
		{
			return !SteamDSConfig.isDedicatedServer && SteamManager.Instance.initialized;
		}
	}

	
	
	public static int BuildId
	{
		get
		{
			return SteamConfig.BuildId;
		}
	}

	
	
	public static string BetaName
	{
		get
		{
			return SteamConfig.BetaName;
		}
	}

	
	public static bool Reset()
	{
		if (!SteamDSConfig.isDedicatedServer)
		{
			if (SteamManager.s_instance)
			{
				UnityEngine.Object.DestroyImmediate(SteamManager.s_instance);
				SteamManager.s_instance = null;
			}
			return SteamManager.Initialized;
		}
		return SteamManager.Initialized;
	}

	
	private static void SteamAPIDebugTextHook(int nSeverity, StringBuilder pchDebugText)
	{
		Debug.LogWarning(pchDebugText);
	}

	
	private void Awake()
	{
		if (!SteamDSConfig.isDedicatedServer && SteamManager.s_instance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		if (!SteamDSConfig.isDedicatedServer)
		{
			SteamManager.s_instance = this;
		}
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		if (!Packsize.Test())
		{
			Debug.LogError("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.", this);
		}
		if (!DllCheck.Test())
		{
			Debug.LogError("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.", this);
		}
		if (!SteamDSConfig.noSteamClient)
		{
			try
			{
				if (SteamAPI.RestartAppIfNecessary((!SteamDSConfig.isDedicatedServer && !CoopPeerStarter.DedicatedHost) ? SteamConfig.AppId : SteamDSConfig.AppIdDS))
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
		}
		this.initialized = (SteamAPI.Init() || SteamDSConfig.noSteamClient);
		if (!this.initialized)
		{
			Debug.LogError("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.", this);
			return;
		}
		if (!SteamDSConfig.noSteamClient)
		{
			SteamConfig.BuildId = SteamApps.GetAppBuildId();
			string betaName;
			if (SteamApps.GetCurrentBetaName(out betaName, 50))
			{
				SteamConfig.BetaName = betaName;
			}
		}
		if (SteamDSConfig.noSteamClient)
		{
			Debug.Log("Steam Started without steam client.");
		}
		else
		{
			Debug.Log("Steam Started");
		}
		if (!SteamDSConfig.isDedicatedServer)
		{
			CoopSteamManager.Initialize();
		}
	}

	
	private void OnEnable()
	{
		if (!SteamDSConfig.isDedicatedServer && SteamManager.s_instance == null)
		{
			SteamManager.s_instance = this;
		}
		if (!this.initialized)
		{
			return;
		}
		if (!SteamDSConfig.noSteamClient && this.m_SteamAPIWarningMessageHook == null)
		{
			this.m_SteamAPIWarningMessageHook = new SteamAPIWarningMessageHook_t(SteamManager.SteamAPIDebugTextHook);
			SteamClient.SetWarningMessageHook(this.m_SteamAPIWarningMessageHook);
		}
	}

	
	private void OnDestroy()
	{
		if (!SteamDSConfig.isDedicatedServer && SteamManager.s_instance != this)
		{
			return;
		}
		if (SteamDSConfig.isDedicatedServer)
		{
			Debug.Log("SteamManager - Someone call OnDestroy");
		}
		if (!this.initialized)
		{
			return;
		}
		if (SteamDSConfig.isDedicatedServer)
		{
			SteamDSConfig.manager.Shutdown();
		}
		CoopSteamServer.Shutdown();
		CoopSteamClient.Shutdown();
		CoopSteamManager.Shutdown();
		CoopLobbyManager.Shutdown();
	}

	
	private void OnApplicationQuit()
	{
		SteamAPI.Shutdown();
	}

	
	private void Update()
	{
		if (!this.initialized)
		{
			return;
		}
		if (SteamDSConfig.isDedicatedServer && !SteamDSConfig.initialized)
		{
			return;
		}
		if (SteamDSConfig.isDedicatedServer)
		{
			SteamDSConfig.manager.SetUpdate();
		}
		SteamAPI.RunCallbacks();
		CoopSteamServer.Update();
		CoopSteamClient.Update();
	}

	
	private static SteamManager s_instance;

	
	public bool initialized;

	
	private SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;
}
