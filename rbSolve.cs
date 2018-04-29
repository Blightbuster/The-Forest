using System;
using UnityEngine;


public class rbSolve : MonoBehaviour
{
	
	private void Start()
	{
		Rigidbody[] allComponentsInChildren = base.transform.GetAllComponentsInChildren<Rigidbody>();
		foreach (Rigidbody rigidbody in allComponentsInChildren)
		{
			rigidbody.solverIterationCount = 128;
			Debug.Log("doing count set");
		}
	}
}
