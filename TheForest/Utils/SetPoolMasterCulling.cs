using System;
using PathologicalGames;
using UnityEngine;

namespace TheForest.Utils
{
	
	public class SetPoolMasterCulling : MonoBehaviour
	{
		
		public void Set(int value)
		{
			foreach (string key in this._pools)
			{
				PoolManager.Pools[key].SetMasterCullAbove(value);
			}
		}

		
		public void Unset()
		{
			foreach (string key in this._pools)
			{
				PoolManager.Pools[key].SetMasterCullAbove(int.MaxValue);
			}
		}

		
		public string[] _pools;
	}
}
