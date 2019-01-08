using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheForest.Utils
{
	public static class SceneHelper
	{
		public static IEnumerator LoadScene(string levelToLoad, bool allowAsync = false)
		{
			if (allowAsync)
			{
				yield return SceneManager.LoadSceneAsync(levelToLoad);
			}
			else
			{
				yield return YieldPresets.WaitForEndOfFrame;
				SceneManager.LoadScene(levelToLoad);
			}
			yield break;
		}

		public static IEnumerator LoadAssetAsync(string assetName, Action<UnityEngine.Object> result)
		{
			UnityEngine.Object asset;
			if (PlayerPreferences.AllowAsync)
			{
				ResourceRequest req = Resources.LoadAsync(assetName);
				while (!req.isDone)
				{
					yield return null;
				}
				asset = req.asset;
			}
			else
			{
				asset = Resources.Load(assetName);
			}
			result(asset);
			yield break;
		}
	}
}
