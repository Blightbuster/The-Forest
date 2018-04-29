using System;
using UnityEngine;

namespace TheForest.Utils
{
	
	public class DisableAfter : MonoBehaviour
	{
		
		private void OnEnable()
		{
			base.Invoke("DisableNow", this._delay);
		}

		
		private void OnDisable()
		{
			base.CancelInvoke("DisableNow");
		}

		
		private void DisableNow()
		{
			base.gameObject.SetActive(false);
		}

		
		public float _delay;
	}
}
