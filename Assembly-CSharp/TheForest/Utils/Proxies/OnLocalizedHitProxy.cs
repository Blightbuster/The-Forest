using System;
using TheForest.World;
using UnityEngine;
using UnityEngine.Events;

namespace TheForest.Utils.Proxies
{
	public class OnLocalizedHitProxy : MonoBehaviour
	{
		public void LocalizedHit(LocalizedHitData data)
		{
			this._positionEvent.Invoke(data._position);
		}

		public OnLocalizedHitProxy.PositionEvent _positionEvent;

		[Serializable]
		public class PositionEvent : UnityEvent<Vector3>
		{
		}
	}
}
