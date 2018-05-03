using System;
using UnityEngine;

namespace TheForest.UI
{
	
	public class UiIconSlot : MonoBehaviour
	{
		
		public void OnEnable()
		{
			this._token.Register(this, this._priority);
		}

		
		public void OnDisable()
		{
			this._token.Unregister(this);
		}

		
		public bool _priority;

		
		public GameObject _filterGo;

		
		public UiIconSlotToken _token;
	}
}
