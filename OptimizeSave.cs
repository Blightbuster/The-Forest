using System;
using UnityEngine;


public class OptimizeSave : MonoBehaviour
{
	
	private void Start()
	{
		foreach (object obj in base.transform)
		{
			Transform transform = (Transform)obj;
			transform.gameObject.SetActive(false);
		}
	}
}
