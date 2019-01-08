using System;
using System.Collections;
using AssetBundles;
using UnityEngine;

namespace TheForest.Utils.AssetBundle
{
	public class BundledPrefabLoader : MonoBehaviour
	{
		private void Awake()
		{
			if (base.GetComponent<LOD_SimpleToggle>())
			{
				base.enabled = false;
			}
		}

		private void OnEnable()
		{
			if (!AssetBundleManager.ReadyToLoad())
			{
				base.StartCoroutine(this.LoadAfterDelay());
			}
			else
			{
				this.Load();
			}
		}

		private void OnDisable()
		{
			bool flag = this._instance || this._loading;
			if (this._instance)
			{
				UnityEngine.Object.Destroy(this._instance);
				this._instance = null;
			}
			this._loading = false;
			if (flag)
			{
				AssetBundleManager.UnloadAssetBundle(this._bundleName);
			}
		}

		private void Load()
		{
			if (!this._loading)
			{
				this._loading = AssetBundleManager.LoadAssetAsyncCallback<GameObject>(this._bundleName, this._assetName, new Func<GameObject, bool>(this.OnPrefabLoaded));
			}
		}

		private IEnumerator LoadAfterDelay()
		{
			while (!AssetBundleManager.ReadyToLoad())
			{
				yield return null;
			}
			this.Load();
			yield break;
		}

		private bool OnPrefabLoaded(GameObject prefab)
		{
			if (this && this._loading)
			{
				this._loading = false;
				if (base.enabled && base.gameObject.activeSelf && prefab && base.gameObject.activeInHierarchy)
				{
					Vector3 position = base.transform.position;
					Quaternion rotation = base.transform.rotation;
					GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(prefab, position, rotation);
					this._instance = gameObject;
					if (BoltNetwork.isServer && !this._doLocalOnlyCheck && gameObject.GetComponent<BoltEntity>())
					{
						BoltNetwork.Attach(gameObject.gameObject);
					}
					gameObject.SendMessage("OnSpawned", SendMessageOptions.DontRequireReceiver);
					if (this._parent)
					{
						gameObject.transform.parent = this._parent.transform.parent;
					}
					return true;
				}
			}
			return false;
		}

		public GameObject Instance
		{
			get
			{
				return this._instance;
			}
		}

		public bool Loading
		{
			get
			{
				return this._loading;
			}
		}

		public string _bundleName;

		public string _assetName;

		public Transform _parent;

		public bool _doLocalOnlyCheck;

		private bool _loading;

		private GameObject _instance;
	}
}
