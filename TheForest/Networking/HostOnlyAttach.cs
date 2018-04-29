using System;
using System.Collections;
using Bolt;
using UnityEngine;

namespace TheForest.Networking
{
	
	public class HostOnlyAttach : EntityBehaviour
	{
		
		private void Awake()
		{
			if (BoltNetwork.isServer)
			{
				base.gameObject.AddComponent<CoopAutoAttach>();
			}
		}

		
		private IEnumerator Start()
		{
			if (BoltNetwork.isClient && this._destroyOnClientIfNotAttached)
			{
				yield return null;
				if (!base.entity.isAttached)
				{
					UnityEngine.Object.Destroy(base.gameObject);
				}
			}
			yield break;
		}

		
		public bool _destroyOnClientIfNotAttached;
	}
}
