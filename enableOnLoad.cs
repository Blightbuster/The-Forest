using System;
using System.Collections;
using UnityEngine;


public class enableOnLoad : MonoBehaviour
{
	
	private void Start()
	{
		if (this.enableOnStart)
		{
			base.StartCoroutine("doEnable");
		}
	}

	
	private void OnDeserialized()
	{
		base.StartCoroutine("doEnable");
	}

	
	public IEnumerator doEnable()
	{
		if (this.doingEnable)
		{
			yield break;
		}
		this.doingEnable = true;
		yield return YieldPresets.WaitOneSecond;
		if (this.go)
		{
			this.go.SetActive(true);
		}
		yield return YieldPresets.WaitOneSecond;
		if (this.go)
		{
			this.go.SetActive(false);
		}
		this.doingEnable = false;
		yield break;
	}

	
	public bool enableOnStart;

	
	private bool doingEnable;

	
	public GameObject go;
}
