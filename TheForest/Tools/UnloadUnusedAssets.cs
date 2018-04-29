using System;
using UnityEngine;

namespace TheForest.Tools
{
	
	public class UnloadUnusedAssets : MonoBehaviour
	{
		
		public void DoUnloadUnusedAssets()
		{
			Resources.UnloadUnusedAssets();
			if (this._gcCollect)
			{
				GC.Collect();
			}
		}

		
		public bool _gcCollect;
	}
}
