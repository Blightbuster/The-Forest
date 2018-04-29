using System;
using PathologicalGames;
using TheForest.Utils;
using UnityEngine;


public class torchLightSetup : MonoBehaviour
{
	
	private void OnEnable()
	{
		if (!Scene.SceneTracker.allVisTargets.Contains(base.gameObject))
		{
			Scene.SceneTracker.allVisTargets.Add(base.gameObject);
		}
		if (!this.col)
		{
			this.col = base.transform.GetComponent<Collider>();
		}
		base.Invoke("enableCollider", 0.15f);
		base.Invoke("turnOff", this.turnOffTime);
	}

	
	private void OnDisable()
	{
		if (Scene.SceneTracker && Scene.SceneTracker.allVisTargets.Contains(base.gameObject))
		{
			Scene.SceneTracker.allVisTargets.Remove(base.gameObject);
		}
		if (this.col)
		{
			this.col.enabled = false;
		}
		base.CancelInvoke("enableCollider");
	}

	
	private void enableCollider()
	{
		if (this.col)
		{
			this.col.enabled = true;
		}
	}

	
	private void turnOff()
	{
		if (PoolManager.Pools["misc"].IsSpawned(base.transform))
		{
			PoolManager.Pools["misc"].Despawn(base.transform);
		}
	}

	
	public float distanceToPlayer;

	
	public Vector3 sourcePos;

	
	private Collider col;

	
	public float turnOffTime;
}
