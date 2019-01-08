using System;
using System.Collections;
using TheForest.Commons.Delegates;
using TheForest.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheForest.World
{
	public class SceneUnloadInCave : MonoBehaviour
	{
		private void Awake()
		{
			WorkScheduler.RegisterGlobal(new WsTask(this.CheckInCave), false);
		}

		private void OnDestroy()
		{
			WorkScheduler.UnregisterGlobal(new WsTask(this.CheckInCave));
		}

		public void CheckInCave()
		{
			if ((!BoltNetwork.isRunning && LocalPlayer.IsInCaves) || (BoltNetwork.isRunning && TheForest.Utils.Scene.SceneTracker.AreAllPlayersInCave()) || this._forcedUnload)
			{
				this.Unload();
			}
			else
			{
				this.Load();
			}
		}

		public void ForcedUnload(bool onoff)
		{
			this._forcedUnload = onoff;
		}

		private void Load()
		{
			if (!this._loadedSceneRoot && this._loader == null)
			{
				base.StartCoroutine(this.LoadRoutine());
			}
		}

		private IEnumerator LoadRoutine()
		{
			this._loader = Application.LoadLevelAdditiveAsync(this._sceneName);
			yield return this._loader;
			this._loader = null;
			this.GetLoadedSceneRoot();
			yield break;
		}

		private void Unload()
		{
			if (this._loadedSceneRoot)
			{
				UnityEngine.Object.Destroy(this._loadedSceneRoot.gameObject);
				this._loadedSceneRoot = null;
				base.Invoke("DelayedCleanUp", 0.1f);
				SceneManager.UnloadSceneAsync(this._sceneName);
			}
		}

		private void DelayedCleanUp()
		{
			ResourcesHelper.UnloadUnusedAssets();
		}

		private GameObject GetLoadedSceneRoot()
		{
			if (!this._loadedSceneRoot)
			{
				GameObject gameObject = GameObject.FindWithTag(this._rootTag);
				if (gameObject)
				{
					this._loadedSceneRoot = gameObject.transform.root.gameObject;
				}
			}
			return this._loadedSceneRoot;
		}

		public string _sceneName;

		public string _rootTag;

		private bool _forcedUnload;

		private GameObject _loadedSceneRoot;

		private AsyncOperation _loader;
	}
}
