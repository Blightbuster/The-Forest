using System;
using System.Collections;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class CaveData
{
	public GameObject GetLoadedSceneRoot()
	{
		return this.LoadedCaveGo;
	}

	public void Unload()
	{
		if (this.LoadedCaveGo && this.Async == null)
		{
			SceneManager.sceneUnloaded += this.SceneManager_sceneUnloaded;
			this.Async = SceneManager.UnloadSceneAsync(this.SceneName);
		}
	}

	public IEnumerator LoadIn()
	{
		if (!this.LoadedOrLoading && !this.GetLoadedSceneRoot())
		{
			this.Loading = true;
			SceneManager.sceneLoaded += this.SceneManager_sceneLoaded;
			this.Async = SceneManager.LoadSceneAsync(this.SceneName, LoadSceneMode.Additive);
			if (LocalPlayer.Inventory && LocalPlayer.Inventory.CurrentView != PlayerInventory.PlayerViews.Loading && !TheForest.Utils.Scene.HudGui.LoadingCavesInfo.activeSelf)
			{
				TheForest.Utils.Scene.HudGui.LoadingCavesInfo.SetActive(true);
				TheForest.Utils.Scene.HudGui.LoadingCavesFill.fillAmount = 0.1f;
				while (this.Async != null && !this.Async.isDone)
				{
					TheForest.Utils.Scene.HudGui.LoadingCavesFill.fillAmount = Mathf.Max(0.1f, this.Async.progress / 2f);
					yield return null;
				}
				yield return null;
				TheForest.Utils.Scene.HudGui.LoadingCavesInfo.SetActive(false);
				TheForest.Utils.Scene.HudGui.Grid.repositionNow = true;
			}
			yield break;
		}
		yield break;
	}

	private void SceneManager_sceneLoaded(UnityEngine.SceneManagement.Scene arg0, LoadSceneMode arg1)
	{
		if (arg0.name == this.SceneName)
		{
			SceneManager.sceneLoaded -= this.SceneManager_sceneLoaded;
			if (arg0.rootCount == 1)
			{
				this.LoadedCaveGo = arg0.GetRootGameObjects()[0];
			}
			else
			{
				Debug.Log("More than one root object in cave scene: " + this.SceneName);
				this.LoadedCaveGo = GameObject.Find(this.RootName);
			}
			this.Loading = false;
			this.Async = null;
		}
	}

	private void SceneManager_sceneUnloaded(UnityEngine.SceneManagement.Scene arg0)
	{
		if (arg0.name == this.SceneName)
		{
			SceneManager.sceneUnloaded -= this.SceneManager_sceneUnloaded;
			this.LoadedCaveGo = null;
			this.Loading = false;
			this.Async = null;
		}
	}

	public bool LoadedOrLoading
	{
		get
		{
			return this.LoadedCaveGo || this.Loading || this.Async != null;
		}
	}

	public string SceneName;

	public string RootName;

	public GameObject LoadedCaveGo;

	public AsyncOperation Async;

	public bool Loading;

	public bool AllowInEndgame;
}
