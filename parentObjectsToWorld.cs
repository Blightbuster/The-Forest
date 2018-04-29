using System;
using UnityEngine;


public class parentObjectsToWorld : MonoBehaviour
{
	
	private void Start()
	{
		foreach (GameObject gameObject in this.GoList)
		{
			gameObject.transform.parent = null;
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	
	public GameObject[] GoList;
}
