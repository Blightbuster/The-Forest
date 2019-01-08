using System;
using UnityEngine;

namespace TheForest.Utils.Physics
{
	public interface IOnCollisionEnterProxy
	{
		void OnCollisionEnterProxied(Collision col);
	}
}
