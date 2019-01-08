using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Pathfinding;
using Steamworks;
using TheForest.Commons.Enums;
using TheForest.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuMain : MonoBehaviour
{
	private void OnEnable()
	{
		this.selected = false;
		EventDelegate.Add(this.PlayButton.onClick, new EventDelegate.Callback(this.OnPlay));
		EventDelegate.Add(this.ExitButton.onClick, new EventDelegate.Callback(this.OnExit));
		EventDelegate.Add(this.ExitButtonMainMenu.onClick, new EventDelegate.Callback(this.OnExitMenu));
		EventDelegate.Add(this.LoadButton.onClick, new EventDelegate.Callback(this.OnLoad));
		this.audio = this.MenuRoot.GetComponentInChildren<PauseMenuAudio>();
		TheForest.Utils.Input.SetState(InputState.Menu, true);
	}

	private void OnDisable()
	{
		this.selected = false;
		EventDelegate.Remove(this.PlayButton.onClick, new EventDelegate.Callback(this.OnPlay));
		EventDelegate.Remove(this.ExitButton.onClick, new EventDelegate.Callback(this.OnExit));
		EventDelegate.Remove(this.ExitButtonMainMenu.onClick, new EventDelegate.Callback(this.OnExitMenu));
		EventDelegate.Remove(this.LoadButton.onClick, new EventDelegate.Callback(this.OnLoad));
		TheForest.Utils.Input.SetState(InputState.Menu, false);
	}

	private void OnPlay()
	{
		if (this.selected)
		{
			return;
		}
		TheForest.Utils.Scene.HudGui.TogglePauseMenu(false);
	}

	private void OnLoad()
	{
		if (this.selected)
		{
			return;
		}
		this.selected = true;
		if (LevelSerializer.CanResume)
		{
			this.LoadingBar.SetActive(true);
			TheForest.Utils.Input.LockMouse();
			if (this.audio != null)
			{
				this.audio.PrepareForLevelLoad();
			}
			base.Invoke("DelayedLoadLevel", 0.1f);
		}
		Time.timeScale = 1f;
		this.MenuRoot.gameObject.SetActive(false);
	}

	private void OnExitMenu()
	{
		if (this.selected)
		{
			return;
		}
		this.selected = true;
		MenuMain.exitingToMenu = true;
		if (LocalPlayer.SpecialActions && LocalPlayer.AnimControl && LocalPlayer.AnimControl.holdingGlider)
		{
			LocalPlayer.SpecialActions.SendMessage("DropGlider", false);
		}
		Time.timeScale = 1f;
		if (this.audio != null)
		{
			this.audio.PrepareForLevelLoad();
		}
		WorkScheduler.ClearInstance();
		UniqueIdentifier.AllIdentifiers.Clear();
		RecastMeshObj.Clear();
		if (BoltNetwork.isRunning)
		{
			if (MenuMain.<>f__mg$cache0 == null)
			{
				MenuMain.<>f__mg$cache0 = new Action(MenuMain.PostBoltShutdown);
			}
			base.StartCoroutine(this.WaitForBoltShutdown(MenuMain.<>f__mg$cache0));
			return;
		}
		CoopTreeGrid.Clear();
		GameSetup.SetInitType(InitTypes.New);
		GameSetup.SetGameType(GameTypes.Standard);
		GeoHash.ClearAll();
		SceneManager.LoadScene("TitleSceneLoader", LoadSceneMode.Single);
	}

	private static void PostBoltShutdown()
	{
		CoopSteamServer.Shutdown();
		CoopSteamClient.Shutdown();
		CoopTreeGrid.Clear();
		GameSetup.SetInitType(InitTypes.New);
		GameSetup.SetGameType(GameTypes.Standard);
		GeoHash.ClearAll();
		SceneManager.LoadScene("TitleSceneLoader", LoadSceneMode.Single);
	}

	private void DelayedLoadLevel()
	{
		LevelSerializer.Resume();
	}

	private void OnExit()
	{
		if (this.selected)
		{
			return;
		}
		this.selected = true;
		WorkScheduler.ClearInstance();
		UniqueIdentifier.AllIdentifiers.Clear();
		if (BoltNetwork.isRunning)
		{
			UnityEngine.Object.DontDestroyOnLoad(base.transform.root.gameObject);
			MenuMain.exitingToMenu = true;
			MenuMain.exiting = true;
			base.StartCoroutine(this.WaitForBoltShutdown(delegate
			{
				Application.Quit();
			}));
		}
		else
		{
			Application.Quit();
		}
	}

	private void DrawOverlay()
	{
		if (this.texture_Overlay)
		{
			GUI.DrawTexture(new Rect(0f, 0f, (float)Screen.width, (float)Screen.height), this.texture_Overlay, ScaleMode.StretchToFill, true);
		}
	}

	private void DrawLoader()
	{
		this.DrawOverlay();
		if (this.LoaderTexture == null)
		{
			return;
		}
		Matrix4x4 matrix = GUI.matrix;
		GUIUtility.RotateAroundPivot(Time.time * 360f, new Vector2((float)(Screen.width / 2), (float)(Screen.height / 2)));
		if (this.LoaderTexture != null)
		{
			GUI.DrawTexture(new Rect((float)(Screen.width / 2 - 32), (float)(Screen.height / 2 - 32), 64f, 64f), this.LoaderTexture);
		}
		GUI.matrix = matrix;
	}

	private void OnGUI()
	{
		if (this.selected)
		{
			if (!this.texture_Overlay)
			{
				this.texture_Overlay = new Texture2D(2, 2);
				this.texture_Overlay.SetPixels(new Color[]
				{
					new Color(0f, 0f, 0f, 0.75f),
					new Color(0f, 0f, 0f, 0.75f),
					new Color(0f, 0f, 0f, 0.75f),
					new Color(0f, 0f, 0f, 0.75f)
				});
				this.texture_Overlay.Apply();
			}
			this.DrawOverlay();
			this.DrawLoader();
		}
	}

	private IEnumerator WaitForBoltShutdown(Action postBoltShutdownAction)
	{
		yield return null;
		yield return YieldPresets.WaitPointFiveSeconds;
		if (CoopLobby.IsInLobby)
		{
			if (CoopLobby.Instance.Info.IsOwner)
			{
				CoopLobby.Instance.Destroy();
			}
			CoopLobby.LeaveActive();
		}
		if (BoltNetwork.isRunning)
		{
			if (BoltNetwork.isClient)
			{
				BoltNetwork.server.Disconnect();
				Debug.Log("DISCONNECT FROM SERVER");
				if (SteamClientDSConfig.isDedicatedClient)
				{
					SteamUser.TerminateGameConnection(SteamClientDSConfig.EndPoint.Address.Packed, SteamClientDSConfig.EndPoint.Port);
				}
			}
			else
			{
				BoltLauncher.Shutdown();
			}
			int loopCount = 0;
			float timer = Time.realtimeSinceStartup;
			while (BoltNetwork.isRunning)
			{
				loopCount++;
				yield return null;
			}
			Debug.Log(string.Concat(new object[]
			{
				"BoltShutdown took (",
				loopCount,
				" frames) ",
				Time.realtimeSinceStartup - timer,
				"s"
			}));
		}
		if (postBoltShutdownAction != null)
		{
			postBoltShutdownAction();
		}
		yield break;
	}

	private void OnLevelWasLoaded()
	{
		if (MenuMain.exiting)
		{
			Application.Quit();
		}
	}

	private bool selected;

	public static bool exitingToMenu;

	public static bool exiting;

	public GameObject LoadingBar;

	public Transform MenuRoot;

	public UIButton PlayButton;

	public UIButton LoadButton;

	public UIButton ExitButton;

	public UIButton OptionsButton;

	public UIButton ExitButtonMainMenu;

	public Texture2D LoaderTexture;

	private PauseMenuAudio audio;

	private Texture2D texture_Overlay;

	[CompilerGenerated]
	private static Action <>f__mg$cache0;
}
