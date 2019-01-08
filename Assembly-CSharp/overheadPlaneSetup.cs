using System;
using System.Collections;
using UnityEngine;

public class overheadPlaneSetup : MonoBehaviour
{
	private void Start()
	{
		this.lightsGo.SetActive(false);
		if (Clock.Dark)
		{
			base.StartCoroutine(this.lightsRoutine());
		}
	}

	private IEnumerator lightsRoutine()
	{
		this.lightsGo.SetActive(true);
		this.redLight.SetActive(false);
		this.greenLight.SetActive(false);
		for (;;)
		{
			yield return YieldPresets.WaitOnePointFiveSeconds;
			this.redLight.SetActive(true);
			yield return YieldPresets.WaitPointOneSeconds;
			this.greenLight.SetActive(true);
			yield return YieldPresets.WaitPointOneSeconds;
			this.redLight.SetActive(false);
			yield return YieldPresets.WaitPointOneSeconds;
			this.greenLight.SetActive(false);
		}
		yield break;
	}

	public GameObject lightsGo;

	public GameObject redLight;

	public GameObject greenLight;

	public GameObject whiteLight;

	private float todValue;
}
