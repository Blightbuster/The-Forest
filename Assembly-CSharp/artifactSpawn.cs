using System;
using TheForest.Utils;
using UnityEngine;

public class artifactSpawn : MonoBehaviour
{
	private void Start()
	{
		if ((LocalPlayer.Inventory != null && LocalPlayer.Inventory.Owns(LocalPlayer.AnimControl._artifactHeldId, true)) || BoltNetwork.isClient)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		if (CoopPeerStarter.DedicatedHost)
		{
			this.SpawnArtifact();
		}
	}

	private void Update()
	{
		if (!BoltNetwork.isClient && !CoopPeerStarter.DedicatedHost && Time.time > this.spawnTimer)
		{
			if (Scene.SceneTracker.GetClosestPlayerDistanceFromPos(base.transform.position) < 100f)
			{
				this.SpawnArtifact();
				this.spawnTimer = Time.time + 5f;
			}
			else
			{
				this.spawnTimer = Time.time + 5f;
			}
		}
	}

	private void SpawnArtifact()
	{
		UnityEngine.Object.Instantiate<GameObject>(this.artifactGo, base.transform.position, base.transform.rotation);
		base.enabled = false;
	}

	public GameObject artifactGo;

	private float spawnTimer;
}
