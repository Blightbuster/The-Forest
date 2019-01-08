using System;
using TheForest.Buildings.Interfaces;
using UnityEngine;
using UnityEngine.Events;

namespace TheForest.Utils
{
	public class GotCleanProxy : MonoBehaviour, IWetable
	{
		public void GotClean()
		{
			this._callback.Invoke();
		}

		public UnityEvent _callback;
	}
}
