using System;
using UnityEngine;

namespace TheForest.Utils
{
	public class ChangeColorOnHover : MonoBehaviour
	{
		private void OnDisable()
		{
			this.highlighted = false;
			this.UpdateColor();
		}

		private void OnMouseExitCollider()
		{
			if (this.highlighted)
			{
				this.highlighted = false;
				this.UpdateColor();
			}
		}

		private void OnMouseOverCollider()
		{
			if (!this.highlighted)
			{
				this.highlighted = true;
				this.UpdateColor();
			}
		}

		private void UpdateColor()
		{
			if (this.MyMatPropertyBlock == null)
			{
				this.MyMatPropertyBlock = new MaterialPropertyBlock();
			}
			this.rendererToUse.GetPropertyBlock(this.MyMatPropertyBlock);
			this.MyMatPropertyBlock.SetColor("_Color", (!this.highlighted) ? this.defaultColor : this.highlightColor);
			this.rendererToUse.SetPropertyBlock(this.MyMatPropertyBlock);
		}

		public MeshRenderer rendererToUse;

		public Color defaultColor;

		public Color highlightColor;

		private bool highlighted;

		private MaterialPropertyBlock MyMatPropertyBlock;
	}
}
