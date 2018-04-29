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
				Transform[] allTr = this.rocksGo.GetComponentsInChildren<Transform>();
				foreach (Transform tr in allTr)
				{
					if (tr.GetComponent<MeshFilter>())
					{
						MeshCollider col = null;
						if (!tr.GetComponent<MeshCollider>())
						{
							col = tr.gameObject.AddComponent<MeshCollider>();
						}
						if (col && !CoopPeerStarter.DedicatedHost)
						{
							Physics.IgnoreCollision(LocalPlayer.AnimControl.playerCollider, col, true);
							Physics.IgnoreCollision(LocalPlayer.AnimControl.playerHeadCollider, col, true);
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
