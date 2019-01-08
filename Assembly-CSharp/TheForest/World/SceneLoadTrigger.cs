using System;
using System.Collections;
using Bolt;
using TheForest.Items.Inventory;
using TheForest.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace TheForest.World
{
	public class SceneLoadTrigger : MonoBehaviour
	{
		private void OnTriggerExit(Collider other)
		{
			if (other.CompareTag(this._triggerTag))
			{
				bool flag = Vector3.Dot(base.transform.forward, other.transform.position - base.transform.position) > 0f;
				SceneLoadTrigger.Actions actions = (!flag) ? this._backwardAction : this._forwardAction;
				if (flag)
				{
					this._onCrossingForwards.Invoke();
				}
				else if (!flag)
				{
					this._onCrossingBackwards.Invoke();
				}
				if (actions != this._runningAction && actions != this._pendingAction)
				{
					this._pendingAction = actions;
					this.DoPendingAction();
				}
			}
		}

		public void ForceLoad()
		{
			if (this._canLoad && this._runningAction != SceneLoadTrigger.Actions.Load)
			{
				this._pendingAction = SceneLoadTrigger.Actions.Load;
				if (this._runningAction == SceneLoadTrigger.Actions.DelayedLoad || this._runningAction == SceneLoadTrigger.Actions.DelayedUnload)
				{
					this.OnActionFinished();
				}
				else
				{
					this.DoPendingAction();
				}
			}
		}

		public void ForceUnload()
		{
			if (this._runningAction != SceneLoadTrigger.Actions.Unload)
			{
				this._pendingAction = SceneLoadTrigger.Actions.Unload;
				if (this._runningAction == SceneLoadTrigger.Actions.DelayedLoad || this._runningAction == SceneLoadTrigger.Actions.DelayedUnload)
				{
					this.OnActionFinished();
				}
				else
				{
					this.DoPendingAction();
				}
			}
		}

		public void SetCanLoad(bool canLoad)
		{
			this._canLoad = canLoad;
		}

		private void DoPendingAction()
		{
			if (this._runningAction == SceneLoadTrigger.Actions.None)
			{
				this._runningAction = this._pendingAction;
				this._pendingAction = SceneLoadTrigger.Actions.None;
				SceneLoadTrigger.Actions runningAction = this._runningAction;
				if (runningAction != SceneLoadTrigger.Actions.Load)
				{
					if (runningAction != SceneLoadTrigger.Actions.Unload)
					{
						if (runningAction == SceneLoadTrigger.Actions.CrossingEventOnly)
						{
							this.OnActionFinished();
						}
					}
					else
					{
						TheForest.Utils.Scene.ActiveMB.StartCoroutine(this.UnloadScene());
					}
				}
				else
				{
					base.StartCoroutine(this.StreamSceneRoutine());
				}
			}
		}

		private void OnActionFinished()
		{
			this._runningAction = SceneLoadTrigger.Actions.None;
			this.DoPendingAction();
		}

		private GameObject GetLoadedSceneRoot()
		{
			return this._loadedSceneRoot;
		}

		private IEnumerator StreamSceneRoutine()
		{
			if (!this.GetLoadedSceneRoot() && this._canLoad)
			{
				this._onBeforeLoad.Invoke();
				if (this._loadDelay > 0f)
				{
					if (LocalPlayer.Inventory && LocalPlayer.Inventory.CurrentView != PlayerInventory.PlayerViews.Loading && !TheForest.Utils.Scene.HudGui.LoadingCavesInfo.activeSelf)
					{
						TheForest.Utils.Scene.HudGui.LoadingCavesInfo.SetActive(true);
						TheForest.Utils.Scene.HudGui.LoadingCavesFill.fillAmount = 0.1f;
					}
					yield return new WaitForSeconds(this._loadDelay);
				}
				SceneManager.sceneLoaded += this.SceneManager_sceneLoaded;
				SceneManager.LoadScene(this._sceneName, LoadSceneMode.Additive);
				if (LocalPlayer.Inventory && LocalPlayer.Inventory.CurrentView != PlayerInventory.PlayerViews.Loading && TheForest.Utils.Scene.HudGui.LoadingCavesInfo.activeSelf)
				{
					TheForest.Utils.Scene.HudGui.LoadingCavesFill.fillAmount = Mathf.Max(0.1f, 0.5f);
				}
				yield return null;
				if (this._loadAnimPrefabs)
				{
					base.StartCoroutine(this.loadEndBossScene());
				}
				else
				{
					TheForest.Utils.Scene.HudGui.LoadingCavesInfo.SetActive(false);
					TheForest.Utils.Scene.HudGui.Grid.repositionNow = true;
				}
			}
			else
			{
				this.OnActionFinished();
			}
			yield break;
		}

		private void SceneManager_sceneLoaded(UnityEngine.SceneManagement.Scene arg0, LoadSceneMode arg1)
		{
			if (arg0.name == this._sceneName)
			{
				SceneManager.sceneLoaded -= this.SceneManager_sceneLoaded;
				if (arg0.rootCount == 1)
				{
					this._loadedSceneRoot = arg0.GetRootGameObjects()[0];
				}
				else
				{
					Debug.Log("More than one root object in cave scene: " + this._sceneName);
					this._loadedSceneRoot = (string.IsNullOrEmpty(this._sceneObjectName) ? GameObject.FindWithTag(this._sceneObjectTag) : GameObject.Find(this._sceneObjectName));
				}
				this._onFinishedLoading.Invoke();
				this.OnActionFinished();
			}
		}

		private IEnumerator loadEndBossScene()
		{
			yield return YieldPresets.WaitTwoSeconds;
			if (GameObject.FindWithTag("endGameBossPrefab") == null)
			{
				if (BoltNetwork.isClient)
				{
					yield return YieldPresets.WaitTwoSeconds;
					if (GameObject.FindWithTag("endGameBossPrefab") == null)
					{
						AsyncOperation async = SceneManager.LoadSceneAsync(this._animationPrefabsSceneName, LoadSceneMode.Additive);
						while (!async.isDone)
						{
							TheForest.Utils.Scene.HudGui.LoadingCavesFill.fillAmount = Mathf.Max(0.1f, 0.5f + async.progress / 2f);
							yield return null;
						}
					}
					Debug.Log("client loaded end boss prefab");
					if (SteamClientDSConfig.isDedicatedClient)
					{
						loadEndGamePrefabs loadEndGamePrefabs = loadEndGamePrefabs.Create(GlobalTargets.OnlyServer);
						loadEndGamePrefabs.sceneName = this._animationPrefabsSceneName;
						loadEndGamePrefabs.Send();
					}
				}
				else if (GameObject.FindWithTag("endGameBossPrefab") == null)
				{
					AsyncOperation async2 = SceneManager.LoadSceneAsync(this._animationPrefabsSceneName, LoadSceneMode.Additive);
					Debug.Log("loading animation prefabs on server");
					while (!async2.isDone)
					{
						TheForest.Utils.Scene.HudGui.LoadingCavesFill.fillAmount = Mathf.Max(0.1f, 0.5f + async2.progress / 2f);
						yield return null;
					}
				}
			}
			TheForest.Utils.Scene.HudGui.LoadingCavesInfo.SetActive(false);
			TheForest.Utils.Scene.HudGui.Grid.repositionNow = true;
			yield return null;
			yield break;
		}

		private IEnumerator UnloadScene()
		{
			if (this.GetLoadedSceneRoot())
			{
				this._onBeforeUnload.Invoke();
				if (this._unloadDelay > 0f)
				{
					yield return new WaitForSeconds(this._unloadDelay);
				}
				SceneManager.sceneUnloaded += this.SceneManager_sceneUnloaded;
				yield return SceneManager.UnloadSceneAsync(this._sceneName);
				yield return null;
				yield return null;
				ResourcesHelper.UnloadUnusedAssets();
				yield return null;
				yield return null;
				ResourcesHelper.GCCollect();
			}
			this._onFinishedUnloading.Invoke();
			this.OnActionFinished();
			yield break;
		}

		private void SceneManager_sceneUnloaded(UnityEngine.SceneManagement.Scene arg0)
		{
			if (arg0.name == this._sceneName)
			{
				SceneManager.sceneUnloaded -= this.SceneManager_sceneUnloaded;
				this._loadedSceneRoot = null;
			}
		}

		public string _triggerTag;

		public string _sceneName;

		public string _animationPrefabsSceneName;

		public string _sceneObjectTag;

		public string _sceneObjectName;

		public float _loadDelay;

		public float _unloadDelay;

		public bool _canLoad;

		public bool _loadAnimPrefabs;

		public SceneLoadTrigger.Actions _forwardAction;

		public SceneLoadTrigger.Actions _backwardAction;

		public UnityEvent _onCrossingForwards;

		public UnityEvent _onCrossingBackwards;

		public UnityEvent _onBeforeLoad;

		public UnityEvent _onBeforeUnload;

		public UnityEvent _onFinishedLoading;

		public UnityEvent _onFinishedUnloading;

		private GameObject _loadedSceneRoot;

		private SceneLoadTrigger.Actions _pendingAction;

		private SceneLoadTrigger.Actions _runningAction;

		public enum Actions
		{
			None,
			Load,
			Unload,
			DelayedLoad,
			DelayedUnload,
			CrossingEventOnly
		}
	}
}
