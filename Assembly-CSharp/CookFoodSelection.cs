﻿using System;
using UnityEngine;

public class CookFoodSelection : MonoBehaviour
{
	private void OnDisable()
	{
		this.GrabExit();
	}

	private void GrabEnter()
	{
		this.Selected = true;
		this.Sheen.SetActive(false);
		this.MyPickUp.SetActive(true);
	}

	private void GrabExit()
	{
		this.Selected = false;
		this.Sheen.SetActive(true);
		this.MyPickUp.SetActive(false);
	}

	public bool Selected { get; set; }

	public GameObject Sheen;

	public GameObject MyPickUp;
}
