using System;
using AssetBundles;
using TheForest.Buildings.Creation;
using TheForest.Buildings.World;
using TheForest.UI;
using TheForest.Utils;
using UnityEngine;

public class ClearStaticVars : MonoBehaviour
{
	private void Awake()
	{
		BuildMission.ActiveMissions.Clear();
		Clock.Day = 0;
		if (!this.MainScene)
		{
			RainEffigy.RainAdd = 0;
			Scene.FinishGameLoad = false;
			LoadingProgress.Progress = 0f;
			InsideCheck.ClearStaticVars();
			SteamClientDSConfig.Clear();
			CoopLobby.HostGuid = null;
			OverlayIconManager.Clear();
			Cheats.SetAllowed(true);
		}
		AssetBundleManager.Initialize();
		Time.timeScale = 1f;
	}

	public bool MainScene = true;
}
