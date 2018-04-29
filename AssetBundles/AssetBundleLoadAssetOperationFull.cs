using System;
using UnityEngine;

namespace AssetBundles
{
	
	public class AssetBundleLoadAssetOperationFull : AssetBundleLoadAssetOperation
	{
		
		public AssetBundleLoadAssetOperationFull(string bundleName, string assetName, Type type)
		{
			this.m_AssetBundleName = bundleName;
			this.m_AssetName = assetName;
			this.m_Type = type;
		}

		
		public override T GetAsset<T>()
		{
			if (this.m_Request != null && this.m_Request.isDone)
			{
				return this.m_Request.asset as T;
			}
			return (T)((object)null);
		}

		
		public override bool Update()
		{
			if (this.m_Request != null)
			{
				return false;
			}
			LoadedAssetBundle loadedAssetBundle = AssetBundleManager.GetLoadedAssetBundle(this.m_AssetBundleName, out this.m_DownloadingError);
			if (loadedAssetBundle != null)
			{
				this.m_Request = loadedAssetBundle.m_AssetBundle.LoadAssetAsync(this.m_AssetName, this.m_Type);
				return false;
			}
			return string.IsNullOrEmpty(this.m_DownloadingError);
		}

		
		public override bool IsDone()
		{
			if (this.m_Request == null && this.m_DownloadingError != null)
			{
				Debug.LogError(this.m_DownloadingError);
				return true;
			}
			return this.m_Request != null && this.m_Request.isDone;
		}

		
		public override bool MatchBundle(string assetBundleName)
		{
			return string.Compare(this.m_AssetBundleName, assetBundleName) == 0;
		}

		
		protected string m_AssetBundleName;

		
		protected string m_AssetName;

		
		protected string m_DownloadingError;

		
		protected Type m_Type;

		
		protected AssetBundleRequest m_Request;
	}
}
