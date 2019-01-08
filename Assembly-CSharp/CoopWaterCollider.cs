using System;
using TheForest.Utils;
using UnityEngine;

public class CoopWaterCollider : MonoBehaviour
{
	private void Start()
	{
		if (!this.WaterCollider)
		{
			this.WaterCollider = base.GetComponent<Collider>();
		}
	}

	private void Update()
	{
		if (LocalPlayer.GameObject && this.WaterCollider)
		{
			CoopPlayerColliders componentInChildren = LocalPlayer.GameObject.GetComponentInChildren<CoopPlayerColliders>();
			if (componentInChildren)
			{
				foreach (Collider collider in componentInChildren.WorldCollisionColiders)
				{
					Physics.IgnoreCollision(this.WaterCollider, collider, true);
				}
				base.enabled = false;
			}
		}
	}

	public Collider WaterCollider;
}
