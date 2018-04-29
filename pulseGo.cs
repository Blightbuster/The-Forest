using System;
using System.Collections;
using UnityEngine;


public class pulseGo : MonoBehaviour
{
	
	private void Start()
	{
		base.InvokeRepeating("initPulse", 0f, this.interval);
	}

	
	private void initPulse()
	{
		base.StartCoroutine("doPulse");
	}

	
	private IEnumerator doPulse()
	{
		base.gameObject.SetActive(false);
		yield return YieldPresets.WaitForFixedUpdate;
		base.gameObject.SetActive(true);
		yield break;
	}

	
	public float interval;
}
