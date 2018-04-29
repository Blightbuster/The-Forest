using System;
using UnityEngine;


public class tempRagdoll : MonoBehaviour
{
	
	public void blockRagdoll()
	{
		this.rb = base.transform.GetComponentsInChildren<Rigidbody>();
		this.animator = base.GetComponent<Animator>();
		foreach (Rigidbody rigidbody in this.rb)
		{
			rigidbody.isKinematic = true;
			rigidbody.useGravity = false;
			rigidbody.Sleep();
		}
		base.Invoke("startRagdoll", 0.1f);
	}

	
	private void startRagdoll()
	{
		this.animator.enabled = false;
		foreach (Rigidbody rigidbody in this.rb)
		{
			rigidbody.isKinematic = false;
			rigidbody.useGravity = true;
			rigidbody.WakeUp();
		}
	}

	
	private Rigidbody[] rb;

	
	private Animator animator;
}
