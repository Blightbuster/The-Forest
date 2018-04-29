using System;
using TheForest.Utils;
using UnityEngine;


public class mutantPickUp : MonoBehaviour
{
	
	private void Awake()
	{
		base.enabled = false;
	}

	
	private void GrabEnter(GameObject grabber)
	{
		base.enabled = true;
		this.Sheen.SetActive(false);
		this.MyPickUp.SetActive(true);
	}

	
	private void GrabExit(GameObject grabber)
	{
		base.enabled = false;
		this.Sheen.SetActive(true);
		this.MyPickUp.SetActive(false);
	}

	
	private void Update()
	{
		if (LocalPlayer.AnimControl.onRope || LocalPlayer.AnimControl.swimming || LocalPlayer.Inventory.Logs.HasLogs)
		{
			this.MyPickUp.SetActive(false);
			return;
		}
		if (!this.startedPickup && !this.MyPickUp.activeSelf)
		{
			this.MyPickUp.SetActive(true);
		}
		if (TheForest.Utils.Input.GetButtonAfterDelay("Take", this.delay, false))
		{
			this.doPickup();
			this.startedPickup = true;
			this.Sheen.SetActive(false);
			this.MyPickUp.SetActive(false);
		}
	}

	
	private void doPickup()
	{
		if (!LocalPlayer.AnimControl.carry)
		{
			LocalPlayer.AnimControl.setMutantPickUp(this.parentGo);
			if (this.parentGo)
			{
				this.parentGo.SendMessage("doPickupDummy", SendMessageOptions.DontRequireReceiver);
			}
			this.startedPickup = false;
			base.enabled = false;
		}
	}

	
	public GameObject Sheen;

	
	public GameObject MyPickUp;

	
	public GameObject parentGo;

	
	public float delay = 2f;

	
	private bool startedPickup;
}
