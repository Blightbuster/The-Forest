using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AssetBundles
{
	public class AssetBundleManager : MonoBehaviour
	{
		public static AssetBundleManager.LogMode logMode
		{
			get
			{
				return AssetBundleManager.m_LogMode;
			}
			set
			{
				AssetBundleManager.m_LogMode = value;
			}
		}

		public static string BaseDownloadingURL
		{
			get
			{
				return AssetBundleManager.m_BaseDownloadingURL;
			}
			set
			{
				AssetBundleManager.m_BaseDownloadingURL = value;
			}
		}

		public static string[] ActiveVariants
		{
			get
			{
				return AssetBundleManager.m_ActiveVariants;
			}
			set
			{
				AssetBundleManager.m_ActiveVariants = value;
			}
		}

		public static AssetBundleManifest AssetBundleManifestObject
		{
			set
			{
				Debug.Log("setting asset bundle manifest");
				AssetBundleManager.m_AssetBundleManifest = value;
			}
		}

		private static void Log(AssetBundleManager.LogType logType, string text)
		{
			if (logType == AssetBundleManager.LogType.Error)
			{
				Debug.LogError("[AssetBundleManager] " + text);
			}
			else if (AssetBundleManager.m_LogMode == AssetBundleManager.LogMode.All)
			{
				Debug.Log("[AssetBundleManager] " + text);
			}
		}

		private void OnDestroy()
		{
			if (AssetBundleManager.m_Instance == this)
			{
				AssetBundleManager.m_Instance = null;
			}
		}

		private static string GetStreamingAssetsPath()
		{
			if (Application.isEditor)
			{
				return "file://" + Environment.CurrentDirectory.Replace("\\", "/");
			}
			if (Application.isWebPlayer)
			{
				return Path.GetDirectoryName(Application.absoluteURL).Replace("\\", "/") + "/StreamingAssets";
			}
			if (Application.isMobilePlatform || Application.isConsolePlatform)
			{
				return Application.streamingAssetsPath;
			}
			return "file://" + Application.streamingAssetsPath;
		}

		private static string GetCreateFilePath()
		{
			if (Application.isEditor)
			{
				return Environment.CurrentDirectory.Replace("\\", "/");
			}
			if (Application.isWebPlayer)
			{
				return Path.GetDirectoryName(Application.absoluteURL).Replace("\\", "/") + "/StreamingAssets";
			}
			if (Application.isMobilePlatform || Application.isConsolePlatform)
			{
				return Application.streamingAssetsPath;
			}
			return Application.streamingAssetsPath;
		}

		public static void SetSourceAssetBundleDirectory(string relativePath)
		{
			AssetBundleManager.BaseDownloadingURL = AssetBundleManager.GetStreamingAssetsPath() + relativePath;
		}

		public static void SetSourceAssetBundleURL(string absolutePath)
		{
			AssetBundleManager.BaseDownloadingURL = absolutePath;
		}

		public static LoadedAssetBundle GetLoadedAssetBundle(string assetBundleName, out string error)
		{
			if (AssetBundleManager.m_DownloadingErrors.TryGetValue(assetBundleName, out error))
			{
				return null;
			}
			LoadedAssetBundle loadedAssetBundle = null;
			AssetBundleManager.m_LoadedAssetBundles.TryGetValue(assetBundleName, out loadedAssetBundle);
			if (loadedAssetBundle == null)
			{
				return null;
			}
			string[] array = null;
			if (!AssetBundleManager.m_Dependencies.TryGetValue(assetBundleName, out array))
			{
				return loadedAssetBundle;
			}
			foreach (string key in array)
			{
				if (AssetBundleManager.m_DownloadingErrors.TryGetValue(assetBundleName, out error))
				{
					return loadedAssetBundle;
				}
				LoadedAssetBundle loadedAssetBundle2;
				AssetBundleManager.m_LoadedAssetBundles.TryGetValue(key, out loadedAssetBundle2);
				if (loadedAssetBundle2 == null)
				{
					return null;
				}
			}
			return loadedAssetBundle;
		}

		public void InitializeInstance()
		{
			AssetBundleManager.Initialize();
		}

		public static void Initialize()
		{
			if (!AssetBundleManager.m_Instance)
			{
				AssetBundleManager.SetSourceAssetBundleURL(Path.GetFullPath(Application.dataPath).Replace("\\", "/") + "/AssetBundles/");
				AssetBundleManager assetBundleManager = new GameObject("AssetBundleManager").AddComponent<AssetBundleManager>();
				UnityEngine.Object.DontDestroyOnLoad(assetBundleManager);
				AssetBundleManager.m_Instance = assetBundleManager;
				AssetBundleLoadManifestOperation assetBundleLoadManifestOperation = AssetBundleManager.Initialize(Utility.GetPlatformName());
				if (assetBundleLoadManifestOperation != null)
				{
					AssetBundleManager.m_Instance.StartCoroutine(assetBundleLoadManifestOperation);
				}
			}
		}

		public static AssetBundleLoadManifestOperation Initialize(string manifestAssetBundleName)
		{
			AssetBundleManager.LoadAssetBundle(manifestAssetBundleName, true);
			AssetBundleLoadManifestOperation assetBundleLoadManifestOperation = new AssetBundleLoadManifestOperation(manifestAssetBundleName, "AssetBundleManifest", typeof(AssetBundleManifest));
			AssetBundleManager.m_InProgressOperations.Add(assetBundleLoadManifestOperation);
			return assetBundleLoadManifestOperation;
		}

		protected static void LoadAssetBundle(string assetBundleName, bool isLoadingAssetBundleManifest = false)
		{
			if (!isLoadingAssetBundleManifest && AssetBundleManager.m_AssetBundleManifest == null)
			{
				return;
			}
			if (!AssetBundleManager.LoadAssetBundleInternal(assetBundleName, isLoadingAssetBundleManifest) && !isLoadingAssetBundleManifest)
			{
				AssetBundleManager.LoadDependencies(assetBundleName);
			}
		}

		protected static bool LoadAssetBundleInternal(string assetBundleName, bool isLoadingAssetBundleManifest)
		{
			LoadedAssetBundle loadedAssetBundle = null;
			AssetBundleManager.m_LoadedAssetBundles.TryGetValue(assetBundleName, out loadedAssetBundle);
			if (loadedAssetBundle != null)
			{
				loadedAssetBundle.m_ReferencedCount++;
				return true;
			}
			if (AssetBundleManager.m_DownloadingErrors.ContainsKey(assetBundleName))
			{
				AssetBundleManager.m_DownloadingErrors.Remove(assetBundleName);
			}
			if (AssetBundleManager.m_DownloadingReqs.ContainsKey(assetBundleName))
			{
				Dictionary<string, int> downloadingTicks;
				(downloadingTicks = AssetBundleManager.m_DownloadingTicks)[assetBundleName] = downloadingTicks[assetBundleName] + 1;
				return true;
			}
			if (AssetBundleManager.m_ToUnloadAssetBundles.TryGetValue(assetBundleName, out loadedAssetBundle))
			{
				loadedAssetBundle.m_ReferencedCount = 1;
				AssetBundleManager.m_LoadedAssetBundles.Add(assetBundleName, loadedAssetBundle);
				AssetBundleManager.m_ToUnloadAssetBundles.Remove(assetBundleName);
				return false;
			}
			AssetBundleCreateRequest value = AssetBundle.LoadFromFileAsync(AssetBundleManager.m_BaseDownloadingURL + assetBundleName);
			AssetBundleManager.m_DownloadingReqs.Add(assetBundleName, value);
			AssetBundleManager.m_DownloadingTicks.Add(assetBundleName, 1);
			return false;
		}

		protected static void LoadDependencies(string assetBundleName)
		{
			if (AssetBundleManager.m_AssetBundleManifest == null)
			{
				Debug.LogError("Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");
				return;
			}
			if (!AssetBundleManager.m_Dependencies.ContainsKey(assetBundleName))
			{
				string[] allDependencies = AssetBundleManager.m_AssetBundleManifest.GetAllDependencies(assetBundleName);
				if (allDependencies.Length == 0)
				{
					return;
				}
				AssetBundleManager.m_Dependencies.Add(assetBundleName, allDependencies);
				for (int i = 0; i < allDependencies.Length; i++)
				{
					AssetBundleManager.LoadAssetBundleInternal(allDependencies[i], false);
				}
			}
		}

		public static void UnloadAssetBundle(string assetBundleName)
		{
			AssetBundleCreateRequest assetBundleCreateRequest;
			if (AssetBundleManager.m_DownloadingReqs.TryGetValue(assetBundleName, out assetBundleCreateRequest))
			{
				Dictionary<string, int> downloadingTicks;
				if (((downloadingTicks = AssetBundleManager.m_DownloadingTicks)[assetBundleName] = downloadingTicks[assetBundleName] - 1) <= 0)
				{
					AssetBundleManager.m_DownloadingTicks[assetBundleName] = 0;
				}
			}
			else
			{
				AssetBundleManager.UnloadAssetBundleInternal(assetBundleName);
			}
		}

		protected static void UnloadDependencies(string assetBundleName)
		{
			string[] array = null;
			if (!AssetBundleManager.m_Dependencies.TryGetValue(assetBundleName, out array))
			{
				return;
			}
			foreach (string assetBundleName2 in array)
			{
				AssetBundleManager.UnloadAssetBundleInternal(assetBundleName2);
			}
			AssetBundleManager.m_Dependencies.Remove(assetBundleName);
		}

		protected static void UnloadAssetBundleInternal(string assetBundleName)
		{
			string text;
			LoadedAssetBundle loadedAssetBundle = AssetBundleManager.GetLoadedAssetBundle(assetBundleName, out text);
			if (loadedAssetBundle == null)
			{
				return;
			}
			if (--loadedAssetBundle.m_ReferencedCount == 0)
			{
				AssetBundleManager.m_LoadedAssetBundles.Remove(assetBundleName);
				if (!AssetBundleManager.m_ToUnloadAssetBundles.ContainsKey(assetBundleName))
				{
					AssetBundleManager.m_ToUnloadAssetBundles.Add(assetBundleName, loadedAssetBundle);
				}
				AssetBundleManager.UnloadDependencies(assetBundleName);
			}
		}

		private void Update()
		{
			this.keysToRemove.Clear();
			foreach (KeyValuePair<string, LoadedAssetBundle> keyValuePair in AssetBundleManager.m_ToUnloadAssetBundles)
			{
				if (keyValuePair.Value != null && ++keyValuePair.Value.m_UnloadTicks >= 60)
				{
					keyValuePair.Value.m_AssetBundle.Unload(true);
					this.keysToRemove.Add(keyValuePair.Key);
				}
			}
			for (int i = 0; i < this.keysToRemove.Count; i++)
			{
				string key = this.keysToRemove[i];
				AssetBundleManager.m_ToUnloadAssetBundles.Remove(key);
			}
			this.keysToRemove.Clear();
			foreach (KeyValuePair<string, AssetBundleCreateRequest> keyValuePair2 in AssetBundleManager.m_DownloadingReqs)
			{
				AssetBundleCreateRequest value = keyValuePair2.Value;
				if (value.isDone)
				{
					AssetBundle assetBundle = value.assetBundle;
					if (assetBundle == null)
					{
						if (!AssetBundleManager.m_DownloadingErrors.ContainsKey(keyValuePair2.Key))
						{
							AssetBundleManager.m_DownloadingErrors.Add(keyValuePair2.Key, string.Format("{0} is not a valid asset bundle.", keyValuePair2.Key));
						}
						this.keysToRemove.Add(keyValuePair2.Key);
					}
					else
					{
						int num = AssetBundleManager.m_DownloadingTicks[keyValuePair2.Key];
						if (num > 0)
						{
							AssetBundleManager.m_LoadedAssetBundles.Add(keyValuePair2.Key, new LoadedAssetBundle(assetBundle, num));
						}
						else
						{
							assetBundle.Unload(true);
							AssetBundleManager.UnloadDependencies(keyValuePair2.Key);
							for (int j = AssetBundleManager.m_InProgressOperations.Count - 1; j >= 0; j--)
							{
								if (AssetBundleManager.m_InProgressOperations[j].MatchBundle(keyValuePair2.Key))
								{
									AssetBundleManager.m_InProgressOperations[j].Abort();
									AssetBundleManager.m_InProgressOperations.RemoveAt(j);
								}
							}
						}
						this.keysToRemove.Add(keyValuePair2.Key);
					}
				}
			}
			foreach (string key2 in this.keysToRemove)
			{
				AssetBundleManager.m_DownloadingReqs.Remove(key2);
				AssetBundleManager.m_DownloadingTicks.Remove(key2);
			}
			int k = 0;
			while (k < AssetBundleManager.m_InProgressOperations.Count)
			{
				if (!AssetBundleManager.m_InProgressOperations[k].Update())
				{
					AssetBundleManager.m_InProgressOperations.RemoveAt(k);
				}
				else
				{
					k++;
				}
			}
		}

		public static bool LoadAssetAsyncCallback<T>(string assetBundleName, string assetName, Func<T, bool> onAssetLoaded) where T : UnityEngine.Object
		{
			AssetBundleManager.Initialize();
			if (AssetBundleManager.m_Instance)
			{
				if (string.IsNullOrEmpty(assetBundleName) || string.IsNullOrEmpty(assetName))
				{
					Debug.LogError("## Empty bundle or asset name: assetBundleName=" + assetBundleName + ", assetName=" + assetName);
					return false;
				}
				AssetBundleLoadAssetOperation assetBundleLoadAssetOperation = AssetBundleManager.LoadAssetAsync(assetBundleName, assetName, typeof(T));
				if (assetBundleLoadAssetOperation != null)
				{
					if (!AssetBundleManager.m_PendingRoutines.ContainsKey(onAssetLoaded))
					{
						Coroutine value = AssetBundleManager.m_Instance.StartCoroutine(AssetBundleManager.LoadAssetAsyncCallbackRoutine<T>(assetBundleName, assetBundleLoadAssetOperation, onAssetLoaded));
						AssetBundleManager.m_PendingRoutines.Add(onAssetLoaded, value);
					}
					return true;
				}
			}
			return false;
		}

		private static IEnumerator LoadAssetAsyncCallbackRoutine<T>(string assetBundleName, AssetBundleLoadAssetOperation request, Func<T, bool> onAssetLoaded) where T : UnityEngine.Object
		{
			yield return AssetBundleManager.m_Instance.StartCoroutine(request);
			if (AssetBundleManager.m_LoadedAssetBundles.ContainsKey(assetBundleName))
			{
				T asset = request.GetAsset<T>();
				if (onAssetLoaded == null || !onAssetLoaded(asset))
				{
					AssetBundleManager.UnloadAssetBundle(assetBundleName);
				}
			}
			else
			{
				onAssetLoaded((T)((object)null));
			}
			if (AssetBundleManager.m_PendingRoutines.ContainsKey(onAssetLoaded))
			{
				AssetBundleManager.m_PendingRoutines.Remove(onAssetLoaded);
			}
			yield break;
		}

		public static GameObject LoadAssetImmediate(string assetBundleName, string assetName)
		{
			string text = Path.Combine(AssetBundleManager.GetCreateFilePath(), "AssetBundles/" + assetBundleName);
			AssetBundle assetBundle = null;
			if (assetBundle == null)
			{
				Debug.LogError("## Failed to load AssetBundle: " + assetBundleName);
				return null;
			}
			return assetBundle.LoadAsset<GameObject>(assetName);
		}

		public static AssetBundleLoadAssetOperation LoadAssetAsync(string assetBundleName, string assetName, Type type)
		{
			AssetBundleManager.Initialize();
			AssetBundleManager.LoadAssetBundle(assetBundleName, false);
			AssetBundleLoadAssetOperation assetBundleLoadAssetOperation = new AssetBundleLoadAssetOperationFull(assetBundleName, assetName, type);
			AssetBundleManager.m_InProgressOperations.Add(assetBundleLoadAssetOperation);
			return assetBundleLoadAssetOperation;
		}

		public static AssetBundleLoadOperation LoadLevelAsync(string assetBundleName, string levelName, bool isAdditive)
		{
			AssetBundleManager.LoadAssetBundle(assetBundleName, false);
			AssetBundleLoadOperation assetBundleLoadOperation = new AssetBundleLoadLevelOperation(assetBundleName, levelName, isAdditive);
			AssetBundleManager.m_InProgressOperations.Add(assetBundleLoadOperation);
			return assetBundleLoadOperation;
		}

		public static bool ReadyToLoad()
		{
			return true;
		}

		private static AssetBundleManager m_Instance;

		private static AssetBundleManager.LogMode m_LogMode = AssetBundleManager.LogMode.All;

		private static string m_BaseDownloadingURL = string.Empty;

		private static string[] m_ActiveVariants = new string[0];

		private static AssetBundleManifest m_AssetBundleManifest = null;

		private static Dictionary<object, Coroutine> m_PendingRoutines = new Dictionary<object, Coroutine>();

		private static Dictionary<string, LoadedAssetBundle> m_LoadedAssetBundles = new Dictionary<string, LoadedAssetBundle>();

		private static Dictionary<string, LoadedAssetBundle> m_ToUnloadAssetBundles = new Dictionary<string, LoadedAssetBundle>();

		private static Dictionary<string, AssetBundleCreateRequest> m_DownloadingReqs = new Dictionary<string, AssetBundleCreateRequest>();

		private static Dictionary<string, int> m_DownloadingTicks = new Dictionary<string, int>();

		private static Dictionary<string, string> m_DownloadingErrors = new Dictionary<string, string>();

		private static List<AssetBundleLoadOperation> m_InProgressOperations = new List<AssetBundleLoadOperation>();

		private static Dictionary<string, string[]> m_Dependencies = new Dictionary<string, string[]>();

		private List<string> keysToRemove = new List<string>();

		public enum LogMode
		{
			All,
			JustErrors
		}

		public enum LogType
		{
			Info,
			Warning,
			Error
		}
	}
}
