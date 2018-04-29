using System;
using UnityEngine;


public class skullLampSetup : MonoBehaviour
{
	
	private void Start()
	{
		this.joint.parent = this.newParent;
	}

	
	public Transform joint;

	
	public Transform newParent;
}
