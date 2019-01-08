using System;
using UnityEngine;

namespace TheForest.Utils.Physics
{
	public class OnCollisionStayProxy : MonoBehaviour
	{
		private void Awake()
		{
			this._clients = base.GetComponents<IOnCollisionStayProxy>();
		}

		private void OnCollisionStay(Collision col)
		{
			for (int i = 0; i < this._clients.Length; i++)
			{
				if (this._clients[i] != null)
				{
					this._clients[i].OnCollisionStayProxied(col);
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

		private IOnCollisionStayProxy[] _clients;
	}
}
