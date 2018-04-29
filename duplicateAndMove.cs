using System;
using UnityEngine;


public class duplicateAndMove : MonoBehaviour
{
	
	private void Start()
	{
		for (int i = 0; i < this.amount; i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.tempCube, base.transform.position, base.transform.rotation);
		}
	}

	
	public GameObject tempCube;

	
	public int amount;
}
