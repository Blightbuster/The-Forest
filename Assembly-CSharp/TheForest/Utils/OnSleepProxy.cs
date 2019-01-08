using System;
using UnityEngine;
using UnityEngine.Events;

namespace TheForest.Utils
{
	public class OnSleepProxy : MonoBehaviour
	{
		private void OnSleep()
		{
			this._callback.Invoke();
		}

		public UnityEvent _callback;
	}
}
