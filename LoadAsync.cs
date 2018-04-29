using System;
using System.Collections;
using TheForest.Commons.Enums;
using TheForest.UI;
using TheForest.Utils;
using UnityEngine;


public class LoadAsync : MonoBehaviour
{
	
	
	public float progress
	{
		get
		{
			if (!GameSetup.IsSavedGame)
			{
				return (this.async == null) ? 0f : this.async.progress;
			}
			if (LevelSerializer.IsDeserializing)
			{
				return 0.5f;
			}
			return 1f;
		}
	}

	
	
	public bool isDone
	{
		get
		{
			if (GameSetup.IsSavedGame)
			{
				return !LevelSerializer.IsDeserializing;
			}
			return this.async != null && this.async.isDone;
		}
	}

	
	private void Start()
	{
		TheForest.Utils.Input.LockMouse();
		base.transform.parent = null;
		if (this.Menu)
		{
			UnityEngine.Object.Destroy(this.Menu);
		}
		if (LoadAsync.Scenery)
		{
			UnityEngine.Object.Destroy(LoadAsync.Scenery);
			LoadAsync.Scenery = null;
		}
		base.StartCoroutine(this.LoadLevelWithProgress(this._levelName));
	}

	
	private IEnumerator LoadLevelWithProgress(string levelToLoad)
	{
		if (this.delayForCleanup)
		{
			Resources.UnloadUnusedAssets();
			GC.Collect();
			yield return null;
		}
		float failsafe = 5f;
		while (BoltNetwork.isRunning && failsafe > 0f)
		{
			failsafe -= Time.deltaTime;
			yield return null;
		}
		yield return null;
		bool canResume = LevelSerializer.CanResume;
		bool boltIsRunning = BoltNetwork.isRunning;
		bool boltIsServer = BoltNetwork.isServer;
		bool boltIsClient = BoltNetwork.isClient;
		bool isSavedGame = GameSetup.IsSavedGame;
		if (canResume && (!boltIsRunning || boltIsServer) && isSavedGame)
		{
			ResourceRequest req = Resources.LoadAsync("PreloadingPrefabs");
			while (!req.isDone)
			{
				yield return null;
			}
			UnityEngine.Object.Instantiate(req.asset);
			yield return null;
			LevelSerializer.InitPrefabList();
			yield return null;
			LevelSerializer.Resume();
			yield break;
		}
		if (!boltIsClient && !canResume)
		{
			GameSetup.SetInitType(InitTypes.New);
		}
		this.async = Application.LoadLevelAsync(levelToLoad);
		while (!this.async.isDone)
		{
			LoadingProgress.Progress = this.async.progress * 0.5f;
			yield return null;
		}
		LoadingProgress.Progress = 0.5f;
		UnityEngine.Object.Destroy(base.gameObject);
		yield break;
	}

	
	public GameObject Menu;

	
	public static GameObject Scenery;

	
	public bool SavedGameLoader;

	
	public GameObject LoadSavedGameObject;

	
	public AsyncOperation async;

	
	public string _levelName = "ForestMain_v07";

	
	public bool delayForCleanup;

	
	internal bool showGUI = true;
}
