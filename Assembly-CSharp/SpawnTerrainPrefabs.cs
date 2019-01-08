using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnTerrainPrefabs : MonoBehaviour
{
	private IEnumerator Start()
	{
		for (int i = 0; i < this.spawnData.Count; i++)
		{
			this.spawnData[i].Spawn(base.transform, this.hideInEditor);
			yield return null;
		}
		this.spawnData.Clear();
		yield break;
	}

	public void ProcessEntries()
	{
		List<GameObject> list = new List<GameObject>();
		for (int i = 0; i < base.transform.childCount; i++)
		{
			GameObject gameObject = base.transform.GetChild(i).gameObject;
			if (gameObject.GetComponent<BoltEntity>() == null)
			{
				list.Add(gameObject);
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			this.ProcessEntry(list[j]);
		}
	}

	public void ReverseProcess()
	{
		for (int i = 0; i < this.spawnData.Count; i++)
		{
			this.spawnData[i].Spawn(base.transform, false);
		}
		this.spawnData.Clear();
	}

	private void ProcessEntry(GameObject entry)
	{
		for (int i = 0; i < this.spawnData.Count; i++)
		{
			if (this.SamePrefab(this.spawnData[i].prefab, entry))
			{
				this.spawnData[i].AddData(entry);
				UnityEngine.Object.DestroyImmediate(entry);
				return;
			}
		}
		this.spawnData.Add(new TerrainSpawnData(entry));
	}

	private bool SamePrefab(GameObject a, GameObject b)
	{
		LOD_Base component = a.GetComponent<LOD_Base>();
		LOD_Base component2 = b.GetComponent<LOD_Base>();
		LODGroup component3 = a.GetComponent<LODGroup>();
		LODGroup component4 = b.GetComponent<LODGroup>();
		if (component != null && component2 != null)
		{
			return component.High == component2.High && component.Mid == component2.Mid && component.Low == component2.Low && component.billboard == component2.billboard;
		}
		if (component3 != null && component4 != null)
		{
			return this.CompareLODs(component3, component4);
		}
		MeshFilter[] componentsInChildren = a.GetComponentsInChildren<MeshFilter>();
		MeshFilter[] componentsInChildren2 = b.GetComponentsInChildren<MeshFilter>();
		if (componentsInChildren.Length != componentsInChildren2.Length)
		{
			return false;
		}
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].sharedMesh != componentsInChildren2[i].sharedMesh)
			{
				return false;
			}
		}
		return true;
	}

	private bool CompareLODs(LODGroup a, LODGroup b)
	{
		if (a.GetLODs() == null || b.GetLODs() == null || a.GetLODs().Length != b.GetLODs().Length)
		{
			return false;
		}
		MeshFilter[] componentsInChildren = a.GetComponentsInChildren<MeshFilter>();
		MeshFilter[] componentsInChildren2 = b.GetComponentsInChildren<MeshFilter>();
		if (componentsInChildren.Length != componentsInChildren2.Length)
		{
			return false;
		}
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].sharedMesh != componentsInChildren2[i].sharedMesh)
			{
				return false;
			}
		}
		return true;
	}

	public List<TerrainSpawnData> spawnData;

	public bool hideInEditor;
}
