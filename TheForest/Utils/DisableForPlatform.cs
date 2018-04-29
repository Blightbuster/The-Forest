using System;
using UnityEngine;

namespace TheForest.Utils
{
	
	public class DisableForPlatform : MonoBehaviour
	{
		
		private void Reset()
		{
			this._target = base.gameObject;
		}

		
		private void Awake()
		{
			if (this._event == DisableForPlatform.Events.Awake)
			{
				this.CheckPlatform();
			}
		}

		
		private void Start()
		{
			if (this._event == DisableForPlatform.Events.Start)
			{
				this.CheckPlatform();
			}
		}

		
		private void OnEnable()
		{
			if (this._event == DisableForPlatform.Events.OnEnable)
			{
				this.CheckPlatform();
			}
		}

		
		private void OnDisable()
		{
			if (this._event == DisableForPlatform.Events.OnDisable)
			{
				this.CheckPlatform();
			}
		}

		
		private void OnDestroy()
		{
			if (this._event == DisableForPlatform.Events.OnDestroy)
			{
				this.CheckPlatform();
			}
		}

		
		private void CheckPlatform()
		{
			if (this._target)
			{
				this._target.SetActive(this._inverse == (this._platform == Application.platform));
			}
		}

		
		public RuntimePlatform _platform = RuntimePlatform.PS4;

		
		public DisableForPlatform.Events _event;

		
		public GameObject _target;

		
		public bool _inverse;

		
		public enum Events
		{
			
			Awake,
			
			Start,
			
			OnEnable,
			
			OnDisable,
			
			OnDestroy
		}
	}
}
