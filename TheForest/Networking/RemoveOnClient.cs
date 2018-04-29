using System;
using Bolt;
using UnityEngine;

namespace TheForest.Networking
{
	
	public class RemoveOnClient : EntityBehaviour
	{
		
		private void Awake()
		{
			if (this._awake && BoltNetwork.isClient)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}

		
		private void Start()
		{
			if (this._start && BoltNetwork.isClient)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}

		
		public bool _awake;

		
		public bool _start;
	}
}
