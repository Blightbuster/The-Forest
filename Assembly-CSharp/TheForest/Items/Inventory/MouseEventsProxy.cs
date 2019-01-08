using System;
using UnityEngine;
using UnityEngine.Events;

namespace TheForest.Items.Inventory
{
	public class MouseEventsProxy : MonoBehaviour
	{
		public void OnMouseEnterCollider()
		{
			this._mouseEnterEvent.Invoke();
		}

		public void OnMouseExitCollider()
		{
			this._mouseExitEvent.Invoke();
		}

		public UnityEvent _mouseEnterEvent;

		public UnityEvent _mouseExitEvent;
	}
}
