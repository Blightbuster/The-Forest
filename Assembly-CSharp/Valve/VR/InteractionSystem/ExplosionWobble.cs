using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
	public class ExplosionWobble : MonoBehaviour
	{
		public void ExplosionEvent(Vector3 explosionPos)
		{
			Rigidbody component = base.GetComponent<Rigidbody>();
			if (component)
			{
				component.AddExplosionForce(2000f, explosionPos, 10f);
			}
		}
	}
}
