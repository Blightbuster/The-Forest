using System;
using Bolt;
using UnityEngine;


public class FlareWeapon : EntityBehaviour
{
	
	private void Awake()
	{
		if (BoltNetwork.isRunning && base.entity.isAttached && !base.entity.isOwner)
		{
			base.enabled = false;
		}
		else
		{
			base.Invoke("TurnOff", (float)this.Amount);
		}
	}

	
	private void OnCollisionEnter(Collision collision)
	{
		if (this.MyFire)
		{
			this.MyFire.SetActive(false);
		}
	}

	
	private void TurnOff()
	{
		if (!BoltNetwork.isRunning || (base.entity && base.entity.isAttached && base.entity.isOwner))
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	
	public int Amount = 8;

	
	public GameObject MyFire;
}
