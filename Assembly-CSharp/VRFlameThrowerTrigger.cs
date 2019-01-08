using System;
using UnityEngine;

public class VRFlameThrowerTrigger : MonoBehaviour
{
	private void Start()
	{
		if (!ForestVR.Enabled)
		{
			base.gameObject.SetActive(false);
		}
	}

	private void OnDisable()
	{
		this.lighterInRange = false;
		if (this.controller)
		{
			this.controller.VRLighterInRange = false;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("fire"))
		{
			this.lighterInRange = true;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.CompareTag("fire"))
		{
			this.lighterInRange = false;
		}
	}

	private void Update()
	{
		if (this.lighterInRange)
		{
			this.controller.VRLighterInRange = true;
		}
		else
		{
			this.controller.VRLighterInRange = false;
		}
	}

	public flameThrowerController controller;

	public GameObject mistGo;

	public bool lighterInRange;
}
