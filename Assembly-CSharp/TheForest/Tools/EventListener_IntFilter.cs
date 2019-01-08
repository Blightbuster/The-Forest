using System;
using UnityEngine;
using UnityEngine.Events;

namespace TheForest.Tools
{
	public class EventListener_IntFilter : MonoBehaviour
	{
		private void Awake()
		{
			EventRegistry.Get(this._registry).Subscribe(TfEvent.Get(this._event), new EventRegistry.SubscriberCallback(this.OnEvent));
		}

		private void OnDestroy()
		{
			EventRegistry.Get(this._registry).Unsubscribe(TfEvent.Get(this._event), new EventRegistry.SubscriberCallback(this.OnEvent));
		}

		private void OnEvent(object o)
		{
			if ((int)o == this._filter)
			{
				this._onEvent.Invoke();
			}
		}

		public EventRegistry.Types _registry;

		public TfEvent.Types _event;

		public int _filter;

		public UnityEvent _onEvent;
	}
}
