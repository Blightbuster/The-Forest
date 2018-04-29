using System;
using UnityEngine;


[ExecuteInEditMode]
public class DisableMobileContent : MonoBehaviour
{
	
	private void Awake()
	{
		this.enableMobileControls = false;
		this.SetMobileControlsStatus(this.enableMobileControls);
		this.mobileControlsPreviousState = this.enableMobileControls;
	}

	
	private void UpdateControlStatus()
	{
		if (this.mobileControlsPreviousState != this.enableMobileControls)
		{
			this.SetMobileControlsStatus(this.enableMobileControls);
			this.mobileControlsPreviousState = this.enableMobileControls;
		}
	}

	
	private void SetMobileControlsStatus(bool activeStatus)
	{
		foreach (object obj in base.transform)
		{
			Transform transform = (Transform)obj;
			transform.transform.gameObject.SetActive(activeStatus);
		}
	}

	
	public bool enableMobileControls;

	
	private bool mobileControlsPreviousState;
}
