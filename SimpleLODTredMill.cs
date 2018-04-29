﻿using System;
using UnityEngine;


public class SimpleLODTredMill : MonoBehaviour
{
	
	private void Start()
	{
		this.startPosition = base.transform.position;
	}

	
	private void Update()
	{
		if (base.transform.position.z < Camera.main.transform.position.z - 10f)
		{
			base.transform.position = this.startPosition;
		}
	}

	
	private Vector3 startPosition;
}
