using System;
using System.Collections;
using TheForest.Utils;
using UnityEngine;


public class nitrogenTankLoader : MonoBehaviour
{
	
	private void Start()
	{
		if (this.master && (!BoltNetwork.isRunning || (BoltNetwork.isRunning && BoltNetwork.isServer)))
		{
			base.StartCoroutine("loadTankRoutine");
		}
	}

	
	public void loadTank()
	{
		if (!this.tankGo)
		{
			return;
		}
		if (!this.spawnedTank)
		{
			this.spawnedTank = UnityEngine.Object.Instantiate<GameObject>(this.tankGo, base.transform.position, base.transform.rotation);
			base.enabled = false;
		}
	}

	
	private IEnumerator loadTankRoutine()
	{
		nitrogenTankLoader[] loaders = base.transform.GetComponentsInChildren<nitrogenTankLoader>();
		for (;;)
		{
			for (int i = 0; i < loaders.Length; i++)
			{
				if (loaders[i] && loaders[i].enabled && Scene.SceneTracker.GetClosestPlayerDistanceFromPos(loaders[i].transform.position) < 85f)
				{
					loaders[i].loadTank();
				}
				yield return YieldPresets.WaitForFixedUpdate;
				yield return YieldPresets.WaitForFixedUpdate;
				yield return YieldPresets.WaitForFixedUpdate;
			}
		}
		yield break;
	}

	
	private void OnDestroy()
	{
		if (this.spawnedTank)
		{
			UnityEngine.Object.Destroy(this.spawnedTank);
		}
	}

	
	public GameObject tankGo;

	
	private GameObject spawnedTank;

	
	public bool master;
}
