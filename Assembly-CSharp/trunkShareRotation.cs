using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trunkShareRotation : MonoBehaviour
{
	private void Start()
	{
		base.Invoke("startFollowJ1", this.followDelay);
		base.Invoke("startFollowJ2", this.followDelay + this.followDelay);
	}

	private void FixedUpdate()
	{
		this.storeQuat.Add(this.sourceJoint.localRotation);
	}

	private void startFollowJ1()
	{
		base.StartCoroutine("doFollowJ1");
	}

	private void startFollowJ2()
	{
		base.StartCoroutine("doFollowJ2");
	}

	private IEnumerator doFollowJ1()
	{
		int i = 0;
		for (;;)
		{
			this.joint1.localRotation = this.storeQuat[i];
			i++;
			yield return YieldPresets.WaitForFixedUpdate;
		}
		yield break;
	}

	private IEnumerator doFollowJ2()
	{
		int i = 0;
		for (;;)
		{
			this.joint2.localRotation = this.storeQuat[i];
			i++;
			yield return YieldPresets.WaitForFixedUpdate;
		}
		yield break;
	}

	public Transform sourceJoint;

	public Transform joint1;

	public Transform joint2;

	public float joint1Blend;

	public float joint2Blend;

	public float followDelay;

	public List<Quaternion> storeQuat = new List<Quaternion>();
}
