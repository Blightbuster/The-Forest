using System;
using UnityEngine;

namespace TheForest.Utils
{
	
	public class DestroyForPlatform : MonoBehaviour
	{
		
		private void Reset()
		{
			this._target = base.gameObject;
		}

		
		private void Awake()
		{
			if (this._event == DestroyForPlatform.Events.Awake)
			{
				this.CheckPlatform();
			}
		}

		
		private void Start()
		{
			if (this._event == DestroyForPlatform.Events.Start)
			{
				this.CheckPlatform();
			}
		}

		
		private void OnEnable()
		{
			if (this._event == DestroyForPlatform.Events.OnEnable)
			{
				this.CheckPlatform();
			}
		}

		
		private void OnDisable()
		{
			if (this._event == DestroyForPlatform.Events.OnDisable)
			{
				this.CheckPlatform();
			}
		}

		
		private void OnDestroy()
		{
			if (this._event == DestroyForPlatform.Events.OnDestroy)
			{
				this.CheckPlatform();
			}
		}

		
		private void CheckPlatform()
		{
			if (this._target && Application.isPlaying && this._inverse != (this._platform == Application.platform))
			{
				UnityEngine.Object.Destroy(this._target);
			}
		}

		
		public RuntimePlatform _platform = RuntimePlatform.PS4;

		
		public DestroyForPlatform.Events _event;

		
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
