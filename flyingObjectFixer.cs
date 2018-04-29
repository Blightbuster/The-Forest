using System;
using System.Collections;
using UnityEngine;


public class flyingObjectFixer : MonoBehaviour
{
	
	private void OnEnable()
	{
		if (!this.rb)
		{
			this.rb = base.transform.GetComponent<Rigidbody>();
		}
		if (this.rb)
		{
			this.initDrag = this.rb.drag;
			this.initAngularDrag = this.rb.angularDrag;
			base.StartCoroutine(this.dampRigidBody());
		}
	}

	
	private void OnDisable()
	{
		base.StopAllCoroutines();
		if (this.rb)
		{
			this.rb.drag = this.initDrag;
			this.rb.angularDrag = this.initAngularDrag;
		}
	}

	
	private IEnumerator dampRigidBody()
	{
		this.rb.drag = 20f;
		this.rb.angularDrag = 20f;
		yield return YieldPresets.WaitTwoSeconds;
		this.rb.drag = this.initDrag;
		this.rb.angularDrag = this.initAngularDrag;
		yield break;
	}

	
	private Rigidbody rb;

	
	private float initDrag;

	
	private float initAngularDrag;
}
