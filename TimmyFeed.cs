﻿using System;
using TheForest.Items;
using TheForest.Utils;
using UnityEngine;


public class TimmyFeed : MonoBehaviour
{
	
	private void Awake()
	{
		base.enabled = false;
	}

	
	private void GrabEnter()
	{
		base.enabled = true;
	}

	
	private void GrabExit()
	{
		base.enabled = false;
	}

	
	private void Update()
	{
		if (LocalPlayer.Inventory.Owns(this._itemid, true))
		{
			this.AddIcon.SetActive(true);
			if (TheForest.Utils.Input.GetButtonDown("Craft") && LocalPlayer.Inventory.RemoveItem(this._itemid, 1, false, true))
			{
				this.Stats.Fed();
			}
		}
		else
		{
			this.AddIcon.SetActive(false);
		}
	}

	
	[ItemIdPicker]
	public int _itemid;

	
	public GameObject AddIcon;

	
	public TimmyStats Stats;

	
	private int Items;
}
