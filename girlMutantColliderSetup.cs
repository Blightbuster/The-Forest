using System;
using UnityEngine;


public class girlMutantColliderSetup : MonoBehaviour
{
	
	private void OnEnable()
	{
		if (this.parentTarget)
		{
			base.transform.parent = this.parentTarget;
		}
	}

	
	public Transform parentTarget;
}
