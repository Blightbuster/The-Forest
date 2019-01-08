using System;
using UnityEngine;

public class VRLightFire : MonoBehaviour
{
	private void Start()
	{
		if (!ForestVR.Enabled)
		{
			base.gameObject.SetActive(false);
		}
		else
		{
			base.gameObject.tag = "fire";
		}
	}

	private void OnDisable()
	{
		this.InFireTrigger = false;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("fire"))
		{
			this.InFireTrigger = true;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.CompareTag("fire"))
		{
			this.InFireTrigger = false;
		}
	}

	private void Update()
	{
		if (this.InFireTrigger)
		{
			if (Time.time > this.nextFireCheck)
			{
				if (this.ArrowFire)
				{
					this.targetFire.SendMessage("LightArrowVR", SendMessageOptions.DontRequireReceiver);
				}
				else
				{
					this.targetFire.SendMessage("Burn", SendMessageOptions.DontRequireReceiver);
				}
				this.nextFireCheck = Time.time + 1f;
			}
		}
		else
		{
			this.nextFireCheck = Time.time + 1.3f;
		}
	}

	public GameObject targetFire;

	private bool InFireTrigger;

	private float nextFireCheck;

	public bool ArrowFire;
}
