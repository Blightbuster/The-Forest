using System;
using System.Collections;
using UnityEngine;


public class effigyRangeDetect : MonoBehaviour
{
	
	private void Start()
	{
		if (this.destroyTime > 0f)
		{
			base.Invoke("destroyMe", this.destroyTime);
		}
	}

	
	private void doPulse()
	{
	}

	
	private IEnumerator pulse()
	{
		base.GetComponent<Collider>().enabled = false;
		yield return YieldPresets.WaitPointOneSeconds;
		base.GetComponent<Collider>().enabled = true;
		yield break;
	}

	
	private void OnDisable()
	{
	}

	
	private void destroyMe()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	
	public float destroyTime;
}
