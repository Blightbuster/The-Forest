﻿using System;
using UnityEngine;

public class FTEM_VisibleBG : MonoBehaviour
{
	private void Start()
	{
	}

	private void OnMouseDown()
	{
		if (this.myCheck)
		{
			this.BG.SetActive(false);
			this.myCheck = false;
			return;
		}
		if (!this.myCheck)
		{
			this.BG.SetActive(true);
			this.myCheck = true;
			return;
		}
	}

	public bool myCheck = true;

	public GameObject BG;
}
