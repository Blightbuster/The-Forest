using System;
using System.Collections;
using UnityEngine;


[Serializable]
public class CaveData
{
	
	public GameObject GetLoadedSceneRoot()
	{
		if (!this.LoadedCaveGo)
		{
			this.LoadedCaveGo = GameObject.Find(this.RootName);
		}
		return this.LoadedCaveGo;
	}

	
	public void Unload()
	{
		if (this.LoadedCaveGo)
		{
			UnityEngine.Object.Destroy(this.LoadedCaveGo);
		}
		this.Async = null;
	}

	
	public IEnumerator LoadIn()
	{
		if (!this.LoadedOrLoading && !this.GetLoadedSceneRoot())
		{
			this.Loading = true;
			this.Async = Application.LoadLevelAdditiveAsync(this.SceneName);
			yield return this.Async;
			if (this.Async != null)
			{
				this.Async = null;
				this.LoadedCaveGo = GameObject.Find(this.RootName);
				if (this.LoadedCaveGo)
				{
					Debug.Log(this.LoadedCaveGo + " loading complete");
				}
			}
			this.Loading = false;
		}
		yield break;
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
}
