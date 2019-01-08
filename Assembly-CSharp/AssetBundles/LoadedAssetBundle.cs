using System;
using UnityEngine;

namespace AssetBundles
{
	public class LoadedAssetBundle
	{
		public LoadedAssetBundle(AssetBundle assetBundle, int refCount)
		{
			this.m_AssetBundle = assetBundle;
			this.m_ReferencedCount = refCount;
		}

		public AssetBundle m_AssetBundle;

		public int m_ReferencedCount;

		public int m_UnloadTicks;
	}
}
