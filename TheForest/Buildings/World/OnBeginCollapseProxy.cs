using System;
using UnityEngine;

namespace TheForest.Buildings.World
{
	
	public class OnBeginCollapseProxy : MonoBehaviour
	{
		
		private void OnBeginCollapse()
		{
			if (this._target)
			{
				this._target.SendMessage("OnBeginCollapse", SendMessageOptions.DontRequireReceiver);
			}
		}

		
		public GameObject _target;
	}
}
