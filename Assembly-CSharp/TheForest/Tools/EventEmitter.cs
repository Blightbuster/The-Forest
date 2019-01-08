using System;
using UnityEngine;

namespace TheForest.Tools
{
	public class EventEmitter : MonoBehaviour
	{
		public void SendEvent()
		{
			EventRegistry.Get(this._registry).Publish(TfEvent.Get(this._event), this);
		}

		public EventRegistry.Types _registry;

		public TfEvent.Types _event;
	}
}
