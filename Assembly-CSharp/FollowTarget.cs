using System;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
	private void LateUpdate()
	{
		if (this.target)
		{
			base.transform.position = this.target.position + this.offset;
			if (this.followRotation)
			{
				base.transform.rotation = this.target.rotation;
			}
		}
	}

	public Transform target;

	public Vector3 offset = new Vector3(0f, 7.5f, 0f);

	public bool followRotation;
}
