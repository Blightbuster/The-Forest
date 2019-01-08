using System;
using UnityEngine;

namespace TheForest.UI
{
	public class EnableOnHoverProxy : MonoBehaviour
	{
		private void OnHover(bool selected)
		{
			if (this._target)
			{
				this._target.enabled = selected;
			}
		}

		public MonoBehaviour _target;
	}
}
