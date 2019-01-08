using System;
using UnityEngine;

namespace TheForest.Tools
{
	public class EventEmitter_ObjectFilter : MonoBehaviour
	{
		public void SendEvent()
		{
			EventRegistry.Get(this._registry).Publish(TfEvent.Get(this._event), this._filter);
		}

		public EventRegistry.Types _registry;

		public TfEvent.Types _event;

		public UnityEngine.Object _filter;
	}
}
