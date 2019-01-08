using System;
using UnityEngine;

public class placedDeviceOnTreeCutDown : MonoBehaviour
{
	private void OnTreeCutDown(GameObject trunk)
	{
		if (trunk.GetComponent<Rigidbody>())
		{
			this.setDynamic();
		}
	}

	private void setDynamic()
	{
		BoxCollider boxCollider = base.gameObject.GetComponent<BoxCollider>();
		if (!boxCollider)
		{
			boxCollider = base.gameObject.AddComponent<BoxCollider>();
			boxCollider.center = new Vector3(0f, 0.4f, 0f);
			boxCollider.size = new Vector3(0.3f, 1.1f, 0.3f);
		}
		else
		{
			boxCollider.enabled = true;
		}
		Rigidbody rigidbody = base.gameObject.GetComponent<Rigidbody>();
		if (!rigidbody)
		{
			rigidbody = base.gameObject.AddComponent<Rigidbody>();
			rigidbody.useGravity = true;
			rigidbody.isKinematic = false;
		}
		else
		{
			rigidbody.useGravity = true;
			rigidbody.isKinematic = false;
		}
	}
}
