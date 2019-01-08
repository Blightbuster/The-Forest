using System;
using DigitalOpus.MB.Core;
using UnityEngine;

public class MB3_TestBakeAllWithSameMaterial : MonoBehaviour
{
	private void Start()
	{
		this.testCombine();
	}

	private void testCombine()
	{
		MB3_MeshCombinerSingle mb3_MeshCombinerSingle = new MB3_MeshCombinerSingle();
		Debug.Log("About to bake 1");
		mb3_MeshCombinerSingle.AddDeleteGameObjects(this.listOfObjsToCombineGood, null, true);
		mb3_MeshCombinerSingle.Apply();
		mb3_MeshCombinerSingle.UpdateGameObjects(this.listOfObjsToCombineGood, true, true, true, true, false, false, false, false, false, false);
		mb3_MeshCombinerSingle.Apply();
		mb3_MeshCombinerSingle.AddDeleteGameObjects(null, this.listOfObjsToCombineGood, true);
		mb3_MeshCombinerSingle.Apply();
		Debug.Log("Did bake 1");
		Debug.Log("About to bake 2");
		mb3_MeshCombinerSingle.AddDeleteGameObjects(this.listOfObjsToCombineBad, null, true);
		mb3_MeshCombinerSingle.Apply();
		Debug.Log("Did bake 2");
		Debug.Log("Doing same with multi mesh combiner");
		MB3_MultiMeshCombiner mb3_MultiMeshCombiner = new MB3_MultiMeshCombiner();
		Debug.Log("About to bake 3");
		mb3_MultiMeshCombiner.AddDeleteGameObjects(this.listOfObjsToCombineGood, null, true);
		mb3_MultiMeshCombiner.Apply();
		mb3_MultiMeshCombiner.UpdateGameObjects(this.listOfObjsToCombineGood, true, true, true, true, false, false, false, false, false, false);
		mb3_MultiMeshCombiner.Apply();
		mb3_MultiMeshCombiner.AddDeleteGameObjects(null, this.listOfObjsToCombineGood, true);
		mb3_MultiMeshCombiner.Apply();
		Debug.Log("Did bake 3");
		Debug.Log("About to bake 4");
		mb3_MultiMeshCombiner.AddDeleteGameObjects(this.listOfObjsToCombineBad, null, true);
		mb3_MultiMeshCombiner.Apply();
		Debug.Log("Did bake 4");
	}

	public GameObject[] listOfObjsToCombineGood;

	public GameObject[] listOfObjsToCombineBad;
}
