using System;
using Bolt;
using UnityEngine;


public class CoopDestroyPredictedGhost : GlobalEventListener
{
	
	public override void EntityAttached(BoltEntity entity)
	{
		if ((entity.StateIs<IConstructionState>() || entity.StateIs<IConstructionStateEx>()) && (this.count -= 1f) <= 0f)
		{
			UnityEngine.Object.Destroy(base.gameObject, this.delay);
		}
	}

	
	public float count;

	
	public float delay;
}
