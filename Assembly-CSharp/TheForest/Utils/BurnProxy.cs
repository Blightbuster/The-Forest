using System;
using UnityEngine;
using UnityEngine.Events;

namespace TheForest.Utils
{
	public class BurnProxy : MonoBehaviour
	{
		private void Burn()
		{
			this._callback.Invoke();
		}

		public UnityEvent _callback;
	}
}
