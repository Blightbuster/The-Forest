using System;
using System.Collections;
using UnityEngine;

namespace TheForest.Utils.Physics
{
	public class OnCollisionExitProxy : MonoBehaviour
	{
		private void Awake()
		{
			this._clients = base.GetComponents<IOnCollisionExitProxy>();
		}

		private void OnCollisionExit(Collision col)
		{
			foreach (IOnCollisionExitProxy onCollisionExitProxy in this._clients)
			{
				if (onCollisionExitProxy != null)
				{
					onCollisionExitProxy.OnCollisionExitProxied(col);
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

		public T[] ConvertToArray<T>(IList list)
		{
			T[] array = new T[list.Count];
			list.CopyTo(array, 0);
			return array;
		}

		private IOnCollisionExitProxy[] _clients;
	}
}
