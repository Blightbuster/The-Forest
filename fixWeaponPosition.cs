using System;
using System.Collections;
using TheForest.Utils;
using UnityEngine;


public class fixWeaponPosition : MonoBehaviour
{
	
	private void Awake()
	{
		this.parentScript = base.transform.GetComponent<FakeParent>();
	}

	
	private void OnEnable()
	{
		base.StartCoroutine("FixPosition");
	}

	
	private void OnDisable()
	{
		base.StopCoroutine("FixPosition");
	}

	
	private IEnumerator FixPosition()
	{
		for (;;)
		{
			yield return YieldPresets.WaitTwoSeconds;
			this.parentScript.enabled = false;
			yield return null;
			this.parentScript.enabled = true;
		}
		yield break;
	}

	
	private FakeParent parentScript;

	
	private float timer;
}
