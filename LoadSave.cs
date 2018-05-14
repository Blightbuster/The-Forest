using System;
using System.Collections;
using System.Collections.Generic;
using Bolt;
using PathologicalGames;
using TheForest.Buildings.Utils;
using TheForest.Items.Inventory;
using TheForest.Modding;
using TheForest.UI;
using TheForest.Utils;
using TheForest.World;
using UnityEngine;


public class LoadSave : MonoBehaviour
{
	
	
	
	public static event Action OnGameStart;

	
	private void Awake()
	{
		PerfTimerLogger perfTimerLogger = new PerfTimerLogger("[<color=#FFF>TIMER</color>] LoadSave Awake", PerfTimerLogger.LogResultType.Milliseconds, null);
		GeoHash.ClearAll();
		Scene.FinishGameLoad = false;
		UnityEngine.Random.seed = Convert.ToInt32(DateTime.UtcNow.ToUnixTimestamp());
		LevelSerializer.Progress += this.LevelSerializer_Progress;
		if (Scene.PlaneCrash && Scene.PlaneCrash.gameObject && Scene.PlaneCrash.gameObject.activeSelf && Scene.PlaneCrash.ShowCrash)
		{
			Scene.HudGui.Loading._cam.SetActive(!CoopPeerStarter.DedicatedHost);
		}
		if (LevelLoader.Current)
		{
			Scene.HudGui.Loading._cam.SetActive(!CoopPeerStarter.DedicatedHost);
			Scene.HudGui.Loading._message.SetActive(!CoopPeerStarter.DedicatedHost);
			perfTimerLogger.Stop();
			return;
		}
		if (LoadSave.ShouldLoad)
		{
			LoadSave.ShouldLoad = false;
			if (LevelSerializer.CanResume)
			{
				Scene.HudGui.Loading._cam.SetActive(!CoopPeerStarter.DedicatedHost);
				LevelSerializer.Collect();
				LevelSerializer.Resume();
				perfTimerLogger.Stop();
				return;
			}
		}
		if (Scene.HudGui.Loading._message)
		{
			Scene.HudGui.Loading._message.SetActive(false);
		}
		Time.timeScale = 1f;
		MainMenuAudio.FadeOut();
		Debug.Log("****** Game Activation Sequence ******");
		base.StartCoroutine(this.Activation(true));
		perfTimerLogger.Stop();
	}

	
	private void Start()
	{
		if (!LevelSerializer.IsDeserializing && !this.startedSequence)
		{
			base.StartCoroutine(this.Activation(true));
		}
	}

	
	private void OnDestroy()
	{
		LevelSerializer.Progress -= this.LevelSerializer_Progress;
		LoadSave.OnGameStart = null;
	}

	
	private void LevelSerializer_Progress(string section, float alpha)
	{
		LoadingProgress.Progress = 0.5f + alpha * 0.1f;
		if (Mathf.Approximately(alpha, 1f) && section.Equals("Done"))
		{
			Time.timeScale = 0.1f;
			MainMenuAudio.FadeOut();
			base.StartCoroutine(this.Activation(true));
		}
	}

	
	public IEnumerator Activation(bool activate)
	{
		if (ForestVR.Prototype)
		{
			base.enabled = false;
			yield break;
		}
		if (this.startedSequence)
		{
			yield break;
		}
		PerfTimerLogger timer = new PerfTimerLogger("[<color=#FFF>TIMER</color>] Activation", PerfTimerLogger.LogResultType.Milliseconds, null);
		this.startedSequence = true;
		if (!CoopPeerStarter.DedicatedHost)
		{
			while (!LocalPlayer.Rigidbody)
			{
				yield return null;
			}
			LocalPlayer.Rigidbody.useGravity = false;
			LocalPlayer.Rigidbody.isKinematic = true;
		}
		this.SetStartSequenceStep(0, 1f);
		Debug.Log("Game Activation Sequence step 0 (GameStartType=" + GameSetup.Init + ")");
		bool isLoadingSave = GameSetup.IsSavedGame;
		if (isLoadingSave && BoltNetwork.isRunning && BoltNetwork.isServer && Scene.PlaneCrash)
		{
			yield return null;
			if (Scene.PlaneCrash.savePos)
			{
				CoopServerInfo.Instance.state.PlanePosition = Scene.PlaneCrash.savePos.position;
				CoopServerInfo.Instance.state.PlaneRotation = Scene.PlaneCrash.savePos.rotation;
				foreach (BoltConnection connection in BoltNetwork.clients)
				{
					SetJoiningTimeOfDay setJoiningTimeOfDay = SetJoiningTimeOfDay.Create(connection);
					setJoiningTimeOfDay.TimeOfDay = ((!Scene.Atmosphere) ? 302f : Scene.Atmosphere.TimeOfDay);
					setJoiningTimeOfDay.Send();
				}
			}
			else
			{
				Debug.LogError("No plane crash position");
			}
		}
		else if (BoltNetwork.isServer && ((!isLoadingSave && !Scene.PlaneCrash.gameObject.activeSelf) || (isLoadingSave && !Scene.PlaneCrash.savePos)))
		{
			Debug.LogError("Setting fake plane coordinates for testing purposes. Please turn on plane crash when testing MP.");
			CoopServerInfo.Instance.state.PlanePosition = ((!LocalPlayer.Transform) ? new Vector3(400f, 50f, 300f) : LocalPlayer.Transform.position) + new Vector3(0f, 4f, 0f);
			CoopServerInfo.Instance.state.PlaneRotation = Quaternion.Euler(0f, 20f, 0f);
		}
		yield return null;
		if (TheForest.Utils.Input.player != null)
		{
			TheForest.Utils.Input.SetMenuMapping(false);
		}
		this.SetStartSequenceStep(1, 1f);
		if (AstarPath.active)
		{
			Debug.Log("Game Activation Sequence step 1 : enable Astar");
			if (BoltNetwork.isClient)
			{
				GameObject gameObject = GameObject.Find("Astar");
				GameObject gameObject2 = GameObject.Find("AstarGUOGo");
				if (gameObject2)
				{
					UnityEngine.Object.Destroy(gameObject2);
				}
				if (gameObject)
				{
					UnityEngine.Object.Destroy(gameObject);
				}
			}
			else
			{
				AstarPath.active.enabled = true;
			}
		}
		else
		{
			Debug.Log("Game Activation Sequence step 1 : AStar is missing, SKIPPING !!!");
		}
		yield return null;
		int list = 1;
		foreach (LoadSave.GameObjectList frameActivationList in this._activateAfterLoading)
		{
			object arg = "Game Activation Sequence step 2 : Activating GameObject List #";
			int num;
			list = (num = list) + 1;
			Debug.Log(arg + num);
			this.SetStartSequenceStep(2, (float)list / (float)this._activateAfterLoading.Count);
			foreach (GameObject gameObject3 in frameActivationList._frameJobs)
			{
				if (gameObject3)
				{
					gameObject3.SetActive(activate);
				}
				else
				{
					Debug.Log("Null GameObject in LoadSave's lists to activate. Please check something isn't missing or remove the entry.-");
				}
			}
			yield return null;
		}
		if (!CoopPeerStarter.DedicatedHost)
		{
			while (!LocalPlayer.GameObject)
			{
				yield return null;
			}
			GameObject endgameLoader = GameObject.FindWithTag("EndgameLoader");
			if (endgameLoader)
			{
				SceneLoadTrigger component = endgameLoader.GetComponent<SceneLoadTrigger>();
				if (LocalPlayer.ActiveAreaInfo.HasActiveEndgameArea)
				{
					component.SetCanLoad(true);
					component.ForceLoad();
				}
				else
				{
					Vector3 rhs = LocalPlayer.Transform.position - component.transform.position;
					if (Vector3.Dot(component.transform.forward, rhs) > 0f && rhs.magnitude < 150f)
					{
						component._onCrossingForwards.Invoke();
					}
				}
			}
			this.SetStartSequenceStep(3, 1f);
			Debug.Log("Game Activation Sequence step 3 : Enabling GreebleLayers, workscheduler, rainfollow");
			LocalPlayer.GreebleRoot.SetActive(true);
			Scene.RainFollowGO.GetComponent<SmoothTransformConstraint>().target = LocalPlayer.Transform.Find("RainTargetGo");
		}
		else
		{
			this.SetStartSequenceStep(3, 1f);
			Debug.Log("Game Activation Sequence step 3* : Enabling workscheduler");
		}
		WorkScheduler.ShouldDoFullCycle = true;
		if (Scene.PlaneCrash.Crashed || !Scene.PlaneCrash.ShowCrash || !Scene.PlaneCrashAnimGO || !Scene.PlaneCrashAnimGO.activeSelf)
		{
			Scene.WorkScheduler.gameObject.SetActive(true);
		}
		yield return null;
		yield return null;
		this.SetStartSequenceStep(4, 1f);
		Debug.Log("Game Activation Sequence step 4 : Initalize Game Mode");
		if (!BoltNetwork.isClient && (Prefabs.Instance.GameModePrefabs[(int)GameSetup.Game] || GameModeStarter.Prefab))
		{
			if (GameModeStarter.Prefab)
			{
				UnityEngine.Object.Instantiate<GameObject>(GameModeStarter.Prefab);
			}
			else
			{
				UnityEngine.Object.Instantiate<GameObject>(Prefabs.Instance.GameModePrefabs[(int)GameSetup.Game]);
			}
		}
		else
		{
			yield return YieldPresets.WaitPointFiveSeconds;
		}
		this.SetStartSequenceStep(5, 0.5f);
		Debug.Log("Game Activation Sequence step 5 : OnGameStart event");
		Time.timeScale = 1f;
		if (LoadSave.OnGameStart != null)
		{
			LoadSave.OnGameStart();
		}
		LoadSave.OnGameStart = null;
		if (BoltNetwork.isClient && isLoadingSave)
		{
			yield return YieldPresets.WaitPointFiveSeconds;
			Vector3 playerPos = LocalPlayer.Transform.position;
			LayerMask layers = LayerMask.GetMask(new string[]
			{
				"ReflectBig",
				"Terrain",
				"Prop",
				"Cave"
			});
			float mpClientDelayStart = Time.realtimeSinceStartup;
			RaycastHit hit;
			while (Time.realtimeSinceStartup - mpClientDelayStart < 15f && !Physics.Raycast(playerPos, Vector3.down, out hit, 3f, layers))
			{
				this.SetStartSequenceStep(5, 0.5f + (Time.realtimeSinceStartup - mpClientDelayStart) / 15f * 0.5f);
				LocalPlayer.Transform.position = playerPos;
				yield return null;
			}
		}
		this.SetStartSequenceStep(5, 1f);
		CaveTriggers.CheckPlayersInCave();
		MassDestructionSaveManager md = UnityEngine.Object.FindObjectOfType<MassDestructionSaveManager>();
		if (!md.GetComponent<GlobalDataSaver>())
		{
			md.gameObject.AddComponent<GlobalDataSaver>();
			StoreInformation component2 = md.GetComponent<StoreInformation>();
			if (!component2.Components.Contains("TheForest.Utils.GlobalDataSaver"))
			{
				component2.Components.Add("TheForest.Utils.GlobalDataSaver");
			}
		}
		if (!md.GetComponent<GreebleZonesManager>())
		{
			md.gameObject.AddComponent<GreebleZonesManager>();
			StoreInformation component3 = md.GetComponent<StoreInformation>();
			if (!component3.Components.Contains("GreebleZonesManager"))
			{
				component3.Components.Add("GreebleZonesManager");
			}
			UnityEngine.Object.FindObjectOfType<GreebleZonesManager>().RefreshGreebleZones();
		}
		if (!md.GetComponent<SpawnMutantsSerializerMessageProxy>())
		{
			md.gameObject.AddComponent<SpawnMutantsSerializerMessageProxy>();
		}
		foreach (Camera camera in UnityEngine.Object.FindObjectsOfType<Camera>())
		{
			camera.eventMask = 0;
		}
		Scene.LoadSave = null;
		Scene.Atmosphere.ForceSunRotationUpdate = true;
		Scene.SceneTracker.waitForLoadSequence = true;
		this.SetStartSequenceStep(6, 0.25f);
		yield return YieldPresets.WaitPointSixSeconds;
		if (!BoltNetwork.isClient)
		{
			while (Scene.SceneTracker.doingGlobalNavUpdate)
			{
				yield return null;
			}
		}
		if (!CoopPeerStarter.DedicatedHost)
		{
			VirtualCursor.Instance.SetCursorType(VirtualCursor.CursorTypes.Reset);
			VirtualCursor.Instance.SetCursorType(VirtualCursor.CursorTypes.None);
		}
		this.SetStartSequenceStep(6, 0.5f);
		if (!CoopPeerStarter.DedicatedHost)
		{
			if (LocalPlayer.ActiveAreaInfo.HasActiveEndgameArea)
			{
				while (!LocalPlayer.IsInEndgame)
				{
					yield return null;
				}
				yield return null;
			}
			LocalPlayer.Rigidbody.isKinematic = false;
			LocalPlayer.Rigidbody.useGravity = true;
			this.SetStartSequenceStep(6, 1f);
			Debug.Log("Game Activation Sequence step 6 : BlackScreen off");
			if (LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.Loading)
			{
				LocalPlayer.Inventory.CurrentView = PlayerInventory.PlayerViews.World;
			}
			Scene.HudGui.CheckHudState();
		}
		else
		{
			Scene.HudGui.gameObject.SetActive(false);
			this.SetStartSequenceStep(6, 1f);
			Debug.Log("Game Activation Sequence step 6* : BlackScreen off");
		}
		Scene.HudGui.Loading._cam.SetActive(false);
		Debug.Log("Game Activation Sequence step 7 : End of Sequence");
		if (LocalPlayer.Inventory)
		{
			if (LocalPlayer.Inventory.CurrentView != PlayerInventory.PlayerViews.PlaneCrash)
			{
				LocalPlayer.Inventory.enabled = true;
			}
			LocalPlayer.Transform.SendMessage("enableMpRenderers", SendMessageOptions.DontRequireReceiver);
			LocalPlayer.Transform.SendMessage("forceMecanimSync", SendMessageOptions.DontRequireReceiver);
		}
		if (BoltNetwork.isClient)
		{
			foreach (GameObject gameObject4 in Scene.MutantControler.activeNetCannibals)
			{
				if (gameObject4.activeSelf)
				{
					gameObject4.SendMessage("forceSkinColor", SendMessageOptions.DontRequireReceiver);
				}
			}
		}
		if (LocalPlayer.MainRotator)
		{
			LocalPlayer.Transform.localEulerAngles = new Vector3(0f, LocalPlayer.Transform.localEulerAngles.y, 0f);
			LocalPlayer.MainRotator.resetOriginalRotation = true;
		}
		if (LocalPlayer.IsInCaves && BoltNetwork.isClient)
		{
			playerInCave playerInCave = playerInCave.Create(GlobalTargets.OnlyServer);
			playerInCave.target = LocalPlayer.Transform.GetComponent<BoltEntity>();
			playerInCave.inCave = true;
			playerInCave.Send();
		}
		if (LocalPlayer.IsInCaves || LocalPlayer.IsInEndgame)
		{
			for (int j = 0; j < Scene.SceneTracker.caveEntrances.Count; j++)
			{
				Scene.SceneTracker.caveEntrances[j].disableCaveBlack();
			}
		}
		UnityEngine.Object.Destroy(this);
		BridgeAnchorHelper.Clear();
		if (BoltNetwork.isRunning && CoopHellDoors.Instance && CoopHellDoors.Instance.entity.isAttached)
		{
			CoopHellDoors.Instance.entity.Freeze(false);
		}
		if (SteamClientDSConfig.isDedicatedClient && LocalPlayer.Inventory.CurrentView == PlayerInventory.PlayerViews.World)
		{
			SteamClientDSConfig.IsClientAtWorld = true;
		}
		Scene.FinishGameLoad = true;
		LoadingProgress.Progress = 0f;
		timer.Stop();
		yield break;
	}

	
	private void SetStartSequenceStep(int step, float stepAlpha = 1f)
	{
		LoadingProgress.Progress = 0.6f + Mathf.Clamp01(Mathf.Lerp((float)(step - 1), (float)step, Mathf.Clamp01(stepAlpha)) / 5f) * 0.4f;
	}

	
	public static bool ShouldLoad;

	
	public List<LoadSave.GameObjectList> _activateAfterLoading;

	
	private bool startedSequence;

	
	[Serializable]
	public class GameObjectList
	{
		
		public List<GameObject> _frameJobs;
	}
}
