﻿using System;
using UnityEngine;


public class doBurn : MonoBehaviour
{
	
	public void enableFire()
	{
		this.fire.SetActive(true);
		base.Invoke("cancelFire", UnityEngine.Random.Range(7f, 15f));
	}

	
	private void cancelFire()
	{
		this.fire.SetActive(false);
	}

	
	public GameObject fire;
}
