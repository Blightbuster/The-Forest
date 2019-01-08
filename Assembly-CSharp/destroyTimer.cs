using System;
using UnityEngine;

public class destroyTimer : MonoBehaviour
{
	private void Start()
	{
		this.startTime = Time.time;
	}

	private void OnDisable()
	{
		if (base.gameObject)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void setDestroyTime(float val)
	{
		this.timer = val;
	}

	private void Update()
	{
		if (Time.time > this.startTime + this.timer)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public float timer = 20f;

	private float startTime;
}
