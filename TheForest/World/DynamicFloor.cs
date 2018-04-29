using System;
using TheForest.Utils;
using UnityEngine;

namespace TheForest.World
{
	
	public class DynamicFloor : MonoBehaviour
	{
		
		private void Awake()
		{
			base.enabled = false;
			this._rb = base.GetComponent<Rigidbody>();
			Collider component = base.GetComponent<Collider>();
			this._extents = component.bounds.extents.magnitude;
			this._centerOffset = base.transform.InverseTransformPoint(component.bounds.center);
		}

		
		private void FixedUpdate()
		{
			if (!this.WithinExtents() || LocalPlayer.AnimControl.oarHeld.activeSelf)
			{
				base.enabled = false;
				if (this._rb)
				{
					this._rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
				}
			}
			else
			{
				Vector3 b = LocalPlayer.Transform.position - this._prevPlayerPosition;
				LocalPlayer.Transform.position = base.transform.position + base.transform.TransformDirection(this._prevLocalPlayerOffset) + b;
				this._prevPlayerPosition = LocalPlayer.Transform.position;
				this._prevLocalPlayerOffset = base.transform.InverseTransformDirection(LocalPlayer.Transform.position - base.transform.position);
			}
		}

		
		private void OnTriggerEnter(Collider other)
		{
			if (!base.enabled && other.name.Equals("UnderfootCollider") && this.WithinExtents())
			{
				base.enabled = true;
				this._prevPlayerPosition = LocalPlayer.Transform.position;
				this._prevLocalPlayerOffset = base.transform.InverseTransformDirection(LocalPlayer.Transform.position - base.transform.position);
				if (this._rb && this._rb.collisionDetectionMode != CollisionDetectionMode.ContinuousDynamic)
				{
					this._rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
				}
			}
		}

		
		private void OnTriggerExit(Collider other)
		{
			if (base.enabled && other.name.Equals("UnderfootCollider"))
			{
				base.enabled = false;
				if (this._rb && this._rb.collisionDetectionMode != CollisionDetectionMode.Discrete)
				{
					this._rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
				}
			}
		}

		
		private bool WithinExtents()
		{
			float num = this._extents;
			if (this.houseBoat)
			{
				num += 4f;
			}
			if (this.largeRaft)
			{
				num += 10f;
			}
			return Vector3.Distance(LocalPlayer.Transform.position, base.transform.position + this._centerOffset) * 1.1f < num;
		}

		
		public bool houseBoat;

		
		public bool largeRaft;

		
		private Vector3 _centerOffset;

		
		private Vector3 _prevPlayerPosition;

		
		private Vector3 _prevLocalPlayerOffset;

		
		private Rigidbody _rb;

		
		private float _extents;
	}
}
