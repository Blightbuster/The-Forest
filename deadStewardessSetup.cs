using System;
using UnityEngine;


public class deadStewardessSetup : MonoBehaviour
{
	
	private void Start()
	{
	}

	
	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Weapon") && Time.time > this.nextHitTimer)
		{
			this._hits--;
			this.nextHitTimer = Time.time + 0.5f;
			if (this._hits <= 0 && this.skinTrigger)
			{
				this.skinTrigger.SetActive(true);
			}
		}
	}

	
	private void enableSkinTrigger()
	{
		this.skinTrigger.SetActive(true);
	}

	
	public GameObject skinTrigger;

	
	public int _hits;

	
	private float nextHitTimer;
}
