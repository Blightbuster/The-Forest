using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Rewired.UI.ControlMapper
{
	
	[AddComponentMenu("")]
	public abstract class UIElementInfo : MonoBehaviour, IEventSystemHandler, ISelectHandler
	{
		
		
		
		public event Action<GameObject> OnSelectedEvent;

		
		public void OnSelect(BaseEventData eventData)
		{
			if (this.OnSelectedEvent != null)
			{
				this.OnSelectedEvent(base.gameObject);
			}
		}

		
		public string identifier;

		
		public int intData;

		
		public Text text;
	}
}
