using System;
using UnityEngine;

public class transferParent : MonoBehaviour
{
	private void Awake()
	{
		if (this.newParent)
		{
			base.transform.parent = this.newParent;
		}
	}

	public Transform newParent;
}
