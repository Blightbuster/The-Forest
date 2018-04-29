using System;
using UnityEngine;

namespace AssetBundles
{
	
	public class AssetBundleLoadAssetOperationSimulation : AssetBundleLoadAssetOperation
	{
		
		public AssetBundleLoadAssetOperationSimulation(UnityEngine.Object simulatedObject)
		{
			this.m_SimulatedObject = simulatedObject;
		}

		
		public override T GetAsset<T>()
		{
			return this.m_SimulatedObject as T;
		}

		
		public override bool Update()
		{
			return false;
		}

		
		public override bool IsDone()
		{
			return true;
		}

		
		public override bool MatchBundle(string assetBundleName)
		{
			return false;
		}

		
		private UnityEngine.Object m_SimulatedObject;
	}
}
