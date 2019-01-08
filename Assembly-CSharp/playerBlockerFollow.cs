using System;
using UnityEngine;

public class playerBlockerFollow : MonoBehaviour
{
	private void Start()
	{
		this.rb = base.transform.GetComponent<Rigidbody>();
		base.transform.parent = null;
	}

	private void FixedUpdate()
	{
		if (this.follow == null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		if (this.follow)
		{
			this.rb.MovePosition(this.follow.position);
		}
	}

	public Transform follow;

	private Rigidbody rb;
}
