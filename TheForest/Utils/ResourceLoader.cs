using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheForest.Utils
{
	
	public class ResourceLoader : MonoBehaviour
	{
		
		private void OnEnable()
		{
			if (ResourceLoader.InUseAssetsCounters.ContainsKey(this._assetPath))
			{
				Dictionary<string, int> inUseAssetsCounters;
				Dictionary<string, int> dictionary = inUseAssetsCounters = ResourceLoader.InUseAssetsCounters;
				string assetPath;
				string key = assetPath = this._assetPath;
				int num = inUseAssetsCounters[assetPath];
				dictionary[key] = num + 1;
			}
			else
			{
				ResourceLoader.InUseAssetsCounters[this._assetPath] = 1;
			}
			this.AssetLoad();
		}

		
		private void OnDisable()
		{
			bool resourceUnload = false;
			Dictionary<string, int> inUseAssetsCounters;
			Dictionary<string, int> dictionary = inUseAssetsCounters = ResourceLoader.InUseAssetsCounters;
			string assetPath;
			string key = assetPath = this._assetPath;
			int num = inUseAssetsCounters[assetPath];
			if ((dictionary[key] = num - 1) == 0)
			{
				ResourceLoader.InUseAssetsCounters.Remove(this._assetPath);
				resourceUnload = true;
			}
			this.AssetUnload(resourceUnload);
		}

		
		public void AssetLoad()
		{
			this._asset = Resources.Load(this._assetPath);
			ResourceLoader.AssetTypes type = this._type;
			if (type != ResourceLoader.AssetTypes.Mesh)
			{
				if (type == ResourceLoader.AssetTypes.Texture)
				{
					((Material)this._target).mainTexture = (Texture)this._asset;
				}
			}
			else
			{
				((MeshFilter)this._target).mesh = (Mesh)this._asset;
			}
		}

		
		public void AssetUnload(bool resourceUnload)
		{
			ResourceLoader.AssetTypes type = this._type;
			if (type != ResourceLoader.AssetTypes.Mesh)
			{
				if (type == ResourceLoader.AssetTypes.Texture)
				{
					((Material)this._target).mainTexture = null;
				}
			}
			else
			{
				((MeshFilter)this._target).mesh = null;
				if (resourceUnload)
				{
					Resources.UnloadAsset(this._asset);
				}
			}
			this._asset = null;
		}

		
		public ResourceLoader.AssetTypes _type;

		
		public string _assetPath;

		
		public UnityEngine.Object _target;

		
		private UnityEngine.Object _asset;

		
		private static Dictionary<string, int> InUseAssetsCounters = new Dictionary<string, int>();

		
		public enum AssetTypes
		{
			
			Mesh,
			
			Texture
		}
	}
}
