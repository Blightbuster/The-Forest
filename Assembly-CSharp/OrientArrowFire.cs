using System;
using TheForest.Utils;
using UnityEngine;

public class OrientArrowFire : MonoBehaviour
{
	private void Start()
	{
		if (!ForestVR.Enabled)
		{
			base.enabled = false;
		}
	}

	private void Update()
	{
		base.transform.rotation = Quaternion.LookRotation(Vector3.up, -LocalPlayer.vrPlayerControl.VRCamera.forward);
	}
}
