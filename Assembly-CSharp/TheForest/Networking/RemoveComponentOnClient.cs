using System;
using UnityEngine;

namespace TheForest.Networking
{
	public class RemoveComponentOnClient : MonoBehaviour
	{
		private void Awake()
		{
			if (this._awake && BoltNetwork.isClient)
			{
				UnityEngine.Object.Destroy(this._targetComponent);
			}
		}

		private void Start()
		{
			if (this._start && BoltNetwork.isClient)
			{
				UnityEngine.Object.Destroy(this._targetComponent);
			}
		}

		public bool _awake;

		public bool _start;

		public Component _targetComponent;
	}
}
