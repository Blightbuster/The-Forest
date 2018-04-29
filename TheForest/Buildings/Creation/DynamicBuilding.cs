using System;
using UnityEngine;

namespace TheForest.Buildings.Creation
{
	
	public class DynamicBuilding : MonoBehaviour
	{
		
		public void LockPhysics()
		{
			Rigidbody component = base.GetComponent<Rigidbody>();
			if (component)
			{
				component.isKinematic = true;
			}
		}

		
		public void UnlockPhysics()
		{
			Rigidbody component = base.GetComponent<Rigidbody>();
			if (component)
			{
				component.isKinematic = false;
			}
		}

		
		public bool _allowParenting;

		
		public Transform _parentOverride;
	}
}
