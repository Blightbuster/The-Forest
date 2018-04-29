using System;
using UnityEngine;


public class FlickerArtifactLight : MonoBehaviour
{
	
	private void Start()
	{
		this.random = UnityEngine.Random.Range(0f, 65535f);
		this.startLocalPos = base.transform.localPosition;
	}

	
	private void Update()
	{
		float t = Mathf.PerlinNoise(this.random, Time.time * this.timeMult);
		float t2 = Mathf.PerlinNoise(this.random, Time.time * this.wobbleTimeMult);
		base.GetComponent<Light>().intensity = Mathf.Lerp(this.minIntensity, this.maxIntensity, t);
		Vector3 localPosition = this.startLocalPos;
		localPosition.x = Mathf.Lerp(this.wobbleMin, this.wobbleMax, t2);
		base.transform.localPosition = localPosition;
	}

	
	public float minIntensity = 0.6f;

	
	public float maxIntensity = 1.25f;

	
	public float timeMult = 2f;

	
	public float wobbleMin = -0.25f;

	
	public float wobbleMax = 0.25f;

	
	public float wobbleTimeMult = 2f;

	
	private Vector3 startLocalPos;

	
	private float random;
}
