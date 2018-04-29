using System;
using System.Collections;
using Bolt;
using UnityEngine;
using UnityEngine.Events;

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
				switch (this._runningAction)
				{
				case SceneLoadTrigger.Actions.Load:
					base.StartCoroutine(this.StreamSceneRoutine());
					break;
				case SceneLoadTrigger.Actions.Unload:
					base.StartCoroutine(this.UnloadScene());
					break;
				case SceneLoadTrigger.Actions.CrossingEventOnly:
					this.OnActionFinished();
					break;
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
			if (!this._loadedSceneRoot)
			{
				GameObject gameObject = string.IsNullOrEmpty(this._sceneObjectName) ? GameObject.FindWithTag(this._sceneObjectTag) : GameObject.Find(this._sceneObjectName);
				if (gameObject)
				{
					this._loadedSceneRoot = gameObject.transform.root.gameObject;
				}
			}
			return this._loadedSceneRoot;
		}

		
		private IEnumerator StreamSceneRoutine()
		{
			if (!this.GetLoadedSceneRoot() && this._canLoad)
			{
				this._onBeforeLoad.Invoke();
				if (this._loadDelay > 0f)
				{
					yield return new WaitForSeconds(this._loadDelay);
				}
				AsyncOperation async = Application.LoadLevelAdditiveAsync(this._sceneName);
				yield return async;
				if (this.GetLoadedSceneRoot())
				{
					if (this._loadAnimPrefabs)
					{
						if (!CoopPeerStarter.DedicatedHost)
						{
						}
						base.StartCoroutine(this.loadEndBossScene());
					}
					this._onFinishedLoading.Invoke();
					this.OnActionFinished();
				}
				else
				{
					this._runningAction = SceneLoadTrigger.Actions.None;
					this._pendingAction = SceneLoadTrigger.Actions.Load;
					this.DoPendingAction();
				}
			}
			else
			{
				this.OnActionFinished();
			}
			yield break;
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
						AsyncOperation async = Application.LoadLevelAdditiveAsync(this._animationPrefabsSceneName);
						yield return async;
					}
					Debug.Log("client loaded end boss prefab");
					if (SteamClientDSConfig.isDedicatedClient)
					{
						loadEndGamePrefabs ev = loadEndGamePrefabs.Create(GlobalTargets.OnlyServer);
						ev.sceneName = this._animationPrefabsSceneName;
						ev.Send();
					}
				}
				else if (GameObject.FindWithTag("endGameBossPrefab") == null)
				{
					Application.LoadLevelAdditiveAsync(this._animationPrefabsSceneName);
					Debug.Log("loading animation prefabs on server");
				}
			}
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
				UnityEngine.Object.Destroy(this._loadedSceneRoot);
				this._loadedSceneRoot = null;
				yield return null;
				yield return null;
				Resources.UnloadUnusedAssets();
				yield return null;
				yield return null;
				GC.Collect();
			}
			this._onFinishedUnloading.Invoke();
			this.OnActionFinished();
			yield break;
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
