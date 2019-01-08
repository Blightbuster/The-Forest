using System;
using UnityEngine;

namespace TheForest.Utils
{
	public class Reparent : MonoBehaviour
	{
		private void OnEnable()
		{
			if (this._onEvent <= Reparent.EventTypes.Both && this._target && !Reparent.Locked)
			{
				this._target.parent = base.transform;
				this._target.localPosition = Vector3.zero;
				this._target.localRotation = Quaternion.identity;
			}
		}

		private void OnDisable()
		{
			if (this._onEvent >= Reparent.EventTypes.Both && this._target && !Reparent.Locked)
			{
				this._target.parent = base.transform;
				this._target.localPosition = Vector3.zero;
				this._target.localRotation = Quaternion.identity;
			}
		}

		public static bool Locked { get; set; }

		public Transform _target;

		public Reparent.EventTypes _onEvent;

		public enum EventTypes
		{
			OnEnable,
			Both,
			OnDisable
		}
	}
}
