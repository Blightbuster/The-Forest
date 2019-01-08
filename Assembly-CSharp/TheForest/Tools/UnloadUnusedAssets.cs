using System;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.Tools
{
	public class UnloadUnusedAssets : MonoBehaviour
	{
		public void DoUnloadUnusedAssets()
		{
			ResourcesHelper.UnloadUnusedAssets();
			if (this._gcCollect)
			{
				ResourcesHelper.GCCollect();
			}
		}

		public bool _gcCollect;
	}
}
