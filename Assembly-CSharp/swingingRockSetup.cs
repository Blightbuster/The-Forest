using System;
using UnityEngine;

public class swingingRockSetup : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
	}

	private void enableSwingingRock(bool goToEnd)
	{
		Rigidbody component = base.transform.GetComponent<Rigidbody>();
		if (goToEnd)
		{
			Vector3 localPosition = new Vector3(-5.600952f, 12.14005f, 1.480346f);
			Vector3 localEulerAngles = new Vector3(-76.65817f, 98.43245f, 1.848319f);
			base.transform.localPosition = localPosition;
			base.transform.localEulerAngles = localEulerAngles;
			component.velocity = Vector3.zero;
			component.angularVelocity = Vector3.zero;
		}
		component.useGravity = true;
		component.isKinematic = false;
		foreach (Rigidbody rigidbody in base.transform.GetComponentsInChildren<Rigidbody>())
		{
			rigidbody.useGravity = true;
			rigidbody.isKinematic = false;
			if (goToEnd)
			{
				rigidbody.velocity = Vector3.zero;
				rigidbody.angularVelocity = Vector3.zero;
			}
			else
			{
				rigidbody.AddForce(Vector3.down * 20f, ForceMode.VelocityChange);
			}
		}
		this.looseRope.SetActive(false);
		this.straightRope.SetActive(true);
	}

	public GameObject looseRope;

	public GameObject straightRope;
}
