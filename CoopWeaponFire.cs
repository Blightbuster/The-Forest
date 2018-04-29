using System;
using Bolt;
using UnityEngine;


public class CoopWeaponFire : EntityBehaviour<IFireParticle>
{
	
	public override void Attached()
	{
		if (base.entity != null && base.entity.isAttached && base.state != null)
		{
			base.state.Transform.SetTransforms(base.transform);
		}
	}

	
	private void setFireTimeout()
	{
		if (!this.doFireTimeout && this.spawnedFireTransform)
		{
			this.spawnedFireTransform.SendMessage("removeParticlerRoutine");
			this.doFireTimeout = true;
		}
	}

	
	public Transform spawnedFireTransform;

	
	private bool doFireTimeout;
}
