using System;
using System.Collections;
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
		IEnumerator enumerator = base.transform.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				Transform transform = (Transform)obj;
				transform.transform.gameObject.SetActive(activeStatus);
			}
		}
		finally
		{
			IDisposable disposable;
			if ((disposable = (enumerator as IDisposable)) != null)
			{
				disposable.Dispose();
			}
		}
	}

	public bool enableMobileControls;

	private bool mobileControlsPreviousState;
}
