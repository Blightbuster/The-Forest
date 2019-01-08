using System;
using UnityEngine;

namespace TheForest.World
{
	public class DynamicFloorProxy : MonoBehaviour
	{
		private void Awake()
		{
			base.enabled = false;
			this._floor = base.transform.GetComponentInParent<DynamicFloor>();
			Collider[] componentsInChildren = base.transform.GetComponentsInChildren<Collider>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				this._floor.BoundColliders.Add(componentsInChildren[i]);
			}
			Collider component = base.GetComponent<Collider>();
			if (component)
			{
				this._extents = component.bounds.extents.magnitude;
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			if (other.name.Equals("UnderfootCollider"))
			{
				this._floor.ValidPlayerCount++;
				if (!this._floor.enabled)
				{
					this._floor.refreshPlayerOffset();
					this._floor.enabled = true;
				}
			}
		}

		private void OnTriggerExit(Collider other)
		{
			if (other.name.Equals("UnderfootCollider"))
			{
				this._floor.ValidPlayerCount--;
			}
		}

		public bool houseBoat;

		public bool largeRaft;

		private DynamicFloor _floor;

		private float _extents;
	}
}
