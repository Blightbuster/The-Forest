using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public struct TerrainSpawnData
{
	
	public TerrainSpawnData(GameObject newPrefab)
	{
		this.prefab = newPrefab;
		this.positions = new List<Vector3>();
		this.eulerRotations = new List<Vector3>();
	}

	
	public void AddData(GameObject entry)
	{
		this.positions.Add(entry.transform.position);
		this.eulerRotations.Add(entry.transform.rotation.eulerAngles);
	}

	
	public void Spawn(Transform parent, bool hideInEditor)
	{
		for (int i = 0; i < this.positions.Count; i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.prefab, this.positions[i], Quaternion.Euler(this.eulerRotations[i]));
			gameObject.transform.parent = parent;
			if (hideInEditor)
			{
				gameObject.hideFlags = HideFlags.HideInHierarchy;
			}
		}
	}

	
	public GameObject prefab;

	
	public List<Vector3> positions;

	
	public List<Vector3> eulerRotations;
}
