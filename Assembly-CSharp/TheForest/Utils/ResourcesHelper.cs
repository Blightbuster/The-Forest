using System;
using UnityEngine;

namespace TheForest.Utils
{
	public static class ResourcesHelper
	{
		public static AsyncOperation UnloadUnusedAssets()
		{
			Debug.Log("<color=#f0f>UnloadUnusedAssets</color>");
			return Resources.UnloadUnusedAssets();
		}

		public static void GCCollect()
		{
			Debug.Log("<color=#f0f>System.GC.Collect</color>");
			GC.Collect();
		}
	}
}
