using System;
using UnityEngine;

public class artifactExplodeController : MonoBehaviour
{
	private void Start()
	{
	}

	private void OnEnable()
	{
		this.destroyTime = Time.time + 1.5f;
		this.waitTime = Time.time + 0.2f;
		this.flashTime = Time.time + 0.5f;
	}

	private void Update()
	{
		Vector3 localEulerAngles = base.transform.localEulerAngles;
		localEulerAngles.z += 10f;
		base.transform.localEulerAngles = localEulerAngles;
		base.transform.localScale *= 1.1f;
		if (Time.time < this.waitTime)
		{
			return;
		}
		this.flashGo.SetActive(true);
		if (Time.time > this.flashTime)
		{
			this.flashGo.SetActive(false);
		}
		if (Time.time > this.destroyTime)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public GameObject flashGo;

	private float destroyTime;

	private float waitTime;

	private float flashTime;
}
