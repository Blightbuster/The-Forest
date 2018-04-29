using System;
using UnityEngine;

namespace TheForest.UI
{
	
	public class EnableOnSelectProxy : MonoBehaviour
	{
		
		private void OnSelect(bool selected)
		{
			if (this._target)
			{
				this._target.enabled = selected;
			}
		}

		
		public MonoBehaviour _target;
	}
}
