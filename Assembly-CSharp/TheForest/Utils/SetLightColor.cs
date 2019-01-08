using System;
using UnityEngine;

namespace TheForest.Utils
{
	public class SetLightColor : MonoBehaviour
	{
		public void ApplyColor()
		{
			this._light.color = this._color;
		}

		public Light _light;

		public Color _color;
	}
}
