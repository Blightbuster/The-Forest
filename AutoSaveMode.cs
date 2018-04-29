using System;
using System.Collections;
using TheForest.Utils;
using UnityEngine;


public class AutoSaveMode : MonoBehaviour
{
	
	private void Awake()
	{
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	
	private void Start()
	{
		base.StartCoroutine(this.StartWhenready());
	}

	
	private void Update()
	{
		if (!Scene.FinishGameLoad)
		{
			return;
		}
		if ((Scene.SceneTracker && Scene.SceneTracker.allPlayers.Count == 0) || SteamDSConfig.isUsingDummyPlayer)
		{
			if (this.autosaveWhenEmpty)
			{
				this.autosaveWhenEmpty = false;
				this.timer = (float)(60 * SteamDSConfig.GameAutoSaveIntervalMinutes);
				base.StartCoroutine(this.AutoSave());
			}
			return;
		}
		this.autosaveWhenEmpty = true;
		if (SteamDSConfig.GameAutoSaveIntervalMinutes > 0 && this.timer > 0f)
		{
			this.timer -= Time.deltaTime;
			if (this.timer <= 0f)
			{
				this.timer = (float)(60 * SteamDSConfig.GameAutoSaveIntervalMinutes);
				base.StartCoroutine(this.AutoSave());
			}
		}
	}

	
	private IEnumerator StartWhenready()
	{
		this.timer = 0f;
		while (!Scene.FinishGameLoad)
		{
			yield return null;
		}
		this.timer = (float)(60 * SteamDSConfig.GameAutoSaveIntervalMinutes);
		Debug.Log("Game autosave started");
		yield break;
	}

	
	private IEnumerator AutoSave()
	{
		while (!Scene.FinishGameLoad)
		{
			yield return null;
		}
		SteamDSConfig.SetServerStatus(1);
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		SteamDSConfig.SaveGame();
		SteamDSConfig.SetServerStatus(0);
		Debug.Log("Game saved");
		yield break;
	}

	
	private float timer;

	
	private bool autosaveWhenEmpty;
}
