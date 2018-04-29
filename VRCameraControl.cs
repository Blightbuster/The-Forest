using System;
using UnityEngine;


public class VRCameraControl : MonoBehaviour
{
	
	private void Start()
	{
		this.smoothedCameraPos = this.VROrigin.position;
	}

	
	private void LateUpdate()
	{
	}

	
	public Transform VROrigin;

	
	private Vector3 camVelRef;

	
	private Vector3 smoothedCameraPos;
}
