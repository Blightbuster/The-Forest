using System;
using System.Collections.Generic;
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
			if (this.FollowTarget == null)
			{
				this.FollowTarget = base.transform;
			}
			this.BoundColliders.Add(base.transform.GetComponent<Collider>());
			this._thisCollider = base.transform.GetComponent<Collider>();
		}

		public void refreshPlayerOffset()
		{
			this._prevPlayerPosition = LocalPlayer.Transform.position;
			this._prevLocalPlayerOffset = base.transform.InverseTransformDirection(LocalPlayer.Transform.position - this.FollowTarget.position);
		}

		private void FixedUpdate()
		{
			this.UpdatePlayerPosition();
			if (Time.time > this._interectTimer && !this.Crane)
			{
				if (!this.validateIntersectingColliders())
				{
					this.ShutDown();
				}
				this._interectTimer = Time.time + 0.5f;
			}
		}

		public void UpdatePlayerPosition()
		{
			if ((!this.WithinExtents() && this.ValidPlayerCount < 1) || LocalPlayer.AnimControl.oarHeld.activeSelf)
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
				LocalPlayer.Transform.position = this.FollowTarget.position + this.FollowTarget.TransformDirection(this._prevLocalPlayerOffset) + b;
				this._prevPlayerPosition = LocalPlayer.Transform.position;
				this._prevLocalPlayerOffset = this.FollowTarget.InverseTransformDirection(LocalPlayer.Transform.position - this.FollowTarget.position);
			}
		}

		private void OnCollisionEnter(Collision col)
		{
			this.IgnoreParentedStructureCollision(col);
		}

		private void OnTriggerEnter(Collider other)
		{
			if (!base.enabled && other.name.Equals("UnderfootCollider") && this.WithinExtents())
			{
				if (this.ValidPlayerCount < 0)
				{
					this.ValidPlayerCount = 0;
				}
				base.enabled = true;
				this._interectTimer = Time.time + 0.5f;
				this._prevPlayerPosition = LocalPlayer.Transform.position;
				this._prevLocalPlayerOffset = this.FollowTarget.InverseTransformDirection(LocalPlayer.Transform.position - this.FollowTarget.position);
				if (this._rb && this._rb.collisionDetectionMode != CollisionDetectionMode.ContinuousDynamic)
				{
					this._rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
				}
			}
		}

		private void OnTriggerExit(Collider other)
		{
			if (base.enabled && other.name.Equals("UnderfootCollider") && this.ValidPlayerCount < 1)
			{
				base.enabled = false;
				if (this._rb && this._rb.collisionDetectionMode != CollisionDetectionMode.Discrete)
				{
					this._rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
				}
			}
		}

		private void IgnoreParentedStructureCollision(Collision col)
		{
			if (col.transform.root == base.transform && !col.gameObject.CompareTag("Player"))
			{
				Physics.IgnoreCollision(this._thisCollider, col.collider);
			}
		}

		private void ShutDown()
		{
			base.enabled = false;
			this.ValidPlayerCount = 0;
			if (this._rb && this._rb.collisionDetectionMode != CollisionDetectionMode.Discrete)
			{
				this._rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
			}
		}

		private bool validateIntersectingColliders()
		{
			for (int i = 0; i < this.BoundColliders.Count; i++)
			{
				if (this.BoundColliders[i] != null && this.BoundColliders[i].bounds.Intersects(LocalPlayer.AnimControl.playerCollider.bounds))
				{
					return true;
				}
			}
			return false;
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

		public Transform FollowTarget;

		public bool houseBoat;

		public bool largeRaft;

		public bool Crane;

		public int ValidPlayerCount;

		public List<Collider> BoundColliders = new List<Collider>();

		private Collider _thisCollider;

		private Vector3 _centerOffset;

		private Vector3 _prevPlayerPosition;

		private Vector3 _prevLocalPlayerOffset;

		private Rigidbody _rb;

		private float _extents;

		private float _interectTimer;
	}
}
