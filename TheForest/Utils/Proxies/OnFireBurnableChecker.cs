using System;
using TheForest.Interfaces;
using UnityEngine;
using UnityEngine.Events;

namespace TheForest.Utils.Proxies
{
	
	public class OnFireBurnableChecker : MonoBehaviour
	{
		
		private void OnTriggerEnter(Collider other)
		{
			IBurnable component = other.GetComponent<IBurnable>();
			if (component != null && component.IsBurning)
			{
				this._callback.Invoke();
			}
		}

		
		public UnityEvent _callback;
	}
}
