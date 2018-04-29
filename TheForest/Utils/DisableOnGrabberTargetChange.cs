using System;
using UnityEngine;

namespace TheForest.Utils
{
	
	public class DisableOnGrabberTargetChange : MonoBehaviour
	{
		
		private void OnEnable()
		{
			this._target = Grabber.FocusedItemGO;
		}

		
		private void Update()
		{
			if (this._target != Grabber.FocusedItemGO || !this._target || !Grabber.FocusedItemGO)
			{
				base.gameObject.SetActive(false);
			}
		}

		
		private GameObject _target;
	}
}
