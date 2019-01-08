using System;
using UnityEngine;

namespace TheForest.Utils.Physics
{
	public class OnCollisionEnterProxy : MonoBehaviour
	{
		private void Awake()
		{
			this._clients = base.GetComponents<IOnCollisionEnterProxy>();
		}

		private void OnCollisionEnter(Collision col)
		{
			foreach (IOnCollisionEnterProxy onCollisionEnterProxy in this._clients)
			{
				if (onCollisionEnterProxy != null)
				{
					onCollisionEnterProxy.OnCollisionEnterProxied(col);
				}
			}
		}

		private void OnDestroy()
		{
			for (int i = 0; i < this._clients.Length; i++)
			{
				this._clients[i] = null;
			}
			this._clients = null;
		}

		private IOnCollisionEnterProxy[] _clients;
	}
}
