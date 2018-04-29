﻿using System;
using UnityEngine;


public class DestroyWall : MonoBehaviour
{
	
	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Armsy"))
		{
			this.DynamicWall.SetActive(true);
			this.NormalWall.SetActive(false);
		}
	}

	
	public GameObject NormalWall;

	
	public GameObject DynamicWall;
}
