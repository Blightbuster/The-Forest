using System;
using System.Collections;
using UnityEngine;

namespace TheForest.Tools
{
	
	public class PreloadSerializerPrefabs : MonoBehaviour
	{
		
		private void Awake()
		{
			EventRegistry.Clear();
		}

		
		private IEnumerator Start()
		{
			yield return null;
			Resources.UnloadUnusedAssets();
			yield return null;
			GC.Collect();
			yield break;
		}
	}
}
