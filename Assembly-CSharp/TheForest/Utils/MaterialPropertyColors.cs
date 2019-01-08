using System;
using UnityEngine;

namespace TheForest.Utils
{
	public class MaterialPropertyColors : MonoBehaviour
	{
		public void SetColor(int colorNum)
		{
			this._target.SetColor(this._colors[colorNum]);
		}

		public SetMaterialProperty _target;

		public Color[] _colors;
	}
}
