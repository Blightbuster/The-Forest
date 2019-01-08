using System;
using UnityEngine;

public class invertRotateConstrain : MonoBehaviour
{
	private void Start()
	{
		if (!ForestVR.Enabled)
		{
			base.enabled = false;
		}
	}

	private void FixedUpdate()
	{
		if (PlayerPreferences.VRForwardMovement != PlayerPreferences.VRForwardDirectionTypes.PLAYER)
		{
			Vector3 localEulerAngles = this.Target.localEulerAngles;
			localEulerAngles.y *= -1f;
			base.transform.localEulerAngles = localEulerAngles;
		}
		else
		{
			base.transform.localRotation = Quaternion.identity;
		}
	}

	public Transform Target;
}
