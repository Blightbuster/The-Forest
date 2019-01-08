using System;
using UnityEngine;
using U_r_g_utils;

public class clstriggerurgent : MonoBehaviour
{
	private void Awake()
	{
		base.GetComponent<Animation>().wrapMode = WrapMode.ClampForever;
	}

	private void OnTriggerEnter(Collider varsource)
	{
		if (varsource.name == "bumper")
		{
			base.GetComponent<Animation>().Stop();
			clsurgent component = base.GetComponent<clsurgent>();
			if (component != null)
			{
				clsurgutils.metdriveurgent(component, null);
			}
		}
	}
}
