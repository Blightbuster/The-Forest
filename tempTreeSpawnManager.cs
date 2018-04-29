using System;
using System.Collections;
using System.Collections.Generic;
using PathologicalGames;
using UnityEngine;


public class tempTreeSpawnManager : MonoBehaviour
{
	
	private void Start()
	{
		if (this.doSpawn)
		{
			base.StartCoroutine(this.spawnTreesRoutine());
		}
	}

	
	private IEnumerator spawnTreesRoutine()
	{
		yield return new WaitForSeconds(3f);
		for (;;)
		{
			if (this.doHigh)
			{
				this.spawnAllTrees(this.treeHigh);
			}
			if (this.doMid)
			{
				this.spawnAllTrees(this.treeMid);
			}
			if (this.doLow)
			{
				this.spawnAllTrees(this.treeLow);
			}
			yield return null;
			if (this.useInstantiate)
			{
				for (int i = 0; i < this.instantiatedTrees.Count; i++)
				{
					if (this.instantiatedTrees[i])
					{
						UnityEngine.Object.Destroy(this.instantiatedTrees[i]);
					}
				}
			}
			else
			{
				PoolManager.Pools["trees"].DespawnAll();
			}
			yield return null;
		}
		yield break;
	}

	
	private void spawnAllTrees(GameObject tree)
	{
		for (int i = 0; i < this.treeCount; i++)
		{
			Vector3 vector = new Vector3((float)UnityEngine.Random.Range(-200, 200), 0f, (float)UnityEngine.Random.Range(-200, 200));
			if (this.useInstantiate)
			{
				GameObject item = UnityEngine.Object.Instantiate<GameObject>(tree, vector, Quaternion.identity);
				this.instantiatedTrees.Add(item);
			}
			else
			{
				PoolManager.Pools["trees"].Spawn(tree.transform, vector, Quaternion.identity);
			}
		}
	}

	
	public bool doSpawn;

	
	public bool useInstantiate;

	
	public bool doHigh;

	
	public bool doMid;

	
	public bool doLow;

	
	public int treeCount;

	
	public GameObject treeHigh;

	
	public GameObject treeMid;

	
	public GameObject treeLow;

	
	public List<GameObject> instantiatedTrees = new List<GameObject>();
}
