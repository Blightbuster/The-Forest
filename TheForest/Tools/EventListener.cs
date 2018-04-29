using System;
using UnityEngine;
using UnityEngine.Events;

namespace TheForest.Tools
{
	
	public class EventListener : MonoBehaviour
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
			this._onEvent.Invoke();
		}

		
		public EventRegistry.Types _registry;

		
		public TfEvent.Types _event;

		
		public UnityEvent _onEvent;
	}
}
