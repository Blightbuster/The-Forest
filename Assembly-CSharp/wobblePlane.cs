using System;
using UnityEngine;

public class wobblePlane : MonoBehaviour
{
	private void Start()
	{
		this._random = UnityEngine.Random.Range(0f, 65535f);
		this._random2 = UnityEngine.Random.Range(0f, 65535f);
	}

	private void Update()
	{
		float t = Mathf.PerlinNoise(this._random, Time.time * this.wobbleTimeMult);
		float t2 = Mathf.PerlinNoise(this._random2, Time.time * this.wobbleTimeMult);
		Vector3 localEulerAngles = base.transform.localEulerAngles;
		Vector3 localPosition = base.transform.localPosition;
		localEulerAngles.z = Mathf.Lerp(this.bankWobbleMin, this.bankWobbleMax, t2);
		localPosition.y = Mathf.Lerp(this.heightWobbleMin, this.heightWobbleMax, t);
		base.transform.localEulerAngles = localEulerAngles;
		base.transform.localPosition = localPosition;
	}

	public float bankWobbleMin = 12f;

	public float bankWobbleMax = 16f;

	public float heightWobbleMin = 80f;

	public float heightWobbleMax = 95f;

	public float wobbleTimeMult = 0.5f;

	private float _random;

	private float _random2;
}
