using System;
using UnityEngine;

public class OVRWaitCursor : MonoBehaviour
{
	private void Update()
	{
		base.transform.Rotate(this.rotateSpeeds * Time.smoothDeltaTime);
	}

	public Vector3 rotateSpeeds = new Vector3(0f, 0f, -60f);
}
