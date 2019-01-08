using System;
using UnityEngine;
using UnityEngine.Events;

namespace TheForest.Utils
{
	public class DoAfter : MonoBehaviour
	{
		public void BeginDelay()
		{
			base.Invoke("Finished", this._delay);
		}

		private void Finished()
		{
			this._callback.Invoke();
		}

		public float _delay;

		public UnityEvent _callback;
	}
}
