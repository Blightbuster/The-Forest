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
				this._target.SendMessage(this._message, SendMessageOptions.DontRequireReceiver);
			}
		}

		
		public GameObject _target;

		
		public string _message = "OnBeginCollapse";
	}
}
