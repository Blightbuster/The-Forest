using System;
using UnityEngine;

namespace AssetBundles
{
	
	public class AssetBundleLoadLevelOperation : AssetBundleLoadOperation
	{
		
		public AssetBundleLoadLevelOperation(string assetbundleName, string levelName, bool isAdditive)
		{
			this.m_AssetBundleName = assetbundleName;
			this.m_LevelName = levelName;
			this.m_IsAdditive = isAdditive;
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
				if (this.m_IsAdditive)
				{
					this.m_Request = Application.LoadLevelAdditiveAsync(this.m_LevelName);
				}
				else
				{
					this.m_Request = Application.LoadLevelAsync(this.m_LevelName);
				}
				return false;
			}
			return true;
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

		
		protected string m_LevelName;

		
		protected bool m_IsAdditive;

		
		protected string m_DownloadingError;

		
		protected AsyncOperation m_Request;
	}
}
