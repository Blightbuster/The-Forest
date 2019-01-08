using System;
using System.Collections;
using TheForest.Utils;
using UnityEngine;

public class setupGreebledCollision : MonoBehaviour
{
	private void Start()
	{
		base.StartCoroutine(this.setupCollision());
	}

	private IEnumerator setupCollision()
	{
		if (BoltNetwork.isRunning && BoltNetwork.isServer)
		{
			if (!SteamDSConfig.isDedicatedServer)
			{
				while (LocalPlayer.AnimControl.playerCollider == null)
				{
					yield return null;
				}
			}
			while (!Scene.FinishGameLoad)
			{
				yield return null;
			}
			if (this.rocksGo)
			{
				Transform[] componentsInChildren = this.rocksGo.GetComponentsInChildren<Transform>();
				foreach (Transform transform in componentsInChildren)
				{
					if (transform.GetComponent<MeshFilter>())
					{
						MeshCollider meshCollider = null;
						if (!transform.GetComponent<MeshCollider>())
						{
							meshCollider = transform.gameObject.AddComponent<MeshCollider>();
						}
						if (meshCollider && !CoopPeerStarter.DedicatedHost)
						{
							Physics.IgnoreCollision(LocalPlayer.AnimControl.playerCollider, meshCollider, true);
							Physics.IgnoreCollision(LocalPlayer.AnimControl.playerHeadCollider, meshCollider, true);
						}
					}
				}
			}
		}
		yield return null;
		yield break;
	}

	public GameObject rocksGo;
}
