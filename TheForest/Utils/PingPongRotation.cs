using System;
using UnityEngine;

namespace TheForest.Utils
{
	
	public class PingPongRotation : MonoBehaviour
	{
		
		private void OnEnable()
		{
			if (this._resetOnEnable)
			{
				this._alpha = 0f;
			}
		}

		
		private void Update()
		{
			this._alpha += Time.deltaTime / this._duration;
			if (this._alpha > 1f)
			{
				this._alpha = 0f;
			}
			float t = (this._alpha >= 0.5f) ? (1f - (this._alpha - 0.5f) * 2f) : (this._alpha * 2f);
			base.transform.localEulerAngles = Vector3.Lerp(this._fromRotation, this._toRotation, t);
		}

		
		public float _duration = 1.5f;

		
		public Vector3 _fromRotation;

		
		public Vector3 _toRotation;

		
		public bool _resetOnEnable;

		
		public Space _space;

		
		private float _alpha;
	}
}
