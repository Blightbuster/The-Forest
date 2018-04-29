using System;
using UnityEngine;
using UnityEngine.Events;

namespace TheForest.Utils
{
	
	public class OnDrinkWaterProxy : MonoBehaviour
	{
		
		private void OnDrinkWater()
		{
			this._callback.Invoke();
		}

		
		public UnityEvent _callback;
	}
}
