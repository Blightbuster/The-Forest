using System;
using System.Collections;
using Rewired;
using TheForest.Commons.Enums;
using TheForest.Modding;
using TheForest.Modding.Bridge;
using TheForest.Modding.Bridge.Interfaces;
using TheForest.UI;
using TheForest.Utils;
using TheForest.Utils.Settings;
using UnityEngine;
using UnityEngine.SceneManagement;


public class TitleScreen : MonoBehaviour, ITitleScene
{
	
	private void Awake()
	{
		GameModeStarter.Prefab = null;
		GameSetup.SetInitType(InitTypes.New);
		GameSetup.SetGameType(GameTypes.Standard);
		GameSetup.SetDifficulty(DifficultyModes.Normal);
		TitleSceneBridge.TitleScene = this;
		TitleSceneBridge.GameSetup = GameSetup.Bridge;
		TitleSceneBridge.Cheats = Cheats.Bridge;
		LoadSave.ShouldLoad = false;
		CoopAckChecker.ACKED = false;
		CoopSteamServer.Shutdown();
		CoopSteamClient.Shutdown();
		CoopTreeGrid.Clear();
		GeoHash.ClearAll();
		TitleScreen.Instance = this;
		if (LoadAsync.Scenery)
		{
			UnityEngine.Object.Destroy(LoadAsync.Scenery);
			LoadAsync.Scenery = null;
		}
		this.InitMpScreenScenery();
	}

	
	private void Start()
	{
		VirtualCursor.Instance.SetCursorType(VirtualCursor.CursorTypes.None);
		GameSettings.Init();
		TheForest.Utils.Input.player.controllers.maps.SetMapsEnabled(true, ControllerType.Joystick, "Default");
		TheForest.Utils.Input.player.controllers.maps.SetMapsEnabled(true, ControllerType.Keyboard, "Menu");
		TheForest.Utils.Input.player.controllers.maps.SetMapsEnabled(true, ControllerType.Joystick, "Menu");
	}

	
	private void OnEnable()
	{
		this.BreadCrumbLevel0();
		CoopSteamNGUI coopSteamNGUI = UnityEngine.Object.FindObjectOfType<CoopSteamNGUI>();
		if (coopSteamNGUI)
		{
			UnityEngine.Object.Destroy(coopSteamNGUI.gameObject);
		}
		if (BoltNetwork.isRunning)
		{
			BoltLauncher.Shutdown();
		}
		if (MenuMain.exiting)
		{
			Application.Quit();
		}
		else
		{
			MenuMain.exitingToMenu = false;
		}
		TheForest.Utils.Input.UnLockMouse();
	}

	
	private void Update()
	{
		Application.targetFrameRate = 60;
	}

	
	private void OnDestroy()
	{
		TitleSceneBridge.TitleScene = null;
		PlayerPreferences.ApplyMaxFrameRate();
		base.enabled = false;
		if (TitleScreen.Instance == this)
		{
			TitleScreen.Instance = null;
		}
	}

	
	public void BreadCrumbLevel0()
	{
		this.BreadcumbLabel.enabled = false;
	}

	
	public void BreadCrumbLevel1()
	{
		this.BreadcumbLabel.enabled = true;
		this.BreadcumbLabel.text = GameSetup.Mode.ToString();
	}

	
	public void BreadCrumbLevel2()
	{
		this.BreadcumbLabel.enabled = true;
		this.BreadcumbLabel.text = GameSetup.Mode + "." + GameSetup.Init;
	}

	
	public void BreadCrumbLevel2Mp()
	{
		this.BreadcumbLabel.enabled = true;
		this.BreadcumbLabel.text = GameSetup.Mode + "." + GameSetup.MpType;
	}

	
	public void BreadCrumbLevel3Mp()
	{
		this.BreadcumbLabel.enabled = true;
		this.BreadcumbLabel.text = string.Concat(new object[]
		{
			GameSetup.Mode,
			".",
			GameSetup.MpType,
			".",
			GameSetup.Init
		});
	}

	
	public void OnStartDedicated()
	{
		SceneManager.LoadScene("SteamStartSceneDedicatedServer", LoadSceneMode.Single);
	}

	
	public void OnJoinDedicated()
	{
		SceneManager.LoadScene("SteamStartSceneDedicatedServer_Client", LoadSceneMode.Single);
	}

	
	public void OnSinglePlayer()
	{
		GameSetup.SetPlayerMode(PlayerModes.SinglePlayer);
		this.BreadCrumbLevel1();
	}

	
	public void OnCoOp()
	{
		GameSetup.SetPlayerMode(PlayerModes.Multiplayer);
		this.BreadCrumbLevel1();
	}

	
	public void OnMpHost()
	{
		GameSetup.SetMpType(MpTypes.Server);
		this.BreadCrumbLevel2Mp();
	}

	
	public void OnNewPeacefulGame()
	{
		this.OnNewGame(DifficultyModes.Peaceful);
	}

	
	public void OnNewNormalGame()
	{
		this.OnNewGame(DifficultyModes.Normal);
	}

	
	public void OnNewHardGame()
	{
		this.OnNewGame(DifficultyModes.Hard);
	}

	
	public void OnNewHardSurvivalGame()
	{
		this.OnNewGame(DifficultyModes.HardSurvival);
	}

	
	public void OnNewCreativeGame()
	{
		GameSetup.SetGameType(GameTypes.Creative);
		this.OnNewGame(DifficultyModes.Peaceful);
	}

	
	public void OnNewGame(DifficultyModes difficulty)
	{
		GameSetup.SetDifficulty(difficulty);
		GameSetup.SetInitType(InitTypes.New);
		if (GameSetup.IsSinglePlayer)
		{
			this.BreadCrumbLevel2();
			PlaneCrashAudioState.Spawn();
			LoadSave.ShouldLoad = (GameSetup.Init == InitTypes.Continue);
			if (!this.MyLoader)
			{
				this.FixMissingLoaderRef();
			}
			this.MyLoader.SetActive(true);
		}
		else
		{
			this.BreadCrumbLevel3Mp();
			this.InitMpScreenScenery();
			SceneManager.LoadScene(this.CoopScene, LoadSceneMode.Single);
		}
		this.MenuRoot.gameObject.SetActive(false);
	}

	
	public void OnLoad()
	{
		GameSetup.SetInitType(InitTypes.Continue);
		if (GameSetup.IsSinglePlayer)
		{
			this.BreadCrumbLevel2();
		}
		else
		{
			this.BreadCrumbLevel3Mp();
		}
	}

	
	public void OnSlotSelection(int slotNum)
	{
		GameSetup.SetSlot((Slots)slotNum);
		if (GameSetup.IsSinglePlayer)
		{
			LoadSave.ShouldLoad = (GameSetup.Init == InitTypes.Continue);
			if (!this.MyLoader)
			{
				this.FixMissingLoaderRef();
			}
			this.MyLoader.SetActive(true);
		}
		else
		{
			this.InitMpScreenScenery();
			SceneManager.LoadScene(this.CoopScene, LoadSceneMode.Single);
		}
		this.MenuRoot.gameObject.SetActive(false);
	}

	
	public void OnStartMpClient()
	{
		this.InitMpScreenScenery();
		GameSetup.SetMpType(MpTypes.Client);
		SceneManager.LoadScene(this.CoopScene, LoadSceneMode.Single);
		this.MenuRoot.gameObject.SetActive(false);
	}

	
	private void InitMpScreenScenery()
	{
		LoadAsync.Scenery = GameObject.Find("FakeCave");
		if (LoadAsync.Scenery)
		{
			LoadAsync.Scenery.transform.parent = null;
			UnityEngine.Object.DontDestroyOnLoad(LoadAsync.Scenery);
		}
	}

	
	private void OnCredits()
	{
		Application.LoadLevelAsync("CreditsScene");
	}

	
	public void OnExit()
	{
		PlayerPreferences.Save();
		Application.Quit();
	}

	
	private void FixMissingLoaderRef()
	{
		IEnumerator enumerator = base.transform.parent.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				Transform transform = (Transform)obj;
				if (transform.name == "Loading")
				{
					this.MyLoader = transform.gameObject;
				}
			}
		}
		finally
		{
			IDisposable disposable;
			if ((disposable = (enumerator as IDisposable)) != null)
			{
				disposable.Dispose();
			}
		}
	}

	
	public UILabel BreadcumbLabel;

	
	public GameObject MyLoader;

	
	public Transform MenuRoot;

	
	public string CoopScene = "SteamStartScene";

	
	public string CoopScenePS4 = "SteamStartSceneNGUI";

	
	public GameObject PlayerPrefab;

	
	public static TitleScreen Instance;
}
