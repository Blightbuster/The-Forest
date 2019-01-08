using System;
using UnityEngine;

namespace TheForest.Buildings.Creation
{
	public class DynamicBuilding : MonoBehaviour
	{
		public void LockPhysics()
		{
			Rigidbody component = base.GetComponent<Rigidbody>();
			Collider component2 = base.GetComponent<Collider>();
			this.oldPos = base.transform.position;
			this.oldRot = base.transform.rotation;
			if (component)
			{
				component.isKinematic = true;
				component.useGravity = false;
				component.detectCollisions = false;
			}
		}

		public void UnlockPhysics()
		{
			Rigidbody component = base.GetComponent<Rigidbody>();
			Collider component2 = base.GetComponent<Collider>();
			if (component)
			{
				component.isKinematic = false;
				component.useGravity = true;
				component.detectCollisions = true;
			}
			base.transform.position = this.oldPos;
			base.transform.rotation = this.oldRot;
		}

		public bool _allowParenting;

		public Transform _parentOverride;

		private Vector3 oldPos;

		private Quaternion oldRot;
	}
}
