﻿using System;
using UnityEngine;

public class fishPosFix : MonoBehaviour
{
	private void Start()
	{
		this.tr = base.transform;
		this.fish = base.transform.parent.GetComponent<Fish>();
	}

	private void Update()
	{
		if (!this.fish || this.fish.spearedBool)
		{
			this.tr.position = this.refPos.position;
		}
	}

	private void LateUpdate()
	{
		if (!this.fish || this.fish.spearedBool)
		{
			this.tr.position = this.refPos.position;
		}
	}

	private Fish fish;

	public Transform refPos;

	private Transform tr;
}
