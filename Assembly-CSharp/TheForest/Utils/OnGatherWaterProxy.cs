using System;
using UnityEngine;
using UnityEngine.Events;

namespace TheForest.Utils
{
	public class OnGatherWaterProxy : MonoBehaviour
	{
		private void OnGatherWater()
		{
			this._callback.Invoke();
		}

		public UnityEvent _callback;
	}
}
