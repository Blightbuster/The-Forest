using System;
using AssetBundles;
using UnityEngine;

namespace TheForest.Utils.AssetBundle
{
	public class BundledAssetLoader : MonoBehaviour
	{
		private void OnEnable()
		{
			this.AssetLoad();
		}

		private void OnDisable()
		{
			this.AssetUnload();
		}

		public void AssetLoad()
		{
			if (!this._loading)
			{
				switch (this._type)
				{
				case BundledAssetLoader.AssetTypes.Mesh:
				case BundledAssetLoader.AssetTypes.SkinnedMesh:
				case BundledAssetLoader.AssetTypes.SwitcherLod0:
				case BundledAssetLoader.AssetTypes.SwitcherLod1:
				case BundledAssetLoader.AssetTypes.SwitcherLod2:
					this._loading = AssetBundleManager.LoadAssetAsyncCallback<Mesh>(this._bundleName, this._assetName, new Func<Mesh, bool>(this.OnMeshLoaded));
					break;
				case BundledAssetLoader.AssetTypes.Texture:
					this._loading = AssetBundleManager.LoadAssetAsyncCallback<Texture>(this._bundleName, this._assetName, new Func<Texture, bool>(this.OnTextureLoaded));
					break;
				case BundledAssetLoader.AssetTypes.Material:
					this._loading = AssetBundleManager.LoadAssetAsyncCallback<Material>(this._bundleName, this._assetName, new Func<Material, bool>(this.OnMaterialLoaded));
					break;
				}
			}
		}

		private bool OnMeshLoaded(Mesh asset)
		{
			if (this && this._loading)
			{
				this._loading = false;
				if (base.enabled && base.gameObject.activeSelf && asset)
				{
					if (this._type == BundledAssetLoader.AssetTypes.Mesh)
					{
						((MeshFilter)this._target).sharedMesh = asset;
						base.GetComponent<Renderer>().enabled = true;
						return true;
					}
					if (this._type == BundledAssetLoader.AssetTypes.SkinnedMesh)
					{
						SkinnedMeshRenderer skinnedMeshRenderer = (SkinnedMeshRenderer)this._target;
						skinnedMeshRenderer.sharedMesh = asset;
						skinnedMeshRenderer.enabled = skinnedMeshRenderer.sharedMaterial;
						return true;
					}
					if (this._type == BundledAssetLoader.AssetTypes.SwitcherLod0)
					{
						playerMeshSwitcher playerMeshSwitcher = (playerMeshSwitcher)this._target;
						playerMeshSwitcher.Skin.sharedMesh = asset;
						playerMeshSwitcher.Skin.enabled = true;
						playerMeshSwitcher.Lod0 = asset;
						playerMeshSwitcher.enabled = true;
						return true;
					}
					if (this._type == BundledAssetLoader.AssetTypes.SwitcherLod1)
					{
						playerMeshSwitcher playerMeshSwitcher2 = (playerMeshSwitcher)this._target;
						playerMeshSwitcher2.Lod1 = asset;
						playerMeshSwitcher2.enabled = true;
						return true;
					}
					if (this._type == BundledAssetLoader.AssetTypes.SwitcherLod2)
					{
						playerMeshSwitcher playerMeshSwitcher3 = (playerMeshSwitcher)this._target;
						playerMeshSwitcher3.Lod2 = asset;
						playerMeshSwitcher3.enabled = true;
						return true;
					}
				}
			}
			return false;
		}

		private bool OnTextureLoaded(Texture asset)
		{
			if (this && this._loading)
			{
				this._loading = false;
				if (base.enabled && base.gameObject.activeSelf && asset)
				{
					((Material)this._target).mainTexture = asset;
					return true;
				}
			}
			return false;
		}

		private bool OnMaterialLoaded(Material asset)
		{
			if (this && this._loading)
			{
				this._loading = false;
				if (base.enabled && base.gameObject.activeSelf && asset)
				{
					Renderer renderer = (Renderer)this._target;
					renderer.sharedMaterial = asset;
					renderer.enabled = true;
					return true;
				}
			}
			return false;
		}

		public void AssetUnload()
		{
			if (!this._loading)
			{
				switch (this._type)
				{
				case BundledAssetLoader.AssetTypes.Mesh:
					((MeshFilter)this._target).mesh = null;
					base.GetComponent<Renderer>().enabled = false;
					break;
				case BundledAssetLoader.AssetTypes.SkinnedMesh:
				{
					SkinnedMeshRenderer skinnedMeshRenderer = (SkinnedMeshRenderer)this._target;
					skinnedMeshRenderer.sharedMesh = null;
					skinnedMeshRenderer.enabled = false;
					break;
				}
				case BundledAssetLoader.AssetTypes.Texture:
					((Material)this._target).mainTexture = null;
					base.GetComponent<Renderer>().enabled = false;
					break;
				case BundledAssetLoader.AssetTypes.Material:
				{
					Renderer renderer = (Renderer)this._target;
					renderer.sharedMaterial = null;
					renderer.enabled = false;
					break;
				}
				case BundledAssetLoader.AssetTypes.SwitcherLod0:
				{
					playerMeshSwitcher playerMeshSwitcher = (playerMeshSwitcher)this._target;
					playerMeshSwitcher.Lod0 = null;
					playerMeshSwitcher.enabled = false;
					playerMeshSwitcher.Skin.sharedMesh = null;
					playerMeshSwitcher.Skin.enabled = false;
					break;
				}
				case BundledAssetLoader.AssetTypes.SwitcherLod1:
				{
					playerMeshSwitcher playerMeshSwitcher2 = (playerMeshSwitcher)this._target;
					playerMeshSwitcher2.Lod1 = null;
					playerMeshSwitcher2.enabled = false;
					playerMeshSwitcher2.Skin.sharedMesh = null;
					playerMeshSwitcher2.Skin.enabled = false;
					break;
				}
				case BundledAssetLoader.AssetTypes.SwitcherLod2:
				{
					playerMeshSwitcher playerMeshSwitcher3 = (playerMeshSwitcher)this._target;
					playerMeshSwitcher3.Lod2 = null;
					playerMeshSwitcher3.enabled = false;
					playerMeshSwitcher3.Skin.sharedMesh = null;
					playerMeshSwitcher3.Skin.enabled = false;
					break;
				}
				}
				AssetBundleManager.UnloadAssetBundle(this._bundleName);
			}
		}

		public BundledAssetLoader.AssetTypes _type;

		public string _bundleName;

		public string _assetName;

		public UnityEngine.Object _target;

		private bool _loading;

		public enum AssetTypes
		{
			Mesh,
			SkinnedMesh,
			Texture,
			Material,
			SwitcherLod0,
			SwitcherLod1,
			SwitcherLod2
		}
	}
}
