﻿using System;
using UnityEngine;

public class disableOnRaft : MonoBehaviour
{
	private void Start()
	{
		if (base.transform.root.GetComponent<raftOnLand>() && this.coll)
		{
			this.coll.enabled = false;
		}
	}

	public Collider coll;
}
