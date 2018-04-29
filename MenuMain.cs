using System;
using System.Collections;
using Pathfinding;
using Rewired;
using Steamworks;
using TheForest.Commons.Enums;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;


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
		TheForest.Utils.Input.player.controllers.maps.SetMapsEnabled(true, ControllerType.Keyboard, "Menu");
		TheForest.Utils.Input.player.controllers.maps.SetMapsEnabled(true, ControllerType.Joystick, "Menu");
	}

	
	private void OnDisable()
	{
		this.selected = false;
		EventDelegate.Remove(this.PlayButton.onClick, new EventDelegate.Callback(this.OnPlay));
		EventDelegate.Remove(this.ExitButton.onClick, new EventDelegate.Callback(this.OnExit));
		EventDelegate.Remove(this.ExitButtonMainMenu.onClick, new EventDelegate.Callback(this.OnExitMenu));
		EventDelegate.Remove(this.LoadButton.onClick, new EventDelegate.Callback(this.OnLoad));
		if (TheForest.Utils.Input.player != null && (!LocalPlayer.Inventory || LocalPlayer.Inventory.CurrentView != PlayerInventory.PlayerViews.Pause))
		{
			TheForest.Utils.Input.player.controllers.maps.SetMapsEnabled(false, ControllerType.Keyboard, "Menu");
			TheForest.Utils.Input.player.controllers.maps.SetMapsEnabled(false, ControllerType.Joystick, "Menu");
		}
	}

	
	private void OnPlay()
	{
		if (this.selected)
		{
			return;
		}
		Scene.HudGui.TogglePauseMenu(false);
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
			base.StartCoroutine(this.WaitForBoltShutdown(delegate
			{
				CoopSteamServer.Shutdown();
				CoopSteamClient.Shutdown();
				CoopTreeGrid.Clear();
				GameSetup.SetInitType(InitTypes.New);
				GameSetup.SetGameType(GameTypes.Standard);
				GeoHash.ClearAll();
				Application.LoadLevel("TitleSceneLoader");
			}));
		}
		else
		{
			CoopTreeGrid.Clear();
			GameSetup.SetInitType(InitTypes.New);
			GameSetup.SetGameType(GameTypes.Standard);
			GeoHash.ClearAll();
			Resources.UnloadUnusedAssets();
			GC.Collect();
			Application.LoadLevel("TitleSceneLoader");
		}
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
		Matrix4x4 matrix = GUI.matrix;
		GUIUtility.RotateAroundPivot(Time.time * 360f, new Vector2((float)(Screen.width / 2), (float)(Screen.height / 2)));
		GUI.DrawTexture(new Rect((float)(Screen.width / 2 - 32), (float)(Screen.height / 2 - 32), 64f, 64f), Resources.Load("CoopLoaderTexture") as Texture2D);
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

	
	private IEnumerator WaitForBoltShutdown(Action done)
	{
		yield return null;
		if (CoopLobby.IsInLobby)
		{
			if (CoopLobby.Instance.Info.IsOwner)
			{
				CoopLobby.Instance.Destroy();
			}
			CoopLobby.LeaveActive();
		}
		if (BoltNetwork.isClient)
		{
			Debug.Log("DISCONNECT FROM SERVER");
			if (SteamClientDSConfig.isDedicatedClient)
			{
				SteamUser.TerminateGameConnection(SteamClientDSConfig.EndPoint.Address.Packed, SteamClientDSConfig.EndPoint.Port);
			}
			BoltNetwork.server.Disconnect();
		}
		yield return new WaitForSeconds(0.5f);
		BoltLauncher.Shutdown();
		done();
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

	
	public UIButton ExitButtonMainMenu;

	
	private PauseMenuAudio audio;

	
	private Texture2D texture_Overlay;
}
