using System;
using TheForest.Utils;
using UnityEngine;

public class setSpawnConditions : MonoBehaviour
{
	private void Awake()
	{
		this.spawn = base.GetComponent<spawnMutants>();
		base.InvokeRepeating("checkSpawnDist", 0f, UnityEngine.Random.Range(4f, 6f));
		this.spawn.enabled = false;
	}

	private void checkSpawnDist()
	{
		if (!this.spawn.creepySpawner)
		{
			this.spawn.enabled = true;
			base.CancelInvoke("checkSpawnDist");
		}
		if (LocalPlayer.IsInCaves || this.caveOverride)
		{
			this.dist = (LocalPlayer.Transform.position - base.transform.position).magnitude;
			if (this.dist < 90f)
			{
				this.spawn.enabled = true;
				this.spawn.invokeSpawn();
				base.CancelInvoke("checkSpawnDist");
			}
		}
	}

	private spawnMutants spawn;

	public float dist;

	public bool caveOverride;
}
