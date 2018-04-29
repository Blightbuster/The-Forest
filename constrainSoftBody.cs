using System;
using UnityEngine;


public class constrainSoftBody : MonoBehaviour
{
	
	private void Start()
	{
	}

	
	private void FixedUpdate()
	{
		base.transform.position = this.follow.position;
		base.transform.rotation = this.follow.rotation;
	}

	
	public Transform follow;
}
