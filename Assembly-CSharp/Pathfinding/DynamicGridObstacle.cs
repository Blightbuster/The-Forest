using System;
using UnityEngine;

namespace Pathfinding
{
	[RequireComponent(typeof(Collider))]
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_dynamic_grid_obstacle.php")]
	public class DynamicGridObstacle : MonoBehaviour
	{
		private void Start()
		{
			this.col = base.GetComponent<Collider>();
			this.tr = base.transform;
			if (this.col == null)
			{
				throw new Exception("A collider must be attached to the GameObject for the DynamicGridObstacle to work");
			}
			this.prevBounds = this.col.bounds;
			this.prevEnabled = this.col.enabled;
			this.prevRotation = this.tr.rotation;
		}

		private void Update()
		{
			if (!this.col)
			{
				Debug.LogError("Removed collider from DynamicGridObstacle", this);
				base.enabled = false;
				return;
			}
			while (AstarPath.active == null || AstarPath.active.isScanning)
			{
				this.lastCheckTime = Time.realtimeSinceStartup;
			}
			if (Time.realtimeSinceStartup - this.lastCheckTime < this.checkTime)
			{
				return;
			}
			if (this.col.enabled)
			{
				Bounds bounds = this.col.bounds;
				Quaternion rotation = this.tr.rotation;
				Vector3 vector = this.prevBounds.min - bounds.min;
				Vector3 vector2 = this.prevBounds.max - bounds.max;
				float magnitude = bounds.extents.magnitude;
				float num = magnitude * Quaternion.Angle(this.prevRotation, rotation) * 0.0174532924f;
				if (vector.sqrMagnitude > this.updateError * this.updateError || vector2.sqrMagnitude > this.updateError * this.updateError || num > this.updateError || !this.prevEnabled)
				{
					this.DoUpdateGraphs();
				}
			}
			else if (this.prevEnabled)
			{
				this.DoUpdateGraphs();
			}
		}

		private void OnDestroy()
		{
			if (AstarPath.active != null)
			{
				GraphUpdateObject ob = new GraphUpdateObject(this.prevBounds);
				AstarPath.active.UpdateGraphs(ob);
			}
		}

		public void DoUpdateGraphs()
		{
			if (this.col == null)
			{
				return;
			}
			if (!this.col.enabled)
			{
				AstarPath.active.UpdateGraphs(this.prevBounds);
			}
			else
			{
				Bounds bounds = this.col.bounds;
				Bounds bounds2 = bounds;
				bounds2.Encapsulate(this.prevBounds);
				if (DynamicGridObstacle.BoundsVolume(bounds2) < DynamicGridObstacle.BoundsVolume(bounds) + DynamicGridObstacle.BoundsVolume(this.prevBounds))
				{
					AstarPath.active.UpdateGraphs(bounds2);
				}
				else
				{
					AstarPath.active.UpdateGraphs(this.prevBounds);
					AstarPath.active.UpdateGraphs(bounds);
				}
				this.prevBounds = bounds;
			}
			this.prevEnabled = this.col.enabled;
			this.prevRotation = this.tr.rotation;
			this.lastCheckTime = Time.realtimeSinceStartup;
		}

		private static float BoundsVolume(Bounds b)
		{
			return Math.Abs(b.size.x * b.size.y * b.size.z);
		}

		private Collider col;

		private Transform tr;

		public float updateError = 1f;

		public float checkTime = 0.2f;

		private Bounds prevBounds;

		private Quaternion prevRotation;

		private bool prevEnabled;

		private float lastCheckTime = -9999f;
	}
}
