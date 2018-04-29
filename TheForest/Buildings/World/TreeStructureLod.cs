using System;
using System.Collections;
using UnityEngine;

namespace TheForest.Buildings.World
{
	
	public class TreeStructureLod : MonoBehaviour
	{
		
		private void Awake()
		{
			this._cutHeightBlock = new MaterialPropertyBlock();
			this._vanillaBlock = new MaterialPropertyBlock();
		}

		
		private IEnumerator Start()
		{
			if (this._lod && this._lod.CurrentLOD >= 0)
			{
				yield return YieldPresets.WaitForEndOfFrame;
				this.LodChanged(this._lod.CurrentLOD);
			}
			yield break;
		}

		
		private void OnDisable()
		{
			this.SetLeavesCutHeight(0f);
			UnityEngine.Object.Destroy(this);
		}

		
		private void LodChanged(int currentLod)
		{
			switch (currentLod)
			{
			case 0:
				this.FetchLeavesRenderer();
				this.SetLeavesCutHeight(2.75f);
				break;
			case 1:
				this.FetchLeavesRenderer();
				this.SetLeavesCutHeight(2.75f);
				break;
			case 2:
				this.SetLeavesCutHeight(0f);
				this._renderer = null;
				break;
			}
		}

		
		private void SetLeavesCutHeight(float height)
		{
			if (this._renderer)
			{
				this._renderer.GetPropertyBlock(this._cutHeightBlock);
				this._cutHeightBlock.SetFloat("_CutHeight", height);
				this._renderer.SetPropertyBlock(this._cutHeightBlock);
			}
		}

		
		private void FetchLeavesRenderer()
		{
			if (this._renderer)
			{
				this._renderer.SetPropertyBlock(this._vanillaBlock);
			}
			TreeLeavesRenderer treeLeavesRenderer;
			if (this._lod.CurrentView)
			{
				treeLeavesRenderer = this._lod.CurrentView.GetComponent<TreeLeavesRenderer>();
			}
			else
			{
				treeLeavesRenderer = null;
			}
			if (treeLeavesRenderer)
			{
				this._renderer = treeLeavesRenderer._leavesRenderer;
				if (this._renderer)
				{
					this._renderer.GetPropertyBlock(this._vanillaBlock);
				}
			}
			else
			{
				this._renderer = null;
			}
		}

		
		public LOD_Trees _lod;

		
		private Renderer _renderer;

		
		private MaterialPropertyBlock _cutHeightBlock;

		
		private MaterialPropertyBlock _vanillaBlock;
	}
}
