using System;
using System.Collections;
using UnityEngine;


public class ParticleSystemDestroyer : MonoBehaviour
{
	
	private IEnumerator Start()
	{
		ParticleSystem[] systems = base.GetComponentsInChildren<ParticleSystem>();
		foreach (ParticleSystem particleSystem in systems)
		{
			this.maxLifetime = Mathf.Max(particleSystem.startLifetime, this.maxLifetime);
		}
		float stopTime = Time.time + UnityEngine.Random.Range(this.minDuration, this.maxDuration);
		while (Time.time < stopTime || this.earlyStop)
		{
			yield return null;
		}
		Debug.Log("stopping " + base.name);
		foreach (ParticleSystem particleSystem2 in systems)
		{
			particleSystem2.enableEmission = false;
		}
		base.BroadcastMessage("Extinguish", SendMessageOptions.DontRequireReceiver);
		yield return new WaitForSeconds(this.maxLifetime);
		UnityEngine.Object.Destroy(base.gameObject);
		yield break;
	}

	
	public void Stop()
	{
		this.earlyStop = true;
	}

	
	public float minDuration = 8f;

	
	public float maxDuration = 10f;

	
	private float maxLifetime;

	
	private bool earlyStop;
}
