using System;
using UnityEngine;

namespace TheForest.Utils.Physics
{
	public interface IOnCollisionExitProxy
	{
		void OnCollisionExitProxied(Collision col);
	}
}
