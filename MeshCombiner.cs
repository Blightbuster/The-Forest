using System;
using UnityEngine;


public class MeshCombiner : MonoBehaviour
{
	
	public void CombineMeshes()
	{
		Quaternion rotation = base.transform.rotation;
		Vector3 position = base.transform.position;
		base.transform.rotation = Quaternion.identity;
		base.transform.position = Vector3.zero;
		MeshFilter[] componentsInChildren = base.GetComponentsInChildren<MeshFilter>();
		Mesh mesh = new Mesh();
		CombineInstance[] array = new CombineInstance[componentsInChildren.Length];
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (!(componentsInChildren[i].transform == base.transform))
			{
				array[i].subMeshIndex = 0;
				array[i].mesh = componentsInChildren[i].sharedMesh;
				array[i].transform = componentsInChildren[i].transform.localToWorldMatrix;
			}
		}
		mesh.CombineMeshes(array);
		base.GetComponent<MeshFilter>().sharedMesh = mesh;
		base.transform.rotation = rotation;
		base.transform.position = position;
		for (int j = 0; j < base.transform.childCount; j++)
		{
			base.transform.GetChild(j).gameObject.SetActive(false);
		}
	}
}
