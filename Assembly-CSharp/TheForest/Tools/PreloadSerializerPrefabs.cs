using System;
using System.Collections;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Tools
{
	public class PreloadSerializerPrefabs : MonoBehaviour
	{
		private IEnumerator Start()
		{
			yield return null;
			ResourcesHelper.UnloadUnusedAssets();
			yield return null;
			ResourcesHelper.GCCollect();
			yield break;
		}
	}
}
