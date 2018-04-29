using System;
using UnityEngine;


public class setupPlayerAccessories : MonoBehaviour
{
	
	private void Start()
	{
	}

	
	public void doSetup()
	{
		if (this.backpackGo)
		{
			this.backpackGo.SetActive(true);
			SkinnedMeshTools.AddSkinnedMeshTo(this.backpackGo, this.rootTr);
		}
		foreach (GameObject gameObject in this.goList)
		{
			if (gameObject)
			{
				SkinnedMeshTools.AddSkinnedMeshTo(gameObject, this.rootTr);
			}
		}
	}

	
	private void cleanUp()
	{
		if (this.backpackGo)
		{
			UnityEngine.Object.Destroy(this.backpackGo);
		}
	}

	
	public GameObject backpackGo;

	
	public GameObject[] goList;

	
	public Transform rootTr;
}
