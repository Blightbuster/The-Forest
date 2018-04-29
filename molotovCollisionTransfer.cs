using System;
using Bolt;
using UnityEngine;


public class molotovCollisionTransfer : EntityBehaviour<IMultiThrowerProjectileState>
{
	
	private void OnCollisionEnter(Collision collision)
	{
		Molotov componentInChildren = base.transform.GetComponentInChildren<Molotov>();
		if (componentInChildren)
		{
			componentInChildren.doMolotovCollision(collision);
		}
	}
}
