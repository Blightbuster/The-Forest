using System;
using TheForest.Utils;
using UnityEngine;


public class artifactSpawn : MonoBehaviour
{
	
	private void Start()
	{
		if (LocalPlayer.Inventory != null && LocalPlayer.Inventory.Owns(LocalPlayer.AnimControl._artifactHeldId, true))
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		if (!BoltNetwork.isClient)
		{
			this.SpawnArtifact();
		}
	}

	
	private void SpawnArtifact()
	{
		UnityEngine.Object.Instantiate<GameObject>(this.artifactGo, base.transform.position, base.transform.rotation);
		base.enabled = false;
	}

	
	public GameObject artifactGo;
}
