using System;
using UnityEngine;


public class duplicateAndMove : MonoBehaviour
{
	
	private void Start()
	{
		for (int i = 0; i < this.amount; i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(this.tempCube, base.transform.position, base.transform.rotation) as GameObject;
		}
	}

	
	public GameObject tempCube;

	
	public int amount;
}
