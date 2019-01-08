using System;
using UnityEngine;

namespace TheForest.Utils
{
	public class SetMaterialProperty : MonoBehaviour
	{
		private void Awake()
		{
			this._block = new MaterialPropertyBlock();
		}

		public void SetColor(Color color)
		{
			this._renderer.GetPropertyBlock(this._block);
			this._block.SetColor(this._property, color);
			this._renderer.SetPropertyBlock(this._block);
		}

		public void SetFloat(float value)
		{
			this._renderer.GetPropertyBlock(this._block);
			this._block.SetFloat(this._property, value);
			this._renderer.SetPropertyBlock(this._block);
		}

		public void SetMatrix(Matrix4x4 value)
		{
			this._renderer.GetPropertyBlock(this._block);
			this._block.SetMatrix(this._property, value);
			this._renderer.SetPropertyBlock(this._block);
		}

		public void SetTexture(Texture value)
		{
			this._renderer.GetPropertyBlock(this._block);
			this._block.SetTexture(this._property, value);
			this._renderer.SetPropertyBlock(this._block);
		}

		public void SetVector3(Vector3 value)
		{
			this._renderer.GetPropertyBlock(this._block);
			this._block.SetVector(this._property, value);
			this._renderer.SetPropertyBlock(this._block);
		}

		public string _property;

		public Renderer _renderer;

		private MaterialPropertyBlock _block;
	}
}
