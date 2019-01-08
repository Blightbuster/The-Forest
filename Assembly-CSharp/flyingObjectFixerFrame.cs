using System;
using System.Collections;
using UnityEngine;

public class flyingObjectFixerFrame : MonoBehaviour
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
		this.rb.drag = 30f;
		this.rb.angularDrag = 30f;
		for (int i = 0; i < this.frames; i++)
		{
			yield return YieldPresets.WaitForFixedUpdate;
		}
		this.rb.drag = this.initDrag;
		this.rb.angularDrag = this.initAngularDrag;
		this.rb.WakeUp();
		yield break;
	}

	public int frames = 4;

	private Rigidbody rb;

	private float initDrag;

	private float initAngularDrag;
}
