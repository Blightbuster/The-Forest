using System;
using UnityEngine;

namespace TheForest.Utils
{
	
	public class LerpPositionBasedOnRatio : MonoBehaviour
	{
		
		private void OnEnable()
		{
			float num = (float)Screen.width / (float)Screen.height;
			float t = (num - this._fromAspectRatio) / (this._toAspectRatio - this._fromAspectRatio);
			if (this._localPosition)
			{
				base.transform.localPosition = Vector3.Lerp(this._from.localPosition, this._to.localPosition, t);
			}
			else
			{
				base.transform.position = Vector3.Lerp(this._from.position, this._to.position, t);
			}
		}

		
		public Transform _from;

		
		public Transform _to;

		
		public float _fromAspectRatio = 1.25f;

		
		public float _toAspectRatio = 1.7f;

		
		public bool _localPosition;
	}
}
