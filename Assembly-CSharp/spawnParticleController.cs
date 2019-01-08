using System;
using UnityEngine;

public class spawnParticleController : MonoBehaviour
{
	private void OnEnable()
	{
		this.spawnedPrefab = UnityEngine.Object.Instantiate<GameObject>(this.go, base.transform.position, base.transform.rotation);
		this.spawnedPrefab.transform.parent = null;
		this.spawnedPrefab.SendMessage("setFollowTarget", base.transform);
	}

	private void OnDisable()
	{
		if (this.animal && this.spawnedPrefab)
		{
			UnityEngine.Object.Destroy(this.spawnedPrefab);
		}
	}

	private void setFireDuration(float amount)
	{
		base.CancelInvoke("disableThis");
		this.burnDuration = amount;
		ParticleSystem component = this.spawnedPrefab.GetComponent<ParticleSystem>();
		if (component)
		{
			component.Stop();
			component.main.duration = this.burnDuration;
			component.Play();
			this.spawnedPrefab.SendMessage("setDestroyTime", this.burnDuration);
		}
	}

	private void netDisableFollowTarget()
	{
		if (this.spawnedPrefab)
		{
			this.spawnedPrefab.SendMessage("disableFollowTarget");
		}
	}

	public void resetParticleDuration()
	{
		if (this.spawnedPrefab)
		{
			ParticleSystem component = this.spawnedPrefab.GetComponent<ParticleSystem>();
			if (component)
			{
				component.Stop(true);
				component.main.duration = 0.1f;
				component.Play(true);
			}
		}
	}

	public GameObject go;

	public float burnDuration;

	public GameObject spawnedPrefab;

	public bool animal;
}
