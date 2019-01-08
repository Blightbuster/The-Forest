using System;
using System.Collections;
using UnityEngine;

public class enemyParticleFireController : MonoBehaviour
{
	private void Start()
	{
		if (this.burnDummyFire && base.transform.parent)
		{
			this.followTr = base.transform.parent;
			base.transform.parent = null;
			this.doFollowTarget = true;
		}
	}

	private void Update()
	{
		if (this.doFollowTarget)
		{
			if (this.followTr)
			{
				base.transform.position = this.followTr.position;
			}
			if (this.burnDummyFire && this.followTr == null)
			{
				base.StartCoroutine(this.removeParticlerRoutine());
			}
		}
	}

	private void setFollowTarget(Transform target)
	{
		this.followTr = target;
		this.doFollowTarget = true;
	}

	private void disableFollowTarget()
	{
		this.doFollowTarget = false;
	}

	private void DestroyThis()
	{
		UnityEngine.Object.Destroy(base.gameObject, 4f);
	}

	private IEnumerator removeParticlerRoutine()
	{
		ParticleSystem ps = base.transform.GetComponent<ParticleSystem>();
		if (ps)
		{
			if (ps.main.duration < 1f)
			{
				yield break;
			}
			ps.Stop();
			ps.main.duration = 0.1f;
			ps.Play();
		}
		yield return YieldPresets.WaitFourSeconds;
		UnityEngine.Object.Destroy(base.gameObject);
		yield break;
	}

	public bool burnDummyFire;

	public bool firemanFire;

	public bool doFollowTarget;

	private Transform followTr;
}
