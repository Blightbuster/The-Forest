using System;
using UnityEngine;

public class doBurn : MonoBehaviour
{
	public void enableFire()
	{
		this.fire.SetActive(true);
		base.Invoke("cancelFire", 10f);
	}

	private void cancelFire()
	{
		this.fire.SetActive(false);
	}

	public GameObject fire;
}
