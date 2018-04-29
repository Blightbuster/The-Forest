using System;
using UnityEngine;


public class clskinetify : MonoBehaviour
{
	
	public void metgodriven()
	{
		Rigidbody[] componentsInChildren = base.GetComponentsInChildren<Rigidbody>();
		foreach (Rigidbody rigidbody in componentsInChildren)
		{
			rigidbody.isKinematic = false;
			if (this.varsource != null && this.varsource.GetComponent<Rigidbody>() != null)
			{
				this.varsource.GetComponent<Rigidbody>().AddForce(Vector3.up, ForceMode.VelocityChange);
			}
		}
	}

	
	public Transform varsource;
}
